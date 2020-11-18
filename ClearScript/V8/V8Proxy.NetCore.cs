// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        [ThreadStatic] private static string loadLibraryErrorMessage;

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
