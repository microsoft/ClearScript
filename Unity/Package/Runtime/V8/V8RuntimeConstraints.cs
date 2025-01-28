// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Specifies resource constraints for a V8 runtime.
    /// </summary>
    public sealed class V8RuntimeConstraints
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="V8RuntimeConstraints"/></c> instance.
        /// </summary>
        public V8RuntimeConstraints()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Gets or sets the maximum size of the new object heap in
        /// <see href="http://en.wikipedia.org/wiki/Mebibyte">MiB</see>.
        /// </summary>
        /// <remarks>
        /// For maximum compatibility with hosts that predate an inadvertent breaking change in
        /// ClearScript 5.4.1, values greater than 1048576
        /// (1 <see href="http://en.wikipedia.org/wiki/Tebibyte">TiB</see>) are assumed to be in
        /// bytes rather than MiB. For example, the values 16 and 16777216 both specify a limit
        /// of 16 MiB.
        /// </remarks>
        public int MaxNewSpaceSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the old object heap in
        /// <see href="http://en.wikipedia.org/wiki/Mebibyte">MiB</see>.
        /// </summary>
        /// <remarks>
        /// For maximum compatibility with hosts that predate an inadvertent breaking change in
        /// ClearScript 5.4.1, values greater than 1048576
        /// (1 <see href="http://en.wikipedia.org/wiki/Tebibyte">TiB</see>) are assumed to be in
        /// bytes rather than MiB. For example, the values 16 and 16777216 both specify a limit
        /// of 16 MiB.
        /// </remarks>
        public int MaxOldSpaceSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the executable code heap in
        /// <see href="http://en.wikipedia.org/wiki/Mebibyte">MiB</see>.
        /// </summary>
        /// <remarks>
        /// For maximum compatibility with hosts that predate an inadvertent breaking change in
        /// ClearScript 5.4.1, values greater than 1048576
        /// (1 <see href="http://en.wikipedia.org/wiki/Tebibyte">TiB</see>) are assumed to be in
        /// bytes rather than MiB. For example, the values 16 and 16777216 both specify a limit
        /// of 16 MiB.
        /// </remarks>
        [Obsolete("Executable code now occupies the old object heap. See MaxOldSpaceSize.")]
        public int MaxExecutableSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the young object heap in
        /// <see href="http://en.wikipedia.org/wiki/Mebibyte">MiB</see>.
        /// </summary>
        /// <remarks>
        /// For maximum compatibility with hosts that predate an inadvertent breaking change in
        /// ClearScript 5.4.1, values greater than 1048576
        /// (1 <see href="http://en.wikipedia.org/wiki/Tebibyte">TiB</see>) are assumed to be in
        /// bytes rather than MiB. For example, the values 16 and 16777216 both specify a limit
        /// of 16 MiB.
        /// </remarks>
        [Obsolete("Use MaxNewSpaceSize instead.")]
        public int MaxYoungSpaceSize
        {
            get => MaxNewSpaceSize;
            set => MaxNewSpaceSize = value;
        }

        /// <summary>
        /// Gets or sets the heap expansion multiplier.
        /// </summary>
        /// <remarks>
        /// When set to a value greater than 1, this property enables on-demand heap expansion,
        /// which automatically increases the maximum heap size by the specified multiplier
        /// whenever the script engine is close to exceeding the current limit. Note that a buggy
        /// or malicious script can still cause an application to fail by exhausting its address
        /// space or total available memory. On-demand heap expansion is recommended for use in
        /// conjunction with heap size monitoring (see <c><see cref="V8Runtime.MaxHeapSize"/></c>,
        /// <c><see cref="V8ScriptEngine.MaxRuntimeHeapSize"/></c>) to help contain runaway scripts.
        /// </remarks>
        public double HeapExpansionMultiplier { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>
        /// memory the runtime may allocate.
        /// </summary>
        /// <remarks>
        /// This property is specified in bytes. <c>ArrayBuffer</c> memory is allocated outside the
        /// runtime's heap and released when its garbage collector reclaims the corresponding
        /// JavaScript <c>ArrayBuffer</c> object. Leave this property at its default value to
        /// enforce no limit.
        /// </remarks>
        public ulong MaxArrayBufferAllocation { get; set; } = ulong.MaxValue;
    }
}
