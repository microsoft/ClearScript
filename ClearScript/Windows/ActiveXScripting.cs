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
using System.Runtime.InteropServices;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Windows
{
    #region enums

    [Flags]
    internal enum ScriptInfoFlags : uint
    {
        // ReSharper disable InconsistentNaming

        None = 0,
        IUnknown = 0x00000001,
        ITypeInfo = 0x00000002

        // ReSharper restore InconsistentNaming
    }

    [Flags]
    internal enum ScriptInterruptFlags : uint
    {
        None = 0,
        Debug = 0x00000001,
        RaiseException = 0x00000002
    }

    [Flags]
    internal enum ScriptItemFlags : uint
    {
        None = 0,
        IsVisible = 0x00000002,
        IsSource = 0x00000004,
        GlobalMembers = 0x00000008,
        IsPersistent = 0x00000040,
        CodeOnly = 0x00000200,
        NoCode = 0x00000400
    }

    internal enum ScriptState : uint
    {
        Uninitialized = 0,
        Initialized = 5,
        Started = 1,
        Connected = 2,
        Disconnected = 3,
        Closed = 4
    }

    internal enum ScriptThreadState : uint
    {
        NotInScript = 0,
        Running = 1
    }

    [Flags]
    internal enum ScriptTypeLibFlags : uint
    {
        None = 0,
        IsControl = 0x00000010,
        IsPersistent = 0x00000040
    }

    [Flags]
    internal enum ScriptTextFlags : uint
    {
        None = 0,
        DelayExecution = 0x00000001,
        IsVisible = 0x00000002,
        IsExpression = 0x00000020,
        IsPersistent = 0x00000040,
        HostManagesSource = 0x00000080,
        IsXDomain = 0x00000100
    }

    internal enum ScriptGCType
    {
        Normal = 0,
        Exhaustive = 1
    }

    #endregion

    #region constants

    internal static class ScriptThreadID
    {
        public const uint Current = 0xFFFFFFFF;
        public const uint Base = 0xFFFFFFFE;
        public const uint All = 0xFFFFFFFD;
    }

    #endregion

    #region interfaces

    [ComImport]
    [Guid("bb1a2ae1-a4f9-11cf-8f20-00805f2cd064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScript
    {
        void SetScriptSite(
            [In] IActiveScriptSite site
        );

        void GetScriptSite(
            [In] ref Guid iid,
            [Out] [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 0)] out object site
        );

        void SetScriptState(
            [In] ScriptState state
        );

        void GetScriptState(
            [Out] out ScriptState state
        );

        void Close();

        void AddNamedItem(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] ScriptItemFlags flags
        );

        void AddTypeLib(
            [In] ref Guid libid,
            [In] uint major,
            [In] uint minor,
            [In] ScriptTypeLibFlags flags
        );

        void GetScriptDispatch(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string itemName,
            [Out] [MarshalAs(UnmanagedType.IDispatch)] out object dispatch
        );

        void GetCurrentScriptThreadID(
            [Out] out uint scriptThreadID
        );

        void GetScriptThreadID(
            [In] uint win32ThreadID,
            [Out] out uint scriptThreadID
        );

        void GetScriptThreadState(
            [In] uint scriptThreadID,
            [Out] out ScriptThreadState state
        );

        void InterruptScriptThread(
            [In] uint scriptThreadID,
            [In] ref EXCEPINFO excepInfo,
            [In] ScriptInterruptFlags flags
        );

        void Clone(
            [Out] [MarshalAs(UnmanagedType.Interface)] out IActiveScript script
        );
    }

    [ComImport]
    [Guid("bb1a2ae2-a4f9-11cf-8f20-00805f2cd064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptParse32
    {
        void InitNew();

        void AddScriptlet(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string defaultName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string itemName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string subItemName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string eventName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] uint sourceContext,
            [In] uint startingLineNumber,
            [In] ScriptTextFlags flags,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name,
            [Out] out EXCEPINFO excepInfo
        );

        void ParseScriptText(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string itemName,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object context,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] uint sourceContext,
            [In] uint startingLineNumber,
            [In] ScriptTextFlags flags,
            [Out] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo
        );
    }

    [ComImport]
    [Guid("c7ef7658-e1ee-480e-97ea-d52cb4d76d17")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptParse64
    {
        void InitNew();

        void AddScriptlet(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string defaultName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string itemName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string subItemName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string eventName,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ulong sourceContext,
            [In] uint startingLineNumber,
            [In] ScriptTextFlags flags,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name,
            [Out] out EXCEPINFO excepInfo
        );

        void ParseScriptText(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string code,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string itemName,
            [In] [MarshalAs(UnmanagedType.IUnknown)] object context,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string delimiter,
            [In] ulong sourceContext,
            [In] uint startingLineNumber,
            [In] ScriptTextFlags flags,
            [Out] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo
        );
    }

    [ComImport]
    [Guid("db01a1e3-a42b-11cf-8f20-00805f2cd064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSite
    {
        void GetLCID(
            [Out] out uint lcid
        );

        void GetItemInfo(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string name,
            [In] ScriptInfoFlags mask,
            [In] [Out] ref IntPtr pUnkItem,
            [In] [Out] ref IntPtr pTypeInfo
        );

        void GetDocVersionString(
            [Out] [MarshalAs(UnmanagedType.BStr)] out string version
        );

        void OnScriptTerminate(
            [In] IntPtr pVarResult,
            [In] ref EXCEPINFO excepInfo
        );

        void OnStateChange(
            [In] ScriptState state
        );

        void OnScriptError(
            [In] IActiveScriptError error
        );

        void OnEnterScript();

        void OnLeaveScript();
    }

    [ComImport]
    [Guid("539698a0-cdca-11cf-a5eb-00aa0047a063")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptSiteInterruptPoll
    {
        [PreserveSig]
        uint QueryContinue();
    }

    [ComImport]
    [Guid("eae1ba61-a4ed-11cf-8f20-00805f2cd064")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptError
    {
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
    }

    [ComImport]
    [Guid("b21fb2a1-5b8f-4963-8c21-21450f84ed7f")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptError64 // : IActiveScriptError
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

        void GetSourcePosition64(
            [Out] out ulong sourceContext,
            [Out] out uint lineNumber,
            [Out] out int position
        );
    }

    [ComImport]
    [Guid("6aa2c4a0-2b53-11d4-a2a0-00104bd35090")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IActiveScriptGarbageCollector
    {
        void CollectGarbage(
            [In] ScriptGCType type
        );
    }

    #endregion
}
