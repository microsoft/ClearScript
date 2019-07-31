// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Contains properties that control document access.
    /// </summary>
    /// <remarks>
    /// Additional properties can be defined in derived classes to accommodate custom document accessors.
    /// </remarks>
    public class DocumentSettings
    {
        private DocumentLoader loader;

        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <see cref="DocumentSettings"/> instance.
        /// </summary>
        public DocumentSettings()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Gets or sets a document loader.
        /// </summary>
        public DocumentLoader Loader
        {
            get { return loader ?? DocumentLoader.Default; }
            set { loader = value; }
        }

        /// <summary>
        /// Gets or sets document access options.
        /// </summary>
        public DocumentAccessFlags AccessFlags { get; set; }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of directory URLs or paths to search for documents.
        /// </summary>
        public string SearchPath { get; set; }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of supported file name extensions.
        /// </summary>
        public string FileNameExtensions { get; set; }

        /// <summary>
        /// Gets or set an optional method to be called when a document is loaded.
        /// </summary>
        public DocumentLoadCallback LoadCallback { get; set; }

        /// <summary>
        /// Gets or sets an optional document context callback.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property is used as an alternative to <see cref="DocumentInfo.ContextCallback"/>.
        /// If specified, the callback is invoked the first time a module attempts to retrieve its
        /// context information. The properties it returns are made available to the module
        /// implementation. This mechanism can be used to expose host resources selectively,
        /// securely, and without polluting the script engine's global namespace.
        /// </para>
        /// <para>
        /// Use 
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import.meta">import.meta</see></c>
        /// to access the context information of a <see cref="ModuleCategory.Standard"/> JavaScript
        /// module. In a <see cref="ModuleCategory.CommonJS"/> module, use <c>module.meta</c>.
        /// </para>
        /// </remarks>
        public DocumentContextCallback ContextCallback { get; set; }
    }
}
