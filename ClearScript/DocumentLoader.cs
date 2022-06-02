// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

        private sealed class DefaultImpl : DocumentLoader, IStatistics
        {
            public static readonly DefaultImpl Instance = new DefaultImpl();

            private static readonly IReadOnlyCollection<string> relativePrefixes = new List<string>
            {
                "." + Path.DirectorySeparatorChar,
                "." + Path.AltDirectorySeparatorChar,
                ".." + Path.DirectorySeparatorChar,
                ".." + Path.AltDirectorySeparatorChar,
            };

            private readonly List<Document> cache = new List<Document>();
            private long fileCheckCount;
            private long webCheckCount;

            private DefaultImpl()
            {
                MaxCacheSize = 1024;
            }

            private Task<(Document, List<Uri>)> GetCachedDocumentOrCandidateUrisAsync(DocumentSettings settings, DocumentInfo? sourceInfo, Uri uri)
            {
                return GetCachedDocumentOrCandidateUrisWorkerAsync(settings, sourceInfo, uri.ToEnumerable());
            }

            private Task<(Document, List<Uri>)> GetCachedDocumentOrCandidateUrisAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier)
            {
                return GetCachedDocumentOrCandidateUrisWorkerAsync(settings, sourceInfo, GetRawUris(settings, sourceInfo, specifier).Distinct());
            }

            private async Task<(Document, List<Uri>)> GetCachedDocumentOrCandidateUrisWorkerAsync(DocumentSettings settings, DocumentInfo? sourceInfo, IEnumerable<Uri> rawUris)
            {
                if (!string.IsNullOrWhiteSpace(settings.FileNameExtensions))
                {
                    rawUris = rawUris.SelectMany(uri => ApplyExtensions(sourceInfo, uri, settings.FileNameExtensions));
                }

                var testUris = rawUris.ToList();

                foreach (var testUri in testUris)
                {
                    var flag = testUri.IsFile ? DocumentAccessFlags.EnableFileLoading : DocumentAccessFlags.EnableWebLoading;
                    if (settings.AccessFlags.HasFlag(flag))
                    {
                        var document = GetCachedDocument(testUri);
                        if (document != null)
                        {
                            return (document, null);
                        }
                    }
                }

                var candidateUris = new List<Uri>();

                foreach (var testUri in testUris)
                {
                    if (await IsCandidateUriAsync(settings, testUri).ConfigureAwait(false))
                    {
                        candidateUris.Add(testUri);
                    }
                }

                return (null, candidateUris);
            }

            private static IEnumerable<Uri> GetRawUris(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier)
            {
                Uri baseUri;
                Uri uri;

                if (sourceInfo.HasValue && SpecifierMayBeRelative(settings, specifier))
                {
                    baseUri = GetBaseUri(sourceInfo.Value);
                    if ((baseUri != null) && Uri.TryCreate(baseUri, specifier, out uri))
                    {
                        yield return uri;
                    }
                }

                var searchPath = settings.SearchPath;
                if (!string.IsNullOrWhiteSpace(searchPath))
                {
                    foreach (var url in searchPath.SplitSearchPath())
                    {
                        if (Uri.TryCreate(url, UriKind.Absolute, out baseUri) && TryCombineSearchUri(baseUri, specifier, out uri))
                        {
                            yield return uri;
                        }
                    }
                }

                if (MiscHelpers.Try(out var path, () => Path.Combine(Directory.GetCurrentDirectory(), specifier)) && Uri.TryCreate(path, UriKind.Absolute, out uri))
                {
                    yield return uri;
                }

                if (MiscHelpers.Try(out path, () => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, specifier)) && Uri.TryCreate(path, UriKind.Absolute, out uri))
                {
                    yield return uri;
                }

                using (var process = Process.GetCurrentProcess())
                {
                    var module = process.MainModule;
                    if ((module != null) && Uri.TryCreate(module.FileName, UriKind.Absolute, out baseUri) && Uri.TryCreate(baseUri, specifier, out uri))
                    {
                        yield return uri;
                    }
                }
            }

            private static IEnumerable<Uri> ApplyExtensions(DocumentInfo? sourceInfo, Uri uri, string extensions)
            {
                yield return uri;

                var builder = new UriBuilder(uri);
                var path = builder.Path;

                if (!string.IsNullOrEmpty(Path.GetFileName(path)))
                {
                    var existingExtension = Path.GetExtension(path);
                    var compatibleExtensions = GetCompatibleExtensions(sourceInfo, extensions).ToList();

                    if (!compatibleExtensions.Contains(existingExtension, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (var compatibleExtension in compatibleExtensions)
                        {
                            builder.Path = Path.ChangeExtension(path, existingExtension + compatibleExtension);
                            yield return builder.Uri;
                        }
                    }
                }
            }

            private static IEnumerable<string> GetCompatibleExtensions(DocumentInfo? sourceInfo, string extensions)
            {
                string sourceExtension = null;

                if (sourceInfo.HasValue)
                {
                    sourceExtension = Path.GetExtension((sourceInfo.Value.Uri != null) ? new UriBuilder(sourceInfo.Value.Uri).Path : sourceInfo.Value.Name);
                    if (!string.IsNullOrEmpty(sourceExtension))
                    {
                        yield return sourceExtension;
                    }
                }

                foreach (var extension in extensions.SplitSearchPath())
                {
                    var tempExtension = extension.StartsWith(".", StringComparison.Ordinal) ? extension : "." + extension;
                    if (!tempExtension.Equals(sourceExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return tempExtension;
                    }
                }
            }

            private static bool SpecifierMayBeRelative(DocumentSettings settings, string specifier)
            {
                return !settings.AccessFlags.HasFlag(DocumentAccessFlags.EnforceRelativePrefix) || relativePrefixes.Any(specifier.StartsWith);
            }

            private static Uri GetBaseUri(DocumentInfo sourceInfo)
            {
                var sourceUri = sourceInfo.Uri;

                if ((sourceUri == null) && !Uri.TryCreate(sourceInfo.Name, UriKind.RelativeOrAbsolute, out sourceUri))
                {
                    return null;
                }

                if (!sourceUri.IsAbsoluteUri)
                {
                    return null;
                }

                return sourceUri;
            }

            private static bool TryCombineSearchUri(Uri searchUri, string specifier, out Uri uri)
            {
                var searchUrl = searchUri.AbsoluteUri;
                if (!searchUrl.EndsWith("/", StringComparison.Ordinal))
                {
                    searchUri = new Uri(searchUrl + "/");
                }

                return Uri.TryCreate(searchUri, specifier, out uri);
            }

            private async Task<bool> IsCandidateUriAsync(DocumentSettings settings, Uri uri)
            {
                return uri.IsFile ?
                    settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableFileLoading) && await FileDocumentExistsAsync(uri.LocalPath).ConfigureAwait(false) :
                    settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableWebLoading) && await WebDocumentExistsAsync(uri).ConfigureAwait(false);
            }

            private Task<bool> FileDocumentExistsAsync(string path)
            {
                Interlocked.Increment(ref fileCheckCount);
                return Task.FromResult(File.Exists(path));
            }

            private async Task<bool> WebDocumentExistsAsync(Uri uri)
            {
                Interlocked.Increment(ref webCheckCount);
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
                    {
                        try
                        {
                            using (var response = await client.SendAsync(request).ConfigureAwait(false))
                            {
                                return response.IsSuccessStatusCode;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            return false;
                        }
                    }
                }
            }

            private async Task<Document> LoadDocumentAsync(DocumentSettings settings, Uri uri, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                if (uri.IsFile)
                {
                    if (!settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableFileLoading))
                    {
                        throw new UnauthorizedAccessException("The script engine is not configured for loading documents from the file system");
                    }
                }
                else
                {
                    if (!settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableWebLoading))
                    {
                        throw new UnauthorizedAccessException("The script engine is not configured for downloading documents from the Web");
                    }
                }

                var cachedDocument = GetCachedDocument(uri);
                if (cachedDocument != null)
                {
                    return cachedDocument;
                }

                string contents;

                if (uri.IsFile)
                {
                    using (var reader = new StreamReader(uri.LocalPath))
                    {
                        contents = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    using (var client = new HttpClient())
                    {
                        contents = await client.GetStringAsync(uri).ConfigureAwait(false);
                    }
                }

                var documentInfo = new DocumentInfo(uri) { Category = category, ContextCallback = contextCallback };

                var callback = settings.LoadCallback;
                callback?.Invoke(ref documentInfo);

                return CacheDocument(new StringDocument(documentInfo, contents), false);
            }

            #region DocumentLoader overrides

            public override uint MaxCacheSize { get; set; }

            public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                MiscHelpers.VerifyNonNullArgument(settings, nameof(settings));
                MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");

                if ((settings.AccessFlags & DocumentAccessFlags.EnableAllLoading) == DocumentAccessFlags.None)
                {
                    throw new UnauthorizedAccessException("The script engine is not configured for loading documents");
                }

                if (category == null)
                {
                    category = sourceInfo.HasValue ? sourceInfo.Value.Category : DocumentCategory.Script;
                }

                (Document Document, List<Uri> CandidateUris) result;

                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out var uri) && uri.IsAbsoluteUri)
                {
                    result = await GetCachedDocumentOrCandidateUrisAsync(settings, sourceInfo, uri).ConfigureAwait(false);
                }
                else
                {
                    result = await GetCachedDocumentOrCandidateUrisAsync(settings, sourceInfo, specifier).ConfigureAwait(false);
                }

                if (result.Document != null)
                {
                    return result.Document;
                }

                if (result.CandidateUris.Count < 1)
                {
                    throw new FileNotFoundException(null, specifier);
                }

                if (result.CandidateUris.Count == 1)
                {
                    return await LoadDocumentAsync(settings, result.CandidateUris[0], category, contextCallback).ConfigureAwait(false);
                }

                var exceptions = new List<Exception>(result.CandidateUris.Count);

                foreach (var candidateUri in result.CandidateUris)
                {
                    var task = LoadDocumentAsync(settings, candidateUri, category, contextCallback);
                    try
                    {
                        return await task.ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        if ((task.Exception != null) && task.Exception.InnerExceptions.Count == 1)
                        {
                            Debug.Assert(ReferenceEquals(task.Exception.InnerExceptions[0], exception));
                            exceptions.Add(exception);
                        }
                        else
                        {
                            exceptions.Add(task.Exception);
                        }
                    }
                }

                if (exceptions.Count < 1)
                {
                    MiscHelpers.AssertUnreachable();
                    throw new FileNotFoundException(null, specifier);
                }

                if (exceptions.Count == 1)
                {
                    MiscHelpers.AssertUnreachable();
                    throw new FileLoadException(exceptions[0].Message, specifier, exceptions[0]);
                }

                throw new AggregateException(exceptions).Flatten();
            }

            public override Document GetCachedDocument(Uri uri)
            {
                lock (cache)
                {
                    for (var index = 0; index < cache.Count; index++)
                    {
                        var cachedDocument = cache[index];
                        if (cachedDocument.Info.Uri == uri)
                        {
                            cache.RemoveAt(index);
                            cache.Insert(0, cachedDocument);
                            return cachedDocument;
                        }
                    }

                    return null;
                }
            }

            public override Document CacheDocument(Document document, bool replace)
            {
                MiscHelpers.VerifyNonNullArgument(document, nameof(document));
                if (!document.Info.Uri.IsAbsoluteUri)
                {
                    throw new ArgumentException("The document must have an absolute URI");
                }

                lock (cache)
                {
                    for (var index = 0; index < cache.Count;)
                    {
                        var cachedDocument = cache[index];
                        if (cachedDocument.Info.Uri != document.Info.Uri)
                        {
                            index++;
                        }
                        else
                        {
                            if (!replace)
                            {
                                Debug.Assert(cachedDocument.Contents.ReadToEnd().SequenceEqual(document.Contents.ReadToEnd()));
                                return cachedDocument;
                            }

                            cache.RemoveAt(index);
                        }
                    }

                    var maxCacheSize = Math.Max(16, Convert.ToInt32(Math.Min(MaxCacheSize, int.MaxValue)));
                    while (cache.Count >= maxCacheSize)
                    {
                        cache.RemoveAt(cache.Count - 1);
                    }

                    cache.Insert(0, document);
                    return document;
                }
            }

            public override void DiscardCachedDocuments()
            {
                lock (cache)
                {
                    cache.Clear();
                }
            }

            #endregion

            #region IStatistics implementation

            long IStatistics.FileCheckCount => Interlocked.Read(ref fileCheckCount);

            long IStatistics.WebCheckCount => Interlocked.Read(ref webCheckCount);

            void IStatistics.ResetCheckCounts()
            {
                Interlocked.Exchange(ref fileCheckCount, 0);
                Interlocked.Exchange(ref webCheckCount, 0);
            }

            #endregion
        }

        #endregion
    }
}
