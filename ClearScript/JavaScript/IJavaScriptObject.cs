// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Represents a JavaScript object.
    /// </summary>
    public interface IJavaScriptObject : IScriptObject
    {
        /// <summary>
        /// Gets the JavaScript object kind.
        /// </summary>
        JavaScriptObjectKind Kind { get; }

        /// <summary>
        /// Gets the JavaScript object's attributes.
        /// </summary>
        JavaScriptObjectFlags Flags { get; }
    }
}
