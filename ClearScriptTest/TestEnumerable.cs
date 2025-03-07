// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public static IAsyncEnumerable<T> CreateAsync<T>(params T[] items)
        {
            return CreateInternal(items);
        }

        private static TestEnumerableImpl<T> CreateInternal<T>(T[] items)
        {
            return new TestEnumerableImpl<T>(items);
        }

        // ReSharper disable once PartialTypeWithSinglePart
        private partial class TestEnumerableImpl<T> : IEnumerable<T>, IAsyncEnumerable<T>, IDisposableEnumeratorFactory
        {
            private readonly T[] items;
            private int disposedEnumeratorCount;

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
                // ReSharper disable once NotDisposedResourceIsReturned
                return ((IEnumerable<T>)this).GetEnumerator();
            }

            IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                return new Enumerator(this);
            }

            int IDisposableEnumeratorFactory.DisposedEnumeratorCount => disposedEnumeratorCount;

            // ReSharper disable once PartialTypeWithSinglePart
            private partial class Enumerator : IEnumerator<T>, IAsyncEnumerator<T>
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

                T IAsyncEnumerator<T>.Current => ((IEnumerator<T>)this).Current;

                ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync()
                {
                    return new ValueTask<bool>(((IEnumerator)this).MoveNext());
                }

                ValueTask IAsyncDisposable.DisposeAsync()
                {
                    ((IDisposable)this).Dispose();
                    return default;
                }
            }
        }
    }
}
