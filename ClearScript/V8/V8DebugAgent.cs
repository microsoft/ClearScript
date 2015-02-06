// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal sealed class V8DebugAgent : IDisposable
    {
        #region data

        private readonly string name;
        private readonly string version;
        private readonly IV8DebugListener listener;

        private TcpListener tcpListener;
        private TcpClient tcpClient;

        private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private readonly AutoResetEvent queueEvent = new AutoResetEvent(false);
        private RegisteredWaitHandle queueWaitHandle;

        private DisposedFlag disposedFlag = new DisposedFlag();

        #endregion

        #region constructors

        public V8DebugAgent(string name, string version, int port, IV8DebugListener listener)
        {
            this.name = name;
            this.version = version;
            this.listener = listener;

            RegisterWaitForQueueEvent();

            MiscHelpers.Try(() =>
            {
                tcpListener = new TcpListener(IPAddress.Loopback, port);
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
            });
        }

        #endregion

        #region session management

        private void OnClientAccepted(IAsyncResult result)
        {
            if (!disposedFlag.IsSet())
            {
                TcpClient tempTcpClient;
                if (MiscHelpers.Try(out tempTcpClient, () => tcpListener.EndAcceptTcpClient(result)))
                {
                    if (Interlocked.CompareExchange(ref tcpClient, tempTcpClient, null) == null)
                    {
                        ConnectClient();
                    }
                    else
                    {
                        RejectClient(tempTcpClient);
                    }

                    MiscHelpers.Try(() => tcpListener.BeginAcceptTcpClient(OnClientAccepted, null));
                }
            }
        }

        private void ConnectClient()
        {
            SendStringAsync(tcpClient, "Type:connect\r\nV8-Version:" + version + "\r\nProtocol-Version:1\r\nEmbedding-Host:" + name + "\r\nContent-Length:0\r\n\r\n", OnConnectionMessageSent);
        }

        private void OnConnectionMessageSent(bool succeeded)
        {
            if (succeeded)
            {
                ReceiveMessage();
            }
            else
            {
                DisconnectClient("Could not send connection message");
            }
        }

        private static void RejectClient(TcpClient tcpClient)
        {
            SendStringAsync(tcpClient, "Remote debugging session already active\r\n", succeeded => MiscHelpers.Try(tcpClient.Close));
        }

        private void DisconnectClient(string errorMessage)
        {
            var tempTcpClient = Interlocked.Exchange(ref tcpClient, null);
            if (tempTcpClient != null)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    Trace("Disconnecting debugger: " + errorMessage);
                }

                MiscHelpers.Try(tempTcpClient.Close);
            }
        }

        #endregion

        #region inbound message processing

        private void ReceiveMessage()
        {
            ReceiveLineAsync(line => OnHeaderLineReceived(line, -1));
        }

        private void OnHeaderLineReceived(string line, int contentLength)
        {
            if (line != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var segments = line.Split(':');
                    if ((segments.Length == 2) && (segments[0] == "Content-Length"))
                    {
                        int length;
                        if (int.TryParse(segments[1], out length))
                        {
                            contentLength = length;
                        }
                    }

                    ReceiveLineAsync(nextLine => OnHeaderLineReceived(nextLine, contentLength));
                }
                else if (contentLength > 0)
                {
                    ReceiveStringAsync(contentLength, OnMessageReceived);
                }
                else if (contentLength == 0)
                {
                    OnMessageReceived(string.Empty);
                }
                else
                {
                    OnMessageReceivedInternal(null, "Message content length not specified");
                }
            }
            else
            {
                OnMessageReceivedInternal(null, "Could not receive message header");
            }
        }

        void OnMessageReceived(string content)
        {
            OnMessageReceivedInternal(content, "Could not receive message content");
        }

        void OnMessageReceivedInternal(string content, string errorMessage)
        {
            bool disconnect;
            if (content != null)
            {
                disconnect = content.Contains("\"type\":\"request\",\"command\":\"disconnect\"}");
            }
            else
            {
                content = "{\"seq\":1,\"type\":\"request\",\"command\":\"disconnect\"}";
                disconnect = true;
            }

            listener.OnMessageReceived(content);

            if (disconnect)
            {
                DisconnectClient(errorMessage);
            }
            else
            {
                ReceiveMessage();
            }
        }

        #endregion

        #region outbound message queue

        public void SendMessage(string content)
        {
            if (!disposedFlag.IsSet())
            {
                queue.Enqueue(content);
                MiscHelpers.Try(() => queueEvent.Set());
            }
        }

        private void RegisterWaitForQueueEvent()
        {
            var oldQueueWaitHandle = Interlocked.Exchange(ref queueWaitHandle, ThreadPool.RegisterWaitForSingleObject(queueEvent, OnQueueEvent, null, Timeout.Infinite, true));
            if (oldQueueWaitHandle != null)
            {
                oldQueueWaitHandle.Unregister(null);
            }
        }

        private void OnQueueEvent(object state, bool timedOut)
        {
            string content;
            while (queue.TryDequeue(out content))
            {
                var tempTcpClient = tcpClient;
                if (tempTcpClient != null)
                {
                    var contentBytes = Encoding.UTF8.GetBytes(content);
                    var headerBytes = Encoding.UTF8.GetBytes(MiscHelpers.FormatInvariant("Content-Length:{0}\r\n\r\n", contentBytes.Length));
                    tempTcpClient.Client.SendBytesAsync(headerBytes.Concat(contentBytes).ToArray(), OnMessageSent);
                    return;
                }
            }

            if (!disposedFlag.IsSet())
            {
                RegisterWaitForQueueEvent();
            }
        }

        private void OnMessageSent(bool succeeded)
        {
            if (succeeded)
            {
                OnQueueEvent(null, false);
            }
            else
            {
                DisconnectClient("Could not send message");
            }
        }

        #endregion

        #region protocol helpers

        private static void SendStringAsync(TcpClient tcpClient, string content, Action<bool> callback)
        {
            tcpClient.Client.SendBytesAsync(Encoding.UTF8.GetBytes(content), callback);
        }

        private void ReceiveStringAsync(int sizeInBytes, Action<string> callback)
        {
            tcpClient.Client.ReceiveBytesAsync(sizeInBytes, bytes => callback((bytes != null) ? Encoding.UTF8.GetString(bytes) : null));
        }

        private void ReceiveLineAsync(Action<string> callback)
        {
            var lineBytes = new List<byte>(1024);
            tcpClient.Client.ReceiveBytesAsync(1, bytes => OnLineBytesReceived(bytes, lineBytes, callback));
        }

        private void OnLineBytesReceived(byte[] bytes, List<byte> lineBytes, Action<string> callback)
        {
            if (bytes != null)
            {
                var lastIndex = lineBytes.Count - 1;
                if ((lastIndex >= 0) && (lineBytes[lastIndex] == Convert.ToByte('\r')) && (bytes[0] == Convert.ToByte('\n')))
                {
                    lineBytes.RemoveAt(lastIndex);
                    callback(Encoding.UTF8.GetString(lineBytes.ToArray()));
                }
                else
                {
                    lineBytes.Add(bytes[0]);
                    tcpClient.Client.ReceiveBytesAsync(1, nextBytes => OnLineBytesReceived(nextBytes, lineBytes, callback));
                }
            }
            else
            {
                callback(null);
            }
        }

        #endregion

        #region diagnostics

        [Conditional("DEBUG")]
        void Trace(string message)
        {
            Debug.WriteLine(message);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                MiscHelpers.Try(tcpListener.Stop);
                DisconnectClient(null);

                queueWaitHandle.Unregister(null);
                queueEvent.Close();

                listener.Dispose();
            }
        }

        #endregion
    }
}
