// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    // ReSharper disable once PartialTypeWithSinglePart

    /// <summary>
    /// Defines extension methods for use with JavaScript engines.
    /// </summary>
    public static partial class JavaScriptExtensions
    {
        /// <summary>
        /// Converts a <c><see cref="Task{T}"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <typeparam name="T">The task's result type.</typeparam>
        /// <param name="task">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise<T>(this Task<T> task)
        {
            return task.ToPromise(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a <c><see cref="Task{T}"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <typeparam name="T">The task's result type.</typeparam>
        /// <param name="task">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise<T>(this Task<T> task, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(task, nameof(task));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine is null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreatePromiseForTask(task);
        }

        /// <summary>
        /// Converts a <c><see cref="Task"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <param name="task">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise(this Task task)
        {
            return task.ToPromise(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a <c><see cref="Task"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <param name="task">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise(this Task task, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(task, nameof(task));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine is null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreatePromiseForTask(task);
        }

        /// <summary>
        /// Converts a <c><see cref="ValueTask{T}"/></c> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <typeparam name="T">The task's result type.</typeparam>
        /// <param name="valueTask">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
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
        public static object ToPromise<T>(this ValueTask<T> valueTask, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(valueTask, nameof(valueTask));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine is null) || (javaScriptEngine.BaseLanguageVersion < 6))
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
        public static object ToPromise(this ValueTask valueTask, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(valueTask, nameof(valueTask));
            MiscHelpers.VerifyNonNullArgument(engine, nameof(engine));

            var javaScriptEngine = engine as IJavaScriptEngine;
            if ((javaScriptEngine is null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreatePromiseForValueTask(valueTask);
        }

        /// <summary>
        /// Converts a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// to a <c><see cref="Task{Object}">Task&lt;Object&gt;</see></c> instance.
        /// </summary>
        /// <param name="promise">The promise to convert to a task (see remarks).</param>
        /// <returns>A task that represents the promise's asynchronous operation.</returns>
        /// <remarks>
        /// If the argument is a <c><see cref="Task{Object}">Task&lt;Object&gt;</see></c> instance,
        /// this method returns it as is.
        /// </remarks>
        public static Task<object> ToTask(this object promise)
        {
            MiscHelpers.VerifyNonNullArgument(promise, nameof(promise));
            return promise as Task<object> ?? promise.ToTaskInternal();
        }

        /// <summary>
        /// Supports managed iteration over an
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Iteration_protocols#the_iterable_protocol">iterable</see>
        /// script object.
        /// </summary>
        /// <param name="iterable">An iterable script object (see remarks).</param>
        /// <returns>An <c><see cref="IEnumerable{Object}">IEnumerable&lt;Object&gt;</see></c> implementation that supports managed iteration over <paramref name="iterable"/>.</returns>
        /// <remarks>
        /// If the argument implements
        /// <c><see cref="IEnumerable{Object}">IEnumerable&lt;Object&gt;</see></c>, this method
        /// returns it as is.
        /// </remarks>
        public static IEnumerable<object> ToEnumerable(this object iterable)
        {
            // WARNING: The IEnumerable<object> test below is a bit dicey, as most IEnumerable<T>
            // implementations support IEnumerable<object> via covariance. The desired behavior
            // here is for that test to fail for IDictionary<TKey, TValue>, as V8 script objects
            // now support that interface. Luckily, that test does fail, but only because
            // KeyValuePair<TKey, TValue> is a struct, so covariance doesn't apply.

            MiscHelpers.VerifyNonNullArgument(iterable, nameof(iterable));
            return iterable as IEnumerable<object> ?? iterable.ToEnumerableInternal();
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

        private static Task<object> ToTaskInternal(this object promise)
        {
            var scriptObject = promise as ScriptObject;
            if (scriptObject is null)
            {
                throw new ArgumentException("The object is not a promise", nameof(promise));
            }

            var javaScriptEngine = scriptObject.Engine as IJavaScriptEngine;
            if ((javaScriptEngine is null) || (javaScriptEngine.BaseLanguageVersion < 6))
            {
                throw new NotSupportedException("The script engine does not support promises");
            }

            return javaScriptEngine.CreateTaskForPromise(scriptObject);
        }

        private static IEnumerable<object> ToEnumerableInternal(this object iterable)
        {
            if (iterable is ScriptObject scriptObject)
            {
                if (scriptObject.Engine is IJavaScriptEngine javaScriptEngine && (javaScriptEngine.BaseLanguageVersion >= 6))
                {
                    var engineInternal = (ScriptObject)javaScriptEngine.Global["EngineInternal"];
                    if (engineInternal.InvokeMethod("getIterator", scriptObject) is ScriptObject iterator)
                    {
                        while (iterator.InvokeMethod("next") is ScriptObject result && !Equals(result["done"], true))
                        {
                            yield return result["value"];
                        }
                    }
                    else
                    {
                        throw new ArgumentException("The object is not iterable", nameof(iterable));
                    }
                }
                else
                {
                    throw new NotSupportedException("The script engine does not support iteration");
                }
            }
            else if (iterable is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    yield return item;
                }
            }
            else
            {
                throw new ArgumentException("The object is not iterable", nameof(iterable));
            }
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
