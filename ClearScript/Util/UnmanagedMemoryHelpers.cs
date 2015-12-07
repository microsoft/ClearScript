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
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class UnmanagedMemoryHelpers
    {
        private delegate ulong ReadArrayFromUnmanagedMemoryHandler(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex);
        private delegate ulong WriteArrayToUnmanagedMemoryHandler(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination);

        private static readonly Dictionary<Type, ReadArrayFromUnmanagedMemoryHandler> readArrayFromUnmanagedMemoryHandlerMap = new Dictionary<Type, ReadArrayFromUnmanagedMemoryHandler>
        {
            { typeof(byte), ReadByteArrayFromUnmanagedMemory },
            { typeof(sbyte), ReadSByteArrayFromUnmanagedMemory },
            { typeof(ushort), ReadUInt16ArrayFromUnmanagedMemory },
            { typeof(char), ReadCharArrayFromUnmanagedMemory },
            { typeof(short), ReadInt16ArrayFromUnmanagedMemory },
            { typeof(uint), ReadUInt32ArrayFromUnmanagedMemory },
            { typeof(int), ReadInt32ArrayFromUnmanagedMemory },
            { typeof(float), ReadSingleArrayFromUnmanagedMemory },
            { typeof(double), ReadDoubleArrayFromUnmanagedMemory }
        };

        private static readonly Dictionary<Type, WriteArrayToUnmanagedMemoryHandler> writeArrayToUnmanagedMemoryHandlerMap = new Dictionary<Type, WriteArrayToUnmanagedMemoryHandler>
        {
            { typeof(byte), WriteByteArrayToUnmanagedMemory },
            { typeof(sbyte), WriteSByteArrayToUnmanagedMemory },
            { typeof(ushort), WriteUInt16ArrayToUnmanagedMemory },
            { typeof(char), WriteCharArrayToUnmanagedMemory },
            { typeof(short), WriteInt16ArrayToUnmanagedMemory },
            { typeof(uint), WriteUInt32ArrayToUnmanagedMemory },
            { typeof(int), WriteInt32ArrayToUnmanagedMemory },
            { typeof(float), WriteSingleArrayToUnmanagedMemory },
            { typeof(double), WriteDoubleArrayToUnmanagedMemory }
        };

        public static ulong Copy<T>(IntPtr pSource, ulong length, T[] destination, ulong destinationIndex)
        {
            return Copy(typeof(T), pSource, length, destination, destinationIndex);
        }

        public static ulong Copy<T>(T[] source, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            return Copy(typeof(T), source, sourceIndex, length, pDestination);
        }

        private static ulong Copy(Type type, IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            ReadArrayFromUnmanagedMemoryHandler handler;
            if (readArrayFromUnmanagedMemoryHandlerMap.TryGetValue(type, out handler))
            {
                return handler(pSource, length, destinationArray, destinationIndex);
            }

            throw new NotSupportedException("Unsupported unmanaged data transfer operation");
        }

        private static ulong Copy(Type type, Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            WriteArrayToUnmanagedMemoryHandler handler;
            if (writeArrayToUnmanagedMemoryHandlerMap.TryGetValue(type, out handler))
            {
                return handler(sourceArray, sourceIndex, length, pDestination);
            }

            throw new NotSupportedException("Unsupported unmanaged data transfer operation");
        }

        private static unsafe ulong ReadByteArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (byte[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (byte* pDest = destination)
                {
                    var pSrc = (byte*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadSByteArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (sbyte[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            fixed (sbyte* pDest = destination)
            {
                var pSrc = (sbyte*)pSource;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[destinationIndex + index] = pSrc[index];
                }
            }

            return length;
        }

        private static unsafe ulong ReadUInt16ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (ushort[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            fixed (ushort* pDest = destination)
            {
                var pSrc = (ushort*)pSource;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[destinationIndex + index] = pSrc[index];
                }
            }

            return length;
        }

        private static unsafe ulong ReadCharArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (char[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (char* pDest = destination)
                {
                    var pSrc = (char*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadInt16ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (short[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (short* pDest = destination)
                {
                    var pSrc = (short*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadUInt32ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (uint[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            fixed (uint* pDest = destination)
            {
                var pSrc = (uint*)pSource;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[destinationIndex + index] = pSrc[index];
                }
            }

            return length;
        }

        private static unsafe ulong ReadInt32ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (int[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (int* pDest = destination)
                {
                    var pSrc = (int*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadSingleArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (float[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (float* pDest = destination)
                {
                    var pSrc = (float*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadDoubleArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException("destinationIndex");
            }

            var destination = (double[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (double* pDest = destination)
                {
                    var pSrc = (double*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteByteArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (byte[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (byte* pSrc = source)
                {
                    var pDest = (byte*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteSByteArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (sbyte[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            fixed (sbyte* pSrc = source)
            {
                var pDest = (sbyte*)pDestination;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[index] = pSrc[sourceIndex + index];
                }
            }

            return length;
        }

        private static unsafe ulong WriteUInt16ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (ushort[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            fixed (ushort* pSrc = source)
            {
                var pDest = (ushort*)pDestination;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[index] = pSrc[sourceIndex + index];
                }
            }

            return length;
        }

        private static unsafe ulong WriteCharArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (char[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (char* pSrc = source)
                {
                    var pDest = (char*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteInt16ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (short[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (short* pSrc = source)
                {
                    var pDest = (short*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteUInt32ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (uint[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            fixed (uint* pSrc = source)
            {
                var pDest = (uint*)pDestination;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[index] = pSrc[sourceIndex + index];
                }
            }

            return length;
        }

        private static unsafe ulong WriteInt32ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (int[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (int* pSrc = source)
                {
                    var pDest = (int*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteSingleArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (float[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (float* pSrc = source)
                {
                    var pDest = (float*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteDoubleArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var source = (double[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (double* pSrc = source)
                {
                    var pDest = (double*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        #region unit test support

        internal static bool DisableMarshalCopy;

        private static void VerifyMarshalCopyEnabled()
        {
            if (DisableMarshalCopy)
            {
                throw new OverflowException();
            }
        }

        #endregion
    }
}
