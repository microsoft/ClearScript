// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.ClearScript.Util.COM
{
    internal static partial class TypeInfoHelpers
    {
        private static readonly ConcurrentDictionary<Guid, Type> managedTypeMap = new();

        public static Type GetManagedType(this ITypeInfo typeInfo)
        {
            var guid = typeInfo.GetGuid();
            return (guid == Guid.Empty) ? null : managedTypeMap.GetOrAdd(guid, _ =>
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
            });
        }
    }
}
