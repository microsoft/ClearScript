// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Numerics;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a result returned from a fast host property getter, method, or function.
    /// </summary>
    public readonly ref struct V8FastResult
    {
        private readonly V8ScriptEngine engine;
        private readonly HostItemFlags flags;
        private readonly V8Value.FastResult.Ptr pResult;

        internal V8FastResult(V8ScriptEngine engine, HostItemFlags flags, V8Value.FastResult.Ptr pResult)
        {
            this.engine = engine;
            this.flags = flags;
            this.pResult = pResult;
        }

        /// <summary>
        /// Determines whether the result has been set.
        /// </summary>
        /// <remarks>
        /// Once set, a result cannot be modified.
        /// </remarks>
        public bool IsSet => !pResult.AsRef().IsNonexistent;

        /// <summary>
        /// Sets the result to <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>.
        /// </summary>
        public void SetUndefined()
        {
            VerifyUnset();
            pResult.AsRef().SetUndefined();
        }

        /// <summary>
        /// Sets the result to <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/null">null</see></c>.
        /// </summary>
        public void SetNull()
        {
            SetFromObject(engine.PrepareResult(null, typeof(object), ScriptMemberFlags.None, false));
        }

        /// <summary>
        /// Sets the result to a <c><see cref="bool"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="bool"/></c> value to assign to the result.</param>
        public void Set(bool value)
        {
            VerifyUnset();
            pResult.AsRef().SetBoolean(value);
        }

        /// <summary>
        /// Sets the result to a <c><see cref="char"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="char"/></c> value to assign to the result.</param>
        public void Set(char value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="sbyte"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="sbyte"/></c> value to assign to the result.</param>
        public void Set(sbyte value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="byte"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="byte"/></c> value to assign to the result.</param>
        public void Set(byte value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="short"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="short"/></c> value to assign to the result.</param>
        public void Set(short value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="ushort"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="ushort"/></c> value to assign to the result.</param>
        public void Set(ushort value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="int"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="int"/></c> value to assign to the result.</param>
        public void Set(int value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="uint"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="uint"/></c> value to assign to the result.</param>
        public void Set(uint value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="long"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="long"/></c> value to assign to the result.</param>
        public void Set(long value)
        {
            if (engine.Flags.HasAllFlags(V8ScriptEngineFlags.MarshalAllInt64AsBigInt))
            {
                Set((BigInteger)value);
            }
            else if (engine.Flags.HasAllFlags(V8ScriptEngineFlags.MarshalUnsafeInt64AsBigInt) && ((value < -MiscHelpers.MaxInt64InDouble) || (value > MiscHelpers.MaxInt64InDouble)))
            {
                Set((BigInteger)value);
            }
            else
            {
                SetNumber(value);
            }
        }

        /// <summary>
        /// Sets the result to a <c><see cref="ulong"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="ulong"/></c> value to assign to the result.</param>
        public void Set(ulong value)
        {
            if (engine.Flags.HasAllFlags(V8ScriptEngineFlags.MarshalAllInt64AsBigInt))
            {
                Set((BigInteger)value);
            }
            else if (engine.Flags.HasAllFlags(V8ScriptEngineFlags.MarshalUnsafeInt64AsBigInt) && (value > MiscHelpers.MaxInt64InDouble))
            {
                Set((BigInteger)value);
            }
            else
            {
                SetNumber(value);
            }
        }

        /// <summary>
        /// Sets the result to a <c><see cref="float"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="float"/></c> value to assign to the result.</param>
        public void Set(float value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="double"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="double"/></c> value to assign to the result.</param>
        public void Set(double value) => SetNumber(value);

        /// <summary>
        /// Sets the result to a <c><see cref="decimal"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="decimal"/></c> value to assign to the result.</param>
        public void Set(decimal value) => SetNumber((double)value);

        /// <summary>
        /// Sets the result to a string value.
        /// </summary>
        /// <param name="value">The string value to assign to the result.</param>
        public void Set(string value)
        {
            VerifyUnset();
            if (value is not null)
            {
                pResult.AsRef().SetString(value);
            }
            else
            {
                SetNull();
            }
        }

        /// <summary>
        /// Sets the result to a string value, specified as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.
        /// </summary>
        /// <param name="value">The string value to assign to the result, specified as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.</param>
        public void Set(ReadOnlySpan<char> value)
        {
            VerifyUnset();
            pResult.AsRef().SetString(value);
        }

        /// <summary>
        /// Sets the result to a <c><see cref="DateTime"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="DateTime"/></c> value to assign to the result.</param>
        public void Set(DateTime value)
        {
            if (engine.Flags.HasAllFlags(V8ScriptEngineFlags.EnableDateTimeConversion))
            {
                VerifyUnset();
                pResult.AsRef().SetDateTime(value);
            }
            else
            {
                SetFromObject(value);
            }
        }

        /// <summary>
        /// Sets the result to a <c><see cref="BigInteger"/></c> value.
        /// </summary>
        /// <param name="value">The <c><see cref="BigInteger"/></c> value to assign to the result.</param>
        public void Set(BigInteger value)
        {
            VerifyUnset();
            pResult.AsRef().SetBigInteger(value);
        }

        /// <summary>
        /// Sets the result to a nullable value of the given underlying type.
        /// </summary>
        /// <typeparam name="T">The underlying type of the nullable value to assign.</typeparam>
        /// <param name="value">A nullable value of underlying type <typeparamref name="T"/> to assign to the result.</param>
        public void Set<T>(T? value) where T : struct => SetNullable(value);

        /// <summary>
        /// Sets the result to a value of the given type.
        /// </summary>
        /// <typeparam name="T">The type of value to assign to the result.</typeparam>
        /// <param name="value">A value of type <typeparamref name="T"/> to assign to the result.</param>
        public void Set<T>(T value)
        {
            var type = typeof(T);
            if (type == typeof(Undefined))
            {
                if (value is Undefined)
                {
                    SetUndefined();
                }
            }
            else if (type == typeof(bool))
            {
                if (value is bool boolValue)
                {
                    Set(boolValue);
                }
            }
            else if (type == typeof(char))
            {
                if (value is char charValue)
                {
                    Set(charValue);
                }
            }
            else if (type == typeof(sbyte))
            {
                if (value is sbyte sbyteValue)
                {
                    Set(sbyteValue);
                }
            }
            else if (type == typeof(byte))
            {
                if (value is byte byteValue)
                {
                    Set(byteValue);
                }
            }
            else if (type == typeof(short))
            {
                if (value is short shortValue)
                {
                    Set(shortValue);
                }
            }
            else if (type == typeof(ushort))
            {
                if (value is ushort ushortValue)
                {
                    Set(ushortValue);
                }
            }
            else if (type == typeof(int))
            {
                if (value is int intValue)
                {
                    Set(intValue);
                }
            }
            else if (type == typeof(uint))
            {
                if (value is uint uintValue)
                {
                    Set(uintValue);
                }
            }
            else if (type == typeof(long))
            {
                if (value is long longValue)
                {
                    Set(longValue);
                }
            }
            else if (type == typeof(ulong))
            {
                if (value is ulong ulongValue)
                {
                    Set(ulongValue);
                }
            }
            else if (type == typeof(float))
            {
                if (value is float floatValue)
                {
                    Set(floatValue);
                }
            }
            else if (type == typeof(double))
            {
                if (value is double doubleValue)
                {
                    Set(doubleValue);
                }
            }
            else if (type == typeof(decimal))
            {
                if (value is decimal decimalValue)
                {
                    Set(decimalValue);
                }
            }
            else if (type == typeof(string))
            {
                if (value is string stringValue)
                {
                    Set(stringValue);
                }
                else if (value is null)
                {
                    SetNull();
                }
            }
            else if (type == typeof(DateTime))
            {
                if (value is DateTime dateTimeValue)
                {
                    Set(dateTimeValue);
                }
            }
            else if (type == typeof(BigInteger))
            {
                if (value is BigInteger bigIntegerValue)
                {
                    Set(bigIntegerValue);
                }
            }
            else if (Nullable.GetUnderlyingType(type) is {} underlyingType)
            {
                NullableHelpers.Set(underlyingType, this, ref value);
            }
            else
            {
                SetFromObject(engine.PrepareResult(value, ScriptMemberFlags.None, false));
            }
        }

        internal void SetNullable<T>(in T? value) where T : struct
        {
            if (value.HasValue)
            {
                Set(value.Value);
            }
            else
            {
                SetNull();
            }
        }

        private void SetNullInternal()
        {
            VerifyUnset();
            pResult.AsRef().SetNull();
        }

        private void SetNumber(double value)
        {
            VerifyUnset();
            pResult.AsRef().SetNumber(value);
        }

        private void SetFromObject(object obj)
        {
            var result = engine.MarshalToScript(obj, flags);
            if (result is null)
            {
                SetUndefined();
            }
            else if (result is DBNull)
            {
                SetNullInternal();
            }
            else if (result is bool boolValue)
            {
                Set(boolValue);
            }
            else if (result is char charValue)
            {
                Set(charValue);
            }
            else if (result is sbyte sbyteValue)
            {
                Set(sbyteValue);
            }
            else if (result is byte byteValue)
            {
                Set(byteValue);
            }
            else if (result is short shortValue)
            {
                Set(shortValue);
            }
            else if (result is ushort ushortValue)
            {
                Set(ushortValue);
            }
            else if (result is int intValue)
            {
                Set(intValue);
            }
            else if (result is uint uintValue)
            {
                Set(uintValue);
            }
            else if (result is long longValue)
            {
                Set(longValue);
            }
            else if (result is ulong ulongValue)
            {
                Set(ulongValue);
            }
            else if (result is float floatValue)
            {
                Set(floatValue);
            }
            else if (result is double doubleValue)
            {
                Set(doubleValue);
            }
            else if (result is decimal decimalValue)
            {
                Set(decimalValue);
            }
            else if (result is string stringValue)
            {
                Set(stringValue);
            }
            else if (result is DateTime dateTimeValue)
            {
                Set(dateTimeValue);
            }
            else if (result is BigInteger bigIntegerValue)
            {
                Set(bigIntegerValue);
            }
            else if (result is V8ObjectImpl v8Object)
            {
                SetV8Object(v8Object);
            }
            else if (result is IHostItem hostObject)
            {
                SetHostObject(hostObject);
            }
            else
            {
                SetUndefined();
            }
        }

        private void SetV8Object(V8ObjectImpl v8Object)
        {
            VerifyUnset();
            pResult.AsRef().SetV8Object(v8Object);
        }

        private void SetHostObject(IHostItem hostObject)
        {
            VerifyUnset();
            pResult.AsRef().SetHostObject(hostObject);
        }

        private void VerifyUnset()
        {
            if (IsSet)
            {
                throw new InvalidOperationException("The result has already been set");
            }
        }
    }
}
