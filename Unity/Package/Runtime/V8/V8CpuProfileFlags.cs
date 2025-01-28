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
}
