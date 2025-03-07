// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class StructHelpers
    {
        public delegate void GetStruct(out IntPtr pStruct);

        public delegate void ReleaseStruct(IntPtr pStruct);

        public static IScope<T> CreateScope<T>(GetStruct get, ReleaseStruct release)
        {
            get(out var pStruct);
            return Scope.Create(() => (T)Marshal.PtrToStructure(pStruct, typeof(T)), _ => release(pStruct));
        }

        public static IEnumerable<T> GetStructsFromArray<T>(IntPtr pStructs, int count)
        {
            var size = Marshal.SizeOf(typeof(T));
            for (var pStruct = pStructs; count > 0; count--)
            {
                yield return (T)Marshal.PtrToStructure(pStruct, typeof(T));
                pStruct += size;
            }
        }
    }
}
