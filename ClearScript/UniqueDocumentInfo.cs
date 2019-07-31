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

        public DocumentInfo Info { get; private set; }

        public string Name
        {
            get { return Info.Name; }
        }

        public Uri Uri
        {
            get { return Info.Uri; }
        }

        public Uri SourceMapUri
        {
            get { return Info.SourceMapUri; }
        }

        public DocumentCategory Category
        {
            get { return Info.Category; }
        }

        public DocumentFlags? Flags
        {
            get { return Info.Flags; }
        }

        public DocumentContextCallback ContextCallback
        {
            get { return Info.ContextCallback; }
        }

        public ulong UniqueId { get; private set; }

        public string UniqueName { get; private set; }
    }
}
