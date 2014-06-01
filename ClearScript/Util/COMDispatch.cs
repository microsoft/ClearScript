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
using System.Runtime.InteropServices.CustomMarshalers;
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
        void GetTypeInfoCount(
            [Out] out uint count
        );

        void GetTypeInfo(
            [In] uint index,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TypeToTypeInfoMarshaler))] out Type typeInfo
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

        void GetTypeInfoCount(
            [Out] out uint count
        );

        void GetTypeInfo(
            [In] uint index,
            [In] int lcid,
            [Out] [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(TypeToTypeInfoMarshaler))] out Type typeInfo
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

        void InvokeEx(
            [In] int dispid,
            [In] int lcid,
            [In] DispatchFlags flags,
            [In] ref DISPPARAMS args,
            [In] IntPtr pVarResult,
            [Out] out EXCEPINFO excepInfo,
            [In] [Optional] [MarshalAs(UnmanagedType.Interface)] IServiceProvider svpCaller
        );

        void DeleteMemberByName(
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

        void GetMemberName(
            [In] int dispid,
            [Out] [MarshalAs(UnmanagedType.BStr)] out string name
        );

        void GetNextDispID(
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
