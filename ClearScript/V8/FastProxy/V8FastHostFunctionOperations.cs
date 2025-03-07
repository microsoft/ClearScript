// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostFunctionOperations"/></c> based on a private configuration.
    /// </summary>
    /// <typeparam name="TFunction">The fast host function type.</typeparam>
    public class V8FastHostFunctionOperations<TFunction> : V8FastHostObjectOperations<TFunction>, IV8FastHostFunctionOperations where TFunction : IV8FastHostFunction
    {
        private readonly string name;
        private readonly int requiredArgCount;
        private readonly V8FastHostFunctionInvoker invoker;

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance.
        /// </summary>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(V8FastHostFunctionInvoker invoker)
            : this(0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified name.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(string name, V8FastHostFunctionInvoker invoker)
            : this(name, 0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified required argument count.
        /// </summary>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(int requiredArgCount, V8FastHostFunctionInvoker invoker)
            : this(requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified configuration callback.
        /// </summary>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(V8FastHostObjectConfigurator<TFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified name and required argument count.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(string name, int requiredArgCount, V8FastHostFunctionInvoker invoker)
            : this(name, requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified name and configuration callback.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(string name, V8FastHostObjectConfigurator<TFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(name, 0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified required argument count and configuration callback.
        /// </summary>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(int requiredArgCount, V8FastHostObjectConfigurator<TFunction> configurator, V8FastHostFunctionInvoker invoker)
            : this(null, requiredArgCount, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostFunctionOperations{TFunction}"/></c> instance with the specified name, required argument count, and configuration callback.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="requiredArgCount">The required argument count for the function.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The function invocation callback.</param>
        public V8FastHostFunctionOperations(string name, int requiredArgCount, V8FastHostObjectConfigurator<TFunction> configurator, V8FastHostFunctionInvoker invoker)
            : base(configurator)
        {
            MiscHelpers.VerifyNonNullArgument(invoker, nameof(invoker));

            this.name = name;
            this.requiredArgCount = Math.Max(requiredArgCount, 0);
            this.invoker = invoker;
        }

        #region IV8FastHostFunctionOperations implementation

        void IV8FastHostFunctionOperations.Invoke(bool asConstructor, in V8FastArgs args, in V8FastResult result)
        {
            if (args.Count < requiredArgCount)
            {
                throw new ArgumentException($"Too few arguments specified for function '{name.ToNonNull("[unnamed]")}'");
            }

            invoker(asConstructor, args, result);
        }

        #endregion

        #region IV8FastHostObjectOperations implementation

        string IV8FastHostObjectOperations.GetFriendlyName(IV8FastHostObject instance) => $"FastHostFunction:{name.ToNonNull("[unnamed]")}";

        #endregion
    }
}
