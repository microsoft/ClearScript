// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal static partial class AssemblyHelpers
    {
        public static IEnumerable<Type> GetAllTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes().Concat(assembly.GetForwardedTypes());
            }
            catch (ReflectionTypeLoadException exception)
            {
                // ReSharper disable once RedundantEnumerableCastCall
                return exception.Types.OfType<Type>();
            }
        }
    }
}
