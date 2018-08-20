// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines caching options for V8 script compilation.
    /// </summary>
    public enum V8CacheKind
    {
        /// <summary>
        /// Specifies that no cache data is to be generated or consumed during V8 script
        /// compilation. This option results in the most efficient script compilation when no cache
        /// data is available.
        /// </summary>
        None,

        /// <summary>
        /// Selects parser caching. Parser cache data is smaller and less expensive to generate
        /// than code cache data, but it is less effective at accelerating recompilation.
        /// </summary>
        [Obsolete("V8 no longer supports parser caching. This option is now equivalent to Code.")]
        Parser,

        /// <summary>
        /// Selects code caching. Code cache data is larger and more expensive to generate than
        /// parser cache data, but it is more effective at accelerating recompilation.
        /// </summary>
        Code
    }
}
