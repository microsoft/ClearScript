// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast host method.
    /// </summary>
    public interface IV8FastHostMethod : IV8FastHostObject
    {
        /// <summary>
        /// Gets the method's operations interface.
        /// </summary>
        new IV8FastHostMethodOperations Operations { get; }
    }
}
