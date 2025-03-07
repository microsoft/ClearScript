// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class CustomAttributeCache
    {
        private readonly ConditionalWeakTable<ICustomAttributeProvider, Entry> table = new();

        public T[] GetOrLoad<T>(CustomAttributeLoader loader, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            lock (table)
            {
                return GetOrLoad<T>(loader, table.GetOrCreateValue(resource), resource, inherit);
            }
        }

        private T[] GetOrLoad<T>(CustomAttributeLoader loader, Entry entry, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            if (entry.TryGet<T>(out var attrs))
            {
                return attrs;
            }

            attrs = Load<T>(GetIsBypass(entry, resource) ? CustomAttributeLoader.Default : loader, resource, inherit);
            entry.Add(attrs);

            return attrs;
        }

        private static T[] Load<T>(CustomAttributeLoader loader, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return loader.LoadCustomAttributes<T>(resource, inherit) ?? ArrayHelpers.GetEmptyArray<T>();
        }

        private bool GetIsBypass(ICustomAttributeProvider resource)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return GetIsBypass(table.GetOrCreateValue(resource), resource);
        }

        private bool GetIsBypass(Entry entry, ICustomAttributeProvider resource)
        {
            if (!entry.IsBypass.HasValue)
            {
                entry.IsBypass = GetIsBypassInternal(resource);
            }

            return entry.IsBypass.Value;
        }

        private bool GetIsBypassInternal(ICustomAttributeProvider resource)
        {
            if (Load<BypassCustomAttributeLoaderAttribute>(CustomAttributeLoader.Default, resource, false).Length > 0)
            {
                return true;
            }

            var parent = GetParent(resource);
            if (parent is not null)
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

        #region Nested type: Entry

        // ReSharper disable ClassNeverInstantiated.Local

        private sealed class Entry
        {
            private readonly Dictionary<Type, object> map = new();

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
