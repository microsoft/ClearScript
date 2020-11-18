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

        private static IV8SplitProxyNative CreateInstance()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return new WinX64Impl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                {
                    return new WinX86Impl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                {
                    return new WinArmImpl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    return new WinArm64Impl();
                }

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return new LinuxX64Impl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                {
                    return new LinuxArmImpl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    return new LinuxArm64Impl();
                }

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return new MacX64Impl();
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    return new MacArm64Impl();
                }

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            throw new PlatformNotSupportedException("Unsupported operating system");
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
