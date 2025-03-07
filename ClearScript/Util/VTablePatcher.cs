// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    [SuppressMessage("ReSharper", "CommentTypo", Justification = "This class uses comments to show machine code disassembly.")]
    internal abstract class VTablePatcher
    {
        private static readonly HashSet<IntPtr> patchedVTables = new();
        private static IntPtr hHeap;

        public static VTablePatcher GetInstance()
        {
            return Environment.Is64BitProcess ? VTablePatcher64.Instance : VTablePatcher32.Instance;
        }

        public static object PatchLock => patchedVTables;

        public abstract void PatchDispatchEx(IntPtr pDispatchEx);

        private static void ApplyVTablePatches(IntPtr pInterface, params VTablePatch[] patches)
        {
            lock (PatchLock)
            {
                var pVTable = Marshal.ReadIntPtr(pInterface);
                if (patchedVTables.Add(pVTable))
                {
                    EnsureHeap();

                    foreach (var patch in patches)
                    {
                        var pSlot = pVTable + patch.SlotIndex * IntPtr.Size;
                        var pTarget = VTableHelpers.ReadMethodPtr(pSlot);

                        var thunkSize = patch.ThunkBytes.Length;
                        var pThunk = NativeMethods.HeapAlloc(hHeap, 0, (UIntPtr)thunkSize);
                        if (pThunk != IntPtr.Zero)
                        {
                            for (var index = 0; index < thunkSize; index++)
                            {
                                Marshal.WriteByte(pThunk + index, patch.ThunkBytes[index]);
                            }

                            Marshal.WriteIntPtr(pThunk + patch.TargetOffset, pTarget);
                            if (!VTableHelpers.WriteMethodPtr(pSlot, pThunk))
                            {
                                NativeMethods.HeapFree(hHeap, 0, pThunk);
                            }
                        }
                    }
                }
            }
        }

        private static void EnsureHeap()
        {
            if (hHeap == IntPtr.Zero)
            {
                hHeap = NativeMethods.HeapCreate(0x00040001 /*HEAP_CREATE_ENABLE_EXECUTE|HEAP_NO_SERIALIZE*/, UIntPtr.Zero, UIntPtr.Zero);
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

        private sealed class VTablePatch
        {
            public int SlotIndex;
            public byte[] ThunkBytes;
            public int TargetOffset;
        }

        #endregion
    }
}
