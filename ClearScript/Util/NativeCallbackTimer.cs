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
        private readonly InterlockedOneWayFlag disposedFlag = new();

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
                if (MiscHelpers.Try(out var result, static ctx => ctx.timer.Change(ctx.dueTime, ctx.period), (timer, dueTime, period)))
                {
                    return result;
                }
            }

            return false;
        }

        private void OnTimer(object _)
        {
            if (!disposedFlag.IsSet)
            {
                MiscHelpers.Try(static callback => callback.Invoke(), callback);
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
