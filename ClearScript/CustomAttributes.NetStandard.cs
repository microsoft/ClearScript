// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    internal static partial class CustomAttributes
    {
        public static void ClearCache()
        {
            attributeCache.Clear();
        }
    }
}
