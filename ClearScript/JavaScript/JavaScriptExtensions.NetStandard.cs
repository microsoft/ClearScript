// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    public static partial class JavaScriptExtensions
    {
        /// <summary>
        /// Converts a <c><see cref="ValueTask{T}"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <typeparam name="T">The task's result type.</typeparam>
        /// <param name="valueTask">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        /// <remarks>
        /// This method is not available on .NET Framework or Universal Windows Platform (UWP).
        /// </remarks>
        public static object ToPromise<T>(this ValueTask<T> valueTask)
        {
            return valueTask.ToPromise(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a <c><see cref="ValueTask{T}"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <typeparam name="T">The task's result type.</typeparam>
        /// <param name="valueTask">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        /// <remarks>
        /// This method is not available on .NET Framework or Universal Windows Platform (UWP).
        /// </remarks>
        public static object ToPromise<T>(this ValueTask<T> valueTask, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(valueTask, nameof(valueTask));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine == null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreatePromiseForValueTask(valueTask);
        }

        /// <summary>
        /// Converts a <c><see cref="ValueTask"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <param name="valueTask">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        /// <remarks>
        /// This method is not available on .NET Framework or Universal Windows Platform (UWP).
        /// </remarks>
        public static object ToPromise(this ValueTask valueTask)
        {
            return valueTask.ToPromise(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a <c><see cref="ValueTask"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <param name="valueTask">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        /// <remarks>
        /// This method is not available on .NET Framework or Universal Windows Platform (UWP).
        /// </remarks>
        public static object ToPromise(this ValueTask valueTask, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(valueTask, nameof(valueTask));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine == null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreatePromiseForValueTask(valueTask);
        }
    }
}
