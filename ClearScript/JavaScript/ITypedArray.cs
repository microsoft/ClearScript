// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines properties and methods common to all JavaScript
    /// <see href="https://msdn.microsoft.com/en-us/library/br212485(v=vs.94).aspx">typed arrays</see>.
    /// </summary>
    public interface ITypedArray : IArrayBufferView
    {
        /// <summary>
        /// Gets the typed array's length.
        /// </summary>
        ulong Length { get; }
    }

    /// <summary>
    /// Represents a JavaScript <see href="https://msdn.microsoft.com/en-us/library/br212485(v=vs.94).aspx">typed array</see>.
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
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212477(v=vs.94).aspx">Uint8Array</see></term>
    ///         <term><c>ITypedArray&#x3C;byte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/dn641188(v=vs.94).aspx">Uint8ClampedArray</see></term>
    ///         <term><c>ITypedArray&#x3C;byte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212462(v=vs.94).aspx">Int8Array</see></term>
    ///         <term><c>ITypedArray&#x3C;sbyte&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212484(v=vs.94).aspx">Uint16Array</see></term>
    ///         <term><c>ITypedArray&#x3C;ushort&#x3E;</c> and <c>ITypedArray&#x3C;char&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212480(v=vs.94).aspx">Int16Array</see></term>
    ///         <term><c>ITypedArray&#x3C;short&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br230737(v=vs.94).aspx">Uint32Array</see></term>
    ///         <term><c>ITypedArray&#x3C;uint&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212468(v=vs.94).aspx">Int32Array</see></term>
    ///         <term><c>ITypedArray&#x3C;int&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212916(v=vs.94).aspx">Float32Array</see></term>
    ///         <term><c>ITypedArray&#x3C;float&#x3E;</c></term>
    ///     </item>
    ///     <item>
    ///         <term><see href="https://msdn.microsoft.com/en-us/library/br212931(v=vs.94).aspx">Float64Array</see></term>
    ///         <term><c>ITypedArray&#x3C;double&#x3E;</c></term>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface ITypedArray<T> : ITypedArray
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
        /// Copies elements from the specified array into the typed array.
        /// </summary>
        /// <param name="source">The array from which to copy the elements.</param>
        /// <param name="sourceIndex">The index within <paramref name="source"/> of the first element to copy.</param>
        /// <param name="length">The maximum number of elements to copy.</param>
        /// <param name="index">The index within the typed array at which to store the first copied element.</param>
        /// <returns>The number of elements copied.</returns>
        ulong Write(T[] source, ulong sourceIndex, ulong length, ulong index);
    }
}
