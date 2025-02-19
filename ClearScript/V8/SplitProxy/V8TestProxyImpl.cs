// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8TestProxyImpl : V8TestProxy
    {
        public override UIntPtr GetNativeDigest(string value)
        {
            using (V8SplitProxyNative.InvokeNoThrow(out var instance))
            {
                return instance.V8UnitTestSupport_GetTextDigest(value);
            }
        }

        public override Statistics GetStatistics()
        {
            var statistics = new Statistics();

            using (V8SplitProxyNative.InvokeNoThrow(out var instance))
            {
                instance.V8UnitTestSupport_GetStatistics(out statistics.IsolateCount, out statistics.ContextCount);
            }

            return statistics;
        }

        public override void Dispose()
        {
        }
    }
}
