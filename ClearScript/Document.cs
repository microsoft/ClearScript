// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Text;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides an abstract representation of a document.
    /// </summary>
    public abstract class Document
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <see cref="Document"/> instance.
        /// </summary>
        protected Document()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Gets a structure containing meta-information for the document.
        /// </summary>
        public abstract DocumentInfo Info { get; }

        /// <summary>
        /// Gets a stream that provides read access to the document.
        /// </summary>
        public abstract Stream Contents { get; }

        /// <summary>
        /// Gets the document's character encoding.
        /// </summary>
        /// <remarks>
        /// This property returns <c>null</c> if the document contains binary data or if its
        /// character encoding is unknown.
        /// </remarks>
        public virtual Encoding Encoding
        {
            get { return null; }
        }
    }
}
