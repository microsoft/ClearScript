// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using System;

namespace Microsoft.ClearScript.Util
{
    internal static class StructPtr
    {
        public static StructPtr<T> FromRef<T>(ref T value) where T : struct => new(ref value);
    }

    internal readonly struct StructPtr<T> where T : struct
    {
        private readonly IntPtr ptr;

        public unsafe StructPtr(ref T value) => ptr = (IntPtr)Unsafe.AsPointer(ref value);

        public unsafe ref T AsRef() => ref Unsafe.AsRef<T>(ptr.ToPointer());
    }
}
