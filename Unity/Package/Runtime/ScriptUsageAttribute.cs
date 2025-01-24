// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies how the target type member is to be exposed to script code.
    /// </summary>
    /// <remarks>
    /// This attribute is applicable to events, fields, methods, properties, and nested types.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class ScriptUsageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <c><see cref="ScriptUsageAttribute"/></c> instance.
        /// </summary>
        public ScriptUsageAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptUsageAttribute"/></c> instance with the specified script access setting.
        /// </summary>
        /// <param name="access">The script access setting for the target type member.</param>
        public ScriptUsageAttribute(ScriptAccess access)
        {
            Access = access;
        }

        /// <summary>
        /// Gets the script access setting for the target type member.
        /// </summary>
        public ScriptAccess Access { get; }
    }
}
