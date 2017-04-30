// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;

namespace Microsoft.ClearScript.Util
{
    internal struct DisposedFlag
    {
        private bool disposed;

        public bool IsSet()
        {
            return disposed;
        }

        public bool Set()
        {
            return MiscHelpers.Exchange(ref disposed, true) == false;
        }
    }

    internal struct InterlockedDisposedFlag
    {
        private int disposed;

        public bool IsSet()
        {
            return disposed != 0;
        }

        public bool Set()
        {
            return Interlocked.Exchange(ref disposed, 1) == 0;
        }
    }
}
