// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a method that specifies to a script engine whether script execution should continue.
    /// </summary>
    /// <returns><c>True</c> to continue script execution, <c>false</c> to interrupt it.</returns>
    /// <seealso cref="ScriptEngine.ContinuationCallback"/>
    public delegate bool ContinuationCallback();
}
