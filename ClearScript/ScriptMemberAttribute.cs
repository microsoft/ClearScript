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

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies how the target event, field, method, or property is to be exposed to script code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public sealed class ScriptMemberAttribute : ScriptUsageAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance.
        /// </summary>
        public ScriptMemberAttribute()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name that script code will use to access the type member.</param>
        public ScriptMemberAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified script access setting.
        /// </summary>
        /// <param name="access">The script access setting for the type member.</param>
        public ScriptMemberAttribute(ScriptAccess access)
            : base(access)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified name and script access setting.
        /// </summary>
        /// <param name="name">The name that script code will use to access the type member.</param>
        /// <param name="access">The script access setting for the type member.</param>
        public ScriptMemberAttribute(string name, ScriptAccess access)
            : base(access)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified script options.
        /// </summary>
        /// <param name="flags">The script options for the type member.</param>
        public ScriptMemberAttribute(ScriptMemberFlags flags)
        {
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified name and script options.
        /// </summary>
        /// <param name="name">The name that script code will use to access the type member.</param>
        /// <param name="flags">The script options for the type member.</param>
        public ScriptMemberAttribute(string name, ScriptMemberFlags flags)
        {
            Name = name;
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified script access setting and script options.
        /// </summary>
        /// <param name="access">The script access setting for the type member.</param>
        /// <param name="flags">The script options for the type member.</param>
        public ScriptMemberAttribute(ScriptAccess access, ScriptMemberFlags flags)
            : base(access)
        {
            Flags = flags;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptMemberAttribute"/> instance with the specified name, script access setting, and script options.
        /// </summary>
        /// <param name="name">The name that script code will use to access the type member.</param>
        /// <param name="access">The script access setting for the type member.</param>
        /// <param name="flags">The script options for the type member.</param>
        public ScriptMemberAttribute(string name, ScriptAccess access, ScriptMemberFlags flags)
            : base(access)
        {
            Name = name;
            Flags = flags;
        }

        /// <summary>
        /// Gets or sets the name that script code will use to access the type member.
        /// </summary>
        /// <remarks>
        /// The default value is the name of the type member. Note that this property has no effect
        /// on the method binding algorithm. If a script-based call is bound to a method that is
        /// exposed under a different name, it will be rejected even if an overload exists that
        /// could receive the call.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the script options for the type member.
        /// </summary>
        public ScriptMemberFlags Flags { get; set; }
    }
}
