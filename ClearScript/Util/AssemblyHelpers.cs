// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.ClearScript.Util
{
    internal static partial class AssemblyHelpers
    {
        public static string GetFullAssemblyName(string name)
        {
            // ReSharper disable AccessToModifiedClosure

            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            Assembly assembly;
            if (MiscHelpers.Try(out assembly, () => Assembly.Load(name)))
            {
                return assembly.FullName;
            }

            var fileName = name;
            if (!string.Equals(Path.GetExtension(fileName), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName + '.', "dll");
            }

            AssemblyName assemblyName;
            if (MiscHelpers.Try(out assemblyName, () => AssemblyName.GetAssemblyName(fileName)))
            {
                return assemblyName.FullName;
            }

            var dirPath = Path.GetDirectoryName(typeof(string).Assembly.Location);
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                var path = Path.Combine(dirPath, fileName);
                if (File.Exists(path) && MiscHelpers.Try(out assemblyName, () => AssemblyName.GetAssemblyName(path)))
                {
                    return assemblyName.FullName;
                }
            }

            return name;

            // ReSharper restore AccessToModifiedClosure
        }

        public static Assembly TryLoad(AssemblyName name)
        {
            Assembly assembly;
            if (MiscHelpers.Try(out assembly, () => Assembly.Load(name)))
            {
                return assembly;
            }

            return null;
        }

        public static T GetAttribute<T>(this Assembly assembly, bool inherit) where T : Attribute
        {
            return Attribute.GetCustomAttributes(assembly, typeof(T), inherit).SingleOrDefault() as T;
        }

        public static IEnumerable<T> GetAttributes<T>(this Assembly assembly, bool inherit) where T : Attribute
        {
            return Attribute.GetCustomAttributes(assembly, typeof(T), inherit).OfType<T>();
        }

        public static bool IsFriendOf(this Assembly thisAssembly, Assembly thatAssembly)
        {
            if (thatAssembly == thisAssembly)
            {
                return true;
            }

            var thisName = thisAssembly.GetName();
            foreach (var attribute in thatAssembly.GetAttributes<InternalsVisibleToAttribute>(false))
            {
                var thatName = new AssemblyName(attribute.AssemblyName);
                if (AssemblyName.ReferenceMatchesDefinition(thatName, thisName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
