// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostMethodOperations"/></c> based on a private configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type that supports the method.</typeparam>
    /// <typeparam name="TMethod">The fast host method type.</typeparam>
    public class V8FastHostMethodOperations<TObject, TMethod> : V8FastHostObjectOperations<TMethod>, IV8FastHostMethodOperations where TObject : IV8FastHostObject where TMethod : IV8FastHostMethod
    {
        private readonly TObject target;
        private readonly string name;
        private readonly int requiredArgCount;
        private readonly V8FastHostMethodInvoker<TObject> invoker;

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethodOperations{TObject, TMethod}"/></c> instance.
        /// </summary>
        /// <param name="target">The object whose method the instance supports.</param>
        /// <param name="name">The method name.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethodOperations(TObject target, string name, V8FastHostMethodInvoker<TObject> invoker)
            : this(target, name, 0, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethodOperations{TObject, TMethod}"/></c> instance with the specified required argument count.
        /// </summary>
        /// <param name="target">The object whose method the instance supports.</param>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethodOperations(TObject target, string name, int requiredArgCount, V8FastHostMethodInvoker<TObject> invoker)
            : this(target, name, requiredArgCount, null, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethodOperations{TObject, TMethod}"/></c> instance with the specified configuration callback.
        /// </summary>
        /// <param name="target">The object whose method the instance supports.</param>
        /// <param name="name">The method name.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethodOperations(TObject target, string name, V8FastHostObjectConfigurator<TMethod> configurator, V8FastHostMethodInvoker<TObject> invoker)
            : this(target, name, 0, configurator, invoker)
        {
        }

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostMethodOperations{TObject, TMethod}"/></c> instance with the specified required argument count and configuration callback.
        /// </summary>
        /// <param name="target">The object whose method the instance supports.</param>
        /// <param name="name">The method name.</param>
        /// <param name="requiredArgCount">The required argument count for the method.</param>
        /// <param name="configurator">A callback for preparing the configuration associated with the instance.</param>
        /// <param name="invoker">The method invocation callback.</param>
        public V8FastHostMethodOperations(TObject target, string name, int requiredArgCount, V8FastHostObjectConfigurator<TMethod> configurator, V8FastHostMethodInvoker<TObject> invoker)
            : base(configurator)
        {
            MiscHelpers.VerifyNonNullArgument(name, nameof(name));
            MiscHelpers.VerifyNonNullArgument(invoker, nameof(invoker));

            this.target = target;
            this.name = name;
            this.requiredArgCount = Math.Max(requiredArgCount, 0);
            this.invoker = invoker;
        }

        #region IV8FastHostMethodOperations implementation

        void IV8FastHostMethodOperations.Invoke(in V8FastArgs args, in V8FastResult result)
        {
            if (args.Count < requiredArgCount)
            {
                throw new ArgumentException($"Too few arguments specified for method '{name}'");
            }

            invoker(target, args, result);
        }

        #endregion

        #region IV8FastHostObjectOperations implementation

        string IV8FastHostObjectOperations.GetFriendlyName(IV8FastHostObject instance) => $"FastHostMethod:{name}";

        #endregion
    }
}
