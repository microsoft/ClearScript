// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal readonly struct V8EntityHolder
    {
        private readonly string name;
        private readonly bool registered;
        private readonly V8Entity.Handle handle;

        public V8Entity.Handle Handle => (handle != V8Entity.Handle.Empty) ? handle : throw new InvalidOperationException("The " + name + " proxy has been destroyed");

        public V8EntityHolder(string name, Func<V8Entity.Handle> acquireHandle)
        {
            this.name = name;
            V8Proxy.OnEntityHolderCreated();
            registered = true;
            handle = acquireHandle();
        }

        private V8EntityHolder(string name)
        {
            this.name = name;
            registered = false;
            handle = V8Entity.Handle.Empty;
        }

        public void ReleaseEntity()
        {
            var tempHandle = handle;
            if (tempHandle != V8Entity.Handle.Empty)
            {
                V8SplitProxyNative.InvokeNoThrow(static (instance, tempHandle) => instance.V8Entity_Release(tempHandle), tempHandle);
            }
        }

        public static void Destroy(ref V8EntityHolder holder)
        {
            var tempHandle = holder.handle;
            if (tempHandle != V8Entity.Handle.Empty)
            {
                V8SplitProxyNative.InvokeNoThrow(static (instance, tempHandle) => instance.V8Entity_DestroyHandle(tempHandle), tempHandle);
            }

            if (holder.registered)
            {
                V8Proxy.OnEntityHolderDestroyed();
            }

            holder = new V8EntityHolder(holder.name);
        }
    }
}
