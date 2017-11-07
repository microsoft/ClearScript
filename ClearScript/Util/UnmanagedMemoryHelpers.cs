// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "This is a performance-critical class with extensive test coverage.")]
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
