// 
// Copyright © Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.ClearScript.Util;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Provides the base implementation for all Windows Script engines.
    /// </summary>
    /// <remarks>
    /// Each Windows Script engine instance has thread affinity and is bound to a
    /// <see cref="Dispatcher"/> during instantiation. Attempting to execute script code on a
    /// different thread results in an exception. Script delegates and event handlers are marshaled
    /// synchronously onto the correct thread.
    /// </remarks>
    public abstract partial class WindowsScriptEngine : ScriptEngine
    {
        #region data

        private ActiveScriptWrapper activeScript;
        private WindowsScriptEngineFlags engineFlags;

        private HostItemMap hostItemMap = new HostItemMap();
        private readonly dynamic script;

        private ProcessDebugManagerWrapper processDebugManager;
        private string debugApplicationName;
        private DebugApplicationWrapper debugApplication;
        private uint debugApplicationCookie;

        private bool sourceManagement;
        private readonly DebugDocumentMap debugDocumentMap = new DebugDocumentMap();
        private uint nextSourceContext = 1;

        private static readonly IUniqueNameManager debugApplicationNameManager = new UniqueNameManager();
        private readonly IUniqueNameManager debugDocumentNameManager = new UniqueFileNameManager();

        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private bool disposed;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new Windows Script engine instance.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected WindowsScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
        {
            AccessContext = typeof(ScriptEngine);
            script = base.ScriptInvoke(() =>
            {
                activeScript = ActiveScriptWrapper.Create(progID);
                engineFlags = flags;

                if (flags.HasFlag(WindowsScriptEngineFlags.EnableDebugging))
                {
                    processDebugManager = ProcessDebugManagerWrapper.Create();
                    processDebugManager.CreateApplication(out debugApplication);
                    debugApplicationName = debugApplicationNameManager.GetUniqueName(name, GetType().GetRootName());
                    debugApplication.SetName(debugApplicationName);
                    processDebugManager.AddApplication(debugApplication, out debugApplicationCookie);
                    sourceManagement = !flags.HasFlag(WindowsScriptEngineFlags.DisableSourceManagement);
                }

                activeScript.SetScriptSite(new ScriptSite(this));
                activeScript.InitNew();
                activeScript.SetScriptState(ScriptState.Started);
                return WindowsScriptItem.Wrap(this, GetScriptDispatch());
            });
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets the <see cref="Dispatcher"/> associated with the current script engine.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get { return dispatcher; }
        }

        /// <summary>
        /// Determines whether the calling thread has access to the current script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the current script engine, <c>false</c> otherwise.</returns>
        public bool CheckAccess()
        {
            return dispatcher.CheckAccess();
        }

        /// <summary>
        /// Enforces that the calling thread has access to the current script engine.
        /// </summary>
        public void VerifyAccess()
        {
            dispatcher.VerifyAccess();
        }

        #endregion

        #region internal members

        internal abstract IDictionary<int, string> RuntimeErrorMap { get; }

        private object GetScriptDispatch()
        {
            object scriptDispatch;
            activeScript.GetScriptDispatch(null, out scriptDispatch);
            return scriptDispatch;
        }

        private void Parse(string documentName, string code, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo, bool discard)
        {
            var formattedCode = MiscHelpers.FormatCode(code);

            DebugDocument debugDocument;
            var sourceContext = CreateDebugDocument(documentName, formattedCode, discard, out debugDocument);
            if (sourceContext != UIntPtr.Zero)
            {
                flags |= ScriptTextFlags.HostManagesSource;
            }

            try
            {
                activeScript.ParseScriptText(formattedCode, null, null, null, sourceContext, 0, flags, pVarResult, out excepInfo);
            }
            finally
            {
                if (discard && (sourceContext != UIntPtr.Zero))
                {
                    debugDocumentMap.Remove(sourceContext);
                    debugDocument.Close();
                }
            }
        }

        private UIntPtr CreateDebugDocument(string name, string code, bool transient, out DebugDocument document)
        {
            UIntPtr sourceContext;
            if (!sourceManagement)
            {
                sourceContext = UIntPtr.Zero;
                document = null;
            }
            else
            {
                sourceContext = new UIntPtr(nextSourceContext++);
                var uniqueName = debugDocumentNameManager.GetUniqueName(name, "Script Document");
                document = new DebugDocument(this, sourceContext, uniqueName, code, transient);
                debugDocumentMap[sourceContext] = document;
            }

            return sourceContext;
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        #endregion

        #region ScriptEngine overrides (public members)

        /// <summary>
        /// Allows the host to access script resources directly.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It dynamically supports properties and methods that correspond to global
        /// script objects and functions.
        /// </remarks>
        public override dynamic Script
        {
            get
            {
                VerifyNotDisposed();
                return script;
            }
        }

        /// <summary>
        /// Interrupts script execution and causes the script engine to throw an exception.
        /// </summary>
        /// <remarks>
        /// This method can be called safely from any thread.
        /// </remarks>
        public override void Interrupt()
        {
            VerifyNotDisposed();

            var excepInfo = new EXCEPINFO { scode = MiscHelpers.UnsignedAsSigned(RawCOMHelpers.HResult.E_ABORT) };
            activeScript.InterruptScriptThread(ScriptThreadID.Base, ref excepInfo, ScriptInterruptFlags.None);
        }

        #endregion

        #region ScriptEngine overrides (internal members)

        internal override void AddHostItem(string itemName, HostItemFlags flags, object item)
        {
            VerifyNotDisposed();

            MiscHelpers.VerifyNonNullArgument(itemName, "itemName");
            Debug.Assert(item != null);

            var marshaledItem = MarshalToScript(item, flags);
            if (!(marshaledItem is HostItem))
            {
                throw new InvalidOperationException("Invalid host item");
            }

            ScriptInvoke(() =>
            {
                hostItemMap.Add(itemName, marshaledItem);

                var nativeFlags = ScriptItemFlags.IsVisible;
                if (flags.HasFlag(HostItemFlags.GlobalMembers))
                {
                    nativeFlags |= ScriptItemFlags.GlobalMembers;
                }

                try
                {
                    activeScript.AddNamedItem(itemName, nativeFlags);
                }
                catch (Exception)
                {
                    hostItemMap.Remove(itemName);
                    throw;
                }
            });
        }

        internal override object MarshalToScript(object obj, HostItemFlags flags)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }

            if (obj is Undefined)
            {
                return null;
            }

            if (obj is Nonexistent)
            {
                return null;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                if ((hostItem.Engine == this) && (hostItem.Flags == flags))
                {
                    return obj;
                }

                obj = hostItem.Unwrap();
            }

            var scriptItem = obj as ScriptItem;
            if (scriptItem != null)
            {
                if (scriptItem.Engine == this)
                {
                    return scriptItem.Unwrap();
                }
            }

            return HostItem.Wrap(this, obj, flags);
        }

        internal override object MarshalToHost(object obj)
        {
            if (obj == null)
            {
                return Undefined.Value;
            }

            if (obj is DBNull)
            {
                return null;
            }

            var array = obj as Array;
            if (array != null)
            {
                // COM interop converts VBScript arrays to managed arrays
                array.Iterate(indices => array.SetValue(MarshalToHost(array.GetValue(indices)), indices));
                return array;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                return hostItem.Unwrap();
            }

            if (obj is ScriptItem)
            {
                return obj;
            }

            return WindowsScriptItem.Wrap(this, obj);
        }

        internal override object Execute(string documentName, string code, bool evaluate, bool discard)
        {
            VerifyNotDisposed();

            object result = null;

            ScriptInvoke(() =>
            {
                EXCEPINFO excepInfo;
                if (!evaluate)
                {
                    const ScriptTextFlags flags = ScriptTextFlags.IsVisible;
                    Parse(documentName, code, flags, IntPtr.Zero, out excepInfo, discard);
                }
                else
                {
                    var pVarResult = Marshal.AllocCoTaskMem(256);
                    try
                    {
                        const ScriptTextFlags flags = ScriptTextFlags.IsExpression;
                        Parse(documentName, code, flags, pVarResult, out excepInfo, discard);
                        result = Marshal.GetObjectForNativeVariant(pVarResult);
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pVarResult);
                    }
                }
            });

            return result;
        }

        #endregion

        #region ScriptEngine overrides (script-side invocation)

        internal override void ScriptInvoke(Action action)
        {
            VerifyAccess();
            base.ScriptInvoke(action);
        }

        internal override T ScriptInvoke<T>(Func<T> func)
        {
            VerifyAccess();
            return base.ScriptInvoke(func);
        }

        #endregion

        #region ScriptEngine overrides (synchronized invocation)

        internal override void SyncInvoke(Action action)
        {
            dispatcher.Invoke(DispatcherPriority.Send, action);
        }

        internal override T SyncInvoke<T>(Func<T> func)
        {
            return (T)dispatcher.Invoke(DispatcherPriority.Send, func);
        }

        #endregion

        #region ScriptEngine overrides (disposition / finalization)

        /// <summary>
        /// Releases the unmanaged resources used by the script engine and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// This method is called by the public <see cref="ScriptEngine.Dispose()"/> method and the
        /// <see cref="ScriptEngine.Finalize">Finalize</see> method.
        /// <see cref="ScriptEngine.Dispose()"/> invokes the protected <c>Dispose(Boolean)</c>
        /// method with the <paramref name="disposing"/> parameter set to <c>true</c>.
        /// <see cref="ScriptEngine.Finalize">Finalize</see> invokes <c>Dispose(Boolean)</c> with
        /// <paramref name="disposing"/> set to <c>false</c>.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    hostItemMap.Clear();
                    hostItemMap = null;

                    if (sourceManagement)
                    {
                        debugDocumentMap.Values.ForEach(debugDocument => debugDocument.Close());
                        debugDocumentMap.Clear();
                    }

                    if (engineFlags.HasFlag(WindowsScriptEngineFlags.EnableDebugging))
                    {
                        processDebugManager.RemoveApplication(debugApplicationCookie);
                        debugApplication.Close();
                        debugApplication = null;
                        processDebugManager = null;
                    }

                    activeScript.Close();
                    activeScript = null;
                }

                disposed = true;
            }
        }

        #endregion

        #region unit test support

        internal IEnumerable<string> GetDebugDocumentNames()
        {
            return debugDocumentMap.Values.Select(debugDocument =>
            {
                string name;
                debugDocument.GetName(DocumentNameType.Title, out name);
                return name;
            });
        }

        #endregion

        #region Nested type: HostItemMap

        private class HostItemMap : Dictionary<string, object>
        {
        }

        #endregion
    }
}
