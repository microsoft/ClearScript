// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines options for creating a V8 CPU profile.
    /// </summary>
    [Flags]
    public enum V8CpuProfileFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that automatic sample collection is to be enabled.
        /// </summary>
        EnableSampleCollection = 0x00000001
    }

    internal static class V8CpuProfileFlagsHelpers
    {
        public static bool HasAllFlags(this V8CpuProfileFlags value, V8CpuProfileFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this V8CpuProfileFlags value, V8CpuProfileFlags flags) => (value & flags) != 0;
    }
}
