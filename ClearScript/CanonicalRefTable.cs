// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

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

        private class CanonicalRefMap<T> : CanonicalRefMapBase
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