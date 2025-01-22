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
            if (string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (MiscHelpers.Try(out var assembly, () => Assembly.Load(name)))
            {
                return assembly.FullName;
            }

            var fileName = name;
            if (!string.Equals(Path.GetExtension(fileName), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                fileName = Path.ChangeExtension(fileName + '.', "dll");
            }

            if (MiscHelpers.Try(out var assemblyName, () => AssemblyName.GetAssemblyName(fileName)))
            {
                return assemblyName.FullName;
            }

            var dirPath = Path.GetDirectoryName(typeof(string).Assembly.Location);
            if (!string.IsNullOrWhiteSpace(dirPath))
            {
                // ReSharper disable AccessToModifiedClosure

                var path = Path.Combine(dirPath, fileName);
                if (File.Exists(path) && MiscHelpers.Try(out assemblyName, () => AssemblyName.GetAssemblyName(path)))
                {
                    return assemblyName.FullName;
                }

                if (MiscHelpers.Try(out var subDirPaths, () => Directory.EnumerateDirectories(dirPath, "*", SearchOption.AllDirectories)))
                {
                    foreach (var subDirPath in subDirPaths)
                    {
                        path = Path.Combine(subDirPath, fileName);
                        if (File.Exists(path) && MiscHelpers.Try(out assemblyName, () => AssemblyName.GetAssemblyName(path)))
                        {
                            return assemblyName.FullName;
                        }
                    }
                }

                // ReSharper restore AccessToModifiedClosure
            }

            return name;
        }

        public static Assembly TryLoad(AssemblyName name)
        {
            if (MiscHelpers.Try(out var assembly, () => Assembly.Load(name)))
            {
                return assembly;
            }

            return null;
        }

        public static T GetOrLoadCustomAttribute<T>(this Assembly assembly, IHostContext context, bool inherit = true) where T : Attribute
        {
            return CustomAttributes.GetOrLoad<T>(context, assembly, inherit).SingleOrDefault();
        }

        public static IEnumerable<T> GetOrLoadCustomAttributes<T>(this Assembly assembly, IHostContext context, bool inherit = true) where T : Attribute
        {
            return CustomAttributes.GetOrLoad<T>(context, assembly, inherit);
        }

        public static bool HasCustomAttributes<T>(this Assembly assembly, IHostContext context, bool inherit = true) where T : Attribute
        {
            return CustomAttributes.Has<T>(context, assembly, inherit);
        }

        public static bool IsFriendOf(this Assembly thisAssembly, IHostContext context, Assembly thatAssembly)
        {
            if (thatAssembly == thisAssembly)
            {
                return true;
            }

            var thisName = thisAssembly.GetName();
            foreach (var attribute in thatAssembly.GetOrLoadCustomAttributes<InternalsVisibleToAttribute>(context, false))
            {
                var thatName = new AssemblyName(attribute.AssemblyName);
                if (AssemblyName.ReferenceMatchesDefinition(thatName, thisName))
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<Type> GetReferencedEnums(this Assembly assembly)
        {
            var processedTypes = new HashSet<Type>();
            return assembly.GetAllTypes().SelectMany(type => GetReferencedEnums(assembly, type, processedTypes));
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, Type type, HashSet<Type> processedTypes)
        {
            if ((type == null) || !type.IsVisible || type.ContainsGenericParameters || processedTypes.Contains(type))
            {
                yield break;
            }

            processedTypes.Add(type);

            if (type.IsEnum)
            {
                yield return type;
                yield break;
            }

            foreach (var enumType in GetReferencedEnums(assembly, type.GetElementType(), processedTypes))
            {
                yield return enumType;
            }

            foreach (var enumType in type.GetGenericArguments().SelectMany(argType => GetReferencedEnums(assembly, argType, processedTypes)))
            {
                yield return enumType;
            }

            foreach (var enumType in GetReferencedEnums(assembly, type.BaseType, processedTypes))
            {
                yield return enumType;
            }

            foreach (var enumType in type.GetInterfaces().SelectMany(interfaceType => GetReferencedEnums(assembly, interfaceType, processedTypes)))
            {
                yield return enumType;
            }

            if (type.Assembly == assembly)
            {
                foreach (var enumType in type.GetMembers().SelectMany(member => GetReferencedEnums(assembly, member, processedTypes)))
                {
                    yield return enumType;
                }
            }
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, MemberInfo member, HashSet<Type> processedTypes)
        {
            if (member == null)
            {
                return Enumerable.Empty<Type>();
            }

            if (member.MemberType == MemberTypes.Field)
            {
                return GetReferencedEnums(assembly, (FieldInfo)member, processedTypes);
            }

            if (member.MemberType == MemberTypes.Property)
            {
                return GetReferencedEnums(assembly, (PropertyInfo)member, processedTypes);
            }

            if (member.MemberType == MemberTypes.Method)
            {
                return GetReferencedEnums(assembly, (MethodInfo)member, processedTypes);
            }

            if (member.MemberType == MemberTypes.NestedType)
            {
                return GetReferencedEnums(assembly, (Type)member, processedTypes);
            }

            return Enumerable.Empty<Type>();
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, FieldInfo field, HashSet<Type> processedTypes)
        {
            if (field == null)
            {
                return Enumerable.Empty<Type>();
            }

            return GetReferencedEnums(assembly, field.FieldType, processedTypes);
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, PropertyInfo property, HashSet<Type> processedTypes)
        {
            if (property == null)
            {
                yield break;
            }

            foreach (var enumType in GetReferencedEnums(assembly, property.PropertyType, processedTypes))
            {
                yield return enumType;
            }

            foreach (var enumType in GetReferencedEnums(assembly, property.GetMethod, processedTypes))
            {
                yield return enumType;
            }

            foreach (var enumType in GetReferencedEnums(assembly, property.SetMethod, processedTypes))
            {
                yield return enumType;
            }
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, MethodInfo method, HashSet<Type> processedTypes)
        {
            if (method == null)
            {
                yield break;
            }

            foreach (var enumType in GetReferencedEnums(assembly, method.ReturnParameter, processedTypes))
            {
                yield return enumType;
            }

            foreach (var enumType in method.GetParameters().SelectMany(param => GetReferencedEnums(assembly, param, processedTypes)))
            {
                yield return enumType;
            }
        }

        private static IEnumerable<Type> GetReferencedEnums(Assembly assembly, ParameterInfo param, HashSet<Type> processedTypes)
        {
            if (param == null)
            {
                return Enumerable.Empty<Type>();
            }

            return GetReferencedEnums(assembly, param.ParameterType, processedTypes);
        }
    }
}
