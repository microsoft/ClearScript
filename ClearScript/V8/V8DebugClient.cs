// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This class uses a custom method for deterministic teardown.")]
    internal sealed class V8DebugClient
    {
        #region data

        private readonly V8DebugAgent agent;
        private readonly WebSocket webSocket;

        private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private readonly AutoResetEvent queueEvent = new AutoResetEvent(false);
        private RegisteredWaitHandle queueWaitHandle;

        private InterlockedDisposedFlag disposedFlag = new InterlockedDisposedFlag();

        #endregion

        #region initialization

        public V8DebugClient(V8DebugAgent agent, WebSocket webSocket)
        {
            this.agent = agent;
            this.webSocket = webSocket;
        }

        public void Start()
        {
            RegisterWaitForQueueEvent();
            StartReceiveMessage();
        }

        #endregion

        #region inbound message processing

        private void StartReceiveMessage()
        {
            if (!MiscHelpers.Try(() => webSocket.ReceiveMessageAsync(WebSocketMessageType.Text, OnMessageReceived)))
            {
                OnFailed(WebSocketCloseStatus.ProtocolError, "Could not receive data from web socket");
            }
        }

        private void OnMessageReceived(byte[] bytes, WebSocketCloseStatus status, string errorMessage)
        {
            if (!disposedFlag.IsSet)
            {
                if (bytes == null)
                {
                    OnFailed(status, errorMessage);
                }
                else
                {
                    agent.SendCommand(this, Encoding.UTF8.GetString(bytes));
                    StartReceiveMessage();
                }
            }
        }

        #endregion

        #region outbound message queue

        public void SendMessage(string message)
        {
            if (!disposedFlag.IsSet)
            {
                queue.Enqueue(message);
                MiscHelpers.Try(() => queueEvent.Set());
            }
        }

        private void RegisterWaitForQueueEvent()
        {
            RegisteredWaitHandle newQueueWaitHandle;
            if (MiscHelpers.Try(out newQueueWaitHandle, () => ThreadPool.RegisterWaitForSingleObject(queueEvent, OnQueueEvent, null, Timeout.Infinite, true)))
            {
                var oldQueueWaitHandle = Interlocked.Exchange(ref queueWaitHandle, newQueueWaitHandle);
                if (oldQueueWaitHandle != null)
                {
                    oldQueueWaitHandle.Unregister(null);
                }
            }
        }

        private void OnQueueEvent(object state, bool timedOut)
        {
            if (!disposedFlag.IsSet)
            {
                string message;
                if (queue.TryDequeue(out message))
                {
                    if (!MiscHelpers.Try(() => webSocket.SendMessageAsync(WebSocketMessageType.Text, Encoding.UTF8.GetBytes(message), OnMessageSent)))
                    {
                        OnFailed(WebSocketCloseStatus.ProtocolError, "Could not send data to web socket");
                    }
                }
                else
                {
                    RegisterWaitForQueueEvent();
                }
            }
        }

        private void OnMessageSent(bool succeeded, WebSocketCloseStatus status, string errorMessage)
        {
            if (!disposedFlag.IsSet)
            {
                if (succeeded)
                {
                    OnQueueEvent(null, false);
                }
                else
                {
                    OnFailed(status, errorMessage);
                }
            }
        }

        #endregion

        #region shutdown

        private void OnFailed(WebSocketCloseStatus status, string errorMessage)
        {
            Dispose(status, errorMessage);
            agent.OnClientFailed(this);
        }

        public void Dispose(WebSocketCloseStatus status, string errorMessage)
        {
            if (disposedFlag.Set())
            {
                switch (webSocket.State)
                {
                    case WebSocketState.Connecting:
                    case WebSocketState.Open:
                    case WebSocketState.CloseReceived:
                        MiscHelpers.Try(() => webSocket.CloseOutputAsync(status, errorMessage, CancellationToken.None));
                        break;
                }

                var tempQueueWaitHandle = queueWaitHandle;
                if (tempQueueWaitHandle != null)
                {
                    tempQueueWaitHandle.Unregister(null);
                }

                queueEvent.Close();
            }
        }

        #endregion
    }
}
