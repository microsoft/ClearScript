// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class VTableHelpers
    {
        public static T GetMethodDelegate<T>(IntPtr pInterface, int methodIndex) where T : class
        {
            var pMethod = GetMethodPtr(pInterface, methodIndex);
            return Marshal.GetDelegateForFunctionPointer(pMethod, typeof(T)) as T;
        }

        public static IntPtr GetMethodPtr(IntPtr pInterface, int methodIndex)
        {
            var pVTable = Marshal.ReadIntPtr(pInterface);
            return Marshal.ReadIntPtr(pVTable + methodIndex * IntPtr.Size);
        }

        public static IntPtr ReadMethodPtr(IntPtr pSlot)
        {
            return Marshal.ReadIntPtr(pSlot);
        }

        public static T SetMethodDelegate<T>(IntPtr pInterface, int methodIndex, T del) where T : class
        {
            var pMethod = Marshal.GetFunctionPointerForDelegate((Delegate)(object)del);
            SetMethodPtr(pInterface, methodIndex, pMethod);
            return del;
        }

        public static IntPtr SetMethodPtr(IntPtr pInterface, int methodIndex, IntPtr pMethod)
        {
            var pVTable = Marshal.ReadIntPtr(pInterface);
            WriteMethodPtr(pVTable + methodIndex * IntPtr.Size, pMethod);
            return pMethod;
        }

        public static bool WriteMethodPtr(IntPtr pSlot, IntPtr pMethod)
        {
            if (NativeMethods.VirtualProtect(pSlot, (UIntPtr)IntPtr.Size, 0x04 /*PAGE_READWRITE*/, out var oldProtect))
            {
                Marshal.WriteIntPtr(pSlot, pMethod);
                NativeMethods.VirtualProtect(pSlot, (UIntPtr)IntPtr.Size, oldProtect, out oldProtect);
                return true;
            }

            return false;
        }
    }
}
