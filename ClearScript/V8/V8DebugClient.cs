// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.Web;

namespace Microsoft.ClearScript.V8
{
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This class uses a custom method for deterministic teardown.")]
    internal sealed class V8DebugClient
    {
        #region data

        private readonly V8DebugAgent agent;
        private readonly WebSocket webSocket;

        private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim sendSemaphore = new SemaphoreSlim(1);

        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        #endregion

        #region initialization

        public V8DebugClient(V8DebugAgent agent, WebSocket webSocket)
        {
            this.agent = agent;
            this.webSocket = webSocket;
        }

        public void Start()
        {
            StartReceiveMessage();
        }

        #endregion

        #region inbound message reception

        private void StartReceiveMessage()
        {
            webSocket.ReceiveMessageAsync().ContinueWith(OnMessageReceived);
        }

        private void OnMessageReceived(Task<WebSocket.Message> task)
        {
            try
            {
                var message = task.Result;
                if (!disposedFlag.IsSet)
                {
                    if (message.IsBinary)
                    {
                        OnFailed(WebSocket.ErrorCode.InvalidMessageType, "Received unexpected binary message from WebSocket");
                    }
                    else
                    {
                        agent.SendCommand(this, Encoding.UTF8.GetString(message.Payload));
                        StartReceiveMessage();
                    }
                }
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle(exception =>
                {
                    if (!disposedFlag.IsSet)
                    {
                        var webSocketException = exception as WebSocket.Exception;
                        if (webSocketException != null)
                        {
                            OnFailed(webSocketException.ErrorCode, webSocketException.Message);
                        }
                        else
                        {
                            OnFailed(WebSocket.ErrorCode.ProtocolError, "Could not receive message from WebSocket");
                        }
                    }

                    return true;
                });
            }
        }

        #endregion

        #region outbound message delivery

        public void SendMessage(string message)
        {
            if (!disposedFlag.IsSet)
            {
                queue.Enqueue(message);
                SendMessagesAsync().ContinueWith(OnMessagesSent);
            }
        }

        private async Task SendMessagesAsync()
        {
            using (await sendSemaphore.CreateLockScopeAsync().ConfigureAwait(false))
            {
                string message;
                while (queue.TryDequeue(out message))
                {
                    await webSocket.SendMessageAsync(Encoding.UTF8.GetBytes(message)).ConfigureAwait(false);
                }
            }
        }

        private void OnMessagesSent(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle(exception =>
                {
                    if (!disposedFlag.IsSet)
                    {
                        var webSocketException = exception as WebSocket.Exception;
                        if (webSocketException != null)
                        {
                            OnFailed(webSocketException.ErrorCode, webSocketException.Message);
                        }
                        else
                        {
                            OnFailed(WebSocket.ErrorCode.ProtocolError, "Could not send message to WebSocket");
                        }
                    }

                    return true;
                });
            }
        }

        #endregion

        #region teardown

        private void OnFailed(WebSocket.ErrorCode errorCode, string message)
        {
            Dispose(errorCode, message);
            agent.OnClientFailed(this);
        }

        public void Dispose(WebSocket.ErrorCode errorCode, string message)
        {
            if (disposedFlag.Set())
            {
                webSocket.Close(errorCode, message);
                sendSemaphore.Dispose();
            }
        }

        #endregion
    }
}
