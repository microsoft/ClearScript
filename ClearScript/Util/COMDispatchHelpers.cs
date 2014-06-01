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
using DISPPARAMS = System.Runtime.InteropServices.ComTypes.DISPPARAMS;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace Microsoft.ClearScript.Util
{
    internal static class COMDispatchHelpers
    {
        public static object GetProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase, object[] args)
        {
            int dispid;
            Marshal.ThrowExceptionForHR(dispatchEx.GetDispID(name, ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive, out dispid));

            using (var argVariantArrayBlock = new CoTaskMemVariantArrayBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    EXCEPINFO excepInfo;
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyGet, ref dispArgs, resultVariantBlock.Addr, out excepInfo);
                    return Marshal.GetObjectForNativeVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static void SetProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase, object[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("Invalid argument count", "args");
            }

            int dispid;
            var result = dispatchEx.GetDispID(name, DispatchNameFlags.Ensure | (ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive), out dispid);
            if (result == RawCOMHelpers.HResult.DISP_E_UNKNOWNNAME)
            {
                throw new NotSupportedException("Object does not support dynamic properties");
            }

            Marshal.ThrowExceptionForHR(result);
            using (var argVariantArrayBlock = new CoTaskMemVariantArrayBlock(args))
            {
                using (var namedArgDispidBlock = new CoTaskMemBlock(sizeof(int)))
                {
                    EXCEPINFO excepInfo;
                    Marshal.WriteInt32(namedArgDispidBlock.Addr, SpecialDispIDs.PropertyPut);
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 1, rgdispidNamedArgs = namedArgDispidBlock.Addr };
                    dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPut | DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out excepInfo);
                }
            }
        }

        public static object InvokeMethod(this IDispatchEx dispatchEx, string name, bool ignoreCase, object[] args)
        {
            int dispid;
            Marshal.ThrowExceptionForHR(dispatchEx.GetDispID(name, ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive, out dispid));

            using (var argVariantArrayBlock = new CoTaskMemVariantArrayBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    EXCEPINFO excepInfo;
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    dispatchEx.InvokeEx(dispid, 0, DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out excepInfo);
                    return Marshal.GetObjectForNativeVariant(resultVariantBlock.Addr);
                }
            }
        }
    }
}
