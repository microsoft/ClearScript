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
        /// Initializes a new <see cref="V8RuntimeConstraints"/> instance.
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
            get { return MaxNewSpaceSize; }
            set { MaxNewSpaceSize = value; }
        }
    }
}
