// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.ClearScript.Test
{
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class TestEnumerable
    {
        public interface IDisposableEnumeratorFactory
        {
            int DisposedEnumeratorCount { get; }
        }

        public static IEnumerable<T> CreateGeneric<T>(params T[] items)
        {
            return CreateInternal(items);
        }

        public static IEnumerable Create<T>(params T[] items)
        {
            return CreateGeneric(items);
        }

        private static TestEnumerableImpl<T> CreateInternal<T>(T[] items)
        {
            return new TestEnumerableImpl<T>(items);
        }

        // ReSharper disable once PartialTypeWithSinglePart
        private partial class TestEnumerableImpl<T> : IEnumerable<T>, IDisposableEnumeratorFactory
        {
            private readonly T[] items;
            private int disposedEnumeratorCount;

            int IDisposableEnumeratorFactory.DisposedEnumeratorCount => disposedEnumeratorCount;

            public TestEnumerableImpl(T[] items)
            {
                this.items = items;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }

            // ReSharper disable once PartialTypeWithSinglePart
            private partial class Enumerator : IEnumerator<T>
            {
                private readonly TestEnumerableImpl<T> enumerable;
                private int index = -1;

                public Enumerator(TestEnumerableImpl<T> enumerable)
                {
                    this.enumerable = enumerable;
                }

                T IEnumerator<T>.Current => enumerable.items[index];

                object IEnumerator.Current => ((IEnumerator<T>)this).Current;

                bool IEnumerator.MoveNext()
                {
                    return ++index < enumerable.items.Length;
                }

                void IEnumerator.Reset()
                {
                    throw new NotImplementedException();
                }

                void IDisposable.Dispose()
                {
                    Interlocked.Increment(ref enumerable.disposedEnumeratorCount);
                }
            }
        }
    }
}
