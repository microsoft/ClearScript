// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyNative
    {
        private static IV8SplitProxyNative CreateInstance()
        {
            if (MiscHelpers.ProcessorArchitectureIsIntel())
            {
                if (Environment.Is64BitProcess)
                {
                    return new WinX64Impl();
                }

                return new WinX86Impl();
            }

            if (MiscHelpers.ProcessorArchitectureIsArm())
            {
                if (Environment.Is64BitProcess)
                {
                    return new WinArm64Impl();
                }

                return new WinArmImpl();
            }

            throw new PlatformNotSupportedException("Unsupported machine architecture");
        }
    }
}
