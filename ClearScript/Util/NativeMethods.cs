// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SystemInfo
    {
        public ushort ProcessorArchitecture;
        public ushort Reserved;
        public uint PageSize;
        public IntPtr MinimumApplicationAddress;
        public IntPtr MaximumApplicationAddress;
        public IntPtr ActiveProcessorMask;
        public uint NumberOfProcessors;
        public uint ProcessorType;
        public uint AllocationGranularity;
        public ushort ProcessorLevel;
        public ushort ProcessorRevision;
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr LoadLibraryW(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string path
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hLibrary);

        [DllImport("ole32.dll", ExactSpelling = true)]
        public static extern uint CLSIDFromProgID(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string progID,
            [Out] out Guid clsid
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

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern void GetSystemInfo(out SystemInfo info);

        [DllImport("kernel32.dll")]
        public static extern void GetNativeSystemInfo(out SystemInfo info);
    }
}
