// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;

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
                if (MiscHelpers.Try(out var assembly, static assemblyName => Assembly.Load(AssemblyTable.GetFullAssemblyName(assemblyName)), assemblyName))
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (MiscHelpers.Try(out var result, static ctx => ctx.assembly.GetType(ctx.fullTypeName).AssemblyQualifiedName, (assembly, fullTypeName)))
                    {
                        return result;
                    }
                }

                fullTypeName += MiscHelpers.FormatInvariant(", {0}", AssemblyTable.GetFullAssemblyName(assemblyName));
            }

            return fullTypeName;
        }

        public static IntPtr GetTypeInfo(this Type type)
        {
            return IntPtr.Zero;
        }
    }
}
