// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.Util
{
    /// <exclude/>
    [BypassCustomAttributeLoader]
    [DefaultScriptUsage(ScriptAccess.Full)]
    public interface IScriptableEnumerator : IEnumerator, IDisposable
    {
        /// <exclude/>
        object ScriptableCurrent { get; }

        /// <exclude/>
        bool ScriptableMoveNext();

        /// <exclude/>
        void ScriptableDispose();
    }

    /// <exclude/>
    [BypassCustomAttributeLoader]
    [DefaultScriptUsage(ScriptAccess.Full)]
    public interface IScriptableEnumerator<out T> : IScriptableEnumerator, IEnumerator<T>
    {
        /// <exclude/>
        new T ScriptableCurrent { get; }
    }

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

    internal static class EnumerableHelpers
    {
        public static IList<T> ToIList<T>(this IEnumerable<T> source)
        {
            return (source as IList<T>) ?? source.ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var index = 0;
            foreach (var element in source)
            {
                action(element, index++);
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var element in source)
            {
                yield return element;

                foreach (var descendant in selector(element).Flatten(selector))
                {
                    yield return descendant;
                }
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this T element)
        {
            yield return element;
        }

        public static IEnumerable<string> ExcludeIndices(this IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                if (!int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                {
                    yield return name;
                }
            }
        }

        public static IEnumerable<int> GetIndices(this IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    yield return index;
                }
            }
        }
    }

    [BypassCustomAttributeLoader]
    [DefaultScriptUsage(ScriptAccess.Full)]
    internal static class ScriptableEnumerableHelpers
    {
        public static readonly HostType HostType = HostType.Wrap(typeof(ScriptableEnumerableHelpers));

        public static object GetScriptableEnumerator(IEnumerable source)
        {
            // ReSharper disable once NotDisposedResource
            return HostObject.Wrap(new ScriptableEnumeratorOnEnumerator(source.GetEnumerator()), typeof(IScriptableEnumerator));
        }
        public static object GetScriptableAsyncEnumerator(IEnumerable source, ScriptEngine engine)
        {
            // ReSharper disable once NotDisposedResource
            return HostItem.Wrap(engine, new ScriptableAsyncEnumeratorOnEnumerator(engine, source.GetEnumerator()), typeof(IScriptableAsyncEnumerator<object>));
        }
    }

    [BypassCustomAttributeLoader]
    [DefaultScriptUsage(ScriptAccess.Full)]
    internal static class ScriptableEnumerableHelpers<T>
    {
        public static readonly HostType HostType = HostType.Wrap(typeof(ScriptableEnumerableHelpers<T>));

        public static object GetScriptableEnumerator(IEnumerable<T> source)
        {
            return HostObject.Wrap(new ScriptableEnumeratorOnEnumerator<T>(source.GetEnumerator()), typeof(IScriptableEnumerator<T>));
        }

        public static object GetScriptableAsyncEnumerator(IEnumerable<T> source, ScriptEngine engine)
        {
            // ReSharper disable once NotDisposedResource
            return HostItem.Wrap(engine, new ScriptableAsyncEnumeratorOnEnumerator<T>(engine, source.GetEnumerator()), typeof(IScriptableAsyncEnumerator<T>));
        }

        public static object GetScriptableAsyncEnumerator(IAsyncEnumerable<T> source, ScriptEngine engine)
        {
            // ReSharper disable once NotDisposedResource
            return HostItem.Wrap(engine, new ScriptableAsyncEnumeratorOnAsyncEnumerator<T>(engine, source.GetAsyncEnumerator()), typeof(IScriptableAsyncEnumerator<T>));
        }
    }

    internal sealed class ScriptableEnumeratorOnEnumerator : IScriptableEnumerator
    {
        private readonly IEnumerator enumerator;

        public ScriptableEnumeratorOnEnumerator(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        #region IScriptableEnumerator implementation

        public object ScriptableCurrent => Current;

        public bool ScriptableMoveNext()
        {
            return MoveNext();
        }

        public void ScriptableDispose()
        {
            Dispose();
        }

        #endregion

        #region IEnumerator implementation

        public object Current => enumerator.Current;

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            (enumerator as IDisposable)?.Dispose();
        }

        #endregion
    }

    internal sealed class ScriptableEnumeratorOnEnumerator<T> : IScriptableEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public ScriptableEnumeratorOnEnumerator(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator;
        }

        #region IScriptableEnumerator<T> implementation

        public T ScriptableCurrent => Current;

        #endregion

        #region IScriptableEnumerator implementation

        object IScriptableEnumerator.ScriptableCurrent => ScriptableCurrent;

        public bool ScriptableMoveNext()
        {
            return MoveNext();
        }

        public void ScriptableDispose()
        {
            Dispose();
        }

        #endregion

        #region IEnumerator<T> implementation

        public T Current => enumerator.Current;

        #endregion

        #region IEnumerator implementation

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            enumerator.Dispose();
        }

        #endregion
    }

    internal sealed class ScriptableEnumeratorOnEnumVariant : IScriptableEnumerator
    {
        private readonly IEnumVARIANT enumVariant;

        public ScriptableEnumeratorOnEnumVariant(IEnumVARIANT enumVariant)
        {
            this.enumVariant = enumVariant;
        }

        #region IScriptableEnumerator implementation

        public object ScriptableCurrent => Current;

        public bool ScriptableMoveNext()
        {
            return MoveNext();
        }

        public void ScriptableDispose()
        {
            Dispose();
        }

        #endregion

        #region IEnumerator implementation

        public object Current { get; private set; }

        public bool MoveNext()
        {
            var items = new object[1];
            if (enumVariant.Next(1, items, IntPtr.Zero) == HResult.S_OK)
            {
                Current = items[0];
                return true;
            }

            return false;
        }

        public void Reset()
        {
            enumVariant.Reset();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
        }

        #endregion
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
