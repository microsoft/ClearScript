// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.ClearScript.Util.COM;
using TYPEFLAGS = System.Runtime.InteropServices.ComTypes.TYPEFLAGS;
using TYPEKIND = System.Runtime.InteropServices.ComTypes.TYPEKIND;

namespace Microsoft.ClearScript.Util
{
    internal static partial class ObjectHelpers
    {
        private static readonly object[] zeroes =
        {
            (sbyte)0,
            (byte)0,
            (short)0,
            (ushort)0,
            0,
            0U,
            0L,
            0UL,
            IntPtr.Zero,
            UIntPtr.Zero,
            0.0f,
            0.0d,
            0.0m
        };

        public static bool IsZero(this object value) => Array.IndexOf(zeroes, value) >= 0;

        public static bool IsWholeNumber(this object value)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            if (value is null)
            {
                return false;
            }

            if (value.GetType().IsIntegral())
            {
                return true;
            }

            if (value is float floatValue)
            {
                return Math.Truncate(floatValue) == floatValue;
            }

            if (value is double doubleValue)
            {
                return Math.Truncate(doubleValue) == doubleValue;
            }

            if (value is decimal decimalValue)
            {
                return Math.Truncate(decimalValue) == decimalValue;
            }

            return false;

            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static Type GetTypeOrTypeInfo(this object value)
        {
            if (!MiscHelpers.PlatformIsWindows())
            {
                return value.GetType();
            }

            var type = value.GetType();
            IDispatch dispatch = null;

            Type typeInfo = null;
            TYPEKIND typeInfoKind = 0;
            TYPEFLAGS typeInfoFlags = 0;

            if (type.IsUnknownCOMObject())
            {
                // This appears to be a generic COM object with no specific type information.
                // Attempt to acquire COM type information via IDispatch or IProvideClassInfo.

                dispatch = value as IDispatch;
                if (dispatch is not null)
                {
                    var tempTypeInfo = dispatch.GetTypeInfo();
                    if (tempTypeInfo is not null)
                    {
                        typeInfo = GetTypeForTypeInfo(tempTypeInfo);
                        typeInfoKind = tempTypeInfo.GetKind();
                        typeInfoFlags = tempTypeInfo.GetFlags();
                    }
                }

                if (typeInfo is null)
                {
                    if (value is IProvideClassInfo provideClassInfo)
                    {
                        if (HResult.Succeeded(provideClassInfo.GetClassInfo(out var tempTypeInfo)))
                        {
                            typeInfo = GetTypeForTypeInfo(tempTypeInfo);
                            typeInfoKind = tempTypeInfo.GetKind();
                            typeInfoFlags = tempTypeInfo.GetFlags();
                        }
                    }
                }
            }

            if (typeInfo is not null)
            {
                // If the COM type is a dispatch-only interface, use it. Such interfaces typically
                // aren't exposed via QueryInterface(), so there's no way to validate them anyway.

                if ((dispatch is not null) && (typeInfoKind == TYPEKIND.TKIND_DISPATCH) && typeInfoFlags.HasAllFlags(TYPEFLAGS.TYPEFLAG_FDISPATCHABLE) && !typeInfoFlags.HasAllFlags(TYPEFLAGS.TYPEFLAG_FDUAL))
                {
                    return typeInfo;
                }

                // COM type information acquired in this manner may not actually be valid for the
                // original object. In some cases the original object implements a base interface.

                if (typeInfo.IsInstanceOfType(value))
                {
                    return typeInfo;
                }

                foreach (var interfaceType in typeInfo.GetInterfaces())
                {
                    if (interfaceType.IsInstanceOfType(value))
                    {
                        return interfaceType;
                    }
                }
            }

            return type;
        }

        private static Type GetTypeForTypeInfo(ITypeInfo typeInfo)
        {
            // ReSharper disable EmptyGeneralCatchClause

            try
            {
                var typeLib = typeInfo.GetContainingTypeLib(out var index);

                var assembly = LoadPrimaryInteropAssembly(typeLib);
                if (assembly is not null)
                {
                    var name = typeInfo.GetManagedName();
                    var guid = typeInfo.GetGuid();

                    var type = assembly.GetType(name, false, true);
                    if ((type is not null) && (type.GUID == guid))
                    {
                        return type;
                    }

                    var types = assembly.GetAllTypes().ToArray();
                    if ((index >= 0) && (index < types.Length))
                    {
                        type = types[index];
                        if ((type.GUID == guid) && (type.FullName == name))
                        {
                            return type;
                        }
                    }

                    type = types.FirstOrDefault(testType => (testType.GUID == guid) && (testType.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)));
                    if (type is not null)
                    {
                        return type;
                    }
                }

                return typeInfo.GetManagedType();
            }
            catch
            {
            }

