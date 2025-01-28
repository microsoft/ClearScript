// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class UnknownHelpers
    {
        public static IntPtr QueryInterface<T>(IntPtr pUnknown)
        {
            var iid = typeof(T).GUID;
            HResult.Check(Marshal.QueryInterface(pUnknown, ref iid, out var pInterface));
            return pInterface;
        }

        public static IntPtr QueryInterfaceNoThrow<T>(IntPtr pUnknown)
        {
            var iid = typeof(T).GUID;
            var result = Marshal.QueryInterface(pUnknown, ref iid, out var pInterface);
            return (result == HResult.S_OK) ? pInterface : IntPtr.Zero;
        }

        public static void ReleaseAndEmpty(ref IntPtr pInterface)
        {
            if (pInterface != IntPtr.Zero)
            {
                Marshal.Release(pInterface);
                pInterface = IntPtr.Zero;
            }
        }
    }
}
