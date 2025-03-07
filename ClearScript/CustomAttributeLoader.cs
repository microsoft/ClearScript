// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a custom attribute loader.
    /// </summary>
    public class CustomAttributeLoader
    {
        private readonly CustomAttributeCache cache = new();

        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="CustomAttributeLoader"/></c> instance.
        /// </summary>
        public CustomAttributeLoader()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        /// <summary>
        /// Gets the default custom attribute loader.
        /// </summary>
        public static CustomAttributeLoader Default { get; } = new();

        // ReSharper restore EmptyConstructor

        /// <summary>
        /// Loads custom attributes of the specified type for the given resource.
        /// </summary>
        /// <typeparam name="T">The type, or a base type, of the custom attributes to load.</typeparam>
        /// <param name="resource">The resource for which to load custom attributes of type <typeparamref name="T"/>.</param>
        /// <param name="inherit"><c>True</c> to include custom attributes of type <typeparamref name="T"/> defined for ancestors of <paramref name="resource"/>, <c>false</c> otherwise.</param>
        /// <returns>An array of custom attributes of type <typeparamref name="T"/>.</returns>.
        /// <remarks>
        /// This method is performance-critical. Overrides must not invoke script engine methods or
        /// other ClearScript functionality. The base implementation loads custom attributes via
        /// reflection.
        /// </remarks>
        public virtual T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            if (resource is MemberInfo member)
            {
                return Attribute.GetCustomAttributes(member, typeof(T), inherit).OfType<T>().ToArray();
            }

            if (resource is ParameterInfo parameter)
            {
                return Attribute.GetCustomAttributes(parameter, typeof(T), inherit).OfType<T>().ToArray();
            }

            if (resource is Assembly assembly)
            {
                return Attribute.GetCustomAttributes(assembly, typeof(T), inherit).OfType<T>().ToArray();
            }

            if (resource is Module module)
            {
                return Attribute.GetCustomAttributes(module, typeof(T), inherit).OfType<T>().ToArray();
            }

            return resource.GetCustomAttributes(typeof(T), inherit).OfType<T>().ToArray();
        }

        internal T[] GetOrLoad<T>(ICustomAttributeProvider resource, bool inherit) where T : Attribute
        {
            return cache.GetOrLoad<T>(this, resource, inherit);
        }
    }
}
