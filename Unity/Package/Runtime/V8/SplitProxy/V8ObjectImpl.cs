// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8ObjectImpl : IV8Object
    {
        private V8EntityHolder holder;

        public V8Object.Handle Handle => (V8Object.Handle)holder.Handle;

        public V8ObjectImpl(V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags, int identityHash)
        {
            holder = new V8EntityHolder("V8 object", () => hObject);
            Subtype = subtype;
            Flags = flags;
            IdentityHash = identityHash;
        }

        public V8Value.Subtype Subtype { get; }

        public V8Value.Flags Flags { get; }

        #region IV8Object implementation

        public JavaScriptObjectKind ObjectKind
        {
            get
            {
                switch (Subtype)
                {
                    case V8Value.Subtype.Function:
                        return JavaScriptObjectKind.Function;

                    case V8Value.Subtype.Iterator:
                        return JavaScriptObjectKind.Iterator;

                    case V8Value.Subtype.Promise:
                        return JavaScriptObjectKind.Promise;

                    case V8Value.Subtype.Array:
                        return JavaScriptObjectKind.Array;

                    case V8Value.Subtype.ArrayBuffer:
                        return JavaScriptObjectKind.ArrayBuffer;

                    case V8Value.Subtype.DataView:
                        return JavaScriptObjectKind.DataView;

                    case V8Value.Subtype.Uint8Array:
                    case V8Value.Subtype.Uint8ClampedArray:
                    case V8Value.Subtype.Int8Array:
                    case V8Value.Subtype.Uint16Array:
                    case V8Value.Subtype.Int16Array:
                    case V8Value.Subtype.Uint32Array:
                    case V8Value.Subtype.Int32Array:
                    case V8Value.Subtype.BigUint64Array:
                    case V8Value.Subtype.BigInt64Array:
                    case V8Value.Subtype.Float32Array:
                    case V8Value.Subtype.Float64Array:
                        return JavaScriptObjectKind.TypedArray;

                    default:
                        return JavaScriptObjectKind.Unknown;
                }
            }
        }

        public JavaScriptObjectFlags ObjectFlags
        {
            get
            {
                var flags = JavaScriptObjectFlags.None;

                if (Flags.HasFlag(V8Value.Flags.Shared))
                {
                    flags |= JavaScriptObjectFlags.Shared;
                }

                if (Flags.HasFlag(V8Value.Flags.Async))
                {
                    flags |= JavaScriptObjectFlags.Async;
                }

                if (Flags.HasFlag(V8Value.Flags.Generator))
                {
                    flags |= JavaScriptObjectFlags.Generator;
                }

                return flags;
            }
        }

        public int IdentityHash { get; }

        public object GetProperty(string name)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetNamedProperty(Handle, name));
        }

        public bool TryGetProperty(string name, out object value)
        {
            object tempValue = null;
            var result = V8SplitProxyNative.Invoke(instance => instance.V8Object_TryGetNamedProperty(Handle, name, out tempValue));
            value = tempValue;
            return result;
        }

        public void SetProperty(string name, object value)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Object_SetNamedProperty(Handle, name, value));
        }

        public bool DeleteProperty(string name)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_DeleteNamedProperty(Handle, name));
        }

        public string[] GetPropertyNames(bool includeIndices)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetPropertyNames(Handle, includeIndices));
        }

        public object GetProperty(int index)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetIndexedProperty(Handle, index));
        }

        public void SetProperty(int index, object value)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Object_SetIndexedProperty(Handle, index, value));
        }

        public bool DeleteProperty(int index)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_DeleteIndexedProperty(Handle, index));
        }

        public int[] GetPropertyIndices()
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetPropertyIndices(Handle));
        }

        public object Invoke(bool asConstructor, object[] args)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_Invoke(Handle, asConstructor, args));
        }

        public object InvokeMethod(string name, object[] args)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_InvokeMethod(Handle, name, args));
        }

        public bool IsPromise => Subtype == V8Value.Subtype.Promise;

        public bool IsArray => Subtype == V8Value.Subtype.Array;

        public bool IsShared => Flags.HasFlag(V8Value.Flags.Shared);

        public bool IsArrayBufferOrView
        {
            get
            {
                switch (Subtype)
                {
                    case V8Value.Subtype.ArrayBuffer:
                    case V8Value.Subtype.DataView:
                    case V8Value.Subtype.Uint8Array:
                    case V8Value.Subtype.Uint8ClampedArray:
                    case V8Value.Subtype.Int8Array:
                    case V8Value.Subtype.Uint16Array:
                    case V8Value.Subtype.Int16Array:
                    case V8Value.Subtype.Uint32Array:
                    case V8Value.Subtype.Int32Array:
                    case V8Value.Subtype.BigUint64Array:
                    case V8Value.Subtype.BigInt64Array:
                    case V8Value.Subtype.Float32Array:
                    case V8Value.Subtype.Float64Array:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public V8ArrayBufferOrViewKind ArrayBufferOrViewKind
        {
            get
            {
                switch (Subtype)
                {
                    case V8Value.Subtype.ArrayBuffer:
                        return V8ArrayBufferOrViewKind.ArrayBuffer;

                    case V8Value.Subtype.DataView:
                        return V8ArrayBufferOrViewKind.DataView;

                    case V8Value.Subtype.Uint8Array:
                        return V8ArrayBufferOrViewKind.Uint8Array;

                    case V8Value.Subtype.Uint8ClampedArray:
                        return V8ArrayBufferOrViewKind.Uint8ClampedArray;

                    case V8Value.Subtype.Int8Array:
                        return V8ArrayBufferOrViewKind.Int8Array;

                    case V8Value.Subtype.Uint16Array:
                        return V8ArrayBufferOrViewKind.Uint16Array;

                    case V8Value.Subtype.Int16Array:
                        return V8ArrayBufferOrViewKind.Int16Array;

                    case V8Value.Subtype.Uint32Array:
                        return V8ArrayBufferOrViewKind.Uint32Array;

                    case V8Value.Subtype.Int32Array:
                        return V8ArrayBufferOrViewKind.Int32Array;

                    case V8Value.Subtype.BigUint64Array:
                        return V8ArrayBufferOrViewKind.BigUint64Array;

                    case V8Value.Subtype.BigInt64Array:
                        return V8ArrayBufferOrViewKind.BigInt64Array;

                    case V8Value.Subtype.Float32Array:
                        return V8ArrayBufferOrViewKind.Float32Array;

                    case V8Value.Subtype.Float64Array:
                        return V8ArrayBufferOrViewKind.Float64Array;

                    default:
                        return V8ArrayBufferOrViewKind.None;
                }
            }
        }

        public V8ArrayBufferOrViewInfo GetArrayBufferOrViewInfo()
        {
            var kind = ArrayBufferOrViewKind;
            if (kind != V8ArrayBufferOrViewKind.None)
            {
                IV8Object arrayBuffer = null;
                var offset = 0UL;
                var size = 0UL;
                var length = 0UL;
                V8SplitProxyNative.Invoke(instance => instance.V8Object_GetArrayBufferOrViewInfo(Handle, out arrayBuffer, out offset, out size, out length));
                return new V8ArrayBufferOrViewInfo(kind, arrayBuffer, offset, size, length);
            }

            return null;
        }

        public void InvokeWithArrayBufferOrViewData(Action<IntPtr> action)
        {
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Object_InvokeWithArrayBufferOrViewData(Handle, pAction));
            }
        }

        #endregion

        #region disposal / finalization

        public void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~V8ObjectImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
