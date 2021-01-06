// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// An implementation of <see cref="ISyncInvoker"/> that only supports invoking delegates immediately on the current thread.
    /// This implementation can run anywhere, but does not support scripts that use events which are sent from another thread.
    /// </summary>
    public class DefaultSyncInvoker : ISyncInvoker
    {
        private readonly Thread thread;

        /// <summary>
        /// 
        /// </summary>
        public DefaultSyncInvoker()
        {
            this.thread = Thread.CurrentThread;
        }

        /// <inheritdoc/>
        public bool CheckAccess() => Thread.CurrentThread == this.thread;

        /// <inheritdoc/>
        public void VerifyAccess()
        {
            if (!CheckAccess())
            {
                throw new InvalidOperationException("The current thread does not have access to this DefaultSyncInvoker");
            }
        }

        /// <inheritdoc/>
        public void Invoke(Action action)
        {
            if (!CheckAccess())
            {
                throw new NotImplementedException("DefaultSyncInvoker does not support sending a work item from another thread.");
            }

            action();
        }

        /// <inheritdoc/>
        public T Invoke<T>(Func<T> func)
        {
            if (!CheckAccess())
            {
                throw new NotImplementedException("DefaultSyncInvoker does not support sending a work item from another thread.");
            }

            return func();
        }
    }
}
