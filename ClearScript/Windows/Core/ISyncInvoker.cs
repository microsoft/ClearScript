// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Windows.Core
{
    /// <summary>
    /// Represents an object that enforces thread affinity for a Windows Script engine.
    /// </summary>
    public interface ISyncInvoker
    {
        /// <summary>
        /// Determines whether the calling thread has access to the script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the script engine, <c>false</c> otherwise.</returns>
        bool CheckAccess();

        /// <summary>
        /// Enforces that the calling thread has access to the script engine.
        /// </summary>
        void VerifyAccess();

        /// <summary>
        /// Invokes a delegate that returns no value on the script engine's thread.
        /// </summary>
        /// <param name="action">The delegate to invoke on the script engine's thread.</param>
        void Invoke(Action action);

        /// <summary>
        /// Invokes a delegate that returns a value on the script engine's thread.
        /// </summary>
        /// <typeparam name="T">The delegate's return value type.</typeparam>
        /// <param name="func">The delegate to invoke on the script engine's thread.</param>
        /// <returns>The delegate's return value.</returns>
        T Invoke<T>(Func<T> func);
    }
}
