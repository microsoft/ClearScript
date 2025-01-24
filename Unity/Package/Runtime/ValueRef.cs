// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides a factory method for <c><see cref="ValueRef{T}"/></c>.
    /// </summary>
    public static class ValueRef
    {
        /// <summary>
        /// Creates a <c><see cref="ValueRef{T}"/></c> instance with the given value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value to assign to the instance.</param>
        /// <returns>A <c><see cref="ValueRef{T}"/></c> instance holding the given value.</returns>
        public static ValueRef<T> Create<T>(T value = default) where T : struct
        {
            return new ValueRef<T> { Value = value };
        }
    }

    /// <summary>
    /// Holds a <see href="https://learn.microsoft.com/en-us/dotnet/api/system.valuetype">value</see> on the heap.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <remarks>
    /// This utility class can be used for passing mutable values to asynchronous methods.
    /// </remarks>
    public class ValueRef<T> where T : struct
    {
        internal ValueRef()
        {
        }

        /// <summary>
        /// The value currently held by the <c><see cref="ValueRef{T}"/></c> instance.
        /// </summary>
        public T Value;
    }
}
