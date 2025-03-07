// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a set of dynamic properties keyed by name or index.
    /// </summary>
    public interface IV8FastHostDynamicProperties
    {
        /// <summary>
        /// Enumerates the names of all existing named dynamic properties.
        /// </summary>
        IEnumerable<string> DynamicPropertyNames { get; }

        /// <summary>
        /// Enumerates the indices of all existing indexed dynamic properties.
        /// </summary>
        IEnumerable<int> DynamicPropertyIndices { get; }

        /// <summary>
        /// Gets the value of a named dynamic property.
        /// </summary>
        /// <param name="name">The name of the dynamic property to get.</param>
        /// <param name="value">On return, the value of the dynamic property if it was found.</param>
        void GetDynamicProperty(string name, in V8FastResult value);

        /// <summary>
        /// Sets the value of a named dynamic property.
        /// </summary>
        /// <param name="name">The name of the dynamic property to set.</param>
        /// <param name="value">The property value.</param>
        void SetDynamicProperty(string name, in V8FastArg value);

        /// <summary>
        /// Gets the value of an indexed dynamic property.
        /// </summary>
        /// <param name="index">The index of the dynamic property to get.</param>
        /// <param name="value">On return, the value of the dynamic property if it was found.</param>
        void GetDynamicProperty(int index, in V8FastResult value);

        /// <summary>
        /// Sets the value of an indexed dynamic property.
        /// </summary>
        /// <param name="index">The index of the dynamic property to set.</param>
        /// <param name="value">The property value.</param>
        void SetDynamicProperty(int index, in V8FastArg value);

        /// <summary>
        /// Determines whether a dynamic property with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the dynamic property for which to search.</param>
        /// <returns><c>True</c> if the dynamic property exists, <c>false</c> otherwise.</returns>
        bool HasDynamicProperty(string name);

        /// <summary>
        /// Determines whether a dynamic property exists at the specified index.
        /// </summary>
        /// <param name="index">The index of the dynamic property for which to search.</param>
        /// <returns><c>True</c> if the dynamic property exists, <c>false</c> otherwise.</returns>
        bool HasDynamicProperty(int index);

        /// <summary>
        /// Deletes a named dynamic property.
        /// </summary>
        /// <param name="name">The name of the dynamic property to delete.</param>
        void DeleteDynamicProperty(string name);

        /// <summary>
        /// Deletes an indexed dynamic property.
        /// </summary>
        /// <param name="index">The index of the dynamic property to delete.</param>
        void DeleteDynamicProperty(int index);
    }
}
