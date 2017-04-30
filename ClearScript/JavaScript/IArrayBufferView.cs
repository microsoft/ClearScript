// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines properties and methods common to all
    /// <see href="https://msdn.microsoft.com/en-us/library/br212474(v=vs.94).aspx">ArrayBuffer</see>
    /// views.
    /// </summary>
    public interface IArrayBufferView
    {
        /// <summary>
        /// Gets view's underlying <c>ArrayBuffer</c>.
        /// </summary>
        IArrayBuffer ArrayBuffer { get; }

        /// <summary>
        /// Gets the view's offset within the underlying <c>ArrayBuffer</c>.
        /// </summary>
        ulong Offset { get; }

        /// <summary>
        /// Gets the view's size in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Creates a byte array containing a copy of the view's contents.
        /// </summary>
        /// <returns>A new byte array containing a copy of the view's contents.</returns>
        byte[] GetBytes();

        /// <summary>
        /// Copies bytes from the view into the specified byte array.
        /// </summary>
        /// <param name="offset">The offset within the view of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="destination">The byte array into which to copy the bytes.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex);

        /// <summary>
        /// Copies bytes from the specified byte array into the view.
        /// </summary>
        /// <param name="source">The byte array from which to copy the bytes.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="offset">The offset within the view at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset);
    }
}
