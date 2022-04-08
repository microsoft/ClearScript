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
    public interface IAsyncEnumeratorPromise<out T> : IAsyncEnumerator<T>
    {
        /// <exclude/>
        object MoveNextPromise();

        /// <exclude/>
        object DisposePromise();
    }

    internal static partial class EnumerableHelpers
    {
        public static IAsyncEnumeratorPromise<T> ToAsyncEnumerator<T>(this IEnumerator<T> enumerator, ScriptEngine engine)
        {
            return new AsyncEnumeratorPromiseOnEnumerator<T>(engine, enumerator);
        }

        public static IAsyncEnumeratorPromise<object> ToAsyncEnumerator(this IEnumerator enumerator, ScriptEngine engine)
        {
            return new AsyncEnumeratorPromiseOnEnumerator(engine, enumerator);
        }

        public static IAsyncEnumeratorPromise<T> GetAsyncEnumerator<T>(IAsyncEnumerable<T> source, ScriptEngine engine)
        {
            return new AsyncEnumeratorPromiseOnAsyncEnumerator<T>(engine, source.GetAsyncEnumerator());
        }

        public static IAsyncEnumeratorPromise<T> GetAsyncEnumerator<T>(IEnumerable<T> source, ScriptEngine engine)
        {
            return source.GetEnumerator().ToAsyncEnumerator(engine);
        }

        public static IAsyncEnumeratorPromise<object> GetAsyncEnumerator(IEnumerable source, ScriptEngine engine)
        {
            return source.GetEnumerator().ToAsyncEnumerator(engine);
        }
    }

    internal abstract class AsyncEnumeratorPromise<T> : IAsyncEnumeratorPromise<T>
    {
        private readonly ScriptEngine engine;

        protected AsyncEnumeratorPromise(ScriptEngine engine)
        {
            this.engine = engine;
        }

        public abstract T Current { get; }

        public abstract ValueTask<bool> MoveNextAsync();

        public abstract ValueTask DisposeAsync();

        public object MoveNextPromise()
        {
            return MoveNextAsync().ToPromise(engine);
        }

        public object DisposePromise()
        {
            return DisposeAsync().ToPromise(engine);
        }
    }

    internal sealed class AsyncEnumeratorPromiseOnAsyncEnumerator<T> : AsyncEnumeratorPromise<T>
    {
        private readonly IAsyncEnumerator<T> enumerator;

        public AsyncEnumeratorPromiseOnAsyncEnumerator(ScriptEngine engine, IAsyncEnumerator<T> enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        public override T Current => enumerator.Current;

        public override ValueTask<bool> MoveNextAsync()
        {
            return enumerator.MoveNextAsync();
        }

        public override ValueTask DisposeAsync()
        {
            return enumerator.DisposeAsync();
        }
    }

    internal sealed class AsyncEnumeratorPromiseOnEnumerator<T> : AsyncEnumeratorPromise<T>
    {
        private readonly IEnumerator<T> enumerator;

        public AsyncEnumeratorPromiseOnEnumerator(ScriptEngine engine, IEnumerator<T> enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        public override T Current => enumerator.Current;

        public override ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public override ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return default;
        }
    }

    internal sealed class AsyncEnumeratorPromiseOnEnumerator : AsyncEnumeratorPromise<object>
    {
        private readonly IEnumerator enumerator;

        public AsyncEnumeratorPromiseOnEnumerator(ScriptEngine engine, IEnumerator enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        public override object Current => enumerator.Current;

        public override ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public override ValueTask DisposeAsync()
        {
            (enumerator as IDisposable)?.Dispose();
            return default;
        }
    }
}
