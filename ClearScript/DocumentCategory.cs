// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a document category.
    /// </summary>
    public abstract class DocumentCategory
    {
        internal DocumentCategory()
        {
        }

        /// <summary>
        /// Gets the document category for normal scripts.
        /// </summary>
        public static DocumentCategory Script
        {
            get { return ScriptDocument.Instance; }
        }

        internal abstract string DefaultName { get; }

        #region Nested type: ScriptDocument

        private sealed class ScriptDocument : DocumentCategory
        {
            public static readonly ScriptDocument Instance = new ScriptDocument();

            private ScriptDocument()
            {
            }

            #region DocumentCategory overrides

            internal override string DefaultName
            {
                get { return "Script"; }
            }

            #endregion

            #region Object overrides

            public override string ToString()
            {
                return "Script";
            }

            #endregion
        }

        #endregion
    }
}
