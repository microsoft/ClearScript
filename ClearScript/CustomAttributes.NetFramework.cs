// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static partial class CustomAttributes
    {
        private static ConditionalWeakTable<ICustomAttributeProvider, CacheEntry> cache = new ConditionalWeakTable<ICustomAttributeProvider, CacheEntry>();

        public static void ClearCache()
        {
            lock (cacheLock)
            {
                cache = new ConditionalWeakTable<ICustomAttributeProvider, CacheEntry>();
            }
        }
    }
}
