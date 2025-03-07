// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostFunction"/></c> with a private configuration.
    /// </summary>
    public sealed class V8FastHostFunction : V8FastHostFunctionOperations<V8FastHostFunction>, IV8FastHostFunction
    {
        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance.
        /// </summary>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(V8FastHostFunctionInvoker invoker)
            : this(0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified name.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(string name, V8FastHostFunctionInvoker invoker)
            : this(name, 0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified required argument count.
        /// </summary>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(int requiredArgCount, V8FastHostFunctionInvoker invoker)
            : this(requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified configuration callback.
        /// </summary>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(V8FastHostObjectConfigurator<V8FastHostFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified name and required argument count.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(string name, int requiredArgCount, V8FastHostFunctionInvoker invoker)
            : this(name, requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified name and configuration callback.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(string name, V8FastHostObjectConfigurator<V8FastHostFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(name, 0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified required argument count and configuration callback.
        /// </summary>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(int requiredArgCount, V8FastHostObjectConfigurator<V8FastHostFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(null, requiredArgCount, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunction"/></c> instance with the specified name, required argument count, and configuration callback.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunction(string name, int requiredArgCount, V8FastHostObjectConfigurator<V8FastHostFunction> configurator, V8FastHostFunctionInvoker invoker)
            : base(name, requiredArgCount, configurator, invoker)
        {
        }

        /// <summary>
        /// Verifies that a fast host function was invoked as a function and not as a constructor.
        /// </summary>
        /// <param name="asConstructor"><c>True</c> if the function was invoked as a constructor, <c>false</c> otherwise.</param>
        /// <remarks>
        /// This is a simple utility method that checks <paramref name="asConstructor"/> and throws
        /// an exception if it is <c>true</c>. It is intended for use within fast host function
        /// invocation callbacks.
        /// </remarks>
        public static void VerifyFunctionCall(bool asConstructor)
        {
            if (asConstructor)
            {
                throw new NotSupportedException("The function does not support constructor invocation");
            }
        }

        /// <summary>
        /// Verifies that a fast host function was invoked as a constructor and not as a function.
        /// </summary>
        /// <param name="asConstructor"><c>True</c> if the function was invoked as a constructor, <c>false</c> otherwise.</param>
        /// <remarks>
        /// This is a simple utility method that checks <paramref name="asConstructor"/> and throws
        /// an exception if it is <c>false</c>. It is intended for use within fast host function
        /// invocation callbacks.
        /// </remarks>
        public static void VerifyConstructorCall(bool asConstructor)
        {
            if (!asConstructor)
            {
                throw new NotSupportedException("The function requires constructor invocation");
            }
        }

        #region IV8FastHostFunction implementation

        IV8FastHostFunctionOperations IV8FastHostFunction.Operations => this;

        #endregion

        #region IV8FastHostObject implementation

        IV8FastHostObjectOperations IV8FastHostObject.Operations => this;

        #endregion
    }
}
