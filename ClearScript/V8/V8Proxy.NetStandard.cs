// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        private static IntPtr LoadLibrary(string path)
        {
            return NativeMethods.LoadLibrary(path);
        }

        private static void FreeLibrary(IntPtr hLibrary)
        {
            NativeMethods.FreeLibrary(hLibrary);
        }

        private static string GetLoadLibraryErrorMessage()
        {
            return NativeMethods.GetLoadLibraryErrorMessage();
        }
    }
}
