// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines options for initializing a new V8 JavaScript engine instance.
    /// </summary>
    [Flags]
    public enum V8ScriptEngineFlags
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
        /// Specifies that support for <see cref="HostItemFlags.GlobalMembers"/> behavior is to be
        /// disabled. This option yields a significant performance benefit for global item access.
        /// </summary>
        DisableGlobalMembers = 0x00000002,

        /// <summary>
        /// Specifies that remote script debugging is to be enabled. This option is ignored if
        /// <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        EnableRemoteDebugging = 0x00000004,

        /// <summary>
        /// Specifies that the script engine is to wait for a debugger connection and schedule a
        /// pause before executing the first line of application script code. This option is
        /// ignored if <see cref="EnableDebugging"/> is not specified.
        /// </summary>
        AwaitDebuggerAndPauseOnStart = 0x00000008,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion between
        /// .NET <see cref="DateTime"/> objects and JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date">Date</see>
        /// objects. This conversion is bidirectional. A <c>DateTime</c> object constructed from
        /// a JavaScript <c>Date</c> object always represents a Coordinated Universal Timestamp (UTC)
        /// and has its <see cref="DateTime.Kind"/> property set to <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        EnableDateTimeConversion = 0x00000010,

        /// <summary>
        /// Specifies that
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import#Dynamic_Imports">dynamic module imports</see>
        /// are to be enabled. This is an experimental feature and may be removed in a future release.
        /// </summary>
        EnableDynamicModuleImports = 0x00000020
    }
}
