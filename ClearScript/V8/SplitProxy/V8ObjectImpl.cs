// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

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

                if (Flags.HasAllFlags(V8Value.Flags.Shared))
                {
                    flags |= JavaScriptObjectFlags.Shared;
                }

                if (Flags.HasAllFlags(V8Value.Flags.Async))
                {
                    flags |= JavaScriptObjectFlags.Async;
                }

                if (Flags.HasAllFlags(V8Value.Flags.Generator))
                {
                    flags |= JavaScriptObjectFlags.Generator;
                }

                if (Flags.HasAllFlags(V8Value.Flags.Pending))
                {
                    flags |= JavaScriptObjectFlags.Pending;
                }

                if (Flags.HasAllFlags(V8Value.Flags.Rejected))
                {
                    flags |= JavaScriptObjectFlags.Rejected;
                }

                return flags;
            }
        }

        public int IdentityHash { get; }

        public object GetProperty(string name)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_GetNamedProperty(ctx.Handle, ctx.name), (Handle, name));
        }

        public bool TryGetProperty(string name, out object value)
        {
            var ctx = (Handle, name, value: (object)null);

            var result = V8SplitProxyNative.Invoke(
                static (instance, pCtx) =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    return instance.V8Object_TryGetNamedProperty(ctx.Handle, ctx.name, out ctx.value);
                },
                StructPtr.FromRef(ref ctx)
            );

            value = ctx.value;
            return result;
        }

        public void SetProperty(string name, object value)
        {
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_SetNamedProperty(ctx.Handle, ctx.name, ctx.value), (Handle, name, value));
        }

        public bool DeleteProperty(string name)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_DeleteNamedProperty(ctx.Handle, ctx.name), (Handle, name));
        }

        public string[] GetPropertyNames(bool includeIndices)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_GetPropertyNames(ctx.Handle, ctx.includeIndices), (Handle, includeIndices));
        }

        public object GetProperty(int index)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_GetIndexedProperty(ctx.Handle, ctx.index), (Handle, index));
        }

        public void SetProperty(int index, object value)
        {
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_SetIndexedProperty(ctx.Handle, ctx.index, ctx.value), (Handle, index, value));
        }

        public bool DeleteProperty(int index)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_DeleteIndexedProperty(ctx.Handle, ctx.index), (Handle, index));
        }

        public int[] GetPropertyIndices()
        {
            return V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Object_GetPropertyIndices(handle), Handle);
        }

        public object Invoke(bool asConstructor, object[] args)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_Invoke(ctx.Handle, ctx.asConstructor, ctx.args), (Handle, asConstructor, args));
        }

        public object InvokeMethod(string name, object[] args)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_InvokeMethod(ctx.Handle, ctx.name, ctx.args), (Handle, name, args));
        }

        public bool IsPromise => Subtype == V8Value.Subtype.Promise;

        public bool IsArray => Subtype == V8Value.Subtype.Array;

        public bool IsShared => Flags.HasAllFlags(V8Value.Flags.Shared);

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
                var ctx = (Handle, arrayBuffer: (IV8Object)null, offset: 0UL, size: 0UL, length: 0UL);

                V8SplitProxyNative.Invoke(
                    static (instance, pCtx) =>
                    {
                        ref var ctx = ref pCtx.AsRef();
                        instance.V8Object_GetArrayBufferOrViewInfo(ctx.Handle, out ctx.arrayBuffer, out ctx.offset, out ctx.size, out ctx.length);
                    },
                    StructPtr.FromRef(ref ctx)
                );

                return new V8ArrayBufferOrViewInfo(kind, ctx.arrayBuffer, ctx.offset, ctx.size, ctx.length);
            }

            return null;
        }

        public void InvokeWithArrayBufferOrViewData(Action<IntPtr> action)
        {
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_InvokeWithArrayBufferOrViewData(ctx.Handle, ctx.pAction), (Handle, pAction));
            }
        }

        public unsafe void InvokeWithArrayBufferOrViewData<TArg>(Action<IntPtr, TArg> action, in TArg arg)
        {
            var ctx = (action, arg);
            var pCtx = (IntPtr)Unsafe.AsPointer(ref ctx);

            Action<IntPtr, IntPtr> actionWithArg = static (pData, pCtx) =>
            {
                ref var ctx = ref Unsafe.AsRef<(Action<IntPtr, TArg> action, TArg arg)>(pCtx.ToPointer());
                ctx.action(pData, ctx.arg);
            };

            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(actionWithArg))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Object_InvokeWithArrayBufferOrViewDataWithArg(ctx.Handle, ctx.pAction, ctx.pCtx), (Handle, pAction, pCtx));
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
