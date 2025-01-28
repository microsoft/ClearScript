// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a method to be called asynchronously when a document is loaded.
    /// </summary>
    /// <param name="info">A structure containing meta-information for the document.</param>
    /// <param name="contents">A stream that provides read access to the document.</param>
    /// <returns>A task that represents the method's asynchronous operation.</returns>
    /// <remarks>
    /// The callback can modify the document meta-information by specifying or overriding any of
    /// its mutable properties.
    /// </remarks>
    public delegate Task AsyncDocumentLoadCallback(ValueRef<DocumentInfo> info, Stream contents);
}
