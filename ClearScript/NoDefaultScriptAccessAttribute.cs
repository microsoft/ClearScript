// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies that script code is to have no access to type members by default.
    /// </summary>
    /// <remarks>
    /// This attribute is applicable to classes, enums, interfaces, structs, and assemblies. Use
    /// <see cref="ScriptUsageAttribute"/>, <see cref="ScriptMemberAttribute"/>, or
    /// <see cref="NoScriptAccessAttribute"/> to override it for individual type members. Note that
    /// it has no effect on the method binding algorithm. If a script-based call is bound to a
    /// method that is blocked by this attribute, it will be rejected even if an overload exists
    /// that could receive the call.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Assembly)]
    public sealed class NoDefaultScriptAccessAttribute : DefaultScriptUsageAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="NoDefaultScriptAccessAttribute"/> instance.
        /// </summary>
        public NoDefaultScriptAccessAttribute()
            : base(ScriptAccess.None)
        {
        }
    }
}
