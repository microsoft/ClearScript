// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents the result of a host method that returns no value.
    /// </summary>
    /// <remarks>
    /// Some script languages expect every subroutine call to return a value. When script code
    /// written in such a language invokes a host method that explicitly returns no value (such as
    /// a C# <see href="http://msdn.microsoft.com/en-us/library/yah0tteb.aspx">void</see> method),
    /// the ClearScript library provides an instance of this class as a dummy return value.
    /// </remarks>
    public class VoidResult
    {
        internal static readonly VoidResult Value = new VoidResult();

        private VoidResult()
        {
        }
    }
}
