// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Windows.Threading;
using Microsoft.ClearScript.Windows.Core;

namespace Microsoft.ClearScript.Windows
{
    internal sealed class DispatcherSyncInvoker : ISyncInvoker
    {
        public DispatcherSyncInvoker()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public Dispatcher Dispatcher { get; }

        public bool CheckAccess()
        {
            return Dispatcher.CheckAccess();
        }

        public void VerifyAccess()
        {
            Dispatcher.VerifyAccess();
        }

        public void Invoke(Action action)
        {
            Dispatcher.Invoke(action, DispatcherPriority.Send);
        }

        public T Invoke<T>(Func<T> func)
        {
            return Dispatcher.Invoke(func, DispatcherPriority.Send);
        }
    }
}
