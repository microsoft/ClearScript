// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a method that prepares a fast host object configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <param name="configuration">The configuration associated with <typeparamref name="TObject"/>.</param>
    public delegate void V8FastHostObjectConfigurator<TObject>(V8FastHostObjectConfiguration<TObject> configuration) where TObject : IV8FastHostObject;

    /// <summary>
    /// Represents a method that provides access to a field within a fast host object.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <typeparam name="TField">The field type.</typeparam>
    /// <param name="instance">The object whose field to access.</param>
    /// <returns>A reference to the field.</returns>
    public delegate ref TField V8FastHostFieldAccessor<in TObject, TField>(TObject instance) where TObject : IV8FastHostObject;

    /// <summary>
    /// Represents a method that gets the value of a fast host property.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <param name="instance">The object to search for the property.</param>
    /// <param name="value">On return, the property value if it was found.</param>
    public delegate void V8FastHostPropertyGetter<in TObject>(TObject instance, in V8FastResult value) where TObject : IV8FastHostObject;

    /// <summary>
    /// Represents a method that sets the value of a fast host property.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <param name="instance">The object whose property to set.</param>
    /// <param name="value">The property value.</param>
    public delegate void V8FastHostPropertySetter<in TObject>(TObject instance, in V8FastArg value) where TObject : IV8FastHostObject;

    /// <summary>
    /// Represents a method that invokes a fast host method.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <param name="instance">The object whose method to invoke.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <param name="result">On return, the method's return value.</param>
    public delegate void V8FastHostMethodInvoker<in TObject>(TObject instance, in V8FastArgs args, in V8FastResult result) where TObject : IV8FastHostObject;

    /// <summary>
    /// Represents a method that invokes a fast host function.
    /// </summary>
    /// <param name="asConstructor"><c>True</c> to invoke the function as a constructor, <c>false</c> otherwise.</param>
    /// <param name="args">The arguments to pass to the function.</param>
    /// <param name="result">On return, the function's return value.</param>
    public delegate void V8FastHostFunctionInvoker(bool asConstructor, in V8FastArgs args, in V8FastResult result);
}
