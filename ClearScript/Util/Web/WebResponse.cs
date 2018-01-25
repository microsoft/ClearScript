// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.ClearScript.Util.Web
{
    internal sealed class WebResponse : IDisposable
    {
        private readonly Socket socket;
        private int state;

        public int StatusCode { get; set; }

        public string ContentType { get; set; }

        public Stream OutputStream { get; private set; }

        internal WebResponse(Socket socket, int statusCode)
        {
            this.socket = socket;
            state = State.Open;

            StatusCode = statusCode;
            OutputStream = new MemoryStream();
        }

        internal async Task<WebSocket> AcceptWebSocketAsync(string key)
        {
            if (Interlocked.CompareExchange(ref state, State.Upgraded, State.Open) == State.Open)
            {
                using (var stream = CreateWebSocketResponseStream(key))
                {
                    await socket.SendBytesAsync(stream.GetBuffer(), 0, Convert.ToInt32(stream.Length)).ConfigureAwait(false);
                }

                return new WebSocket(socket, true);
            }

            throw new InvalidOperationException("Cannot accept a WebSocket connection in the current state");
        }

        public void Close(int? overrideStatusCode = null)
        {
            if (Interlocked.CompareExchange(ref state, State.Closed, State.Open) == State.Open)
            {
                CloseAsync(overrideStatusCode).ContinueWith(task => MiscHelpers.Try(task.Wait));
            }
        }

        private async Task CloseAsync(int? overrideStatusCode)
        {
            using (socket)
            {
                using (var stream = CreateResponseStream(overrideStatusCode))
                {
                    await socket.SendBytesAsync(stream.GetBuffer(), 0, Convert.ToInt32(stream.Length)).ConfigureAwait(false);
                }
            }
        }

        private MemoryStream CreateResponseStream(int? overrideStatusCode)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.ASCII, 16 * 1024, true))
            {
                var statusCode = overrideStatusCode ?? StatusCode;
                writer.Write("HTTP/1.1 {0} {1}\r\n", statusCode, HttpWorkerRequest.GetStatusDescription(statusCode));

                if (!string.IsNullOrWhiteSpace(ContentType))
                {
                    writer.Write("Content-Type: {0}\r\n", ContentType);
                }

                if (OutputStream.Length > 0)
                {
                    writer.Write("Content-Length: {0}\r\n", OutputStream.Length);
                }

                writer.Write("Cache-Control: no-cache, no-store, must-revalidate\r\n");
                writer.Write("Connection: close\r\n");

                writer.Write("\r\n");
                writer.Flush();
            }

            if (OutputStream.Length > 0)
            {
                stream.Write(((MemoryStream)OutputStream).GetBuffer(), 0, Convert.ToInt32(OutputStream.Length));
            }

            return stream;
        }

        private static MemoryStream CreateWebSocketResponseStream(string key)
        {
            var stream = new MemoryStream();

            using (var writer = new StreamWriter(stream, Encoding.ASCII, 16 * 1024, true))
            {
                const int statusCode = 101;
                writer.Write("HTTP/1.1 {0} {1}\r\n", statusCode, HttpWorkerRequest.GetStatusDescription(statusCode));

                writer.Write("Connection: Upgrade\r\n");
                writer.Write("Upgrade: websocket\r\n");

                var acceptKey = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                writer.Write("Sec-WebSocket-Accept: {0}\r\n", acceptKey);

                writer.Write("\r\n");
                writer.Flush();
            }

            return stream;
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Nested type: State

        private static class State
        {
            public const int Open = 0;
            public const int Upgraded = 1;
            public const int Closed = 2;
        }

        #endregion
    }
}
