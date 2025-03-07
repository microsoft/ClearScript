// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8TestProxyImpl : V8TestProxy
    {
        public override UIntPtr GetNativeDigest(string value)
        {
            return V8SplitProxyNative.InvokeNoThrow(static (instance, value) => instance.V8UnitTestSupport_GetTextDigest(value), value);
        }

        public override Statistics GetStatistics()
        {
            var statistics = new Statistics();
            V8SplitProxyNative.InvokeNoThrow(static (instance, statistics) => instance.V8UnitTestSupport_GetStatistics(out statistics.IsolateCount, out statistics.ContextCount), statistics);
            return statistics;
        }

        public override void Dispose()
        {
        }
    }
}
