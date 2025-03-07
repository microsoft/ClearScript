// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast asynchronous enumerator.
    /// </summary>
    public interface IV8FastAsyncEnumerator : IAsyncDisposable
    {
        /// <summary>
        /// Gets the item at the current position in the collection.
        /// </summary>
        /// <param name="item">On return, contains the item at the current position in the collection.</param>
        void GetCurrent(in V8FastResult item);

        /// <summary>
        /// Advances the enumerator asynchronously to the next position in the collection.
        /// </summary>
        /// <returns>A task structure whose result is <c>true</c> if the enumerator was advanced to the next position, <c>false</c> if it was already at the end of the collection.</returns>
        ValueTask<bool> MoveNextAsync();
    }
}
