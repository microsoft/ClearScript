// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static class CanonicalRefTable
    {
        private static readonly Dictionary<Type, ICanonicalRefMap> table = new();

        public static object GetCanonicalRef(object obj)
        {
            if (obj is ValueType)
            {
                var map = GetMap(obj);
                if (map is not null)
                {
                    obj = map.GetRef(obj);
                }
            }

            return obj;
        }

        private static ICanonicalRefMap GetMap(object obj)
        {
            var type = obj.GetType();
            lock (table)
            {
                if (!table.TryGetValue(type, out var map))
                {
                    if (type.IsEnum ||
                        type.IsNumeric() ||
                        type == typeof(DateTime) ||
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                    #if NET471_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                        type.GetOrLoadCustomAttributes<System.Runtime.CompilerServices.IsReadOnlyAttribute>(null, false).Any() ||
                    #endif
                        type.GetOrLoadCustomAttributes<ImmutableValueAttribute>(null, false).Any())
                    {
                        map = (ICanonicalRefMap)Activator.CreateInstance(typeof(CanonicalRefMap<>).MakeGenericType(type));
                    }

                    table.Add(type, map);
                }

                return map;
            }
        }

        #region Nested type: ICanonicalRefMap

        private interface ICanonicalRefMap
        {
            object GetRef(object obj);
        }

        #endregion

        #region Nested type: CanonicalRefMapBase

        private abstract class CanonicalRefMapBase : ICanonicalRefMap
        {
            protected const int CompactionThreshold = 256 * 1024;
            protected static readonly TimeSpan CompactionInterval = TimeSpan.FromMinutes(2);

            #region ICanonicalRefMap implementation

            public abstract object GetRef(object obj);

            #endregion
        }

        #endregion

        #region Nested type: CanonicalRefMap<T>

        private sealed class CanonicalRefMap<T> : CanonicalRefMapBase where T : struct
        {
            private readonly Dictionary<T, WeakReference<object>> map = new();
            private DateTime lastCompactionTime = DateTime.MinValue;

            private object GetRefInternal(object obj)
            {
                var value = (T)obj;
                object result;

                if (map.TryGetValue(value, out var weakRef))
                {
                    if (!weakRef.TryGetTarget(out result))
                    {
                        result = obj;
                        weakRef.SetTarget(result);
                    }
                }
                else
                {
                    result = obj;
                    map.Add(value, new WeakReference<object>(result));
                }

                return result;
            }

            private void CompactIfNecessary()
            {
                if (map.Count >= CompactionThreshold)
                {
                    var now = DateTime.UtcNow;
                    if ((lastCompactionTime + CompactionInterval) <= now)
                    {
                        map.Where(pair => !pair.Value.TryGetTarget(out _)).ToList().ForEach(pair => map.Remove(pair.Key));
                        lastCompactionTime = now;
                    }
                }
            }

            #region CanonicalRefMapBase overrides

            public override object GetRef(object obj)
            {
                lock (map)
                {
                    var result = GetRefInternal(obj);
                    CompactIfNecessary();
                    return result;
                }
            }

            #endregion
        }

        #endregion
    }
}
