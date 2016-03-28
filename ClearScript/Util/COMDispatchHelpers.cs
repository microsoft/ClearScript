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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    EXCEPINFO excepInfo;
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    Marshal.ThrowExceptionForHR(dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyGet, ref dispArgs, resultVariantBlock.Addr, out excepInfo));
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
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var namedArgDispidBlock = new CoTaskMemBlock(sizeof(int)))
                {
                    EXCEPINFO excepInfo;
                    Marshal.WriteInt32(namedArgDispidBlock.Addr, SpecialDispIDs.PropertyPut);
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 1, rgdispidNamedArgs = namedArgDispidBlock.Addr };

                    result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPut | DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out excepInfo);
                    if (result == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND)
                    {
                        // VBScript objects can be finicky about property-put dispatch flags

                        result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPut, ref dispArgs, IntPtr.Zero, out excepInfo);
                        if (result == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND)
                        {
                            result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out excepInfo);
                        }
                    }

                    Marshal.ThrowExceptionForHR(result);
                }
            }
        }

        public static bool DeleteProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase)
        {
            return dispatchEx.DeleteMemberByName(name, ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive) == RawCOMHelpers.HResult.S_OK;
        }

        public static IEnumerable<string> GetPropertyNames(this IDispatchEx dispatchEx)
        {
            int dispid;
            var result = dispatchEx.GetNextDispID(DispatchEnumFlags.All, SpecialDispIDs.StartEnum, out dispid);
            while (result == RawCOMHelpers.HResult.S_OK)
            {
                string name;
                if (dispatchEx.GetMemberName(dispid, out name) == RawCOMHelpers.HResult.S_OK)
                {
                    yield return name;
                }

                result = dispatchEx.GetNextDispID(DispatchEnumFlags.All, dispid, out dispid);
            }
        }

        public static object Invoke(this IDispatchEx dispatchEx, object[] args, bool asConstructor)
        {
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    EXCEPINFO excepInfo;
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    Marshal.ThrowExceptionForHR(dispatchEx.InvokeEx(SpecialDispIDs.Default, 0, asConstructor ? DispatchFlags.Construct : DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out excepInfo));
                    return Marshal.GetObjectForNativeVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static object InvokeMethod(this IDispatchEx dispatchEx, string name, bool ignoreCase, object[] args)
        {
            int dispid;
            Marshal.ThrowExceptionForHR(dispatchEx.GetDispID(name, ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive, out dispid));

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    EXCEPINFO excepInfo;
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    Marshal.ThrowExceptionForHR(dispatchEx.InvokeEx(dispid, 0, DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out excepInfo));
                    return Marshal.GetObjectForNativeVariant(resultVariantBlock.Addr);
                }
            }
        }
    }

    internal class DynamicDispatchExWrapper : IDynamic
    {
        private readonly IDispatchEx dispatchEx;

        public DynamicDispatchExWrapper(IDispatchEx dispatchEx)
        {
            this.dispatchEx = dispatchEx;
        }

        public object GetProperty(string name, object[] args)
        {
            bool isCacheable;
            return GetProperty(name, args, out isCacheable);
        }

        public object GetProperty(string name, object[] args, out bool isCacheable)
        {
            isCacheable = false;
            return dispatchEx.GetProperty(name, false, args);
        }

        public void SetProperty(string name, object[] args)
        {
            dispatchEx.SetProperty(name, false, args);
        }

        public bool DeleteProperty(string name)
        {
            return dispatchEx.DeleteProperty(name, false);
        }

        public string[] GetPropertyNames()
        {
            return dispatchEx.GetPropertyNames().ExcludeIndices().ToArray();
        }

        public object GetProperty(int index)
        {
            return dispatchEx.GetProperty(index.ToString(CultureInfo.InvariantCulture), false, MiscHelpers.GetEmptyArray<object>());
        }

        public void SetProperty(int index, object value)
        {
            dispatchEx.SetProperty(index.ToString(CultureInfo.InvariantCulture), false, new[] { value });
        }

        public bool DeleteProperty(int index)
        {
            return dispatchEx.DeleteProperty(index.ToString(CultureInfo.InvariantCulture), false);
        }

        public int[] GetPropertyIndices()
        {
            return dispatchEx.GetPropertyNames().GetIndices().ToArray();
        }

        public object Invoke(object[] args, bool asConstructor)
        {
            return dispatchEx.Invoke(args, asConstructor);
        }

        public object InvokeMethod(string name, object[] args)
        {
            return dispatchEx.InvokeMethod(name, false, args);
        }
    }
}
