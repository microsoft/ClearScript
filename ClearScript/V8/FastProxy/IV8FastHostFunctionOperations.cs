// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8.FastProxy
{
    /// <summary>
    /// Represents the operations supported by a fast host function.
    /// </summary>
    public interface IV8FastHostFunctionOperations : IV8FastHostObjectOperations
    {
        /// <summary>
        /// Invokes the function.
        /// </summary>
        /// <param name="asConstructor"><c>True</c> to invoke the function as a constructor, <c>false</c> otherwise.</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <param name="result">On return, the function's return value.</param>
        void Invoke(bool asConstructor, in V8FastArgs args, in V8FastResult result);
    }
}
