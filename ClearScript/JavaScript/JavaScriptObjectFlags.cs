// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines JavaScript object attributes.
    /// </summary>
    [Flags]
    public enum JavaScriptObjectFlags
    {
        /// <summary>
        /// Indicates that no attributes are present.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the object is an
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/ArrayBuffer">ArrayBuffer</see></c>,
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView">DataView</see></c>,
        /// or
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Typed_arrays">typed array</see> whose contents reside in shared memory.
        /// </summary>
        Shared = 0x00000001,

        /// <summary>
        /// Indicates that the object is an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function">async function</see>
        /// or
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols#the_async_iterator_and_async_iterable_protocols">async iterator</see>.
        /// </summary>
        Async = 0x00000002,

        /// <summary>
        /// Indicates that the object is a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Iterators_and_Generators#generator_functions">generator function</see>
        /// or
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function*">async generator function</see>.
        /// </summary>
        Generator = 0x00000004,

        /// <summary>
        /// Indicates that the object is a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// that is not yet settled. If the object is a promise and this attribute is not present,
        /// then the promise is fulfilled unless <c><see cref="Rejected"/></c> is present.
        /// </summary>
        Pending = 0x00000008,

        /// <summary>
        /// Indicates that the object is a rejected
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>.
        /// This attribute has no meaning if <c><see cref="Pending"/></c> is present.
        /// </summary>
        Rejected = 0x00000010
    }

    internal static class JavaScriptObjectFlagsHelpers
    {
        public static bool HasAllFlags(this JavaScriptObjectFlags value, JavaScriptObjectFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this JavaScriptObjectFlags value, JavaScriptObjectFlags flags) => (value & flags) != 0;
    }
}
