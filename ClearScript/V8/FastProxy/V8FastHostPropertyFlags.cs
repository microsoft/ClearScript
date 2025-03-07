// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Defines fast host property attributes.
    /// </summary>
    [Flags]
    public enum V8FastHostPropertyFlags
    {
        // IMPORTANT: maintain bitwise equivalence with native enum FastHostObjectUtil::PropertyFlags

        /// <summary>
        /// Indicates that no attributes are present.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the property exists.
        /// </summary>
        Available = 0x00000001,

        /// <summary>
        /// Indicates that the property value is a constant that may be cached for faster retrieval.
        /// </summary>
        Cacheable = 0x00000002,

        /// <summary>
        /// Indicates that the property is enumerable via mechanisms such as <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/for...in">for...in</see></c>.
        /// </summary>
        Enumerable = 0x00000004,

        /// <summary>
        /// Indicates that the property can be assigned.
        /// </summary>
        Writable = 0x00000008,

        /// <summary>
        /// Indicates that the property can be deleted.
        /// </summary>
        Deletable = 0x00000010
    }

    internal static class V8FastHostPropertyFlagsHelpers
    {
        public static bool HasAllFlags(this V8FastHostPropertyFlags value, V8FastHostPropertyFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this V8FastHostPropertyFlags value, V8FastHostPropertyFlags flags) => (value & flags) != 0;
    }
}
