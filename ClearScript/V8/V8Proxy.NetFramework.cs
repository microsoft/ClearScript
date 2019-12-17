// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy
    {
        private static Assembly GetAssembly()
        {
            if (assembly == null)
            {
                assembly = LoadAssembly();
            }

            return assembly;
        }
    }
}
