// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;

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

        private object CreateAsyncEnumerator<T>(IEnumerable<T> enumerable)
        {
            return HostObject.Wrap(enumerable.GetEnumerator().ToAsyncEnumerator(Engine), typeof(IAsyncEnumeratorPromise<T>));
        }

        private object CreateAsyncEnumerator()
        {
            if (BindSpecialTarget(out IEnumerable _))
            {
                var enumerableHelpersHostItem = Wrap(Engine, EnumerableHelpers.HostType, HostItemFlags.PrivateAccess);
                if (MiscHelpers.Try(out var enumerator, () => ((IDynamic)enumerableHelpersHostItem).InvokeMethod("GetAsyncEnumerator", this, Engine)))
                {
                    return enumerator;
                }
            }

            throw new NotSupportedException("The object is not async-enumerable");
        }

        #endregion

        #endregion
    }
}
