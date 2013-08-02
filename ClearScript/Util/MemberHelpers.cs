// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal static class MemberHelpers
    {
        public static bool IsScriptable(this EventInfo eventInfo)
        {
            return !eventInfo.IsSpecialName && !IsExplicitImplementation(eventInfo) && !eventInfo.IsBlockedFromScript();
        }

        public static bool IsScriptable(this FieldInfo field)
        {
            return !field.IsSpecialName && !field.IsBlockedFromScript();
        }

        public static bool IsScriptable(this MethodInfo method)
        {
            return !method.IsSpecialName && !IsExplicitImplementation(method) && !method.IsBlockedFromScript();
        }

        public static bool IsScriptable(this PropertyInfo property)
        {
            return !property.IsSpecialName && !IsExplicitImplementation(property) && !property.IsBlockedFromScript();
        }

        public static string GetScriptName(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptMemberAttribute>(true);
            return ((attribute != null) && (attribute.Name != null)) ? attribute.Name : member.GetShortName();
        }

        public static bool IsBlockedFromScript(this MemberInfo member)
        {
            return member.GetScriptAccess() == ScriptAccess.None;
        }

        public static bool IsReadOnlyForScript(this MemberInfo member)
        {
            return member.GetScriptAccess() == ScriptAccess.ReadOnly;
        }

        public static bool IsRestrictedForScript(this MemberInfo member)
        {
            return !member.GetScriptMemberFlags().HasFlag(ScriptMemberFlags.ExposeRuntimeType);
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

        private static ScriptAccess GetScriptAccess(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptUsageAttribute>(true);
            return (attribute != null) ? attribute.Access : ScriptAccess.Full;
        }

        private static ScriptMemberFlags GetScriptMemberFlags(this MemberInfo member)
        {
            var attribute = member.GetAttribute<ScriptMemberAttribute>(true);
            return (attribute != null) ? attribute.Flags : ScriptMemberFlags.None;
        }

        private static T GetAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            return Attribute.GetCustomAttribute(member, typeof(T), inherit) as T;
        }
    }
}
