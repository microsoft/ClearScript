// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal ref struct ValueScope
    {
        private readonly Action exitAction;
        private bool disposed;

        public ValueScope(Action exitAction)
        {
            this.exitAction = exitAction;
            disposed = false;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                exitAction?.Invoke();
            }
        }
    }

    internal ref struct ValueScope<TValue>
    {
        private readonly Action<TValue> exitAction;
        private bool disposed;

        public TValue Value { get; }

        public ValueScope(TValue value, Action<TValue> exitAction)
        {
            this.exitAction = exitAction;
            disposed = false;
            Value = value;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                exitAction?.Invoke(Value);
            }
        }
    }

    internal static class ScopeFactory
    {
        public static ValueScope Create(Action enterAction, Action exitAction)
        {
            enterAction?.Invoke();
            return new ValueScope(exitAction);
        }

        public static ValueScope Create<TArg>(Action<TArg> enterAction, Action exitAction, in TArg arg)
        {
            enterAction?.Invoke(arg);
            return new ValueScope(exitAction);
        }

        public static ValueScope<TValue> Create<TValue>(Func<TValue> enterFunc, Action<TValue> exitAction)
        {
            var value = (enterFunc is not null) ? enterFunc() : default;
            return new ValueScope<TValue>(value, exitAction);
        }

        public static ValueScope<TValue> Create<TArg, TValue>(Func<TArg, TValue> enterFunc, Action<TValue> exitAction, in TArg arg)
        {
            var value = (enterFunc is not null) ? enterFunc(arg) : default;
            return new ValueScope<TValue>(value, exitAction);
        }
    }
}
