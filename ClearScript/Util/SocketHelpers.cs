// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Util
{
    internal static class SocketHelpers
    {
        public static Task SendStringAsync(this Socket socket, string value, Encoding encoding = null)
        {
            return socket.SendBytesAsync((encoding ?? Encoding.UTF8).GetBytes(value));
        }

        public static async Task<string> ReceiveLineAsync(this Socket socket, Encoding encoding = null)
        {
            var lineBytes = new List<byte>(1024);
            var bytes = new byte[1];

            while (true)
            {
                await socket.ReceiveBytesAsync(bytes, 0, 1).ConfigureAwait(false);

                var lastIndex = lineBytes.Count - 1;
                if ((lastIndex >= 0) && (lineBytes[lastIndex] == Convert.ToByte('\r')) && (bytes[0] == Convert.ToByte('\n')))
                {
                    lineBytes.RemoveAt(lastIndex);
                    break;
                }

                lineBytes.Add(bytes[0]);
            }

            return (encoding ?? Encoding.UTF8).GetString(lineBytes.ToArray());
        }

        public static Task SendBytesAsync(this Socket socket, byte[] bytes)
        {
            return socket.SendBytesAsync(bytes, 0, bytes.Length);
        }

        public static async Task SendBytesAsync(this Socket socket, byte[] bytes, int offset, int count)
        {
            while (count > 0)
            {
                var sentCount = await socket.SendAsync(bytes, offset, count).ConfigureAwait(false);
                if (sentCount < 1)
                {
                    throw new IOException("Failed to send data to socket");
                }

                offset += sentCount;
                count -= sentCount;
            }
        }

        public static async Task<byte[]> ReceiveBytesAsync(this Socket socket, int count)
        {
            var bytes = new byte[count];
            await socket.ReceiveBytesAsync(bytes, 0, count).ConfigureAwait(false);
            return bytes;
        }

        public static async Task ReceiveBytesAsync(this Socket socket, byte[] bytes, int offset, int count)
        {
            while (count > 0)
            {
                var receivedCount = await socket.ReceiveAsync(bytes, offset, count).ConfigureAwait(false);
                if (receivedCount < 1)
                {
                    throw new IOException("Failed to receive data from socket");
                }

                offset += receivedCount;
                count -= receivedCount;
            }
        }

        private static Task<int> SendAsync(this Socket socket, byte[] bytes, int offset, int count)
        {
            return Task<int>.Factory.FromAsync(
                (callback, state) => socket.BeginSend(bytes, offset, count, SocketFlags.None, callback, state),
                socket.EndSend,
                null
            );
        }

        private static Task<int> ReceiveAsync(this Socket socket, byte[] bytes, int offset, int count)
        {
            return Task<int>.Factory.FromAsync(
                (callback, state) => socket.BeginReceive(bytes, offset, count, SocketFlags.None, callback, state),
                socket.EndReceive,
                null
            );
        }
    }
}
