// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a script object.
    /// </summary>
    public interface IScriptObject : IDisposable
    {
        /// <summary>
        /// Gets the script engine that owns the object.
        /// </summary>
        ScriptEngine Engine { get; }

        /// <summary>
        /// Gets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="args">Optional arguments for property retrieval.</param>
        /// <returns>The value of the specified property.</returns>
        object GetProperty(string name, params object[] args);

        /// <summary>
        /// Sets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="args">An array containing optional arguments and the new property value.</param>
        /// <remarks>
        /// The <paramref name="args"></paramref> array must contain at least one element. The new
        /// property value must be the last element of the array.
        /// </remarks>
        void SetProperty(string name, params object[] args);

        /// <summary>
        /// Removes a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <returns><c>True</c> if the property was removed, <c>false</c> otherwise.</returns>
        bool DeleteProperty(string name);

        /// <summary>
        /// Enumerates the script object's property names.
        /// </summary>
        IEnumerable<string> PropertyNames { get; }

        /// <summary>
        /// Gets or sets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to get or set.</param>
        /// <param name="args">Optional arguments for property access.</param>
        /// <returns>The value of the specified property.</returns>
        object this[string name, params object[] args] { get; set; }

        /// <summary>
        /// Gets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        /// <returns>The value of the specified property.</returns>
        object GetProperty(int index);

        /// <summary>
        /// Sets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to set.</param>
        /// <param name="value">The new property value.</param>
        void SetProperty(int index, object value);

        /// <summary>
        /// Removes an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to remove.</param>
        /// <returns><c>True</c> if the property was removed, <c>false</c> otherwise.</returns>
        bool DeleteProperty(int index);

        /// <summary>
        /// Enumerates the script object's property indices.
        /// </summary>
        IEnumerable<int> PropertyIndices { get; }

        /// <summary>
        /// Gets or sets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to get or set.</param>
        /// <returns>The value of the specified property.</returns>
        object this[int index] { get; set; }

        /// <summary>
        /// Invokes the script object.
        /// </summary>
        /// <param name="asConstructor"><c>True</c> to invoke the object as a constructor, <c>false</c> otherwise.</param>
        /// <param name="args">Optional arguments for object invocation.</param>
        /// <returns>The invocation result value.</returns>
        object Invoke(bool asConstructor, params object[] args);

        /// <summary>
        /// Invokes a script object method.
        /// </summary>
        /// <param name="name">The name of the method to invoke.</param>
        /// <param name="args">Optional arguments for method invocation.</param>
        /// <returns>The invocation result value.</returns>
        object InvokeMethod(string name, params object[] args);

        /// <summary>
        /// Invokes the script object as a function.
        /// </summary>
        /// <param name="args">Optional arguments for object invocation.</param>
        /// <returns>The invocation result value.</returns>
        object InvokeAsFunction(params object[] args);
    }
}
