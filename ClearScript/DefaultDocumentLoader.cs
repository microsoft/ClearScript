// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides a default <c><see cref="DocumentLoader"></see></c> implementation.
    /// </summary>
    public class DefaultDocumentLoader : DocumentLoader, DocumentLoader.IStatistics
    {
        private static readonly IReadOnlyCollection<string> relativePrefixes = new List<string>
        {
            "." + Path.DirectorySeparatorChar,
            "." + Path.AltDirectorySeparatorChar,
            ".." + Path.DirectorySeparatorChar,
            ".." + Path.AltDirectorySeparatorChar,
        };

        private readonly List<Document> cache = new();
        private long fileCheckCount;
        private long webCheckCount;

        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="DefaultDocumentLoader"/></c> instance.
        /// </summary>
        public DefaultDocumentLoader()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

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
                if (settings.AccessFlags.HasAllFlags(flag))
                {
                    var document = GetCachedDocument(testUri);
                    if (document is not null)
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
                if ((baseUri is not null) && Uri.TryCreate(baseUri, specifier, out uri))
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

            if (MiscHelpers.Try(out var path, static specifier => Path.Combine(Directory.GetCurrentDirectory(), specifier), specifier) && Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                yield return uri;
            }

            if (MiscHelpers.Try(out path, static specifier => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, specifier), specifier) && Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                yield return uri;
            }

            using (var process = Process.GetCurrentProcess())
            {
                var module = process.MainModule;
                if ((module is not null) && Uri.TryCreate(module.FileName, UriKind.Absolute, out baseUri) && Uri.TryCreate(baseUri, specifier, out uri))
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
                sourceExtension = Path.GetExtension((sourceInfo.Value.Uri is not null) ? new UriBuilder(sourceInfo.Value.Uri).Path : sourceInfo.Value.Name);
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
            return !settings.AccessFlags.HasAllFlags(DocumentAccessFlags.EnforceRelativePrefix) || relativePrefixes.Any(specifier.StartsWith);
        }

        private static Uri GetBaseUri(DocumentInfo sourceInfo)
        {
            var sourceUri = sourceInfo.Uri;

            if ((sourceUri is null) && !Uri.TryCreate(sourceInfo.Name, UriKind.RelativeOrAbsolute, out sourceUri))
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
                settings.AccessFlags.HasAllFlags(DocumentAccessFlags.EnableFileLoading) && await FileDocumentExistsAsync(uri.LocalPath).ConfigureAwait(false) :
                settings.AccessFlags.HasAllFlags(DocumentAccessFlags.EnableWebLoading) && await WebDocumentExistsAsync(uri).ConfigureAwait(false);
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
                if (!settings.AccessFlags.HasAllFlags(DocumentAccessFlags.EnableFileLoading))
                {
                    throw new UnauthorizedAccessException("The script engine is not configured for loading documents from the file system");
                }
            }
            else
            {
                if (!settings.AccessFlags.HasAllFlags(DocumentAccessFlags.EnableWebLoading))
                {
                    throw new UnauthorizedAccessException("The script engine is not configured for downloading documents from the Web");
                }
            }

            var cachedDocument = GetCachedDocument(uri);
            if (cachedDocument is not null)
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

            if (!settings.AccessFlags.HasAllFlags(DocumentAccessFlags.UseAsyncLoadCallback))
            {
                var callback = settings.LoadCallback;
                callback?.Invoke(ref documentInfo);
            }
            else
            {
                var callback = settings.AsyncLoadCallback;
                if (callback is not null)
                {
                    var documentInfoRef = ValueRef.Create(documentInfo);
                    await callback(documentInfoRef, new MemoryStream(Encoding.UTF8.GetBytes(contents), false)).ConfigureAwait(false);
                    documentInfo = documentInfoRef.Value;
                }
            }

            var document = CacheDocument(new StringDocument(documentInfo, contents), false);

            var expectedCategory = category ?? DocumentCategory.Script;
            if (!settings.AccessFlags.HasAllFlags(DocumentAccessFlags.AllowCategoryMismatch) && (documentInfo.Category != expectedCategory))
            {
                throw new FileLoadException($"Document category mismatch: '{expectedCategory}' expected, '{documentInfo.Category}' loaded", uri.IsFile ? uri.LocalPath : uri.AbsoluteUri);
            }

            return document;
        }

        #region DocumentLoader overrides

        /// <inheritdoc/>
        public override uint MaxCacheSize { get; set; } = 1024;

        /// <inheritdoc/>
        public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonNullArgument(settings, nameof(settings));
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");

            if ((settings.AccessFlags & DocumentAccessFlags.EnableAllLoading) == DocumentAccessFlags.None)
            {
                throw new UnauthorizedAccessException("The script engine is not configured for loading documents");
            }

            if (category is null)
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

            if (result.Document is not null)
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
                    if ((task.Exception is not null) && task.Exception.InnerExceptions.Count == 1)
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override Document CacheDocument(Document document, bool replace)
        {
            MiscHelpers.VerifyNonNullArgument(document, nameof(document));
            if ((document.Info.Uri is null) || !document.Info.Uri.IsAbsoluteUri)
            {
                throw new ArgumentException("The document must have an absolute URI", nameof(document));
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

        /// <inheritdoc/>
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
}
