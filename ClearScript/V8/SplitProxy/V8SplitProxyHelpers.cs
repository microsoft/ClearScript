// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.FastProxy;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    #region native object helpers

    internal static class StdString
    {
        public static ValueScope<Ptr> CreateScope(string value = null)
        {
            return ScopeFactory.Create(
                static value => V8SplitProxyNative.InvokeRaw(static (instance, value) => instance.StdString_New(value ?? string.Empty), value),
                static pString => V8SplitProxyNative.InvokeRaw(static (instance, pString) => instance.StdString_Delete(pString), pString),
                value
            );
        }

        public static string GetValue(Ptr pString)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pString) => instance.StdString_GetValue(pString), pString);
        }

        public static TValue GetValue<TValue>(Ptr pString, Func<IntPtr, int, TValue> factory)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdString_GetValue(ctx.pString, ctx.factory), (pString, factory));
        }

        public static TValue GetValue<TValue, TArg>(Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdString_GetValue(ctx.pString, ctx.factory, ctx.arg), (pString, factory, arg));
        }

        public static void SetValue(Ptr pString, string value)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdString_SetValue(ctx.pString, ctx.value), (pString, value));
        }

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdStringArray
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdStringArray_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdStringArray_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(string[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdStringArray_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdStringArray_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdStringArray_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static string[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
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
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, string[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdStringArray_SetElementCount(ctx.pArray, elementCount);

                    for (var index = 0; index < elementCount; index++)
                    {
                        instance.StdStringArray_SetElement(ctx.pArray, index, ctx.array[index]);
                    }
                },
                (pArray, array)
            );
        }

        public static void CopyFromEnumerable(Ptr pArray, IEnumerable<string> source)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var itemCount = ctx.source?.Count() ?? 0;
                    instance.StdStringArray_SetElementCount(ctx.pArray, itemCount);

                    var index = 0;
                    foreach (var item in ctx.source)
                    {
                        instance.StdStringArray_SetElement(ctx.pArray, index++, item);
                    }
                },
                (pArray, source)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdByteArray
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdByteArray_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdByteArray_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(byte[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdByteArray_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdByteArray_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdByteArray_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static byte[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
                {
                    var elementCount = instance.StdByteArray_GetElementCount(pArray);
                    var array = new byte[elementCount];

                    if (elementCount > 0)
                    {
                        Marshal.Copy(instance.StdByteArray_GetData(pArray), array, 0, elementCount);
                    }

                    return array;
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, byte[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdByteArray_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        Marshal.Copy(ctx.array, 0, instance.StdByteArray_GetData(ctx.pArray), elementCount);
                    }
                },
                (pArray, array)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdInt32Array
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdInt32Array_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdInt32Array_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(int[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdInt32Array_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdInt32Array_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdInt32Array_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static int[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
                {
                    var elementCount = instance.StdInt32Array_GetElementCount(pArray);
                    var array = new int[elementCount];

                    if (elementCount > 0)
                    {
                        Marshal.Copy(instance.StdInt32Array_GetData(pArray), array, 0, elementCount);
                    }

                    return array;
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, int[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdInt32Array_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        Marshal.Copy(ctx.array, 0, instance.StdInt32Array_GetData(ctx.pArray), elementCount);
                    }
                },
                (pArray, array)
            );
        }

        public static void CopyFromEnumerable(Ptr pArray, IEnumerable<int> source)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var itemCount = ctx.source?.Count() ?? 0;
                    instance.StdInt32Array_SetElementCount(ctx.pArray, itemCount);

                    if (itemCount > 0)
                    {
                        var index = 0;
                        var pData = instance.StdInt32Array_GetData(ctx.pArray);
                        foreach (var item in ctx.source)
                        {
                            Marshal.WriteInt32(pData, index++, item);
                        }
                    }
                },
                (pArray, source)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdUInt32Array
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdUInt32Array_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt32Array_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(uint[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt32Array_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt32Array_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdUInt32Array_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static uint[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
                {
                    var elementCount = instance.StdUInt32Array_GetElementCount(pArray);
                    var array = new uint[elementCount];

                    if (elementCount > 0)
                    {
                        UnmanagedMemoryHelpers.Copy(instance.StdUInt32Array_GetData(pArray), (ulong)elementCount, array, 0);
                    }

                    return array;
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, uint[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdUInt32Array_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        UnmanagedMemoryHelpers.Copy(ctx.array, 0, (ulong)elementCount, instance.StdUInt32Array_GetData(ctx.pArray));
                    }
                },
                (pArray, array)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdUInt64Array
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdUInt64Array_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt64Array_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(ulong[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt64Array_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdUInt64Array_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdUInt64Array_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static ulong[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
                {
                    var elementCount = instance.StdUInt64Array_GetElementCount(pArray);
                    var array = new ulong[elementCount];

                    if (elementCount > 0)
                    {
                        UnmanagedMemoryHelpers.Copy(instance.StdUInt64Array_GetData(pArray), (ulong)elementCount, array, 0);
                    }

                    return array;
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, ulong[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdUInt64Array_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        UnmanagedMemoryHelpers.Copy(ctx.array, 0, (ulong)elementCount, instance.StdUInt64Array_GetData(ctx.pArray));
                    }
                },
                (pArray, array)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdPtrArray
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdPtrArray_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdPtrArray_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(IntPtr[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeRaw(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdPtrArray_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdPtrArray_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.StdPtrArray_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static IntPtr[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pArray) =>
                {
                    var elementCount = instance.StdPtrArray_GetElementCount(pArray);
                    var array = new IntPtr[elementCount];

                    if (elementCount > 0)
                    {
                        Marshal.Copy(instance.StdPtrArray_GetData(pArray), array, 0, elementCount);
                    }

                    return array;
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, IntPtr[] array)
        {
            V8SplitProxyNative.InvokeRaw(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdPtrArray_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        Marshal.Copy(ctx.array, 0, instance.StdPtrArray_GetData(ctx.pArray), elementCount);
                    }
                },
                (pArray, array)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion
    }

    internal static class StdV8ValueArray
    {
        public static ValueScope<Ptr> CreateScope(int elementCount = 0)
        {
            return ScopeFactory.Create(
                static elementCount => V8SplitProxyNative.InvokeRaw(static (instance, elementCount) => instance.StdV8ValueArray_New(elementCount), elementCount),
                static pArray => V8SplitProxyNative.InvokeNoThrow(static (instance, pArray) => instance.StdV8ValueArray_Delete(pArray), pArray),
                elementCount
            );
        }

        public static ValueScope<Ptr> CreateScope(object[] array)
        {
            return ScopeFactory.Create(
                static array => V8SplitProxyNative.InvokeNoThrow(static (instance, array) => NewFromArray(instance, array), array),
                static pArray => V8SplitProxyNative.InvokeNoThrow(static (instance, pArray) => instance.StdV8ValueArray_Delete(pArray), pArray),
                array
            );
        }

        public static int GetElementCount(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, pArray) => instance.StdV8ValueArray_GetElementCount(pArray), pArray);
        }

        public static void SetElementCount(Ptr pArray, int elementCount)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.StdV8ValueArray_SetElementCount(ctx.pArray, ctx.elementCount), (pArray, elementCount));
        }

        public static object[] ToArray(Ptr pArray)
        {
            return V8SplitProxyNative.InvokeNoThrow(
                static (instance, pArray) =>
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
                },
                pArray
            );
        }

        public static void CopyFromArray(Ptr pArray, object[] array)
        {
            V8SplitProxyNative.InvokeNoThrow(
                static (instance, ctx) =>
                {
                    var elementCount = ctx.array?.Length ?? 0;
                    instance.StdV8ValueArray_SetElementCount(ctx.pArray, elementCount);

                    if (elementCount > 0)
                    {
                        var pElements = instance.StdV8ValueArray_GetData(ctx.pArray);
                        for (var index = 0; index < elementCount; index++)
                        {
                            V8Value.Set(GetElementPtr(pElements, index), ctx.array[index]);
                        }
                    }
                },
                (pArray, array)
            );
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

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

        private const short positiveOrZero = 0;
        private const short negative = 1;

        public static ValueScope<Ptr> CreateScope()
        {
            return ScopeFactory.Create(
                static () => V8SplitProxyNative.InvokeRaw(static instance => instance.V8Value_New()),
                static pV8Value => V8SplitProxyNative.InvokeNoThrow(static (instance, pV8Value) => instance.V8Value_Delete(pV8Value), pV8Value)
            );
        }

        public static ValueScope<Ptr> CreateScope(object obj)
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
            }
            else if (obj is null)
            {
                SetUndefined(pV8Value);
            }
            else if (obj is DBNull)
            {
                SetNull(pV8Value);
            }
            else if (obj is bool boolValue)
            {
                SetBoolean(pV8Value, boolValue);
            }
            else if (obj is char charValue)
            {
                SetNumber(pV8Value, charValue);
            }
            else if (obj is sbyte sbyteValue)
            {
                SetNumber(pV8Value, sbyteValue);
            }
            else if (obj is byte byteValue)
            {
                SetNumber(pV8Value, byteValue);
            }
            else if (obj is short shortValue)
            {
                SetNumber(pV8Value, shortValue);
            }
            else if (obj is ushort ushortValue)
            {
                SetNumber(pV8Value, ushortValue);
            }
            else if (obj is int intValue)
            {
                SetNumber(pV8Value, intValue);
            }
            else if (obj is uint uintValue)
            {
                SetNumber(pV8Value, uintValue);
            }
            else if (obj is long longValue)
            {
                SetNumber(pV8Value, longValue);
            }
            else if (obj is ulong ulongValue)
            {
                SetNumber(pV8Value, ulongValue);
            }
            else if (obj is float floatValue)
            {
                SetNumber(pV8Value, floatValue);
            }
            else if (obj is double doubleValue)
            {
                SetNumber(pV8Value, doubleValue);
            }
            else if (obj is decimal decimalValue)
            {
                SetNumber(pV8Value, (double)decimalValue);
            }
            else if (obj is string stringValue)
            {
                SetString(pV8Value, stringValue);
            }
            else if (obj is DateTime dateTimeValue)
            {
                SetDateTime(pV8Value, dateTimeValue);
            }
            else if (obj is BigInteger bigIntegerValue)
            {
                SetBigInteger(pV8Value, bigIntegerValue);
            }
            else if (obj is V8ObjectImpl v8Object)
            {
                SetV8Object(pV8Value, v8Object);
            }
            else if (obj is IHostItem hostObject)
            {
                SetHostObject(pV8Value, hostObject);
            }
            else
            {
                SetUndefined(pV8Value);
            }
        }

        public static object Get(Ptr pV8Value)
        {
            return V8SplitProxyNative.InvokeRaw(
                static (instance, pV8Value) =>
                {
                    instance.V8Value_Decode(pV8Value, out var decoded);
                    return decoded.Get();
                },
                pV8Value
            );
        }

        private static void SetNonexistent(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, pV8Value) => instance.V8Value_SetNonexistent(pV8Value), pV8Value);
        }

        private static void SetUndefined(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, pV8Value) => instance.V8Value_SetUndefined(pV8Value), pV8Value);
        }

        private static void SetNull(Ptr pV8Value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, pV8Value) => instance.V8Value_SetNull(pV8Value), pV8Value);
        }

        private static void SetBoolean(Ptr pV8Value, bool value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetBoolean(ctx.pV8Value, ctx.value), (pV8Value, value));
        }

        private static void SetNumber(Ptr pV8Value, double value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetNumber(ctx.pV8Value, ctx.value), (pV8Value, value));
        }

        private static void SetString(Ptr pV8Value, string value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetString(ctx.pV8Value, ctx.value), (pV8Value, value));
        }

        private static void SetDateTime(Ptr pV8Value, DateTime value)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetDateTime(ctx.pV8Value, ctx.value.ToUnixMilliseconds()), (pV8Value, value));
        }

        private static void SetBigInteger(Ptr pV8Value, BigInteger value)
        {
            var signBit = positiveOrZero;
            if (value.Sign < 0)
            {
                signBit = negative;
                value = BigInteger.Negate(value);
            }

            var bytes = value.ToByteArray();
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetBigInt(ctx.pV8Value, ctx.signBit, ctx.bytes), (pV8Value, signBit, bytes));
        }

        private static void SetV8Object(Ptr pV8Value, V8ObjectImpl v8Object)
        {
            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetV8Object(ctx.pV8Value, ctx.v8Object.Handle, ctx.v8Object.Subtype, ctx.v8Object.Flags), (pV8Value, v8Object));
        }

        private static void SetHostObject(Ptr pV8Value, IHostItem hostObject)
        {
            var subtype = Subtype.None;
            var flags = Flags.None;

            if (hostObject is V8FastHostItem fastHostItem)
            {
                flags = Flags.Fast;
                if (fastHostItem.IsInvocable)
                {
                    subtype = Subtype.Function;
                }
            }

            V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Value_SetHostObject(ctx.pV8Value, V8ProxyHelpers.AddRefHostObject(ctx.obj), ctx.subtype, ctx.flags), (pV8Value, obj: hostObject, subtype, flags));
        }

        #region Nested type: Type

        public enum Type : byte
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Type
            Nonexistent,
            Undefined,
            Null,
            Boolean,
            Number,
            String,
            DateTime,
            BigInt,
            V8Object,
            HostObject
        }

        #endregion

        #region Nested type: Subtype

        public enum Subtype : byte
        {
            // IMPORTANT: maintain bitwise equivalence with native enum V8Value::Subtype
            None,
            Function,
            Iterator,
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
            Shared = 0x0001,
            Fast = 0x0001,
            Async = 0x0002,
            Generator = 0x0004,
            Pending = 0x0008,
            Rejected = 0x0010
        }

        public static bool HasAllFlags(this Flags value, Flags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this Flags value, Flags flags) => (value & flags) != 0;

        #endregion

        #region Nested type: Ptr

        public readonly struct Ptr
        {
            private readonly IntPtr bits;

            private Ptr(IntPtr bits) => this.bits = bits;

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

            #region Object overrides

            public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
            public override int GetHashCode() => bits.GetHashCode();

            #endregion
        }

        #endregion

        #region Nested type: WireData

        [StructLayout(LayoutKind.Explicit)]
        public struct WireData
        {
            // IMPORTANT: maintain bitwise equivalence with native struct V8Value::WireData
            [FieldOffset(0)] public Type Type;
            [FieldOffset(1)] public Subtype Subtype;
            [FieldOffset(2)] public Flags Flags;
            [FieldOffset(2)] public short SignBit;
            [FieldOffset(4)] public int Length;
            [FieldOffset(4)] public int IdentityHash;
            [FieldOffset(8)] public int Int32Value;
            [FieldOffset(8)] public double DoubleValue;
            [FieldOffset(8)] public IntPtr PtrOrHandle;

            public readonly bool TryCreateBigInteger(out BigInteger value)
            {
                value = BigInteger.Zero;

                if (Length > 0)
                {
                    var byteCount = (ulong)Length * sizeof(ulong);
                    if (byteCount >= int.MaxValue)
                    {
                        return false;
                    }

                    // use extra zero byte to force unsigned interpretation
                    var bytes = new byte[byteCount + 1];
                    UnmanagedMemoryHelpers.Copy(PtrOrHandle, byteCount, bytes, 0);

                    // construct result and negate if necessary
                    value = new BigInteger(bytes);
                    if (SignBit != 0)
                    {
                        value = BigInteger.Negate(value);
                    }
                }

                return true;
            }
        }

        #endregion

        #region Nested type: Decoded

        [StructLayout(LayoutKind.Explicit)]
        public struct Decoded
        {
            // IMPORTANT: maintain bitwise equivalence with native struct V8Value::Decoded
            [FieldOffset(0)] private WireData data;

            public object Get()
            {
                switch (data.Type)
                {
                    case Type.Nonexistent:
                        return Nonexistent.Value;

                    case Type.Null:
                        return DBNull.Value;

                    case Type.Boolean:
                        return data.Int32Value != 0;

                    case Type.Number:
                        return data.DoubleValue;

                    case Type.String:
                        return Marshal.PtrToStringUni(data.PtrOrHandle, data.Length);

                    case Type.DateTime:
                        return DateTimeHelpers.FromUnixMilliseconds(data.DoubleValue);

                    case Type.BigInt:
                        return data.TryCreateBigInteger(out var result) ? result : null;

                    case Type.V8Object:
                        return new V8ObjectImpl((V8Object.Handle)data.PtrOrHandle, data.Subtype, data.Flags, data.IdentityHash);

                    case Type.HostObject:
                        return V8ProxyHelpers.GetHostObject(data.PtrOrHandle);

                    default:
                        return null;
                }
            }

            public static unsafe object Get(Ptr pValues, int index) => ((Decoded*)(IntPtr)pValues + index)->Get();

            public static object[] ToArray(int count, Ptr pValues)
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
            public readonly struct Ptr
            {
                private readonly IntPtr bits;

                private Ptr(IntPtr bits) => this.bits = bits;

                public static readonly Ptr Null = new(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new(bits);

                public unsafe ref Decoded AsRef() => ref Unsafe.AsRef<Decoded>(bits.ToPointer());
                public unsafe ReadOnlySpan<Decoded> ToSpan(int length) => new(bits.ToPointer(), length);

                #region Object overrides

                public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
                public override int GetHashCode() => bits.GetHashCode();

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: FastArg

        [StructLayout(LayoutKind.Explicit)]
        public readonly struct FastArg
        {
            // IMPORTANT: maintain bitwise equivalence with native struct V8Value::FastArg
            [FieldOffset(0)] private readonly WireData data;

            public bool IsUndefined() => (data.Type == Type.Undefined) || (data.Type == Type.Nonexistent);

            public bool IsNull() => data.Type == Type.Null;

            public bool TryGetBoolean(out bool value)
            {
                if (data.Type == Type.Boolean)
                {
                    value = data.Int32Value != 0;
                    return true;
                }

                value = false;
                return false;
            }

            public bool TryGetNumber(out double value)
            {
                if (data.Type == Type.Number)
                {
                    value = data.DoubleValue;
                    return true;
                }

                value = 0;
                return false;
            }

            public bool TryGetString(out string value)
            {
                if (data.Type == Type.String)
                {
                    value = Marshal.PtrToStringUni(data.PtrOrHandle, data.Length);
                    return true;
                }

                value = null;
                return false;
            }

            public unsafe bool TryGetCharSpan(out ReadOnlySpan<char> value)
            {
                if (data.Type == Type.String)
                {
                    value = new ReadOnlySpan<char>(data.PtrOrHandle.ToPointer(), data.Length);
                    return true;
                }

                value = ReadOnlySpan<char>.Empty;
                return false;
            }

            public bool TryGetDateTime(out DateTime value)
            {
                if (data.Type == Type.DateTime)
                {
                    value = DateTimeHelpers.FromUnixMilliseconds(data.DoubleValue);
                    return true;
                }

                value = default;
                return false;
            }

            public bool TryGetBigInteger(out BigInteger value)
            {
                if (data.Type == Type.BigInt)
                {
                    return data.TryCreateBigInteger(out value);
                }

                value = default;
                return false;
            }

            public bool TryGetV8Object(out V8ObjectImpl v8Object)
            {
                if (data.Type == Type.V8Object)
                {
                    v8Object = new V8ObjectImpl((V8Object.Handle)V8Entity.CloneHandle((V8Object.Handle)data.PtrOrHandle), data.Subtype, data.Flags, data.IdentityHash);
                    return true;
                }

                v8Object = null;
                return false;
            }

            public bool TryGetHostObject(out object hostObject)
            {
                if (data.Type == Type.HostObject)
                {
                    hostObject = V8ProxyHelpers.GetHostObject(data.PtrOrHandle);
                    return true;
                }

                hostObject = null;
                return false;
            }

            #region Nested type: Ptr

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly struct Ptr
            {
                private readonly IntPtr bits;

                private Ptr(IntPtr bits) => this.bits = bits;

                public static readonly Ptr Null = new(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new(bits);

                public unsafe ref FastArg AsRef() => ref Unsafe.AsRef<FastArg>(bits.ToPointer());
                public unsafe ReadOnlySpan<FastArg> ToSpan(int length) => new(bits.ToPointer(), length);

                #region Object overrides

                public override bool Equals(object obj) => (obj is Ptr ptr) && (this == ptr);
                public override int GetHashCode() => bits.GetHashCode();

                #endregion
            }

            #endregion
        }

        #endregion

        #region Nested type: FastResult

        [StructLayout(LayoutKind.Explicit)]
        public struct FastResult
        {
            // IMPORTANT: maintain bitwise equivalence with native struct V8Value::FastResult
            [FieldOffset(0)] private WireData data;

            public bool IsNonexistent => data.Type == Type.Nonexistent;

            public void SetUndefined()
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.Undefined;
            }

            public void SetNull()
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.Null;
            }

            public void SetBoolean(bool value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.Boolean;
                data.Int32Value = value ? 1 : 0;
            }

            public void SetNumber(double value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.Number;
                data.DoubleValue = value;
            }

            public void SetString(string value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.String;
                data.Length = value.Length;
                data.PtrOrHandle = CreateStringData(value);
            }

            public void SetString(in ReadOnlySpan<char> value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.String;
                data.Length = value.Length;
                data.PtrOrHandle = CreateStringData(value);
            }

            public void SetDateTime(DateTime value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.DateTime;
                data.DoubleValue = value.ToUnixMilliseconds();
            }

            public void SetBigInteger(BigInteger value)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.BigInt;
                data.PtrOrHandle = CreateBigIntData(value, out data.SignBit, out data.Length);
            }

            public void SetV8Object(V8ObjectImpl v8Object)
            {
                Debug.Assert(IsNonexistent);
                data.Type = Type.V8Object;
                data.Subtype = v8Object.Subtype;
                data.Flags = v8Object.Flags;
                data.IdentityHash = v8Object.IdentityHash;
                data.PtrOrHandle = (IntPtr)V8Entity.CloneHandle(v8Object.Handle);
            }

            public void SetHostObject(IHostItem hostObject)
            {
                var subtype = Subtype.None;
                var flags = Flags.None;

                if (hostObject is V8FastHostItem fastHostItem)
                {
                    flags = Flags.Fast;
                    if (fastHostItem.IsInvocable)
                    {
                        subtype = Subtype.Function;
                    }
                }

                Debug.Assert(IsNonexistent);
                data.Type = Type.HostObject;
                data.Subtype = subtype;
                data.Flags = flags;
                data.PtrOrHandle = V8ProxyHelpers.AddRefHostObject(hostObject);
            }

            private static unsafe IntPtr CreateStringData(string value)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return IntPtr.Zero;
                }

                var length = value.Length;

                var bufferSize = checked((length + 1) * sizeof(char));
                var pBuffer = V8SplitProxyNative.InvokeRaw(static (instance, size) => instance.Memory_AllocateZeroed(size), (UIntPtr)bufferSize);

                if (length > 0)
                {
                    fixed (char* pValue = value)
                    {
                        var bytesToCopy = bufferSize - sizeof(char);
                        Buffer.MemoryCopy(pValue, pBuffer.ToPointer(), bytesToCopy, bytesToCopy);
                    }
                }

                return pBuffer;
            }

            private static unsafe IntPtr CreateStringData(in ReadOnlySpan<char> value)
            {
                if (value.IsEmpty)
                {
                    return IntPtr.Zero;
                }

                var length = value.Length;

                var bufferSize = checked((length + 1) * sizeof(char));
                var pBuffer = V8SplitProxyNative.InvokeRaw(static (instance, size) => instance.Memory_AllocateZeroed(size), (UIntPtr)bufferSize);

                if (length > 0)
                {
                    fixed (char* pValue = value)
                    {
                        var bytesToCopy = bufferSize - sizeof(char);
                        Buffer.MemoryCopy(pValue, pBuffer.ToPointer(), bytesToCopy, bytesToCopy);
                    }
                }

                return pBuffer;
            }

            private static IntPtr CreateBigIntData(BigInteger value, out short signBit, out int wordCount)
            {
                signBit = positiveOrZero;
                if (value.Sign < 0)
                {
                    signBit = negative;
                    value = BigInteger.Negate(value);
                }

                var bytes = value.ToByteArray();
                var arrayLength = bytes.Length;

                wordCount = checked(arrayLength + sizeof(ulong) - 1) / sizeof(ulong);
                var bufferSize = wordCount * sizeof(ulong);
                var pWords = V8SplitProxyNative.InvokeRaw(static (instance, size) => instance.Memory_AllocateZeroed(size), (UIntPtr)bufferSize);

                UnmanagedMemoryHelpers.Copy(bytes, 0, (ulong)arrayLength, pWords);
                return pWords;
            }

            #region Nested type: Ptr

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public readonly struct Ptr
            {
                private readonly IntPtr bits;

                private Ptr(IntPtr bits) => this.bits = bits;

                public static readonly Ptr Null = new(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new(bits);

                public unsafe ref FastResult AsRef() => ref Unsafe.AsRef<FastResult>(bits.ToPointer());
                public unsafe ReadOnlySpan<FastResult> ToSpan(int length) => new(bits.ToPointer(), length);

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
            var infoCtx = (
                pProfile,
                hEntity,
                name: (string)null,
                startTimestamp: 0UL,
                endTimestamp: 0UL,
                sampleCount: 0,
                pRootNode: Node.Ptr.Null
            );

            V8SplitProxyNative.InvokeNoThrow(
                static (instance, pInfoCtx) =>
                {
                    ref var infoCtx = ref pInfoCtx.AsRef();
                    instance.V8CpuProfile_GetInfo(
                        infoCtx.pProfile,
                        infoCtx.hEntity,
                        out infoCtx.name,
                        out infoCtx.startTimestamp,
                        out infoCtx.endTimestamp,
                        out infoCtx.sampleCount,
                        out infoCtx.pRootNode
                    );
                },
                StructPtr.FromRef(ref infoCtx)
            );

            profile.Name = infoCtx.name;
            profile.StartTimestamp = infoCtx.startTimestamp;
            profile.EndTimestamp = infoCtx.endTimestamp;

            if (infoCtx.pRootNode != Node.Ptr.Null)
            {
                profile.RootNode = CreateNode(hEntity, infoCtx.pRootNode);
            }

            if (infoCtx.sampleCount > 0)
            {
                var samples = new List<V8.V8CpuProfile.Sample>(infoCtx.sampleCount);

                for (var index = 0; index < infoCtx.sampleCount; index++)
                {
                    var sampleCtx = (
                        pProfile,
                        sampleIndex: index,
                        nodeId: 0UL,
                        timestamp: 0UL
                    );

                    var found = V8SplitProxyNative.InvokeNoThrow(
                        static (instance, pSampleCtx) =>
                        {
                            ref var sampleCtx = ref pSampleCtx.AsRef();
                            return instance.V8CpuProfile_GetSample(
                                sampleCtx.pProfile,
                                sampleCtx.sampleIndex,
                                out sampleCtx.nodeId,
                                out sampleCtx.timestamp
                            );
                        },
                        StructPtr.FromRef(ref sampleCtx)
                    );

                    if (found)
                    {
                        var node = profile.FindNode(sampleCtx.nodeId);
                        if (node is not null)
                        {
                            samples.Add(new V8.V8CpuProfile.Sample { Node = node, Timestamp = sampleCtx.timestamp });
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
            var infoCtx = (
                pNode,
                hEntity,
                nodeId: 0UL,
                scriptId: 0L,
                scriptName: (string)null,
                functionName: (string)null,
                bailoutReason: (string)null,
                lineNumber: 0L,
                columnNumber: 0L,
                hitCount: 0UL,
                hitLineCount: 0U,
                childCount: 0
            );

            V8SplitProxyNative.InvokeNoThrow(
                static (instance, pInfoCtx) =>
                {
                    ref var infoCtx = ref pInfoCtx.AsRef();
                    instance.V8CpuProfileNode_GetInfo(
                        infoCtx.pNode,
                        infoCtx.hEntity,
                        out infoCtx.nodeId,
                        out infoCtx.scriptId,
                        out infoCtx.scriptName,
                        out infoCtx.functionName,
                        out infoCtx.bailoutReason,
                        out infoCtx.lineNumber,
                        out infoCtx.columnNumber,
                        out infoCtx.hitCount,
                        out infoCtx.hitLineCount,
                        out infoCtx.childCount
                    );
                },
                StructPtr.FromRef(ref infoCtx)
            );

            var node = new V8.V8CpuProfile.Node
            {
                NodeId = infoCtx.nodeId,
                ScriptId = infoCtx.scriptId,
                ScriptName = infoCtx.scriptName,
                FunctionName = infoCtx.functionName,
                BailoutReason = infoCtx.bailoutReason,
                LineNumber = infoCtx.lineNumber,
                ColumnNumber = infoCtx.columnNumber,
                HitCount = infoCtx.hitCount,
            };

            if (infoCtx.hitLineCount > 0)
            {
                var hitLinesCtx = (
                    pNode,
                    lineNumbers: (int[])null,
                    hitCounts: (uint[])null
                );

                var found = V8SplitProxyNative.InvokeNoThrow(
                    static (instance, pHitLinesCtx) =>
                    {
                        ref var hitLinesCtx = ref pHitLinesCtx.AsRef();
                        return instance.V8CpuProfileNode_GetHitLines(
                            hitLinesCtx.pNode,
                            out hitLinesCtx.lineNumbers,
                            out hitLinesCtx.hitCounts
                        );
                    },
                    StructPtr.FromRef(ref hitLinesCtx)
                );

                if (found)
                {
                    var actualHitLineCount = Math.Min(hitLinesCtx.lineNumbers.Length, hitLinesCtx.hitCounts.Length);
                    if (actualHitLineCount > 0)
                    {
                        var hitLines = new V8.V8CpuProfile.Node.HitLine[actualHitLineCount];

                        for (var index = 0; index < actualHitLineCount; index++)
                        {
                            hitLines[index].LineNumber = hitLinesCtx.lineNumbers[index];
                            hitLines[index].HitCount = hitLinesCtx.hitCounts[index];
                        }

                        node.HitLines = new ReadOnlyCollection<V8.V8CpuProfile.Node.HitLine>(hitLines);
                    }
                }
            }

            if (infoCtx.childCount > 0)
            {
                var childNodes = new List<V8.V8CpuProfile.Node>(infoCtx.childCount);

                for (var index = 0; index < infoCtx.childCount; index++)
                {
                    var childIndex = index;
                    var pChildNode = V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8CpuProfileNode_GetChildNode(ctx.pNode, ctx.childIndex), (pNode, childIndex));
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

            public static readonly Ptr Null = new(IntPtr.Zero);

            public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
            public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

            public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
            public static explicit operator Ptr(IntPtr bits) => new(bits);

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

                public static readonly Ptr Null = new(IntPtr.Zero);

                public static bool operator ==(Ptr left, Ptr right) => left.bits == right.bits;
                public static bool operator !=(Ptr left, Ptr right) => left.bits != right.bits;

                public static explicit operator IntPtr(Ptr ptr) => ptr.bits;
                public static explicit operator Ptr(IntPtr bits) => new(bits);

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
        public static Handle CloneHandle(Handle handle)
        {
            return V8SplitProxyNative.InvokeRaw(static (instance, handle) => instance.V8Entity_CloneHandle(handle), handle);
        }

        #region Nested type: Handle

        public readonly struct Handle
        {
            private readonly IntPtr guts;

            private Handle(IntPtr guts) => this.guts = guts;

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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

            public static readonly Handle Empty = new(IntPtr.Zero);

            public static bool operator ==(Handle left, Handle right) => left.guts == right.guts;
            public static bool operator !=(Handle left, Handle right) => left.guts != right.guts;

            public static explicit operator IntPtr(Handle handle) => handle.guts;
            public static explicit operator Handle(IntPtr guts) => new(guts);

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
