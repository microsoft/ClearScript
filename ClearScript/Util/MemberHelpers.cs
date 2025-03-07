// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class MemberHelpers
    {
        public static bool IsScriptable(this EventInfo eventInfo, IHostContext context)
        {
            return !eventInfo.IsSpecialName && !eventInfo.IsExplicitImplementation() && eventInfo.IsAccessible(context) && !eventInfo.IsBlockedFromScript(context, context.DefaultAccess);
        }

        public static bool IsScriptable(this FieldInfo field, IHostContext context)
        {
            return !field.IsSpecialName && field.IsAccessible(context) && !field.IsBlockedFromScript(context, context.DefaultAccess);
        }

        public static bool IsScriptable(this MethodInfo method, IHostContext context)
        {
            return !method.IsSpecialName && !method.IsExplicitImplementation() && method.IsAccessible(context) && !method.IsBlockedFromScript(context, context.DefaultAccess);
        }

        public static bool IsScriptable(this PropertyInfo property, IHostContext context)
        {
            return !property.IsSpecialName && !property.IsExplicitImplementation() && property.IsAccessible(context) && !property.IsBlockedFromScript(context, context.DefaultAccess);
        }

        public static bool IsScriptable(this Type type, IHostContext context)
        {
            return !type.IsSpecialName && type.IsAccessible(context) && !type.IsBlockedFromScript(context, context.DefaultAccess);
        }

        public static bool IsAccessible(this EventInfo eventInfo, IHostContext context)
        {
            return eventInfo.AddMethod.IsAccessible(context);
        }

        public static bool IsAccessible(this FieldInfo field, IHostContext context)
        {
            var type = field.DeclaringType;

            if (!type.IsAccessible(context))
            {
                return false;
            }

            var access = field.Attributes & FieldAttributes.FieldAccessMask;

            if (access == FieldAttributes.Public)
            {
                return true;
            }

            var accessContext = context.AccessContext;

            if (accessContext is null)
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
                return accessContext.IsFriendOf(context, type);
            }

            if (access == FieldAttributes.FamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(context, type);
            }

            if (access == FieldAttributes.FamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(context, type);
            }

            return false;
        }

        public static bool IsAccessible(this MethodBase method, IHostContext context)
        {
            var type = method.DeclaringType;

            if (!type.IsAccessible(context))
            {
                return false;
            }

            var access = method.Attributes & MethodAttributes.MemberAccessMask;

            if (access == MethodAttributes.Public)
            {
                return true;
            }

            var accessContext = context.AccessContext;

            if (accessContext is null)
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
                return accessContext.IsFriendOf(context, type);
            }

            if (access == MethodAttributes.FamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(context, type);
            }

            if (access == MethodAttributes.FamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(context, type);
            }

            return false;
        }

        public static bool IsAccessible(this MethodInfo method, IHostContext context)
        {
            return ((MethodBase)method.GetBaseDefinition()).IsAccessible(context);
        }

        public static bool IsAccessible(this PropertyInfo property, IHostContext context)
        {
            var getMethod = property.GetMethod;
            if ((getMethod is not null) && getMethod.IsAccessible(context))
            {
                return true;
            }

            var setMethod = property.SetMethod;
            if ((setMethod is not null) && setMethod.IsAccessible(context))
            {
                return true;
            }

            return false;
        }

        public static bool IsAccessible(this Type type, IHostContext context)
        {
            var visibility = (type.IsAnonymous(context) && !context.Engine.EnforceAnonymousTypeAccess) ? TypeAttributes.Public : type.Attributes & TypeAttributes.VisibilityMask;

            if (visibility == TypeAttributes.Public)
            {
                return true;
            }

            var accessContext = context.AccessContext;

            if (accessContext is null)
            {
                return (visibility == TypeAttributes.NestedPublic) && type.DeclaringType.IsAccessible(context);
            }

            if (visibility == TypeAttributes.NotPublic)
            {
                return accessContext.IsFriendOf(context, type);
            }

            type = type.DeclaringType;

            if (!type.IsAccessible(context))
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
                return accessContext.IsFriendOf(context, type);
            }

            if (visibility == TypeAttributes.NestedFamORAssem)
            {
                return accessContext.IsFamilyOf(type) || accessContext.IsFriendOf(context, type);
            }

            if (visibility == TypeAttributes.NestedFamANDAssem)
            {
                return accessContext.IsFamilyOf(type) && accessContext.IsFriendOf(context, type);
            }

            return false;
        }

        public static string GetScriptName(this MemberInfo member, IHostContext context)
        {
            var attribute = member.GetOrLoadCustomAttribute<ScriptMemberAttribute>(context);
            return attribute?.Name ?? member.GetShortName();
        }

        public static bool IsBlockedFromScript(this MemberInfo member, IHostContext context, ScriptAccess defaultAccess, bool chain = true)
        {
            return member.GetScriptAccess(context, defaultAccess, chain) == ScriptAccess.None;
        }

        public static bool IsReadOnlyForScript(this MemberInfo member, IHostContext context, ScriptAccess defaultAccess)
        {
            return member.GetScriptAccess(context, defaultAccess) == ScriptAccess.ReadOnly;
        }

        public static ScriptAccess GetScriptAccess(this MemberInfo member, IHostContext context, ScriptAccess defaultValue, bool chain = true)
        {
            var attribute = member.GetOrLoadCustomAttribute<ScriptUsageAttribute>(context);
            if (attribute is not null)
            {
                return attribute.Access;
            }

            if (chain)
            {
                var declaringType = member.DeclaringType;
                if (declaringType is not null)
                {
                    var testType = declaringType;
                    do
                    {
                        if (testType.IsNested)
                        {
                            var nestedTypeAttribute = testType.GetOrLoadCustomAttribute<ScriptUsageAttribute>(context);
                            if (nestedTypeAttribute is not null)
                            {
                                return nestedTypeAttribute.Access;
                            }
                        }

                        var typeAttribute = testType.GetOrLoadCustomAttribute<DefaultScriptUsageAttribute>(context);
                        if (typeAttribute is not null)
                        {
                            return typeAttribute.Access;
                        }

                        testType = testType.DeclaringType;

                    } while (testType is not null);

                    var assemblyAttribute = declaringType.Assembly.GetOrLoadCustomAttribute<DefaultScriptUsageAttribute>(context);
                    if (assemblyAttribute is not null)
                    {
                        return assemblyAttribute.Access;
                    }
                }
            }

            return defaultValue;
        }

        public static bool IsRestrictedForScript(this MemberInfo member, IHostContext context)
        {
            return !member.GetScriptMemberFlags(context).HasAllFlags(ScriptMemberFlags.ExposeRuntimeType);
        }

        public static bool IsDispID(this MemberInfo member, IHostContext context, int dispid)
        {
            var attribute = member.GetOrLoadCustomAttribute<DispIdAttribute>(context);
            return (attribute is not null) && (attribute.Value == dispid);
        }

        public static ScriptMemberFlags GetScriptMemberFlags(this MemberInfo member, IHostContext context)
        {
            var attribute = member.GetOrLoadCustomAttribute<ScriptMemberAttribute>(context);
            return attribute?.Flags ?? ScriptMemberFlags.None;
        }

        public static string GetShortName(this MemberInfo member)
        {
            var name = member.Name;
            var index = name.LastIndexOf('.');
            return (index >= 0) ? name.Substring(index + 1) : name;
        }

        public static T GetOrLoadCustomAttribute<T>(this MemberInfo member, IHostContext context, bool inherit = true) where T : Attribute
        {
            try
            {
                return CustomAttributes.GetOrLoad<T>(context, member, inherit).SingleOrDefault();
            }
            catch (AmbiguousMatchException)
            {
                if (inherit)
                {
                    // this affects SqlDataReader and is indicative of a .NET issue described here:
                    // http://connect.microsoft.com/VisualStudio/feedback/details/646399/attribute-isdefined-throws-ambiguousmatchexception-for-indexer-properties-and-inherited-attributes

                    return CustomAttributes.GetOrLoad<T>(context, member, false).SingleOrDefault();
                }

                throw;
            }
        }

        public static IEnumerable<T> GetOrLoadCustomAttributes<T>(this MemberInfo member, IHostContext context, bool inherit = true) where T : Attribute
        {
            return CustomAttributes.GetOrLoad<T>(context, member, inherit);
        }

        public static bool HasCustomAttributes<T>(this MemberInfo member, IHostContext context, bool inherit = true) where T : Attribute
        {
            return CustomAttributes.Has<T>(context, member, inherit);
        }

        private static bool IsExplicitImplementation(this MemberInfo member)
        {
            return member.Name.IndexOf('.') >= 0;
        }
    }
}
