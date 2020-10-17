// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading;

namespace Microsoft.ClearScript.Util
{
    internal sealed class OneWayFlag
    {
        private bool isSet;

        public bool IsSet => isSet;

        public bool Set()
        {
            return MiscHelpers.Exchange(ref isSet, true) == false;
        }
    }

    internal sealed class InterlockedOneWayFlag
    {
        private int isSet;

        public bool IsSet => isSet != 0;

        public bool Set()
        {
            return Interlocked.Exchange(ref isSet, 1) == 0;
        }
    }
}
