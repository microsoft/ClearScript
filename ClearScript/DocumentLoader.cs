// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        /// Initializes a new <see cref="DocumentLoader"/> instance.
        /// </summary>
        protected DocumentLoader()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Gets the default document loader.
        /// </summary>
        public static DocumentLoader Default
        {
            get { return DefaultImpl.Instance; }
        }

        /// <summary>
        /// Loads a document.
        /// </summary>
        /// <param name="settings">Document access settings for the operation.</param>
        /// <param name="sourceInfo">An optional structure containing meta-information for the requesting document.</param>
        /// <param name="specifier">A string specifying the document to be loaded.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <returns>A <see cref="Document"/> instance that represents the loaded document.</returns>
        /// <remarks>
        /// A loaded document must have an absolute <see cref="DocumentInfo.Uri">URI</see>. Once a
        /// load operation has completed successfully, subsequent requests that resolve to the same
        /// URI are expected to return the same <see cref="Document"/> reference, although loaders
        /// are not required to manage document caches of unlimited size.
        /// </remarks>
        public virtual Document LoadDocument(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, "specifier", "Invalid document specifier");

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
        /// <returns>A task that represents the asynchronous operation. Upon completion, the task's result is a <see cref="Document"/> instance that represents the loaded document.</returns>
        /// <remarks>
        /// A loaded document must have an absolute <see cref="DocumentInfo.Uri">URI</see>. Once a
        /// load operation has completed successfully, subsequent requests that resolve to the same
        /// URI are expected to return the same <see cref="Document"/> reference, although loaders
        /// are not required to manage document caches of unlimited size.
        /// </remarks>
        public abstract Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback);

        /// <summary>
        /// Discards all cached documents.
        /// </summary>
        public virtual void DiscardCachedDocuments()
        {
        }

        #region Nested type: DefaultImpl

        private class DefaultImpl : DocumentLoader
        {
            public static readonly DefaultImpl Instance = new DefaultImpl();

            private static readonly IReadOnlyCollection<string> relativePrefixes = new List<string>
            {
                "." + Path.DirectorySeparatorChar,
                "." + Path.AltDirectorySeparatorChar,
                ".." + Path.DirectorySeparatorChar,
                ".." + Path.AltDirectorySeparatorChar,
            };

            private readonly object cacheLock = new object();
            private readonly List<Document> cache = new List<Document>();
            private const int maxCacheSize = 1024;

            private DefaultImpl()
            {
            }

            private static async Task<List<Uri>> GetCandidateUrisAsync(DocumentSettings settings, DocumentInfo? sourceInfo, Uri uri)
            {
                var candidateUris = new List<Uri>();

                if (string.IsNullOrWhiteSpace(settings.FileNameExtensions))
                {
                    candidateUris.Add(uri);
                }
                else
                {
                    foreach (var testUri in ApplyFileNameExtensions(sourceInfo, uri, settings.FileNameExtensions))
                    {
                        if (await IsCandidateUriAsync(settings, testUri).ConfigureAwait(false))
                        {
                            candidateUris.Add(testUri);
                        }
                    }
                }

                return candidateUris;
            }

            private static async Task<List<Uri>> GetCandidateUrisAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier)
            {
                var candidateUris = new List<Uri>();

                var rawUris = GetRawUris(settings, sourceInfo, specifier).Distinct();
                if (!string.IsNullOrWhiteSpace(settings.FileNameExtensions))
                {
                    rawUris = rawUris.SelectMany(uri => ApplyFileNameExtensions(sourceInfo, uri, settings.FileNameExtensions));
                }

                foreach (var testUri in rawUris)
                {
                    if (await IsCandidateUriAsync(settings, testUri).ConfigureAwait(false))
                    {
                        candidateUris.Add(testUri);
                    }
                }

                return candidateUris;
            }

            private static IEnumerable<Uri> GetRawUris(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier)
            {
                Uri baseUri;
                Uri uri;
                string path;

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

                if (MiscHelpers.Try(out path, () => Path.Combine(Directory.GetCurrentDirectory(), specifier)) && Uri.TryCreate(path, UriKind.Absolute, out uri))
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

            private static IEnumerable<Uri> ApplyFileNameExtensions(DocumentInfo? sourceInfo, Uri uri, string fileNameExtensions)
            {
                yield return uri;

                var builder = new UriBuilder(uri);

                var path = builder.Path;
                if (!string.IsNullOrEmpty(Path.GetFileName(path)) && !Path.HasExtension(path))
                {
                    string sourceFileNameExtension = null;
                    if (sourceInfo.HasValue)
                    {
                        sourceFileNameExtension = Path.GetExtension((sourceInfo.Value.Uri != null) ? new UriBuilder(sourceInfo.Value.Uri).Path : sourceInfo.Value.Name);
                        if (!string.IsNullOrEmpty(sourceFileNameExtension))
                        {
                            builder.Path = Path.ChangeExtension(path, sourceFileNameExtension);
                            yield return builder.Uri;
                        }
                    }

                    foreach (var fileNameExtension in fileNameExtensions.SplitSearchPath())
                    {
                        var testFileNameExtension = fileNameExtension.StartsWith(".", StringComparison.Ordinal) ? fileNameExtension : "." + fileNameExtension;
                        if (!testFileNameExtension.Equals(sourceFileNameExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            builder.Path = Path.ChangeExtension(path, testFileNameExtension);
                            yield return builder.Uri;
                        }
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

            private static async Task<bool> IsCandidateUriAsync(DocumentSettings settings, Uri uri)
            {
                return uri.IsFile ?
                    settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableFileLoading) && File.Exists(uri.LocalPath) :
                    settings.AccessFlags.HasFlag(DocumentAccessFlags.EnableWebLoading) && await WebDocumentExistsAsync(uri).ConfigureAwait(false);
            }

            private static async Task<bool> WebDocumentExistsAsync(Uri uri)
            {
                using (var client = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
                    {
                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            return response.IsSuccessStatusCode;
                        }
                    }
                }
            }

            private async Task<Document> LoadDocumentAsync(DocumentSettings settings, Uri uri, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                var cachedDocument = GetCachedDocument(uri);
                if (cachedDocument != null)
                {
                    return cachedDocument;
                }

                string contents;
                var flags = settings.AccessFlags;

                if (uri.IsFile)
                {
                    if (!flags.HasFlag(DocumentAccessFlags.EnableFileLoading))
                    {
                        throw new UnauthorizedAccessException("This script engine is not configured for loading documents from the file system.");
                    }

                    using (var reader = new StreamReader(uri.LocalPath))
                    {
                        contents = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (!flags.HasFlag(DocumentAccessFlags.EnableWebLoading))
                    {
                        throw new UnauthorizedAccessException("This script engine is not configured for downloading documents from the Web.");
                    }

                    using (var client = new WebClient())
                    {
                        contents = await client.DownloadStringTaskAsync(uri).ConfigureAwait(false);
                    }
                }

                var documentInfo = new DocumentInfo(uri) { Category = category, ContextCallback = contextCallback };

                var callback = settings.LoadCallback;
                if (callback != null)
                {
                    callback(ref documentInfo);
                }

                return CacheDocument(new StringDocument(documentInfo, contents));
            }

            private Document GetCachedDocument(Uri uri)
            {
                lock (cacheLock)
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

            private Document CacheDocument(Document document)
            {
                lock (cacheLock)
                {
                    var cachedDocument = cache.FirstOrDefault(testDocument => testDocument.Info.Uri == document.Info.Uri);
                    if (cachedDocument != null)
                    {
                        Debug.Assert(cachedDocument.Contents.ReadToEnd().SequenceEqual(document.Contents.ReadToEnd()));
                        return cachedDocument;
                    }

                    while (cache.Count >= maxCacheSize)
                    {
                        cache.RemoveAt(cache.Count - 1);
                    }

                    cache.Insert(0, document);
                    return document;
                }
            }

            #region DocumentLoader overrides

            public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                MiscHelpers.VerifyNonBlankArgument(specifier, "specifier", "Invalid document specifier");

                if ((settings.AccessFlags & DocumentAccessFlags.EnableAllLoading) == DocumentAccessFlags.None)
                {
                    throw new UnauthorizedAccessException("This script engine is not configured for loading documents.");
                }

                if (category == null)
                {
                    category = sourceInfo.HasValue ? sourceInfo.Value.Category : DocumentCategory.Script;
                }

                List<Uri> candidateUris;

                Uri uri;
                if (Uri.TryCreate(specifier, UriKind.RelativeOrAbsolute, out uri) && uri.IsAbsoluteUri)
                {
                    candidateUris = await GetCandidateUrisAsync(settings, sourceInfo, uri).ConfigureAwait(false);
                }
                else
                {
                    candidateUris = await GetCandidateUrisAsync(settings, sourceInfo, specifier).ConfigureAwait(false);
                }

                if (candidateUris.Count < 1)
                {
                    throw new FileNotFoundException(null, specifier);
                }

                if (candidateUris.Count == 1)
                {
                    return await LoadDocumentAsync(settings, candidateUris[0], category, contextCallback).ConfigureAwait(false);
                }

                var exceptions = new List<Exception>(candidateUris.Count);

                foreach (var candidateUri in candidateUris)
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

            public override void DiscardCachedDocuments()
            {
                lock (cacheLock)
                {
                    cache.Clear();
                }

                base.DiscardCachedDocuments();
            }

            #endregion
        }

        #endregion
    }
}
