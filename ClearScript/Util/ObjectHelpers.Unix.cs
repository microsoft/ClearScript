// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal static partial class ObjectHelpers
    {
        public static Type GetTypeOrTypeInfo(this object value)
        {
            return value.GetType();
        }
    }
}
