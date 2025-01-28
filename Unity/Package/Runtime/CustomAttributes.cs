// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static class CustomAttributes
    {
        public static T[] GetOrLoad<T>(IHostContext context, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            var loader = context?.CustomAttributeLoader ?? HostSettings.CustomAttributeLoader;
            return loader.GetOrLoad<T>(resource, inherit);
        }

        public static bool Has<T>(IHostContext context, ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return GetOrLoad<T>(context, resource, inherit).Length > 0;
        }
    }
}
