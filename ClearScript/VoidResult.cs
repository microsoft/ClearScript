// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents the result of a host method that returns no value.
    /// </summary>
    /// <remarks>
    /// Some script languages expect every subroutine call to return a value. When script code
    /// written in such a language invokes a host method that explicitly returns no value (such
    /// as a C#
    /// <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/void">void</see>
    /// method), the ClearScript library provides an instance of this class as a dummy return
    /// value.
    /// </remarks>
    /// <seealso cref="ScriptEngine.VoidResultValue"/>
    public class VoidResult
    {
        /// <summary>
        /// The sole instance of the <see cref="VoidResult"/> class.
        /// </summary>
        public static readonly VoidResult Value = new VoidResult();

        private VoidResult()
        {
        }

        #region Object overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <remarks>
        /// The <see cref="VoidResult"/> version of this method returns "[void]".
        /// </remarks>
        public override string ToString()
        {
            return "[void]";
        }

        #endregion
    }
}
