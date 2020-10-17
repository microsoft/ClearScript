// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    internal class UniqueDocumentInfo
    {
        public UniqueDocumentInfo(DocumentInfo info, ulong uniqueId, string uniqueName)
        {
            Info = info;
            UniqueId = uniqueId;
            UniqueName = uniqueName;
        }

        public DocumentInfo Info { get; }

        public string Name => Info.Name;

        public Uri Uri => Info.Uri;

        public Uri SourceMapUri => Info.SourceMapUri;

        public DocumentCategory Category => Info.Category;

        public DocumentFlags? Flags => Info.Flags;

        public DocumentContextCallback ContextCallback => Info.ContextCallback;

        public ulong UniqueId { get; }

        public string UniqueName { get; }
    }
}
