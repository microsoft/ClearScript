// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows.Core
{
    /// <summary>
    /// Provides the core implementation for all Windows Script engines.
    /// </summary>
    /// <remarks>
    /// This class can be used in non-desktop environments such as server applications. An
    /// implementation of <c><see cref="ISyncInvoker"/></c> is required to enforce thread affinity.
    /// </remarks>
    public abstract partial class WindowsScriptEngine : ScriptEngine, IWindowsScriptEngineTag
    {
        #region data

        private static readonly object nullDispatch = new DispatchWrapper(null);

        private ActiveScriptWrapper activeScript;
        private WindowsScriptEngineFlags engineFlags;

        private readonly HostItemMap hostItemMap = new HostItemMap();
        private readonly WindowsScriptItem script;

        private ProcessDebugManagerWrapper processDebugManager;
        private DebugApplicationWrapper debugApplication;
        private uint debugApplicationCookie;

        private bool sourceManagement;
        private readonly DebugDocumentMap debugDocumentMap = new DebugDocumentMap();
        private uint nextSourceContext = 1;

        private readonly ISyncInvoker syncInvoker;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new Windows Script engine instance with the specified list of supported file name extensions and synchronous invoker.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : base(name, fileNameExtensions)
        {
            MiscHelpers.VerifyNonNullArgument(syncInvoker, nameof(syncInvoker));
            this.syncInvoker = syncInvoker;

            script = (WindowsScriptItem)base.ScriptInvoke(() =>
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
        /// Gets the <c><see cref="ISyncInvoker"/></c> implementation associated with the current script engine.
        /// </summary>
        public ISyncInvoker SyncInvoker
        {
            get
            {
                VerifyNotDisposed();
                return syncInvoker;
            }
        }

        /// <summary>
        /// Determines whether the calling thread has access to the current script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the current script engine, <c>false</c> otherwise.</returns>
        public bool CheckAccess()
        {
            VerifyNotDisposed();
            return syncInvoker.CheckAccess();
        }

        /// <summary>
        /// Enforces that the calling thread has access to the current script engine.
        /// </summary>
        public void VerifyAccess()
        {
            VerifyNotDisposed();
            syncInvoker.VerifyAccess();
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
            activeScript.GetScriptDispatch(null, out var scriptDispatch);
            return scriptDispatch;
        }

        private void Parse(UniqueDocumentInfo documentInfo, string code, ScriptTextFlags flags, IntPtr pVarResult)
        {
            var sourceContext = CreateDebugDocument(documentInfo, code, out var debugDocument);
            if (sourceContext != UIntPtr.Zero)
            {
                flags |= ScriptTextFlags.HostManagesSource;
            }

            try
            {
                activeScript.ParseScriptText(code, null, null, null, sourceContext, 0, flags, pVarResult, out _);
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

            activeScript.EnumStackFrames(out var enumFrames);

            while (true)
            {
                enumFrames.Next(1, out var descriptor, out var countFetched);
                if (countFetched < 1)
                {
                    break;
                }

                try
                {
                    descriptor.Frame.GetDescriptionString(true, out var description);
                    descriptor.Frame.GetCodeContext(out var codeContext);

                    codeContext.GetDocumentContext(out var documentContext);
                    if (documentContext == null)
                    {
                        stackTrace += MiscHelpers.FormatInvariant("    at {0}\n", description);
                    }
                    else
                    {
                        documentContext.GetDocument(out var document);
                        var documentText = (IDebugDocumentText)document;

                        document.GetName(DocumentNameType.UniqueTitle, out var documentName);
                        documentText.GetPositionOfContext(documentContext, out var position, out var length);

                        using (var bufferBlock = new CoTaskMemArrayBlock(sizeof(char), (int)length))
                        {
                            uint lengthReturned = 0;
                            documentText.GetText(position, bufferBlock.Addr, IntPtr.Zero, ref lengthReturned, length);
                            var codeLine = Marshal.PtrToStringUni(bufferBlock.Addr, (int)lengthReturned);

                            documentText.GetLineOfPosition(position, out var lineNumber, out var offsetInLine);
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

        internal void VerifyNotDisposed()
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
                if (item is IScriptMarshalWrapper scriptMarshalWrapper)
                {
                    item = scriptMarshalWrapper.Unwrap();
                    if (ReferenceEquals(item, scriptMarshalWrapper))
                    {
                        break;
                    }

                    continue;
                }

                if (item is HostTarget hostTarget)
                {
                    item = hostTarget.Target;
                    if (ReferenceEquals(item, hostTarget))
                    {
                        break;
                    }

                    continue;
                }

                if ((item != null) && (item.GetType().IsCOMObject || item.GetType().IsCOMVisible()))
                {
                    directAccessItem = item;
                    return true;
                }

                break;
            }

            directAccessItem = null;
            return false;
        }

        private object MarshalToScriptInternal(object obj, HostItemFlags flags, HashSet<Array> marshaledArraySet)
        {
            if (obj == null)
            {
                if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalNullAsDispatch))
                {
                    return nullDispatch;
                }

                obj = NullExportValue;
            }

            if (obj == null)
            {
                return DBNull.Value;
            }

            if (obj is DBNull)
            {
                return obj;
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

            if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalDateTimeAsDate) && (obj is DateTime))
            {
                return obj;
            }

            if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalDecimalAsCurrency) && (obj is decimal))
            {
                return new CurrencyWrapper(obj);
            }

            if (obj is HostItem hostItem)
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

            if (obj is ScriptItem scriptItem)
            {
                if (scriptItem.Engine == this)
                {
                    return scriptItem.Unwrap();
                }
            }

            if (engineFlags.HasFlag(WindowsScriptEngineFlags.MarshalArraysByValue))
            {
                if ((obj is Array array) && ((hostTarget == null) || hostTarget.Type.IsArray))
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
                return UndefinedImportValue;
            }

            if (obj is DBNull)
            {
                return null;
            }

            if (MiscHelpers.TryMarshalPrimitiveToHost(obj, DisableFloatNarrowing, out var result))
            {
                return result;
            }

            if (obj is Array array)
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

            if (obj is HostTarget hostTarget)
            {
                return preserveHostTarget ? hostTarget : hostTarget.Target;
            }

            if (obj is HostItem hostItem)
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
                throw new COMException(exception.Message, HResult.CLEARSCRIPT_E_HOSTEXCEPTION);
            }
        }

        private void ThrowScriptError(Exception exception)
        {
            if (exception is COMException comException)
            {
                if (comException.ErrorCode == HResult.SCRIPT_E_REPORTED)
                {
                    // a script error was reported; the corresponding exception should be in the script frame
                    ThrowScriptError(CurrentScriptFrame.ScriptError ?? CurrentScriptFrame.PendingScriptError);
                }
                else if (comException.ErrorCode == HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                {
                    // A host exception surrogate passed through the COM boundary; this happens
                    // when some script engines are invoked via script item access rather than
                    // script execution. Use the exception in the script frame if one is available.
                    // Otherwise chain the host exception to a new script exception.

                    ThrowScriptError(CurrentScriptFrame.ScriptError ?? CurrentScriptFrame.PendingScriptError);

                    var hostException = CurrentScriptFrame.HostException;
                    if (hostException != null)
                    {
                        throw new ScriptEngineException(Name, hostException.Message, null, HResult.CLEARSCRIPT_E_HOSTEXCEPTION, false, true, null, hostException);
                    }
                }
                else
                {
                    // It's likely that an error occurred in a DirectAccess object. Throw the exception
                    // in the script frame if one is available; otherwise do nothing.

                    ThrowScriptError(CurrentScriptFrame.ScriptError ?? CurrentScriptFrame.PendingScriptError);
                }
            }
            else
            {
                // It's likely that an error occurred in a DirectAccess object. Throw the exception
                // in the script frame if one is available; otherwise do nothing.

                ThrowScriptError(CurrentScriptFrame.ScriptError ?? CurrentScriptFrame.PendingScriptError);
            }
        }

        #endregion

        #region ScriptEngine overrides (public members)

        /// <summary>
        /// Allows the host to access script resources dynamically.
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
        /// Allows the host to access script resources.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It allows you to access global script resources via the
        /// <c><see cref="ScriptObject"/></c> class interface. Doing so is likely to perform better than
        /// dynamic access via <c><see cref="Script"/></c>.
        /// </remarks>
        public override ScriptObject Global
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
        /// The <c><see cref="WindowsScriptEngine"/></c> version of this method returns the empty string
        /// if script debugging features have not been enabled for the instance.
        /// </para>
        /// </remarks>
        public override string GetStackTrace()
        {
            VerifyNotDisposed();
            return (processDebugManager != null) ? ScriptInvoke(GetStackTraceInternal) : string.Empty;
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

            var excepInfo = new EXCEPINFO { scode = HResult.E_ABORT };
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

        internal override IUniqueNameManager DocumentNameManager { get; } = new UniqueFileNameManager();

        internal override void AddHostItem(string itemName, HostItemFlags flags, object item)
        {
            VerifyNotDisposed();

            MiscHelpers.VerifyNonNullArgument(itemName, nameof(itemName));
            Debug.Assert(item != null);

            ScriptInvoke(() =>
            {
                if (!flags.HasFlag(HostItemFlags.DirectAccess) || !GetDirectAccessItem(item, out var marshaledItem))
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
                catch
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
            if (!evaluate)
            {
                const ScriptTextFlags flags = ScriptTextFlags.IsVisible;
                Parse(documentInfo, code, flags, IntPtr.Zero);
                return null;
            }

            using (var resultVariantBlock = new CoTaskMemVariantBlock())
            {
                const ScriptTextFlags flags = ScriptTextFlags.IsExpression;
                Parse(documentInfo, code, flags, resultVariantBlock.Addr);
                return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
            }
        }

        internal override HostItemCollateral HostItemCollateral { get; } = new HostItemCollateral();

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
            syncInvoker.Invoke(action);
        }

        internal override T SyncInvoke<T>(Func<T> func)
        {
            return syncInvoker.Invoke(func);
        }

        #endregion

        #region ScriptEngine overrides (disposal / finalization)

        /// <summary>
        /// Releases the unmanaged resources used by the script engine and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// This method is called by the public <c><see cref="ScriptEngine.Dispose()"/></c> method and the
        /// <c><see cref="ScriptEngine.Finalize">Finalize</see></c> method.
        /// <c><see cref="ScriptEngine.Dispose()"/></c> invokes the protected <c>Dispose(Boolean)</c>
        /// method with the <paramref name="disposing"/> parameter set to <c>true</c>.
        /// <c><see cref="ScriptEngine.Finalize">Finalize</see></c> invokes <c>Dispose(Boolean)</c> with
        /// <paramref name="disposing"/> set to <c>false</c>.
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (disposedFlag.Set())
                {
                    base.Dispose(true);
                    syncInvoker.VerifyAccess();

                    if (sourceManagement)
                    {
                        debugDocumentMap.Values.ForEach(debugDocument => debugDocument.Close());
                    }

                    if (processDebugManager != null)
                    {
                        processDebugManager.RemoveApplication(debugApplicationCookie);
                        debugApplication.Close();
                    }

                    script.Dispose();
                    activeScript.Close();
                }
            }
            else
            {
                base.Dispose(false);
            }
        }

        #endregion

        #region unit test support

        internal IEnumerable<string> GetDebugDocumentNames()
        {
            return debugDocumentMap.Values.Select(debugDocument =>
            {
                debugDocument.GetName(DocumentNameType.UniqueTitle, out var name);
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
