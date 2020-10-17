// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class NativeCallbackImpl : INativeCallback
    {
        private V8EntityHolder holder;

        private NativeCallback.Handle Handle => (NativeCallback.Handle)holder.Handle;

        public NativeCallbackImpl(NativeCallback.Handle hCallback)
        {
            holder = new V8EntityHolder("native callback", () => hCallback);
        }

        #region INativeCallback implementation

        public void Invoke()
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.NativeCallback_Invoke(Handle));
        }

        #endregion

        #region disposal / finalization

        public void Dispose()
        {
            holder.ReleaseEntity();
        }

        ~NativeCallbackImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
