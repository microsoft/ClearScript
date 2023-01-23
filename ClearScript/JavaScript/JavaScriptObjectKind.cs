// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines JavaScript object kinds.
    /// </summary>
    public enum JavaScriptObjectKind
    {
        /// <summary>
        /// Indicates that the object is generic or of an unknown kind.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates that the object is a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Functions">function</see>.
        /// </summary>
        Function,

        /// <summary>
        /// Indicates that the object is an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols">iterator</see>.
        /// </summary>
        Iterator,

        /// <summary>
        /// Indicates that the object is a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>.
        /// </summary>
        Promise,

        /// <summary>
        /// Indicates that the object is an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array">array</see>
        /// and implements
        /// <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.ilist">IList</see></c>.
        /// </summary>
        Array,

        /// <summary>
        /// Indicates that the object is an
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>
        /// and implements <c><see cref="IArrayBuffer"/></c>.
        /// </summary>
        ArrayBuffer,

        /// <summary>
        /// Indicates that the object is a
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView">DataView</see></c>
        /// and implements <c><see cref="IDataView"/></c>.
        /// </summary>
        DataView,

        /// <summary>
        /// Indicates that the object is a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays">typed array</see>
        /// and implements <c><see cref="ITypedArray{T}"/></c>.
        /// </summary>
        TypedArray
    }
}
