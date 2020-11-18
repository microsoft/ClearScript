// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class NativeMethods
    {
        public static IntPtr LoadLibraryW(string path)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.LoadLibraryW(path);
            }

            throw new PlatformNotSupportedException();
        }

        public static bool FreeLibrary(IntPtr hLibrary)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.FreeLibrary(hLibrary);
            }

            throw new PlatformNotSupportedException();
        }

        public static uint CLSIDFromProgID(string progID, out Guid clsid)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.CLSIDFromProgID(progID, out clsid);
            }

            throw new PlatformNotSupportedException();
        }

        public static uint ProgIDFromCLSID(ref Guid clsid, out string progID)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.ProgIDFromCLSID(ref clsid, out progID);
            }

            throw new PlatformNotSupportedException();
        }

        public static uint CoCreateInstance(ref Guid clsid, IntPtr pOuter, uint clsContext, ref Guid iid, out IntPtr pInterface)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.CoCreateInstance(ref clsid, pOuter, clsContext, ref iid, out pInterface);
            }

            throw new PlatformNotSupportedException();
        }

        public static void VariantInit(IntPtr pVariant)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                NativeWindowsMethods.VariantInit(pVariant);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static uint VariantClear(IntPtr pVariant)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.VariantClear(pVariant);
            }

            throw new PlatformNotSupportedException();
        }

        public static IntPtr HeapCreate(uint options, UIntPtr initialSize, UIntPtr maximumSize)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.HeapCreate(options, initialSize, maximumSize);
            }

            throw new PlatformNotSupportedException();
        }

        public static IntPtr HeapAlloc(IntPtr hHeap, uint flags, UIntPtr size)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.HeapAlloc(hHeap, flags, size);
            }

            throw new PlatformNotSupportedException();
        }

        public static bool HeapFree(IntPtr hHeap, uint flags, IntPtr pBlock)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.HeapFree(hHeap, flags, pBlock);
            }

            throw new PlatformNotSupportedException();
        }

        public static bool HeapDestroy(IntPtr hHeap)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.HeapDestroy(hHeap);
            }

            throw new PlatformNotSupportedException();
        }

        public static bool VirtualProtect(IntPtr pBlock, UIntPtr size, uint newProtect, out uint oldProtect)
        {
            if (MiscHelpers.PlatformIsWindows())
            {
                return NativeWindowsMethods.VirtualProtect(pBlock, size, newProtect, out oldProtect);
            }

            throw new PlatformNotSupportedException();
        }

        #region Nested type: WindowsNativeMethods

        private static class NativeWindowsMethods
        {
            // ReSharper disable MemberHidesStaticFromOuterClass

            [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr LoadLibraryW(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string path
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(
                [In] IntPtr hLibrary
            );

            [DllImport("ole32.dll", ExactSpelling = true)]
            public static extern uint CLSIDFromProgID(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string progID,
                [Out] out Guid clsid
            );

            [DllImport("ole32.dll")]
            public static extern uint ProgIDFromCLSID(
                [In] ref Guid clsid,
                [Out] [MarshalAs(UnmanagedType.LPWStr)] out string progID
            );
            
            [DllImport("ole32.dll", ExactSpelling = true)]
            public static extern uint CoCreateInstance(
                [In] ref Guid clsid,
                [In] IntPtr pOuter,
                [In] uint clsContext,
                [In] ref Guid iid,
                [Out] out IntPtr pInterface
            );

            [DllImport("oleaut32.dll", ExactSpelling = true)]
            public static extern void VariantInit(
                [In] IntPtr pVariant
            );

            [DllImport("oleaut32.dll", ExactSpelling = true)]
            public static extern uint VariantClear(
                [In] IntPtr pVariant
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr HeapCreate(
                [In] uint options,
                [In] UIntPtr initialSize,
                [In] UIntPtr maximumSize
            );

            [DllImport("kernel32.dll", SetLastError = false)]
            public static extern IntPtr HeapAlloc(
                [In] IntPtr hHeap,
                [In] uint flags,
                [In] UIntPtr size
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool HeapFree(
                [In] IntPtr hHeap,
                [In] uint flags,
                [In] IntPtr pBlock
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool HeapDestroy(
                [In] IntPtr hHeap
            );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool VirtualProtect(
                [In] IntPtr pBlock,
                [In] UIntPtr size,
                [In] uint newProtect,
                [Out] out uint oldProtect
            );

            // ReSharper restore MemberHidesStaticFromOuterClass
        }
    }
        
    #endregion
}
