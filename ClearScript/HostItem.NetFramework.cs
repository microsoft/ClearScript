// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            return HostObject.Wrap(enumerable.GetEnumerator().ToScriptableAsyncEnumerator(Engine), typeof(IScriptableAsyncEnumerator<T>));
        }

        private object CreateAsyncEnumerator()
        {
            if ((Target is HostObject) || (Target is IHostVariable) || (Target is IByRefArg))
            {
                if ((Target.InvokeTarget != null) && Target.Type.IsAssignableToGenericType(typeof(IEnumerable<>), out var typeArgs))
                {
                    var helpersHostItem = Wrap(Engine, typeof(ScriptableEnumerableHelpers<>).MakeGenericType(typeArgs).InvokeMember("HostType", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField, null, null, null), HostItemFlags.PrivateAccess);
                    if (MiscHelpers.Try(out var enumerator, () => ((IDynamic)helpersHostItem).InvokeMethod("GetScriptableAsyncEnumerator", this, Engine)))
                    {
                        return enumerator;
                    }
                }
                else if (BindSpecialTarget(out IEnumerable _))
                {
                    var helpersHostItem = Wrap(Engine, ScriptableEnumerableHelpers.HostType, HostItemFlags.PrivateAccess);
                    if (MiscHelpers.Try(out var enumerator, () => ((IDynamic)helpersHostItem).InvokeMethod("GetScriptableAsyncEnumerator", this, Engine)))
                    {
                        return enumerator;
                    }
                }
            }

            throw new NotSupportedException("The object is not async-enumerable");
        }

        #endregion

        #endregion
    }
}
