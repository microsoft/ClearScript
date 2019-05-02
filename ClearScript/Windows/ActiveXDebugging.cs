// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    #region enums

    [Flags]
    internal enum AppBreakFlags : uint
    {
        None = 0,
        DebuggerBlock = 0x00000001,
        DebuggerHalt = 0x00000002,
        Step = 0x00010000,
        Nested = 0x00020000,
        SteptypeSource = 0x00000000,
        SteptypeBytecode = 0x00100000,
        SteptypeMachine = 0x00200000,
        SteptypeMask = 0x00F00000,
        InBreakpoint = 0x80000000
    }

    internal enum BreakReason
    {
        Step,
        Breakpoint,
        DebuggerBlock,
        HostInitiated,
        LanguageInitiated,
        DebuggerHalt,
        Error,
        JIT
    }

    internal enum BreakResumeAction
    {
        Abort,
        Continue,
        StepInto,
        StepOver,
        StepOut,
        Ignore
    }

    internal enum ErrorResumeAction
    {
        ReexecuteErrorStatement,
        AbortCallAndReturnErrorToCaller,
        SkipErrorStatement
    }

    [Flags]
    internal enum TextDocAttrs : uint
    {
        None = 0,
        ReadOnly = 0x00000001
    }

    [Flags]
    internal enum SourceTextAttrs : ushort
    {
        None = 0,
        Keyword = 0x0001,
        Comment = 0x0002,
        NonSource = 0x0004,
        Operator = 0x0008,
        Number = 0x0010,
        String = 0x0020,
        FunctionStart = 0x0040
    }

    internal enum DocumentNameType
    {
        AppNode,
        Title,
        FileTail,
        URL,
        UniqueTitle,
        SourceMapURL
    }

    internal enum BreakpointState
    {
        Deleted,
        Disabled,
        Enabled
    }

    #endregion

    #region structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct DebugStackFrameDescriptor
    {
        [MarshalAs(UnmanagedType.Interface)]
        public IDebugStackFrame Frame;

        public uint Minimum;
        public uint Limit;

        [MarshalAs(UnmanagedType.Bool)]
        public bool IsFinal;

        public IntPtr pFinalObject;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DebugStackFrameDescriptor64
    {
        [MarshalAs(UnmanagedType.Interface)]
        public IDebugStackFrame Frame;

        public ulong Minimum;
        public ulong Limit;

        [MarshalAs(UnmanagedType.Bool)]
        public bool IsFinal;

        public IntPtr pFinalObject;
    }

    #endregion

    #region interfaces

    [ComImport]
    [Guid("51973c10-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptDebug32
    {
        void GetScriptTextAttributes(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] uint length,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ScriptTextFlags flags,
            [In] IntPtr pAttrs
        );

        void GetScriptletTextAttributes(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] uint length,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ScriptTextFlags flags,
            [In] IntPtr pAttrs
        );

        void EnumCodeContextsOfPosition(
            [In] uint sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );
    }

    [ComImport]
    [Guid("bc437e23-f5b8-47f4-bb79-7d1ce5483b86")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptDebug64
    {
        void GetScriptTextAttributes(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] uint length,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ScriptTextFlags flags,
            [In] IntPtr pAttrs
        );

        void GetScriptletTextAttributes(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] uint length,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ScriptTextFlags flags,
            [In] IntPtr pAttrs
        );

        void EnumCodeContextsOfPosition(
            [In] ulong sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );
    }

    [ComImport]
    [Guid("51973c11-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSiteDebug32
    {
        void GetDocumentContextFromPosition(
            [In] uint sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentContext context
        );

        void GetApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication32 application
        );

        void GetRootApplicationNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void OnScriptErrorDebug(
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptErrorDebug errorDebug,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool enterDebugger,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool callOnScriptErrorWhenContinuing
        );
    }

    [ComImport]
    [Guid("d6b96b0a-7463-402c-92ac-89984226942f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSiteDebug64
    {
        void GetDocumentContextFromPosition(
            [In] ulong sourceContext,
            [In] uint offset,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentContext context
        );

        void GetApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication64 application
        );

        void GetRootApplicationNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void OnScriptErrorDebug(
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptErrorDebug errorDebug,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool enterDebugger,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool callOnScriptErrorWhenContinuing
        );
    }

    [ComImport]
    [Guid("bb722ccb-6ad2-41c6-b780-af9c03ee69f5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSiteDebugEx
    {
        void OnCanNotJITScriptErrorDebug(
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptErrorDebug errorDebug,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool callOnScriptErrorWhenContinuing
        );
    }

    [ComImport]
    [Guid("51973c2f-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProcessDebugManager32
    {
        void CreateApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication32 application
        );

        void GetDefaultApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication32 application
        );

        [PreserveSig]
        uint AddApplication(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugApplication32 application,
            [Out] out uint appCookie);

        void RemoveApplication(
            [In] uint appCookie
        );

        void CreateDebugDocumentHelper(
            [In] [MarshalAs(UnmanagedType.IUnknown)] object outer,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentHelper32 helper
        );
    }

    [ComImport]
    [Guid("56b9fc1c-63a9-4cc1-ac21-087d69a17fab")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProcessDebugManager64
    {
        void CreateApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication64 application
        );

        void GetDefaultApplication(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplication64 application
        );

        [PreserveSig]
        uint AddApplication(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugApplication64 application,
            [Out] out uint appCookie
        );

        void RemoveApplication(
            [In] uint appCookie
        );

        void CreateDebugDocumentHelper(
            [In] [MarshalAs(UnmanagedType.IUnknown)] object outer,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentHelper64 helper
        );
    }

    [ComImport]
    [Guid("51973c30-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IRemoteDebugApplication
    {
        void ResumeFromBreakPoint(
            [In] [MarshalAs(UnmanagedType.Interface)] IRemoteDebugApplicationThread thread,
            [In] BreakResumeAction breakResumeAction,
            [In] ErrorResumeAction errorResumeAction
        );

        void CauseBreak();

        void ConnectDebugger(
            [In] [MarshalAs(UnmanagedType.Interface)] IApplicationDebugger debugger
        );

        void DisconnectDebugger();

        [PreserveSig]
        uint GetDebugger(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IApplicationDebugger debugger
        );

        void CreateInstanceAtApplication(
            [In] ref Guid clsid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object outer,
            [In] uint clsContext,
            [In] ref Guid iid,
            [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 3)] out object instance
        );

        void QueryAlive();

        void EnumThreads(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumRemoteDebugApplicationThreads enumThreads
        );

        void GetName(
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetRootNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void EnumGlobalExpressionContexts(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugExpressionContexts enumContexts
        );
    }

    [ComImport]
    [Guid("51973c32-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugApplication32 // : IRemoteDebugApplication
    {
        #region IRemoteDebugApplication methods

        void ResumeFromBreakPoint(
            [In] [MarshalAs(UnmanagedType.Interface)] IRemoteDebugApplicationThread thread,
            [In] BreakResumeAction breakResumeAction,
            [In] ErrorResumeAction errorResumeAction
        );

        void CauseBreak();

        void ConnectDebugger(
            [In] [MarshalAs(UnmanagedType.Interface)] IApplicationDebugger debugger
        );

        void DisconnectDebugger();

        [PreserveSig]
        uint GetDebugger(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IApplicationDebugger debugger
        );

        void CreateInstanceAtApplication(
            [In] ref Guid clsid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object outer,
            [In] uint clsContext,
            [In] ref Guid iid,
            [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 3)] out object instance
        );

        void QueryAlive();

        void EnumThreads(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumRemoteDebugApplicationThreads enumThreads
        );

        void GetName(
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetRootNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void EnumGlobalExpressionContexts(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugExpressionContexts enumContexts
        );

        #endregion

        void SetName(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string name
        );

        void StepOutComplete();

        void DebugOutput(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string str
        );

        void StartDebugSession();

        void HandleBreakPoint(
            [In] BreakReason reason,
            [Out] out BreakResumeAction resumeAction
        );

        void Close();

        void GetBreakFlags(
            [Out] out AppBreakFlags flags,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IRemoteDebugApplicationThread thread
        );

        void GetCurrentThread(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationThread thread
        );

        void CreateAsyncDebugOperation(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugSyncOperation syncOperation,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugAsyncOperation asyncOperation
        );

        void AddStackFrameSniffer(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugStackFrameSniffer sniffer,
            [Out] out uint cookie
        );

        void RemoveStackFrameSniffer(
            [In] uint cookie
        );

        [PreserveSig]
        uint QueryCurrentThreadIsDebuggerThread();

        void SynchronousCallInDebuggerThread(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugThreadCall32 call,
            [In] uint param1,
            [In] uint param2,
            [In] uint param3
        );

        void CreateApplicationNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void FireDebuggerEvent(
            [In] ref Guid iid,
            [In] [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 0)] object eventObject
        );

        void HandleRuntimeError(
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptErrorDebug errorDebug,
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptSite scriptSite,
            [Out] out BreakResumeAction breakResumeAction,
            [Out] out ErrorResumeAction errorResumeAction,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool callOnScriptError
        );

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool FCanJitDebug();

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool FIsAutoJitDebugEnabled();

        void AddGlobalExpressionContextProvider(
            [In] [MarshalAs(UnmanagedType.Interface)] IProvideExpressionContexts provider,
            [Out] out uint cookie
        );

        void RemoveGlobalExpressionContextProvider(
            [In] uint cookie
        );
    }

    [ComImport]
    [Guid("4dedc754-04c7-4f10-9e60-16a390fe6e62")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugApplication64 // : IRemoteDebugApplication
    {
        #region IRemoteDebugApplication methods

        void ResumeFromBreakPoint(
            [In] [MarshalAs(UnmanagedType.Interface)] IRemoteDebugApplicationThread thread,
            [In] BreakResumeAction breakResumeAction,
            [In] ErrorResumeAction errorResumeAction
        );

        void CauseBreak();

        void ConnectDebugger(
            [In] [MarshalAs(UnmanagedType.Interface)] IApplicationDebugger debugger
        );

        void DisconnectDebugger();

        [PreserveSig]
        uint GetDebugger(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IApplicationDebugger debugger
        );

        void CreateInstanceAtApplication(
            [In] ref Guid clsid,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object outer,
            [In] uint clsContext,
            [In] ref Guid iid,
            [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 3)] out object instance
        );

        void QueryAlive();

        void EnumThreads(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumRemoteDebugApplicationThreads enumThreads
        );

        void GetName(
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetRootNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void EnumGlobalExpressionContexts(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugExpressionContexts enumContexts
        );

        #endregion

        void SetName(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string name
        );

        void StepOutComplete();

        void DebugOutput(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string str
        );

        void StartDebugSession();

        void HandleBreakPoint(
            [In] BreakReason reason,
            [Out] out BreakResumeAction resumeAction
        );

        void Close();

        void GetBreakFlags(
            [Out] out AppBreakFlags flags,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IRemoteDebugApplicationThread thread
        );

        void GetCurrentThread(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationThread thread
        );

        void CreateAsyncDebugOperation(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugSyncOperation syncOperation,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugAsyncOperation asyncOperation
        );

        void AddStackFrameSniffer(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugStackFrameSniffer sniffer,
            [Out] out uint cookie
        );

        void RemoveStackFrameSniffer(
            [In] uint cookie
        );

        [PreserveSig]
        uint QueryCurrentThreadIsDebuggerThread();

        void SynchronousCallInDebuggerThread(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugThreadCall64 call,
            [In] ulong param1,
            [In] ulong param2,
            [In] ulong param3
        );

        void CreateApplicationNode(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void FireDebuggerEvent(
            [In] ref Guid iid,
            [In] [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 0)] object eventObject
        );

        void HandleRuntimeError(
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptErrorDebug errorDebug,
            [In] [MarshalAs(UnmanagedType.Interface)] IActiveScriptSite scriptSite,
            [Out] out BreakResumeAction breakResumeAction,
            [Out] out ErrorResumeAction errorResumeAction,
            [Out] [MarshalAs(UnmanagedType.Bool)] out bool callOnScriptError
        );

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool FCanJitDebug();

        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Bool)]
        bool FIsAutoJitDebugEnabled();

        void AddGlobalExpressionContextProvider(
            [In] [MarshalAs(UnmanagedType.Interface)] IProvideExpressionContexts provider,
            [Out] out uint cookie
        );

        void RemoveGlobalExpressionContextProvider(
            [In] uint cookie
        );
    }

    [ComImport]
    [Guid("51973c1f-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentInfo
    {
        [PreserveSig]
        uint GetName(
            [In] DocumentNameType type,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetDocumentClassId(
            [Out] out Guid clsid
        );
    }

    [ComImport]
    [Guid("51973c20-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentProvider // : IDebugDocumentInfo
    {
        #region IDebugDocumentInfo methods

        [PreserveSig]
        uint GetName(
            [In] DocumentNameType type,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetDocumentClassId(
            [Out] out Guid clsid
        );

        #endregion

        void GetDocument(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocument document
        );
    }

    [ComImport, Guid("51973c21-cb0c-11d0-b5c9-00a0244a0e7a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocument // : IDebugDocumentInfo
    {
        #region IDebugDocumentInfo methods

        [PreserveSig]
        uint GetName(
            [In] DocumentNameType type,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetDocumentClassId(
            [Out] out Guid clsid
        );

        #endregion
    }

    [ComImport]
    [Guid("51973c22-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentText // : IDebugDocument
    {
        #region IDebugDocumentInfo methods

        [PreserveSig]
        uint GetName(
            [In] DocumentNameType type,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetDocumentClassId(
            [Out] out Guid clsid
        );

        #endregion

        #region IDebugDocument methods

        #endregion

        void GetDocumentAttributes(
            [Out] out TextDocAttrs attrs
        );

        void GetSize(
            [Out] out uint numLines,
            [Out] out uint length
        );

        void GetPositionOfLine(
            [In] uint lineNumber,
            [Out] out uint position
        );

        void GetLineOfPosition(
            [In] uint position,
            [Out] out uint lineNumber,
            [Out] out uint offsetInLine
        );

        void GetText(
            [In] uint position,
            [In] IntPtr pChars,
            [In] IntPtr pAttrs,
            [In] [Out] ref uint length,
            [In] uint maxChars
        );

        void GetPositionOfContext(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugDocumentContext context,
            [Out] out uint position,
            [Out] out uint length
        );

        void GetContextOfPosition(
            [In] uint position,
            [In] uint length,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentContext context
        );
    }

    [ComImport]
    [Guid("51973c34-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugApplicationNode // : IDebugDocumentProvider
    {
        #region IDebugDocumentInfo methods

        [PreserveSig]
        uint GetName(
            [In] DocumentNameType type,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetDocumentClassId(
            [Out] out Guid clsid
        );

        #endregion

        #region IDebugDocumentProvider methods

        void GetDocument(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocument document
        );

        #endregion

        void EnumChildren(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugApplicationNodes enumNodes
        );

        void GetParent(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationNode node
        );

        void SetDocumentProvider(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugDocumentProvider provider
        );

        void Close();

        void Attach(
            [In] [MarshalAs(UnmanagedType.Interface)] IDebugApplicationNode node
        );

        void Detach();
    }

    [ComImport]
    [Guid("51973c28-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentContext
    {
        void GetDocument(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocument document
        );

        void EnumCodeContexts(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );
    }

    [ComImport]
    [Guid("51973c13-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugCodeContext
    {
        void GetDocumentContext(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugDocumentContext context
        );

        void SetBreakPoint(
            [In] BreakpointState state
        );
    }

    [ComImport]
    [Guid("51973c1d-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumDebugCodeContexts
    {
        void Next(
            [In] uint count,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugCodeContext context,
            [Out] out uint countFetched
        );

        void Skip(
            [In] uint count
        );

        void Reset();

        void Clone(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugCodeContexts enumContexts
        );
    };

    [ComImport]
    [Guid("51973c18-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugStackFrameSniffer
    {
        void EnumStackFrames( 
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );
    }

    [ComImport]
    [Guid("51973c19-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugStackFrameSnifferEx32 // : IDebugStackFrameSniffer
    {
        #region IDebugStackFrameSniffer methods

        void EnumStackFrames( 
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );

        #endregion

        void EnumStackFramesEx32( 
            [In] uint minimum,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );
    }

    [ComImport]
    [Guid("8cd12af4-49c1-4d52-8d8a-c146f47581aa")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugStackFrameSnifferEx64 // : IDebugStackFrameSniffer
    {
        #region IDebugStackFrameSniffer methods

        void EnumStackFrames( 
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );

        #endregion

        void EnumStackFramesEx64( 
            [In] ulong minimum,
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );
    }

    [ComImport]
    [Guid("51973c1e-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumDebugStackFrames
    {
        void Next(
            [In] uint count,
            [Out] out DebugStackFrameDescriptor descriptor,
            [Out] out uint countFetched
        );

        void Skip(
            [In] uint count
        );

        void Reset();

        void Clone(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );
    };

    [ComImport]
    [Guid("0dc38853-c1b0-4176-a984-b298361027af")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumDebugStackFrames64 // : IEnumDebugStackFrames
    {
        #region IEnumDebugStackFrames methods

        void Next(
            [In] uint count,
            [Out] out DebugStackFrameDescriptor descriptor,
            [Out] out uint countFetched
        );

        void Skip(
            [In] uint count
        );

        void Reset();

        void Clone(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IEnumDebugStackFrames enumFrames
        );

        #endregion

        void Next64(
            [In] uint count,
            [Out] out DebugStackFrameDescriptor64 descriptor,
            [Out] out uint countFetched
        );
    }

    [ComImport]
    [Guid("51973c17-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugStackFrame
    {
        void GetCodeContext( 
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugCodeContext context
        );
        
        void GetDescriptionString( 
            [In] [MarshalAs(UnmanagedType.Bool)] bool longString,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string description
        );
        
        void GetLanguageString( 
            [In] [MarshalAs(UnmanagedType.Bool)] bool longString,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string language
        );
        
        void GetThread( 
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugApplicationThread thread
        );
        
        void GetDebugProperty(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IDebugProperty property
        );
    }

    #endregion

    #region interface stubs

    [ComImport]
    [Guid("51973c26-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentHelper32
    {
        // methods omitted
    }

    [ComImport]
    [Guid("c4c7363c-20fd-47f9-bd82-4855e0150871")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentHelper64
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c37-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IRemoteDebugApplicationThread
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c3c-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumRemoteDebugApplicationThreads
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c38-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugApplicationThread // : IRemoteDebugApplicationThread
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c2a-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IApplicationDebugger
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c3a-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumDebugApplicationNodes
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c40-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumDebugExpressionContexts
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c1a-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugSyncOperation
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c1b-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugAsyncOperation
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c36-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugThreadCall32
    {
        // methods omitted
    }

    [ComImport]
    [Guid("cb3fa335-e979-42fd-9fcf-a7546a0f3905")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugThreadCall64
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c12-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptErrorDebug // : IActiveScriptError
    {
        #region IActiveScriptError methods

        void GetExceptionInfo(
            [Out] out EXCEPINFO excepInfo
        );

        void GetSourcePosition(
            [Out] out uint sourceContext,
            [Out] out uint lineNumber,
            [Out] out int position
        );

        void GetSourceLineText(
            [Out] [MarshalAs(UnmanagedType.BStr)] out string sourceLine
        );

        #endregion

        // methods omitted
    }

    [ComImport]
    [Guid("51973c41-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IProvideExpressionContexts
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c27-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugDocumentHost
    {
        // methods omitted
    }

    [ComImport]
    [Guid("51973c50-cb0c-11d0-b5c9-00a0244a0e7a")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDebugProperty
    {
        // methods omitted
    }

    #endregion
}
