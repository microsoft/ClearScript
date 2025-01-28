// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines properties that comprise ClearScript's V8 configuration.
    /// </summary>
    public static class V8Settings
    {
        /// <summary>
        /// Enables or disables Top-Level Await.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see href="https://github.com/tc39/proposal-top-level-await">Top-Level Await</see>
        /// enables code at the outermost scope of an
        /// <see href="https://www.ecma-international.org/ecma-262/6.0/#sec-modules">ECMAScript 6</see>
        /// module to be executed as an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Statements/async_function">async function</see>.
        /// When this feature is enabled, modules can <c>await</c> resources, causing importers to
        /// delay evaluation as necessary.
        /// </para>
        /// <para>
        /// To enable Top-Level Await, set this property to <c>true</c> before instantiating
        /// <c><see cref="V8ScriptEngine"/></c> or <c><see cref="V8Runtime"/></c> for the first time. Subsequent
        /// reassignment will have no effect.
        /// </para>
        /// </remarks>
        [Obsolete("V8 no longer supports Top-Level Await control. The feature is always enabled.")]
        public static bool EnableTopLevelAwait { get; set; }

        /// <summary>
        /// Gets or sets global V8 options.
        /// </summary>
        /// <remarks>
        /// To override the default global options, set this property before instantiating
        /// <c><see cref="V8ScriptEngine"/></c> or <c><see cref="V8Runtime"/></c> for the first time. Subsequent
        /// reassignment will have no effect.
        /// </remarks>
        public static V8GlobalFlags GlobalFlags { get; set; }
    }
}
