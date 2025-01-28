// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Dynamic;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides the base implementation for all script objects.
    /// </summary>
    public abstract class ScriptObject : DynamicObject, IScriptObject
    {
        internal ScriptObject()
        {
        }

        #region IScriptObject implementation

        /// <inheritdoc/>
        public abstract ScriptEngine Engine { get; }

        /// <inheritdoc/>
        public abstract object GetProperty(string name, params object[] args);

        /// <inheritdoc/>
        public abstract void SetProperty(string name, params object[] args);

        /// <inheritdoc/>
        public abstract bool DeleteProperty(string name);

        /// <inheritdoc/>
        public abstract IEnumerable<string> PropertyNames { get; }

        /// <inheritdoc/>
        public abstract object this[string name, params object[] args] { get; set; }

        /// <inheritdoc/>
        public abstract object GetProperty(int index);

        /// <inheritdoc/>
        public abstract void SetProperty(int index, object value);

        /// <inheritdoc/>
        public abstract bool DeleteProperty(int index);

        /// <inheritdoc/>
        public abstract IEnumerable<int> PropertyIndices { get; }

        /// <inheritdoc/>
        public abstract object this[int index] { get; set; }

        /// <inheritdoc/>
        public abstract object Invoke(bool asConstructor, params object[] args);

        /// <inheritdoc/>
        public abstract object InvokeMethod(string name, params object[] args);

        /// <inheritdoc/>
        public object InvokeAsFunction(params object[] args)
        {
            return Invoke(false, args);
        }

        #endregion

        #region IDisposable implementation

        /// <inheritdoc/>
        public abstract void Dispose();

        #endregion
    }
}
