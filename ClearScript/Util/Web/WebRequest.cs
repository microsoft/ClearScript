// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Specialized;

namespace Microsoft.ClearScript.Util.Web
{
    internal sealed class WebRequest
    {
        internal static readonly string[] Methods = { "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE", "PATCH" };

        public bool IsWebSocketRequest { get; private set; }

        public Uri Uri { get; private set; }

        public string RawUrl { get; private set; }

        public NameValueCollection Headers { get; private set; }

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