            return null;

            // ReSharper restore EmptyGeneralCatchClause
        }

        private static Assembly LoadPrimaryInteropAssembly(ITypeLib typeLib)
        {
            if (typeLib is null)
            {
                return null;
            }

            // ReSharper disable EmptyGeneralCatchClause

            try
            {
                using (var attrScope = typeLib.CreateAttrScope())
                {
                    if (GetPrimaryInteropAssembly(attrScope.Value.guid, attrScope.Value.wMajorVerNum, attrScope.Value.wMinorVerNum, out var name, out var codeBase))
                    {
                        return Assembly.Load(new AssemblyName(name) { CodeBase = codeBase });
                    }
                }
            }
            catch
            {
            }

            return null;

            // ReSharper restore EmptyGeneralCatchClause
        }

        public static string GetFriendlyName(this object value)
        {
            return value.GetFriendlyName(null);
        }

        public static string GetFriendlyName(this object value, Type type)
        {
            if (type is null)
            {
                if (value is null)
                {
                    return "[null]";
                }

                type = value.GetType();
            }

            if (type.IsArray && (value is not null))
            {
                var array = (Array)value;
                var dimensions = Enumerable.Range(0, type.GetArrayRank());
                var lengths = string.Join(",", dimensions.Select(array.GetLength));
                return MiscHelpers.FormatInvariant("{0}[{1}]", type.GetElementType().GetFriendlyName(), lengths);
            }

            if (type.IsUnknownCOMObject())
            {
                if (value is IDispatch dispatch)
                {
                    var typeInfo = dispatch.GetTypeInfo();
                    if (typeInfo is not null)
                    {
                        return typeInfo.GetName();
                    }
                }
            }

            return type.GetFriendlyName();
        }

        public static T DynamicCast<T>(this object value)
        {
            return DynamicCaster<T>.Cast(value);
        }

        public static object ToDynamicResult(this object result, ScriptEngine engine)
        {
            if (result is Nonexistent)
            {
                return Undefined.Value;
            }

            if ((result is HostTarget) || (result is IPropertyBag))
            {
                // Returning an instance of HostTarget (an internal type) isn't likely to be
                // useful. Wrapping it in a dynamic object makes sense in this context. Wrapping
                // a property bag allows it to participate in dynamic invocation chaining, which
                // may be useful when dealing with things like host type collections. HostItem
                // supports dynamic conversion, so the client can unwrap the object if necessary.

                return HostItem.Wrap(engine, result);
            }

            return result;
        }

        #region Nested type: IProvideClassInfo

        [ComImport]
        [Guid("b196b283-bab4-101a-b69c-00aa00341d07")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IProvideClassInfo
        {
            [PreserveSig]
            int GetClassInfo(
                [Out] [MarshalAs(UnmanagedType.Interface)] out ITypeInfo typeInfo
            );
        }

        #endregion

        #region Nested type: DynamicCaster<T>

        private static class DynamicCaster<T>
        {
            public static T Cast(object value)
            {
                // ReSharper disable EmptyGeneralCatchClause

                try
                {
                    if (!typeof(T).IsValueType)
                    {
                        return (T)value;
                    }

                    if (typeof(T).IsEnum)
                    {
                        return (T)Enum.ToObject(typeof(T), value);
                    }

                    if (typeof(T).IsNullable())
                    {
                        return (T)CastToNullable(value);
                    }

                    if (value is IConvertible)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }

                    return (T)value;
                }
                catch
                {
                }

                return (T)(dynamic)value;

                // ReSharper restore EmptyGeneralCatchClause
            }

            private static object CastToNullable(object value)
            {
                if (value is not null)
                {
                    var valueCastType = typeof(DynamicCaster<>).MakeGenericType(Nullable.GetUnderlyingType(typeof(T)));
                    value = valueCastType.InvokeMember("Cast", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new[] { value });
                    return typeof(T).CreateInstance(value);
                }

                return null;
            }
        }

        #endregion
    }
}
