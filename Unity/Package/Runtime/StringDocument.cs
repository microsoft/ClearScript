// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides an in-memory <c><see cref="Document"/></c> implementation for a text document.
    /// </summary>
    public class StringDocument : Document
    {
        /// <summary>
        /// Initializes a new <c><see cref="StringDocument"/></c> instance.
        /// </summary>
        /// <param name="info">A structure containing meta-information for the document.</param>
        /// <param name="contents">A string containing the document's contents.</param>
        public StringDocument(DocumentInfo info, string contents)
        {
            Info = info;
            StringContents = contents;
        }

        /// <summary>
        /// Gets the document's contents as a string.
        /// </summary>
        public string StringContents { get; }

        #region Document overrides

        /// <summary>
        /// Gets a structure containing meta-information for the document.
        /// </summary>
        public override DocumentInfo Info { get; }

        /// <summary>
        /// Gets a stream that provides read access to the document.
        /// </summary>
        /// <remarks>
        /// The <c><see cref="StringDocument"/></c> implementation of this property returns a
        /// <c><see cref="MemoryStream"/></c> instance.
        /// </remarks>
        public override Stream Contents => new MemoryStream(Encoding.GetBytes(StringContents), false);

        /// <summary>
        /// Gets the document's character encoding.
        /// </summary>
        /// <remarks>
        /// <c><see cref="StringDocument"/></c> instances return <c><see cref="System.Text.Encoding.UTF8"/></c> for this property.
        /// </remarks>
        public override Encoding Encoding => Encoding.UTF8;

        #endregion
    }
}
