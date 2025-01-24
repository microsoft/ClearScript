// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines options for initializing a new V8 runtime instance.
    /// </summary>
    [Flags]
    public enum V8RuntimeFlags
    {
        // IMPORTANT: maintain bitwise equivalence with unmanaged enum V8Isolate::Flags

        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that script debugging features are to be enabled.
        /// </summary>
        EnableDebugging = 0x00000001,

        /// <summary>
        /// Specifies that remote script debugging is to be enabled. This option is ignored if
        /// <c><see cref="EnableDebugging"/></c> is not specified.
        /// </summary>
        EnableRemoteDebugging = 0x00000002,

        /// <summary>
        /// Specifies that
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import#Dynamic_Imports">dynamic module imports</see>
        /// are to be enabled. This is an experimental feature and may be removed in a future release.
        /// </summary>
        EnableDynamicModuleImports = 0x00000004
    }
}
