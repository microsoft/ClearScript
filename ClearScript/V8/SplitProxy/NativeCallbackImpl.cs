﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
            using (V8SplitProxyNative.InvokeNoThrow(out var instance))
            {
                instance.NativeCallback_Invoke(Handle);
            }
        }

        #endregion

        #region disposal / finalization

        public void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~NativeCallbackImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
