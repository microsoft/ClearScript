// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Defines options for initializing a new Windows Script engine instance.
    /// </summary>
    [Flags]
    public enum WindowsScriptEngineFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that script debugging features are to be enabled.
        /// </summary>
        EnableDebugging = 0x00000001,

        /// <summary>
        /// Specifies that Just-In-Timestamp script debugging is to be enabled. This option is ignored
        /// if <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        EnableJITDebugging = 0x00000002,

        /// <summary>
        /// Specifies that smart source document management is to be disabled. This option is
        /// ignored if <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        DisableSourceManagement = 0x00000004,

        /// <summary>
        /// Specifies that script language features that enhance standards compliance are to be
        /// enabled. This option only affects <see cref="Core.JScriptEngine"/>.
        /// </summary>
        EnableStandardsMode = 0x00000008,

        /// <summary>
        /// Specifies that <c>null</c> is to be marshaled as a variant of type <c>VT_DISPATCH</c>.
        /// This option does not affect field, property, or method return values declared as
        /// <see cref="object"/>, <see cref="string"/>, nullable <see cref="bool"/>, or nullable
        /// numeric types.
        /// </summary>
        MarshalNullAsDispatch = 0x00000010,

        /// <summary>
        /// Specifies that <see cref="decimal"/> values are to be marshaled as variants of type
        /// <c>VT_CY</c>.
        /// </summary>
        MarshalDecimalAsCurrency = 0x00000020,

        /// <summary>
        /// Specifies that managed arrays that are passed or returned to script code are to be
        /// converted to script arrays and marshaled as variants of type <c>VT_ARRAY</c>. In
        /// VBScript these objects are the native array type. JScript code can use the
        /// <see href="https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/y39d47w8(v=vs.84)">VBArray</see>
        /// object to access them.
        /// </summary>
        MarshalArraysByValue = 0x00000040,

        /// <summary>
        /// When <see cref="EnableStandardsMode"/> is specified, the ClearScript library uses
        /// virtual method table patching to support JScript-specific
        /// <see href="https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/sky96ah7(v=vs.94)">IDispatchEx</see>
        /// extensions that otherwise interfere with some host object functionality. Virtual method
        /// table patching is a very low-level mechanism with global effect. This option specifies
        /// that virtual method table patching is not to be enabled on behalf of the current
        /// <see cref="Core.JScriptEngine"/> instance.
        /// </summary>
        DoNotEnableVTablePatching = 0x00000080,

        /// <summary>
        /// Specifies that <see cref="DateTime"/> values are to be marshaled as variants of type
        /// <c>VT_DATE</c>. In VBScript these objects are the native date-time type. JScript code
        /// can pass them to the
        /// <see href="https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2010/dca21baa(v=vs.100)">Date</see>
        /// constructor for property access.
        /// </summary>
        MarshalDateTimeAsDate = 0x00000100
    }
}
