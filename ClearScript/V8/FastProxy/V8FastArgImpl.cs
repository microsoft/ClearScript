// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    internal static class V8FastArgImpl
    {
        public static object InitializeObject(V8ScriptEngine engine, in V8Value.FastArg arg)
        {
            if (arg.IsUndefined())
            {
                var importValue = engine.UndefinedImportValue;
                return (importValue is not Undefined) ? importValue : Nonexistent.Value;
            }

            if (arg.IsNull())
            {
                return engine.NullImportValue ?? Nonexistent.Value;
            }

            if (arg.TryGetV8Object(out var v8Object))
            {
                return engine.MarshalToHost(v8Object, false);
            }

            if (arg.TryGetHostObject(out var hostObject))
            {
                return engine.MarshalToHost(hostObject, false);
            }

            return Nonexistent.Value;
        }

        public static bool IsFalsy(in V8Value.FastArg arg, object obj)
        {
            if (IsNull(arg, obj) || IsUndefined(arg, obj))
            {
                return true;
            }

            if (TryGet(arg, obj, out bool boolValue))
            {
                return !boolValue;
            }

            if (TryGet(arg, obj, out double doubleValue))
            {
                return (doubleValue == 0) || double.IsNaN(doubleValue);
            }

            if (TryGet(arg, obj, out BigInteger bigIntegerValue))
            {
                return bigIntegerValue.IsZero;
            }

            if (TryGet(arg, obj, out ReadOnlySpan<char> charSpanValue))
            {
                return charSpanValue.IsEmpty;
            }

            return false;
        }

        public static bool IsTruthy(in V8Value.FastArg arg, object obj) => !IsFalsy(arg, obj);

        public static bool IsUndefined(in V8Value.FastArg arg, object obj) => (arg.IsUndefined() && (obj is Nonexistent)) || (obj is Undefined);

        public static bool IsNull(in V8Value.FastArg arg, object obj) => (arg.IsNull() && (obj is Nonexistent)) || (obj is null);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out bool value) => arg.TryGetBoolean(out value) || TryGetFromObject(obj, out value, out _);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out char value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out sbyte value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out byte value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out short value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out ushort value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out int value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out uint value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out long value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out ulong value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out float value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out double value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out decimal value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out string value) => arg.TryGetString(out value) || TryGetFromObject(obj, out value, out _);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out ReadOnlySpan<char> value)
        {
            if (arg.TryGetCharSpan(out value))
            {
                return true;
            }

            if (TryGetFromObject(obj, out string stringValue, out _))
            {
                value = stringValue.AsSpan();
                return true;
            }

            return false;
        }

        public static bool TryGet(in V8Value.FastArg arg, object obj, out DateTime value) => arg.TryGetDateTime(out value) || TryGetFromObject(obj, out value, out _);

        public static bool TryGet(in V8Value.FastArg arg, object obj, out BigInteger value) => TryGetNumber(arg, obj, out value);

        public static bool TryGet<T>(in V8Value.FastArg arg, object obj, out T? value) where T : struct
        {
            if (IsNull(arg, obj) || IsUndefined(arg, obj))
            {
                value = null;
                return true;
            }

            if (TryGet(arg, obj, out T structValue))
            {
                value = structValue;
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGet<T>(in V8Value.FastArg arg, object obj, out T value)
        {
            value = default;
            var type = typeof(T);

            if (type == typeof(bool))
            {
                return TryGet(arg, obj, out Unsafe.As<T, bool>(ref value));
            }

            if (type == typeof(char))
            {
                return TryGet(arg, obj, out Unsafe.As<T, char>(ref value));
            }

            if (type == typeof(sbyte))
            {
                return TryGet(arg, obj, out Unsafe.As<T, sbyte>(ref value));
            }

            if (type == typeof(byte))
            {
                return TryGet(arg, obj, out Unsafe.As<T, byte>(ref value));
            }

            if (type == typeof(short))
            {
                return TryGet(arg, obj, out Unsafe.As<T, short>(ref value));
            }

            if (type == typeof(ushort))
            {
                return TryGet(arg, obj, out Unsafe.As<T, ushort>(ref value));
            }

            if (type == typeof(int))
            {
                return TryGet(arg, obj, out Unsafe.As<T, int>(ref value));
            }

            if (type == typeof(uint))
            {
                return TryGet(arg, obj, out Unsafe.As<T, uint>(ref value));
            }

            if (type == typeof(long))
            {
                return TryGet(arg, obj, out Unsafe.As<T, long>(ref value));
            }

            if (type == typeof(ulong))
            {
                return TryGet(arg, obj, out Unsafe.As<T, ulong>(ref value));
            }

            if (type == typeof(float))
            {
                return TryGet(arg, obj, out Unsafe.As<T, float>(ref value));
            }

            if (type == typeof(double))
            {
                return TryGet(arg, obj, out Unsafe.As<T, double>(ref value));
            }

            if (type == typeof(decimal))
            {
                return TryGet(arg, obj, out Unsafe.As<T, decimal>(ref value));
            }

            if (type == typeof(string))
            {
                return TryGet(arg, obj, out Unsafe.As<T, string>(ref value));
            }

            if (type == typeof(DateTime))
            {
                return TryGet(arg, obj, out Unsafe.As<T, DateTime>(ref value));
            }

            if (type == typeof(BigInteger))
            {
                return TryGet(arg, obj, out Unsafe.As<T, BigInteger>(ref value));
            }

            if (Nullable.GetUnderlyingType(type) is {} underlyingType)
            {
                return NullableHelpers.TryGet(underlyingType, arg, obj, out value);
            }

            if (TryGetFromObject(obj, out value, out var isObject))
            {
                return true;
            }

            if (isObject)
            {
                return false;
            }

            {
                if (arg.IsUndefined())
                {
                    if (Undefined.Value is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            {
                if (arg.TryGetBoolean(out var boolValue))
                {
                    if (boolValue is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            {
                if (arg.TryGetNumber(out var doubleValue))
                {
                    if (doubleValue is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            {
                if (arg.TryGetString(out var stringValue))
                {
                    if (stringValue is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            {
                if (arg.TryGetDateTime(out var dateTimeValue))
                {
                    if (dateTimeValue is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            {
                if (arg.TryGetBigInteger(out var bigIntegerValue))
                {
                    if (bigIntegerValue is T checkedValue)
                    {
                        value = checkedValue;
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public static bool GetBoolean(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out bool result), result, kind, name);

        public static char GetChar(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out char result), result, kind, name);

        public static sbyte GetSByte(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out sbyte result), result, kind, name);

        public static byte GetByte(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out byte result), result, kind, name);

        public static short GetInt16(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out short result), result, kind, name);

        public static ushort GetUInt16(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out ushort result), result, kind, name);

        public static int GetInt32(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out int result), result, kind, name);

        public static uint GetUInt32(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out uint result), result, kind, name);

        public static long GetInt64(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out long result), result, kind, name);

        public static ulong GetUInt64(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out ulong result), result, kind, name);

        public static float GetSingle(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out float result), result, kind, name);

        public static double GetDouble(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out double result), result, kind, name);

        public static decimal GetDecimal(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out decimal result), result, kind, name);

        public static string GetString(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => IsNull(arg, obj) ? null : ReturnOrThrow(TryGet(arg, obj, out string result), result, kind, name);

        public static ReadOnlySpan<char> GetCharSpan(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out ReadOnlySpan<char> result), result, kind, name);

        public static DateTime GetDateTime(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out DateTime result), result, kind, name);

        public static BigInteger GetBigInteger(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => ReturnOrThrow(TryGet(arg, obj, out BigInteger result), result, kind, name);

        public static T? GetNullable<T>(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) where T : struct => ReturnOrThrow(TryGet(arg, obj, out T? result), result, kind, name);

        public static T Get<T>(in V8Value.FastArg arg, object obj, V8FastArgKind kind, string name) => (IsNull(arg, obj) && (default(T) is null)) ? default : ReturnOrThrow(TryGet(arg, obj, out T result), result, kind, name);

        private static bool TryGetFromObject<T>(object obj, out T value, out bool isObject)
        {
            isObject = obj is not Nonexistent;
            if (isObject)
            {
                if (obj is T checkedValue)
                {
                    value = checkedValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryGetNumber<T>(in V8Value.FastArg arg, object obj, out T value) where T : struct
        {
            if (arg.TryGetNumber(out var doubleValue))
            {
                return TryGetNumberFromDouble(doubleValue, out value);
            }

            if (arg.TryGetBigInteger(out var bigIntegerValue))
            {
                return TryGetNumberFromBigInteger(bigIntegerValue, out value);
            }

            if (obj is not Nonexistent)
            {
                return TryGetNumberFromObject(obj, out value);
            }

            value = default;
            return false;
        }

        private static bool TryGetNumberFromObject<T>(object obj, out T value) where T : struct
        {
            if (obj is char charValue)
            {
                return TryGetNumberFromInt64(charValue, out value);
            }

            if (obj is sbyte sbyteValue)
            {
                return TryGetNumberFromInt64(sbyteValue, out value);
            }

            if (obj is byte byteValue)
            {
                return TryGetNumberFromInt64(byteValue, out value);
            }

            if (obj is short shortValue)
            {
                return TryGetNumberFromInt64(shortValue, out value);
            }

            if (obj is ushort ushortValue)
            {
                return TryGetNumberFromInt64(ushortValue, out value);
            }

            if (obj is int intValue)
            {
                return TryGetNumberFromInt64(intValue, out value);
            }

            if (obj is uint uintValue)
            {
                return TryGetNumberFromInt64(uintValue, out value);
            }

            if (obj is long longValue)
            {
                return TryGetNumberFromInt64(longValue, out value);
            }

            if (obj is ulong ulongValue)
            {
                return TryGetNumberFromUInt64(ulongValue, out value);
            }

            if (obj is float floatValue)
            {
                return TryGetNumberFromDouble(floatValue, out value);
            }

            if (obj is double doubleValue)
            {
                return TryGetNumberFromDouble(doubleValue, out value);
            }

            if (obj is decimal decimalValue)
            {
                return TryGetNumberFromDecimal(decimalValue, out value);
            }

            if (obj is BigInteger bigIntegerValue)
            {
                return TryGetNumberFromBigInteger(bigIntegerValue, out value);
            }

            value = default;
            return false;
        }

        private static bool TryGetNumberFromInt64<T>(long longValue, out T value) where T : struct
        {
            if (TryGetIntegerFromInt64(longValue, out value))
            {
                return true;
            }

            var type = typeof(T);
            if (type == typeof(float))
            {
                if ((longValue >= -MiscHelpers.MaxInt32InSingle) && (longValue <= MiscHelpers.MaxInt32InSingle))
                {
                    Unsafe.As<T, float>(ref value) = longValue;
                    return true;
                }
            }
            else if (type == typeof(double))
            {
                if ((longValue >= -MiscHelpers.MaxInt64InDouble) && (longValue <= MiscHelpers.MaxInt64InDouble))
                {
                    Unsafe.As<T, double>(ref value) = longValue;
                    return true;
                }
            }
            else if (type == typeof(decimal))
            {
                Unsafe.As<T, decimal>(ref value) = longValue;
                return true;
            }

            return false;
        }

        private static bool TryGetNumberFromUInt64<T>(ulong ulongValue, out T value) where T : struct
        {
            if (TryGetIntegerFromUInt64(ulongValue, out value))
            {
                return true;
            }

            var type = typeof(T);

            if (type == typeof(float))
            {
                if (ulongValue <= MiscHelpers.MaxInt32InSingle)
                {
                    Unsafe.As<T, float>(ref value) = ulongValue;
                    return true;
                }
            }
            else if (type == typeof(double))
            {
                if (ulongValue <= MiscHelpers.MaxInt64InDouble)
                {
                    Unsafe.As<T, double>(ref value) = ulongValue;
                    return true;
                }
            }
            else if (type == typeof(decimal))
            {
                Unsafe.As<T, decimal>(ref value) = ulongValue;
                return true;
            }

            return false;
        }

        private static bool TryGetNumberFromDouble<T>(double doubleValue, out T value) where T : struct
        {
            if (Math.Abs(doubleValue) <= MiscHelpers.MaxInt64InDoubleAsDouble)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator

                var truncatedValue = Math.Truncate(doubleValue);
                if ((truncatedValue == doubleValue) && TryGetIntegerFromInt64((long)truncatedValue, out value))
                {
                    return true;
                }

                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            return TryGetFloatingPointFromDouble(doubleValue, out value);
        }

        private static bool TryGetNumberFromDecimal<T>(decimal decimalValue, out T value) where T : struct
        {
            if (Math.Abs(decimalValue) <= MiscHelpers.MaxBigIntegerInDecimalAsDecimal)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator

                var truncatedValue = Math.Truncate(decimalValue);
                if ((truncatedValue == decimalValue) && TryGetIntegerFromBigInteger((BigInteger)truncatedValue, out value))
                {
                    return true;
                }

                // ReSharper restore CompareOfFloatsByEqualityOperator
            }

            return TryGetFloatingPointFromDecimal(decimalValue, out value);
        }

        private static bool TryGetNumberFromBigInteger<T>(BigInteger bigIntegerValue, out T value) where T : struct
        {
            if (TryGetIntegerFromBigInteger(bigIntegerValue, out value))
            {
                return true;
            }

            var type = typeof(T);
            if (type == typeof(float))
            {
                if (BigInteger.Abs(bigIntegerValue) <= MiscHelpers.MaxInt32InSingle)
                {
                    Unsafe.As<T, float>(ref value) = (float)bigIntegerValue;
                    return true;
                }
            }
            else if (type == typeof(double))
            {
                if (BigInteger.Abs(bigIntegerValue) <= MiscHelpers.MaxInt64InDouble)
                {
                    Unsafe.As<T, double>(ref value) = (double)bigIntegerValue;
                    return true;
                }
            }
            else if (type == typeof(decimal))
            {
                if (BigInteger.Abs(bigIntegerValue) <= MiscHelpers.MaxBigIntegerInDecimal)
                {
                    Unsafe.As<T, decimal>(ref value) = (decimal)bigIntegerValue;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetIntegerFromInt64<T>(long longValue, out T value) where T : struct
        {
            value = default;

            var type = typeof(T);
            if (type == typeof(char))
            {
                if ((longValue >= char.MinValue) && (longValue <= char.MaxValue))
                {
                    Unsafe.As<T, char>(ref value) = (char)longValue;
                    return true;
                }
            }
            else if (type == typeof(sbyte))
            {
                if ((longValue >= sbyte.MinValue) && (longValue <= sbyte.MaxValue))
                {
                    Unsafe.As<T, sbyte>(ref value) = (sbyte)longValue;
                    return true;
                }
            }
            else if (type == typeof(byte))
            {
                if ((longValue >= 0) && (longValue <= byte.MaxValue))
                {
                    Unsafe.As<T, byte>(ref value) = (byte)longValue;
                    return true;
                }
            }
            else if (type == typeof(short))
            {
                if ((longValue >= short.MinValue) && (longValue <= short.MaxValue))
                {
                    Unsafe.As<T, short>(ref value) = (short)longValue;
                    return true;
                }
            }
            else if (type == typeof(ushort))
            {
                if ((longValue >= 0) && (longValue <= ushort.MaxValue))
                {
                    Unsafe.As<T, ushort>(ref value) = (ushort)longValue;
                    return true;
                }
            }
            else if (type == typeof(int))
            {
                if ((longValue >= int.MinValue) && (longValue <= int.MaxValue))
                {
                    Unsafe.As<T, int>(ref value) = (int)longValue;
                    return true;
                }
            }
            else if (type == typeof(uint))
            {
                if ((longValue >= 0) && (longValue <= uint.MaxValue))
                {
                    Unsafe.As<T, uint>(ref value) = (uint)longValue;
                    return true;
                }
            }
            else if (type == typeof(long))
            {
                Unsafe.As<T, long>(ref value) = longValue;
                return true;
            }
            else if (type == typeof(ulong))
            {
                if (longValue >= 0)
                {
                    Unsafe.As<T, ulong>(ref value) = (ulong)longValue;
                    return true;
                }
            }
            else if (type == typeof(BigInteger))
            {
                Unsafe.As<T, BigInteger>(ref value) = longValue;
                return true;
            }

            return false;
        }

        private static bool TryGetIntegerFromUInt64<T>(ulong ulongValue, out T value) where T : struct
        {
            if (ulongValue <= long.MaxValue)
            {
                return TryGetIntegerFromInt64((long)ulongValue, out value);
            }

            value = default;

            if (typeof(T) == typeof(ulong))
            {
                Unsafe.As<T, ulong>(ref value) = ulongValue;
                return true;
            }

            if (typeof(T) == typeof(BigInteger))
            {
                Unsafe.As<T, BigInteger>(ref value) = ulongValue;
                return true;
            }

            return false;
        }

        private static bool TryGetIntegerFromBigInteger<T>(BigInteger bigIntegerValue, out T value) where T : struct
        {
            if ((bigIntegerValue >= long.MinValue) && (bigIntegerValue <= long.MaxValue))
            {
                return TryGetIntegerFromInt64((long)bigIntegerValue, out value);
            }

            if ((bigIntegerValue >= 0) && (bigIntegerValue <= ulong.MaxValue))
            {
                return TryGetIntegerFromUInt64((ulong)bigIntegerValue, out value);
            }

            value = default;

            if (typeof(T) == typeof(BigInteger))
            {
                Unsafe.As<T, BigInteger>(ref value) = bigIntegerValue;
                return true;
            }

            return false;
        }

        private static bool TryGetFloatingPointFromDouble<T>(double doubleValue, out T value) where T : struct
        {
            value = default;
            var type = typeof(T);

            if (type == typeof(float))
            {
                if ((doubleValue >= float.MinValue) && (doubleValue <= float.MaxValue))
                {
                    Unsafe.As<T, float>(ref value) = (float)doubleValue;
                    return true;
                }

                return false;
            }

            if (type == typeof(double))
            {
                Unsafe.As<T, double>(ref value) = doubleValue;
                return true;
            }

            if (type == typeof(decimal))
            {
                return MiscHelpers.Try(out Unsafe.As<T, decimal>(ref value), static doubleValue => (decimal)doubleValue, doubleValue);
            }

            return false;
        }

        private static bool TryGetFloatingPointFromDecimal<T>(decimal decimalValue, out T value) where T : struct
        {
            value = default;
            var type = typeof(T);

            if (type == typeof(float))
            {
                Unsafe.As<T, float>(ref value) = (float)decimalValue;
                return true;
            }

            if (type == typeof(double))
            {
                Unsafe.As<T, double>(ref value) = (double)decimalValue;
                return true;
            }

            if (type == typeof(decimal))
            {
                Unsafe.As<T, decimal>(ref value) = decimalValue;
                return true;
            }

            return false;
        }

        private static T ReturnOrThrow<T>(bool succeeded, T result, V8FastArgKind kind, string name) => succeeded ? result : throw CreateArgumentException(kind, name);

        private static ReadOnlySpan<char> ReturnOrThrow(bool succeeded, in ReadOnlySpan<char> result, V8FastArgKind kind, string name) => succeeded ? result : throw CreateArgumentException(kind, name);

        private static ArgumentException CreateArgumentException(V8FastArgKind kind, string name)
        {
            return kind switch
            {
                V8FastArgKind.PropertyValue => new ArgumentException($"Invalid value specified for property '{name.ToNonNull("[unknown]")}'"),
                V8FastArgKind.MethodArg => new ArgumentException($"Invalid argument specified for method parameter '{name.ToNonNull("[unnamed]")}'"),
                _ => new ArgumentException($"Invalid argument specified for function parameter '{name.ToNonNull("[unnamed]")}'")
            };
        }
    }
}
