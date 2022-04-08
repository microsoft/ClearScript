// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines global V8 options.
    /// </summary>
    [Flags]
    public enum V8GlobalFlags : uint
    {
        // IMPORTANT: maintain bitwise equivalence with unmanaged enum V8GlobalFlags

        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that <see href="https://github.com/tc39/proposal-top-level-await">Top-Level Await</see> is to be enabled.
        /// </summary>
        [Obsolete("V8 no longer supports Top-Level Await control. The feature is always enabled.")]
        EnableTopLevelAwait = 0x00000001,

        /// <summary>
        /// Specifies that just-in-time compilation is to be disabled.
        /// </summary>
        DisableJITCompilation = 0x00000002,

        /// <summary>
        /// Specifies that background work is to be disabled. By default, V8 performs various tasks
        /// in the background, accelerating garbage collection, just-in-time compilation, and other
        /// activities. Use this option if you encounter issues related to V8's background work.
        /// </summary>
        DisableBackgroundWork = 0x00000004
    }
}
