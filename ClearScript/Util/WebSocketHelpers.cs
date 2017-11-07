// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;

namespace Microsoft.ClearScript.Util
{
    internal static class WebSocketHelpers
    {
        private const int bufferSize = 16 * 1024;

        public static void ReceiveMessageAsync(this WebSocket webSocket, WebSocketMessageType type, Action<byte[], WebSocketCloseStatus, string> callback)
        {
            ReceiveMessageAsyncWorker(webSocket, type, MiscHelpers.GetEmptyArray<byte>(), callback);
        }

        public static void SendMessageAsync(this WebSocket webSocket, WebSocketMessageType type, byte[] bytes, Action<bool, WebSocketCloseStatus, string> callback)
        {
            webSocket.SendAsync(new ArraySegment<byte>(bytes), type, true, CancellationToken.None).ContinueWith(task =>
            {
                if (!MiscHelpers.Try(task.Wait))
                {
                    callback(false, WebSocketCloseStatus.ProtocolError, "Could not send data to web socket");
                }
                else
                {
                    callback(true, WebSocketCloseStatus.Empty, null);
                }
            });
        }

        private static void ReceiveMessageAsyncWorker(WebSocket webSocket, WebSocketMessageType type, byte[] bytes, Action<byte[], WebSocketCloseStatus, string> callback)
        {
            var buffer = new byte[bufferSize];
            webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ContinueWith(task =>
            {
                WebSocketReceiveResult result;
                if (!MiscHelpers.Try(out result, () => task.Result) || (result == null))
                {
                    callback(null, WebSocketCloseStatus.ProtocolError, "Could not receive data from web socket");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    callback(null, result.CloseStatus ?? WebSocketCloseStatus.Empty, MiscHelpers.EnsureNonBlank(result.CloseStatusDescription, "Could not determine web socket close status"));
                }
                else if (result.MessageType != type)
                {
                    callback(null, WebSocketCloseStatus.ProtocolError, "Received unrecognized data from web socket");
                }
                else
                {
                    bytes = bytes.Concat(buffer.Take(result.Count)).ToArray();
                    if (!result.EndOfMessage)
                    {
                        ReceiveMessageAsyncWorker(webSocket, type, bytes, callback);
                    }
                    else
                    {
                        callback(bytes, WebSocketCloseStatus.Empty, null);
                    }
                }
            });
        }
    }
}
