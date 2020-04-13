// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.ClearScript.Util.COM;
using Microsoft.Win32;
using TYPEFLAGS = System.Runtime.InteropServices.ComTypes.TYPEFLAGS;
using TYPEKIND = System.Runtime.InteropServices.ComTypes.TYPEKIND;

namespace Microsoft.ClearScript.Util
{
    internal static class ObjectHelpers
    {
        public static Type GetTypeOrTypeInfo(this object value)
        {
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
                if (dispatch != null)
                {
                    var tempTypeInfo = dispatch.GetTypeInfo();
                    if (tempTypeInfo != null)
                    {
                        typeInfo = GetTypeForTypeInfo(tempTypeInfo);
                        typeInfoKind = tempTypeInfo.GetKind();
                        typeInfoFlags = tempTypeInfo.GetFlags();
                    }
                }

                if (typeInfo == null)
                {
                    var provideClassInfo = value as IProvideClassInfo;
                    if (provideClassInfo != null)
                    {
                        ITypeInfo tempTypeInfo;
                        if (HResult.Succeeded(provideClassInfo.GetClassInfo(out tempTypeInfo)))
                        {
                            typeInfo = GetTypeForTypeInfo(tempTypeInfo);
                            typeInfoKind = tempTypeInfo.GetKind();
                            typeInfoFlags = tempTypeInfo.GetFlags();
                        }
                    }
                }
            }

            if (typeInfo != null)
            {
                // If the COM type is a dispatch-only interface, use it. Such interfaces typically
                // aren't exposed via QueryInterface(), so there's no way to validate them anyway.

                if ((dispatch != null) && (typeInfoKind == TYPEKIND.TKIND_DISPATCH) && typeInfoFlags.HasFlag(TYPEFLAGS.TYPEFLAG_FDISPATCHABLE) && !typeInfoFlags.HasFlag(TYPEFLAGS.TYPEFLAG_FDUAL))
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

        public static string GetFriendlyName(this object value)
        {
            return value.GetFriendlyName(null);
        }

        public static string GetFriendlyName(this object value, Type type)
        {
            if (type == null)
            {
                if (value == null)
                {
                    return "[null]";
                }

                type = value.GetType();
            }

            if (type.IsArray && (value != null))
            {
                var array = (Array)value;
                var dimensions = Enumerable.Range(0, type.GetArrayRank());
                var lengths = string.Join(",", dimensions.Select(array.GetLength));
                return MiscHelpers.FormatInvariant("{0}[{1}]", type.GetElementType().GetFriendlyName(), lengths);
            }

            if (type.IsUnknownCOMObject())
            {
                var dispatch = value as IDispatch;
                if (dispatch != null)
                {
                    var typeInfo = dispatch.GetTypeInfo();
                    if (typeInfo != null)
                    {
                        return typeInfo.GetName();
                    }
                }
            }

            return type.GetFriendlyName();
        }

        public static T DynamicCast<T>(this object value)
        {
            // ReSharper disable RedundantCast

            // the cast to dynamic is not redundant; removing it breaks tests
            return (T)(dynamic)value;

            // ReSharper restore RedundantCast
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

        private static Type GetTypeForTypeInfo(ITypeInfo typeInfo)
        {
            // ReSharper disable EmptyGeneralCatchClause

            try
            {
                int index;
                var typeLib = typeInfo.GetContainingTypeLib(out index);

                var assembly = LoadPrimaryInteropAssembly(typeLib);
                if (assembly != null)
                {
                    var name = typeInfo.GetManagedName();
                    var guid = typeInfo.GetGuid();

                    var type = assembly.GetType(name, false, true);
                    if ((type != null) && (type.GUID == guid))
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

                    // ReSharper disable once PossibleNullReferenceException
                    type = types.FirstOrDefault(testType => (testType.GUID == guid) && (testType.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)));
                    if (type != null)
                    {
                        return type;
                    }
                }

                return typeInfo.GetManagedType();
            }
            catch (Exception)
            {
            }

            return null;

            // ReSharper restore EmptyGeneralCatchClause
        }

        private static Assembly LoadPrimaryInteropAssembly(ITypeLib typeLib)
        {
            // ReSharper disable EmptyGeneralCatchClause

            if (typeLib == null)
            {
                return null;
            }

            try
            {
                using (var attrScope = typeLib.CreateAttrScope())
                {
                    string name;
                    string codeBase;
                    if (GetPrimaryInteropAssembly(attrScope.Value.guid, attrScope.Value.wMajorVerNum, attrScope.Value.wMinorVerNum, out name, out codeBase))
                    {
                        return Assembly.Load(new AssemblyName(name) { CodeBase = codeBase });
                    }
                }
            }
            catch (Exception)
            {
            }

            return null;

            // ReSharper restore EmptyGeneralCatchClause
        }

        private static bool GetPrimaryInteropAssembly(Guid libid, int major, int minor, out string name, out string codeBase)
        {
            name = null;
            codeBase = null;

            using (var containerKey = Registry.ClassesRoot.OpenSubKey("TypeLib", false))
            {
                if (containerKey != null)
                {
                    var typeLibName = "{" + libid.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
                    using (var typeLibKey = containerKey.OpenSubKey(typeLibName))
                    {
                        if (typeLibKey != null)
                        {
                            var versionName = major.ToString("x", CultureInfo.InvariantCulture) + "." + minor.ToString("x", CultureInfo.InvariantCulture);
                            using (var versionKey = typeLibKey.OpenSubKey(versionName, false))
                            {
                                if (versionKey != null)
                                {
                                    name = (string)versionKey.GetValue("PrimaryInteropAssemblyName");
                                    codeBase = (string)versionKey.GetValue("PrimaryInteropAssemblyCodeBase");
                                }
                            }
                        }
                    }
                }
            }

            return name != null;
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
    }
}
