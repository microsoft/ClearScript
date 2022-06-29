// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Contains memory usage information for a V8 runtime.
    /// </summary>
    public class V8RuntimeHeapInfo
    {
        internal V8RuntimeHeapInfo()
        {
        }

        /// <summary>
        /// Gets the total heap size in bytes.
        /// </summary>
        public ulong TotalHeapSize { get; internal set; }

        /// <summary>
        /// Gets the total executable heap size in bytes.
        /// </summary>
        public ulong TotalHeapSizeExecutable { get; internal set; }

        /// <summary>
        /// Gets the total physical memory size in bytes.
        /// </summary>
        public ulong TotalPhysicalSize { get; internal set; }

        /// <summary>
        /// Gets the total available memory size in bytes.
        /// </summary>
        public ulong TotalAvailableSize { get; internal set; }

        /// <summary>
        /// Gets the used heap size in bytes.
        /// </summary>
        public ulong UsedHeapSize { get; internal set; }

        /// <summary>
        /// Gets the heap size limit in bytes.
        /// </summary>
        public ulong HeapSizeLimit { get; internal set; }

        /// <summary>
        /// Gets the total external memory size in bytes.
        /// </summary>
        public ulong TotalExternalSize { get; internal set; }
    }
}
