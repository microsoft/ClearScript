// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal static class AssemblyTable
    {
        public static string GetFullAssemblyName(string name)
        {
            return AssemblyHelpers.GetFullAssemblyName(name);
        }
    }
}
