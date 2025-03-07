// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
        private static readonly IV8SplitProxyNative instance = CreateInstance();

        public static string GetVersion()
        {
            try
            {
                return instance.V8SplitProxyNative_GetVersion();
            }
            catch (EntryPointNotFoundException)
            {
                return "[unknown]";
            }
        }

        public static void Invoke(Action<IV8SplitProxyNative> action)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            try
            {
                InvokeNoThrow(action);
                ThrowScheduledException();
            }
            finally
            {
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static void Invoke<TArg>(Action<IV8SplitProxyNative, TArg> action, in TArg arg)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            try
            {
                InvokeNoThrow(action, arg);
                ThrowScheduledException();
            }
            finally
            {
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static TResult Invoke<TResult>(Func<IV8SplitProxyNative, TResult> func)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            try
            {
                var result = InvokeNoThrow(func);
                ThrowScheduledException();
                return result;
            }
            finally
            {
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static TResult Invoke<TResult, TArg>(Func<IV8SplitProxyNative, TArg, TResult> func, in TArg arg)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            try
            {
                var result = InvokeNoThrow(func, arg);
                ThrowScheduledException();
                return result;
            }
            finally
            {
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static void InvokeNoThrow(Action<IV8SplitProxyNative> action)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                InvokeRaw(action);
            }
            finally
            {
                if (previousMethodTable != V8SplitProxyManaged.MethodTable)
                {
                    instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                }
            }
        }

        public static void InvokeNoThrow<TArg>(Action<IV8SplitProxyNative, TArg> action, in TArg arg)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                InvokeRaw(action, arg);
            }
            finally
            {
                if (previousMethodTable != V8SplitProxyManaged.MethodTable)
                {
                    instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                }
            }
        }

        public static TResult InvokeNoThrow<TResult>(Func<IV8SplitProxyNative, TResult> func)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                return InvokeRaw(func);
            }
            finally
            {
                if (previousMethodTable != V8SplitProxyManaged.MethodTable)
                {
                    instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                }
            }
        }

        public static TResult InvokeNoThrow<TResult, TArg>(Func<IV8SplitProxyNative, TArg, TResult> func, in TArg arg)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                return InvokeRaw(func, arg);
            }
            finally
            {
                if (previousMethodTable != V8SplitProxyManaged.MethodTable)
                {
                    instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                }
            }
        }

        public static void InvokeRaw(Action<IV8SplitProxyNative> action) => action(instance);

        public static void InvokeRaw<TArg>(Action<IV8SplitProxyNative, TArg> action, in TArg arg) => action(instance, arg);

        public static TResult InvokeRaw<TResult>(Func<IV8SplitProxyNative, TResult> func) => func(instance);

        public static TResult InvokeRaw<TResult, TArg>(Func<IV8SplitProxyNative, TArg, TResult> func, in TArg arg) => func(instance, arg);

        private static void ThrowScheduledException()
        {
            if (V8SplitProxyManaged.ScheduledException is not null)
            {
                throw V8SplitProxyManaged.ScheduledException;
            }
        }
    }
}
