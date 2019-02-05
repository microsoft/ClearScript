// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a scriptable collection of named properties.
    /// </summary>
    /// <remarks>
    /// If an object that implements this interface is added to a script engine (see
    /// <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>), script code
    /// will be able to access the properties stored in the collection as if they were members of
    /// the object itself, using the script language's native syntax for member access. No
    /// other members of the object will be accessible. This interface also allows objects to
    /// implement dynamic properties for script languages that support them.
    /// </remarks>
    public interface IPropertyBag : IDictionary<string, object>
    {
    }

    /// <summary>
    /// Provides a default <see cref="IPropertyBag"/> implementation.
    /// </summary>
    public class PropertyBag : IPropertyBag, INotifyPropertyChanged, IScriptableObject
    {
        #region data

        private readonly Dictionary<string, object> dictionary;
        private readonly ICollection<KeyValuePair<string, object>> collection;
        private readonly bool isReadOnly;
        private readonly ConcurrentWeakSet<ScriptEngine> engineSet = new ConcurrentWeakSet<ScriptEngine>();

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new writable <see cref="PropertyBag"/>.
        /// </summary>
        public PropertyBag()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="PropertyBag"/>.
        /// </summary>
        /// <param name="isReadOnly"><c>True</c> to make the <see cref="PropertyBag"/> read-only, <c>false</c> to make it writable.</param>
        /// <remarks>
        /// The host can modify a read-only <see cref="PropertyBag"/> by calling
        /// <see cref="SetPropertyNoCheck">SetPropertyNoCheck</see>,
        /// <see cref="RemovePropertyNoCheck">RemovePropertyNoCheck</see>, or
        /// <see cref="ClearNoCheck">ClearNoCheck</see>.
        /// </remarks>
        public PropertyBag(bool isReadOnly)
        {
            dictionary = new Dictionary<string, object>();
            collection = dictionary;
            this.isReadOnly = isReadOnly;
        }

        #endregion

        #region public members

        /// <summary>
        /// Sets a property value without checking whether the <see cref="PropertyBag"/> is read-only.
        /// </summary>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The property value.</param>
        /// <remarks>
        /// This operation is never exposed to script code.
        /// </remarks>
        public void SetPropertyNoCheck(string name, object value)
        {
            dictionary[name] = value;
            NotifyPropertyChanged(name);
            NotifyExposedToScriptCode(value);
        }

        /// <summary>
        /// Removes a property without checking whether the <see cref="PropertyBag"/> is read-only.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <returns><c>True</c> if the property was found and removed, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This operation is never exposed to script code.
        /// </remarks>
        public bool RemovePropertyNoCheck(string name)
        {
            if (dictionary.Remove(name))
            {
                NotifyPropertyChanged(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all properties without checking whether the <see cref="PropertyBag"/> is read-only.
        /// </summary>
        /// <remarks>
        /// This operation is never exposed to script code.
        /// </remarks>
        public void ClearNoCheck()
        {
            dictionary.Clear();
            NotifyPropertyChanged(null);
        }

        #endregion

        #region internal members

        internal int EngineCount
        {
            get { return engineSet.Count; }
        }

        private void CheckReadOnly()
        {
            if (isReadOnly)
            {
                throw new UnauthorizedAccessException("Object is read-only");
            }
        }

        private void AddPropertyNoCheck(string name, object value)
        {
            dictionary.Add(name, value);
            NotifyPropertyChanged(name);
            NotifyExposedToScriptCode(value);
        }

        private void NotifyPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void NotifyExposedToScriptCode(object value)
        {
            var scriptableObject = value as IScriptableObject;
            if (scriptableObject != null)
            {
                engineSet.ForEach(scriptableObject.OnExposedToScriptCode);
            }
        }

        #endregion

        #region IEnumerable implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation to resolve ambiguity.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, object>> implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation to resolve ambiguity.")]
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion

        #region ICollection<KeyValuePair<string, object>> implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            CheckReadOnly();
            SetPropertyNoCheck(item.Key, item.Value);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            CheckReadOnly();
            ClearNoCheck();
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return collection.Contains(item);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            collection.CopyTo(array, arrayIndex);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            CheckReadOnly();
            if (collection.Remove(item))
            {
                NotifyPropertyChanged(item.Key);
                return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return collection.Count; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return isReadOnly; }
        }

        #endregion

        #region IDictionary<string, object> implementation

        /// <summary>
        /// Determines whether the <see cref="PropertyBag"/> contains a property with the specified name.
        /// </summary>
        /// <param name="key">The name of the property to locate.</param>
        /// <returns><c>True</c> if the <see cref="PropertyBag"/> contains a property with the specified name, <c>false</c> otherwise.</returns>
        public bool ContainsKey(string key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Adds a property to the <see cref="PropertyBag"/>.
        /// </summary>
        /// <param name="key">The name of the property to add.</param>
        /// <param name="value">The property value.</param>
        public void Add(string key, object value)
        {
            CheckReadOnly();
            AddPropertyNoCheck(key, value);
        }

        /// <summary>
        /// Removes a property from the <see cref="PropertyBag"/>.
        /// </summary>
        /// <param name="key">The name of the property to remove.</param>
        /// <returns><c>True</c> if the property was successfully found and removed, <c>false</c> otherwise.</returns>
        public bool Remove(string key)
        {
            CheckReadOnly();
            return RemovePropertyNoCheck(key);
        }

        /// <summary>
        /// Looks up a property value in the <see cref="PropertyBag"/>.
        /// </summary>
        /// <param name="key">The name of the property to locate.</param>
        /// <param name="value">The property value if the property was found, <c>null</c> otherwise.</param>
        /// <returns><c>True</c> if the property was found, <c>false</c> otherwise.</returns>
        public bool TryGetValue(string key, out object value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or sets a property value in the <see cref="PropertyBag"/>.
        /// </summary>
        /// <param name="key">The name of the property to get or set.</param>
        /// <returns>The property value.</returns>
        public object this[string key]
        {
            get { return dictionary[key]; }

            set
            {
                CheckReadOnly();
                SetPropertyNoCheck(key, value);
            }
        }

        /// <summary>
        /// Gets a collection of property names from the <see cref="PropertyBag"/>.
        /// </summary>
        public ICollection<string> Keys
        {
            get { return dictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection of property values from the <see cref="PropertyBag"/>.
        /// </summary>
        public ICollection<object> Values
        {
            get { return dictionary.Values; }
        }

        #endregion

        #region INotifyPropertyChanged implementation

        /// <summary>
        /// Occurs when a property is added or replaced, or when the collection is cleared.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IScriptableObject implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member is not expected to be re-implemented in derived classes.")]
        void IScriptableObject.OnExposedToScriptCode(ScriptEngine engine)
        {
            if ((engine != null) && engineSet.TryAdd(engine))
            {
                foreach (var scriptableObject in Values.OfType<IScriptableObject>())
                {
                    scriptableObject.OnExposedToScriptCode(engine);
                }
            }
        }

        #endregion
    }
}
