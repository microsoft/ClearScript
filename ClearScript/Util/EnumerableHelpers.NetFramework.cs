// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript.Util
{
    /// <exclude/>
    [BypassCustomAttributeLoader]
    public interface IScriptableAsyncEnumerator<out T>
    {
        /// <exclude/>
        T ScriptableCurrent { get; }

        /// <exclude/>
        object ScriptableMoveNextAsync();

        /// <exclude/>
        object ScriptableDisposeAsync();
    }

    internal static partial class EnumerableHelpers
    {
        public static IScriptableAsyncEnumerator<object> ToScriptableAsyncEnumerator(this IEnumerator enumerator, ScriptEngine engine)
        {
            return new ScriptableAsyncEnumeratorOnEnumerator(engine, enumerator);
        }

        public static IScriptableAsyncEnumerator<T> ToScriptableAsyncEnumerator<T>(this IEnumerator<T> enumerator, ScriptEngine engine)
        {
            return new ScriptableAsyncEnumeratorOnEnumerator<T>(engine, enumerator);
        }
    }

    internal static partial class ScriptableEnumerableHelpers
    {
        public static IScriptableAsyncEnumerator<object> GetScriptableAsyncEnumerator(IEnumerable source, ScriptEngine engine)
        {
            return source.GetEnumerator().ToScriptableAsyncEnumerator(engine);
        }
    }

    internal static partial class ScriptableEnumerableHelpers<T>
    {
        public static IScriptableAsyncEnumerator<T> GetScriptableAsyncEnumerator(IEnumerable<T> source, ScriptEngine engine)
        {
            return source.GetEnumerator().ToScriptableAsyncEnumerator(engine);
        }
    }

    internal abstract class ScriptableAsyncEnumeratorBase
    {
        protected static readonly Task CompletedTask = Task.FromResult(0);
    }

    internal abstract class ScriptableAsyncEnumerator<T> : ScriptableAsyncEnumeratorBase, IScriptableAsyncEnumerator<T>
    {
        private readonly ScriptEngine engine;

        protected ScriptableAsyncEnumerator(ScriptEngine engine)
        {
            this.engine = engine;
        }

        public abstract T Current { get; }

        public abstract Task<bool> MoveNextAsync();

        public abstract Task DisposeAsync();

        #region IScriptableAsyncEnumerator<T> implementation

        public T ScriptableCurrent => Current;

        public object ScriptableMoveNextAsync()
        {
            return MoveNextAsync().ToPromise(engine);
        }

        public object ScriptableDisposeAsync()
        {
            return DisposeAsync().ToPromise(engine);
        }

        #endregion
    }

    internal sealed class ScriptableAsyncEnumeratorOnEnumerator : ScriptableAsyncEnumerator<object>
    {
        private readonly IEnumerator enumerator;

        public ScriptableAsyncEnumeratorOnEnumerator(ScriptEngine engine, IEnumerator enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        #region ScriptableAsyncEnumerator<object> overrides

        public override object Current => enumerator.Current;

        public override Task<bool> MoveNextAsync()
        {
            return Task.FromResult(enumerator.MoveNext());
        }

        public override Task DisposeAsync()
        {
            (enumerator as IDisposable)?.Dispose();
            return CompletedTask;
        }

        #endregion
    }

    internal sealed class ScriptableAsyncEnumeratorOnEnumerator<T> : ScriptableAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public ScriptableAsyncEnumeratorOnEnumerator(ScriptEngine engine, IEnumerator<T> enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        #region ScriptableAsyncEnumerator<T> overrides

        public override T Current => enumerator.Current;

        public override Task<bool> MoveNextAsync()
        {
            return Task.FromResult(enumerator.MoveNext());
        }

        public override Task DisposeAsync()
        {
            enumerator.Dispose();
            return CompletedTask;
        }

        #endregion
    }
}
