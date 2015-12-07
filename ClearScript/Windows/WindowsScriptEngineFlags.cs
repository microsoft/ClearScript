// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

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
        /// Specifies that Just-In-Time script debugging is to be enabled. This option is ignored
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
        /// enabled. This option only affects <see cref="JScriptEngine"/>.
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
        /// <see href="http://msdn.microsoft.com/en-us/library/y39d47w8(v=vs.84).aspx">VBArray</see>
        /// object to to access them.
        /// </summary>
        MarshalArraysByValue = 0x00000040,

        /// <summary>
        /// When <see cref="EnableStandardsMode"/> is specified, the ClearScript library uses
        /// virtual method table patching to support JScript-specific
        /// <see href="https://msdn.microsoft.com/en-us/library/sky96ah7(VS.94).aspx">IDispatchEx</see>
        /// extensions that otherwise interfere with some host object functionality. Virtual method
        /// table patching is a very low-level mechanism with global effect. This option specifies
        /// that virtual method table patching is not to be enabled on behalf of the current
        /// <see cref="JScriptEngine"/> instance.
        /// </summary>
        DoNotEnableVTablePatching = 0x00000080,
    }
}
