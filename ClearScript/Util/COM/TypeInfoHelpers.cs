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
        public static ITypeLib GetContainingTypeLib(this ITypeInfo typeInfo)
        {
            int index;
            return typeInfo.GetContainingTypeLib(out index);
        }

        public static ITypeLib GetContainingTypeLib(this ITypeInfo typeInfo, out int index)
        {
            ITypeLib typeLib;
            typeInfo.GetContainingTypeLib(out typeLib, out index);
            return typeLib;
        }

        public static string GetName(this ITypeInfo typeInfo)
        {
            return typeInfo.GetMemberName(-1);
        }

        public static string GetMemberName(this ITypeInfo typeInfo, int memid)
        {
            string name;
            string docString;
            int helpContext;
            string helpFile;
            typeInfo.GetDocumentation(memid, out name, out docString, out helpContext, out helpFile);
            return name;
        }

        public static Guid GetGuid(this ITypeInfo typeInfo)
        {
            return typeInfo.GetTypeAttr().guid;
        }

        public static TYPEFLAGS GetFlags(this ITypeInfo typeInfo)
        {
            return typeInfo.GetTypeAttr().wTypeFlags;
        }

        public static TYPEKIND GetKind(this ITypeInfo typeInfo)
        {
            return typeInfo.GetTypeAttr().typekind;
        }

        public static IEnumerable<DispatchMember> GetDispatchMembers(this ITypeInfo typeInfo)
        {
            var funcCount = typeInfo.GetTypeAttr().cFuncs;
            var isEnumerable = false;

            var names = new string[1];
            for (var index = 0; index < funcCount; index++)
            {
                IntPtr pDesc;
                typeInfo.GetFuncDesc(index, out pDesc);
                try
                {
                    var desc = (FUNCDESC)Marshal.PtrToStructure(pDesc, typeof(FUNCDESC));
                    if (desc.memid == SpecialDispIDs.NewEnum)
                    {
                        isEnumerable = true;
                    }

                    if ((desc.wFuncFlags & (short)FUNCFLAGS.FUNCFLAG_FRESTRICTED) != 0)
                    {
                        continue;
                    }

                    int nameCount;
                    typeInfo.GetNames(desc.memid, names, 1, out nameCount);
                    if (nameCount > 0)
                    {
                        yield return new DispatchMember(names[0], desc.memid, desc.invkind);
                    }
                }
                finally
                {
                    typeInfo.ReleaseFuncDesc(pDesc);
                }

                if (isEnumerable)
                {
                    yield return new DispatchMember("GetEnumerator", SpecialDispIDs.GetEnumerator, INVOKEKIND.INVOKE_FUNC);
                }
            }
        }

        public static IPropertyBag GetTypeLibEnums(this ITypeInfo typeInfo)
        {
            var typeLib = typeInfo.GetContainingTypeLib();
            var typeLibName = typeLib.GetName();

            var rootNode = new PropertyBag(true);

            var typeInfoCount = typeLib.GetTypeInfoCount();
            for (var typeInfoIndex = 0; typeInfoIndex < typeInfoCount; typeInfoIndex++)
            {
                typeLib.GetTypeInfo(typeInfoIndex, out typeInfo);
                var typeInfoName = typeInfo.GetName();

                var typeAttr = typeInfo.GetTypeAttr();
                if (typeAttr.typekind == TYPEKIND.TKIND_ALIAS)
                {
                    ITypeInfo refTypeInfo;
                    typeInfo.GetRefTypeInfo(unchecked((int)(long)typeAttr.tdescAlias.lpValue), out refTypeInfo);

                    typeInfo = refTypeInfo;
                    typeAttr = typeInfo.GetTypeAttr();
                }

                if (typeAttr.typekind == TYPEKIND.TKIND_ENUM)
                {
                    var varCount = typeAttr.cVars;
                    for (var varIndex = 0; varIndex < varCount; varIndex++)
                    {
                        IntPtr pVarDesc;
                        typeInfo.GetVarDesc(varIndex, out pVarDesc);
                        try
                        {
                            var varDesc = (VARDESC)Marshal.PtrToStructure(pVarDesc, typeof(VARDESC));
                            if (varDesc.varkind == VARKIND.VAR_CONST)
                            {
                                var varName = typeInfo.GetMemberName(varDesc.memid);

                                object typeLibNodeObj;
                                if (!rootNode.TryGetValue(typeLibName, out typeLibNodeObj) || !(typeLibNodeObj is PropertyBag))
                                {
                                    typeLibNodeObj = new PropertyBag(true);
                                    rootNode.SetPropertyNoCheck(typeLibName, typeLibNodeObj);
                                }

                                object typeInfoNodeObj;
                                var typeLibNode = (PropertyBag)typeLibNodeObj;
                                if (!typeLibNode.TryGetValue(typeInfoName, out typeInfoNodeObj) || !(typeInfoNodeObj is PropertyBag))
                                {
                                    typeInfoNodeObj = new PropertyBag(true);
                                    typeLibNode.SetPropertyNoCheck(typeInfoName, typeInfoNodeObj);
                                }

                                var typeInfoNode = (PropertyBag)typeInfoNodeObj;
                                typeInfoNode.SetPropertyNoCheck(varName, Marshal.GetObjectForNativeVariant(varDesc.desc.lpvarValue));
                            }
                        }
                        finally
                        {
                            typeInfo.ReleaseVarDesc(pVarDesc);
                        }
                    }
                }
            }

            return rootNode;
        }

        private static TYPEATTR GetTypeAttr(this ITypeInfo typeInfo)
        {
            IntPtr pTypeAttr;
            typeInfo.GetTypeAttr(out pTypeAttr);
            try
            {
                return (TYPEATTR)Marshal.PtrToStructure(pTypeAttr, typeof(TYPEATTR));
            }
            finally
            {
                typeInfo.ReleaseTypeAttr(pTypeAttr);
            }
        }
    }
}
