// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.Util
{
    internal class CoTaskMemBlock : IDisposable
    {
        public IntPtr Addr { get; private set; }

        public CoTaskMemBlock(int size)
        {
            Addr = Marshal.AllocCoTaskMem(size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Addr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(Addr);
                Addr = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CoTaskMemBlock()
        {
            Dispose(false);
        }
    }

    internal sealed class CoTaskMemArrayBlock : CoTaskMemBlock
    {
        private readonly int elementSize;
        private readonly int length;

        public CoTaskMemArrayBlock(int elementSize, int length)
            : base(elementSize * length)
        {
            this.elementSize = elementSize;
            this.length = length;
        }

        public IntPtr GetAddr(int index)
        {
            if ((index < 0) || (index >= length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Addr == IntPtr.Zero)
            {
                throw new ObjectDisposedException(ToString());
            }

            return GetAddrInternal(index);
        }

        private IntPtr GetAddrInternal(int index)
        {
            return Addr + (index * elementSize);
        }
    }

    internal sealed class CoTaskMemVariantBlock : CoTaskMemBlock
    {
        public CoTaskMemVariantBlock()
            : base(DispatchHelpers.VariantSize)
        {
            NativeMethods.VariantInit(Addr);
        }

        public CoTaskMemVariantBlock(object obj)
            : base(DispatchHelpers.VariantSize)
        {
            Marshal.GetNativeVariantForObject(obj, Addr);
        }

        #region CoTaskMemBlock overrides

        protected override void Dispose(bool disposing)
        {
            if (Addr != IntPtr.Zero)
            {
                NativeMethods.VariantClear(Addr);
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    internal sealed class CoTaskMemVariantArgsBlock : CoTaskMemBlock
    {
        private readonly int length;

        public CoTaskMemVariantArgsBlock(object[] args)
            : base(args.Length * DispatchHelpers.VariantSize)
        {
            length = args.Length;
            for (var index = 0; index < length; index++)
            {
                Marshal.GetNativeVariantForObject(args[index], GetAddrInternal(length - 1 - index));
            }
        }

        public IntPtr GetAddr(int index)
        {
            if ((index < 0) || (index >= length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Addr == IntPtr.Zero)
            {
                throw new ObjectDisposedException(ToString());
            }

            return GetAddrInternal(length - 1 - index);
        }

        private IntPtr GetAddrInternal(int index)
        {
            return Addr + (index * DispatchHelpers.VariantSize);
        }

        #region CoTaskMemBlock overrides

        protected override void Dispose(bool disposing)
        {
            if (Addr != IntPtr.Zero)
            {
                for (var index = 0; index < length; index++)
                {
                    NativeMethods.VariantClear(GetAddrInternal(index));
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    internal sealed class CoTaskMemVariantArgsByRefBlock : CoTaskMemBlock
    {
        private readonly object[] args;

        public CoTaskMemVariantArgsByRefBlock(object[] args)
            : base(args.Length * 2 * DispatchHelpers.VariantSize)
        {
            this.args = args;
            for (var index = 0; index < args.Length; index++)
            {
                var pArg = GetAddrInternal(args.Length + index);
                Marshal.GetNativeVariantForObject(args[index], pArg);

                var pArgRef = GetAddrInternal(args.Length - 1 - index);
                NativeMethods.VariantInit(pArgRef);
                Marshal.WriteInt16(pArgRef, 0, 0x400C /*VT_BYREF|VT_VARIANT*/);
                Marshal.WriteIntPtr(pArgRef, sizeof(ushort) * 4, pArg);
            }
        }
        
        public IntPtr GetAddr(int index)
        {
            if ((index < 0) || (index >= args.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Addr == IntPtr.Zero)
            {
                throw new ObjectDisposedException(ToString());
            }

            return GetAddrInternal(args.Length - 1 - index);
        }

        private IntPtr GetAddrInternal(int index)
        {
            return Addr + (index * DispatchHelpers.VariantSize);
        }

        #region CoTaskMemBlock overrides

        protected override void Dispose(bool disposing)
        {
            if (Addr != IntPtr.Zero)
            {
                for (var index = 0; index < args.Length; index++)
                {
                    var pArg = GetAddrInternal(args.Length + index);
                    args[index] = MiscHelpers.GetObjectForVariant(pArg);
                    NativeMethods.VariantClear(pArg);
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
