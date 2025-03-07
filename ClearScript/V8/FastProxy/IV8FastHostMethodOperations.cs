// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents the operations supported by a fast host method.
    /// </summary>
    public interface IV8FastHostMethodOperations : IV8FastHostObjectOperations
    {
        /// <summary>
        /// Invokes the method.
        /// </summary>
        /// <param name="args">The arguments to pass to the method.</param>
        /// <param name="result">On return, the method's return value.</param>
        void Invoke(in V8FastArgs args, in V8FastResult result);
    }
}
