// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Numerics;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a list of arguments passed to a fast host object from script code.
    /// </summary>
    public readonly ref struct V8FastArgs
    {
        private readonly ReadOnlySpan<V8Value.FastArg> args;
        private readonly V8FastArgKind argKind;
        private readonly object[] objects;

        internal V8FastArgs(V8ScriptEngine engine, in ReadOnlySpan<V8Value.FastArg> args, V8FastArgKind argKind)
        {
            this.args = args;
            this.argKind = argKind;
            objects = null;

            for (var index = 0; index < args.Length; index++)
            {
                var tempObject = V8FastArgImpl.InitializeObject(engine, args[index]);
                if (tempObject is not Nonexistent)
                {
                    EnsureObjects(ref objects, args.Length);
                    objects[index] = tempObject;
                }
            }
        }

        /// <summary>
        /// Gets the number of arguments in the list.
        /// </summary>
        public int Count => args.Length;

        /// <summary>
        /// Determines whether the argument at the specified index is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Falsy">falsy</see>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns><c>True</c> if the argument at the specified index is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Falsy">falsy</see>, <c>false</c> otherwise.</returns>
        public bool IsFalsy(int index) => V8FastArgImpl.IsFalsy(args[index], GetObject(index));

        /// <summary>
        /// Determines whether the argument at the specified index is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Truthy">truthy</see>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns><c>True</c> if the argument at the specified index is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Truthy">truthy</see>, <c>false</c> otherwise.</returns>
        public bool IsTruthy(int index) => V8FastArgImpl.IsTruthy(args[index], GetObject(index));

        /// <summary>
        /// Determines whether the argument at the specified index is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns><c>True</c> if the argument at the specified index is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>, <c>false</c> otherwise.</returns>
        public bool IsUndefined(int index) => V8FastArgImpl.IsUndefined(args[index], GetObject(index));

        /// <summary>
        /// Determines whether the argument at the specified index is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/null">null</see></c>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <returns><c>True</c> if the argument at the specified index is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>, <c>false</c> otherwise.</returns>
        public bool IsNull(int index) => V8FastArgImpl.IsNull(args[index], GetObject(index));

        /// <summary>
        /// Gets the <c><see cref="bool"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="bool"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out bool value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="char"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="char"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out char value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="sbyte"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="sbyte"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out sbyte value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="byte"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="byte"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out byte value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="short"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="short"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out short value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="ushort"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="ushort"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out ushort value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="int"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="int"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out int value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="uint"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="uint"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out uint value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="long"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="long"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out long value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="ulong"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="ulong"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out ulong value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="float"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="float"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out float value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="double"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="double"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out double value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="decimal"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="decimal"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out decimal value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the string value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the string value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out string value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the string value of the argument at the specified index as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c> if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the string value of the argument at the specified index as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out ReadOnlySpan<char> value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="DateTime"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="DateTime"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out DateTime value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="BigInteger"/></c> value of the argument at the specified index if possible.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, the <c><see cref="BigInteger"/></c> value of the argument at the specified index if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(int index, out BigInteger value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets a nullable value of the given underlying type from the argument at the specified index if possible.
        /// </summary>
        /// <typeparam name="T">The underlying type of the nullable value to get.</typeparam>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, a nullable value of underlying type <typeparamref name="T"/> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet<T>(int index, out T? value) where T : struct => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets a value of the given type from the argument at the specified index if possible.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="index">The argument index.</param>
        /// <param name="value">On return, a value of type <typeparamref name="T"/> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet<T>(int index, out T value) => V8FastArgImpl.TryGet(args[index], GetObject(index), out value);

        /// <summary>
        /// Gets the <c><see cref="bool"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="bool"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public bool GetBoolean(int index, string name = null) => V8FastArgImpl.GetBoolean(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="char"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="char"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public char GetChar(int index, string name = null) => V8FastArgImpl.GetChar(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="sbyte"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="sbyte"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public sbyte GetSByte(int index, string name = null) => V8FastArgImpl.GetSByte(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="byte"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="byte"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public byte GetByte(int index, string name = null) => V8FastArgImpl.GetByte(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="short"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="short"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public short GetInt16(int index, string name = null) => V8FastArgImpl.GetInt16(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="ushort"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="ushort"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ushort GetUInt16(int index, string name = null) => V8FastArgImpl.GetUInt16(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="int"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="int"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public int GetInt32(int index, string name = null) => V8FastArgImpl.GetInt32(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="uint"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="uint"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public uint GetUInt32(int index, string name = null) => V8FastArgImpl.GetUInt32(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="long"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="long"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public long GetInt64(int index, string name = null) => V8FastArgImpl.GetInt64(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="ulong"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="ulong"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ulong GetUInt64(int index, string name = null) => V8FastArgImpl.GetUInt64(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="float"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="float"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public float GetSingle(int index, string name = null) => V8FastArgImpl.GetSingle(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="double"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="double"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public double GetDouble(int index, string name = null) => V8FastArgImpl.GetDouble(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="decimal"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="decimal"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public decimal GetDecimal(int index, string name = null) => V8FastArgImpl.GetDecimal(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the string value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The string value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public string GetString(int index, string name = null) => V8FastArgImpl.GetString(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the string value of the argument at the specified index as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The string value of the argument at the specified index as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ReadOnlySpan<char> GetCharSpan(int index, string name = null) => V8FastArgImpl.GetCharSpan(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="DateTime"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="DateTime"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public DateTime GetDateTime(int index, string name = null) => V8FastArgImpl.GetDateTime(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets the <c><see cref="BigInteger"/></c> value of the argument at the specified index.
        /// </summary>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="BigInteger"/></c> value of the argument at the specified index.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public BigInteger GetBigInteger(int index, string name = null) => V8FastArgImpl.GetBigInteger(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets a nullable value of the given underlying type from the argument at the specified index.
        /// </summary>
        /// <typeparam name="T">The underlying type of the nullable value to get.</typeparam>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>A nullable value of underlying type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public T? GetNullable<T>(int index, string name = null) where T : struct => V8FastArgImpl.GetNullable<T>(args[index], GetObject(index), argKind, name);

        /// <summary>
        /// Gets a value of the given type from the argument at the specified index.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="index">The argument index.</param>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>A value of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public T Get<T>(int index, string name = null) => V8FastArgImpl.Get<T>(args[index], GetObject(index), argKind, name);

        private static void EnsureObjects(ref object[] objects, int count)
        {
            if (objects is null)
            {
                objects = new object[count];
                for (var index = 0; index < count; index++)
                {
                    objects[index] = Nonexistent.Value;
                }
            }
        }

        private object GetObject(int index) => (objects is not null) ? objects[index] : Nonexistent.Value;
    }
}
