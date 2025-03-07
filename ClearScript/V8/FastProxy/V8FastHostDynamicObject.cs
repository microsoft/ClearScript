// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostObject"/></c> with a shared configuration and support for dynamic properties.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <remarks>
    /// This class is a generic base for fast host object classes derived from it via the
    /// <see href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">Curiously Recurring Template Pattern (CRTP)</see>.
    /// It uses a shared instance of <c><see cref="V8FastHostObjectConfiguration{TObject}"/></c>
    /// for each unique type argument. Derived classes are therefore advised to use a static
    /// constructor to invoke the <c><see cref="Configure"/></c> method.
    /// </remarks>
    public abstract class V8FastHostDynamicObject<TObject> : V8FastHostObject<TObject>, IV8FastHostDynamicProperties where TObject : V8FastHostDynamicObject<TObject>
    {
        private Dictionary<string, object> namedProperties;
        private Dictionary<int, object> indexedProperties;

        private Dictionary<string, object> NamedProperties => namedProperties ?? (namedProperties = new Dictionary<string, object>());

        private Dictionary<int, object> IndexedProperties => indexedProperties ?? (indexedProperties = new Dictionary<int, object>());

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostDynamicObject{TObject}"/></c> instance.
        /// </summary>
        protected V8FastHostDynamicObject()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        /// <summary>
        /// Prepares the shared configuration associated with <typeparamref name="TObject"/>.
        /// </summary>
        /// <param name="configurator">The configuration callback.</param>
        /// <remarks>
        /// Derived classes are advised to invoke this method from a static constructor. Once
        /// prepared, a fast host object configuration cannot be modified.
        /// </remarks>
        protected new static void Configure(V8FastHostObjectConfigurator<TObject> configurator)
        {
            V8FastHostObject<TObject>.Configure(configuration =>
            {
                configuration.SetDynamicPropertiesGetter(static instance => instance);
                configurator(configuration);
            });
        }

        #region IV8FastHostDynamicProperties implementation

        /// <inheritdoc/>
        public IEnumerable<string> DynamicPropertyNames => (IEnumerable<string>)namedProperties?.Keys ?? Enumerable.Empty<string>();

        /// <inheritdoc/>
        public IEnumerable<int> DynamicPropertyIndices => (IEnumerable<int>)indexedProperties?.Keys ?? Enumerable.Empty<int>();

        /// <summary>
        /// Gets the value of a named dynamic property.
        /// </summary>
        /// <param name="name">The name of the dynamic property to get.</param>
        /// <param name="value">On return, the value of the dynamic property if it was found.</param>
        public void GetDynamicProperty(string name, in V8FastResult value)
        {
            // the help file builder (SHFB) fails to inherit the documentation for this method

            if (namedProperties?.TryGetValue(name, out var tempValue) == true)
            {
                value.Set(tempValue);
            }
        }

        /// <summary>
        /// Sets the value of a named dynamic property.
        /// </summary>
        /// <param name="name">The name of the dynamic property to set.</param>
        /// <param name="value">The property value.</param>
        public void SetDynamicProperty(string name, in V8FastArg value)
        {
            // the help file builder (SHFB) fails to inherit the documentation for this method

            if (value.IsNull)
            {
                NamedProperties[name] = null;
            }
            else if (value.TryGet(out object tempValue))
            {
                NamedProperties[name] = tempValue;
            }
            else
            {
                throw new ArgumentException("Invalid property value", nameof(value));
            }
        }

        /// <summary>
        /// Gets the value of an indexed dynamic property.
        /// </summary>
        /// <param name="index">The index of the dynamic property to get.</param>
        /// <param name="value">On return, the value of the dynamic property if it was found.</param>
        public void GetDynamicProperty(int index, in V8FastResult value)
        {
            // the help file builder (SHFB) fails to inherit the documentation for this method

            if (indexedProperties?.TryGetValue(index, out var tempValue) == true)
            {
                value.Set(tempValue);
            }
        }

        /// <summary>
        /// Sets the value of an indexed dynamic property.
        /// </summary>
        /// <param name="index">The index of the dynamic property to set.</param>
        /// <param name="value">The property value.</param>
        public void SetDynamicProperty(int index, in V8FastArg value)
        {
            // the help file builder (SHFB) fails to inherit the documentation for this method

            if (value.IsNull)
            {
                IndexedProperties[index] = null;
            }
            else if (value.TryGet(out object tempValue))
            {
                IndexedProperties[index] = tempValue;
            }
            else
            {
                throw new ArgumentException("Invalid property value", nameof(value));
            }
        }

        /// <inheritdoc/>
        public bool HasDynamicProperty(string name)
        {
            return namedProperties?.ContainsKey(name) == true;
        }

        /// <inheritdoc/>
        public bool HasDynamicProperty(int index)
        {
            return indexedProperties?.ContainsKey(index) == true;
        }

        /// <inheritdoc/>
        public void DeleteDynamicProperty(string name)
        {
            namedProperties?.Remove(name);
        }

        /// <inheritdoc/>
        public void DeleteDynamicProperty(int index)
        {
            indexedProperties?.Remove(index);
        }

        #endregion
    }
}
