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
            if (handle != V8Entity.Handle.Empty)
            {
                using (V8SplitProxyNative.InvokeNoThrow(out var instance))
                {
                    instance.V8Entity_Release(handle);
                }
            }
        }

        public static void Destroy(ref V8EntityHolder holder)
        {
            if (holder.handle != V8Entity.Handle.Empty)
            {
                using (V8SplitProxyNative.InvokeNoThrow(out var instance))
                {
                    instance.V8Entity_DestroyHandle(holder.handle);
                }
            }

            if (holder.registered)
            {
                V8Proxy.OnEntityHolderDestroyed();
            }

            holder = new V8EntityHolder(holder.name);
        }
    }
}
