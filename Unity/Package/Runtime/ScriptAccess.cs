// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines script access settings for type members.
    /// </summary>
    public enum ScriptAccess
    {
        /// <summary>
        /// Specifies that script code is to have full access to the type member. This is the
        /// default setting.
        /// </summary>
        Full,

        /// <summary>
        /// Specifies that script code is to have read-only access to the type member. This setting
        /// only affects fields and writable properties.
        /// </summary>
        ReadOnly,

        /// <summary>
        /// Specifies that script code is to have no access to the type member. Note that this
        /// setting has no effect on the method binding algorithm. If a script-based call is bound
        /// to a method that is blocked by this setting, it will be rejected even if an overload
        /// exists that could receive the call.
        /// </summary>
        None
    }
}
