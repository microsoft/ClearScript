// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
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

        private static readonly object nullDispatch = new DispatchWrapper(null);

        private ActiveScriptWrapper activeScript;
        private WindowsScriptEngineFlags engineFlags;

        private readonly HostItemMap hostItemMap = new HostItemMap();
        private readonly HostItemCollateral hostItemCollateral = new HostItemCollateral();
        private readonly object script;

        private ProcessDebugManagerWrapper processDebugManager;
        private DebugApplicationWrapper debugApplication;
        private uint debugApplicationCookie;
        private readonly IUniqueNameManager debugDocumentNameManager = new UniqueFileNameManager();

        private bool sourceManagement;
        private readonly DebugDocumentMap debugDocumentMap = new DebugDocumentMap();
        private uint nextSourceContext = 1;

        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

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
        [Obsolete("Use WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags) instead.")]
        protected WindowsScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
            : this(progID, name, null, flags)
        {
        }

        /// <summary>
        /// Initializes a new Windows Script engine instance with the specified list of supported file name extensions.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(name, fileNameExtensions)
        {
            script = base.ScriptInvoke(() =>
            {
                activeScript = ActiveScriptWrapper.Create(progID, flags);
                engineFlags = flags;

                if (flags.HasFlag(WindowsScriptEngineFlags.EnableDebugging) && ProcessDebugManagerWrapper.TryCreate(out processDebugManager))
                {
                    processDebugManager.CreateApplication(out debugApplication);
                    debugApplication.SetName(Name);

                    if (processDebugManager.TryAddApplication(debugApplication, out debugApplicationCookie))
                    {
                        sourceManagement = !flags.HasFlag(WindowsScriptEngineFlags.DisableSourceManagement);
                    }
                    else
                    {
                        debugApplication.Close();
                        debugApplication = null;
                        processDebugManager = null;
                    }
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
            get
            {
                VerifyNotDisposed();
                return dispatcher;
            }
        }

        /// <summary>
        /// Determines whether the calling thread has access to the current script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the current script engine, <c>false</c> otherwise.</returns>
        public bool CheckAccess()
        {
            VerifyNotDisposed();
            return dispatcher.CheckAccess();
        }

        /// <summary>
        /// Enforces that the calling thread has access to the current script engine.
        /// </summary>
        public void VerifyAccess()
        {
            VerifyNotDisposed();
            dispatcher.VerifyAccess();
        }

        /// <summary>
        /// Gets or sets an interface that supports the display of dialogs on behalf of script code.
        /// </summary>
        public IHostWindow HostWindow { get; set; }

        #endregion

        #region internal members

        internal abstract IDictionary<int, string> RuntimeErrorMap { get; }

        internal abstract IDictionary<int, string> SyntaxErrorMap { get; }

        private object GetScriptDispatch()
        {
            object scriptDispatch;
            activeScript.GetScriptDispatch(null, out scriptDispatch);
            return scriptDispatch;
        }

        private void Parse(UniqueDocumentInfo documentInfo, string code, ScriptTextFlags flags, IntPtr pVarResult, out EXCEPINFO excepInfo)
        {
            DebugDocument debugDocument;
            var sourceContext = CreateDebugDocument(documentInfo, code, out debugDocument);
            if (sourceContext != UIntPtr.Zero)
            {
                flags |= ScriptTextFlags.HostManagesSource;
            }

            try
            {
                activeScript.ParseScriptText(code, null, null, null, sourceContext, 0, flags, pVarResult, out excepInfo);
            }
            finally
            {
                if (documentInfo.Flags.GetValueOrDefault().HasFlag(DocumentFlags.IsTransient) && (sourceContext != UIntPtr.Zero))
                {
                    debugDocumentMap.Remove(sourceContext);
                    debugDocument.Close();
                }
            }
        }

        private UIntPtr CreateDebugDocument(UniqueDocumentInfo documentInfo, string code, out DebugDocument document)
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
                document = new DebugDocument(this, sourceContext, documentInfo, code);
                debugDocumentMap[sourceContext] = document;
            }

            return sourceContext;
        }

        private string GetStackTraceInternal()
        {
            Debug.Assert(processDebugManager != null);
            var stackTrace = string.Empty;

            IEnumDebugStackFrames enumFrames;
            activeScript.EnumStackFrames(out enumFrames);

            while (true)
            {
                DebugStackFrameDescriptor descriptor;
                uint countFetched;
                enumFrames.Next(1, out descriptor, out countFetched);
                if (countFetched < 1)
                {
                    break;
                }

                try
                {
                    string description;
                    descriptor.Frame.GetDescriptionString(true, out description);

                    IDebugCodeContext codeContext;
                    descriptor.Frame.GetCodeContext(out codeContext);

                    IDebugDocumentContext documentContext;
                    codeContext.GetDocumentContext(out documentContext);
                    if (documentContext == null)
                    {
                        stackTrace += MiscHelpers.FormatInvariant("    at {0}\n", description);
                    }
                    else
                    {
                        IDebugDocument document;
                        documentContext.GetDocument(out document);
                        var documentText = (IDebugDocumentText)document;

                        string documentName;
                        document.GetName(DocumentNameType.UniqueTitle, out documentName);

                        uint position;
                        uint length;
                        documentText.GetPositionOfContext(documentContext, out position, out length);

                        using (var bufferBlock = new CoTaskMemArrayBlock(sizeof(char), (int)length))
                        {
                            uint lengthReturned = 0;
                            documentText.GetText(position, bufferBlock.Addr, IntPtr.Zero, ref lengthReturned, length);
                            var codeLine = Marshal.PtrToStringUni(bufferBlock.Addr, (int)lengthReturned);

                            uint lineNumber;
                            uint offsetInLine;
                            documentText.GetLineOfPosition(position, out lineNumber, out offsetInLine);

                            stackTrace += MiscHelpers.FormatInvariant("    at {0} ({1}:{2}:{3}) -> {4}\n", description, documentName, lineNumber, offsetInLine, codeLine);
                        }
                    }
                }
                finally
                {
                    if (descriptor.pFinalObject != IntPtr.Zero)
                    {
                        Marshal.Release(descriptor.pFinalObject);
                    }
                }
            }

            return stackTrace.TrimEnd('\n');
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        private static bool GetDirectAccessItem(object item, out object directAccessItem)
        {
            while (true)
            {
                var scriptMarshalWrapper = item as IScriptMarshalWrapper;
                if (scriptMarshalWrapper != null)
                {
                    item = scriptMarshalWrapper.Unwrap();
                    continue;
                }

                var hostTarget = item as HostTarget;
                if (hostTarget != null)
                {
                    item = hostTarget.Target;
                    continue;
                }

                if ((item != null) && item.GetType().IsCOMObject)
                {
                    directAccessItem = item;
                    return true;
                }

                directAccessItem = null;
                return false;
            }
        }

        private object MarshalToScriptInternal(object obj, HostItemFlags flags, HashSet<Array> marshaledArraySet)
        {
            if (obj == null)
            {
                if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalNullAsDispatch))
                {
                    return nullDispatch;
                }

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

            if (obj is Nothing)
            {
                return nullDispatch;
            }

            if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalDecimalAsCurrency) && (obj is decimal))
            {
                return new CurrencyWrapper(obj);
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                if ((hostItem.Engine == this) && (hostItem.Flags == flags))
                {
                    return obj;
                }

                obj = hostItem.Target;
            }

            var hostTarget = obj as HostTarget;
            if ((hostTarget != null) && !(hostTarget is IHostVariable))
            {
                obj = hostTarget.Target;
            }

            var scriptItem = obj as ScriptItem;
            if (scriptItem != null)
            {
                if (scriptItem.Engine == this)
                {
                    return scriptItem.Unwrap();
                }
            }

            if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalArraysByValue))
            {
                var array = obj as Array;
                if ((array != null) && ((hostTarget == null) || (typeof(Array).IsAssignableFrom(hostTarget.Type))))
                {
                    bool alreadyMarshaled;
                    if (marshaledArraySet != null)
                    {
                        alreadyMarshaled = marshaledArraySet.Contains(array);
                    }
                    else
                    {
                        marshaledArraySet = new HashSet<Array>();
                        alreadyMarshaled = false;
                    }

                    if (!alreadyMarshaled)
                    {
                        marshaledArraySet.Add(array);
                        var dimensions = Enumerable.Range(0, array.Rank).ToArray();
                        var marshaledArray = Array.CreateInstance(typeof(object), dimensions.Select(array.GetLength).ToArray(), dimensions.Select(array.GetLowerBound).ToArray());
                        array.Iterate(indices => marshaledArray.SetValue(MarshalToScriptInternal(array.GetValue(indices), flags, marshaledArraySet), indices));
                        return marshaledArray;
                    }

                    // COM interop can't handle circularly referenced arrays
                    return MarshalToScriptInternal(null, flags, marshaledArraySet);
                }
            }

            return HostItem.Wrap(this, hostTarget ?? obj, flags);
        }

        private object MarshalToHostInternal(object obj, bool preserveHostTarget, HashSet<Array> marshaledArraySet)
        {
            if (obj == null)
            {
                return Undefined.Value;
            }

            if (obj is DBNull)
            {
                return null;
            }

            object result;
            if (MiscHelpers.TryMarshalPrimitiveToHost(obj, out result))
            {
                return result;
            }

            var array = obj as Array;
            if (array != null)
            {
                // COM interop converts VBScript arrays to managed arrays

                bool alreadyMarshaled;
                if (marshaledArraySet != null)
                {
                    alreadyMarshaled = marshaledArraySet.Contains(array);
                }
                else
                {
                    marshaledArraySet = new HashSet<Array>();
                    alreadyMarshaled = false;
                }

                if (!alreadyMarshaled)
                {
                    marshaledArraySet.Add(array);
                    array.Iterate(indices => array.SetValue(MarshalToHostInternal(array.GetValue(indices), preserveHostTarget, marshaledArraySet), indices));
                }

                return array;
            }

            var hostTarget = obj as HostTarget;
            if (hostTarget != null)
            {
                return preserveHostTarget ? hostTarget : hostTarget.Target;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                return preserveHostTarget ? hostItem.Target : hostItem.Unwrap();
            }

            if (obj is ScriptItem)
            {
                return obj;
            }

            return WindowsScriptItem.Wrap(this, obj);
        }

        private void ThrowHostException(Exception exception)
        {
            if (CurrentScriptFrame != null)
            {
                // Record the host exception in the script frame and throw an easily recognizable
                // surrogate across the COM boundary. Recording the host exception enables
                // downstream chaining. The surrogate exception indicates to the site that the
                // reported script error actually corresponds to the host exception in the frame.

                CurrentScriptFrame.HostException = exception;
                throw new COMException(exception.Message, RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION);
            }
        }

        private void ThrowScriptError(Exception exception)
        {
            var comException = exception as COMException;
            if (comException != null)
            {
                if (comException.ErrorCode == RawCOMHelpers.HResult.SCRIPT_E_REPORTED)
                {
                    // a script error was reported; the corresponding exception should be in the script frame
                    ThrowScriptError(CurrentScriptFrame.ScriptError ?? CurrentScriptFrame.PendingScriptError);
                }
                else if (comException.ErrorCode == RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                {
                    // A host exception surrogate passed through the COM boundary; this happens
                    // when some script engines are invoked via script item access rather than
                    // script execution. Chain the host exception to a new script exception.

                    var hostException = CurrentScriptFrame.HostException;
                    if (hostException != null)
                    {
                        throw new ScriptEngineException(Name, hostException.Message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION, false, true, null, hostException);
                    }
                }
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
        /// Gets a string representation of the script call stack.
        /// </summary>
        /// <returns>The script call stack formatted as a string.</returns>
        /// <remarks>
        /// <para>
        /// This method returns an empty string if the script engine is not executing script code.
        /// The stack trace text format is defined by the script engine.
        /// </para>
        /// <para>
        /// The <see cref="WindowsScriptEngine"/> version of this method returns the empty string
        /// if script debugging features have not been enabled for the instance.
        /// </para>
        /// </remarks>
        public override string GetStackTrace()
        {
            VerifyNotDisposed();
            return (processDebugManager != null) ? ScriptInvoke(() => GetStackTraceInternal()) : string.Empty;
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

            var excepInfo = new EXCEPINFO { scode = RawCOMHelpers.HResult.E_ABORT };
            activeScript.InterruptScriptThread(ScriptThreadID.Base, ref excepInfo, ScriptInterruptFlags.None);
        }

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        public override void CollectGarbage(bool exhaustive)
        {
            VerifyNotDisposed();
            ScriptInvoke(() => activeScript.CollectGarbage(exhaustive ? ScriptGCType.Exhaustive : ScriptGCType.Normal));
        }

        #endregion

        #region ScriptEngine overrides (internal members)

        internal override IUniqueNameManager DocumentNameManager
        {
            get { return debugDocumentNameManager; }
        }

        internal override void AddHostItem(string itemName, HostItemFlags flags, object item)
        {
            VerifyNotDisposed();

            MiscHelpers.VerifyNonNullArgument(itemName, "itemName");
            Debug.Assert(item != null);

            ScriptInvoke(() =>
            {
                object marshaledItem;
                if (!flags.HasFlag(HostItemFlags.DirectAccess) || !GetDirectAccessItem(item, out marshaledItem))
                {
                    marshaledItem = MarshalToScript(item, flags);
                    if (!(marshaledItem is HostItem))
                    {
                        throw new InvalidOperationException("Invalid host item");
                    }
                }

                var oldItem = ((IDictionary)hostItemMap)[itemName];
                hostItemMap[itemName] = marshaledItem;

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
                    if (oldItem != null)
                    {
                        hostItemMap[itemName] = oldItem;
                    }
                    else
                    {
                        hostItemMap.Remove(itemName);
                    }

                    throw;
                }
            });
        }

        internal override object PrepareResult(object result, Type type, ScriptMemberFlags flags, bool isListIndexResult)
        {
            var tempResult = base.PrepareResult(result, type, flags, isListIndexResult);
            if ((tempResult != null) || !engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalNullAsDispatch))
            {
                return tempResult;
            }

            if ((type == typeof(object)) || (type == typeof(string)) || type == typeof(bool?) || type.IsNullableNumeric())
            {
                return DBNull.Value;
            }

            return null;
        }

        internal override object MarshalToScript(object obj, HostItemFlags flags)
        {
            return MarshalToScriptInternal(obj, flags, null);
        }

        internal override object MarshalToHost(object obj, bool preserveHostTarget)
        {
            return MarshalToHostInternal(obj, preserveHostTarget, null);
        }

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            VerifyNotDisposed();
            return ScriptInvoke(() => ExecuteRaw(documentInfo, code, evaluate));
        }

        internal sealed override object ExecuteRaw(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            EXCEPINFO excepInfo;
            if (!evaluate)
            {
                const ScriptTextFlags flags = ScriptTextFlags.IsVisible;
                Parse(documentInfo, code, flags, IntPtr.Zero, out excepInfo);
                return null;
            }

            using (var resultVariantBlock = new CoTaskMemVariantBlock())
            {
                const ScriptTextFlags flags = ScriptTextFlags.IsExpression;
                Parse(documentInfo, code, flags, resultVariantBlock.Addr, out excepInfo);
                return Marshal.GetObjectForNativeVariant(resultVariantBlock.Addr);
            }
        }

        internal override HostItemCollateral HostItemCollateral
        {
            get { return hostItemCollateral; }
        }

        #endregion

        #region ScriptEngine overrides (host-side invocation)

        internal override void HostInvoke(Action action)
        {
            try
            {
                base.HostInvoke(action);
            }
            catch (Exception exception)
            {
                ThrowHostException(exception);
                throw;
            }
        }

        internal override T HostInvoke<T>(Func<T> func)
        {
            try
            {
                return base.HostInvoke(func);
            }
            catch (Exception exception)
            {
                ThrowHostException(exception);
                throw;
            }
        }

        #endregion

        #region ScriptEngine overrides (script-side invocation)

        internal override void ScriptInvoke(Action action)
        {
            VerifyAccess();
            base.ScriptInvoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    ThrowScriptError(exception);
                    throw;
                }
            });
        }

        internal override T ScriptInvoke<T>(Func<T> func)
        {
            VerifyAccess();
            return base.ScriptInvoke(() =>
            {
                try
                {
                    return func();
                }
                catch (Exception exception)
                {
                    ThrowScriptError(exception);
                    throw;
                }
            });
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

        #region ScriptEngine overrides (disposal / finalization)

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
            if (disposedFlag.Set())
            {
                if (disposing)
                {
                    if (sourceManagement)
                    {
                        debugDocumentMap.Values.ForEach(debugDocument => debugDocument.Close());
                    }

                    if (processDebugManager != null)
                    {
                        processDebugManager.RemoveApplication(debugApplicationCookie);
                        debugApplication.Close();
                    }

                    ((IDisposable)script).Dispose();
                    activeScript.Close();
                }
            }
        }

        #endregion

        #region unit test support

        internal IEnumerable<string> GetDebugDocumentNames()
        {
            return debugDocumentMap.Values.Select(debugDocument =>
            {
                string name;
                debugDocument.GetName(DocumentNameType.UniqueTitle, out name);
                return name;
            });
        }

        #endregion

        #region Nested type: HostItemMap

        private sealed class HostItemMap : Dictionary<string, object>
        {
        }

        #endregion
    }
}
