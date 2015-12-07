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
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal abstract class VTablePatcher
    {
        private static readonly object dataLock = new object();
        private static readonly HashSet<IntPtr> patchedVTables = new HashSet<IntPtr>();
        private static IntPtr hHeap;

        public static VTablePatcher GetInstance()
        {
            return Environment.Is64BitProcess ? VTablePatcher64.Instance : VTablePatcher32.Instance;
        }

        public abstract void PatchDispatchEx(IntPtr pDispatchEx);

        private static void ApplyVTablePatches(IntPtr pInterface, params VTablePatch[] patches)
        {
            lock (dataLock)
            {
                var pVTable = Marshal.ReadIntPtr(pInterface);
                if (!patchedVTables.Contains(pVTable))
                {
                    patchedVTables.Add(pVTable);
                    EnsureHeap();

                    foreach (var patch in patches)
                    {
                        var pSlot = pVTable + patch.SlotIndex * IntPtr.Size;
                        var pTarget = Marshal.ReadIntPtr(pSlot);

                        uint oldProtect;
                        if (NativeMethods.VirtualProtect(pSlot, (UIntPtr)IntPtr.Size, 0x04 /*PAGE_READWRITE*/, out oldProtect))
                        {
                            var thunkSize = patch.ThunkBytes.Length;
                            var pThunk = NativeMethods.HeapAlloc(hHeap, 0, (UIntPtr)thunkSize);
                            for (var index = 0; index < thunkSize; index++)
                            {
                                Marshal.WriteByte(pThunk + index, patch.ThunkBytes[index]);
                            }

                            Marshal.WriteIntPtr(pThunk + patch.TargetOffset, pTarget);
                            Marshal.WriteIntPtr(pSlot, pThunk);
                            NativeMethods.VirtualProtect(pSlot, (UIntPtr)IntPtr.Size, oldProtect, out oldProtect);
                        }
                    }
                }
            }
        }

        private static void EnsureHeap()
        {
            if (hHeap == IntPtr.Zero)
            {
                hHeap = NativeMethods.HeapCreate(0x00040005 /*HEAP_CREATE_ENABLE_EXECUTE|HEAP_GENERATE_EXCEPTIONS|HEAP_NO_SERIALIZE*/, UIntPtr.Zero, UIntPtr.Zero);
                if (hHeap == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }
        }

        #region Nested type: VTablePatcher32

        private sealed class VTablePatcher32 : VTablePatcher
        {
            public static readonly VTablePatcher Instance = new VTablePatcher32();

            private VTablePatcher32()
            {
            }

            public override void PatchDispatchEx(IntPtr pDispatchEx)
            {
                // JScript in Standards Mode extends the IDispatchEx contract slightly in order to
                // pass extra data to the host. This confuses the CLR's IDispatchEx implementation.
                // The vtable patches below sanitize the arguments before passing them through.

                ApplyVTablePatches(
                    pDispatchEx,
                    new VTablePatch
                    {
                        SlotIndex = 7,                                                      // IDispatchEx::GetDispID()
                        ThunkBytes = new byte[] {                                           //-------------------------
                            0x55,                                                           // push ebp
                            0x8B, 0xEC,                                                     // mov ebp,esp
                            0x81, 0x65, 0x10, 0xFF, 0xFF, 0xFF, 0x0F,                       // and dword ptr [grfdex],0FFFFFFFh
                            0xB8, 0x0D, 0xF0, 0xAD, 0xBA,                                   // mov eax,0BAADF00Dh <- Target
                            0x5D,                                                           // pop ebp
                            0xFF, 0xE0                                                      // jmp eax
                        },
                        TargetOffset = 11
                    },
                    new VTablePatch
                    {
                        SlotIndex = 9,                                                      // IDispatchEx::DeleteMemberByName()
                        ThunkBytes = new byte[] {                                           // ---------------------------------
                            0x55,                                                           // push ebp
                            0x8B, 0xEC,                                                     // mov ebp,esp
                            0x8B, 0x45, 0x10,                                               // mov eax,dword ptr [grfdex]
                            0x25, 0xFF, 0xFF, 0xFF, 0x0F,                                   // and eax,0FFFFFFFh
                            0x89, 0x45, 0x10,                                               // mov dword ptr [grfdex],eax
                            0xB8, 0x0D, 0xF0, 0xAD, 0xBA,                                   // mov eax,0BAADF00Dh <- Target
                            0x5D,                                                           // pop ebp
                            0xFF, 0xE0                                                      // jmp eax
                        },
                        TargetOffset = 15
                    },
                    new VTablePatch
                    {
                        SlotIndex = 13,                                                     // IDispatchEx::GetNextDispID()
                        ThunkBytes = new byte[] {                                           // ----------------------------
                            0x55,                                                           // push ebp
                            0x8B, 0xEC,                                                     // mov ebp,esp
                            0x81, 0x65, 0x0C, 0xFF, 0xFF, 0xFF, 0x0F,                       // and dword ptr [grfdex],0FFFFFFFh
                            0xB8, 0x0D, 0xF0, 0xAD, 0xBA,                                   // mov eax,0BAADF00Dh <- Target
                            0x5D,                                                           // pop ebp
                            0xFF, 0xE0                                                      // jmp eax
                        },
                        TargetOffset = 11
                    }
                );
            }
        }

        #endregion

        #region Nested type: VTablePatcher64

        private sealed class VTablePatcher64 : VTablePatcher
        {
            public static readonly VTablePatcher Instance = new VTablePatcher64();

            private VTablePatcher64()
            {
            }

            public override void PatchDispatchEx(IntPtr pDispatchEx)
            {
                // JScript in Standards Mode extends the IDispatchEx contract slightly in order to
                // pass extra data to the host. This confuses the CLR's IDispatchEx implementation.
                // The vtable patches below sanitize the arguments before passing them through.

                ApplyVTablePatches(
                    pDispatchEx,
                    new VTablePatch
                    {
                        SlotIndex = 7,                                                      // IDispatchEx::GetDispID()
                        ThunkBytes = new byte[] {                                           //-------------------------
                            0x41, 0x81, 0xE0, 0xFF, 0xFF, 0xFF, 0x0F,                       // and r8d,0FFFFFFFh
                            0x48, 0xB8, 0x0D, 0xF0, 0xAD, 0xBA, 0x0D, 0xF0, 0xAD, 0xBA,     // mov rax,0BAADF00DBAADF00Dh <- Target
                            0x48, 0xFF, 0xE0                                                // jmp rax
                        },
                        TargetOffset = 9
                    },
                    new VTablePatch {
                        SlotIndex = 9,                                                      // IDispatchEx::DeleteMemberByName()
                        ThunkBytes = new byte[] {                                           // ---------------------------------
                            0x41, 0x81, 0xE0, 0xFF, 0xFF, 0xFF, 0x0F,                       // and r8d,0FFFFFFFh
                            0x48, 0xB8, 0x0D, 0xF0, 0xAD, 0xBA, 0x0D, 0xF0, 0xAD, 0xBA,     // mov rax,0BAADF00DBAADF00Dh <- Target
                            0x48, 0xFF, 0xE0                                                // jmp rax
                        },
                        TargetOffset = 9
                    },
                    new VTablePatch {
                        SlotIndex = 13,                                                     // IDispatchEx::GetNextDispID()
                        ThunkBytes = new byte[] {                                           // ----------------------------
                            0x81, 0xE2, 0xFF, 0xFF, 0xFF, 0x0F,                             // and edx,0FFFFFFFh
                            0x48, 0xB8, 0x0D, 0xF0, 0xAD, 0xBA, 0x0D, 0xF0, 0xAD, 0xBA,     // mov rax,0BAADF00DBAADF00Dh <- Target
                            0x48, 0xFF, 0xE0                                                // jmp rax
                        },
                        TargetOffset = 8
                    }
                );
            }
        }

        #endregion

        #region Nested type: VTablePatch

        private class VTablePatch
        {
            public int SlotIndex;
            public byte[] ThunkBytes;
            public int TargetOffset;
        }

        #endregion
    }
}
