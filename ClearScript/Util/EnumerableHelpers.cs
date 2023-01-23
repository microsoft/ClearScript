// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

    internal static partial class EnumerableHelpers
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
    internal static partial class ScriptableEnumerableHelpers
    {
        public static readonly HostType HostType = HostType.Wrap(typeof(ScriptableEnumerableHelpers));

        public static object GetScriptableEnumerator(IEnumerable source)
        {
            return HostObject.Wrap(new ScriptableEnumeratorOnEnumerator(source.GetEnumerator()), typeof(IScriptableEnumerator));
        }
    }

    [BypassCustomAttributeLoader]
    [DefaultScriptUsage(ScriptAccess.Full)]
    internal static partial class ScriptableEnumerableHelpers<T>
    {
        public static readonly HostType HostType = HostType.Wrap(typeof(ScriptableEnumerableHelpers<T>));

        public static object GetScriptableEnumerator(IEnumerable<T> source)
        {
            return HostObject.Wrap(new ScriptableEnumeratorOnEnumerator<T>(source.GetEnumerator()), typeof(IScriptableEnumerator<T>));
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
}
