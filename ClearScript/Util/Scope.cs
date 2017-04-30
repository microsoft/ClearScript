// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal interface IScope<out T>: IDisposable
    {
        T Value { get; }
    }

    internal static class Scope
    {
        public static IDisposable Create(Action enterAction, Action exitAction)
        {
            return new ScopeImpl(enterAction, exitAction);
        }

        public static IScope<T> Create<T>(Func<T> enterFunc, Action<T> exitAction)
        {
            return new ScopeImpl<T>(enterFunc, exitAction);
        }

        #region Nested type: ScopeImpl

        private class ScopeImpl : IDisposable
        {
            private readonly Action exitAction;
            private DisposedFlag disposedFlag = new DisposedFlag();

            public ScopeImpl(Action enterAction, Action exitAction)
            {
                this.exitAction = exitAction;
                enterAction();
            }

            #region IDisposable implementation

            public void Dispose()
            {
                if (disposedFlag.Set() && (exitAction != null))
                {
                    exitAction();
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: ScopeImpl<T>

        private class ScopeImpl<T> : IScope<T>
        {
            private readonly T value;
            private readonly Action<T> exitAction;
            private DisposedFlag disposedFlag = new DisposedFlag();

            public ScopeImpl(Func<T> enterFunc, Action<T> exitAction)
            {
                this.exitAction = exitAction;
                value = enterFunc();
            }

            #region IScope<T> implementation

            public T Value
            {
                get { return value; }
            }

            public void Dispose()
            {
                if (disposedFlag.Set() && (exitAction != null))
                {
                    exitAction(value);
                }
            }

            #endregion
        }

        #endregion
    }
}
