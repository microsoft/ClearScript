// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// Supports managed asynchronous iteration over an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols#the_async_iterator_and_async_iterable_protocols">async-iterable</see>
        /// script object.
        /// </summary>
        /// <param name="asyncIterable">An async-iterable script object (see remarks).</param>
        /// <returns>An <c><see cref="IAsyncEnumerable{Object}">IAsyncEnumerator&lt;Object&gt;</see></c> implementation that supports managed asynchronous iteration over <paramref name="asyncIterable"/>.</returns>
        /// <remarks>
        /// If the argument implements <c><see cref="IAsyncEnumerable{Object}">IAsyncEnumerator&lt;Object&gt;</see></c>, this method returns it as is.
        /// </remarks>
        public static IAsyncEnumerable<object> ToAsyncEnumerable(this object asyncIterable)
        {
            MiscHelpers.VerifyNonNullArgument(asyncIterable, nameof(asyncIterable));
            return asyncIterable as IAsyncEnumerable<object> ?? asyncIterable.ToAsyncEnumerableInternal();
        }

        private static async IAsyncEnumerable<object> ToAsyncEnumerableInternal(this object asyncIterable)
        {
            if (asyncIterable is IEnumerable<object> objectEnumerable)
            {
                foreach (var item in objectEnumerable)
                {
                    yield return item;
                }
            }
            else if (asyncIterable is ScriptObject scriptObject)
            {
                if (scriptObject.Engine is IJavaScriptEngine javaScriptEngine && (javaScriptEngine.BaseLanguageVersion >= 6))
                {
                    var engineInternal = (ScriptObject)javaScriptEngine.Global["EngineInternal"];
                    if (engineInternal.InvokeMethod("getAsyncIterator", scriptObject) is ScriptObject asyncIterator)
                    {
                        while (await asyncIterator.InvokeMethod("next").ToTask().ConfigureAwait(false) is ScriptObject result && !Equals(result["done"], true))
                        {
                            yield return result["value"];
                        }
                    }
                    else if (engineInternal.InvokeMethod("getIterator", scriptObject) is ScriptObject iterator)
                    {
                        while (iterator.InvokeMethod("next") is ScriptObject result && !Equals(result["done"], true))
                        {
                            yield return result["value"];
                        }
                    }
                    else
                    {
                        throw new ArgumentException("The object is not async-iterable", nameof(asyncIterable));
                    }
                }
                else
                {
                    throw new NotSupportedException("The script engine does not support async iteration");
                }
            }
            else if (asyncIterable is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
            else
            {
                throw new ArgumentException("The object is not async-iterable", nameof(asyncIterable));
            }
        }
    }
}
