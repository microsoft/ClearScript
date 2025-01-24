// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Specialized;

namespace Microsoft.ClearScript.Util.Web
{
    internal sealed class WebRequest
    {
        internal static readonly string[] Methods = { "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE", "PATCH" };

        public bool IsWebSocketRequest { get; }

        public Uri Uri { get; }

        public string RawUrl { get; }

        public NameValueCollection Headers { get; }

        internal WebRequest(Uri uri, NameValueCollection headers)
        {
            Uri = uri;
            RawUrl = Uri.PathAndQuery;
            Headers = headers;

            IsWebSocketRequest =
                string.Equals(headers["Connection"], "Upgrade", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(headers["Upgrade"], "WebSocket", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(headers["Sec-WebSocket-Key"]);
        }
    }
}
