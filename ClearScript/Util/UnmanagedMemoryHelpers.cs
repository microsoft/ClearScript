// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class UnmanagedMemoryHelpers
    {
        private delegate ulong ReadArrayFromUnmanagedMemoryHandler(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex);
        private delegate ulong WriteArrayToUnmanagedMemoryHandler(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination);

        private static readonly Dictionary<Type, ReadArrayFromUnmanagedMemoryHandler> readArrayFromUnmanagedMemoryHandlerMap = new()
        {
            { typeof(byte), ReadByteArrayFromUnmanagedMemory },
            { typeof(sbyte), ReadSByteArrayFromUnmanagedMemory },
            { typeof(ushort), ReadUInt16ArrayFromUnmanagedMemory },
            { typeof(char), ReadCharArrayFromUnmanagedMemory },         // blittable because ClearScriptV8's StdChar is char16_t
            { typeof(short), ReadInt16ArrayFromUnmanagedMemory },
            { typeof(uint), ReadUInt32ArrayFromUnmanagedMemory },
            { typeof(int), ReadInt32ArrayFromUnmanagedMemory },
            { typeof(long), ReadInt64ArrayFromUnmanagedMemory },
            { typeof(ulong), ReadUInt64ArrayFromUnmanagedMemory },
            { typeof(IntPtr), ReadIntPtrArrayFromUnmanagedMemory },
            { typeof(UIntPtr), ReadUIntPtrArrayFromUnmanagedMemory },
            { typeof(float), ReadSingleArrayFromUnmanagedMemory },
            { typeof(double), ReadDoubleArrayFromUnmanagedMemory }
        };

        private static readonly Dictionary<Type, WriteArrayToUnmanagedMemoryHandler> writeArrayToUnmanagedMemoryHandlerMap = new()
        {
            { typeof(byte), WriteByteArrayToUnmanagedMemory },
            { typeof(sbyte), WriteSByteArrayToUnmanagedMemory },
            { typeof(ushort), WriteUInt16ArrayToUnmanagedMemory },
            { typeof(char), WriteCharArrayToUnmanagedMemory },          // blittable because ClearScriptV8's StdChar is char16_t
            { typeof(short), WriteInt16ArrayToUnmanagedMemory },
            { typeof(uint), WriteUInt32ArrayToUnmanagedMemory },
            { typeof(int), WriteInt32ArrayToUnmanagedMemory },
            { typeof(long), WriteInt64ArrayToUnmanagedMemory },
            { typeof(ulong), WriteUInt64ArrayToUnmanagedMemory },
            { typeof(IntPtr), WriteIntPtrArrayToUnmanagedMemory },
            { typeof(UIntPtr), WriteUIntPtrArrayToUnmanagedMemory },
            { typeof(float), WriteSingleArrayToUnmanagedMemory },
            { typeof(double), WriteDoubleArrayToUnmanagedMemory }
        };

        public static ulong Copy<T>(IntPtr pSource, ulong length, T[] destination, ulong destinationIndex)
        {
            return Copy(typeof(T), pSource, length, destination, destinationIndex);
        }

        public static unsafe ulong Copy<T>(IntPtr pSource, ulong length, in Span<T> destination, ulong destinationIndex) where T : unmanaged
        {
            var destinationLength = (ulong)destination.Length;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            length = Math.Min(length, destinationLength - destinationIndex);
            var elementSize = (ulong)Unsafe.SizeOf<T>();

            fixed (void* pDestinationSpan = destination)
            {
                var pDestination = ((IntPtr)pDestinationSpan + Convert.ToInt32(destinationIndex * elementSize)).ToPointer();
                var bytesToCopy = Convert.ToInt64(length * elementSize);
                Buffer.MemoryCopy(pSource.ToPointer(), pDestination, bytesToCopy, bytesToCopy);
            }

            return length;
        }

        public static ulong Copy<T>(T[] source, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            return Copy(typeof(T), source, sourceIndex, length, pDestination);
        }

        public static unsafe ulong Copy<T>(in ReadOnlySpan<T> source, ulong sourceIndex, ulong length, IntPtr pDestination) where T : unmanaged
        {
            var sourceLength = (ulong)source.Length;
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            length = Math.Min(length, sourceLength - sourceIndex);
            var elementSize = (ulong)Unsafe.SizeOf<T>();

            fixed (void* pSourceSpan = source)
            {
                var pSource = ((IntPtr)pSourceSpan + Convert.ToInt32(sourceIndex * elementSize)).ToPointer();
                var bytesToCopy = Convert.ToInt64(length * elementSize);
                Buffer.MemoryCopy(pSource, pDestination.ToPointer(), bytesToCopy, bytesToCopy);
            }

            return length;
        }

        private static ulong Copy(Type type, IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            if (readArrayFromUnmanagedMemoryHandlerMap.TryGetValue(type, out var handler))
            {
                return handler(pSource, length, destinationArray, destinationIndex);
            }

            throw new NotSupportedException("Unsupported unmanaged data transfer operation");
        }

        private static ulong Copy(Type type, Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            if (writeArrayToUnmanagedMemoryHandlerMap.TryGetValue(type, out var handler))
            {
                return handler(sourceArray, sourceIndex, length, pDestination);
            }

            throw new NotSupportedException("Unsupported unmanaged data transfer operation");
        }

        private static unsafe ulong ReadByteArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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

        private static unsafe ulong ReadUInt64ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var destination = (ulong[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            fixed (ulong* pDest = destination)
            {
                var pSrc = (ulong*)pSource;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[destinationIndex + index] = pSrc[index];
                }
            }

            return length;
        }

        private static unsafe ulong ReadInt64ArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var destination = (long[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (long* pDest = destination)
                {
                    var pSrc = (long*)pSource;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[destinationIndex + index] = pSrc[index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong ReadUIntPtrArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var destination = (UIntPtr[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            fixed (UIntPtr* pDest = destination)
            {
                var pSrc = (UIntPtr*)pSource;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[destinationIndex + index] = pSrc[index];
                }
            }

            return length;
        }

        private static unsafe ulong ReadIntPtrArrayFromUnmanagedMemory(IntPtr pSource, ulong length, Array destinationArray, ulong destinationIndex)
        {
            var destinationLength = (ulong)destinationArray.LongLength;
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var destination = (IntPtr[])destinationArray;
            length = Math.Min(length, destinationLength - destinationIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(pSource, destination, Convert.ToInt32(destinationIndex), Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (IntPtr* pDest = destination)
                {
                    var pSrc = (IntPtr*)pSource;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (destinationLength < 1)
            {
                if (destinationIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(destinationIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (destinationIndex >= destinationLength)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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

        private static unsafe ulong WriteUInt64ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var source = (ulong[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            fixed (ulong* pSrc = source)
            {
                var pDest = (ulong*)pDestination;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[index] = pSrc[sourceIndex + index];
                }
            }

            return length;
        }

        private static unsafe ulong WriteInt64ArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var source = (long[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (long* pSrc = source)
                {
                    var pDest = (long*)pDestination;
                    for (var index = 0UL; index < length; index++)
                    {
                        pDest[index] = pSrc[sourceIndex + index];
                    }
                }
            }

            return length;
        }

        private static unsafe ulong WriteUIntPtrArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var source = (UIntPtr[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            fixed (UIntPtr* pSrc = source)
            {
                var pDest = (UIntPtr*)pDestination;
                for (var index = 0UL; index < length; index++)
                {
                    pDest[index] = pSrc[sourceIndex + index];
                }
            }

            return length;
        }

        private static unsafe ulong WriteIntPtrArrayToUnmanagedMemory(Array sourceArray, ulong sourceIndex, ulong length, IntPtr pDestination)
        {
            var sourceLength = (ulong)sourceArray.LongLength;
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
            }

            var source = (IntPtr[])sourceArray;
            length = Math.Min(length, sourceLength - sourceIndex);

            try
            {
                VerifyMarshalCopyEnabled();
                Marshal.Copy(source, Convert.ToInt32(sourceIndex), pDestination, Convert.ToInt32(length));
            }
            catch (OverflowException)
            {
                fixed (IntPtr* pSrc = source)
                {
                    var pDest = (IntPtr*)pDestination;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
            if (sourceLength < 1)
            {
                if (sourceIndex > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sourceIndex));
                }

                if (length > 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(length));
                }
            }
            else if (sourceIndex >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (length < 1)
            {
                return 0UL;
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
