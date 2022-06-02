// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies defaults for how type members are to be exposed to script code.
    /// </summary>
    /// <remarks>
    /// This attribute is applicable to classes, enums, interfaces, structs, and assemblies. Use
    /// <c><see cref="ScriptUsageAttribute"/></c>, <c><see cref="ScriptMemberAttribute"/></c>, or
    /// <c><see cref="NoScriptAccessAttribute"/></c> to override it for individual type members.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Assembly)]
    public class DefaultScriptUsageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <c><see cref="DefaultScriptUsageAttribute"/></c> instance.
        /// </summary>
        public DefaultScriptUsageAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="DefaultScriptUsageAttribute"/></c> instance with the specified default script access setting.
        /// </summary>
        /// <param name="access">The default script access setting for type members.</param>
        public DefaultScriptUsageAttribute(ScriptAccess access)
        {
            Access = access;
        }

        /// <summary>
        /// Gets the default script access setting for type members.
        /// </summary>
        public ScriptAccess Access { get; }
    }
}
