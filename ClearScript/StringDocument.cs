// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides an in-memory <see cref="Document"/> implementation for a text document.
    /// </summary>
    public class StringDocument : Document
    {
        private readonly DocumentInfo info;
        private readonly byte[] contents;

        /// <summary>
        /// Initializes a new <see cref="StringDocument"/> instance.
        /// </summary>
        /// <param name="info">A structure containing meta-information for the document.</param>
        /// <param name="contents">A string containing the document's contents.</param>
        public StringDocument(DocumentInfo info, string contents)
        {
            this.info = info;
            this.contents = Encoding.UTF8.GetBytes(contents);
        }

        #region Document overrides

        /// <summary>
        /// Gets a structure containing meta-information for the document.
        /// </summary>
        public override DocumentInfo Info
        {
            get { return info; }
        }

        /// <summary>
        /// Gets a stream that provides read access to the document.
        /// </summary>
        /// <remarks>
        /// The <see cref="StringDocument"/> implementation of this property returns a
        /// <see cref="MemoryStream"/> instance.
        /// </remarks>
        public override Stream Contents
        {
            get { return new MemoryStream(contents, false); }
        }

        /// <summary>
        /// Gets the document's character encoding.
        /// </summary>
        /// <remarks>
        /// <see cref="StringDocument"/> instances return <see cref="System.Text.Encoding.UTF8"/> for this property.
        /// </remarks>
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion
    }
}
