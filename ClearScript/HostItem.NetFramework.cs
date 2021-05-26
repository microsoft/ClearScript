// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region initialization

        private static HostItem Create(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            return TargetSupportsExpandoMembers(target, flags) ? new ExpandoHostItem(engine, target, flags) : new HostItem(engine, target, flags);
        }

        #endregion

        #region internal members

        #region member invocation

        // ReSharper disable once UnusedParameter.Local
        private static object CreateAsyncEnumerator<T>(IEnumerable<T> enumerable)
        {
            throw new PlatformNotSupportedException("Async enumerators are not supported on this platform");
        }

        private object CreateAsyncEnumerator()
        {
            throw new PlatformNotSupportedException("Async enumerators are not supported on this platform");
        }

        #endregion

        #endregion
    }
}
