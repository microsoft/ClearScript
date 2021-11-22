// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8ObjectImpl : IV8Object
    {
        private V8EntityHolder holder;

        public V8Object.Handle Handle => (V8Object.Handle)holder.Handle;

        public V8ObjectImpl(V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags)
        {
            holder = new V8EntityHolder("V8 object", () => hObject);
            Subtype = subtype;
            Flags = flags;
        }

        public V8Value.Subtype Subtype { get; }

        public V8Value.Flags Flags { get; }

        #region IV8Object implementation

        public object GetProperty(string name)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetNamedProperty(Handle, name));
        }

        public void SetProperty(string name, object value)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Object_SetNamedProperty(Handle, name, value));
        }

        public bool DeleteProperty(string name)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_DeleteNamedProperty(Handle, name));
        }

        public string[] GetPropertyNames()
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Object_GetPropertyNames(Handle));
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

        public V8ArrayBufferOrViewKind GetArrayBufferOrViewKind()
        {
            var kind = V8ArrayBufferOrViewKind.None;

            if (Subtype == V8Value.Subtype.ArrayBuffer)
                kind = V8ArrayBufferOrViewKind.ArrayBuffer;
            else if (Subtype == V8Value.Subtype.DataView)
                kind = V8ArrayBufferOrViewKind.DataView;
            else if (Subtype == V8Value.Subtype.Uint8Array)
                kind = V8ArrayBufferOrViewKind.Uint8Array;
            else if (Subtype == V8Value.Subtype.Uint8ClampedArray)
                kind = V8ArrayBufferOrViewKind.Uint8ClampedArray;
            else if (Subtype == V8Value.Subtype.Int8Array)
                kind = V8ArrayBufferOrViewKind.Int8Array;
            else if (Subtype == V8Value.Subtype.Uint16Array)
                kind = V8ArrayBufferOrViewKind.Uint16Array;
            else if (Subtype == V8Value.Subtype.Int16Array)
                kind = V8ArrayBufferOrViewKind.Int16Array;
            else if (Subtype == V8Value.Subtype.Uint32Array)
                kind = V8ArrayBufferOrViewKind.Uint32Array;
            else if (Subtype == V8Value.Subtype.Int32Array)
                kind = V8ArrayBufferOrViewKind.Int32Array;
            else if (Subtype == V8Value.Subtype.BigUint64Array)
                kind = V8ArrayBufferOrViewKind.BigUint64Array;
            else if (Subtype == V8Value.Subtype.BigInt64Array)
                kind = V8ArrayBufferOrViewKind.BigInt64Array;
            else if (Subtype == V8Value.Subtype.Float32Array)
                kind = V8ArrayBufferOrViewKind.Float32Array;
            else if (Subtype == V8Value.Subtype.Float64Array)
                kind = V8ArrayBufferOrViewKind.Float64Array;

            return kind;
        }

        public V8ArrayBufferOrViewInfo GetArrayBufferOrViewInfo()
        {
            var kind = GetArrayBufferOrViewKind();
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
