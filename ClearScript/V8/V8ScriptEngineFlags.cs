// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;

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
        /// objects. This conversion is bidirectional and lossy. A <c>DateTime</c> object
        /// constructed from a JavaScript <c>Date</c> object always represents a Coordinated
        /// Universal Timestamp (UTC) and has its <see cref="DateTime.Kind"/> property set to
        /// <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        EnableDateTimeConversion = 0x00000010,

        /// <summary>
        /// Specifies that
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/import#Dynamic_Imports">dynamic module imports</see>
        /// are to be enabled. This is an experimental feature and may be removed in a future release.
        /// </summary>
        EnableDynamicModuleImports = 0x00000020,

        /// <summary>
        /// Specifies that long integers with values greater than
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER">Number.MAX_SAFE_INTEGER</see>
        /// or less than
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MIN_SAFE_INTEGER">Number.MIN_SAFE_INTEGER</see>
        /// are to be marshaled as
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see>.
        /// This option is ignored if <see cref="MarshalAllLongAsBigInt"/> is specified.
        /// </summary>
        MarshalUnsafeLongAsBigInt = 0x00000040,

        /// <summary>
        /// Specifies that all long integers are to be marshaled as
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see>.
        /// </summary>
        MarshalAllLongAsBigInt = 0x00000080,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion between
        /// .NET <see cref="Task"/> objects and JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promises</see>.
        /// This conversion is bidirectional and lossy. A <c>Task</c> object constructed from a
        /// JavaScript promise always has a result type of <see cref="object"/>.
        /// </summary>
        EnableTaskPromiseConversion = 0x00000100,

    #if NETFRAMEWORK || UWP

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion from
        /// .NET <c>ValueTask</c> and <c>ValueTask&lt;TResult&gt;</c> structures to JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promises</see>.
        /// This conversion is unidirectional and lossy. This option is ignored if
        /// <see cref="EnableTaskPromiseConversion"/> is not specified.
        /// </summary>
        EnableValueTaskPromiseConversion = 0x00000200,

    #else

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion from
        /// .NET <see cref="ValueTask"/> and <see cref="ValueTask{T}"/> structures to JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promises</see>.
        /// This conversion is unidirectional and lossy. This option is ignored if
        /// <see cref="EnableTaskPromiseConversion"/> is not specified.
        /// </summary>
        EnableValueTaskPromiseConversion = 0x00000200,

    #endif

        /// <summary>
        /// Specifies that access to host object and class members is to be case-insensitive. This
        /// option can introduce ambiguity if the host resource has distinct members whose names
        /// differ only in case, so it should be used with caution.
        /// </summary>
        UseCaseInsensitiveMemberBinding = 0x00000400
    }
}
