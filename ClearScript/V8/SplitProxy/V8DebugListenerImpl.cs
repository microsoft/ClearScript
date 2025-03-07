// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8DebugListenerImpl : IV8DebugListener
    {
        private V8EntityHolder holder;

        private V8DebugCallback.Handle Handle => (V8DebugCallback.Handle)holder.Handle;

        public V8DebugListenerImpl(V8DebugCallback.Handle hCallback)
        {
            holder = new V8EntityHolder("V8 debug listener", () => hCallback);
        }

        #region IV8DebugListener implementation

        public void ConnectClient()
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, handle) => instance.V8DebugCallback_ConnectClient(handle), Handle);
        }

        public void SendCommand(string command)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8DebugCallback_SendCommand(ctx.Handle, ctx.command), (Handle, command));
        }

        public void DisconnectClient()
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, handle) => instance.V8DebugCallback_DisconnectClient(handle), Handle);
        }

        #endregion

        #region disposal / finalization

        public void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~V8DebugListenerImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
