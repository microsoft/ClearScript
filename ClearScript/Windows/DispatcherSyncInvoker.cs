// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Windows.Threading;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// An implementation of <see cref="ISyncInvoker"/> based on a <see cref="Dispatcher"/>.
    /// It processes work items using the Windows message queue. This allows to use script with event handling.
    /// This also allows to use COM types that implement event handling using the Windows message queue, such as MSXML2.XMLHTTP XMLHttpRequest.
    /// </summary>
    public class DispatcherSyncInvoker : ISyncInvoker
    {
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Creates an instance of <see cref="DispatcherSyncInvoker"/> based on <see cref="Dispatcher.CurrentDispatcher"/>.
        /// </summary>
        /// <returns>An instance of <see cref="DispatcherSyncInvoker"/>.</returns>
        public static DispatcherSyncInvoker FromCurrent()
        {
            return new DispatcherSyncInvoker(Dispatcher.CurrentDispatcher);
        }

        /// <summary>
        /// Creates an instance of <see cref="DispatcherSyncInvoker"/> with the specified dispatcher.
        /// </summary>
        /// <param name="dispatcher">A dispatcher.</param>
        public DispatcherSyncInvoker(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc/>
        public bool CheckAccess() => dispatcher.CheckAccess();

        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            dispatcher.Invoke(DispatcherPriority.Send, action);
        }

        /// <inheritdoc/>
        public T Invoke<T>(Func<T> func)
        {
            return (T)dispatcher.Invoke(DispatcherPriority.Send, func);
        }

        /// <inheritdoc/>
        public void VerifyAccess() => dispatcher.VerifyAccess();
    }
}
