// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.FastProxy
{
    internal static class NullableHelpers
    {
        private static readonly ConditionalWeakTable<Type, Holder<IImpl>> table = new();

        public static bool TryGet<TNullable>(Type underlyingType, in V8Value.FastArg arg, object obj, out TNullable value)
        {
            return GetImpl(underlyingType).TryGet(arg, obj, out value);
        }

        public static void Set<TNullable>(Type underlyingType, in V8FastResult result, ref TNullable value)
        {
            GetImpl(underlyingType).Set(result, ref value);
        }

        private static IImpl GetImpl(Type underlyingType)
        {
            lock (table)
            {
                var holder = table.GetOrCreateValue(underlyingType);
                return holder.Value ?? (holder.Value = CreateImpl(underlyingType));
            }
        }

        private static IImpl CreateImpl(Type underlyingType)
        {
            return (IImpl)typeof(Impl<>).MakeGenericType(underlyingType).CreateInstance();
        }

        #region Nested type: IImpl

        private interface IImpl
        {
            bool TryGet<TNullable>(in V8Value.FastArg arg, object obj, out TNullable value);
            void Set<TNullable>(in V8FastResult result, ref TNullable value);
        }

        #endregion

        #region Nested type: Impl<TUnderlying>

        private sealed class Impl<TUnderlying> : IImpl where TUnderlying : struct
        {
            #region IImpl implementation

            bool IImpl.TryGet<TNullable>(in V8Value.FastArg arg, object obj, out TNullable value)
            {
                value = default;
                return V8FastArgImpl.TryGet(arg, obj, out Unsafe.As<TNullable, TUnderlying?>(ref value));
            }

            void IImpl.Set<TNullable>(in V8FastResult result, ref TNullable value)
            {
                result.SetNullable(Unsafe.As<TNullable, TUnderlying?>(ref value));
            }

            #endregion
        }

        #endregion
    }
}
