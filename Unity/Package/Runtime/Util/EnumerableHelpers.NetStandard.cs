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
    [DefaultScriptUsage(ScriptAccess.Full)]
    public interface IScriptableAsyncEnumerator<out T> : IAsyncEnumerator<T>
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

        public static IScriptableAsyncEnumerator<T> ToScriptableAsyncEnumerator<T>(this IAsyncEnumerator<T> enumerator, ScriptEngine engine)
        {
            return new ScriptableAsyncEnumeratorOnAsyncEnumerator<T>(engine, enumerator);
        }
    }

    internal static partial class ScriptableEnumerableHelpers
    {
        public static object GetScriptableAsyncEnumerator(IEnumerable source, ScriptEngine engine)
        {
            return HostItem.Wrap(engine, source.GetEnumerator().ToScriptableAsyncEnumerator(engine), typeof(IScriptableAsyncEnumerator<object>));
        }
    }

    internal static partial class ScriptableEnumerableHelpers<T>
    {
        public static object GetScriptableAsyncEnumerator(IEnumerable<T> source, ScriptEngine engine)
        {
            return HostItem.Wrap(engine, source.GetEnumerator().ToScriptableAsyncEnumerator(engine), typeof(IScriptableAsyncEnumerator<T>));
        }

        public static object GetScriptableAsyncEnumerator(IAsyncEnumerable<T> source, ScriptEngine engine)
        {
            return HostItem.Wrap(engine, source.GetAsyncEnumerator().ToScriptableAsyncEnumerator(engine), typeof(IScriptableAsyncEnumerator<T>));
        }
    }

    internal abstract class ScriptableAsyncEnumerator<T> : IScriptableAsyncEnumerator<T>
    {
        private readonly ScriptEngine engine;

        protected ScriptableAsyncEnumerator(ScriptEngine engine)
        {
            this.engine = engine;
        }

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

        #region IAsyncEnumerable<T> implementation

        public abstract T Current { get; }

        public abstract ValueTask<bool> MoveNextAsync();

        #endregion

        #region IAsyncDisposable implementation

        public abstract ValueTask DisposeAsync();

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

        public override ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public override ValueTask DisposeAsync()
        {
            (enumerator as IDisposable)?.Dispose();
            return default;
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

        public override ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }

        public override ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return default;
        }

        #endregion
    }

    internal sealed class ScriptableAsyncEnumeratorOnAsyncEnumerator<T> : ScriptableAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> enumerator;

        public ScriptableAsyncEnumeratorOnAsyncEnumerator(ScriptEngine engine, IAsyncEnumerator<T> enumerator)
            : base(engine)
        {
            this.enumerator = enumerator;
        }

        #region ScriptableAsyncEnumerator<T> overrides

        public override T Current => enumerator.Current;

        public override ValueTask<bool> MoveNextAsync()
        {
            return enumerator.MoveNextAsync();
        }

        public override ValueTask DisposeAsync()
        {
            return enumerator.DisposeAsync();
        }

        #endregion
    }
}
