// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static partial class CustomAttributes
    {
        private static readonly object cacheLock = new object();

        public static T[] GetOrLoad<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            lock (cacheLock)
            {
                return GetOrLoad<T>(cache.GetOrCreateValue(resource), resource, inherit);
            }
        }

        public static bool Has<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            lock (cacheLock)
            {
                return Has<T>(cache.GetOrCreateValue(resource), resource, inherit);
            }
        }

        private static T[] GetOrLoad<T>(CacheEntry entry, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            if (entry.TryGet<T>(out var attrs))
            {
                return attrs;
            }

            attrs = GetOrLoad<T>(GetIsBypass(entry, resource), resource, inherit);
            entry.Add(attrs);

            return attrs;
        }

        private static bool Has<T>(CacheEntry entry, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return GetOrLoad<T>(entry, resource, inherit).Length > 0;
        }

        private static T[] GetOrLoad<T>(bool isBypass, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            var loader = isBypass ? HostSettings.DefaultCustomAttributeLoader : HostSettings.CustomAttributeLoader;
            return loader.LoadCustomAttributes<T>(resource, inherit) ?? ArrayHelpers.GetEmptyArray<T>();
        }

        private static bool Has<T>(bool isBypass, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return GetOrLoad<T>(isBypass, resource, inherit).Length > 0;
        }

        private static bool GetIsBypass(ICustomAttributeProvider resource)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return GetIsBypass(cache.GetOrCreateValue(resource), resource);
        }

        private static bool GetIsBypass(CacheEntry entry, ICustomAttributeProvider resource)
        {
            if (!entry.IsBypass.HasValue)
            {
                entry.IsBypass = GetIsBypassInternal(resource);
            }

            return entry.IsBypass.Value;
        }

        private static bool GetIsBypassInternal(ICustomAttributeProvider resource)
        {
            if (Has<BypassCustomAttributeLoaderAttribute>(true, resource, false))
            {
                return true;
            }

            var parent = GetParent(resource);
            if (parent != null)
            {
                return GetIsBypass(parent);
            }

            return false;
        }

        private static ICustomAttributeProvider GetParent(ICustomAttributeProvider resource)
        {
            if (resource is ParameterInfo parameter)
            {
                return parameter.Member;
            }

            if (resource is Type type)
            {
                return (type.DeclaringType as ICustomAttributeProvider) ?? type.Module;
            }

            if (resource is MemberInfo member)
            {
                return member.DeclaringType;
            }

            if (resource is Module module)
            {
                return module.Assembly;
            }

            return null;
        }

        #region Nested type: CacheEntry

        // ReSharper disable ClassNeverInstantiated.Local

        private sealed class CacheEntry
        {
            private readonly Dictionary<Type, object> map = new Dictionary<Type, object>();

            public bool? IsBypass { get; set; }

            public void Add<T>(T[] attrs)
            {
                map.Add(typeof(T), attrs);
            }

            public bool TryGet<T>(out T[] attrs)
            {
                if (map.TryGetValue(typeof(T), out var attrsObject))
                {
                    attrs = attrsObject as T[];
                    return true;
                }

                attrs = null;
                return false;
            }
        }

        // ReSharper restore ClassNeverInstantiated.Local

        #endregion
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    internal sealed class BypassCustomAttributeLoaderAttribute : Attribute
    {
    }
}
