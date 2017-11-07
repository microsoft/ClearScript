// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.Properties;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal sealed class V8DebugAgent : IDisposable
    {
        #region data

        private readonly Guid targetId = Guid.NewGuid();

        private readonly string name;
        private readonly string version;
        private readonly IV8DebugListener listener;

        private HttpListener httpListener;
        private V8DebugClient activeClient;

        private InterlockedDisposedFlag disposedFlag = new InterlockedDisposedFlag();

        #endregion

        #region constructors

        public V8DebugAgent(string name, string version, int port, bool remote, IV8DebugListener listener)
        {
            this.name = name;
            this.version = version;
            this.listener = listener;

            var started = false;

            if (remote)
            {
                started = MiscHelpers.Try(() =>
                {
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add("http://+:" + port + "/");
                    httpListener.Start();
                });
            }

            if (!started)
            {
                started = MiscHelpers.Try(() =>
                {
                    httpListener = new HttpListener();
                    httpListener.Prefixes.Add("http://127.0.0.1:" + port + "/");
                    httpListener.Start();
                });
            }

            if (started)
            {
                StartAcquireHttpListenerContext();
            }
        }

        #endregion

        #region public members

        public void SendCommand(V8DebugClient client, string command)
        {
            if (client == activeClient)
            {
                listener.SendCommand(command);
            }
        }

        public void SendMessage(string message)
        {
            if (!disposedFlag.IsSet)
            {
                var client = activeClient;
                if (client != null)
                {
                    client.SendMessage(message);
                }
            }
        }

        public void OnClientFailed(V8DebugClient client)
        {
            if (Interlocked.CompareExchange(ref activeClient, null, client) == client)
            {
                listener.DisconnectClient();
            }
        }

        #endregion

        #region HTTP endpoint

        private void StartAcquireHttpListenerContext()
        {
            MiscHelpers.Try(() => httpListener.GetContextAsync().ContinueWith(OnHttpListenerContextAcquired));
        }

        private void OnHttpListenerContextAcquired(Task<HttpListenerContext> task)
        {
            if (!disposedFlag.IsSet)
            {
                HttpListenerContext httpContext;
                if (MiscHelpers.Try(out httpContext, () => task.Result))
                {
                    if (!httpContext.Request.IsWebSocketRequest)
                    {
                        HandleHttpRequest(httpContext);
                    }
                    else if (!httpContext.Request.RawUrl.Equals("/" + targetId, StringComparison.OrdinalIgnoreCase))
                    {
                        httpContext.Response.StatusCode = 404;
                        httpContext.Response.Close();
                    }
                    else if (!StartAcceptWebSocket(httpContext))
                    {
                        httpContext.Response.StatusCode = 500;
                        httpContext.Response.Close();
                    }
                }

                StartAcquireHttpListenerContext();
            }
        }

        private void HandleHttpRequest(HttpListenerContext httpContext)
        {
            // https://github.com/buggerjs/bugger-daemon/blob/master/README.md#api,
            // https://github.com/nodejs/node/blob/master/src/inspector_socket_server.cc

            if (httpContext.Request.RawUrl.Equals("/json", StringComparison.OrdinalIgnoreCase) ||
                httpContext.Request.RawUrl.Equals("/json/list", StringComparison.OrdinalIgnoreCase))
            {
                if (activeClient != null)
                {
                    SendHttpResponse(httpContext, MiscHelpers.FormatInvariant(
                        "[ {{\n" +
                            "  \"id\": \"{0}\",\n" +
                            "  \"type\": \"node\",\n" +
                            "  \"description\": \"ClearScript V8 runtime: {1}\",\n" +
                            "  \"title\": \"{2}\",\n" +
                            "  \"url\": \"{3}\"\n" +
                        "}} ]\n",
                        targetId,
                        JsonEscape(name),
                        JsonEscape(AppDomain.CurrentDomain.FriendlyName),
                        JsonEscape(new Uri(Process.GetCurrentProcess().MainModule.FileName))
                    ));
                }
                else
                {
                    SendHttpResponse(httpContext, MiscHelpers.FormatInvariant(
                        "[ {{\n" +
                            "  \"id\": \"{0}\",\n" +
                            "  \"type\": \"node\",\n" +
                            "  \"description\": \"ClearScript V8 runtime: {1}\",\n" +
                            "  \"title\": \"{2}\",\n" +
                            "  \"url\": \"{3}\",\n" +
                            "  \"devtoolsFrontendUrl\": \"chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws={4}:{5}/{0}\",\n" +
                            "  \"webSocketDebuggerUrl\": \"ws://{4}:{5}/{0}\"\n" +
                        "}} ]\n",
                        targetId,
                        JsonEscape(name),
                        JsonEscape(AppDomain.CurrentDomain.FriendlyName),
                        JsonEscape(new Uri(Process.GetCurrentProcess().MainModule.FileName)),
                        httpContext.Request.Url.Host,
                        httpContext.Request.Url.Port
                    ));
                }
            }
            else if (httpContext.Request.RawUrl.Equals("/json/version", StringComparison.OrdinalIgnoreCase))
            {
                SendHttpResponse(httpContext, MiscHelpers.FormatInvariant(
                    "{{\n" +
                        "  \"Browser\": \"ClearScript {0} with V8 {1}\",\n" +
                        "  \"Protocol-Version\": \"1.1\"\n" +
                    "}}\n",
                    ClearScriptVersion.Value,
                    version
                ));
            }
            else if (httpContext.Request.RawUrl.StartsWith("/json/activate/", StringComparison.OrdinalIgnoreCase))
            {
                var requestTargetId = httpContext.Request.RawUrl.Substring(15);
                if (requestTargetId.Equals(targetId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    SendHttpResponse(httpContext, "Target activated", "text/plain");
                }
                else
                {
                    SendHttpResponse(httpContext, "No such target id: " + requestTargetId, "text/plain", 404);
                }
            }
            else if (httpContext.Request.RawUrl.StartsWith("/json/close/", StringComparison.OrdinalIgnoreCase))
            {
                var requestTargetId = httpContext.Request.RawUrl.Substring(12);
                if (requestTargetId.Equals(targetId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    SendHttpResponse(httpContext, "Target is closing", "text/plain");
                }
                else
                {
                    SendHttpResponse(httpContext, "No such target id: " + requestTargetId, "text/plain", 404);
                }
            }
            else if (httpContext.Request.RawUrl.StartsWith("/json/new?", StringComparison.OrdinalIgnoreCase) ||
                     httpContext.Request.RawUrl.Equals("/json/protocol", StringComparison.OrdinalIgnoreCase))
            {
                httpContext.Response.StatusCode = 501;
                httpContext.Response.Close();
            }
            else
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.Close();
            }
        }

        #endregion

        #region WebSocket client connection

        private bool StartAcceptWebSocket(HttpListenerContext httpContext)
        {
            return MiscHelpers.Try(() => httpContext.AcceptWebSocketAsync(null).ContinueWith(task => OnWebSocketAccepted(httpContext, task)));
        }

        private void OnWebSocketAccepted(HttpListenerContext httpContext, Task<HttpListenerWebSocketContext> task)
        {
            if (!disposedFlag.IsSet)
            {
                HttpListenerWebSocketContext webSocketContext;
                if (MiscHelpers.Try(out webSocketContext, () => task.Result))
                {
                    if ((webSocketContext == null) || (webSocketContext.WebSocket == null))
                    {
                        httpContext.Response.StatusCode = 500;
                        httpContext.Response.Close();
                    }
                    else if (!ConnectClient(webSocketContext.WebSocket))
                    {
                        httpContext.Response.StatusCode = 403;
                        httpContext.Response.Close();
                    }
                }
            }
        }

        private bool ConnectClient(WebSocket webSocket)
        {
            var client = new V8DebugClient(this, webSocket);
            if (Interlocked.CompareExchange(ref activeClient, client, null) == null)
            {
                listener.ConnectClient();
                client.Start();
                return true;
            }

            return false;
        }

        private void DisconnectClient(WebSocketCloseStatus status, string errorMessage)
        {
            var client = Interlocked.Exchange(ref activeClient, null);
            if (client != null)
            {
                client.Dispose(status, errorMessage);
                listener.DisconnectClient();
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                MiscHelpers.Try(httpListener.Stop);
                DisconnectClient(WebSocketCloseStatus.EndpointUnavailable, "The V8 runtime has been destroyed");
                listener.Dispose();
            }
        }

        #endregion

        #region protocol utilities

        private static void SendHttpResponse(HttpListenerContext httpContext, string content, string contentType = "application/json", int statusCode = 200)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            httpContext.Response.SendChunked = false;
            httpContext.Response.ContentType = contentType + "; charset=UTF-8";
            httpContext.Response.ContentLength64 = contentBytes.Length;
            httpContext.Response.OutputStream.Write(contentBytes, 0, contentBytes.Length);
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.Close();
        }

        private static string JsonEscape(object value)
        {
            return new string(value.ToString().Select(ch => ((ch == '\"') || (ch == '\\')) ? '_' : ch).ToArray());
        }

        #endregion
    }
}
