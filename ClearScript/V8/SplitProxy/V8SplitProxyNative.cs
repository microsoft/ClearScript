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
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                action(instance);
                ThrowScheduledException();
            }
            finally
            {
                instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static T Invoke<T>(Func<IV8SplitProxyNative, T> func)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                var result = func(instance);
                ThrowScheduledException();
                return result;
            }
            finally
            {
                instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }

        public static void InvokeNoThrow(Action<IV8SplitProxyNative> action)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                action(instance);
            }
            finally
            {
                instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
            }
        }

        public static T InvokeNoThrow<T>(Func<IV8SplitProxyNative, T> func)
        {
            var previousMethodTable = instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            try
            {
                return func(instance);
            }
            finally
            {
                instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
            }
        }

        private static void ThrowScheduledException()
        {
            if (V8SplitProxyManaged.ScheduledException != null)
            {
                throw V8SplitProxyManaged.ScheduledException;
            }
        }
    }
}
