// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies how the target type member is to be exposed to script code. This extended version
    /// supports additional options.
    /// </summary>
    /// <remarks>
    /// This attribute is applicable to events, fields, methods, and properties.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class ScriptMemberAttribute : ScriptUsageAttribute
    {
        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance.
        /// </summary>
        public ScriptMemberAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified name.
        /// </summary>
        /// <param name="name">The name that script code will use to access the target type member.</param>
        public ScriptMemberAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified script access setting.
        /// </summary>
        /// <param name="access">The script access setting for the target type member.</param>
        public ScriptMemberAttribute(ScriptAccess access)
            : base(access)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified name and script access setting.
        /// </summary>
        /// <param name="name">The name that script code will use to access the target type member.</param>
        /// <param name="access">The script access setting for the target type member.</param>
        public ScriptMemberAttribute(string name, ScriptAccess access)
            : base(access)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified script options.
        /// </summary>
        /// <param name="flags">The script options for the target type member.</param>
        public ScriptMemberAttribute(ScriptMemberFlags flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified name and script options.
        /// </summary>
        /// <param name="name">The name that script code will use to access the target type member.</param>
        /// <param name="flags">The script options for the target type member.</param>
        public ScriptMemberAttribute(string name, ScriptMemberFlags flags)
        {
            Name = name;
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified script access setting and script options.
        /// </summary>
        /// <param name="access">The script access setting for the target type member.</param>
        /// <param name="flags">The script options for the target type member.</param>
        public ScriptMemberAttribute(ScriptAccess access, ScriptMemberFlags flags)
            : base(access)
        {
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <c><see cref="ScriptMemberAttribute"/></c> instance with the specified name, script access setting, and script options.
        /// </summary>
        /// <param name="name">The name that script code will use to access the target type member.</param>
        /// <param name="access">The script access setting for the target type member.</param>
        /// <param name="flags">The script options for the target type member.</param>
        public ScriptMemberAttribute(string name, ScriptAccess access, ScriptMemberFlags flags)
            : base(access)
        {
            Name = name;
            Flags = flags;
        }

        /// <summary>
        /// Gets or sets the name that script code will use to access the target type member.
        /// </summary>
        /// <remarks>
        /// The default value is the name of the target type member. Note that this property has no
        /// effect on the method binding algorithm. If a script-based call is bound to a method
        /// that is exposed under a different name, it will be rejected even if an overload exists
        /// that could receive the call.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the script options for the target type member.
        /// </summary>
        public ScriptMemberFlags Flags { get; set; }
    }
}
