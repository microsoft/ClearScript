// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents an undefined value.
    /// </summary>
    /// <remarks>
    /// Some script languages support one or more special non-<c>null</c> values that represent
    /// nonexistent, missing, unknown, or undefined data. The ClearScript library maps such values
    /// to an instance of this class.
    /// </remarks>
    /// <c><seealso cref="ScriptEngine.UndefinedImportValue"/></c>
    public class Undefined
    {
        /// <summary>
        /// The sole instance of the <c><see cref="Undefined"/></c> class.
        /// </summary>
        public static readonly Undefined Value = new();

        private Undefined()
        {
        }

        #region Object overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <remarks>
        /// The <c><see cref="Undefined"/></c> version of this method returns "[undefined]".
        /// </remarks>
        public override string ToString()
        {
            return "[undefined]";
        }

        #endregion
    }
}
