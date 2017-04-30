// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DISPPARAMS = System.Runtime.InteropServices.ComTypes.DISPPARAMS;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Util
{
    #region enums

    [Flags]
    internal enum DispatchFlags : ushort
    {
        Method = 0x1,
        PropertyGet = 0x2,
        PropertyPut = 0x4,
        PropertyPutRef = 0x8,
        Construct = 0x4000
    }

    [Flags]
    internal enum DispatchNameFlags : uint
    {
        CaseSensitive = 0x00000001,
        Ensure = 0x00000002,
        Implicit = 0x00000004,
        CaseInsensitive = 0x00000008,
        Internal = 0x00000010,
        NoDynamicProperties = 0x00000020
    }

    [Flags]
    internal enum DispatchPropFlags : uint
    {
        CanGet = 0x00000001,
        CannotGet = 0x00000002,
        CanPut = 0x00000004,
        CannotPut = 0x00000008,
        CanPutRef = 0x00000010,
        CannotPutRef = 0x00000020,
        NoSideEffects = 0x00000040,
        DynamicType = 0x00000080,
        CanCall = 0x00000100,
        CannotCall = 0x00000200,
        CanConstruct = 0x00000400,
        CannotConstruct = 0x00000800,
        CanSourceEvents = 0x00001000,
        CannotSourceEvents = 0x00002000,
        CanAll = CanGet | CanPut | CanPutRef | CanCall | CanConstruct | CanSourceEvents,
        CannotAll = CannotGet | CannotPut | CannotPutRef | CannotCall | CannotConstruct | CannotSourceEvents,
        ExtraAll = NoSideEffects | DynamicType,
        All = CanAll | CannotAll | ExtraAll
    }

    [Flags]
    internal enum DispatchEnumFlags : uint
    {
        Default = 0x00000001,
        All = 0x00000002
    }

    #endregion

    #region interfaces

    [ComImport]
    [Guid("00020400-0000-0000-c000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDispatch
    {
        [PreserveSig]
        int GetTypeInfoCount(
            [Out] out uint count
        );

        [PreserveSig]
        int GetTypeInfo(
            [In] uint index,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.Interface)] out ITypeInfo typeInfo
        );

        void GetIDsOfNames(
            [In] ref Guid iid,
            [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] names,
            [In] uint count,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] dispids
        );

        void Invoke(
            [In] int dispid,
            [In] ref Guid iid,
            [In] int lcid,
            [In] DispatchFlags flags,
            [In] ref DISPPARAMS args,
            [In] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo,
            [Out] out uint argErr
        );
    }

    [ComImport]
    [Guid("a6ef9860-c720-11d0-9337-00a0c90dcaa9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDispatchEx // : IDispatch
    {
        #region IDispatch members

        [PreserveSig]
        int GetTypeInfoCount(
            [Out] out uint count
        );

        [PreserveSig]
        int GetTypeInfo(
            [In] uint index,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.Interface)] out ITypeInfo typeInfo
        );

        void GetIDsOfNames(
            [In] ref Guid iid,
            [In] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] names,
            [In] uint count,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] int[] dispids
        );

        void Invoke(
            [In] int dispid,
            [In] ref Guid iid,
            [In] int lcid,
            [In] DispatchFlags flags,
            [In] ref DISPPARAMS args,
            [In] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo,
            [Out] out uint argErr
        );

        #endregion

        [PreserveSig]
        int GetDispID(
            [In] [MarshalAs(UnmanagedType.BStr)] string name,
            [In] DispatchNameFlags flags,
            [Out] out int dispid
        );

        [PreserveSig]
        int InvokeEx(
            [In] int dispid,
            [In] int lcid,
            [In] DispatchFlags flags,
            [In] ref DISPPARAMS args,
            [In] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo,
            [In] [Optional] [MarshalAs(UnmanagedType.Interface)] IServiceProvider svpCaller
        );

        [PreserveSig]
        int DeleteMemberByName(
            [In] [MarshalAs(UnmanagedType.BStr)] string name,
            [In] DispatchNameFlags flags
        );

        void DeleteMemberByDispID(
            [In] int dispid
        );

        void GetMemberProperties(
            [In] int dispid,
            [In] DispatchPropFlags fetchFlags,
            [Out] out DispatchPropFlags flags
        );

        [PreserveSig]
        int GetMemberName(
            [In] int dispid,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        [PreserveSig]
        int GetNextDispID(
            [In] DispatchEnumFlags flags,
            [In] int dispidCurrent,
            [Out] out int dispidNext
        );

        void GetNameSpaceParent(
            [Out] [MarshalAs(UnmanagedType.IUnknown)] out object parent
        );
    }

    [ComImport]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IServiceProvider
    {
        void QueryService(
            [In] ref Guid guidService,
            [In] ref Guid iid,
            [Out] [MarshalAs(UnmanagedType.IUnknown, IidParameterIndex = 1)] out object service
        );
    }

    #endregion
}
