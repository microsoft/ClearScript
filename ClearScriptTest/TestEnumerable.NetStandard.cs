// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Test
{
    public static partial class TestEnumerable
    {
        public static IAsyncEnumerable<T> CreateAsync<T>(params T[] items)
        {
            return CreateInternal(items);
        }

        private partial class TestEnumerableImpl<T> : IAsyncEnumerable<T>
        {
            IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
            {
                return new Enumerator(this);
            }

            private partial class Enumerator : IAsyncEnumerator<T>
            {
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
