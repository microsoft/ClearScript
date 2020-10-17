// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal struct V8SplitProxyImplCore
    {
        private bool active;

        public IntPtr Handle { get; private set; }

        public void Initialize(Func<IntPtr> getHandle)
        {
            V8Proxy.OnSplitImplCreated();
            Handle = getHandle();
            active = true;
        }

        public void Destroy()
        {
            if (Handle != IntPtr.Zero)
            {
                var handle = Handle;
                V8SplitProxyNative.Invoke(instance => instance.V8EntityHandle_Release(handle));
                Handle = IntPtr.Zero;
            }

            if (active)
            {
                V8Proxy.OnSplitImplDestroyed();
                active = false;
            }
        }
    }
}
