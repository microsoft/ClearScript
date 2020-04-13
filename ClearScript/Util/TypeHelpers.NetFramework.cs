// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static partial class TypeHelpers
    {
        private static string GetFullTypeName(string name, string assemblyName, bool useAssemblyName, int typeArgCount)
        {
            var fullTypeName = name;

            if (typeArgCount > 0)
            {
                fullTypeName += MiscHelpers.FormatInvariant("`{0}", typeArgCount);
            }

            if (useAssemblyName)
            {
                fullTypeName += MiscHelpers.FormatInvariant(", {0}", AssemblyTable.GetFullAssemblyName(assemblyName));
            }

            return fullTypeName;
        }

        public static IntPtr GetTypeInfo(this Type type)
        {
            return Marshal.GetITypeInfoForType(type);
        }
    }
}
