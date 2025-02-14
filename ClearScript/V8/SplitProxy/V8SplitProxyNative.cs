// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
        internal static readonly IV8SplitProxyNative Instance = CreateInstance();

        public static string GetVersion()
        {
            try
            {
                return Instance.V8SplitProxyNative_GetVersion();
            }
            catch (EntryPointNotFoundException)
            {
                return "[unknown]";
            }
        }

        public static InvokeScope Invoke(out IV8SplitProxyNative instance)
        {
            var previousScheduledException = MiscHelpers.Exchange(ref V8SplitProxyManaged.ScheduledException, null);
            var previousMethodTable = Instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            instance = Instance;
            return new InvokeScope(previousScheduledException, previousMethodTable);
        }

        public static InvokeNoThrowScope InvokeNoThrow(out IV8SplitProxyNative instance)
        {
            var previousMethodTable = Instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable);
            instance = Instance;
            return new InvokeNoThrowScope(previousMethodTable);
        }

        private static void ThrowScheduledException()
        {
            if (V8SplitProxyManaged.ScheduledException != null)
            {
                throw V8SplitProxyManaged.ScheduledException;
            }
        }

        public readonly ref struct InvokeNoThrowScope
        {
            private readonly IntPtr previousMethodTable;

            public InvokeNoThrowScope(IntPtr previousMethodTable)
            {
                this.previousMethodTable = previousMethodTable;
            }

            public void Dispose()
            {
                Instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
            }
        }

        public readonly ref struct InvokeScope
        {
            private readonly Exception previousScheduledException;
            private readonly IntPtr previousMethodTable;

            public InvokeScope(Exception previousScheduledException, IntPtr previousMethodTable)
            {
                this.previousScheduledException = previousScheduledException;
                this.previousMethodTable = previousMethodTable;
            }

            public void Dispose()
            {
                ThrowScheduledException();
                Instance.V8SplitProxyManaged_SetMethodTable(previousMethodTable);
                V8SplitProxyManaged.ScheduledException = previousScheduledException;
            }
        }
    }
}
