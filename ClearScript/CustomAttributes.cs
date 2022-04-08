// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static partial class CustomAttributes
    {
        private static readonly ConcurrentDictionary<(ICustomAttributeProvider, Type), object> keyCache = new ConcurrentDictionary<(ICustomAttributeProvider, Type), object>();
        private static readonly ConditionalWeakTable<object, object> attributeCache = new ConditionalWeakTable<object, object>();

        public static T[] GetOrLoad<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return (T[])attributeCache.GetValue(GetKey<T>(resource), _ => HostSettings.CustomAttributeLoader.LoadCustomAttributes<T>(resource, inherit) ?? ArrayHelpers.GetEmptyArray<T>());
        }

        public static bool Has<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return GetOrLoad<T>(resource, inherit).Length > 0;
        }

        private static object GetKey<T>(ICustomAttributeProvider resource) where T : Attribute
        {
            return keyCache.GetOrAdd((resource, typeof(T)), _ => new object());
        }
    }
}
