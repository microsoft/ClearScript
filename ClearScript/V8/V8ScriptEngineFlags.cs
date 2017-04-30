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
        DisableGlobalMembers = 0x00000002
    }
}
