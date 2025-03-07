// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents the operations supported by a fast host object.
    /// </summary>
    public interface IV8FastHostObjectOperations
    {
        /// <summary>
        /// Gets a human-readable name for the specified object.
        /// </summary>
        /// <param name="instance">The object for which to get a human-readable name.</param>
        /// <returns>A human-readable name for the specified object.</returns>
        string GetFriendlyName(IV8FastHostObject instance);

        /// <summary>
        /// Gets the value of a named property.
        /// </summary>
        /// <param name="instance">The object to search for the property.</param>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="value">On return, the value of the property if it was found.</param>
        /// <param name="isCacheable">On return, <c>true</c> if the property value is a cacheable constant, <c>false</c> otherwise.</param>
        void GetProperty(IV8FastHostObject instance, string name, in V8FastResult value, out bool isCacheable);

        /// <summary>
        /// Sets the value of a named property.
        /// </summary>
        /// <param name="instance">The object whose property to set.</param>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="value">The property value.</param>
        void SetProperty(IV8FastHostObject instance, string name, in V8FastArg value);

        /// <summary>
        /// Gets the attributes of a named property.
        /// </summary>
        /// <param name="instance">The object to search for the property.</param>
        /// <param name="name">The name of the property to query.</param>
        /// <returns>A set of property attributes.</returns>
        V8FastHostPropertyFlags QueryProperty(IV8FastHostObject instance, string name);

        /// <summary>
        /// Deletes a named property.
        /// </summary>
        /// <param name="instance">The object whose property to delete.</param>
        /// <param name="name">The name of the property to delete.</param>
        /// <returns><c>True</c> if the property was deleted or does not exist; <c>false</c> if the property exists but is not deletable.</returns>
        bool DeleteProperty(IV8FastHostObject instance, string name);

        /// <summary>
        /// Enumerates the names of all existing named properties.
        /// </summary>
        /// <param name="instance">The object to search for named properties.</param>
        /// <returns>A collection containing the names of all existing named properties.</returns>
        IEnumerable<string> GetPropertyNames(IV8FastHostObject instance);

        /// <summary>
        /// Gets the value of an indexed property.
        /// </summary>
        /// <param name="instance">The object to search for the property.</param>
        /// <param name="index">The index of the property to get.</param>
        /// <param name="value">On return, the value of the property if it was found.</param>
        void GetProperty(IV8FastHostObject instance, int index, in V8FastResult value);

        /// <summary>
        /// Sets the value of an indexed property.
        /// </summary>
        /// <param name="instance">The object whose property to set.</param>
        /// <param name="index">The index of the property to set.</param>
        /// <param name="value">The property value.</param>
        void SetProperty(IV8FastHostObject instance, int index, in V8FastArg value);

        /// <summary>
        /// Gets the attributes of an indexed property.
        /// </summary>
        /// <param name="instance">The object to search for the property.</param>
        /// <param name="index">The index of the property to query.</param>
        /// <returns>A set of property attributes.</returns>
        V8FastHostPropertyFlags QueryProperty(IV8FastHostObject instance, int index);

        /// <summary>
        /// Deletes an indexed property.
        /// </summary>
        /// <param name="instance">The object whose property to delete.</param>
        /// <param name="index">The index of the property to delete.</param>
        /// <returns><c>True</c> if the property was deleted or does not exist; <c>false</c> if the property exists but is not deletable.</returns>
        bool DeleteProperty(IV8FastHostObject instance, int index);

        /// <summary>
        /// Enumerates the indices of all existing indexed properties.
        /// </summary>
        /// <param name="instance">The object to search for indexed properties.</param>
        /// <returns>A collection containing the indices of all existing indexed properties.</returns>
        IEnumerable<int> GetPropertyIndices(IV8FastHostObject instance);

        /// <summary>
        /// Creates a fast enumerator that iterates through the object's contents.
        /// </summary>
        /// <param name="instance">The object for which to create a fast enumerator.</param>
        /// <returns>A fast enumerator for the specified object's contents.</returns>
        IV8FastEnumerator CreateEnumerator(IV8FastHostObject instance);

        /// <summary>
        /// Creates a fast asynchronous enumerator that iterates through the object's contents.
        /// </summary>
        /// <param name="instance">The object for which to create a fast asynchronous enumerator.</param>
        /// <returns>A fast asynchronous enumerator for the specified object's contents.</returns>
        IV8FastAsyncEnumerator CreateAsyncEnumerator(IV8FastHostObject instance);
    }
}
