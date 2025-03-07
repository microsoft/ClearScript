// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines properties and methods common to all JavaScript
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays">typed arrays</see>.
    /// </summary>
    public interface ITypedArray : IArrayBufferView
    {
        /// <summary>
        /// Gets the typed array's length.
        /// </summary>
        ulong Length { get; }
    }

    // ReSharper disable GrammarMistakeInComment

    /// <summary>
    /// Represents a JavaScript <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays">typed array</see>.
    /// </summary>
    /// <typeparam name="T">The typed array's element type.</typeparam>
    /// <remarks>
    /// <para>
    /// The following table lists the specific interfaces implemented by JavaScript typed arrays:
    /// </para>
    /// <para>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Typed&#xA0;Array</term>
    ///         <term>Interface(s)&#xA0;(C#)</term>
    ///     </listheader>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint8Array">Uint8Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;byte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint8ClampedArray">Uint8ClampedArray</see></c></term>
    ///         <term><c>ITypedArray&#x3C;byte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Int8Array">Int8Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;sbyte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint16Array">Uint16Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;ushort&#x3E;</c> and <c>ITypedArray&#x3C;char&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Int16Array">Int16Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;short&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Uint32Array">Uint32Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;uint&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Int32Array">Int32Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;int&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigUint64Array">BigUint64Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;ulong&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt64Array">BigInt64Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;long&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Float32Array">Float32Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;float&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Float64Array">Float64Array</see></c></term>
    ///         <term><c>ITypedArray&#x3C;double&#x3E;</c></term>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface ITypedArray<T> : ITypedArray where T : unmanaged
    {
        /// <summary>
        /// Creates an array containing a copy of the typed array's contents.
        /// </summary>
        /// <returns>A new array containing a copy of the typed array's contents.</returns>
        T[] ToArray();

        /// <summary>
        /// Copies elements from the typed array into the specified array.
        /// </summary>
        /// <param name="index">The index within the typed array of the first element to copy.</param>
        /// <param name="length">The maximum number of elements to copy.</param>
        /// <param name="destination">The array into which to copy the elements.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied element.</param>
        /// <returns>The number of elements copied.</returns>
        ulong Read(ulong index, ulong length, T[] destination, ulong destinationIndex);

        /// <summary>
        /// Copies elements from the typed array into the specified span.
        /// </summary>
        /// <param name="index">The index within the typed array of the first element to copy.</param>
        /// <param name="length">The maximum number of elements to copy.</param>
        /// <param name="destination">The span into which to copy the elements.</param>
        /// <param name="destinationIndex">The index within <paramref name="destination"/> at which to store the first copied element.</param>
        /// <returns>The number of elements copied.</returns>
        ulong Read(ulong index, ulong length, Span<T> destination, ulong destinationIndex);

        /// <summary>
        /// Copies elements from the specified array into the typed array.
        /// </summary>
        /// <param name="source">The array from which to copy the elements.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first element to copy.</param>
        /// <param name="length">The maximum number of elements to copy.</param>
        /// <param name="index">The index within the typed array at which to store the first copied element.</param>
        /// <returns>The number of elements copied.</returns>
        ulong Write(T[] source, ulong sourceIndex, ulong length, ulong index);

        /// <summary>
        /// Copies elements from the specified span into the typed array.
        /// </summary>
        /// <param name="source">The span from which to copy the elements.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first element to copy.</param>
        /// <param name="length">The maximum number of elements to copy.</param>
        /// <param name="index">The index within the typed array at which to store the first copied element.</param>
        /// <returns>The number of elements copied.</returns>
        ulong Write(ReadOnlySpan<T> source, ulong sourceIndex, ulong length, ulong index);
    }

    // ReSharper restore GrammarMistakeInComment
}
