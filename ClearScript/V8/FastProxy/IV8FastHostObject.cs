// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents a fast host object.
    /// </summary>
    public interface IV8FastHostObject
    {
        /// <summary>
        /// Gets the object's operations interface.
        /// </summary>
        IV8FastHostObjectOperations Operations { get; }
    }
}
