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
        public static bool IsScriptable(this EventInfo eventInfo, ScriptAccess defaultAccess)
        {
            return !eventInfo.IsSpecialName && !eventInfo.IsExplicitImplementation() && !eventInfo.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this FieldInfo field, ScriptAccess defaultAccess)
        {
            return !field.IsSpecialName && !field.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this MethodInfo method, ScriptAccess defaultAccess)
        {
            return !method.IsSpecialName && !method.IsExplicitImplementation() && !method.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this PropertyInfo property, ScriptAccess defaultAccess)
        {
            return !property.IsSpecialName && !property.IsExplicitImplementation() && !property.IsBlockedFromScript(defaultAccess);
        }

        public static bool IsScriptable(this Type type, ScriptAccess defaultAccess)
        {
            return !type.IsSpecialName && !type.IsBlockedFromScript(defaultAccess);
        }

        public static string GetScriptName(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptMemberAttribute>(true);
            return ((attribute != null) && (attribute.Name != null)) ? attribute.Name : member.GetShortName();
        }

        public static bool IsBlockedFromScript(this MemberInfo member, ScriptAccess defaultAccess)
        {
            return member.GetScriptAccess(defaultAccess) == ScriptAccess.None;
        }

        public static bool IsReadOnlyForScript(this MemberInfo member, ScriptAccess defaultAccess)
        {
            return member.GetScriptAccess(defaultAccess) == ScriptAccess.ReadOnly;
        }

        public static ScriptAccess GetScriptAccess(this MemberInfo member, ScriptAccess defaultValue)
        {
            var attribute = member.GetAttribute<ScriptUsageAttribute>(true);
            if (attribute != null)
            {
                return attribute.Access;
            }

            var declaringType = member.DeclaringType;
            if (declaringType != null)
            {
                var testType = declaringType;
                do
                {
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
