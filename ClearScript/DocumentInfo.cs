// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Contains information about a script document.
    /// </summary>
    public struct DocumentInfo
    {
        internal const string DefaultName = "Script Document";

        private readonly string name;
        private readonly Uri uri;

        /// <summary>
        /// Initializes a new <see cref="DocumentInfo"/> structure with the specified document name.
        /// </summary>
        /// <param name="name">The document name.</param>
        public DocumentInfo(string name)
            : this()
        {
            this.name = MiscHelpers.EnsureNonBlank(name, DefaultName);
        }

        /// <summary>
        /// Initializes a new <see cref="DocumentInfo"/> structure with the specified document URI.
        /// </summary>
        /// <param name="uri">The document URI.</param>
        public DocumentInfo(Uri uri)
            : this()
        {
            MiscHelpers.VerifyNonNullArgument(uri, "uri");
            this.uri = uri.IsAbsoluteUri ? uri : new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + uri);
            name = Path.GetFileName(this.uri.AbsolutePath);
        }

        /// <summary>
        /// Gets the document's name.
        /// </summary>
        /// <remarks>
        /// This property always returns a non-blank string. If a null or blank document name was
        /// specified at instantiation time, this property returns a default document name.
        /// </remarks>
        public string Name
        {
            get { return MiscHelpers.EnsureNonBlank(name, DefaultName); }
        }

        /// <summary>
        /// Gets the document's URI.
        /// </summary>
        /// <remarks>
        /// This property returns <c>null</c> if a URI was not specified at instantiation time.
        /// </remarks>
        public Uri Uri
        {
            get { return uri; }
        }

        /// <summary>
        /// Gets or sets a source map URI for the document.
        /// </summary>
        public Uri SourceMapUri { get; set; }

        /// <summary>
        /// Gets or sets optional document attributes.
        /// </summary>
        public DocumentFlags? Flags { get; set; }

        internal string UniqueName { get; set; }
    }
}
