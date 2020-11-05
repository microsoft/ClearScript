// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        private static IntPtr LoadNativeAssembly()
        {
            string architecture;

            if (MiscHelpers.ProcessorArchitectureIsIntel())
            {
                architecture = Environment.Is64BitProcess ? "x64" : "x86";
            }
            else if (MiscHelpers.ProcessorArchitectureIsArm())
            {
                architecture = Environment.Is64BitProcess ? "arm64" : "arm";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported processor architecture");
            }

            return LoadNativeLibrary("ClearScriptV8", "win", architecture, "dll");
        }

        private static IntPtr LoadLibrary(string path)
        {
            return NativeMethods.LoadLibraryW(path);
        }

        private static void FreeLibrary(IntPtr hLibrary)
        {
            NativeMethods.FreeLibrary(hLibrary);
        }

        private static string GetLoadLibraryErrorMessage()
        {
            return new Win32Exception().Message;
        }
    }
}
