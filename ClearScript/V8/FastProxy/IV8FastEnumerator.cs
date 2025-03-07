// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast enumerator.
    /// </summary>
    public interface IV8FastEnumerator : IDisposable
    {
        /// <summary>
        /// Gets the item at the current position in the collection.
        /// </summary>
        /// <param name="item">On return, contains the item at the current position in the collection.</param>
        void GetCurrent(in V8FastResult item);

        /// <summary>
        /// Advances the enumerator to the next position in the collection.
        /// </summary>
        /// <returns><c>True</c> if the enumerator was advanced to the next position, <c>false</c> if it is already at the end of the collection.</returns>
        bool MoveNext();
    }
}
