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
        /// Specifies that support for <c><see cref="HostItemFlags.GlobalMembers"/></c> behavior is to be
        /// disabled. This option yields a significant performance benefit for global item access.
        /// </summary>
        DisableGlobalMembers = 0x00000002,

        /// <summary>
        /// Specifies that remote script debugging is to be enabled. This option is ignored if
        /// <c><see cref="EnableDebugging"/></c> is not specified.
        /// </summary>
        EnableRemoteDebugging = 0x00000004,

        /// <summary>
        /// Specifies that the script engine is to wait for a debugger connection and schedule a
        /// pause before executing the first line of application script code. This option is
        /// ignored if <c><see cref="EnableDebugging"/></c> is not specified.
        /// </summary>
        AwaitDebuggerAndPauseOnStart = 0x00000008,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion between
        /// .NET <c><see cref="DateTime"/></c> objects and JavaScript
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date">Date</see></c>
        /// objects. This conversion is bidirectional and lossy. A <c>DateTime</c> object
        /// constructed from a JavaScript <c>Date</c> object always represents a Coordinated
        /// Universal Time (UTC) and has its <c><see cref="DateTime.Kind"/></c> property set to
        /// <c><see cref="DateTimeKind.Utc"/></c>.
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
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER">Number.MAX_SAFE_INTEGER</see></c>
        /// or less than
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MIN_SAFE_INTEGER">Number.MIN_SAFE_INTEGER</see></c>
        /// are to be marshaled as
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see></c>.
        /// This option is ignored if <c><see cref="MarshalAllLongAsBigInt"/></c> is specified.
        /// </summary>
        MarshalUnsafeLongAsBigInt = 0x00000040,

        /// <summary>
        /// Specifies that all long integers are to be marshaled as
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see></c>.
        /// </summary>
        MarshalAllLongAsBigInt = 0x00000080,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion between
        /// .NET <c><see cref="Task"/></c> objects and JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promises</see>.
        /// This conversion is bidirectional and lossy. A <c>Task</c> object constructed from a
        /// JavaScript promise always has a result type of <c><see cref="object"/></c>.
        /// </summary>
        EnableTaskPromiseConversion = 0x00000100,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion from
        /// .NET
        /// <c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask">ValueTask</see></c> and
        /// <c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1">ValueTask&lt;TResult&gt;</see></c>
        /// structures to JavaScript
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promises</see>.
        /// This conversion is unidirectional and lossy. This option is ignored if
        /// <c><see cref="EnableTaskPromiseConversion"/></c> is not specified.
        /// </summary>
        EnableValueTaskPromiseConversion = 0x00000200,

        /// <summary>
        /// Specifies that access to host object and class members is to be case-insensitive. This
        /// option can introduce ambiguity if the host resource has distinct members whose names
        /// differ only in case, so it should be used with caution.
        /// </summary>
        UseCaseInsensitiveMemberBinding = 0x00000400,

        /// <summary>
        /// Specifies that
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify">JSON.stringify</see></c>
        /// enhancements are to be enabled. These enhancements add support for host objects via the
        /// <see href="https://www.newtonsoft.com/json">Json.NET</see> library.
        /// </summary>
        EnableStringifyEnhancements = 0x00000800,

        /// <summary>
        /// Specifies that host exceptions are to be hidden from script code. If an exception
        /// thrown by the host reaches the script engine, it is caught automatically, and an
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Error">Error</see></c>
        /// object is thrown in its place. By default, ClearScript makes the managed exception
        /// accessible to script code via the <c>Error</c> object's <c>hostException</c> property.
        /// This option suppresses that behavior.
        /// </summary>
        HideHostExceptions = 0x00001000
    }
}
