// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
        /// <see cref="V8ScriptEngine"/> or <see cref="V8Runtime"/> for the first time. Subsequent
        /// reassignment will have no effect.
        /// </para>
        /// </remarks>
        public static bool EnableTopLevelAwait { get; set; }
    }
}
