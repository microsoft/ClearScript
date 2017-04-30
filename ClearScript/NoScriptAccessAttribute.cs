// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies that script code is to have no access to the target type member.
    /// </summary>
    /// <remarks>
    /// This attribute is applicable to events, fields, methods, properties, and nested types. Note
    /// that it has no effect on the method binding algorithm. If a script-based call is bound to a
    /// method that is blocked by this attribute, it will be rejected even if an overload exists
    /// that could receive the call.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class NoScriptAccessAttribute : ScriptUsageAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="NoScriptAccessAttribute"/> instance.
        /// </summary>
        public NoScriptAccessAttribute()
            : base(ScriptAccess.None)
        {
        }
    }
}
