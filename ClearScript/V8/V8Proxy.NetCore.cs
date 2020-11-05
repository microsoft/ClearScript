// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        [ThreadStatic] private static string loadLibraryErrorMessage;

        private static IntPtr LoadNativeAssembly()
        {
            string platform;
            string architecture;
            string extension;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "win";
                extension = "dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "linux";
                extension = "so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "osx";
                extension = "dylib";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform");
            }

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                architecture = "x64";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                architecture = "x86";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                architecture = "arm";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                architecture = "arm64";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            return LoadNativeLibrary("ClearScriptV8", platform, architecture, extension);
        }

        private static IntPtr LoadLibrary(string path)
        {
            try
            {
                return NativeLibrary.Load(path);
            }
            catch (Exception exception)
            {
                loadLibraryErrorMessage = exception.Message;
                return IntPtr.Zero;
            }
        }

        private static void FreeLibrary(IntPtr hLibrary)
        {
            NativeLibrary.Free(hLibrary);
        }

        private static string GetLoadLibraryErrorMessage()
        {
            return loadLibraryErrorMessage;
        }
    }
}
