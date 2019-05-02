using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a script object.
    /// </summary>
    /// <seealso cref="ScriptEngine.Evaluate(string, bool, string)"/>
    public abstract class ScriptObject : DynamicObject
    {
        internal ScriptObject()
        {
        }

        /// <summary>
        /// Gets the script engine that owns the object.
        /// </summary>
        public abstract ScriptEngine Engine { get; }

        /// <summary>
        /// Gets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to get.</param>
        /// <param name="args">Optional arguments for property retrieval.</param>
        /// <returns>The value of the specified property.</returns>
        public abstract object GetProperty(string name, params object[] args);

        /// <summary>
        /// Sets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to set.</param>
        /// <param name="args">An array containing optional arguments and the new property value.</param>
        /// <remarks>
        /// The <paramref name="args"></paramref> array must contain at least one element. The new
        /// property value must be the last element of the array.
        /// </remarks>
        public abstract void SetProperty(string name, params object[] args);

        /// <summary>
        /// Removes a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to remove.</param>
        /// <returns><c>True</c> if the property was removed successfully, <c>false</c> otherwise.</returns>
        public abstract bool DeleteProperty(string name);

        /// <summary>
        /// Enumerates the script object's property names.
        /// </summary>
        public abstract IEnumerable<string> PropertyNames { get; }

        /// <summary>
        /// Gets or sets the value of a named script object property.
        /// </summary>
        /// <param name="name">The name of the property to get or set.</param>
        /// <param name="args">Optional arguments for property access.</param>
        /// <returns>The value of the specified property.</returns>
        public object this[string name, params object[] args]
        {
            get { return GetProperty(name, args); }
            set { SetProperty(name, args.Concat(value.ToEnumerable()).ToArray()); }
        }

        /// <summary>
        /// Gets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        /// <returns>The value of the specified property.</returns>
        public abstract object GetProperty(int index);

        /// <summary>
        /// Sets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to set.</param>
        /// <param name="value">The new property value.</param>
        public abstract void SetProperty(int index, object value);

        /// <summary>
        /// Removes an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to remove.</param>
        /// <returns><c>True</c> if the property was removed successfully, <c>false</c> otherwise.</returns>
        public abstract bool DeleteProperty(int index);

        /// <summary>
        /// Enumerates the script object's property indices.
        /// </summary>
        public abstract IEnumerable<int> PropertyIndices { get; }

        /// <summary>
        /// Gets or sets the value of an indexed script object property.
        /// </summary>
        /// <param name="index">The index of the property to get or set.</param>
        /// <returns>The value of the specified property.</returns>
        public object this[int index]
        {
            get { return GetProperty(index); }
            set { SetProperty(index, value); }
        }

        /// <summary>
        /// Invokes the script object.
        /// </summary>
        /// <param name="asConstructor"><c>True</c> to invoke the object as a constructor, <c>false</c> otherwise.</param>
        /// <param name="args">Optional arguments for object invocation.</param>
        /// <returns>The invocation result value.</returns>
        public abstract object Invoke(bool asConstructor, params object[] args);

        /// <summary>
        /// Invokes a script object method.
        /// </summary>
        /// <param name="name">The name of the method to invoke.</param>
        /// <param name="args">Optional arguments for method invocation.</param>
        /// <returns>The invocation result value.</returns>
        public abstract object InvokeMethod(string name, params object[] args);
    }
}
