// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.Properties;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.Web;

namespace Microsoft.ClearScript.V8
{
    internal sealed class V8DebugAgent : IDisposable
    {
        #region data

        private const string faviconUrl = "https://microsoft.github.io/ClearScript/favicon.png";

        private readonly Guid targetId = Guid.NewGuid();
        private readonly string name;
        private readonly string version;
        private readonly IV8DebugListener listener;

        private TcpListener tcpListener;
        private V8DebugClient activeClient;

        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

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
                    tcpListener = new TcpListener(IPAddress.Any, port);
                    tcpListener.Start();
                });
            }

            if (!started)
            {
                started = MiscHelpers.Try(() =>
                {
                    tcpListener = new TcpListener(IPAddress.Loopback, port);
                    tcpListener.Start();
                });
            }

            if (started)
            {
                StartAcceptWebClient();
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

        #region Web endpoint

        private void StartAcceptWebClient()
        {
            tcpListener.AcceptSocketAsync().ContinueWith(OnWebClientAccepted);
        }

        private void OnWebClientAccepted(Task<Socket> task)
        {
            Socket socket;
            var succeeded = MiscHelpers.Try(out socket, () => task.Result);

            if (!disposedFlag.IsSet)
            {
                if (succeeded)
                {
                    WebContext.CreateAsync(socket).ContinueWith(OnWebContextCreated);
                }

                StartAcceptWebClient();
            }
        }

        private void OnWebContextCreated(Task<WebContext> task)
        {
            WebContext webContext;
            if (MiscHelpers.Try(out webContext, () => task.Result) && !disposedFlag.IsSet)
            {
                if (!webContext.Request.IsWebSocketRequest)
                {
                    HandleWebRequest(webContext);
                }
                else if (!webContext.Request.RawUrl.Equals("/" + targetId, StringComparison.OrdinalIgnoreCase))
                {
                    webContext.Response.Close(404);
                }
                else
                {
                    StartAcceptWebSocket(webContext);
                }
            }
        }

        private void HandleWebRequest(WebContext webContext)
        {
            // https://github.com/buggerjs/bugger-daemon/blob/master/README.md#api,
            // https://github.com/nodejs/node/blob/master/src/inspector_socket_server.cc

            if (webContext.Request.RawUrl.Equals("/json", StringComparison.OrdinalIgnoreCase) ||
                webContext.Request.RawUrl.Equals("/json/list", StringComparison.OrdinalIgnoreCase))
            {
                // ReSharper disable PossibleNullReferenceException

                if (activeClient != null)
                {
                    SendWebResponse(webContext, MiscHelpers.FormatInvariant(
                        "[ {{\r\n" +
                            "  \"id\": \"{0}\",\r\n" +
                            "  \"type\": \"node\",\r\n" +
                            "  \"description\": \"ClearScript V8 runtime: {1}\",\r\n" +
                            "  \"title\": \"{2}\",\r\n" +
                            "  \"url\": \"{3}\",\r\n" +
                            "  \"faviconUrl\": \"{4}\"\r\n" +
                        "}} ]\r\n",
                        targetId,
                        JsonEscape(name),
                        JsonEscape(AppDomain.CurrentDomain.FriendlyName),
                        JsonEscape(new Uri(Process.GetCurrentProcess().MainModule.FileName)),
                        faviconUrl
                    ));
                }
                else
                {
                    SendWebResponse(webContext, MiscHelpers.FormatInvariant(
                        "[ {{\r\n" +
                            "  \"id\": \"{0}\",\r\n" +
                            "  \"type\": \"node\",\r\n" +
                            "  \"description\": \"ClearScript V8 runtime: {1}\",\r\n" +
                            "  \"title\": \"{2}\",\r\n" +
                            "  \"url\": \"{3}\",\r\n" +
                            "  \"faviconUrl\": \"{6}\",\r\n" +
                            "  \"devtoolsFrontendUrl\": \"chrome-devtools://devtools/bundled/inspector.html?experiments=true&v8only=true&ws={4}:{5}/{0}\",\r\n" +
                            "  \"webSocketDebuggerUrl\": \"ws://{4}:{5}/{0}\"\r\n" +
                        "}} ]\r\n",
                        targetId,
                        JsonEscape(name),
                        JsonEscape(AppDomain.CurrentDomain.FriendlyName),
                        JsonEscape(new Uri(Process.GetCurrentProcess().MainModule.FileName)),
                        webContext.Request.Uri.Host,
                        webContext.Request.Uri.Port,
                        faviconUrl
                    ));
                }

                // ReSharper restore PossibleNullReferenceException
            }
            else if (webContext.Request.RawUrl.Equals("/json/version", StringComparison.OrdinalIgnoreCase))
            {
                SendWebResponse(webContext, MiscHelpers.FormatInvariant(
                    "{{\r\n" +
                        "  \"Browser\": \"ClearScript {0} [V8 {1}]\",\r\n" +
                        "  \"Protocol-Version\": \"1.1\"\r\n" +
                    "}}\r\n",
                    ClearScriptVersion.Value,
                    version
                ));
            }
            else if (webContext.Request.RawUrl.StartsWith("/json/activate/", StringComparison.OrdinalIgnoreCase))
            {
                var requestTargetId = webContext.Request.RawUrl.Substring(15);
                if (requestTargetId.Equals(targetId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    SendWebResponse(webContext, "Target activated", "text/plain");
                }
                else
                {
                    SendWebResponse(webContext, "No such target id: " + requestTargetId, "text/plain", 404);
                }
            }
            else if (webContext.Request.RawUrl.StartsWith("/json/close/", StringComparison.OrdinalIgnoreCase))
            {
                var requestTargetId = webContext.Request.RawUrl.Substring(12);
                if (requestTargetId.Equals(targetId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    SendWebResponse(webContext, "Target is closing", "text/plain");
                }
                else
                {
                    SendWebResponse(webContext, "No such target id: " + requestTargetId, "text/plain", 404);
                }
            }
            else if (webContext.Request.RawUrl.StartsWith("/json/new?", StringComparison.OrdinalIgnoreCase) ||
                     webContext.Request.RawUrl.Equals("/json/protocol", StringComparison.OrdinalIgnoreCase))
            {
                webContext.Response.Close(501);
            }
            else
            {
                webContext.Response.Close(404);
            }
        }

        #endregion

        #region WebSocket client connection

        private void StartAcceptWebSocket(WebContext webContext)
        {
            webContext.AcceptWebSocketAsync().ContinueWith(task => OnWebSocketAccepted(webContext, task));
        }

        private void OnWebSocketAccepted(WebContext webContext, Task<WebSocket> task)
        {
            WebSocket webSocket;
            if (MiscHelpers.Try(out webSocket, () => task.Result))
            {
                if (!ConnectClient(webSocket))
                {
                    webSocket.Close(WebSocket.ErrorCode.PolicyViolation, "A debugger is already connected");
                }
            }
            else
            {
                webContext.Response.Close(500);
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

        private void DisconnectClient(WebSocket.ErrorCode errorCode, string message)
        {
            var client = Interlocked.Exchange(ref activeClient, null);
            if (client != null)
            {
                client.Dispose(errorCode, message);
                listener.DisconnectClient();
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                MiscHelpers.Try(tcpListener.Stop);
                DisconnectClient(WebSocket.ErrorCode.EndpointUnavailable, "The V8 runtime has been destroyed");
                listener.Dispose();
            }
        }

        #endregion

        #region protocol utilities

        private static void SendWebResponse(WebContext webContext, string content, string contentType = "application/json", int statusCode = 200)
        {
            using (webContext.Response)
            {
                var contentBytes = Encoding.UTF8.GetBytes(content);
                webContext.Response.ContentType = contentType + "; charset=UTF-8";
                webContext.Response.OutputStream.Write(contentBytes, 0, contentBytes.Length);
                webContext.Response.StatusCode = statusCode;
            }
        }

        private static string JsonEscape(object value)
        {
            return new string(value.ToString().Select(ch => ((ch == '\"') || (ch == '\\')) ? '_' : ch).ToArray());
        }

        #endregion
    }
}
