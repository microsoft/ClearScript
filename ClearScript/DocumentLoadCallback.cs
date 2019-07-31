// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a method to be called when a document is loaded.
    /// </summary>
    /// <param name="info">A structure containing meta-information for the document.</param>
    /// <remarks>
    /// The callback can modify the document meta-information by specifying or overriding any of
    /// its mutable properties.
    /// </remarks>
    public delegate void DocumentLoadCallback(ref DocumentInfo info);
}
