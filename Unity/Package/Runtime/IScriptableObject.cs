// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines a method to be called when a host object is exposed to script code.
    /// </summary>
    public interface IScriptableObject
    {
        /// <summary>
        /// Notifies the host object that it has been exposed to script code.
        /// </summary>
        /// <param name="engine">The script engine in which the host object was exposed.</param>
        /// <remarks>
        /// This method may be called more than once for a given host object. The object may be
        /// exposed in multiple script engines or many times in one script engine. Implementers
        /// should avoid expensive operations within this method, or cache the results of such
        /// operations for efficient retrieval during subsequent invocations.
        /// </remarks>
        void OnExposedToScriptCode(ScriptEngine engine);
    }
}
