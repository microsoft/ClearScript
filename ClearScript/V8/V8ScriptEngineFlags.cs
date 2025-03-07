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
        // IMPORTANT: maintain bitwise equivalence with unmanaged enum V8Context::Flags

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
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/import">dynamic module imports</see>
        /// are to be enabled. This is an experimental feature and may be removed in a future release.
        /// </summary>
        EnableDynamicModuleImports = 0x00000020,

        /// <summary>
        /// Specifies that 64-bit integers with values greater than
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER">Number.MAX_SAFE_INTEGER</see></c>
        /// or less than
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MIN_SAFE_INTEGER">Number.MIN_SAFE_INTEGER</see></c>
        /// are to be marshaled as
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see></c>.
        /// This option is ignored if <c><see cref="MarshalAllInt64AsBigInt"/></c> is specified.
        /// </summary>
        MarshalUnsafeInt64AsBigInt = 0x00000040,

        /// <summary>
        /// Equivalent to <c><see cref="MarshalUnsafeInt64AsBigInt"/></c>.
        /// </summary>
        [Obsolete("This option has been renamed to MarshalUnsafeInt64AsBigInt.")]
        MarshalUnsafeLongAsBigInt = MarshalUnsafeInt64AsBigInt,

        /// <summary>
        /// Specifies that all 64-bit integers are to be marshaled as
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/BigInt">BigInt</see></c>.
        /// </summary>
        MarshalAllInt64AsBigInt = 0x00000080,

        /// <summary>
        /// Equivalent to <c><see cref="MarshalAllInt64AsBigInt"/></c>.
        /// </summary>
        [Obsolete("This option has been renamed to MarshalAllInt64AsBigInt.")]
        MarshalAllLongAsBigInt = MarshalAllInt64AsBigInt,

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
        HideHostExceptions = 0x00001000,

        /// <summary>
        /// Specifies that support for synchronization contexts is to be enabled for task-promise
        /// interoperability. This option is ignored if
        /// <c><see cref="EnableTaskPromiseConversion"/></c> is not specified.
        /// </summary>
        UseSynchronizationContexts = 0x00002000,

        /// <summary>
        /// Specifies that the
        /// <c><see href="https://microsoft.github.io/ClearScript/2024/03/21/performance-api.html">Performance</see></c>
        /// object is to be added to the script engine's global namespace. This object provides a
        /// set of low-level native facilities for performance-sensitive scripts.
        /// </summary>
        AddPerformanceObject = 0x00004000,

        /// <summary>
        /// Specifies that native timers are to be set to the highest available resolution while
        /// the current <c><see cref="V8ScriptEngine"/></c> instance is active. This option is
        /// ignored if <c><see cref="AddPerformanceObject"/></c> is not specified. It is only a
        /// hint and may be ignored on some systems. On platforms that support it, this option can
        /// degrade overall system performance or power efficiency, so caution is recommended.
        /// </summary>
        SetTimerResolution = 0x00008000,

        /// <summary>
        /// Specifies that the script engine is to perform automatic conversion between .NET and
        /// JavaScript arrays. This conversion is bidirectional and lossy. A .NET array constructed
        /// from a JavaScript array always has an element type of <c><see cref="object"/></c>,
        /// making it impossible for script code to specify a strongly typed array as a .NET method
        /// argument or property value. Excessive copying of array contents can also impact
        /// application performance and/or memory consumption. Caution is recommended.
        /// </summary>
        EnableArrayConversion = 0x00010000
    }

    internal static class V8ScriptEngineFlagsHelpers
    {
        public static bool HasAllFlags(this V8ScriptEngineFlags value, V8ScriptEngineFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this V8ScriptEngineFlags value, V8ScriptEngineFlags flags) => (value & flags) != 0;
    }
}
