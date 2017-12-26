// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Util.Web
{
    internal sealed class WebContext
    {
        public WebRequest Request { get; private set; }

        public WebResponse Response { get; private set; }

        private WebContext(Socket socket, Uri uri, NameValueCollection headers)
        {
            Request = new WebRequest(uri, headers);
            Response = new WebResponse(socket, 200);
        }

        public static async Task<WebContext> CreateAsync(Socket socket)
        {
            try
            {
                var lines = new List<string>();

                while (true)
                {
                    var line = await socket.ReceiveLineAsync().ConfigureAwait(false);
                    if (line.Length < 1)
                    {
                        break;
                    }

                    lines.Add(line);
                }

                if (lines.Count < 1)
                {
                    throw new InvalidDataException("HTTP request line not found");
                }

                var parts = lines[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    throw new InvalidDataException("Malformed HTTP request line");
                }

                var method = parts[0].Trim().ToUpperInvariant();
                if (!WebRequest.Methods.Contains(method))
                {
                    throw new InvalidDataException("Unrecognized HTTP method");
                }

                var requestUrl = parts[1].Trim();
                if (string.IsNullOrEmpty(requestUrl))
                {
                    throw new InvalidDataException("Invalid HTTP request URI");
                }

                var headers = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
                for (var index = 1; index < lines.Count; ++index)
                {
                    var line = lines[index];

                    var pos = line.IndexOf(':');
                    if (pos < 0)
                    {
                        throw new InvalidDataException("Malformed HTTP header line");
                    }

                    var name = line.Substring(0, pos).Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidDataException("Malformed HTTP header line");
                    }

                    var value = line.Substring(pos + 1).Trim();
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new InvalidDataException("Malformed HTTP header line");
                    }

                    headers[name] = value;
                }

                string hostName = null;
                var port = -1;

                var hostHeader = headers.Get("Host");
                if (!string.IsNullOrEmpty(hostHeader))
                {
                    var pos = hostHeader.IndexOf(':');
                    if (pos < 0)
                    {
                        hostName = hostHeader.Trim();
                    }
                    else
                    {
                        hostName = hostHeader.Substring(0, pos).Trim();

                        int tempPort;
                        if (int.TryParse(hostHeader.Substring(pos + 1), out tempPort))
                        {
                            port = tempPort;
                        }
                    }
                }

                if (string.IsNullOrEmpty(hostName))
                {
                    hostName = Dns.GetHostName();
                }

                if (port < 1)
                {
                    port = ((IPEndPoint)socket.LocalEndPoint).Port;
                }

                var uri = new Uri("http://" + hostName + ":" + port + "/");
                if (requestUrl != "*")
                {
                    uri = new Uri(uri, requestUrl);
                }

                return new WebContext(socket, uri, headers);
            }
            catch (Exception)
            {
                Abort(socket, 400);
                throw;
            }
        }

        public async Task<WebSocket> AcceptWebSocketAsync()
        {
            if (!Request.IsWebSocketRequest)
            {
                throw new InvalidOperationException("The request is not a WebSocket handshake");
            }

            return await Response.AcceptWebSocketAsync(Request.Headers["Sec-WebSocket-Key"].Trim()).ConfigureAwait(false);
        }

        private static void Abort(Socket socket, int statusCode)
        {
            using (new WebResponse(socket, statusCode))
            {
            }
        }
    }
}
