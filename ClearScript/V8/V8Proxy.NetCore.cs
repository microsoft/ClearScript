// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.IJW;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        private static Assembly GetAssembly()
        {
            if (assembly == null)
            {
                assembly = LoadAssemblyWithIJWHostLibrary();
            }

            return assembly;
        }

        private static Assembly LoadAssemblyWithIJWHostLibrary()
        {
            var hIJWHostLibrary = IJWHostLibrary.Load();
            try
            {
                return LoadAssembly();
            }
            finally
            {
                NativeMethods.FreeLibrary(hIJWHostLibrary);
            }
        }
    }
}
