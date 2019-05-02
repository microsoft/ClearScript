// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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

        public static object Invoke(this IDispatchEx dispatchEx, bool asConstructor, object[] args)
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

    internal sealed class DynamicDispatchExWrapper : IDynamic
    {
        private readonly IDispatchEx dispatchEx;

        public DynamicDispatchExWrapper(IDispatchEx dispatchEx)
        {
            this.dispatchEx = dispatchEx;
        }

        public object GetProperty(string name, params object[] args)
        {
            bool isCacheable;
            return GetProperty(name, out isCacheable, args);
        }

        public object GetProperty(string name, out bool isCacheable, params object[] args)
        {
            isCacheable = false;
            return dispatchEx.GetProperty(name, false, args);
        }

        public void SetProperty(string name, params object[] args)
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
            return dispatchEx.GetProperty(index.ToString(CultureInfo.InvariantCulture), false, ArrayHelpers.GetEmptyArray<object>());
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

        public object Invoke(bool asConstructor, params object[] args)
        {
            return dispatchEx.Invoke(asConstructor, args);
        }

        public object InvokeMethod(string name, params object[] args)
        {
            return dispatchEx.InvokeMethod(name, false, args);
        }
    }
}
