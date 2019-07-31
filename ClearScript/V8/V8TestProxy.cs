// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8TestProxy : V8Proxy
    {
        public static V8TestProxy Create()
        {
            return CreateImpl<V8TestProxy>();
        }

        public abstract UIntPtr GetNativeDigest(string value);

        public abstract Statistics GetStatistics();

        #region Nested type: Statistics

        internal sealed class Statistics
        {
            public ulong IsolateCount;
            public ulong ContextCount;
        }

        #endregion
    }
}
