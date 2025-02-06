using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    /// <summary>
    /// Implement this interface to talk to ClearScript's native wrapper around V8 directly.
    /// </summary>
    public interface IV8HostObject
    {
        /// <summary>
        /// Implement this to support getting the values of named properties.
        /// </summary>
        /// <param name="name">The name of the property JavaScript wants the value of.</param>
        /// <param name="value">Write the value of the property here.</param>
        /// <param name="isConst">If you set this to to true, V8 will cache the value not ask for it
        /// again.</param>
        void GetNamedProperty(StdString name, V8Value value, out bool isConst) =>
            throw new NotImplementedException($"Named property {name.ToString()} is not implemented");

        /// <summary>
        /// Implement this to support setting the values of named properties.
        /// </summary>
        /// <param name="name">The name of the property JavaScript wants to set.</param>
        /// <param name="value">The value JavaScript wants to set the property to.</param>
        void SetNamedProperty(StdString name, V8Value.Decoded value) =>
            throw new NotImplementedException($"Named property {name.ToString()} is not implemented");

        /// <summary>
        /// Implement this to support deleting named properties.
        /// </summary>
        /// <param name="name">The name of the property JavaScript wants to delete.</param>
        /// <returns>TODO</returns>
        bool DeleteNamedProperty(StdString name) =>
            throw new NotImplementedException($"Named property {name.ToString()} is not implemented");

        /// <summary>
        /// Implement this to support getting the values of indexed properties.
        /// </summary>
        /// <param name="index">The index of the property JavaScript wants the value of.</param>
        /// <param name="value">Write the value of the property here.</param>
        void GetIndexedProperty(int index, V8Value value) =>
            throw new NotImplementedException($"Indexed property {index} is not implemented");

        /// <summary>
        /// Implement this to support setting the values of indexed properties.
        /// </summary>
        /// <param name="index">The index of the property JavaScript wants to set.</param>
        /// <param name="value">The value JavaScript wants to set the property to.</param>
        void SetIndexedProperty(int index, V8Value.Decoded value) =>
            throw new NotImplementedException($"Indexed property {index} is not implemented");

        /// <summary>
        /// Implement this to support deleting indexed properties.
        /// </summary>
        /// <param name="index">The index of the property JavaScript wants to delete.</param>
        /// <returns>TODO</returns>
        bool DeleteIndexedProperty(int index) =>
            throw new NotImplementedException($"Indexed property {index} is not implemented");

        /// <summary>
        /// Implement this to support being enumerated.
        /// </summary>
        /// <param name="result">Write the enumerator here.</param>
        /// <remarks>
        /// The enumerator class should implement <see cref="IV8HostObject"/> and implement MoveNext(),
        /// ScriptableDispose(), and CurrentValue.
        /// </remarks>
        void GetEnumerator(V8Value result) =>
            throw new NotImplementedException("Enumerator is not implemented");

        /// <summary>
        /// Implement this to support being async enumerated.
        /// </summary>
        /// <param name="result">Write the async enumerator here.</param>
        void GetAsyncEnumerator(V8Value result) =>
            throw new NotImplementedException("Async enumerator is not implemented");

        /// <summary>
        /// Implement this to support listing of all your named proeprties.
        /// </summary>
        /// <param name="names">Write the names of your properties here.</param>
        void GetNamedPropertyNames(StdStringArray names) =>
            throw new NotImplementedException("Listing named properties is not implemented");

        /// <summary>
        /// Implement this to support listing all your indexed properties.
        /// </summary>
        /// <param name="indices">Write the indices of your properties here.</param>
        void GetIndexedPropertyIndices(StdInt32Array indices) =>
            throw new NotImplementedException("Listing indexed properties is not implemented");

        /// <summary>
        /// I don't know when ClearScript calls this.
        /// </summary>
        /// <param name="name">The name of the method JavaScript wants to invoke.</param>
        /// <param name="args">The arguments JavaScript is passing to the method.</param>
        /// <param name="result">Write the return value, if not <see cref="void"/>, of the method here.
        /// </param>
        void InvokeMethod(StdString name, ReadOnlySpan<V8Value.Decoded> args, V8Value result)
        {
            GetNamedProperty(name, result, out _);
            object method = result.Decode().GetHostObject();
            result.SetNonexistent();
            ((InvokeHostObject)method)(args, result);
        }
    }

    /// <summary>
    /// Return a delegate of this type from a property to tell JavaScript that it is a callable
    /// function.
    /// </summary>
    /// <param name="args">The arguments JavaScript will pass to your method when invoking it.</param>
    /// <param name="result">Write the return value, if not <see cref="void"/>, of the method here.
    /// </param>
    public delegate void InvokeHostObject(ReadOnlySpan<V8Value.Decoded> args, V8Value result);
}
