// 
// Copyright (c) Microsoft Corporation. All rights reserved.
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

        private class ScriptSite : IActiveScriptSite, IActiveScriptSiteInterruptPoll, IActiveScriptSiteDebug32, IActiveScriptSiteDebug64, IActiveScriptSiteDebugEx, ICustomQueryInterface
        {
            private readonly WindowsScriptEngine engine;

            public ScriptSite(WindowsScriptEngine engine)
            {
                this.engine = engine;
            }

            private string GetDetails(object error, string message)
            {
                if (engine.engineFlags.HasFlag(WindowsScriptEngineFlags.EnableDebugging))
                {
                    try
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
                        document.GetName(DocumentNameType.Title, out documentName);

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
                        engine.CurrentScriptFrame.ScriptError = new ScriptInterruptedException(engine.Name, description, GetDetails(error, description), excepInfo.scode, false, null);
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

                        engine.CurrentScriptFrame.ScriptError = new ScriptEngineException(engine.Name, description, GetDetails(error, description), excepInfo.scode, false, innerException);
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

                return keepGoing ? RawCOMHelpers.HResult.S_OK : MiscHelpers.SignedAsUnsigned(RawCOMHelpers.HResult.E_ABORT);
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
                        engine.CurrentScriptFrame.PendingScriptError = new ScriptInterruptedException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, null);
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

                        engine.CurrentScriptFrame.PendingScriptError = new ScriptEngineException(engine.Name, description, GetDetails(errorDebug, description), excepInfo.scode, false, innerException);
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
                    var debuggingEnabled = engine.engineFlags.HasFlag(WindowsScriptEngineFlags.EnableDebugging);
                    return debuggingEnabled ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }

                return CustomQueryInterfaceResult.NotHandled;
            }

            #endregion
        }

        #endregion
    }
}
