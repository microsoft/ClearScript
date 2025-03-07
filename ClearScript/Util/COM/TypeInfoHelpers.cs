// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FUNCDESC = System.Runtime.InteropServices.ComTypes.FUNCDESC;
using FUNCFLAGS = System.Runtime.InteropServices.ComTypes.FUNCFLAGS;
using INVOKEKIND = System.Runtime.InteropServices.ComTypes.INVOKEKIND;
using TYPEATTR = System.Runtime.InteropServices.ComTypes.TYPEATTR;
using TYPEFLAGS = System.Runtime.InteropServices.ComTypes.TYPEFLAGS;
using TYPEKIND = System.Runtime.InteropServices.ComTypes.TYPEKIND;
using VARDESC = System.Runtime.InteropServices.ComTypes.VARDESC;

namespace Microsoft.ClearScript.Util.COM
{
    internal static partial class TypeInfoHelpers
    {
        // GUID_ManagedName (um\cor.h)
        private static readonly Guid managedNameGuid = new("{0f21f359-ab84-41e8-9a78-36d110e6d2f9}");

        public static ITypeLib GetContainingTypeLib(this ITypeInfo typeInfo)
        {
            return typeInfo.GetContainingTypeLib(out _);
        }

        public static ITypeLib GetContainingTypeLib(this ITypeInfo typeInfo, out int index)
        {
            typeInfo.GetContainingTypeLib(out var typeLib, out index);
            return typeLib;
        }

        public static string GetName(this ITypeInfo typeInfo)
        {
            return typeInfo.GetMemberName(-1);
        }

        public static string GetManagedName(this ITypeInfo typeInfo)
        {
            if (typeInfo is ITypeInfo2 typeInfo2)
            {
                // ReSharper disable EmptyGeneralCatchClause

                try
                {
                    var guid = managedNameGuid;
                    typeInfo2.GetCustData(ref guid, out var data);

                    if (data is string name)
                    {
                        return name.Trim();
                    }
                }
                catch
                {
                }

                // ReSharper restore EmptyGeneralCatchClause
            }

            return typeInfo.GetContainingTypeLib().GetManagedName() + "." + Marshal.GetTypeInfoName(typeInfo);
        }

        public static string GetMemberName(this ITypeInfo typeInfo, int memid)
        {
            typeInfo.GetDocumentation(memid, out var name, out _, out _, out _);
            return name;
        }

        public static Guid GetGuid(this ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                return attrScope.Value.guid;
            }
        }

        public static Guid GetOrCreateGuid(this ITypeInfo typeInfo)
        {
            var guid = typeInfo.GetGuid();
            if (guid != Guid.Empty)
            {
                return guid;
            }

            var guidBytes = typeInfo.GetContainingTypeLib().GetGuid().ToByteArray();

            var nameBytes = BitConverter.GetBytes(typeInfo.GetName().GetDigestAsUInt64());
            for (var index = 0; index < guidBytes.Length; index++)
            {
                guidBytes[index] ^= nameBytes[index % nameBytes.Length];
            }

            return new Guid(guidBytes);
        }

        public static TYPEFLAGS GetFlags(this ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                return attrScope.Value.wTypeFlags;
            }
        }

        public static TYPEKIND GetKind(this ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                return attrScope.Value.typekind;
            }
        }

        public static IEnumerable<DispatchMember> GetDispatchMembers(this ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                var count = attrScope.Value.cFuncs;
                var isEnumerable = false;

                var names = new string[1];
                for (var index = 0; index < count; index++)
                {
                    using (var funcDescScope = typeInfo.CreateFuncDescScope(index))
                    {
                        if (funcDescScope.Value.memid == SpecialDispIDs.NewEnum)
                        {
                            isEnumerable = true;
                        }

                        if ((funcDescScope.Value.wFuncFlags & (short)FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0)
                        {
                            continue;
                        }

                        typeInfo.GetNames(funcDescScope.Value.memid, names, 1, out var nameCount);
                        if (nameCount > 0)
                        {
                            yield return new DispatchMember(names[0], funcDescScope.Value.memid, funcDescScope.Value.invkind);
                        }

                        if (isEnumerable)
                        {
                            yield return new DispatchMember("GetEnumerator", SpecialDispIDs.GetEnumerator, INVOKEKIND.INVOKE_FUNC);
                        }
                    }
                }
            }
        }

        public static bool IsEnum(this ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                if (attrScope.Value.typekind == TYPEKIND.TKIND_ENUM)
                {
                    return true;
                }

                if (attrScope.Value.typekind == TYPEKIND.TKIND_ALIAS)
                {
                    typeInfo.GetRefTypeInfo(unchecked((int)attrScope.Value.tdescAlias.lpValue.ToInt64()), out var refTypeInfo);
                    return refTypeInfo.IsEnum();
                }

                return false;
            }
        }

        public static IScope<TYPEATTR> CreateAttrScope(this ITypeInfo typeInfo)
        {
            return StructHelpers.CreateScope<TYPEATTR>(typeInfo.GetTypeAttr, typeInfo.ReleaseTypeAttr);
        }

        public static IScope<VARDESC> CreateVarDescScope(this ITypeInfo typeInfo, int index)
        {
            return StructHelpers.CreateScope<VARDESC>((out IntPtr pVarDesc) => typeInfo.GetVarDesc(index, out pVarDesc), typeInfo.ReleaseVarDesc);
        }

        public static IScope<FUNCDESC> CreateFuncDescScope(this ITypeInfo typeInfo, int index)
        {
            return StructHelpers.CreateScope<FUNCDESC>((out IntPtr pFuncDesc) => typeInfo.GetFuncDesc(index, out pFuncDesc), typeInfo.ReleaseFuncDesc);
        }
    }
}
