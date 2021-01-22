// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.ClearScript.Util
{
    internal sealed class ConcurrentWeakSet<T> where T : class
    {
        private readonly object dataLock = new object();
        private List<WeakReference> weakRefs = new List<WeakReference>();

        public int Count => GetItems().Count;

        public bool Contains(T item)
        {
            MiscHelpers.VerifyNonNullArgument(item, nameof(item));
            return GetItems().Contains(item);
        }

        public bool TryAdd(T item)
        {
            MiscHelpers.VerifyNonNullArgument(item, nameof(item));
            lock (dataLock)
            {
                if (!GetItemsInternal().Contains(item))
                {
                    weakRefs.Add(new WeakReference(item));
                    return true;
                }

                return false;
            }
        }

        public void ForEach(Action<T> action)
        {
            MiscHelpers.VerifyNonNullArgument(action, nameof(action));
            GetItems().ForEach(action);
        }

        private List<T> GetItems()
        {
            lock (dataLock)
            {
                return GetItemsInternal();
            }
        }

        private List<T> GetItemsInternal()
        {
            var items = new List<T>();
            var tempWeakRefs = new List<WeakReference>();
            foreach (var weakRef in weakRefs)
            {
                if (weakRef.Target is T item)
                {
                    items.Add(item);
                    tempWeakRefs.Add(weakRef);
                }
            }

            weakRefs = tempWeakRefs;
            return items;
        }
    }
}
