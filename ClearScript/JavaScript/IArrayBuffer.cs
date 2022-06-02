// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Represents a JavaScript
    /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>.
    /// </summary>
    public interface IArrayBuffer
    {
        /// <summary>
        /// Gets the size of the <c>ArrayBuffer</c> in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Creates a byte array containing a copy of the <c>ArrayBuffer</c>'s contents.
        /// </summary>
        /// <returns>A new byte array containing a copy of the <c>ArrayBuffer</c>'s contents.</returns>
        byte[] GetBytes();

        /// <summary>
        /// Copies bytes from the <c>ArrayBuffer</c> into the specified byte array.
        /// </summary>
        /// <param name="offset">The offset within the <c>ArrayBuffer</c> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="destination">The byte array into which to copy the bytes.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex);

        /// <summary>
        /// Copies bytes from the specified byte array into the <c>ArrayBuffer</c>.
        /// </summary>
        /// <param name="source">The byte array from which to copy the bytes.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="offset">The offset within the <c>ArrayBuffer</c> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset);

        /// <summary>
        /// Invokes a delegate that returns no value, giving it direct access to the <c>ArrayBuffer</c>'s contents.
        /// </summary>
        /// <param name="action">The delegate to invoke.</param>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the
        /// <c>ArrayBuffer</c>'s contents. This memory address is valid only while the delegate is
        /// executing. The delegate must not access memory outside the <c>ArrayBuffer</c>'s range.
        /// </remarks>
        void InvokeWithDirectAccess(Action<IntPtr> action);

        /// <summary>
        /// Invokes a delegate that returns a value, giving it direct access to the <c>ArrayBuffer</c>'s contents.
        /// </summary>
        /// <typeparam name="T">The delegate's return type.</typeparam>
        /// <param name="func">The delegate to invoke.</param>
        /// <returns>The delegate's return value.</returns>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the
        /// <c>ArrayBuffer</c>'s contents. This memory address is valid only while the delegate is
        /// executing. The delegate must not access memory outside the <c>ArrayBuffer</c>'s range.
        /// </remarks>
        T InvokeWithDirectAccess<T>(Func<IntPtr, T> func);
    }
}
