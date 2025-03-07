// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#if NET5_0_OR_GREATER

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal static partial class V8SplitProxyManaged
    {
        #region fast method pointers

        private readonly struct StdBool
        {
            private readonly byte bits;

            // ReSharper disable once UnusedMember.Local
            private StdBool(byte bits) => this.bits = bits;

            private static readonly StdBool @false = new(0);
            private static readonly StdBool @true = new(1);

            public static implicit operator bool(StdBool value) => value.bits != 0;
            public static implicit operator StdBool(bool value) => value ? @true : @false;

            public static void Write(IntPtr ptr, StdBool value) => Marshal.WriteByte(ptr, value.bits);
        }

        private static unsafe IntPtr CacheV8ObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pCache, IntPtr pObject, IntPtr pV8Object)
                {
                    CacheV8Object(pCache, pObject, pV8Object);
                }

                delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetCachedV8ObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static IntPtr Thunk(IntPtr pCache, IntPtr pObject)
                {
                    return GetCachedV8Object(pCache, pObject);
                }

                delegate* unmanaged[Stdcall]<IntPtr, IntPtr, IntPtr> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr AddRefHostObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static IntPtr Thunk(IntPtr pObject)
                {
                    return AddRefHostObject(pObject);
                }

                delegate* unmanaged[Stdcall]<IntPtr, IntPtr> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr ReleaseHostObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject)
                {
                    ReleaseHostObject(pObject);
                }

                delegate* unmanaged[Stdcall]<IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetHostObjectInvocabilityFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static Invocability Thunk(IntPtr pObject)
                {
                    return GetHostObjectInvocability(pObject);
                }

                delegate* unmanaged[Stdcall]<IntPtr, Invocability> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr InvokeHostActionFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pAction)
                {
                    InvokeHostAction(pAction);
                }

                delegate* unmanaged[Stdcall]<IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr InvokeHostActionWithArgFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pAction, IntPtr pArg)
                {
                    InvokeHostActionWithArg(pAction, pArg);
                }

                delegate* unmanaged[Stdcall]<IntPtr, IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetHostObjectNamedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdString.Ptr pName, V8Value.Ptr pValue, IntPtr pIsCacheable)
                {
                    GetHostObjectNamedProperty(pObject, pName, pValue, out var isCacheable);
                    StdBool.Write(pIsCacheable, isCacheable);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdString.Ptr, V8Value.Ptr, IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr SetHostObjectNamedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdString.Ptr pName, V8Value.Decoded.Ptr pValue)
                {
                    SetHostObjectNamedProperty(pObject, pName, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdString.Ptr, V8Value.Decoded.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetHostObjectIndexedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, int index, V8Value.Ptr pValue)
                {
                    GetHostObjectIndexedProperty(pObject, index, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, int, V8Value.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr SetHostObjectIndexedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, int index, V8Value.Decoded.Ptr pValue)
                {
                    SetHostObjectIndexedProperty(pObject, index, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, int, V8Value.Decoded.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr InvokeHostObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdBool asConstructor, int argCount, V8Value.Decoded.Ptr pArgs, V8Value.Ptr pResult)
                {
                    InvokeHostObject(pObject, asConstructor, argCount, pArgs, pResult);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdBool, int, V8Value.Decoded.Ptr, V8Value.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr InvokeHostObjectMethodFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdString.Ptr pName, int argCount, V8Value.Decoded.Ptr pArgs, V8Value.Ptr pResult)
                {
                    InvokeHostObjectMethod(pObject, pName, argCount, pArgs, pResult);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdString.Ptr, int, V8Value.Decoded.Ptr, V8Value.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetFastHostObjectNamedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdString.Ptr pName, V8Value.FastResult.Ptr pValue, IntPtr pIsCacheable)
                {
                    GetFastHostObjectNamedProperty(pObject, pName, pValue, out var isCacheable);
                    StdBool.Write(pIsCacheable, isCacheable);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdString.Ptr, V8Value.FastResult.Ptr, IntPtr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr SetFastHostObjectNamedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdString.Ptr pName, V8Value.FastArg.Ptr pValue)
                {
                    SetFastHostObjectNamedProperty(pObject, pName, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdString.Ptr, V8Value.FastArg.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr GetFastHostObjectIndexedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, int index, V8Value.FastResult.Ptr pValue)
                {
                    GetFastHostObjectIndexedProperty(pObject, index, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, int, V8Value.FastResult.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr SetFastHostObjectIndexedPropertyFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, int index, V8Value.FastArg.Ptr pValue)
                {
                    SetFastHostObjectIndexedProperty(pObject, index, pValue);
                }

                delegate* unmanaged[Stdcall]<IntPtr, int, V8Value.FastArg.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        private static unsafe IntPtr InvokeFastHostObjectFastMethodPtr
        {
            get
            {
                [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
                static void Thunk(IntPtr pObject, StdBool asConstructor, int argCount, V8Value.FastArg.Ptr pArgs, V8Value.FastResult.Ptr pResult)
                {
                    InvokeFastHostObject(pObject, asConstructor, argCount, pArgs, pResult);
                }

                delegate* unmanaged[Stdcall]<IntPtr, StdBool, int, V8Value.FastArg.Ptr, V8Value.FastResult.Ptr, void> pThunk = &Thunk;
                return (IntPtr)pThunk;
            }
        }

        #endregion
    }
}

#endif // NET5_0_OR_GREATER
