// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Threading;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Contains meta-information for a document.
    /// </summary>
    public struct DocumentInfo
    {
        private static long lastUniqueId;

        private readonly string name;
        private readonly Uri uri;
        private DocumentCategory category;
        private ulong uniqueId;

        /// <summary>
        /// Initializes a new <see cref="DocumentInfo"/> structure with the specified document name.
        /// </summary>
        /// <param name="name">The document name.</param>
        public DocumentInfo(string name)
            : this()
        {
            this.name = name;
            uniqueId = Interlocked.Increment(ref lastUniqueId).ToUnsigned();
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
            uniqueId = Interlocked.Increment(ref lastUniqueId).ToUnsigned();
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
            get { return MiscHelpers.EnsureNonBlank(name, Category.DefaultName); }
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
        /// Gets or sets an optional source map URI for the document.
        /// </summary>
        public Uri SourceMapUri { get; set; }

        /// <summary>
        /// Gets or sets the document's category.
        /// </summary>
        public DocumentCategory Category
        {
            get { return category ?? DocumentCategory.Script; }
            set { category = value; }
        }

        /// <summary>
        /// Gets or sets optional document attributes.
        /// </summary>
        public DocumentFlags? Flags { get; set; }

        /// <summary>
        /// Gets or sets an optional context callback for the document.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property currently applies only to modules. If specified, the callback is invoked
        /// the first time the module attempts to retrieve its context information. The properties
        /// it returns are made available to the module implementation. This mechanism can be used
        /// to expose host resources selectively, securely, and without polluting the script
        /// engine's global namespace.
        /// </para>
        /// <para>
        /// Use 
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import.meta">import.meta</see></c>
        /// to access the context information of a <see cref="ModuleCategory.Standard"/> JavaScript
        /// module. In a <see cref="ModuleCategory.CommonJS"/> module, use <c>module.meta</c>.
        /// </para>
        /// </remarks>
        public DocumentContextCallback ContextCallback { get; set; }

        internal UniqueDocumentInfo MakeUnique(ScriptEngine engine)
        {
            return MakeUnique(engine.DocumentNameManager);
        }

        internal UniqueDocumentInfo MakeUnique(ScriptEngine engine, DocumentFlags? defaultFlags)
        {
            return MakeUnique(engine.DocumentNameManager, defaultFlags);
        }

        internal UniqueDocumentInfo MakeUnique(IUniqueNameManager manager)
        {
            return MakeUnique(manager, null);
        }

        internal UniqueDocumentInfo MakeUnique(IUniqueNameManager manager, DocumentFlags? defaultFlags)
        {
            var info = this;
            if (!info.Flags.HasValue && defaultFlags.HasValue)
            {
                info.Flags = defaultFlags;
            }

            if (uniqueId < 1)
            {
                uniqueId = Interlocked.Increment(ref lastUniqueId).ToUnsigned();
            }

            var uniqueName = manager.GetUniqueName(Name, Category.DefaultName);
            if (Flags.GetValueOrDefault().HasFlag(DocumentFlags.IsTransient))
            {
                uniqueName += " [temp]";
            }

            return new UniqueDocumentInfo(info, uniqueId, uniqueName);
        }
    }
}
