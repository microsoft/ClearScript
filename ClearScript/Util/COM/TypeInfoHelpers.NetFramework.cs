// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.ClearScript.Util.COM
{
    internal static partial class TypeInfoHelpers
    {
        public static Type GetManagedType(this ITypeInfo typeInfo)
        {
            var pTypeInfo = Marshal.GetComInterfaceForObject(typeInfo, typeof(ITypeInfo));
            try
            {
                return Marshal.GetTypeForITypeInfo(pTypeInfo);
            }
            finally
            {
                Marshal.Release(pTypeInfo);
            }
        }
    }
}
