// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8
{
    internal class V8ProxyCounters
    {
        public ulong IsolateCount { get; set; }

        public ulong ContextCount { get; set; }
    }

    internal abstract class V8TestProxy : V8Proxy
    {
        public static V8TestProxy Create()
        {
            return CreateImpl<V8TestProxy>();
        }

        public abstract V8ProxyCounters GetCounters();
    }
}
