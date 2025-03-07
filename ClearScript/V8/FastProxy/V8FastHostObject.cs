// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Provides an implementation of <c><see cref="IV8FastHostObject"/></c> with a shared configuration.
    /// </summary>
    /// <typeparam name="TObject">The fast host object type.</typeparam>
    /// <remarks>
    /// This class is a generic base for fast host object classes derived from it via the
    /// <see href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">Curiously Recurring Template Pattern (CRTP)</see>.
    /// It uses a shared instance of <c><see cref="V8FastHostObjectConfiguration{TObject}"/></c>
    /// for each unique type argument. Derived classes are therefore advised to use a static
    /// constructor to invoke the <c><see cref="Configure"/></c> method.
    /// </remarks>
    public abstract class V8FastHostObject<TObject> : IV8FastHostObject where TObject : V8FastHostObject<TObject>
    {
        private static readonly V8FastHostObjectOperations<TObject> operations = new();

        /// <summary>
        /// Initializes a new <c><see cref="V8FastHostObject{TObject}"/></c> instance.
        /// </summary>
        protected V8FastHostObject()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        /// <summary>
        /// Prepares the shared configuration associated with <typeparamref name="TObject"/>.
        /// </summary>
        /// <param name="configurator">The configuration callback.</param>
        /// <remarks>
        /// Derived classes are advised to invoke this method from a static constructor. Once
        /// prepared, a fast host object configuration cannot be modified.
        /// </remarks>
        protected static void Configure(V8FastHostObjectConfigurator<TObject> configurator) => operations.Configure(configurator);

        #region IV8FastHostObject implementation

        IV8FastHostObjectOperations IV8FastHostObject.Operations => operations;

        #endregion
    }
}
