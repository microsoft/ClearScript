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
        private static readonly object tableLock = new object();
        private static readonly Dictionary<Type, ICanonicalRefMap> table = new Dictionary<Type, ICanonicalRefMap>();

        public static object GetCanonicalRef(object obj)
        {
            if (obj is ValueType)
            {
                var map = GetMap(obj);
                if (map != null)
                {
                    obj = map.GetRef(obj);
                }
            }

            return obj;
        }

        private static ICanonicalRefMap GetMap(object obj)
        {
            var type = obj.GetType();
            lock (tableLock)
            {
                ICanonicalRefMap map;
                if (!table.TryGetValue(type, out map))
                {
                    if (type.IsEnum ||
                        type.IsNumeric() ||
                        type == typeof(DateTime) ||
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type.GetCustomAttributes(typeof(ImmutableValueAttribute), false).Any())
                    {
                        map = (ICanonicalRefMap)typeof(CanonicalRefMap<>).MakeGenericType(type).CreateInstance();
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

            #region ICanonicalRefMap implementation (abstract)

            public abstract object GetRef(object obj);

            #endregion
        }

        #endregion

        #region Nested type: CanonicalRefMap<T>

        private sealed class CanonicalRefMap<T> : CanonicalRefMapBase
        {
            private readonly object mapLock = new object();
            private readonly Dictionary<T, WeakReference> map = new Dictionary<T, WeakReference>();
            private DateTime lastCompactionTime = DateTime.MinValue;

            private object GetRefInternal(object obj)
            {
                var value = (T)obj;
                object result;

                WeakReference weakRef;
                if (map.TryGetValue(value, out weakRef))
                {
                    result = weakRef.Target;
                    if (result == null)
                    {
                        result = obj;
                        weakRef.Target = result;
                    }
                }
                else
                {
                    result = obj;
                    map.Add(value, new WeakReference(result));
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
                        map.Where(pair => !pair.Value.IsAlive).ToList().ForEach(pair => map.Remove(pair.Key));
                        lastCompactionTime = now;
                    }
                }
            }

            #region CanonicalRefMapBase overrides

            public override object GetRef(object obj)
            {
                lock (mapLock)
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