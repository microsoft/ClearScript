// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class MemberHelpers
    {
        public static bool IsScriptable(this EventInfo eventInfo, Type accessContext, ScriptAccess defaultAccess)
        {
            return !eventInfo.IsSpecialName && !eventInfo.IsExplicitImplementation() && eventInfo.IsAccessible(accessContext) && !eventInfo.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this FieldInfo field, Type accessContext, ScriptAccess defaultAccess)
        {
            return !field.IsSpecialName && field.IsAccessible(accessContext) && !field.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this MethodInfo method, Type accessContext, ScriptAccess defaultAccess)
        {
            return !method.IsSpecialName && !method.IsExplicitImplementation() && method.IsAccessible(accessContext) && !method.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this PropertyInfo property, Type accessContext, ScriptAccess defaultAccess)
        {
            return !property.IsSpecialName && !property.IsExplicitImplementation() && property.IsAccessible(accessContext) && !property.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this Type type, Type accessContext, ScriptAccess defaultAccess)
        {
            return !type.IsSpecialName && type.IsAccessible(accessContext) && !type.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsAccessible(this EventInfo eventInfo, Type accessContext)
        {
            return eventInfo.AddMethod.IsAccessible(accessContext);
        }

        public static bool IsAccessible(this FieldInfo field, Type accessContext)
        {
            var type = field.DeclaringType;

            if (!type.IsAccessible(accessContext))
            {
                return false;
            }

            var access = field.Attributes & FieldAttributes.FieldAccessMask;

            if (access == FieldAttributes.Public)
            {
                return true;
            }

            if (accessContext == null)
            {
                return false;
            }

            if (access == FieldAttributes.Private)
            {
                return type.EqualsOrDeclares(accessContext);
            }

            if (access == FieldAttributes.Family)
            {
                return accessContext.IsFamilyOf(type);
            }

            if (access == FieldAttributes.Assembly)
            {
                return accessContext.IsFriendOf(type);
            }

            if (access == FieldAttributes.FamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(type);
            }

            if (access == FieldAttributes.FamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(type);
            }

            return false;
        }

        public static bool IsAccessible(this MethodBase method, Type accessContext)
        {
            var type = method.DeclaringType;

            if (!type.IsAccessible(accessContext))
            {
                return false;
            }

            var access = method.Attributes & MethodAttributes.MemberAccessMask;

            if (access == MethodAttributes.Public)
            {
                return true;
            }

            if (accessContext == null)
            {
                return false;
            }

            if (access == MethodAttributes.Private)
            {
                return type.EqualsOrDeclares(accessContext);
            }

            if (access == MethodAttributes.Family)
            {
                return accessContext.IsFamilyOf(type);
            }

            if (access == MethodAttributes.Assembly)
            {
                return accessContext.IsFriendOf(type);
            }

            if (access == MethodAttributes.FamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(type);
            }

            if (access == MethodAttributes.FamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(type);
            }

            return false;
        }

        public static bool IsAccessible(this MethodInfo method, Type accessContext)
        {
            return ((MethodBase)method.GetBaseDefinition()).IsAccessible(accessContext);
        }

        public static bool IsAccessible(this PropertyInfo property, Type accessContext)
        {
            var getMethod = property.GetMethod;
            if ((getMethod != null) && getMethod.IsAccessible(accessContext))
            {
                return true;
            }

            var setMethod = property.SetMethod;
            if ((setMethod != null) && setMethod.IsAccessible(accessContext))
            {
                return true;
            }

            return false;
        }

        public static bool IsAccessible(this Type type, Type accessContext)
        {
            var visibility = type.Attributes & TypeAttributes.VisibilityMask;

            if (visibility == TypeAttributes.Public)
            {
                return true;
            }

            if (accessContext == null)
            {
                return (visibility == TypeAttributes.NestedPublic) && type.DeclaringType.IsAccessible(null);
            }

            if (visibility == TypeAttributes.NotPublic)
            {
                return accessContext.IsFriendOf(type);
            }

            type = type.DeclaringType;

            if (!type.IsAccessible(accessContext))
            {
                return false;
            }

            if (visibility == TypeAttributes.NestedPublic)
            {
                return true;
            }

            if (visibility == TypeAttributes.NestedPrivate)
            {
                return type.EqualsOrDeclares(accessContext);
            }

            if (visibility == TypeAttributes.NestedFamily)
            {
                return accessContext.IsFamilyOf(type);
            }

            if (visibility == TypeAttributes.NestedAssembly)
            {
                return accessContext.IsFriendOf(type);
            }

            if (visibility == TypeAttributes.NestedFamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(type);
            }

            if (visibility == TypeAttributes.NestedFamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(type);
            }

            return false;
        }

        public static string GetScriptName(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptMemberAttribute>(true);
            return ((attribute != null) && (attribute.Name != null)) ? attribute.Name : member.GetShortName();
        }

        public static bool IsBlockedFromScript(this MemberInfo member, ScriptAccess defaultAccess, bool chain = true)
        {
            return member.GetScriptAccess(defaultAccess, chain) == ScriptAccess.None;
        }

        public static bool IsReadOnlyForScript(this MemberInfo member, ScriptAccess defaultAccess)
        {
            return member.GetScriptAccess(defaultAccess) == ScriptAccess.ReadOnly;
        }

        public static ScriptAccess GetScriptAccess(this MemberInfo member, ScriptAccess defaultValue, bool chain = true)
        {
            var attribute = member.GetAttribute<ScriptUsageAttribute>(true);
            if (attribute != null)
            {
                return attribute.Access;
            }

            if (chain)
            {
                var declaringType = member.DeclaringType;
                if (declaringType != null)
                {
                    var testType = declaringType;
                    do
                    {
                        if (testType.IsNested)
                        {
                            var nestedTypeAttribute = testType.GetAttribute<ScriptUsageAttribute>(true);
                            if (nestedTypeAttribute != null)
                            {
                                return nestedTypeAttribute.Access;
                            }
                        }

                        var typeAttribute = testType.GetAttribute<DefaultScriptUsageAttribute>(true);
                        if (typeAttribute != null)
                        {
                            return typeAttribute.Access;
                        }

                        testType = testType.DeclaringType;

                    } while (testType != null);

                    var assemblyAttribute = declaringType.Assembly.GetAttribute<DefaultScriptUsageAttribute>(true);
                    if (assemblyAttribute != null)
                    {
                        return assemblyAttribute.Access;
                    }
                }
            }

            return defaultValue;
        }

        public static bool IsRestrictedForScript(this MemberInfo member)
        {
            return !member.GetScriptMemberFlags().HasFlag(ScriptMemberFlags.ExposeRuntimeType);
        }

        public static bool IsDispID(this MemberInfo member, int dispid)
        {
            var attribute = member.GetAttribute<DispIdAttribute>(true);
            return (attribute != null) && (attribute.Value == dispid);
        }

        public static ScriptMemberFlags GetScriptMemberFlags(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptMemberAttribute>(true);
            return (attribute != null) ? attribute.Flags : ScriptMemberFlags.None;
        }

        public static string GetShortName(this MemberInfo member)
        {
            var name = member.Name;
            var index = name.LastIndexOf('.');
            return (index >= 0) ? name.Substring(index + 1) : name;
        }

        private static bool IsExplicitImplementation(this MemberInfo member)
        {
            return member.Name.IndexOf('.') >= 0;
        }

        private static T GetAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            try
            {
                return Attribute.GetCustomAttributes(member, typeof(T), inherit).SingleOrDefault() as T;
            }
            catch (AmbiguousMatchException)
            {
                if (inherit)
                {
                    // this affects SqlDataReader and is indicative of a .NET issue described here:
                    // http://connect.microsoft.com/VisualStudio/feedback/details/646399/attribute-isdefined-throws-ambiguousmatchexception-for-indexer-properties-and-inherited-attributes

                    return Attribute.GetCustomAttributes(member, typeof(T), false).SingleOrDefault() as T;
                }

                throw;
            }
        }
    }
}
