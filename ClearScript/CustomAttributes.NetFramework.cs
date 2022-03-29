// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static partial class CustomAttributes
    {
        public static void ClearCache()
        {
            keyCache.Values.ForEach(key => attributeCache.Remove(key));
        }
    }
}
