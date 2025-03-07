// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ELEMDESC = System.Runtime.InteropServices.ComTypes.ELEMDESC;
using FUNCDESC = System.Runtime.InteropServices.ComTypes.FUNCDESC;
using TYPEDESC = System.Runtime.InteropServices.ComTypes.TYPEDESC;
using TYPELIBATTR = System.Runtime.InteropServices.ComTypes.TYPELIBATTR;
using VARDESC = System.Runtime.InteropServices.ComTypes.VARDESC;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class TypeLibHelpers
    {
        // GUID_ManagedName (um\cor.h)
        private static readonly Guid managedNameGuid = new("{0f21f359-ab84-41e8-9a78-36d110e6d2f9}");

        public static string GetName(this ITypeLib typeLib)
        {
            return typeLib.GetMemberName(-1);
        }

        public static string GetMemberName(this ITypeLib typeLib, int index)
        {
            typeLib.GetDocumentation(index, out var name, out _, out _, out _);
            return name;
        }

        public static string GetManagedName(this ITypeLib typeLib)
        {
            if (typeLib is ITypeLib2 typeLib2)
            {
                // ReSharper disable EmptyGeneralCatchClause

                try
                {
                    var guid = managedNameGuid;
                    typeLib2.GetCustData(ref guid, out var data);

                    if (data is string name)
                    {
                        name = name.Trim();
                        if (name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            return name.Substring(0, name.Length - 4);
                        }

                        return name;
                    }
                }
                catch
                {
                }

                // ReSharper restore EmptyGeneralCatchClause
            }

            return typeLib.GetName();
        }

        public static Guid GetGuid(this ITypeLib typeLib)
        {
            using (var attrScope = typeLib.CreateAttrScope())
            {
                return attrScope.Value.guid;
            }
        }

        public static IScope<TYPELIBATTR> CreateAttrScope(this ITypeLib typeLib)
        {
            return StructHelpers.CreateScope<TYPELIBATTR>(typeLib.GetLibAttr, typeLib.ReleaseTLibAttr);
        }

        public static IEnumerable<ITypeInfo> GetReferencedEnums(this ITypeLib typeLib)
        {
            var processedTypeInfo = new Dictionary<Guid, ITypeInfo>();

            var count = typeLib.GetTypeInfoCount();
            for (var index = 0; index < count; index++)
            {
                typeLib.GetTypeInfo(index, out var typeInfo);
                foreach (var enumTypeInfo in GetReferencedEnums(typeLib, typeInfo, processedTypeInfo))
                {
                    yield return enumTypeInfo;
                }
            }
        }

        private static IEnumerable<ITypeInfo> GetReferencedEnums(ITypeLib typeLib, ITypeInfo typeInfo, Dictionary<Guid, ITypeInfo> processedTypeInfo)
        {
            if (typeInfo is null)
            {
                yield break;
            }

            var guid = typeInfo.GetOrCreateGuid();

            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd
            if (processedTypeInfo.ContainsKey(guid))
            {
                yield break;
            }

            processedTypeInfo.Add(guid, typeInfo);

            if (typeInfo.IsEnum())
            {
                yield return typeInfo;
                yield break;
            }

            if (typeInfo.GetContainingTypeLib().GetGuid() != typeLib.GetGuid())
            {
                yield break;
            }

            using (var typeAttrScope = typeInfo.CreateAttrScope())
            {
                for (var funcIndex = 0; funcIndex < typeAttrScope.Value.cFuncs; funcIndex++)
                {
                    using (var funcDescScope = typeInfo.CreateFuncDescScope(funcIndex))
                    {
                        foreach (var enumTypeInfo in GetReferencedEnums(typeLib, typeInfo, funcDescScope.Value, processedTypeInfo))
                        {
                            yield return enumTypeInfo;
                        }
                    }
                }

                for (var varIndex = 0; varIndex < typeAttrScope.Value.cVars; varIndex++)
                {
                    using (var varDescScope = typeInfo.CreateVarDescScope(varIndex))
                    {
                        foreach (var enumTypeInfo in GetReferencedEnums(typeLib, typeInfo, varDescScope.Value, processedTypeInfo))
                        {
                            yield return enumTypeInfo;
                        }
                    }
                }

                for (var implTypeIndex = 0; implTypeIndex < typeAttrScope.Value.cImplTypes; implTypeIndex++)
                {
                    typeInfo.GetRefTypeOfImplType(implTypeIndex, out var href);
                    typeInfo.GetRefTypeInfo(href, out var refTypeInfo);

                    var refGuid = refTypeInfo.GetGuid();
                    if ((refGuid == typeof(IDispatch).GUID) || (refGuid == typeof(IDispatchEx).GUID))
                    {
                        continue;
                    }

                    foreach (var enumTypeInfo in GetReferencedEnums(typeLib, refTypeInfo, processedTypeInfo))
                    {
                        yield return enumTypeInfo;
                    }
                }
            }
        }

        private static IEnumerable<ITypeInfo> GetReferencedEnums(ITypeLib typeLib, ITypeInfo typeInfo, FUNCDESC funcDesc, Dictionary<Guid, ITypeInfo> processedTypeInfo)
        {
            foreach (var enumTypeInfo in GetReferencedEnums(typeLib, typeInfo, funcDesc.elemdescFunc, processedTypeInfo))
            {
                yield return enumTypeInfo;
            }

            foreach (var elemDesc in StructHelpers.GetStructsFromArray<ELEMDESC>(funcDesc.lprgelemdescParam, funcDesc.cParams))
            {
                foreach (var enumTypeInfo in GetReferencedEnums(typeLib, typeInfo, elemDesc, processedTypeInfo))
                {
                    yield return enumTypeInfo;
                }
            }
        }

        private static IEnumerable<ITypeInfo> GetReferencedEnums(ITypeLib typeLib, ITypeInfo typeInfo, VARDESC varDesc, Dictionary<Guid, ITypeInfo> processedTypeInfo)
        {
            return GetReferencedEnums(typeLib, typeInfo, varDesc.elemdescVar, processedTypeInfo);
        }

        private static IEnumerable<ITypeInfo> GetReferencedEnums(ITypeLib typeLib, ITypeInfo typeInfo, ELEMDESC elemDesc, Dictionary<Guid, ITypeInfo> processedTypeInfo)
        {
            return GetReferencedEnums(typeLib, typeInfo, elemDesc.tdesc, processedTypeInfo);
        }

        private static IEnumerable<ITypeInfo> GetReferencedEnums(ITypeLib typeLib, ITypeInfo typeInfo, TYPEDESC typeDesc, Dictionary<Guid, ITypeInfo> processedTypeInfo)
        {
            if ((typeDesc.vt == (short)VarEnum.VT_PTR) || (typeDesc.vt == (short)VarEnum.VT_CARRAY))
            {
                return GetReferencedEnums(typeLib, typeInfo, (TYPEDESC)Marshal.PtrToStructure(typeDesc.lpValue, typeof(TYPEDESC)), processedTypeInfo);
            }

            if (typeDesc.vt == (short)VarEnum.VT_USERDEFINED)
            {
                typeInfo.GetRefTypeInfo(unchecked((int)typeDesc.lpValue.ToInt64()), out var refTypeInfo);
                return GetReferencedEnums(typeLib, refTypeInfo, processedTypeInfo);
            }

            return Enumerable.Empty<ITypeInfo>();
        }
    }
}
