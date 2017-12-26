// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;

namespace Microsoft.ClearScript.Util
{
    internal sealed class NativeCallbackTimer : IDisposable
    {
        private readonly Timer timer;
        private readonly INativeCallback callback;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        public NativeCallbackTimer(int dueTime, int period, INativeCallback callback)
        {
            this.callback = callback;
            timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);

            if ((dueTime != Timeout.Infinite) || (period != Timeout.Infinite))
            {
                timer.Change(dueTime, period);
            }
        }

        public bool Change(int dueTime, int period)
        {
            if (!disposedFlag.IsSet)
            {
                bool result;
                if (MiscHelpers.Try(out result, () => timer.Change(dueTime, period)))
                {
                    return result;
                }
            }

            return false;
        }

        private void OnTimer(object state)
        {
            if (!disposedFlag.IsSet)
            {
                MiscHelpers.Try(callback.Invoke);
            }
        }

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                timer.Dispose();
                callback.Dispose();
            }
        }
    }
}
