// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal static class AssemblyHelpers
    {
        public static T GetAttribute<T>(this Assembly assembly, bool inherit) where T : Attribute
        {
            return Attribute.GetCustomAttributes(assembly, typeof(T), inherit).SingleOrDefault() as T;
        }
    }
}
