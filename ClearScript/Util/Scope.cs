// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal interface IScope<out TValue>: IDisposable
    {
        TValue Value { get; }
    }

    internal static class Scope
    {
        public static IDisposable Create(Action enterAction, Action exitAction)
        {
            enterAction?.Invoke();
            return new ScopeImpl(exitAction);
        }

        public static IDisposable Create<TArg>(Action<TArg> enterAction, Action exitAction, in TArg arg)
        {
            enterAction?.Invoke(arg);
            return new ScopeImpl(exitAction);
        }

        public static IScope<TValue> Create<TValue>(Func<TValue> enterFunc, Action<TValue> exitAction)
        {
            var value = (enterFunc is not null) ? enterFunc() : default;
            return new ScopeImpl<TValue>(value, exitAction);
        }

        public static IScope<TValue> Create<TArg, TValue>(Func<TArg, TValue> enterFunc, Action<TValue> exitAction, in TArg arg)
        {
            var value = (enterFunc is not null) ? enterFunc(arg) : default;
            return new ScopeImpl<TValue>(value, exitAction);
        }

        #region Nested type: ScopeImpl

        private sealed class ScopeImpl : IDisposable
        {
            private readonly Action exitAction;
            private readonly OneWayFlag disposedFlag = new();

            public ScopeImpl(Action exitAction)
            {
                this.exitAction = exitAction;
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (disposedFlag.Set())
                {
                    exitAction?.Invoke();
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: ScopeImpl<TValue>

        private sealed class ScopeImpl<TValue> : IScope<TValue>
        {
            private readonly Action<TValue> exitAction;
            private readonly OneWayFlag disposedFlag = new();

            public ScopeImpl(TValue value, Action<TValue> exitAction)
            {
                this.exitAction = exitAction;
                Value = value;
            }

            #region IScope<TValue> implementation

            public TValue Value { get; }

            public void Dispose()
            {
                if (disposedFlag.Set())
                {
                    exitAction?.Invoke(Value);
                }
            }

            #endregion
        }

        #endregion
    }
}
