// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// An interface to invoke delegates with thread-affinity.
    /// </summary>
    public interface ISyncInvoker

    {
        /// <summary>
        /// Determines whether the calling thread is the thread associated with this <see cref="ISyncInvoker"/>.
        /// </summary>
        /// <returns></returns>
        bool CheckAccess();

        /// <summary>
        /// Determines whether the calling thread has access to this <see cref="ISyncInvoker"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The calling thread does not have access to this <see cref="ISyncInvoker"/>.</exception>
        void VerifyAccess();

        /// <summary>
        /// Executes the specified <see cref="Action"/> synchronously on the thread the <see cref="ISyncInvoker"/> is associated with.
        /// </summary>
        /// <param name="action"></param>
        void Invoke(Action action);

        /// <summary>
        /// Executes the specified <see cref="Func{T}"/> synchronously on the thread the <see cref="ISyncInvoker"/> is associated with.
        /// </summary>
        /// <param name="func"></param>
        T Invoke<T>(Func<T> func);
    }
}
