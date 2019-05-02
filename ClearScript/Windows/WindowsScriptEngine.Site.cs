// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    public abstract partial class WindowsScriptEngine
    {
        #region Nested type: ScriptSite

        private sealed class ScriptSite : IActiveScriptSite, IActiveScriptSiteInterruptPoll, IActiveScriptSiteWindow, IActiveScriptSiteDebug32, IActiveScriptSiteDebug64, IActiveScriptSiteDebugEx, ICustomQueryInterface
        {
            private readonly WindowsScriptEngine engine;

            public ScriptSite(WindowsScriptEngine engine)
            {
                this.engine = engine;
            }

            private string GetDetails(object error, string message)
            {
                if (engine.processDebugManager != null)
                {
                    try
                    {
                        var syntaxError = false;

                        var scriptError = error as IActiveScriptError;
                        if (scriptError != null)
                        {
                            EXCEPINFO excepInfo;
                            scriptError.GetExceptionInfo(out excepInfo);
                            if (RawCOMHelpers.HResult.GetFacility(excepInfo.scode) == RawCOMHelpers.HResult.FACILITY_CONTROL)
                            {
                                syntaxError = engine.SyntaxErrorMap.ContainsKey(RawCOMHelpers.HResult.GetCode(excepInfo.scode));
                            }
                        }

                        if (syntaxError)
                        {
                            var details = message;

                            var errorLocation = GetErrorLocation(error);
                            if (!string.IsNullOrWhiteSpace(errorLocation))
                            {
                                details += "\n" + errorLocation;
                            }

                            var stackTrace = engine.GetStackTraceInternal();
                            if (!string.IsNullOrWhiteSpace(stackTrace))
                            {
                                details += "\n" + stackTrace;
                            }

                            return details;
                        }
                        else
                        {
                            var stackTrace = engine.GetStackTraceInternal();
                            if (!string.IsNullOrWhiteSpace(stackTrace))
                            {
                                return message + "\n" + stackTrace;
                            }

                            var errorLocation = GetErrorLocation(error);
                            if (!string.IsNullOrWhiteSpace(errorLocation))
                            {
                                return message + "\n" + errorLocation;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.Assert(false, "Exception caught during error processing", exception.ToString());
                    }
                }

                return message;
            }

            private string GetErrorLocation(object error)
            {
                var scriptError = error as IActiveScriptError;
                if (scriptError != null)
                {
                    uint sourceContext;
                    uint lineNumber;
                    int offsetInLine;
                    scriptError.GetSourcePosition(out sourceContext, out lineNumber, out offsetInLine);

                    DebugDocument document;
                    if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out document))
                    {
                        string documentName;
                        document.GetName(DocumentNameType.UniqueTitle, out documentName);

                        int position;
                        if (lineNumber > 0)
                        {
                            uint linePosition;
                            document.GetPositionOfLine(lineNumber, out linePosition);
                            position = (int)linePosition + offsetInLine;
                        }
                        else
                        {
                            position = offsetInLine;
                        }

                        var text = new string(document.Code.Skip(position).TakeWhile(ch => ch != '\n').ToArray());
                        return MiscHelpers.FormatInvariant("    at ({0}:{1}:{2}) -> {3}", documentName, lineNumber, offsetInLine, text);
                    }
                }

                return null;
            }

            #region IActiveScriptSite implementation

            public void GetLCID(out uint lcid)
            {
                lcid = (uint)CultureInfo.CurrentCulture.LCID;
            }

            public void GetItemInfo(string name, ScriptInfoFlags mask, ref IntPtr pUnkItem, ref IntPtr pTypeInfo)
            {
                var item = engine.hostItemMap[name];

                if (mask.HasFlag(ScriptInfoFlags.IUnknown))
                {
                    pUnkItem = Marshal.GetIDispatchForObject(item);
                }

                if (mask.HasFlag(ScriptInfoFlags.ITypeInfo))
                {
                    pTypeInfo = Marshal.GetITypeInfoForType(item.GetType());
                }
            }

            public void GetDocVersionString(out string version)
            {
                throw new NotImplementedException();
            }

            public void OnScriptTerminate(IntPtr pVarResult, ref EXCEPINFO excepInfo)
            {
            }

            public void OnStateChange(ScriptState state)
            {
            }

            public void OnScriptError(IActiveScriptError error)
            {
                if ((engine.CurrentScriptFrame != null) && (error != null))
                {
                    EXCEPINFO excepInfo;
                    error.GetExceptionInfo(out excepInfo);
                    if (excepInfo.scode == RawCOMHelpers.HResult.E_ABORT)
                    {
                        // Script execution was interrupted explicitly. At this point the script
                        // engine might be in an odd state; the following call seems to get it back
                        // to normal.

                        engine.activeScript.SetScriptState(ScriptState.Started);

                        var description = excepInfo.bstrDescription ?? "Script execution interrupted by host";
                        engine.CurrentScriptFrame.ScriptError = new ScriptInterruptedException(engine.Name, description, GetDetails(error, description), excepInfo.scode, false, true, null, null);
                    }
                    else
                    {
                        var description = excepInfo.bstrDescription;

                        Exception innerException;
                        if (excepInfo.scode != RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                        {
                            innerException = null;
                        }
                        else
                        {
                            innerException = engine.CurrentScriptFrame.HostException;
                            if ((innerException != null) && string.IsNullOrWhiteSpace(description))
                            {
                                description = innerException.Message;
                            }
                        }

                        engine.CurrentScriptFrame.ScriptError = new ScriptEngineException(engine.Name, description, GetDetails(error, description), excepInfo.scode, false, true, null, innerException);
                    }
                }
            }

            public void OnEnterScript()
            {
                Debug.Assert(engine.CheckAccess());
            }

            public void OnLeaveScript()
            {
            }

            #endregion

            #region IActiveScriptSiteInterruptPoll implementation

            public uint QueryContinue()
            {
                var callback = engine.ContinuationCallback;
                var keepGoing = ((callback == null) || callback());
                if (engine.CurrentScriptFrame != null)
                {
                    keepGoing = keepGoing && !engine.CurrentScriptFrame.InterruptRequested;
                }

                return keepGoing ? RawCOMHelpers.HResult.S_OK : RawCOMHelpers.HResult.E_ABORT.ToUnsigned();
            }

            #endregion

            #region IActiveScriptSiteWindow implementation

            public void GetWindow(out IntPtr hwnd)
            {
                var hostWindow = engine.HostWindow;
                hwnd = (hostWindow != null) ? hostWindow.OwnerHandle : IntPtr.Zero;
            }

            public void EnableModeless(bool enable)
            {
                var hostWindow = engine.HostWindow;
                if (hostWindow != null)
                    hostWindow.EnableModeless(enable);
            }

            #endregion

            #region IActiveScriptSiteDebug32 implementation

            public void GetDocumentContextFromPosition(uint sourceContext, uint offset, uint length, out IDebugDocumentContext context)
            {
                context = null;
                DebugDocument document;
                if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out document))
                {
                    document.GetContextOfPosition(offset, length, out context);
                }
            }

            public void GetApplication(out IDebugApplication32 application)
            {
                application = DebugApplicationWrapper32.Unwrap(engine.debugApplication);
            }

            public void GetRootApplicationNode(out IDebugApplicationNode node)
            {
                engine.debugApplication.GetRootNode(out node);
            }

            public void OnScriptErrorDebug(IActiveScriptErrorDebug errorDebug, out bool enterDebugger, out bool callOnScriptErrorWhenContinuing)
            {
                if ((engine.CurrentScriptFrame != null) && (errorDebug != null))
                {
                    EXCEPINFO excepInfo;
                    errorDebug.GetExceptionInfo(out excepInfo);
                    if (excepInfo.scode == RawCOMHelpers.HResult.E_ABORT)
                    {
                        var description = excepInfo.bstrDescription ?? "Script execution interrupted by host";
                        engine.CurrentScriptFrame.PendingScriptError = new ScriptInterruptedException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, true, null, null);
                    }
                    else
                    {
                        var description = excepInfo.bstrDescription;

                        Exception innerException;
                        if (excepInfo.scode != RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                        {
                            innerException = null;
                        }
                        else
                        {
                            innerException = engine.CurrentScriptFrame.HostException;
                            if ((innerException != null) && string.IsNullOrWhiteSpace(description))
                            {
                                description = innerException.Message;
                            }
                        }

                        engine.CurrentScriptFrame.PendingScriptError = new ScriptEngineException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, true, null, innerException);
                    }
                }

                enterDebugger = engine.engineFlags.HasFlag(WindowsScriptEngineFlags.EnableJITDebugging);
                callOnScriptErrorWhenContinuing = true;
            }

            #endregion

            #region IActiveScriptSiteDebug64 implementation

            public void GetDocumentContextFromPosition(ulong sourceContext, uint offset, uint length, out IDebugDocumentContext context)
            {
                context = null;
                DebugDocument document;
                if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out document))
                {
                    document.GetContextOfPosition(offset, length, out context);
                }
            }

            public void GetApplication(out IDebugApplication64 application)
            {
                application = DebugApplicationWrapper64.Unwrap(engine.debugApplication);
            }

            #endregion

            #region IActiveScriptSiteDebugEx implementation

            public void OnCanNotJITScriptErrorDebug(IActiveScriptErrorDebug errorDebug, out bool callOnScriptErrorWhenContinuing)
            {
                bool enterDebugger;
                OnScriptErrorDebug(errorDebug, out enterDebugger, out callOnScriptErrorWhenContinuing);
            }

            #endregion

            #region ICustomQueryInterface implementation

            public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr pInterface)
            {
                pInterface = IntPtr.Zero;

                if ((iid == typeof(IActiveScriptSiteDebug32).GUID) || (iid == typeof(IActiveScriptSiteDebug64).GUID) || (iid == typeof(IActiveScriptSiteDebugEx).GUID))
                {
                    return (engine.processDebugManager != null) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }

                if (iid == typeof(IActiveScriptSiteWindow).GUID)
                {
                    return (engine.HostWindow != null) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }

                return CustomQueryInterfaceResult.NotHandled;
            }

            #endregion
        }

        #endregion
    }
}
