// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a document loader.
    /// </summary>
    public abstract class DocumentLoader
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="DocumentLoader"/></c> instance.
        /// </summary>
        protected DocumentLoader()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Gets the default document loader.
        /// </summary>
        public static DocumentLoader Default => DefaultImpl.Instance;

        /// <summary>
        /// Gets or sets the maximum size of the document loader's cache.
        /// </summary>
        /// <remarks>
        /// This property specifies the maximum number of documents to be cached by the document
        /// loader. For the default document loader, its initial value is 1024.
        /// </remarks>
        /// <c><seealso cref="Default"/></c>
        public virtual uint MaxCacheSize
        {
            get => 0;
            set => throw new NotSupportedException("The document loader does not support caching");
        }

        /// <summary>
        /// Loads a document.
        /// </summary>
        /// <param name="settings">Document access settings for the operation.</param>
        /// <param name="sourceInfo">An optional structure containing meta-information for the requesting document.</param>
        /// <param name="specifier">A string specifying the document to be loaded.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <returns>A <c><see cref="Document"/></c> instance that represents the loaded document.</returns>
        /// <remarks>
        /// A loaded document must have an absolute <see cref="DocumentInfo.Uri">URI</see>. Once a
        /// load operation has completed successfully, subsequent requests that resolve to the same
        /// URI are expected to return the same <c><see cref="Document"/></c> reference, although loaders
        /// are not required to manage document caches of unlimited size.
        /// </remarks>
        public virtual Document LoadDocument(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");

            try
            {
                return LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback).Result;
            }
            catch (AggregateException exception)
            {
                exception = exception.Flatten();
                if (exception.InnerExceptions.Count == 1)
                {
                    throw new FileLoadException(null, specifier, exception.InnerExceptions[0]);
                }

                throw new FileLoadException(null, specifier, exception);
            }
        }

        /// <summary>
        /// Loads a document asynchronously.
        /// </summary>
        /// <param name="settings">Document access settings for the operation.</param>
        /// <param name="sourceInfo">An optional structure containing meta-information for the requesting document.</param>
        /// <param name="specifier">A string specifying the document to be loaded.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <returns>A task that represents the asynchronous operation. Upon completion, the task's result is a <c><see cref="Document"/></c> instance that represents the loaded document.</returns>
        /// <remarks>
        /// A loaded document must have an absolute <see cref="DocumentInfo.Uri">URI</see>. Once a
        /// load operation has completed successfully, subsequent requests that resolve to the same
        /// URI are expected to return the same <c><see cref="Document"/></c> reference, although loaders
        /// are not required to manage document caches of unlimited size.
        /// </remarks>
        public abstract Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback);

        /// <summary>
        /// Searches for a cached document by <see cref="DocumentInfo.Uri">URI</see>.
        /// </summary>
        /// <param name="uri">The document URI for which to search.</param>
        /// <returns>The cached document if it was found, <c>null</c> otherwise.</returns>
        public virtual Document GetCachedDocument(Uri uri)
        {
            return null;
        }

        /// <summary>
        /// Stores a document in the cache.
        /// </summary>
        /// <param name="document">The document to store in the cache.</param>
        /// <param name="replace"><c>True</c> to replace any existing document with the same URI, <c>false</c> otherwise.</param>
        /// <returns>The cached document, which may be different from <paramref name="document"/> if <paramref name="replace"/> is <c>false</c>.</returns>
        /// <remarks>
        /// A cached document must have an absolute <see cref="DocumentInfo.Uri">URI</see>.
        /// </remarks>
        public virtual Document CacheDocument(Document document, bool replace)
        {
            throw new NotSupportedException("The document loader does not support caching");
        }

        /// <summary>
        /// Discards all cached documents.
        /// </summary>
        public virtual void DiscardCachedDocuments()
        {
        }

        #region Nested type: IStatistics

        internal interface IStatistics
        {
            long FileCheckCount { get; }
            long WebCheckCount { get; }
            void ResetCheckCounts();
        }

        #endregion

        #region Nested type: DefaultImpl

        // IMPORTANT: Before its implementation was factored out and made public, some hosts used
        // reflection to instantiate this class in order to maintain multiple document caches. It
        // should therefore be treated and retained as part of the public API, as well as a
        // placeholder for any future overrides of the default functionality.

        private sealed class DefaultImpl : DefaultDocumentLoader
        {
            public static readonly DefaultImpl Instance = new();

            private DefaultImpl()
            {
            }
        }

        #endregion
    }
}
