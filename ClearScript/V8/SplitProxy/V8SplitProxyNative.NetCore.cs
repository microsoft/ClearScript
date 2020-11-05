// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
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

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return new MacX64Impl();
                }

                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            throw new PlatformNotSupportedException("Unsupported operating system");
        }
    }
}
