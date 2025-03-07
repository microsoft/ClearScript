// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8ScriptItem : ScriptItem, IJavaScriptObject
    {
        private readonly V8ScriptEngine engine;
        private readonly IV8Object target;
        private V8ScriptItem holder;
        private readonly InterlockedOneWayFlag disposedFlag = new();

        private V8ScriptItem(V8ScriptEngine engine, IV8Object target)
        {
            this.engine = engine;
            this.target = target;
        }

        public static object Wrap(V8ScriptEngine engine, object obj)
        {
            Debug.Assert(obj is not IScriptMarshalWrapper);

            if (obj is null)
            {
                return null;
            }

            if (obj is IV8Object target)
            {
                if (target.IsArray)
                {
                    return new V8Array(engine, target);
                }

                if (!target.IsArrayBufferOrView)
                {
                    return new V8ScriptObject(engine, target);
                }

                switch (target.ArrayBufferOrViewKind)
                {
                    case V8ArrayBufferOrViewKind.ArrayBuffer:
                        return new V8ArrayBuffer(engine, target);

                    case V8ArrayBufferOrViewKind.DataView:
                        return new V8DataView(engine, target);

                    case V8ArrayBufferOrViewKind.Uint8Array:
                    case V8ArrayBufferOrViewKind.Uint8ClampedArray:
                        return new V8TypedArray<byte>(engine, target);

                    case V8ArrayBufferOrViewKind.Int8Array:
                        return new V8TypedArray<sbyte>(engine, target);

                    case V8ArrayBufferOrViewKind.Uint16Array:
                        return new V8UInt16Array(engine, target);

                    case V8ArrayBufferOrViewKind.Int16Array:
                        return new V8TypedArray<short>(engine, target);

                    case V8ArrayBufferOrViewKind.Uint32Array:
                        return new V8TypedArray<uint>(engine, target);

                    case V8ArrayBufferOrViewKind.Int32Array:
                        return new V8TypedArray<int>(engine, target);

                    case V8ArrayBufferOrViewKind.BigUint64Array:
                        return new V8TypedArray<ulong>(engine, target);

                    case V8ArrayBufferOrViewKind.BigInt64Array:
                        return new V8TypedArray<long>(engine, target);

                    case V8ArrayBufferOrViewKind.Float32Array:
                        return new V8TypedArray<float>(engine, target);

                    case V8ArrayBufferOrViewKind.Float64Array:
                        return new V8TypedArray<double>(engine, target);

                    default:
                        return new V8ScriptObject(engine, target);
                }
            }

            return obj;
        }

        public bool IsPromise => target.IsPromise;

        public bool IsShared => target.IsShared;

        public object GetProperty(bool marshalValue, int index)
        {
            VerifyNotDisposed();
            var value = engine.ScriptInvoke(static ctx => ctx.target.GetProperty(ctx.index), (target, index));
            return marshalValue ? engine.MarshalToHost(value, false) : value;
        }

        public void SetProperty(bool marshalValue, int index, object value)
        {
            VerifyNotDisposed();
            engine.ScriptInvoke(static ctx => ctx.self.target.SetProperty(ctx.index, ctx.marshalValue ? ctx.self.engine.MarshalToScript(ctx.value) : ctx.value), (self: this, marshalValue, index, value));
        }

        public object InvokeMethod(bool marshalResult, string name, params object[] args)
        {
            VerifyNotDisposed();

            var result = engine.ScriptInvoke(static ctx => ctx.self.target.InvokeMethod(ctx.name, ctx.self.engine.MarshalToScript(ctx.args)), (self: this, name, args));
            if (marshalResult)
            {
                return engine.MarshalToHost(result, false);
            }

            return result;
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        #region ScriptItem overrides

        protected override bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            VerifyNotDisposed();

            try
            {
                if (binder is GetMemberBinder getMemberBinder)
                {
                    result = target.GetProperty(getMemberBinder.Name);
                    return true;
                }

                if ((binder is SetMemberBinder setMemberBinder) && (args is not null) && (args.Length > 0))
                {
                    target.SetProperty(setMemberBinder.Name, args[0]);
                    result = args[0];
                    return true;
                }

                if (binder is GetIndexBinder)
                {
                    if ((args is not null) && (args.Length == 1))
                    {
                        result = MiscHelpers.TryGetNumericIndex(args[0], out int index) ? target.GetProperty(index) : target.GetProperty(args[0].ToString());
                        return true;
                    }

                    throw new InvalidOperationException("Invalid argument or index count");
                }

                if (binder is SetIndexBinder)
                {
                    if ((args is not null) && (args.Length == 2))
                    {
                        if (MiscHelpers.TryGetNumericIndex(args[0], out int index))
                        {
                            target.SetProperty(index, args[1]);
                        }
                        else
                        {
                            target.SetProperty(args[0].ToString(), args[1]);
                        }

                        result = args[1];
                        return true;
                    }

                    throw new InvalidOperationException("Invalid argument or index count");
                }

                if (binder is InvokeBinder)
                {
                    result = target.Invoke(false, args);
                    return true;
                }

                if (binder is InvokeMemberBinder invokeMemberBinder)
                {
                    result = target.InvokeMethod(invokeMemberBinder.Name, args);
                    return true;
                }
            }
            catch (Exception exception)
            {
                if (engine.CurrentScriptFrame is not null)
                {
                    if (exception is IScriptEngineException scriptError)
                    {
                        if (scriptError.ExecutionStarted)
                        {
                            throw;
                        }

                        engine.CurrentScriptFrame.ScriptError = scriptError;
                    }
                    else
                    {
                        engine.CurrentScriptFrame.ScriptError = new ScriptEngineException(engine.Name, exception.Message, null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception);
                    }
                }
            }

            result = null;
            return false;
        }

        public override string[] GetPropertyNames()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static target => target.GetPropertyNames(false), target);
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static target => target.GetPropertyIndices(), target);
        }

        #endregion

        #region ScriptObject overrides

        public override object GetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();
            if ((args is not null) && (args.Length != 0))
            {
                throw new InvalidOperationException("Invalid argument or index count");
            }

            var result = engine.MarshalToHost(engine.ScriptInvoke(static ctx => ctx.target.GetProperty(ctx.name), (target, name)), false);

            if ((result is V8ScriptItem resultScriptItem) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();
            if ((args is null) || (args.Length != 1))
            {
                throw new InvalidOperationException("Invalid argument or index count");
            }

            engine.ScriptInvoke(static ctx => ctx.self.target.SetProperty(ctx.name, ctx.self.engine.MarshalToScript(ctx.value)), (self: this, name, value: args[0]));
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static ctx => ctx.target.DeleteProperty(ctx.name), (target, name));
        }

        public override object GetProperty(int index)
        {
            return GetProperty(true, index);
        }

        public override void SetProperty(int index, object value)
        {
            SetProperty(true, index, value);
        }

        public override bool DeleteProperty(int index)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static ctx => ctx.target.DeleteProperty(ctx.index), (target, index));
        }

        public override object Invoke(bool asConstructor, params object[] args)
        {
            VerifyNotDisposed();

            if (asConstructor || (holder is null))
            {
                return engine.MarshalToHost(engine.ScriptInvoke(static ctx => ctx.self.target.Invoke(ctx.asConstructor, ctx.self.engine.MarshalToScript(ctx.args)), (self: this, asConstructor, args)), false);
            }

            var engineInternal = (ScriptObject)engine.Global.GetProperty("EngineInternal");
            return engineInternal.InvokeMethod("invokeMethod", holder, this, args);
        }

        public override object InvokeMethod(string name, params object[] args)
        {
            return InvokeMethod(true, name, args);
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public override ScriptEngine Engine => engine;

        public override object Unwrap()
        {
            return target;
        }

        #endregion

        #region Object overrides

        public override bool Equals(object obj) => (obj is V8ScriptItem that) && engine.Equals(this, that);

        public override int GetHashCode() => target.IdentityHash;

        #endregion

        #region IJavaScriptObject implementation

        public JavaScriptObjectKind Kind => target.ObjectKind;

        public JavaScriptObjectFlags Flags => target.ObjectFlags;

        #endregion

        #region IDisposable implementation

        public override void Dispose()
        {
            if (disposedFlag.Set())
            {
                target.Dispose();
            }
        }

        #endregion

        #region Nested type: V8ScriptObject

        private sealed class V8ScriptObject : V8ScriptItem, IDictionary<string, object>
        {
            public V8ScriptObject(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            private bool TryGetProperty(string name, out object value)
            {
                VerifyNotDisposed();

                var ctx = (target, name, value: (object)null);

                var found = engine.ScriptInvoke(
                    static pCtx =>
                    {
                        ref var ctx = ref pCtx.AsRef();
                        return ctx.target.TryGetProperty(ctx.name, out ctx.value);
                    },
                    StructPtr.FromRef(ref ctx)
                );

                if (found)
                {
                    var result = engine.MarshalToHost(ctx.value, false);
                    if ((result is V8ScriptItem resultScriptItem) && (resultScriptItem.engine == engine))
                    {
                        resultScriptItem.holder = this;
                    }

                    value = result;
                    return true;
                }

                value = null;
                return false;
            }

            #region IDictionary<string, object> implementation

            private IDictionary<string, object> ThisDictionary => this;

            private IEnumerable<string> PropertyKeys => GetPropertyKeys();

            private IEnumerable<KeyValuePair<string, object>> KeyValuePairs => PropertyKeys.Select(name => new KeyValuePair<string, object>(name, GetProperty(name)));

            private string[] GetPropertyKeys()
            {
                VerifyNotDisposed();
                return engine.ScriptInvoke(static target => target.GetPropertyNames(true), target);
            }

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                // ReSharper disable once NotDisposedResourceIsReturned
                return KeyValuePairs.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                // ReSharper disable once NotDisposedResourceIsReturned
                return ThisDictionary.GetEnumerator();
            }

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                SetProperty(item.Key, item.Value);
            }

            void ICollection<KeyValuePair<string, object>>.Clear()
            {
                PropertyKeys.ForEach(name => DeleteProperty(name));
            }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                return TryGetProperty(item.Key, out var value) && Equals(value, item.Value);
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                var source = KeyValuePairs.ToArray();
                Array.Copy(source, 0, array, arrayIndex, source.Length);
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                return ThisDictionary.Contains(item) && DeleteProperty(item.Key);
            }

            int ICollection<KeyValuePair<string, object>>.Count => PropertyKeys.Count();

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

            void IDictionary<string, object>.Add(string key, object value)
            {
                SetProperty(key, value);
            }

            bool IDictionary<string, object>.ContainsKey(string key)
            {
                return PropertyKeys.Contains(key);
            }

            bool IDictionary<string, object>.Remove(string key)
            {
                return DeleteProperty(key);
            }

            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                return TryGetProperty(key, out value);
            }

            object IDictionary<string, object>.this[string key]
            {
                get => TryGetProperty(key, out var value) ? value : throw new KeyNotFoundException();
                set => SetProperty(key, value);
            }

            ICollection<string> IDictionary<string, object>.Keys => PropertyKeys.ToList();

            ICollection<object> IDictionary<string, object>.Values => PropertyKeys.Select(name => GetProperty(name)).ToList();

            #endregion
        }

        #endregion

        #region Nested type: V8Array

        private sealed class V8Array : V8ScriptItem, IList<object>, IList
        {
            public V8Array(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region IList<T> implementation

            private IList<object> ThisGenericList => this;

            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                return new Enumerator(this);
            }

            bool ICollection<object>.Remove(object item)
            {
                var index = ThisList.IndexOf(item);
                if (index >= 0)
                {
                    ThisList.RemoveAt(index);
                    return true;
                }

                return false;
            }

            int ICollection<object>.Count => ThisList.Count;

            bool ICollection<object>.IsReadOnly => ThisList.IsReadOnly;

            void ICollection<object>.Clear()
            {
                ThisList.Clear();
            }

            bool ICollection<object>.Contains(object item)
            {
                return ThisList.Contains(item);
            }

            void ICollection<object>.CopyTo(object[] array, int arrayIndex)
            {
                ThisList.CopyTo(array, arrayIndex);
            }

            void ICollection<object>.Add(object item)
            {
                ThisList.Add(item);
            }

            void IList<object>.Insert(int index, object item)
            {
                ThisList.Insert(index, item);
            }

            void IList<object>.RemoveAt(int index)
            {
                ThisList.RemoveAt(index);
            }

            int IList<object>.IndexOf(object item)
            {
                return ThisList.IndexOf(item);
            }

            #endregion

            #region IList implementation

            private IList ThisList => this;

            IEnumerator IEnumerable.GetEnumerator()
            {
                // ReSharper disable once NotDisposedResourceIsReturned
                return ThisGenericList.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                MiscHelpers.VerifyNonNullArgument(array, nameof(array));

                if (array.Rank > 1)
                {
                    throw new ArgumentException("Invalid target array", nameof(array));
                }

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var length = ThisList.Count;
                if ((index + length) > array.Length)
                {
                    throw new ArgumentException("Insufficient space in target array", nameof(array));
                }

                for (var sourceIndex = 0; sourceIndex < length; sourceIndex++)
                {
                    array.SetValue(this[sourceIndex], index + sourceIndex);
                }
            }

            int ICollection.Count => Convert.ToInt32(GetProperty("length"));

            object ICollection.SyncRoot => this;

            bool ICollection.IsSynchronized => false;

            int IList.Add(object value)
            {
                return Convert.ToInt32(InvokeMethod("push", value)) - 1;
            }

            bool IList.Contains(object value)
            {
                return ThisList.IndexOf(value) >= 0;
            }

            void IList.Clear()
            {
                InvokeMethod("splice", 0, ThisList.Count);
            }

            int IList.IndexOf(object value)
            {
                return Convert.ToInt32(InvokeMethod("indexOf", value));
            }

            void IList.Insert(int index, object value)
            {
                InvokeMethod("splice", index, 0, value);
            }

            void IList.Remove(object value)
            {
                ThisGenericList.Remove(value);
            }

            void IList.RemoveAt(int index)
            {
                InvokeMethod("splice", index, 1);
            }

            bool IList.IsReadOnly => false;

            bool IList.IsFixedSize => false;

            #region Nested type: Enumerator

            private class Enumerator : IEnumerator<object>
            {
                private readonly V8Array array;
                private readonly int count;
                private int index = -1;

                public Enumerator(V8Array array)
                {
                    this.array = array;
                    count = array.ThisList.Count;
                }

                public bool MoveNext()
                {
                    if (index >= (count - 1))
                    {
                        return false;
                    }

                    ++index;
                    return true;
                }

                public void Reset()
                {
                    index = -1;
                }

                public object Current => array[index];

                public void Dispose()
                {
                }
            }

            #endregion

            #endregion
        }

        #endregion

        #region Nested type: V8ArrayBufferOrView

        private abstract class V8ArrayBufferOrView : V8ScriptItem
        {
            private V8ArrayBufferOrViewInfo info;
            private IArrayBuffer arrayBuffer;

            protected V8ArrayBufferOrView(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            protected IArrayBuffer ArrayBuffer => GetArrayBuffer();

            protected ulong Offset => GetInfo().Offset;

            protected ulong Size => GetInfo().Size;

            protected ulong Length => GetInfo().Length;

            protected byte[] GetBytes()
            {
                var size = Size;
                return InvokeWithDirectAccess(
                    static (pData, size) =>
                    {
                        var result = new byte[size];
                        UnmanagedMemoryHelpers.Copy(pData, size, result, 0);
                        return result;
                    },
                    size
                );
            }

            protected ulong ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex)
            {
                var size = Size;
                if (size < 1)
                {
                    if (offset > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    }

                    if (count > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
                else if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(GetPtrWithOffset(pData, ctx.offset), Math.Min(ctx.count, ctx.size - ctx.offset), ctx.destination, ctx.destinationIndex),
                    (offset, count, destination, destinationIndex, size)
                );
            }

            protected unsafe ulong ReadBytes(ulong offset, ulong count, Span<byte> destination, ulong destinationIndex)
            {
                var size = Size;
                if (size < 1)
                {
                    if (offset > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    }

                    if (count > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
                else if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (void* pDestination = destination)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(GetPtrWithOffset(pData, ctx.offset), Math.Min(ctx.count, ctx.size - ctx.offset), new Span<byte>(ctx.pDestination.ToPointer(), ctx.destinationLength), ctx.destinationIndex),
                        (offset, count, pDestination: (IntPtr)pDestination, destinationLength: destination.Length, destinationIndex, size)
                    );
                }
            }

            protected ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                var size = Size;
                if (size < 1)
                {
                    if (offset > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    }

                    if (count > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
                else if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.source, ctx.sourceIndex, Math.Min(ctx.count, ctx.size - ctx.offset), GetPtrWithOffset(pData, ctx.offset)),
                    (source, sourceIndex, count, offset, size)
                );
            }

            protected unsafe ulong WriteBytes(in ReadOnlySpan<byte> source, ulong sourceIndex, ulong count, ulong offset)
            {
                var size = Size;
                if (size < 1)
                {
                    if (offset > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset));
                    }

                    if (count > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
                else if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                fixed (void* pSource = source)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(new ReadOnlySpan<byte>(ctx.pSource.ToPointer(), ctx.sourceLength), ctx.sourceIndex, Math.Min(ctx.count, ctx.size - ctx.offset), GetPtrWithOffset(pData, ctx.offset)),
                        (pSource: (IntPtr)pSource, sourceLength: source.Length, sourceIndex, count, offset, size)
                    );
                }
            }

            protected void InvokeWithDirectAccess(Action<IntPtr> action)
            {
                engine.ScriptInvoke(static ctx => ctx.target.InvokeWithArrayBufferOrViewData(ctx.action), (target, action));
            }

            protected TResult InvokeWithDirectAccess<TResult>(Func<IntPtr, TResult> func)
            {
                var ctx = (self: this, func, result: default(TResult));

                return engine.ScriptInvoke(
                    static pCtx =>
                    {
                        ref var ctx = ref pCtx.AsRef();

                        ctx.self.target.InvokeWithArrayBufferOrViewData(
                            static (pData, pCtx) =>
                            {
                                ref var ctx = ref pCtx.AsRef();
                                ctx.result = ctx.func(pData);
                            },
                            pCtx
                        );

                        return ctx.result;
                    },
                    StructPtr.FromRef(ref ctx)
                );
            }

            protected void InvokeWithDirectAccess<TArg>(Action<IntPtr, TArg> action, in TArg arg)
            {
                engine.ScriptInvoke(static ctx => ctx.target.InvokeWithArrayBufferOrViewData(ctx.action, ctx.arg), (target, action, arg));
            }

            protected TResult InvokeWithDirectAccess<TArg, TResult>(Func<IntPtr, TArg, TResult> func, in TArg arg)
            {
                var ctx = (self: this, func, arg, result: default(TResult));

                return engine.ScriptInvoke(
                    static pCtx =>
                    {
                        ref var ctx = ref pCtx.AsRef();

                        ctx.self.target.InvokeWithArrayBufferOrViewData(
                            static (pData, pCtx) =>
                            {
                                ref var ctx = ref pCtx.AsRef();
                                ctx.result = ctx.func(pData, ctx.arg);
                            },
                            pCtx
                        );

                        return ctx.result;
                    },
                    StructPtr.FromRef(ref ctx)
                );
            }

            private V8ArrayBufferOrViewInfo GetInfo()
            {
                VerifyNotDisposed();

                if (info is null)
                {
                    engine.ScriptInvoke(
                        static ctx =>
                        {
                            if (ctx.self.info is null)
                            {
                                ctx.self.info = ctx.target.GetArrayBufferOrViewInfo();
                            }
                        },
                        (self: this, target)
                    );
                }

                return info;
            }

            private IArrayBuffer GetArrayBuffer()
            {
                return arrayBuffer ?? (arrayBuffer = (IArrayBuffer)engine.MarshalToHost(GetInfo().ArrayBuffer, false));
            }

            private static IntPtr GetPtrWithOffset(IntPtr pData, ulong offset)
            {
                if (offset < 1)
                {
                    return pData;
                }

                var baseAddr = unchecked((ulong)pData.ToInt64());
                return (IntPtr)unchecked((long)checked(baseAddr + offset));
            }
        }

        #endregion

        #region Nested type: V8ArrayBuffer

        private sealed class V8ArrayBuffer : V8ArrayBufferOrView, IArrayBuffer
        {
            public V8ArrayBuffer(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region IArrayBuffer implementation

            ulong IArrayBuffer.Size => Size;

            byte[] IArrayBuffer.GetBytes()
            {
                return GetBytes();
            }

            ulong IArrayBuffer.ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex)
            {
                return ReadBytes(offset, count, destination, destinationIndex);
            }

            ulong IArrayBuffer.ReadBytes(ulong offset, ulong count, Span<byte> destination, ulong destinationIndex)
            {
                return ReadBytes(offset, count, destination, destinationIndex);
            }

            ulong IArrayBuffer.WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
            }

            ulong IArrayBuffer.WriteBytes(ReadOnlySpan<byte> source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
            }

            void IArrayBuffer.InvokeWithDirectAccess(Action<IntPtr> action)
            {
                MiscHelpers.VerifyNonNullArgument(action, nameof(action));
                InvokeWithDirectAccess(action);
            }

            TResult IArrayBuffer.InvokeWithDirectAccess<TResult>(Func<IntPtr, TResult> func)
            {
                MiscHelpers.VerifyNonNullArgument(func, nameof(func));
                return InvokeWithDirectAccess(func);
            }

            void IArrayBuffer.InvokeWithDirectAccess<TArg>(Action<IntPtr, TArg> action, in TArg arg)
            {
                MiscHelpers.VerifyNonNullArgument(action, nameof(action));
                InvokeWithDirectAccess(action, arg);
            }

            TResult IArrayBuffer.InvokeWithDirectAccess<TArg, TResult>(Func<IntPtr, TArg, TResult> func, in TArg arg)
            {
                MiscHelpers.VerifyNonNullArgument(func, nameof(func));
                return InvokeWithDirectAccess(func, arg);
            }

            #endregion
        }

        #endregion

        #region Nested type: V8ArrayBufferView

        private abstract class V8ArrayBufferView : V8ArrayBufferOrView, IArrayBufferView
        {
            protected V8ArrayBufferView(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region IArrayBufferView implementation

            IArrayBuffer IArrayBufferView.ArrayBuffer => ArrayBuffer;

            ulong IArrayBufferView.Offset => Offset;

            ulong IArrayBufferView.Size => Size;

            byte[] IArrayBufferView.GetBytes()
            {
                return GetBytes();
            }

            ulong IArrayBufferView.ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex)
            {
                return ReadBytes(offset, count, destination, destinationIndex);
            }

            ulong IArrayBufferView.ReadBytes(ulong offset, ulong count, Span<byte> destination, ulong destinationIndex)
            {
                return ReadBytes(offset, count, destination, destinationIndex);
            }

            ulong IArrayBufferView.WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
            }

            ulong IArrayBufferView.WriteBytes(ReadOnlySpan<byte> source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
            }

            void IArrayBufferView.InvokeWithDirectAccess(Action<IntPtr> action)
            {
                MiscHelpers.VerifyNonNullArgument(action, nameof(action));
                InvokeWithDirectAccess(action);
            }

            TResult IArrayBufferView.InvokeWithDirectAccess<TResult>(Func<IntPtr, TResult> func)
            {
                MiscHelpers.VerifyNonNullArgument(func, nameof(func));
                return InvokeWithDirectAccess(func);
            }

            void IArrayBufferView.InvokeWithDirectAccess<TArg>(Action<IntPtr, TArg> action, in TArg arg)
            {
                MiscHelpers.VerifyNonNullArgument(action, nameof(action));
                InvokeWithDirectAccess(action, arg);
            }

            TResult IArrayBufferView.InvokeWithDirectAccess<TArg, TResult>(Func<IntPtr, TArg, TResult> func, in TArg arg)
            {
                MiscHelpers.VerifyNonNullArgument(func, nameof(func));
                return InvokeWithDirectAccess(func, arg);
            }

            #endregion
        }

        #endregion

        #region Nested type: V8DataView

        private sealed class V8DataView : V8ArrayBufferView, IDataView
        {
            public V8DataView(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }
        }

        #endregion

        #region Nested type: V8TypedArray

        private class V8TypedArray : V8ArrayBufferView, ITypedArray
        {
            protected V8TypedArray(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            protected IntPtr GetPtrWithIndex(IntPtr pData, ulong index)
            {
                if (index < 1)
                {
                    return pData;
                }

                var baseAddr = unchecked((ulong)pData.ToInt64());
                return (IntPtr)unchecked((long)checked(baseAddr + (index * (Size / Length))));
            }

            #region ITypedArray implementation

            ulong ITypedArray.Length => Length;

            #endregion
        }

        #endregion

        #region Nested type: V8TypedArray<T>

        private class V8TypedArray<T> : V8TypedArray, ITypedArray<T> where T : unmanaged
        {
            public V8TypedArray(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region ITypedArray<T> implementation

            T[] ITypedArray<T>.ToArray()
            {
                var length = Length;
                return InvokeWithDirectAccess(
                    static (pData, length) =>
                    {
                        var result = new T[length];
                        UnmanagedMemoryHelpers.Copy(pData, length, result, 0);
                        return result;
                    },
                    length
                );
            }

            ulong ITypedArray<T>.Read(ulong index, ulong length, T[] destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.self.GetPtrWithIndex(pData, ctx.index), Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.destination, ctx.destinationIndex),
                    (self: this, index, length, destination, destinationIndex, totalLength)
                );
            }

            unsafe ulong ITypedArray<T>.Read(ulong index, ulong length, Span<T> destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                fixed (void* pDestination = destination)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.self.GetPtrWithIndex(pData, ctx.index), Math.Min(ctx.length, ctx.totalLength - ctx.index), new Span<T>(ctx.pDestination.ToPointer(), ctx.destinationLength), ctx.destinationIndex),
                        (self: this, index, length, pDestination: (IntPtr)pDestination, destinationLength: destination.Length, destinationIndex, totalLength)
                    );
                }
            }

            ulong ITypedArray<T>.Write(T[] source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.source, ctx.sourceIndex, Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.self.GetPtrWithIndex(pData, ctx.index)),
                    (self: this, source, sourceIndex, length, index, totalLength)
                );
            }

            unsafe ulong ITypedArray<T>.Write(ReadOnlySpan<T> source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                fixed (void* pSource = source)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(new ReadOnlySpan<T>(ctx.pSource.ToPointer(), ctx.sourceLength), ctx.sourceIndex, Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.self.GetPtrWithIndex(pData, ctx.index)),
                        (self: this, pSource: (IntPtr)pSource, sourceLength: source.Length, sourceIndex, length, index, totalLength)
                    );
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: V8UInt16Array

        // special case to support both ITypedArray<ushort> and ITypedArray<char>

        private sealed class V8UInt16Array : V8TypedArray<ushort>, ITypedArray<char>
        {
            public V8UInt16Array(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region ITypedArray<char> implementation

            char[] ITypedArray<char>.ToArray()
            {
                var length = Length;
                return InvokeWithDirectAccess(
                    static (pData, length) =>
                    {
                        var result = new char[length];
                        UnmanagedMemoryHelpers.Copy(pData, length, result, 0);
                        return result;
                    },
                    length
                );
            }

            ulong ITypedArray<char>.Read(ulong index, ulong length, char[] destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.self.GetPtrWithIndex(pData, ctx.index), Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.destination, ctx.destinationIndex),
                    (self: this, index, length, destination, destinationIndex, totalLength)
                );
            }

            unsafe ulong ITypedArray<char>.Read(ulong index, ulong length, Span<char> destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                fixed (void* pDestination = destination)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.self.GetPtrWithIndex(pData, ctx.index), Math.Min(ctx.length, ctx.totalLength - ctx.index), new Span<char>(ctx.pDestination.ToPointer(), ctx.destinationLength), ctx.destinationIndex),
                        (self: this, index, length, pDestination: (IntPtr)pDestination, destinationLength: destination.Length, destinationIndex, totalLength)
                    );
                }
            }

            ulong ITypedArray<char>.Write(char[] source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return InvokeWithDirectAccess(
                    static (pData, ctx) => UnmanagedMemoryHelpers.Copy(ctx.source, ctx.sourceIndex, Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.self.GetPtrWithIndex(pData, ctx.index)),
                    (self: this, source, sourceIndex, length, index, totalLength)
                );
            }

            unsafe ulong ITypedArray<char>.Write(ReadOnlySpan<char> source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (totalLength < 1)
                {
                    if (index > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    if (length > 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(length));
                    }
                }
                else if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                fixed (void* pSource = source)
                {
                    return InvokeWithDirectAccess(
                        static (pData, ctx) => UnmanagedMemoryHelpers.Copy(new ReadOnlySpan<char>(ctx.pSource.ToPointer(), ctx.sourceLength), ctx.sourceIndex, Math.Min(ctx.length, ctx.totalLength - ctx.index), ctx.self.GetPtrWithIndex(pData, ctx.index)),
                        (self: this, pSource: (IntPtr)pSource, sourceLength: source.Length, sourceIndex, length, index, totalLength)
                    );
                }
            }

            #endregion
        }

        #endregion
    }
}
