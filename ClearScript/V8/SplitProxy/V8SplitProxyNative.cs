// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
        private static readonly IV8SplitProxyNative instance = CreateInstance();

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

        private static bool IsOSPlatform(string os)
        {
            if (os == "Android")
            {
                return HostSettings.IsAndroid;
            }

            if (os == "Windows")
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            }

            if (os == "Linux")
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            }

            if (os == "OSX")
            {
                return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            }

            return false;
        }


        private static bool IsArchitecture(string os, string arch)
        {
            var architecture = RuntimeInformation.ProcessArchitecture;

            if (os == "Android")
            {
                if (arch == "X86" || arch == "Arm")
                {
                    return architecture == Architecture.X86 || architecture == Architecture.Arm;
                }
                else if (arch == "X64" || arch == "Arm64")
                {
                    return architecture == Architecture.X64 || architecture == Architecture.Arm64;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (arch == "X86")
                {
                    return architecture == Architecture.X86;
                }
                else if (arch == "X64")
                {
                    return architecture == Architecture.X64;
                }
                else if (arch == "Arm")
                {
                    return architecture == Architecture.Arm;
                }
                else if (arch == "Arm64")
                {
                    return architecture == Architecture.Arm64;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
