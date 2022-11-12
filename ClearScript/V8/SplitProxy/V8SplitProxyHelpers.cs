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

    internal static class StdString
    {
        public static IScope<Ptr> CreateScope(string value = null)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdString_New(value ?? string.Empty), instance.StdString_Delete));
        }

        public static string GetValue(Ptr pString)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdString_GetValue(pString));
        }

        public static void SetValue(Ptr pString, string value)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdString_SetValue(pString, value));
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

    internal static class StdStringArray
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdStringArray_New(elementCount), instance.StdStringArray_Delete));
        }

        public static IScope<Ptr> CreateScope(string[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdStringArray_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdStringArray_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdStringArray_SetElementCount(pArray, elementCount));
        }

        public static string[] ToArray(Ptr pArray)
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

        public static void CopyFromArray(Ptr pArray, string[] array)
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

    internal static class StdInt32Array
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdInt32Array_New(elementCount), instance.StdInt32Array_Delete));
        }

        public static IScope<Ptr> CreateScope(int[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdInt32Array_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdInt32Array_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdInt32Array_SetElementCount(pArray, elementCount));
        }

        public static int[] ToArray(Ptr pArray)
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

        public static void CopyFromArray(Ptr pArray, int[] array)
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

    internal static class StdV8ValueArray
    {
        public static IScope<Ptr> CreateScope(int elementCount = 0)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => instance.StdV8ValueArray_New(elementCount), instance.StdV8ValueArray_Delete));
        }

        public static IScope<Ptr> CreateScope(object[] array)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(() => NewFromArray(instance, array), instance.StdV8ValueArray_Delete));
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => instance.StdV8ValueArray_GetElementCount(pArray));
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(instance => instance.StdV8ValueArray_SetElementCount(pArray, elementCount));
        }

        public static object[] ToArray(Ptr pArray)
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

        public static void CopyFromArray(Ptr pArray, object[] array)
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

        private static Ptr NewFromArray(IV8SplitProxyNative instance, object[] array)
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

        public static V8Value.Ptr GetElementPtr(V8Value.Ptr pV8Value, int index)
        {
            return (V8Value.Ptr)((IntPtr)pV8Value + index * V8Value.Size);
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

    internal static class V8Value
    {
        public const int Size = 16;

        public static IScope<Ptr> CreateScope()
        {
            return V8SplitProxyNative.InvokeNoThrow(instance => Scope.Create(instance.V8Value_New, instance.V8Value_Delete));
        }

        public static IScope<Ptr> CreateScope(object obj)
        {
            var scope = CreateScope();
            Set(scope.Value, obj);
            return scope;
        }

        public static void Set(Ptr pV8Value, object obj)
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

        public static object Get(Ptr pV8Value)
        {
            var intValue = 0;
            var uintValue = 0U;
            var doubleValue = 0D;
            var ptrOrHandle = IntPtr.Zero;

            switch (V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Value_Decode(pV8Value, out intValue, out uintValue, out doubleValue, out ptrOrHandle)))
            {
                case Type.Nonexistent:
                    return Nonexistent.Value;

                case Type.Null:
                    return DBNull.Value;

                case Type.Boolean:
                    return intValue != 0;

                case Type.Number:
                    return doubleValue;

                case Type.Int32:
                    return intValue;

                case Type.UInt32:
                    return uintValue;

                case Type.String:
                    return Marshal.PtrToStringUni(ptrOrHandle, intValue);

                case Type.DateTime:
                    return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(doubleValue);

                case Type.BigInt:
                    return TryGetBigInteger(intValue, (int)uintValue, ptrOrHandle, out var result) ? (object)result : null;

                case Type.V8Object:
                    return new V8ObjectImpl((V8Object.Handle)ptrOrHandle, (Subtype)(uintValue & 0xFFFFU), (Flags)(uintValue >> 16), intValue);

                case Type.HostObject:
                    return V8ProxyHelpers.GetHostObject(ptrOrHandle);

                default:
                    return null;
            }
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

        public enum Type : ushort
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Type
            Nonexistent,
            Undefined,
            Null,
            Boolean,
            Number,
            Int32,
            UInt32,
            String,
            DateTime,
            BigInt,
            V8Object,
            HostObject
        }

        #endregion

        #region Nested type: Subtype

        public enum Subtype : ushort
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Subtype
            None,
            Promise,
            Array,
            ArrayBuffer,
            DataView,
            Uint8Array,
            Uint8ClampedArray,
            Int8Array,
            Uint16Array,
            Int16Array,
            Uint32Array,
            Int32Array,
            BigUint64Array,
            BigInt64Array,
            Float32Array,
            Float64Array
        }

        #endregion

        #region Nested type: Flags

        [Flags]
        public enum Flags : ushort
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Flags
            None = 0,
            Shared = 0x0001
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

    internal static class V8Object
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
