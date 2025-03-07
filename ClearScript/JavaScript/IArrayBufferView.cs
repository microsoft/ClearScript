// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines properties and methods common to all
    /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>
    /// views.
    /// </summary>
    public interface IArrayBufferView : IJavaScriptObject
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
        /// Copies bytes from the view into the specified byte span.
        /// </summary>
        /// <param name="offset">The offset within the view of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="destination">The byte span into which to copy the bytes.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong ReadBytes(ulong offset, ulong count, Span<byte> destination, ulong destinationIndex);

        /// <summary>
        /// Copies bytes from the specified byte array into the view.
        /// </summary>
        /// <param name="source">The byte array from which to copy the bytes.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="offset">The offset within the view at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset);

        /// <summary>
        /// Copies bytes from the specified byte span into the view.
        /// </summary>
        /// <param name="source">The byte span from which to copy the bytes.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first byte to copy.</param>
        /// <param name="count">The maximum number of bytes to copy.</param>
        /// <param name="offset">The offset within the view at which to store the first copied byte.</param>
        /// <returns>The number of bytes copied.</returns>
        ulong WriteBytes(ReadOnlySpan<byte> source, ulong sourceIndex, ulong count, ulong offset);

        /// <summary>
        /// Invokes a delegate that returns no value, giving it direct access to the view's contents.
        /// </summary>
        /// <param name="action">The delegate to invoke.</param>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the view's
        /// contents. This memory address is valid only while the delegate is executing. The
        /// delegate must not access memory outside the view's range.
        /// </remarks>
        void InvokeWithDirectAccess(Action<IntPtr> action);

        /// <summary>
        /// Invokes a delegate that returns a value, giving it direct access to the view's contents.
        /// </summary>
        /// <typeparam name="TResult">The delegate's return type.</typeparam>
        /// <param name="func">The delegate to invoke.</param>
        /// <returns>The delegate's return value.</returns>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the view's
        /// contents. This memory address is valid only while the delegate is executing. The
        /// delegate must not access memory outside the view's range.
        /// </remarks>
        TResult InvokeWithDirectAccess<TResult>(Func<IntPtr, TResult> func);

        /// <summary>
        /// Invokes a delegate that takes an argument and returns no value, giving it direct access to the view's contents.
        /// </summary>
        /// <typeparam name="TArg">The delegate's argument type.</typeparam>
        /// <param name="action">The delegate to invoke.</param>
        /// <param name="arg">The argument to pass to the delegate.</param>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the view's
        /// contents. This memory address is valid only while the delegate is executing. The
        /// delegate must not access memory outside the view's range.
        /// </remarks>
        void InvokeWithDirectAccess<TArg>(Action<IntPtr, TArg> action, in TArg arg);

        /// <summary>
        /// Invokes a delegate that takes an argument and returns a value, giving it direct access to the view's contents.
        /// </summary>
        /// <typeparam name="TArg">The delegate's argument type.</typeparam>
        /// <typeparam name="TResult">The delegate's return type.</typeparam>
        /// <param name="func">The delegate to invoke.</param>
        /// <param name="arg">The argument to pass to the delegate.</param>
        /// <returns>The delegate's return value.</returns>
        /// <remarks>
        /// This method invokes the specified delegate, passing in the memory address of the view's
        /// contents. This memory address is valid only while the delegate is executing. The
        /// delegate must not access memory outside the view's range.
        /// </remarks>
        TResult InvokeWithDirectAccess<TArg, TResult>(Func<IntPtr, TArg, TResult> func, in TArg arg);
    }
}
