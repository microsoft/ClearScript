// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostMethod"/></c> with a private configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type that supports the method.</typeparam>
    public sealed class V8FastHostMethod<TObject> : V8FastHostMethodOperations<TObject, V8FastHostMethod<TObject>>, IV8FastHostMethod where TObject : IV8FastHostObject
    {
        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethod{TObject}"/></c> instance.
        /// </summary>
        /// <param name="parent">The object whose method the instance represents.</param>
        /// <param name="name">The method name.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethod(TObject parent, string name, V8FastHostMethodInvoker<TObject> invoker)
            : this(parent, name, 0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethod{TObject}"/></c> instance with the specified required argument count.
        /// </summary>
        /// <param name="parent">The object whose method the instance represents.</param>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethod(TObject parent, string name, int requiredArgCount, V8FastHostMethodInvoker<TObject> invoker)
            : this(parent, name, requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethod{TObject}"/></c> instance with the specified configuration callback.
        /// </summary>
        /// <param name="parent">The object whose method the instance represents.</param>
        /// <param name="name">The method name.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethod(TObject parent, string name, V8FastHostObjectConfigurator<V8FastHostMethod<TObject>> configurator, V8FastHostMethodInvoker<TObject> invoker)
            : this(parent, name, 0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethod{TObject}"/></c> instance with the specified required argument count and configuration callback.
        /// </summary>
        /// <param name="parent">The object whose method the instance represents.</param>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethod(TObject parent, string name, int requiredArgCount, V8FastHostObjectConfigurator<V8FastHostMethod<TObject>> configurator, V8FastHostMethodInvoker<TObject> invoker)
            : base(parent, name, requiredArgCount, configurator, invoker)
        {
        }

        #region IV8FastHostMethod implementation

        IV8FastHostMethodOperations IV8FastHostMethod.Operations => this;

        #endregion

        #region IV8FastHostObject implementation

        IV8FastHostObjectOperations IV8FastHostObject.Operations => this;

        #endregion
    }
}
