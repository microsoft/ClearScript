// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows.Core
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
                if (engine.processDebugManager is not null)
                {
                    try
                    {
                        var syntaxError = false;

                        if (error is IActiveScriptError scriptError)
                        {
                            scriptError.GetExceptionInfo(out var excepInfo);
                            if (HResult.GetFacility(excepInfo.scode) == HResult.FACILITY_CONTROL)
                            {
                                syntaxError = engine.SyntaxErrorMap.ContainsKey(HResult.GetCode(excepInfo.scode));
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
                else
                {
                    var errorLocation = GetErrorLocation(error);
                    if (!string.IsNullOrWhiteSpace(errorLocation))
                    {
                        return message + "\n" + errorLocation;
                    }
                }

                return message;
            }

            private string GetErrorLocation(object error)
            {
                if (error is IActiveScriptError scriptError)
                {
                    scriptError.GetSourcePosition(out var sourceContext, out var lineNumber, out var offsetInLine);

                    if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out var document))
                    {
                        document.GetName(DocumentNameType.UniqueTitle, out var documentName);

                        int position;
                        if (lineNumber > 0)
                        {
                            document.GetPositionOfLine(lineNumber, out var linePosition);
                            position = (int)linePosition + offsetInLine;
                        }
                        else
                        {
                            position = offsetInLine;
                        }

                        var text = new string(document.Code.Skip(position).TakeWhile(ch => ch != '\n').ToArray());
                        return MiscHelpers.FormatInvariant("    at ({0}:{1}:{2}) -> {3}", documentName, lineNumber, offsetInLine, text);
                    }

                    return MiscHelpers.FormatInvariant("    at ([unknown]:{0}:{1})", lineNumber, offsetInLine);
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

                if (mask.HasAllFlags(ScriptInfoFlags.IUnknown))
                {
                    pUnkItem = Marshal.GetIUnknownForObject(item);
                }

                if (mask.HasAllFlags(ScriptInfoFlags.ITypeInfo))
                {
                    pTypeInfo = item.GetType().GetTypeInfo();
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
                if ((engine.CurrentScriptFrame is not null) && (error is not null))
                {
                    error.GetExceptionInfo(out var excepInfo);
                    if (excepInfo.scode == HResult.E_ABORT)
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
                        if (excepInfo.scode != HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                        {
                            innerException = null;
                        }
                        else
                        {
                            innerException = engine.CurrentScriptFrame.HostException;
                            if ((innerException is not null) && string.IsNullOrWhiteSpace(description))
                            {
                                description = innerException.GetBaseException().Message;
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
                var keepGoing = ((callback is null) || callback());
                if (engine.CurrentScriptFrame is not null)
                {
                    keepGoing = keepGoing && !engine.CurrentScriptFrame.InterruptRequested;
                }

                return keepGoing ? HResult.S_OK : HResult.E_ABORT.ToUnsigned();
            }

            #endregion

            #region IActiveScriptSiteWindow implementation

            public void GetWindow(out IntPtr hwnd)
            {
                var hostWindow = engine.HostWindow;
                hwnd = hostWindow?.OwnerHandle ?? IntPtr.Zero;
            }

            public void EnableModeless(bool enable)
            {
                var hostWindow = engine.HostWindow;
                hostWindow?.EnableModeless(enable);
            }

            #endregion

            #region IActiveScriptSiteDebug32 implementation

            public void GetDocumentContextFromPosition(uint sourceContext, uint offset, uint length, out IDebugDocumentContext context)
            {
                context = null;
                if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out var document))
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
                if ((engine.CurrentScriptFrame is not null) && (errorDebug is not null))
                {
                    errorDebug.GetExceptionInfo(out var excepInfo);
                    if (excepInfo.scode == HResult.E_ABORT)
                    {
                        var description = excepInfo.bstrDescription ?? "Script execution interrupted by host";
                        engine.CurrentScriptFrame.PendingScriptError = new ScriptInterruptedException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, true, null, null);
                    }
                    else
                    {
                        var description = excepInfo.bstrDescription;

                        Exception innerException;
                        if (excepInfo.scode != HResult.CLEARSCRIPT_E_HOSTEXCEPTION)
                        {
                            innerException = null;
                        }
                        else
                        {
                            innerException = engine.CurrentScriptFrame.HostException;
                            if ((innerException is not null) && string.IsNullOrWhiteSpace(description))
                            {
                                description = innerException.GetBaseException().Message;
                            }
                        }

                        engine.CurrentScriptFrame.PendingScriptError = new ScriptEngineException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, true, null, innerException);
                    }
                }

                enterDebugger = engine.engineFlags.HasAllFlags(WindowsScriptEngineFlags.EnableJITDebugging);
                callOnScriptErrorWhenContinuing = true;
            }

            #endregion

            #region IActiveScriptSiteDebug64 implementation

            public void GetDocumentContextFromPosition(ulong sourceContext, uint offset, uint length, out IDebugDocumentContext context)
            {
                context = null;
                if (engine.debugDocumentMap.TryGetValue(new UIntPtr(sourceContext), out var document))
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
                OnScriptErrorDebug(errorDebug, out _, out callOnScriptErrorWhenContinuing);
            }

            #endregion

            #region ICustomQueryInterface implementation

            public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr pInterface)
            {
                pInterface = IntPtr.Zero;

                if ((iid == typeof(IActiveScriptSiteDebug32).GUID) || (iid == typeof(IActiveScriptSiteDebug64).GUID) || (iid == typeof(IActiveScriptSiteDebugEx).GUID))
                {
                    return (engine.processDebugManager is not null) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }

                if (iid == typeof(IActiveScriptSiteWindow).GUID)
                {
                    return (engine.HostWindow is not null) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }

                return CustomQueryInterfaceResult.NotHandled;
            }

            #endregion
        }

        #endregion
    }
}
