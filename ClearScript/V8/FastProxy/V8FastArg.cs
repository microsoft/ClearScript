// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Numerics;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents an argument passed to a fast host object from script code.
    /// </summary>
    public readonly ref struct V8FastArg
    {
        private readonly V8Value.FastArg.Ptr pArg;
        private readonly V8FastArgKind kind;
        private readonly object obj;

        internal V8FastArg(V8ScriptEngine engine, V8Value.FastArg.Ptr pArg, V8FastArgKind kind)
        {
            this.pArg = pArg;
            this.kind = kind;
            obj = V8FastArgImpl.InitializeObject(engine, pArg.AsRef());
        }

        /// <summary>
        /// Determines whether the argument is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Falsy">falsy</see>.
        /// </summary>
        public bool IsFalsy => V8FastArgImpl.IsFalsy(pArg.AsRef(), obj);

        /// <summary>
        /// Determines whether the argument is <see href="https://developer.mozilla.org/en-US/docs/Glossary/Truthy">truthy</see>.
        /// </summary>
        public bool IsTruthy => V8FastArgImpl.IsTruthy(pArg.AsRef(), obj);

        /// <summary>
        /// Determines whether the argument is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>.
        /// </summary>
        public bool IsUndefined => V8FastArgImpl.IsUndefined(pArg.AsRef(), obj);

        /// <summary>
        /// Determines whether the argument is <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/null">null</see></c>.
        /// </summary>
        public bool IsNull => V8FastArgImpl.IsNull(pArg.AsRef(), obj);

        /// <summary>
        /// Gets the <c><see cref="bool"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="bool"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out bool value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="char"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="char"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out char value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="sbyte"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="sbyte"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out sbyte value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="byte"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="byte"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out byte value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="short"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="short"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out short value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="ushort"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="ushort"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out ushort value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="int"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="int"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out int value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="uint"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="uint"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out uint value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="long"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="long"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out long value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="ulong"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="ulong"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out ulong value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="float"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="float"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out float value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="double"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="double"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out double value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="decimal"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="decimal"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out decimal value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the string value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the string value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out string value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the string value of the argument as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c> if possible.
        /// </summary>
        /// <param name="value">On return, the string value of the argument as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out ReadOnlySpan<char> value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="DateTime"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="DateTime"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out DateTime value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="BigInteger"/></c> value of the argument if possible.
        /// </summary>
        /// <param name="value">On return, the <c><see cref="BigInteger"/></c> value of the argument if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet(out BigInteger value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets a nullable value of the given underlying type from the argument if possible.
        /// </summary>
        /// <typeparam name="T">The underlying type of the nullable value to get.</typeparam>
        /// <param name="value">On return, a nullable value of underlying type <typeparamref name="T"/> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet<T>(out T? value) where T : struct => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets a value of the given type from the argument if possible.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="value">On return, a value of type <typeparamref name="T"/> if the operation succeeded.</param>
        /// <returns><c>True</c> if the operation succeeded, <c>false</c> otherwise.</returns>
        public bool TryGet<T>(out T value) => V8FastArgImpl.TryGet(pArg.AsRef(), obj, out value);

        /// <summary>
        /// Gets the <c><see cref="bool"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="bool"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public bool GetBoolean(string name = null) => V8FastArgImpl.GetBoolean(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="char"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="char"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public char GetChar(string name = null) => V8FastArgImpl.GetChar(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="sbyte"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="sbyte"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public sbyte GetSByte(string name = null) => V8FastArgImpl.GetSByte(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="byte"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="byte"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public byte GetByte(string name = null) => V8FastArgImpl.GetByte(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="short"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="short"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public short GetInt16(string name = null) => V8FastArgImpl.GetInt16(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="ushort"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="ushort"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ushort GetUInt16(string name = null) => V8FastArgImpl.GetUInt16(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="int"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="int"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public int GetInt32(string name = null) => V8FastArgImpl.GetInt32(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="uint"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="uint"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public uint GetUInt32(string name = null) => V8FastArgImpl.GetUInt32(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="long"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="long"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public long GetInt64(string name = null) => V8FastArgImpl.GetInt64(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="ulong"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="ulong"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ulong GetUInt64(string name = null) => V8FastArgImpl.GetUInt64(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="float"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="float"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public float GetSingle(string name = null) => V8FastArgImpl.GetSingle(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="double"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="double"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public double GetDouble(string name = null) => V8FastArgImpl.GetDouble(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="decimal"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="decimal"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public decimal GetDecimal(string name = null) => V8FastArgImpl.GetDecimal(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the string value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The string value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public string GetString(string name = null) => V8FastArgImpl.GetString(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the string value of the argument as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The string value of the argument as a <c><see href="https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1">ReadOnlySpan&#x3C;char&#x3E;</see></c>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public ReadOnlySpan<char> GetCharSpan(string name = null) => V8FastArgImpl.GetCharSpan(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="DateTime"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="DateTime"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public DateTime GetDateTime(string name = null) => V8FastArgImpl.GetDateTime(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets the <c><see cref="BigInteger"/></c> value of the argument.
        /// </summary>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>The <c><see cref="BigInteger"/></c> value of the argument.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public BigInteger GetBigInteger(string name = null) => V8FastArgImpl.GetBigInteger(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets a nullable value of the given underlying type from the argument.
        /// </summary>
        /// <typeparam name="T">The underlying type of the nullable value to get.</typeparam>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>A nullable value of underlying type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public T? GetNullable<T>(string name = null) where T : struct => V8FastArgImpl.GetNullable<T>(pArg.AsRef(), obj, kind, name);

        /// <summary>
        /// Gets a value of the given type from the argument.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="name">An optional argument or property name.</param>
        /// <returns>A value of type <typeparamref name="T"/>.</returns>
        /// <remarks>
        /// This method throws an exception if the operation fails.
        /// </remarks>
        public T Get<T>(string name = null) => V8FastArgImpl.Get<T>(pArg.AsRef(), obj, kind, name);
    }
}
