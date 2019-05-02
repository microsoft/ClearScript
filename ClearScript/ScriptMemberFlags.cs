// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines options for exposing type members to script code.
    /// </summary>
    [Flags]
    public enum ScriptMemberFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the field, property, or method return value is not to be restricted to
        /// its declared type.
        /// </summary>
        ExposeRuntimeType = 0x00000001,

        /// <summary>
        /// Specifies that the field, property, or method return value is to be marshaled with full
        /// .NET type information even if it is <c>null</c>. Note that such a value will always
        /// fail equality comparison with JavaScript's
        /// <c><see href="https://msdn.microsoft.com/en-us/library/ie/fhcc96d6(v=vs.94).aspx">null</see></c>,
        /// VBScript's
        /// <c><see href="https://msdn.microsoft.com/en-us/library/f8tbc79x(v=vs.85).aspx">Nothing</see></c>,
        /// and other similar values. Instead, use <see cref="HostFunctions.isNull"/> or
        /// <see cref="object.Equals(object, object)"/> to perform such a comparison.
        /// </summary>
        WrapNullResult = 0x00000002
    }
}
