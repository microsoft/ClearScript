// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents an undefined value.
    /// </summary>
    /// <remarks>
    /// Most script languages support one or more special values that represent nonexistent,
    /// missing, unknown, or undefined data. The ClearScript library maps some such values to
    /// <c>null</c>, and others to instances of this class.
    /// </remarks>
    public class Undefined
    {
        internal static readonly Undefined Value = new Undefined();

        private Undefined()
        {
        }

        #region Object overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <remarks>
        /// The <see cref="Undefined"/> version of this method returns "[undefined]".
        /// </remarks>
        public override string ToString()
        {
            return "[undefined]";
        }

        #endregion
    }
}
