// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Windows.Core
{
    /// <summary>
    /// Provides a null <c><see cref="ISyncInvoker"/></c> implementation.
    /// </summary>
    /// <remarks>
    /// This class does not enforce thread affinity and should be used with caution. Windows Script
    /// engines can behave unpredictably when thread affinity is violated.
    /// </remarks>
    public class NullSyncInvoker : ISyncInvoker
    {
        /// <summary>
        /// The sole instance of the <c><see cref="NullSyncInvoker"/></c> class.
        /// </summary>
        public static readonly ISyncInvoker Instance = new NullSyncInvoker();

        private NullSyncInvoker()
        {
        }

        #region ISyncInvoker implementation

        /// <summary>
        /// Determines whether the calling thread has access to the script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the script engine, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// The <c><see cref="NullSyncInvoker"/></c> implementation of this method always returns <c>true</c>.
        /// </remarks>
        public bool CheckAccess()
        {
            return true;
        }

        /// <summary>
        /// Enforces that the calling thread has access to the script engine.
        /// </summary>
        /// <remarks>
        /// The <c><see cref="NullSyncInvoker"/></c> implementation of this method performs no action.
        /// </remarks>
        public void VerifyAccess()
        {
        }

        /// <summary>
        /// Invokes a delegate that returns no value on the script engine's thread.
        /// </summary>
        /// <param name="action">The delegate to invoke on the script engine's thread.</param>
        /// <remarks>
        /// The <c><see cref="NullSyncInvoker"/></c> implementation of this method invokes
        /// <paramref name="action"></paramref> without synchronization.
        /// </remarks>
        public void Invoke(Action action)
        {
            action();
        }

        /// <summary>
        /// Invokes a delegate that returns a value on the script engine's thread.
        /// </summary>
        /// <typeparam name="T">The delegate's return value type.</typeparam>
        /// <param name="func">The delegate to invoke on the script engine's thread.</param>
        /// <returns>The delegate's return value.</returns>
        /// <remarks>
        /// The <c><see cref="NullSyncInvoker"/></c> implementation of this method invokes
        /// <paramref name="func"></paramref> without synchronization.
        /// </remarks>
        public T Invoke<T>(Func<T> func)
        {
            return func();
        }

        #endregion
    }
}
