// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a method that retrieves context information for a document.
    /// </summary>
    /// <param name="info">A structure containing meta-information for the document.</param>
    /// <returns>A property collection containing context information for the document.</returns>
    public delegate IDictionary<string, object> DocumentContextCallback(DocumentInfo info);
}
