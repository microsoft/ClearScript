// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Defines options for V8 runtime behavior in response to a violation.
    /// </summary>
    /// <c><seealso cref="V8Runtime.MaxHeapSize"/></c>
    /// <c><seealso cref="V8ScriptEngine.MaxRuntimeHeapSize"/></c>
    public enum V8RuntimeViolationPolicy
    {
        /// <summary>
        /// Specifies that the runtime is to interrupt script execution and throw a managed
        /// exception. Additionally, further script execution is to be blocked until the host sets
        /// the exceeded limit to a new value. This is the default behavior and the most effective
        /// option for preventing process termination.
        /// </summary>
        Interrupt,

        /// <summary>
        /// Specifies that the runtime is to throw a script exception and disable monitoring until
        /// the host sets the exceeded limit to a new value. This option is less effective at
        /// preventing process termination than <c><see cref="Interrupt"/></c>, but it is more friendly to
        /// asynchronous JavaScript, which relies on post-error processing for mechanisms such as
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// rejection.
        /// </summary>
        Exception
    }
}
