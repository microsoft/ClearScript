// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines options for initializing a new V8 JavaScript engine instance.
    /// </summary>
    [Flags]
    public enum V8ScriptEngineFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that script debugging features are to be enabled.
        /// </summary>
        EnableDebugging = 0x00000001,

        /// <summary>
        /// Specifies that support for <see cref="HostItemFlags.GlobalMembers"/> behavior is to be
        /// disabled. This option yields a significant performance benefit for global item access.
        /// </summary>
        DisableGlobalMembers = 0x00000002,

        /// <summary>
        /// Specifies that remote script debugging is to be enabled. This option is ignored if
        /// <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        EnableRemoteDebugging = 0x00000004,

        /// <summary>
        /// Specifies that the script engine is to wait for a debugger connection and schedule a
        /// pause before executing the first line of application script code. This option is
        /// ignored if <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        AwaitDebuggerAndPauseOnStart = 0x00000008
    }
}
