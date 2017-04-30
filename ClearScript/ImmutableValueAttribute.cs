// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Specifies that instances of the target struct are immutable.
    /// </summary>
    /// <remarks>
    /// When this attribute is applied to a struct, ClearScript exposes the same object reference
    /// for all instances of the struct that satisfy equality comparison, giving script code the
    /// ability to use native equality operators to compare the exposed objects. This behavior is
    /// also enabled automatically for all enums, numeric types, <see cref="DateTime"/>,
    /// <see cref="DateTimeOffset"/>, and <see cref="TimeSpan"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ImmutableValueAttribute : Attribute
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <see cref="ImmutableValueAttribute"/> instance.
        /// </summary>
        public ImmutableValueAttribute()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor
    }
}
