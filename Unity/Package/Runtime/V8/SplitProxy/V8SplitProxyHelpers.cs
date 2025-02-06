// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    #region native object helpers

    /// <summary>
    /// Wraps an std::wstring.
    /// </summary>
    public readonly ref struct StdString
    {
        internal readonly Ptr ptr;
        private readonly bool owns;

        /// <summary>
        /// Create a new std::wstring.
        /// </summary>
        /// <param name="value">The contents of the new std::wstring.</param>
        public StdString(string value)
        {
            if (value != null)
            {
                ptr = V8SplitProxyNative.Instance.StdString_New(value);
                owns = true;
            }
            else
            {
                ptr = Ptr.Null;
                owns = false;
            }
        }

        internal StdString(Ptr pValue)
        {
            ptr = pValue;
            owns = false;
        }

        /// <summary>
        /// Delete the wrapped std::wstring.
        /// </summary>
        public void Dispose()
        {
            if (owns)
            {
                V8SplitProxyNative.Instance.StdString_Delete(ptr);
            }
        }

        /// <summary>
        /// Compare the wrapped std::wstring to a <see cref="string"/>.
        /// </summary>
        /// <param name="other">The string to compare the wrapped std::wstring to.</param>
        /// <returns>True if the strings are byte for byte equal, false otherwise.</returns>
        public bool Equals(string other)
        {
            if (ptr == Ptr.Null)
            {
                return other == null;
            }
            else if (other == null)
            {
                return false;
            }

            V8SplitProxyNative.Instance.StdString_GetValue(ptr, out IntPtr value, out int length);

            if (length != other.Length)
            {
                return false;
            }
            else if (length == 0)
            {
                return true;
            }

            unsafe
            {
                char* i = (char*)value;
                char* end = i + length;

                fixed (char* otherPtr = other)
                {
                    char* j = otherPtr;

                    while (i < end)
                    {
                        if (*i != *j)
                        {
                            return false;
                        }

                        i++;
                        j++;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Create a new string with the contents of the wrapped std::wstring.
        /// </summary>
        /// <returns>A new managed string with the same contents as the wrapped std::wtring byte for byte.</returns>
        public override string ToString()
        {
            return ptr != Ptr.Null ? GetValue(ptr) : null;
        }

        internal static IScope<Ptr> CreateScope(string value = null)
        {
            return Scope.Create(() => V8SplitProxyNative.Instance.StdString_New(value ?? string.Empty), V8SplitProxyNative.Instance.StdString_Delete);
        }

        internal static string GetValue(Ptr pString)
        {
            return V8SplitProxyNative.Instance.StdString_GetValue(pString);
        }

        internal static void SetValue(Ptr pString, string value)
        {
            V8SplitProxyNative.Instance.StdString_SetValue(pString, value);
        }

        #region Nested type: Ptr

        internal readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Wraps an std::vector&lt;std::wstring&gt;.
    /// </summary>
    public readonly ref struct StdStringArray
    {
        private readonly Ptr ptr;

        internal StdStringArray(Ptr pValue)
        {
            ptr = pValue;
        }

        /// <summary>
        /// Set the length of the wrapped std::vector&lt;std::wstring&gt;.
        /// </summary>
        /// <param name="elementCount">The new length</param>
        public void SetElementCount(int elementCount)
        {
            V8SplitProxyNative.Instance.StdStringArray_SetElementCount(ptr, elementCount);
        }

        internal static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdStringArray_New(elementCount), instance.StdStringArray_Delete));
        }

        internal static IScope<Ptr> CreateScope(string[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdStringArray_Delete));
        }

        internal static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdStringArray_GetElementCount(pArray));
        }

        internal static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdStringArray_SetElementCount(pArray, elementCount));
        }

        internal static string[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdStringArray_GetElementCount(pArray);
                var array = new string[elementCount];

                if (elementCount > 0)
                {
                    for (var index = 0; index < elementCount; index++)
                    {
                        array[index] = instance.StdStringArray_GetElement(pArray, index);
                    }
                }

                return array;
            });
        }

        internal static void CopyFromArray(Ptr pArray, string[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdStringArray_SetElementCount(pArray, elementCount);

                for (var index = 0; index < elementCount; index++)
                {
                    instance.StdStringArray_SetElement(pArray, index, array[index]);
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, string[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdStringArray_New(elementCount);

            if (elementCount > 0)
            {
                for (var index = 0; index < elementCount; index++)
                {
                    instance.StdStringArray_SetElement(pArray, index, array[index]);
                }
            }

            return pArray;
        }

        #region Nested type: Ptr

        internal readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdByteArray
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdByteArray_New(elementCount), instance.StdByteArray_Delete));
        }

        public static IScope<Ptr> CreateScope(byte[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdByteArray_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdByteArray_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdByteArray_SetElementCount(pArray, elementCount));
        }

        public static byte[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdByteArray_GetElementCount(pArray);
                var array = new byte[elementCount];

                if (elementCount > 0)
                {
                    Marshal.Copy(instance.StdByteArray_GetData(pArray), array, 0, elementCount);
                }

                return array;
            });
        }

        public static void CopyFromArray(Ptr pArray, byte[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdByteArray_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    Marshal.Copy(array, 0, instance.StdByteArray_GetData(pArray), elementCount);
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, byte[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdByteArray_New(elementCount);

            if (elementCount > 0)
            {
                Marshal.Copy(array, 0, instance.StdByteArray_GetData(pArray), elementCount);
            }

            return pArray;
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Wraps an std::vector&lt;int32_t&gt;.
    /// </summary>
    public readonly ref struct StdInt32Array
    {
        private readonly Ptr ptr;

        internal StdInt32Array(Ptr pValue)
        {
            ptr = pValue;
        }

        /// <summary>
        /// Set the length of the wrapped std::vector&lt;int32_t&gt;.
        /// </summary>
        /// <param name="elementCount">The new length.</param>
        public void SetElementCount(int elementCount)
        {
            V8SplitProxyNative.Instance.StdInt32Array_SetElementCount(ptr, elementCount);
        }

        internal static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdInt32Array_New(elementCount), instance.StdInt32Array_Delete));
        }

        internal static IScope<Ptr> CreateScope(int[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdInt32Array_Delete));
        }

        internal static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdInt32Array_GetElementCount(pArray));
        }

        internal static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdInt32Array_SetElementCount(pArray, elementCount));
        }

        internal static int[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdInt32Array_GetElementCount(pArray);
                var array = new int[elementCount];

                if (elementCount > 0)
                {
                    Marshal.Copy(instance.StdInt32Array_GetData(pArray), array, 0, elementCount);
                }

                return array;
            });
        }

        internal static void CopyFromArray(Ptr pArray, int[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdInt32Array_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    Marshal.Copy(array, 0, instance.StdInt32Array_GetData(pArray), elementCount);
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, int[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdInt32Array_New(elementCount);

            if (elementCount > 0)
            {
                Marshal.Copy(array, 0, instance.StdInt32Array_GetData(pArray), elementCount);
            }

            return pArray;
        }

        #region Nested type: Ptr

        internal readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdUInt32Array
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdUInt32Array_New(elementCount), instance.StdUInt32Array_Delete));
        }

        public static IScope<Ptr> CreateScope(uint[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdUInt32Array_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdUInt32Array_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdUInt32Array_SetElementCount(pArray, elementCount));
        }

        public static uint[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdUInt32Array_GetElementCount(pArray);
                var array = new uint[elementCount];

                if (elementCount > 0)
                {
                    UnmanagedMemoryHelpers.Copy(instance.StdUInt32Array_GetData(pArray), (ulong)elementCount, array, 0);
                }

                return array;
            });
        }

        public static void CopyFromArray(Ptr pArray, uint[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdUInt32Array_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    UnmanagedMemoryHelpers.Copy(array, 0, (ulong)elementCount, instance.StdUInt32Array_GetData(pArray));
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, uint[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdUInt32Array_New(elementCount);

            if (elementCount > 0)
            {
                UnmanagedMemoryHelpers.Copy(array, 0, (ulong)elementCount, instance.StdUInt32Array_GetData(pArray));
            }

            return pArray;
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdUInt64Array
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdUInt64Array_New(elementCount), instance.StdUInt64Array_Delete));
        }

        public static IScope<Ptr> CreateScope(ulong[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdUInt64Array_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdUInt64Array_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdUInt64Array_SetElementCount(pArray, elementCount));
        }

        public static ulong[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdUInt64Array_GetElementCount(pArray);
                var array = new ulong[elementCount];

                if (elementCount > 0)
                {
                    UnmanagedMemoryHelpers.Copy(instance.StdUInt64Array_GetData(pArray), (ulong)elementCount, array, 0);
                }

                return array;
            });
        }

        public static void CopyFromArray(Ptr pArray, ulong[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdUInt64Array_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    UnmanagedMemoryHelpers.Copy(array, 0, (ulong)elementCount, instance.StdUInt64Array_GetData(pArray));
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, ulong[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdUInt64Array_New(elementCount);

            if (elementCount > 0)
            {
                UnmanagedMemoryHelpers.Copy(array, 0, (ulong)elementCount, instance.StdUInt64Array_GetData(pArray));
            }

            return pArray;
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdPtrArray
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdPtrArray_New(elementCount), instance.StdPtrArray_Delete));
        }

        public static IScope<Ptr> CreateScope(IntPtr[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdPtrArray_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdPtrArray_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdPtrArray_SetElementCount(pArray, elementCount));
        }

        public static IntPtr[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdPtrArray_GetElementCount(pArray);
                var array = new IntPtr[elementCount];

                if (elementCount > 0)
                {
                    Marshal.Copy(instance.StdPtrArray_GetData(pArray), array, 0, elementCount);
                }

                return array;
            });
        }

        public static void CopyFromArray(Ptr pArray, IntPtr[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdPtrArray_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    Marshal.Copy(array, 0, instance.StdPtrArray_GetData(pArray), elementCount);
                }
            });
        }

        private static Ptr NewFromArray(IV8SplitProxyNative instance, IntPtr[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdPtrArray_New(elementCount);

            if (elementCount > 0)
            {
                Marshal.Copy(array, 0, instance.StdPtrArray_GetData(pArray), elementCount);
            }

            return pArray;
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Wraps an std::vector&lt;V8Value&gt;.
    /// </summary>
    public readonly ref struct StdV8ValueArray
    {
        internal readonly Ptr ptr;
        private readonly V8Value.Ptr data;
        private readonly bool owns;

        /// <summary>
        /// Create a new std::vector&lt;V8Value&gt; of a given length.
        /// </summary>
        /// <param name="elementCount">The length of the new std::vector&lt;V8Value&gt;.</param>
        public StdV8ValueArray(int elementCount)
        {
            ptr = V8SplitProxyNative.Instance.StdV8ValueArray_New(elementCount);
            data = V8SplitProxyNative.Instance.StdV8ValueArray_GetData(ptr);
            owns = true;
        }

        internal StdV8ValueArray(Ptr pArray)
        {
            ptr = pArray;
            owns = false;

            data = pArray != Ptr.Null
                ? V8SplitProxyNative.Instance.StdV8ValueArray_GetData(ptr) : V8Value.Ptr.Null;
        }

        /// <summary>
        /// Delete the wrapped std::vector&lt;V8Value&gt;.
        /// </summary>
        public void Dispose()
        {
            if (owns)
            {
                Ptr ptr = this.ptr;
                V8SplitProxyNative.InvokeNoThrow(instance => instance.StdV8ValueArray_Delete(ptr));
            }
        }

        /// <summary>
        /// Retrieve an element of the wrapped std::vector&lt;V8Value&gt;.
        /// </summary>
        /// <param name="index">The index of the element to retrieve.</param>
        /// <returns>The element.</returns>
        public V8Value this[int index] => new V8Value(GetElementPtr(data, index));

        internal static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdV8ValueArray_New(elementCount), instance.StdV8ValueArray_Delete));
        }

        internal static IScope<Ptr> CreateScope(object[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdV8ValueArray_Delete));
        }

        internal static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdV8ValueArray_GetElementCount(pArray));
        }

        internal static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdV8ValueArray_SetElementCount(pArray, elementCount));
        }

        internal static object[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = instance.StdV8ValueArray_GetElementCount(pArray);
                var array = new object[elementCount];

                if (elementCount > 0)
                {
                    var pElements = instance.StdV8ValueArray_GetData(pArray);
                    for (var index = 0; index < elementCount; index++)
                    {
                        array[index] = V8Value.Get(GetElementPtr(pElements, index));
                    }
                }

                return array;
            });
        }

        internal static void CopyFromArray(Ptr pArray, object[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                var elementCount = array?.Length ?? 0;
                instance.StdV8ValueArray_SetElementCount(pArray, elementCount);

                if (elementCount > 0)
                {
                    var pElements = instance.StdV8ValueArray_GetData(pArray);
                    for (var index = 0; index < elementCount; index++)
                    {
                        V8Value.Set(GetElementPtr(pElements, index), array[index]);
                    }
                }
            });
        }

        internal static Ptr NewFromArray(IV8SplitProxyNative instance, object[] array)
        {
            var elementCount = array?.Length ?? 0;
            var pArray = instance.StdV8ValueArray_New(elementCount);

            var pData = instance.StdV8ValueArray_GetData(pArray);
            for (var index = 0; index < elementCount; index++)
            {
                V8Value.Set(GetElementPtr(pData, index), array[index]);
            }

            return pArray;
        }

        internal static V8Value.Ptr GetElementPtr(V8Value.Ptr pV8Value, int index)
        {
            return (V8Value.Ptr)((IntPtr)pV8Value + index * V8Value.Size);
        }

        #region Nested type: Ptr

        internal readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Wraps a V8Value.
    /// </summary>
    public readonly ref struct V8Value
    {
        internal readonly Ptr ptr;
        private readonly bool owns;

        /// <summary>
        /// Create a new <see cref="Type.Nonexistent"/> value.
        /// </summary>
        /// <returns></returns>
        public static V8Value New()
        {
            Ptr ptr = V8SplitProxyNative.Instance.V8Value_New();
            return new V8Value(ptr, true);
        }

        internal V8Value(Ptr pValue) : this(pValue, false) { }

        private V8Value(Ptr pValue, bool owns)
        {
            ptr = pValue;
            this.owns = owns;
        }

        /// <summary>
        /// Delete the wrapped V8Value.
        /// </summary>
        public void Dispose()
        {
            if (owns)
            {
                Ptr ptr = this.ptr;
                V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_Delete(ptr));
            }
        }

        /// <summary>
        /// Retrieve the value of the wrapped V8Value.
        /// </summary>
        /// <returns>A variant struct that holds a copy of the contents of the wrapped V8Value.</returns>
        /// <remarks>
        /// If the retrieved value is a <see cref="Type.V8Object"/>, it is your responsibility to
        /// dispose of it by calling <see cref="IV8SplitProxyNative.V8Entity_DestroyHandle"/> on it.
        /// </remarks>
        public Decoded Decode()
        {
            Ptr ptr = this.ptr;
            return V8SplitProxyNative.InvokeNoThrow(instance =>
            {
                instance.V8Value_Decode(ptr, out Decoded decoded);
                return decoded;
            });
        }

        /// <summary>
        /// Store a <see cref="BigInteger"/> in the wrapped V8Value as a <see cref="Type.BigInt"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetBigInt(BigInteger value)
        {
            SetBigInt(ptr, value);
        }

        /// <summary>
        /// Store a <see cref="bool"/> in the wrapped V8Value as a <see cref="Type.Boolean"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetBoolean(bool value)
        {
            SetBoolean(ptr, value);
        }

        /// <summary>
        /// Store a <see cref="DateTime"/> in the wrapped V8Value as a <see cref="Type.DateTime"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetDateTime(DateTime value)
        {
            SetDateTime(ptr, value);
        }

        /// <summary>
        /// Store a pointer to a host object in the wrapped V8Value as a <see cref="Type.HostObject"/>
        /// or <see cref="Type.Null"/>.
        /// </summary>
        /// <param name="value">The pointer to store.</param>
        /// <remarks>
        /// For best performance, only pass <see cref="IV8HostObject"/> and
        /// <see cref="InvokeHostObject"/> to JavaScript.
        /// </remarks>
        public void SetHostObject(object value)
        {
            if (value != null)
                SetHostObject(ptr, value);
            else
                SetNull(ptr);
        }

        /// <summary>
        /// Store an <see cref="int"/> in the wrapped V8Value as a <see cref="Type.Int32"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetInt32(int value)
        {
            SetNumeric(ptr, value);
        }

        /// <summary>
        /// Store a <see cref="Void"/> (nothing, not even null, not even undefined) in the wrapped
        /// V8Value as a <see cref="Type.Nonexistent"/>. Nonexistent is the default value of V8Value.
        /// </summary>
        public void SetNonexistent()
        {
            SetNonexistent(ptr);
        }

        /// <summary>
        /// Store a null in the wrapped V8Value as a <see cref="Type.Null"/>.
        /// </summary>
        public void SetNull()
        {
            SetNull(ptr);
        }

        /// <summary>
        /// Store a <see cref="double"/> in the wrapped V8Value as a <see cref="Type.Number"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetNumber(double value)
        {
            SetNumeric(ptr, value);
        }

        /// <summary>
        /// Store a <see cref="string"/> in the wrapped V8Value as a <see cref="Type.String"/> or
        /// <see cref="Type.Null"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetString(string value)
        {
            if (value != null)
                SetString(ptr, value);
            else
                SetNull(ptr);
        }

        /// <summary>
        /// Store an <see cref="Undefined"/> in the wrapped V8Value as a <see cref="Type.Undefined"/>.
        /// </summary>
        public void SetUndefined()
        {
            SetUndefined(ptr);
        }

        /// <summary>
        /// Store a <see cref="uint"/> in the wrapped V8Value as a <see cref="Type.UInt32"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetUInt32(uint value)
        {
            SetNumeric(ptr, value);
        }

        /// <summary>
        /// Store a JavaScript object in the wrapped V8Value as a <see cref="Type.V8Object"/>.
        /// </summary>
        /// <param name="value">The value to store.</param>
        public void SetV8Object(ScriptObject value)
        {
            var impl = ((V8ScriptItem)value).Unwrap();
            SetV8Object(ptr, (V8ObjectImpl)impl);
        }

        internal const int Size = 16;

        internal static IScope<Ptr> CreateScope()
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(instance.V8Value_New, instance.V8Value_Delete));
        }

        internal static IScope<Ptr> CreateScope(object obj)
        {
            var scope = CreateScope();
            Set(scope.Value, obj);
            return scope;
        }

        internal static void Set(Ptr pV8Value, object obj)
        {
            if (obj is Nonexistent)
            {
                SetNonexistent(pV8Value);
                return;
            }

            if (obj == null)
            {
                SetUndefined(pV8Value);
                return;
            }

            if (obj is DBNull)
            {
                SetNull(pV8Value);
                return;
            }

            {
                if (obj is bool value)
                {
                    SetBoolean(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is char value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is sbyte value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is byte value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is short value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is ushort value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is int value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is uint value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is long value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is ulong value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is float value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is double value)
                {
                    SetNumeric(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is decimal value)
                {
                    SetNumeric(pV8Value, (double)value);
                    return;
                }
            }

            {
                if (obj is string value)
                {
                    SetString(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is DateTime value)
                {
                    SetDateTime(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is BigInteger value)
                {
                    SetBigInt(pV8Value, value);
                    return;
                }
            }

            {
                if (obj is V8ObjectImpl v8ObjectImpl)
                {
                    SetV8Object(pV8Value, v8ObjectImpl);
                    return;
                }
            }

            SetHostObject(pV8Value, obj);
        }

        internal static object Get(Ptr pV8Value)
        {
            var decoded = default(Decoded);
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_Decode(pV8Value, out decoded));
            return decoded.Get();
        }

        private static bool TryGetBigInteger(int signBit, int wordCount, IntPtr pWords, out BigInteger result)
        {
            result = BigInteger.Zero;

            if (wordCount > 0)
            {
                var byteCount = (ulong)wordCount * sizeof(ulong);
                if (byteCount >= int.MaxValue)
                {
                    return false;
                }

                // use extra zero byte to force unsigned interpretation
                var bytes = new byte[byteCount + 1];
                UnmanagedMemoryHelpers.Copy(pWords, byteCount, bytes, 0);

                // construct result and negate if necessary
                result = new BigInteger(bytes);
                if (signBit != 0)
                {
                    result = BigInteger.Negate(result);
                }
            }

            return true;
        }

        private static void SetNonexistent(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetNonexistent(pV8Value));
        }

        private static void SetUndefined(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetUndefined(pV8Value));
        }

        private static void SetNull(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetNull(pV8Value));
        }

        private static void SetBoolean(Ptr pV8Value, bool value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetBoolean(pV8Value, value));
        }

        private static void SetNumeric(Ptr pV8Value, double value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetNumber(pV8Value, value));
        }

        private static void SetNumeric(Ptr pV8Value, int value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetInt32(pV8Value, value));
        }

        private static void SetNumeric(Ptr pV8Value, uint value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetUInt32(pV8Value, value));
        }

        private static void SetString(Ptr pV8Value, string value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetString(pV8Value, value));
        }

        private static void SetDateTime(Ptr pV8Value, DateTime value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetDateTime(pV8Value, (value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds));
        }

        private static void SetBigInt(Ptr pV8Value, BigInteger value)
        {
            var signBit = 0;
            if (value.Sign < 0)
            {
                signBit = 1;
                value = BigInteger.Negate(value);
            }

            var bytes = value.ToByteArray();
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetBigInt(pV8Value, signBit, bytes));
        }

        private static void SetV8Object(Ptr pV8Value, V8ObjectImpl v8ObjectImpl)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetV8Object(pV8Value, v8ObjectImpl.Handle, v8ObjectImpl.Subtype, v8ObjectImpl.Flags));
        }

        private static void SetHostObject(Ptr pV8Value, object obj)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_SetHostObject(pV8Value, V8ProxyHelpers.AddRefHostObject(obj)));
        }

        #region Nested type: Type

        /// <summary>
        /// The type of the wrapped V8Value.
        /// </summary>
        public enum Type : byte
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Type

            /// <summary>
            /// <see cref="Void"/>. This is the default value of V8Value.
            /// </summary>
            Nonexistent,

            /// <summary>
            /// Returned by JavaScript when a property does not exist.
            /// </summary>
            Undefined,

            /// <summary>
            /// null.
            /// </summary>
            Null,

            /// <summary>
            /// <see cref="bool"/>.
            /// </summary>
            Boolean,

            /// <summary>
            /// <see cref="double"/>.
            /// </summary>
            Number,

            /// <summary>
            /// <see cref="int"/>.
            /// </summary>
            Int32,

            /// <summary>
            /// <see cref="uint"/>.
            /// </summary>
            UInt32,

            /// <summary>
            /// <see cref="string"/>.
            /// </summary>
            String,

            /// <summary>
            /// <see cref="System.DateTime"/>.
            /// </summary>
            DateTime,

            /// <summary>
            /// <see cref="BigInteger"/>.
            /// </summary>
            BigInt,

            /// <summary>
            /// A JavaScript object.
            /// </summary>
            V8Object,

            /// <summary>
            /// A host object.
            /// </summary>
            HostObject
        }

        #endregion

        #region Nested type: Subtype

        /// <summary>
        /// If the wrapped V8Value is a <see cref="Type.V8Object"/>, what kind of object it is.
        /// </summary>
        public enum Subtype : byte
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Subtype

            /// <summary>
            /// A regular JavaScript object.
            /// </summary>
            None,

            /// <summary>
            /// A function.
            /// </summary>
            Function,

            /// <summary>
            /// An iterator.
            /// </summary>
            Iterator,

            /// <summary>
            /// A promise.
            /// </summary>
            Promise,

            /// <summary>
            /// An array of JavaScript objects.
            /// </summary>
            Array,

            /// <summary>
            /// An array buffer.
            /// </summary>
            ArrayBuffer,

            /// <summary>
            /// A data view.
            /// </summary>
            DataView,

            /// <summary>
            /// An array of <see cref="byte"/>.
            /// </summary>
            Uint8Array,

            /// <summary>
            /// An array of <see cref="byte"/>.
            /// </summary>
            Uint8ClampedArray,

            /// <summary>
            /// A array of <see cref="sbyte"/>.
            /// </summary>
            Int8Array,

            /// <summary>
            /// A array of <see cref="ushort"/>.
            /// </summary>
            Uint16Array,

            /// <summary>
            /// An array of <see cref="short"/>.
            /// </summary>
            Int16Array,

            /// <summary>
            /// An array of <see cref="uint"/>.
            /// </summary>
            Uint32Array,

            /// <summary>
            /// An array of <see cref="int"/>.
            /// </summary>
            Int32Array,

            /// <summary>
            /// An array of <see cref="ulong"/>.
            /// </summary>
            BigUint64Array,

            /// <summary>
            /// An array of <see cref="long"/>.
            /// </summary>
            BigInt64Array,

            /// <summary>
            /// An array of <see cref="float"/>.
            /// </summary>
            Float32Array,

            /// <summary>
            /// An array of <see cref="double"/>.
            /// </summary>
            Float64Array
        }

        #endregion

        #region Nested type: Flags

        /// <summary>
        /// If the wrapped V8Value is a <see cref="Type.V8Object"/>, additional information about it.
        /// </summary>
        [Flags]
        public enum Flags : ushort
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Flags

            /// <summary>
            /// No special meaning.
            /// </summary>
            None = 0,

            /// <summary>
            /// If the object is shared between something, I don't know.
            /// </summary>
            Shared = 0x0001,

            /// <summary>
            /// If the wrapped V8Value is a <see cref="Subtype.Function"/>, if it's async.
            /// </summary>
            Async = 0x0002,

            /// <summary>
            /// If thewrapped V8Value is a <see cref="Subtype.Function"/>, if it's a generator (contains
            /// yield statements).
            /// </summary>
            Generator = 0x0004
        }

        #endregion

        #region Nested type: Ptr

        internal readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion

        #region Nested type: Decoded

        /// <summary>
        /// A variant struct that contains the value retrieved from a <see cref="V8Value"/>.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct Decoded : IDisposable
        {
            // IMPORTANT: maintain bitwise equivalence with native struct V8Value::Decoded

            /// <summary>
            /// The type of the value.
            /// </summary>
            [FieldOffset(0)] public Type Type;

            /// <summary>
            /// If the value is a <see cref="Type.V8Object"/>, what kind of object it is.
            /// </summary>
            [FieldOffset(1)] public Subtype Subtype;

            /// <summary>
            /// If the value is a <see cref="Type.V8Object"/>, additional information about it.
            /// </summary>
            [FieldOffset(2)] public Flags Flags;

            /// <summary>
            /// If the value is a <see cref="Type.BigInt"/>, if it's negative.
            /// </summary>
            [FieldOffset(2)] public short SignBit;

            /// <summary>
            /// If the value is a <see cref="Type.BigInt"/> or a <see cref="Type.String"/>, how long it
            /// is.
            /// </summary>
            [FieldOffset(4)] public int Length;

            /// <summary>
            /// If the value is a pointer to a <see cref="Type.V8Object"/>, the object's identity hash.
            /// </summary>
            [FieldOffset(4)] public int IdentityHash;

            /// <summary>
            /// If the value is a <see cref="Type.Int32"/>, its value.
            /// </summary>
            [FieldOffset(8)] public int Int32Value;

            /// <summary>
            /// If the value is a <see cref="Type.UInt32"/>, its value.
            /// </summary>
            [FieldOffset(8)] public uint UInt32Value;

            /// <summary>
            /// If the value is a <see cref="Type.Number"/>, its value.
            /// </summary>
            [FieldOffset(8)] public double DoubleValue;

            /// <summary>
            /// If the value is a <see cref="Type.HostObject"/> or a <see cref="Type.V8Object"/>,
            /// pointer to it.
            /// </summary>
            [FieldOffset(8)] public IntPtr PtrOrHandle;

            /// <summary>
            /// If the value is a <see cref="Type.V8Object"/>, we must dispose of it when we're done.
            /// </summary>
            public void Dispose()
            {
                if (Type == Type.V8Object)
                    V8SplitProxyNative.Instance.V8Entity_DestroyHandle((V8Entity.Handle)PtrOrHandle);
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.BigInt"/> and return it as a
            /// <see cref="BigInteger"/>.
            /// </summary>
            /// <returns>The <see cref="Type.BigInt"/> value as a <see cref="BigInteger"/></returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.BigInt"/>.</exception>
            /// <exception cref="NotSupportedException">If the big integer is more than two gigabytes in size.</exception>
            public readonly BigInteger GetBigInt()
            {
                if (Type != Type.BigInt)
                    throw new InvalidCastException($"Tried to get a BigInt out of a {GetTypeName()}");

                if (!TryGetBigInteger(SignBit, Length, PtrOrHandle, out var result))
                    throw new NotSupportedException("The size of the big integer exceeds two gigabytes");

                return result;
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.Boolean"/> and return it as a
            /// <see cref="bool"/>.
            /// </summary>
            /// <returns>The <see cref="Type.Boolean"/> value as a <see cref="bool"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.Boolean"/>.</exception>
            public readonly bool GetBoolean()
            {
                if (Type != Type.Boolean)
                    throw new InvalidCastException($"Tried to get a Boolean out of a {GetTypeName()}");

                return Int32Value != 0;
            }

            /// <summary>
            /// Chech that the value is a <see cref="Type.DateTime"/> and return it as a
            /// <see cref="DateTime"/>.
            /// </summary>
            /// <returns>The <see cref="Type.DateTime"/> value as a <see cref="DateTime"/>.</returns>
            /// <remarks>
            /// The <see cref="V8ScriptEngine"/> must have been created with the
            /// <see cref="V8ScriptEngineFlags.EnableDateTimeConversion"/> flag set for this method to
            /// work. Else, the Date object will be passed from JavaScript as a
            /// <see cref="Type.V8Object"/>.
            /// </remarks>
            public readonly DateTime GetDateTime()
            {
                if (Type != Type.DateTime)
                    throw new InvalidCastException($"Tried to get a DateTime out of a {GetTypeName()}");

                return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                       + TimeSpan.FromMilliseconds(DoubleValue);
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.HostObject"/> and return it as a
            /// <see cref="decimal"/>.
            /// </summary>
            /// <returns>The <see cref="Type.HostObject"/> as a <see cref="decimal"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.HostObject"/> or it could not be cast to a <see cref="decimal"/>.</exception>
            /// <remarks>
            /// This method is not actually implemented yet.
            /// </remarks>
            public readonly decimal GetDecimal()
            {
                throw new NotImplementedException("TODO: Actually test this");

                /*if (Type != Type.HostObject)
                    throw new InvalidCastException($"Tried to get a Decimal out of a {GetTypeName()}");

                var hostObject = GetHostObject();
                return (decimal)((HostObject)hostObject).Target;*/
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.Number"/> and return it as a
            /// <see cref="double"/>.
            /// </summary>
            /// <returns>The <see cref="Type.Number"/> value as a <see cref="double"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.Number"/>.</exception>
            public readonly double GetNumber()
            {
                if (Type != Type.Number)
                    throw new InvalidCastException($"Tried to get a Double out of a {GetTypeName()}");

                return DoubleValue;
            }

            /// <summary>
            /// Check that the value is a pointer to a <see cref="Type.HostObject"/> and return it as an
            /// <see cref="object"/>.
            /// </summary>
            /// <returns>The <see cref="Type.HostObject"/> pointer as an <see cref="object"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.HostObject"/>.</exception>
            public readonly object GetHostObject()
            {
                if (Type != Type.HostObject)
                    throw new InvalidCastException($"Tried to get a host object out of a {GetTypeName()}");

                return V8ProxyHelpers.GetHostObject(PtrOrHandle);
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.Int32"/> and return it as an
            /// <see cref="int"/>.
            /// </summary>
            /// <returns>The <see cref="Type.Int32"/> value as an <see cref="int"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.Int32"/>.</exception>
            public readonly int GetInt32()
            {
                if (Type != Type.Int32)
                    throw new InvalidCastException($"Tried to get an Int32 out of a {GetTypeName()}");

                return Int32Value;
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.V8Object"/> and return a <see cref="V8Object"/>
            /// that wraps it.
            /// </summary>
            /// <returns>The pointer to the <see cref="Type.V8Object"/> wrapped in a <see cref="V8Object"/>.</returns>
            /// <exception cref="InvalidCastException">The value is not a pointer to a <see cref="Type.V8Object"/>.</exception>
            public readonly V8Object GetV8Object()
            {
                if (Type != Type.V8Object)
                    throw new InvalidCastException($"Tried to get a JavaScript object out of a {GetTypeName()}");

                return new V8Object((V8Object.Handle)PtrOrHandle, IdentityHash);
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.String"/> and return it as a
            /// <see cref="string"/>.
            /// </summary>
            /// <returns>The <see cref="Type.String"/> value decoded as a <see cref="string"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.String"/>.</exception>
            public readonly string GetString()
            {
                if (Type != Type.String)
                    throw new InvalidCastException($"Tried to get a String out of a {GetTypeName()}");

                return Marshal.PtrToStringUni(PtrOrHandle, Length);
            }

            /// <summary>
            /// Check that the value is a <see cref="Type.UInt32"/> and return it as a
            /// <see cref="uint"/>.
            /// </summary>
            /// <returns>The <see cref="Type.UInt32"/> value decoded as a <see cref="uint"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a <see cref="Type.UInt32"/>.</exception>
            public readonly uint GetUInt32()
            {
                if (Type != Type.UInt32)
                    throw new InvalidCastException($"Tried to get a UInt32 out of a {GetTypeName()}");

                return UInt32Value;
            }

            /// <summary>
            /// Check that the value is a pointer to <see cref="Subtype.Uint8Array"/> and return a
            /// <see cref="Uint8Array"/> that wraps it.
            /// </summary>
            /// <returns>The pointer to the <see cref="Subtype.Uint8Array"/> wrapped in a <see cref="Uint8Array"/>.</returns>
            /// <exception cref="InvalidCastException">If the value is not a pointer to a <see cref="Subtype.Uint8Array"/>.</exception>
            public readonly Uint8Array GetUint8Array()
            {
                if (Type != Type.V8Object || Subtype != Subtype.Uint8Array)
                    throw new InvalidCastException(
                        $"Tried to get a Uint8Array out of a {GetTypeName()}");

                return new Uint8Array((V8Object.Handle)PtrOrHandle);
            }

            /// <summary>
            /// Return a string describing the type of the value.
            /// </summary>
            /// <returns>A string describing the type of the value.</returns>
            public readonly string GetTypeName()
            {
                return Type != Type.V8Object || Subtype == Subtype.None
                    ? Type.ToString() : Subtype.ToString();
            }

            /// <summary>
            /// Returns a string representing the value and the type of the value.
            /// </summary>
            /// <returns>A string representing the value and the type of the value.</returns>
            public override string ToString() => Type switch
            {
                Type.Nonexistent => "void (Nonexistent)",
                Type.Undefined => "undefined (Undefined)",
                Type.Null => "null (Null)",
                Type.Boolean => Int32Value != 0 ? "True (Boolean)" : "False (Boolean)",
                Type.Number => $"{DoubleValue} (Number)",
                Type.Int32 => $"{Int32Value} (Int32)",
                Type.UInt32 => $"{UInt32Value} (UInt32)",
                Type.String => $"\"{Marshal.PtrToStringUni(PtrOrHandle, Length)}\" (String)",
                Type.DateTime => $"{new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(DoubleValue)} (DateTime)",
                Type.BigInt => $"{(TryGetBigInteger(SignBit, Length, PtrOrHandle, out var bigInt) ? bigInt.ToString() : "null")} (BigInt)",
                Type.V8Object => $"0x{PtrOrHandle:x} ({Subtype})",
                Type.HostObject => $"0x{PtrOrHandle:x} (HostObject)",
                _ => $"unknown ({Type})"
            };

            internal readonly object Get()
            {
                switch (Type)
                {
                    case Type.Nonexistent:
                        return Nonexistent.Value;

                    case Type.Null:
                        return DBNull.Value;

                    case Type.Boolean:
                        return Int32Value != 0;

                    case Type.Number:
                        return DoubleValue;

                    case Type.Int32:
                        return Int32Value;

                    case Type.UInt32:
                        return UInt32Value;

                    case Type.String:
                        return Marshal.PtrToStringUni(PtrOrHandle, Length);

                    case Type.DateTime:
                        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(DoubleValue);

                    case Type.BigInt:
                        return TryGetBigInteger(SignBit, Length, PtrOrHandle, out var result) ? (object)result : null;

                    case Type.V8Object:
                        return new V8ObjectImpl((V8Object.Handle)PtrOrHandle, Subtype, Flags, IdentityHash);

                    case Type.HostObject:
                        return V8ProxyHelpers.GetHostObject(PtrOrHandle);

                    default:
                        return null;
                }
            }

            internal static unsafe object Get(Ptr pValues, int index)
            {
                return ((Decoded*)(IntPtr)pValues + index)->Get();
            }

            internal static object[] ToArray(int count, Ptr pValues)
            {
                var array = new object[count];

                for (var index = 0; index < count; index++)
                {
                    array[index] = Get(pValues, index);
                }

                return array;
            }

            #region Nested type: Ptr

            // ReSharper disable once MemberHidesStaticFromOuterClass
            internal readonly struct Ptr
            {
                private readonly IntPtr bits;

                private Ptr(IntPtr bits) => this.bits = bits;

                public static readonly Ptr Null = new Ptr(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

                #region Object overrides

                public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
                public override int GetHashCode() => bits.GetHashCode();

                #endregion
            }

            #endregion
        }

        #endregion
    }

    internal static class V8CpuProfile
    {
        public static void ProcessProfile(V8Entity.Handle hEntity, Ptr pProfile, V8.V8CpuProfile profile)
        {
            string name = null;
            var startTimestamp = 0UL;
            var endTimestamp = 0UL;
            var sampleCount = 0;
            var pRootNode = Node.Ptr.Null;
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8CpuProfile_GetInfo(pProfile, hEntity, out name, out startTimestamp, out endTimestamp, out sampleCount, out pRootNode));

            profile.Name = name;
            profile.StartTimestamp = startTimestamp;
            profile.EndTimestamp = endTimestamp;

            if (pRootNode != Node.Ptr.Null)
            {
                profile.RootNode = CreateNode(hEntity, pRootNode);
            }

            if (sampleCount > 0)
            {
                var samples = new List<V8.V8CpuProfile.Sample>(sampleCount);

                for (var index = 0; index < sampleCount; index++)
                {
                    var nodeId = 0UL;
                    var timestamp = 0UL;
                    var sampleIndex = index;
                    var found = V8SplitProxyNative.InvokeNoThrow(instance => instance.V8CpuProfile_GetSample(pProfile, sampleIndex, out nodeId, out timestamp));
                    if (found)
                    {
                        var node = profile.FindNode(nodeId);
                        if (node != null)
                        {
                            samples.Add(new V8.V8CpuProfile.Sample { Node = node, Timestamp = timestamp });
                        }
                    }
                }

                if (samples.Count > 0)
                {
                    profile.Samples = new ReadOnlyCollection<V8.V8CpuProfile.Sample>(samples);
                }
            }
        }

        private static V8.V8CpuProfile.Node CreateNode(V8Entity.Handle hEntity, Node.Ptr pNode)
        {
            var nodeId = 0UL;
            var scriptId = 0L;
            string scriptName = null;
            string functionName = null;
            string bailoutReason = null;
            var lineNumber = 0L;
            var columnNumber = 0L;
            var hitCount = 0UL;
            var hitLineCount = 0U;
            var childCount = 0;
            V8SplitProxyNative.InvokeNoThrow(instance => instance.V8CpuProfileNode_GetInfo(pNode, hEntity, out nodeId, out scriptId, out scriptName, out functionName, out bailoutReason, out lineNumber, out columnNumber, out hitCount, out hitLineCount, out childCount));

            var node = new V8.V8CpuProfile.Node
            {
                NodeId = nodeId,
                ScriptId = scriptId,
                ScriptName = scriptName,
                FunctionName = functionName,
                BailoutReason = bailoutReason,
                LineNumber = lineNumber,
                ColumnNumber = columnNumber,
                HitCount = hitCount,
            };

            if (hitLineCount > 0)
            {
                int[] lineNumbers = null;
                uint[] hitCounts = null;
                if (V8SplitProxyNative.InvokeNoThrow(instance => instance.V8CpuProfileNode_GetHitLines(pNode, out lineNumbers, out hitCounts)))
                {
                    var actualHitLineCount = Math.Min(lineNumbers.Length, hitCounts.Length);
                    if (actualHitLineCount > 0)
                    {
                        var hitLines = new V8.V8CpuProfile.Node.HitLine[actualHitLineCount];

                        for (var index = 0; index < actualHitLineCount; index++)
                        {
                            hitLines[index].LineNumber = lineNumbers[index];
                            hitLines[index].HitCount = hitCounts[index];
                        }

                        node.HitLines = new ReadOnlyCollection<V8.V8CpuProfile.Node.HitLine>(hitLines);
                    }
                }
            }

            if (childCount > 0)
            {
                var childNodes = new List<V8.V8CpuProfile.Node>(childCount);

                for (var index = 0; index < childCount; index++)
                {
                    var childIndex = index;
                    var pChildNode = V8SplitProxyNative.InvokeNoThrow(instance => instance.V8CpuProfileNode_GetChildNode(pNode, childIndex));
                    if (pChildNode != Node.Ptr.Null)
                    {
                        childNodes.Add(CreateNode(hEntity, pChildNode));
                    }
                }

                if (childNodes.Count > 0)
                {
                    node.ChildNodes = new ReadOnlyCollection<V8.V8CpuProfile.Node>(childNodes);
                }
            }

            return node;
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new Ptr(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion

        #region Nested type: Node

        internal static class Node
        {
            #region Nested type: Ptr

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly struct Ptr
            {
                private readonly IntPtr bits;

                private Ptr(IntPtr bits) => this.bits = bits;

                public static readonly Ptr Null = new Ptr(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new Ptr(bits);

                #region Object overrides

                public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
                public override int GetHashCode() => bits.GetHashCode();

                #endregion
            }

            #endregion
        }

        #endregion
    }

    #endregion

    #region V8 entity helpers

    internal static class V8Entity
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class V8Isolate
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class V8Context
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Wraps a JavaScript object.
    /// </summary>
    public readonly ref struct V8Object
    {
        private readonly Handle ptr;
        private readonly int identityHash;

        internal V8Object(Handle hObject, int identityHash)
        {
            if (hObject == Handle.Empty)
                throw new ArgumentNullException(nameof(hObject));

            ptr = hObject;
            this.identityHash = identityHash;
        }

        /// <summary>
        /// Obtain a thin wrapper around a JavaScript object wrapped by a <see cref="ScriptObject"/>.
        /// </summary>
        /// <param name="scriptObject">A wrapper around a JavaScript object.</param>
        /// <remarks>
        /// The common case is to pass <see cref="V8ScriptEngine.Global"/> to this constructor.
        /// </remarks>
        public V8Object(ScriptObject scriptObject)
        {
            object target = ((V8ScriptItem)scriptObject).Unwrap();
            var impl = (V8ObjectImpl)target;
            ptr = impl.Handle;
            identityHash = impl.IdentityHash;
        }

        /// <summary>
        /// Returns the identity hash of the JavaScript object.
        /// </summary>
        /// <returns>The identity hash of the JavaScript object.</returns>
        public override int GetHashCode()
        {
            return identityHash;
        }

        /// <summary>
        /// Obtain the value of a named property of the wrapped JavaScript object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property will be written here.</param>
        public void GetNamedProperty(StdString name, V8Value value)
        {
            Handle hObject = ptr;
            StdString.Ptr pName = name.ptr;
            V8Value.Ptr pValue = value.ptr;

            if (pName == StdString.Ptr.Null)
                throw new ArgumentNullException(nameof(name));

            if (pValue == V8Value.Ptr.Null)
                throw new ArgumentNullException(nameof(value));

            V8SplitProxyNative.Invoke(instance => instance.V8Object_GetNamedProperty(hObject, pName, pValue));
        }

        /// <summary>
        /// Invoke the wrapped JavaScript object as a function.
        /// </summary>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <param name="result">The return value of the function will be written here.</param>
        /// <param name="asConstructor">Call the JavaScript as a constructor?</param>
        public void Invoke(StdV8ValueArray args, V8Value result, bool asConstructor = false)
        {
            Handle hObject = ptr;
            StdV8ValueArray.Ptr pArgs = args.ptr;
            V8Value.Ptr pResult = result.ptr;

            if (pArgs == StdV8ValueArray.Ptr.Null)
                throw new ArgumentNullException(nameof(args));

            if (pResult == V8Value.Ptr.Null)
                throw new ArgumentNullException(nameof(result));

            V8SplitProxyNative.Invoke(instance => instance.V8Object_Invoke(hObject, asConstructor, pArgs, pResult));
        }

        #region Nested type: Handle

        internal readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class V8Script
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class V8DebugCallback
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class NativeCallback
    {
        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new Handle(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new Handle(guts);

            public static implicit operator V8Entity.Handle(Handle handle) => (V8Entity.Handle)handle.guts;
            public static explicit operator Handle(V8Entity.Handle handle) => (Handle)(IntPtr)handle;

            #region Object overrides

            public override bool Equals(object obj) => (obj is Handle handle) && (this == handle);
            public override int GetHashCode() => guts.GetHashCode();

            #endregion
        }

        #endregion
    }

    #endregion
}
