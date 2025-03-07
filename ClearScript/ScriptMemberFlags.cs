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
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/null">null</see></c>,
        /// VBScript's
        /// <c><see href="https://docs.microsoft.com/en-us/previous-versions//f8tbc79x(v=vs.85)">Nothing</see></c>,
        /// and other similar values. Instead, use <c><see cref="HostFunctions.isNull"/></c> or
        /// <c><see cref="object.Equals(object, object)"/></c> to perform such a comparison.
        /// </summary>
        WrapNullResult = 0x00000002
    }

    internal static class ScriptMemberFlagsHelpers
    {
        public static bool HasAllFlags(this ScriptMemberFlags value, ScriptMemberFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this ScriptMemberFlags value, ScriptMemberFlags flags) => (value & flags) != 0;
    }
}
