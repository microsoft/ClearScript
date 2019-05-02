namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an empty object reference.
    /// </summary>
    /// <remarks>
    /// When passed to a Windows Script engine, an instance of this class is marshaled as an empty
    /// variant of type <c>VT_DISPATCH</c>. VBScript interprets this as the special object
    /// reference
    /// <c><see href="https://msdn.microsoft.com/en-us/library/f8tbc79x(v=vs.85).aspx">Nothing</see></c>.
    /// In JScript it appears as a value that is equal to, but not strictly equal to,
    /// <c><see href="https://developer.mozilla.org/en-US/docs/Glossary/Undefined">undefined</see></c>.
    /// </remarks>
    public class Nothing
    {
        /// <summary>
        /// The sole instance of the <see cref="Nothing"/> class.
        /// </summary>
        public static readonly Nothing Value = new Nothing();

        private Nothing()
        {
        }

        #region Object overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        /// <remarks>
        /// The <see cref="Nothing"/> version of this method returns "[nothing]".
        /// </remarks>
        public override string ToString()
        {
            return "[nothing]";
        }

        #endregion
    }
}
