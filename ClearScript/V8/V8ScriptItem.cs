// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.V8
{
    internal class V8ScriptItem : ScriptItem, IDisposable
    {
        private readonly V8ScriptEngine engine;
        private readonly IV8Object target;
        private V8ScriptItem holder;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        private V8ScriptItem(V8ScriptEngine engine, IV8Object target)
        {
            this.engine = engine;
            this.target = target;
        }

        public static object Wrap(V8ScriptEngine engine, object obj)
        {
            Debug.Assert(!(obj is IScriptMarshalWrapper));

            if (obj == null)
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
                    return new V8ScriptItem(engine, target);
                }

                switch (target.GetArrayBufferOrViewKind())
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
                        return new V8ScriptItem(engine, target);
                }
            }

            return obj;
        }

        public bool IsPromise => target.IsPromise;

        public bool IsShared => target.IsShared;

        public object InvokeMethod(bool marshalResult, string name, params object[] args)
        {
            VerifyNotDisposed();

            var result = engine.ScriptInvoke(() => target.InvokeMethod(name, engine.MarshalToScript(args)));
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

                if ((binder is SetMemberBinder setMemberBinder) && (args != null) && (args.Length > 0))
                {
                    target.SetProperty(setMemberBinder.Name, args[0]);
                    result = args[0];
                    return true;
                }

                if (binder is GetIndexBinder)
                {
                    if ((args != null) && (args.Length == 1))
                    {
                        result = MiscHelpers.TryGetNumericIndex(args[0], out int index) ? target.GetProperty(index) : target.GetProperty(args[0].ToString());
                        return true;
                    }

                    throw new InvalidOperationException("Invalid argument or index count");
                }

                if (binder is SetIndexBinder)
                {
                    if ((args != null) && (args.Length == 2))
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
                if (engine.CurrentScriptFrame != null)
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
            return engine.ScriptInvoke(() => target.GetPropertyNames());
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetPropertyIndices());
        }

        #endregion

        #region ScriptObject overrides

        public override object GetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();
            if ((args != null) && (args.Length != 0))
            {
                throw new InvalidOperationException("Invalid argument or index count");
            }

            var result = engine.MarshalToHost(engine.ScriptInvoke(() => target.GetProperty(name)), false);

            if ((result is V8ScriptItem resultScriptItem) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();
            if ((args == null) || (args.Length != 1))
            {
                throw new InvalidOperationException("Invalid argument or index count");
            }

            engine.ScriptInvoke(() => target.SetProperty(name, engine.MarshalToScript(args[0])));
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.DeleteProperty(name));
        }

        public override object GetProperty(int index)
        {
            VerifyNotDisposed();
            return engine.MarshalToHost(engine.ScriptInvoke(() => target.GetProperty(index)), false);
        }

        public override void SetProperty(int index, object value)
        {
            VerifyNotDisposed();
            engine.ScriptInvoke(() => target.SetProperty(index, engine.MarshalToScript(value)));
        }

        public override bool DeleteProperty(int index)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.DeleteProperty(index));
        }

        public override object Invoke(bool asConstructor, params object[] args)
        {
            VerifyNotDisposed();

            if (asConstructor || (holder == null))
            {
                return engine.MarshalToHost(engine.ScriptInvoke(() => target.Invoke(asConstructor, engine.MarshalToScript(args))), false);
            }

            return engine.Script.EngineInternal.invokeMethod(holder, this, args);
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

        #region IDisposable implementation

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                target.Dispose();
            }
        }

        #endregion

        #region Nested type: V8Array

        private sealed class V8Array : V8ScriptItem, IList
        {
            public V8Array(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region IList implementation

            public IEnumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            public void CopyTo(Array array, int index)
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

                var length = Count;
                if ((index + length) > array.Length)
                {
                    throw new ArgumentException("Insufficient space in target array", nameof(array));
                }

                for (var sourceIndex = 0; sourceIndex < length; sourceIndex++)
                {
                    array.SetValue(this[sourceIndex], index + sourceIndex);
                }
            }

            public int Count => Convert.ToInt32(GetProperty("length"));

            public object SyncRoot => this;

            public bool IsSynchronized => false;

            public int Add(object value)
            {
                return Convert.ToInt32(InvokeMethod("push", value)) - 1;
            }

            public bool Contains(object value)
            {
                return IndexOf(value) >= 0;
            }

            public void Clear()
            {
                InvokeMethod("splice", 0, Count);
            }

            public int IndexOf(object value)
            {
                return Convert.ToInt32(InvokeMethod("indexOf", value));
            }

            public void Insert(int index, object value)
            {
                InvokeMethod("splice", index, 0, value);
            }

            public void Remove(object value)
            {
                var index = IndexOf(value);
                if (index >= 0)
                {
                    RemoveAt(index);
                }
            }

            public void RemoveAt(int index)
            {
                InvokeMethod("splice", index, 1);
            }

            public bool IsReadOnly => false;

            public bool IsFixedSize => false;

            #region Nested type: Enumerator

            private class Enumerator : IEnumerator
            {
                private readonly V8Array array;
                private readonly int count;
                private int index = -1;

                public Enumerator(V8Array array)
                {
                    this.array = array;
                    count = array.Count;
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
            }

            #endregion

            #endregion
        }

        #endregion

        #region Nested type: V8ArrayBufferOrView

        private class V8ArrayBufferOrView : V8ScriptItem
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
                return engine.ScriptInvoke(() =>
                {
                    var result = new byte[Size];
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        UnmanagedMemoryHelpers.Copy(pData, Size, result, 0);
                    });

                    return result;
                });
            }

            protected ulong ReadBytes(ulong offset, ulong count, byte[] destination, ulong destinationIndex)
            {
                var size = Size;
                if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                count = Math.Min(count, size - offset);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        count = UnmanagedMemoryHelpers.Copy(GetPtrWithOffset(pData, offset), count, destination, destinationIndex);
                    });

                    return count;
                });
            }

            protected ulong WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                var size = Size;
                if (offset >= size)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }

                count = Math.Min(count, size - offset);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        count = UnmanagedMemoryHelpers.Copy(source, sourceIndex, count, GetPtrWithOffset(pData, offset));
                    });

                    return count;
                });
            }

            private V8ArrayBufferOrViewInfo GetInfo()
            {
                VerifyNotDisposed();

                if (info == null)
                {
                    engine.ScriptInvoke(() =>
                    {
                        if (info == null)
                        {
                            info = target.GetArrayBufferOrViewInfo();
                        }
                    });
                }

                return info;
            }

            private IArrayBuffer GetArrayBuffer()
            {
                return arrayBuffer ?? (arrayBuffer = (IArrayBuffer)engine.MarshalToHost(GetInfo().ArrayBuffer, false));
            }

            private static IntPtr GetPtrWithOffset(IntPtr pData, ulong offset)
            {
                var baseAddr = unchecked((ulong)pData.ToInt64());
                return new IntPtr(unchecked((long)checked(baseAddr + offset)));
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

            ulong IArrayBuffer.WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
            }

            #endregion
        }

        #endregion

        #region Nested type: V8ArrayBufferView

        private class V8ArrayBufferView : V8ArrayBufferOrView, IArrayBufferView
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

            ulong IArrayBufferView.WriteBytes(byte[] source, ulong sourceIndex, ulong count, ulong offset)
            {
                return WriteBytes(source, sourceIndex, count, offset);
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
                var baseAddr = unchecked((ulong)pData.ToInt64());
                return new IntPtr(unchecked((long)checked(baseAddr + (index * (Size / Length)))));
            }

            #region ITypedArray implementation

            ulong ITypedArray.Length => Length;

            #endregion
        }

        #endregion

        #region Nested type: V8TypedArray<T>

        private class V8TypedArray<T> : V8TypedArray, ITypedArray<T>
        {
            public V8TypedArray(V8ScriptEngine engine, IV8Object target)
                : base(engine, target)
            {
            }

            #region ITypedArray<T> implementation

            T[] ITypedArray<T>.ToArray()
            {
                return engine.ScriptInvoke(() =>
                {
                    var result = new T[Length];
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        UnmanagedMemoryHelpers.Copy(pData, Length, result, 0);
                    });

                    return result;
                });
            }

            ulong ITypedArray<T>.Read(ulong index, ulong length, T[] destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                length = Math.Min(length, totalLength - index);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        length = UnmanagedMemoryHelpers.Copy(GetPtrWithIndex(pData, index), length, destination, destinationIndex);
                    });

                    return length;
                });
            }

            ulong ITypedArray<T>.Write(T[] source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                length = Math.Min(length, totalLength - index);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        length = UnmanagedMemoryHelpers.Copy(source, sourceIndex, length, GetPtrWithIndex(pData, index));
                    });

                    return length;
                });
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
                return engine.ScriptInvoke(() =>
                {
                    var result = new char[Length];
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        UnmanagedMemoryHelpers.Copy(pData, Length, result, 0);
                    });

                    return result;
                });
            }

            ulong ITypedArray<char>.Read(ulong index, ulong length, char[] destination, ulong destinationIndex)
            {
                var totalLength = Length;
                if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                length = Math.Min(length, totalLength - index);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        length = UnmanagedMemoryHelpers.Copy(GetPtrWithIndex(pData, index), length, destination, destinationIndex);
                    });

                    return length;
                });
            }

            ulong ITypedArray<char>.Write(char[] source, ulong sourceIndex, ulong length, ulong index)
            {
                var totalLength = Length;
                if (index >= totalLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                length = Math.Min(length, totalLength - index);
                return engine.ScriptInvoke(() =>
                {
                    target.InvokeWithArrayBufferOrViewData(pData =>
                    {
                        length = UnmanagedMemoryHelpers.Copy(source, sourceIndex, length, GetPtrWithIndex(pData, index));
                    });

                    return length;
                });
            }

            #endregion
        }

        #endregion
    }
}
