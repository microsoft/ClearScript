// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast host function.
    /// </summary>
    public interface IV8FastHostFunction : IV8FastHostObject
    {
        /// <summary>
        /// Gets the function's operations interface.
        /// </summary>
        new IV8FastHostFunctionOperations Operations { get; }
    }
}
