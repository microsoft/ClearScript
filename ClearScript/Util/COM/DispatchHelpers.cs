// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DISPPARAMS = System.Runtime.InteropServices.ComTypes.DISPPARAMS;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class DispatchHelpers
    {
        private static Guid iid = Guid.Empty;

        public static readonly int VariantSize = sizeof(ushort) * 4 + IntPtr.Size * 2;

        public static ITypeInfo GetTypeInfo(this IDispatch dispatch)
        {
            if (HResult.Succeeded(dispatch.GetTypeInfoCount(out var count)) && (count > 0))
            {
                if (HResult.Succeeded(dispatch.GetTypeInfo(0, 0, out var typeInfo)))
                {
                    return typeInfo;
                }
            }

            return null;
        }

        public static object GetProperty(this IDispatch dispatch, string name, params object[] args)
        {
            if (!MiscHelpers.Try(out int dispid, static ctx => ctx.dispatch.GetDispIDForName(ctx.name), (dispatch, name)))
            {
                return Nonexistent.Value;
            }

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatch.Invoke(dispid, ref iid, 0, DispatchFlags.PropertyGet, ref dispArgs, resultVariantBlock.Addr, out _, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static void SetProperty(this IDispatch dispatch, string name, params object[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("Invalid argument count", nameof(args));
            }

            var dispid = dispatch.GetDispIDForName(name);
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var namedArgDispidBlock = new CoTaskMemBlock(sizeof(int)))
                {
                    Marshal.WriteInt32(namedArgDispidBlock.Addr, SpecialDispIDs.PropertyPut);
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 1, rgdispidNamedArgs = namedArgDispidBlock.Addr };

                    var result = dispatch.Invoke(dispid, ref iid, 0, DispatchFlags.PropertyPut | DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out _, out _);
                    if (result == HResult.DISP_E_MEMBERNOTFOUND)
                    {
                        // VBScript objects can be finicky about property-put dispatch flags

                        result = dispatch.Invoke(dispid, iid, 0, DispatchFlags.PropertyPut, ref dispArgs, IntPtr.Zero, out _, out _);
                        if (result == HResult.DISP_E_MEMBERNOTFOUND)
                        {
                            result = dispatch.Invoke(dispid, iid, 0, DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out _, out _);
                        }
                    }

                    HResult.Check(result);
                }
            }
        }

        public static IEnumerable<string> GetPropertyNames(this IDispatch dispatch)
        {
            return dispatch.GetMembers().Select(member => member.Name);
        }

        public static object Invoke(this IDispatch dispatch, params object[] args)
        {
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatch.Invoke(SpecialDispIDs.Default, ref iid, 0, DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out _, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static object InvokeMethod(this IDispatch dispatch, string name, params object[] args)
        {
            var dispid = dispatch.GetDispIDForName(name);
            if (dispid == SpecialDispIDs.GetEnumerator)
            {
                return dispatch.GetProperty(SpecialMemberNames.NewEnum, args);
            }

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatch.Invoke(dispid, iid, 0, DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out _, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static IEnumerable<DispatchMember> GetMembers(this IDispatch dispatch)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return dispatch.GetMembersRaw().GroupBy(member => member.DispID, DispatchMember.Merge).OfType<DispatchMember>();
        }

        private static int GetDispIDForName(this IDispatch dispatch, string name)
        {
            var dispids = new int[1];
            var names = new[] { name };
            if (HResult.Succeeded(dispatch.GetIDsOfNames(ref iid, names, 1, 0, dispids)))
            {
                return dispids[0];
            }

            if (name.IsDispIDName(out dispids[0]))
            {
                return dispids[0];
            }

            var member = dispatch.GetMembers().FirstOrDefault(testMember => testMember.Name == name);
            if (member is null)
            {
                throw new MissingMemberException(MiscHelpers.FormatInvariant("The object has no property named '{0}'", name));
            }

            return member.DispID;
        }

        private static IEnumerable<DispatchMember> GetMembersRaw(this IDispatch dispatch)
        {
            var typeInfo = dispatch.GetTypeInfo();
            if (typeInfo is null)
            {
                throw new NotSupportedException("The object does not support late binding");
            }

            return typeInfo.GetDispatchMembers();
        }
    }

    internal static class DispatchExHelpers
    {
        public static ITypeInfo GetTypeInfo(this IDispatchEx dispatchEx)
        {
            if (HResult.Succeeded(dispatchEx.GetTypeInfoCount(out var count)) && (count > 0))
            {
                if (HResult.Succeeded(dispatchEx.GetTypeInfo(0, 0, out var typeInfo)))
                {
                    return typeInfo;
                }
            }

            return null;
        }

        public static object GetProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase, params object[] args)
        {
            if (!MiscHelpers.Try(out int dispid, static ctx => ctx.dispatchEx.GetDispIDForName(ctx.name, false, ctx.ignoreCase), (dispatchEx, name, ignoreCase)))
            {
                return Nonexistent.Value;
            }

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyGet, ref dispArgs, resultVariantBlock.Addr, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static void SetProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase, params object[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("Invalid argument count", nameof(args));
            }

            var dispid = dispatchEx.GetDispIDForName(name, true, ignoreCase);
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsBlock(args))
            {
                using (var namedArgDispidBlock = new CoTaskMemBlock(sizeof(int)))
                {
                    Marshal.WriteInt32(namedArgDispidBlock.Addr, SpecialDispIDs.PropertyPut);
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 1, rgdispidNamedArgs = namedArgDispidBlock.Addr };

                    var result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPut | DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out _);
                    if (result == HResult.DISP_E_MEMBERNOTFOUND)
                    {
                        // VBScript objects can be finicky about property-put dispatch flags

                        result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPut, ref dispArgs, IntPtr.Zero, out _);
                        if (result == HResult.DISP_E_MEMBERNOTFOUND)
                        {
                            result = dispatchEx.InvokeEx(dispid, 0, DispatchFlags.PropertyPutRef, ref dispArgs, IntPtr.Zero, out _);
                        }
                    }

                    HResult.Check(result);
                }
            }
        }

        public static bool DeleteProperty(this IDispatchEx dispatchEx, string name, bool ignoreCase)
        {
            return dispatchEx.DeleteMemberByName(name, ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive) == HResult.S_OK;
        }

        public static IEnumerable<string> GetPropertyNames(this IDispatchEx dispatchEx)
        {
            return dispatchEx.GetMembers().Select(member => member.Name);
        }

        public static object Invoke(this IDispatchEx dispatchEx, bool asConstructor, params object[] args)
        {
            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatchEx.InvokeEx(SpecialDispIDs.Default, 0, asConstructor ? DispatchFlags.Construct : DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static object InvokeMethod(this IDispatchEx dispatchEx, string name, bool ignoreCase, params object[] args)
        {
            var dispid = dispatchEx.GetDispIDForName(name, false, ignoreCase);
            if (dispid == SpecialDispIDs.GetEnumerator)
            {
                return dispatchEx.GetProperty(SpecialMemberNames.NewEnum, ignoreCase, args);
            }

            using (var argVariantArrayBlock = new CoTaskMemVariantArgsByRefBlock(args))
            {
                using (var resultVariantBlock = new CoTaskMemVariantBlock())
                {
                    var dispArgs = new DISPPARAMS { cArgs = args.Length, rgvarg = argVariantArrayBlock.Addr, cNamedArgs = 0, rgdispidNamedArgs = IntPtr.Zero };
                    HResult.Check(dispatchEx.InvokeEx(dispid, 0, DispatchFlags.Method, ref dispArgs, resultVariantBlock.Addr, out _));
                    return MiscHelpers.GetObjectForVariant(resultVariantBlock.Addr);
                }
            }
        }

        public static IEnumerable<DispatchMember> GetMembers(this IDispatchEx dispatchEx)
        {
            // ReSharper disable once RedundantEnumerableCastCall
            return dispatchEx.GetMembersRaw().GroupBy(member => member.DispID, DispatchMember.Merge).OfType<DispatchMember>();
        }

        private static int GetDispIDForName(this IDispatchEx dispatchEx, string name, bool ensure, bool ignoreCase)
        {
            var flags = ignoreCase ? DispatchNameFlags.CaseInsensitive : DispatchNameFlags.CaseSensitive;
            if (ensure)
            {
                flags |= DispatchNameFlags.Ensure;
            }

            var result = dispatchEx.GetDispID(name, flags, out var dispid);
            if (ensure && (result == HResult.DISP_E_UNKNOWNNAME))
            {
                throw new NotSupportedException("The object does not support dynamic properties");
            }

            if (HResult.Succeeded(result))
            {
                return dispid;
            }

            if (name.IsDispIDName(out dispid))
            {
                return dispid;
            }

            var member = dispatchEx.GetMembers().FirstOrDefault(testMember => testMember.Name.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
            if (member is not null)
            {
                return member.DispID;
            }

            throw new MissingMemberException(MiscHelpers.FormatInvariant("The object has no property named '{0}'", name));
        }

        private static IEnumerable<DispatchMember> GetMembersRaw(this IDispatchEx dispatchEx)
        {
            var isEnumerable = false;

            var result = dispatchEx.GetNextDispID(DispatchEnumFlags.All, SpecialDispIDs.StartEnum, out var dispid);
            while (result == HResult.S_OK)
            {
                if (HResult.Succeeded(dispatchEx.GetMemberName(dispid, out var name)))
                {
                    if (HResult.Succeeded(dispatchEx.GetMemberProperties(dispid, DispatchPropFlags.CanAll, out var flags)))
                    {
                        if (dispid == SpecialDispIDs.NewEnum)
                        {
                            isEnumerable = true;
                        }

                        yield return new DispatchMember(name, dispid, flags);
                    }
                }

                result = dispatchEx.GetNextDispID(DispatchEnumFlags.All, dispid, out dispid);
            }

            if (isEnumerable)
            {
                yield return new DispatchMember("GetEnumerator", SpecialDispIDs.GetEnumerator, DispatchPropFlags.CanCall);
            }
        }
    }
}
