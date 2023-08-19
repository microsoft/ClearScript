// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines cache data processing results for V8 script compilation.
    /// </summary>
    public enum V8CacheResult
    {
        // IMPORTANT: maintain bitwise equivalence with unmanaged enum V8CacheResult

        /// <summary>
        /// Indicates that cache data processing was disabled because the caller specified
        /// <c><see cref="V8CacheKind">V8CacheKind.None</see></c>.
        /// </summary>
        Disabled,

        /// <summary>
        /// Indicates that the provided cache data was accepted and used to accelerate script
        /// compilation.
        /// </summary>
        Accepted,

        /// <summary>
        /// Indicates that script compilation was bypassed because a suitable compiled script was
        /// found in the V8 runtime's memory, but the provided cache data was verified to be
        /// correct.
        /// </summary>
        Verified,

        /// <summary>
        /// Indicates that the provided cache data was updated because it was empty, stale, or
        /// invalid.
        /// </summary>
        Updated,

        /// <summary>
        /// Indicates that the provided cache data was empty, stale, or invalid, but new cache data
        /// could not be generated, and no additional information was provided by the V8 runtime.
        /// </summary>
        UpdateFailed
    }
}
