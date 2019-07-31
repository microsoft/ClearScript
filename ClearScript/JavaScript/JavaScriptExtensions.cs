// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines extension methods for use with JavaScript engines.
    /// </summary>
    public static class JavaScriptExtensions
    {
        private delegate void Executor(dynamic resolve, dynamic reject);

        /// <summary>
        /// Converts a <see cref="Task{TResult}"/> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code currently running on the calling thread.
        /// </summary>
        /// <typeparam name="TResult">The task's result type.</typeparam>
        /// <param name="task">The task to convert to a promise.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise<TResult>(this Task<TResult> task)
        {
            return task.ToPromise(ScriptEngine.Current);
        }

        /// <summary>
        /// Converts a <see cref="Task{TResult}"/> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <typeparam name="TResult">The task's result type.</typeparam>
        /// <param name="task">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise<TResult>(this Task<TResult> task, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(task, "task");
            MiscHelpers.VerifyNonNullArgument(engine, "engine");

            var ctor = (ScriptObject)engine.Script.Promise;
            return ctor.Invoke(true, new Executor((resolve, reject) =>
            {
                task.ContinueWith(thisTask =>
                {
                    if (thisTask.IsCompleted)
                    {
                        resolve(thisTask.Result);
                    }
                    else
                    {
                        reject(thisTask.Exception);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }));
        }

        /// <summary>
        /// Converts a <see cref="Task"/> instance to a
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
        /// Converts a <see cref="Task"/> instance to a
        /// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
        /// for use with script code running in the specified script engine.
        /// </summary>
        /// <param name="task">The task to convert to a promise.</param>
        /// <param name="engine">The script engine in which the promise will be used.</param>
        /// <returns>A promise that represents the task's asynchronous operation.</returns>
        public static object ToPromise(this Task task, ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(task, "task");
            MiscHelpers.VerifyNonNullArgument(engine, "engine");

            var ctor = (ScriptObject)engine.Script.Promise;
            return ctor.Invoke(true, new Executor((resolve, reject) =>
            {
                task.ContinueWith(thisTask =>
                {
                    if (thisTask.IsCompleted)
                    {
                        resolve();
                    }
                    else
                    {
                        reject(thisTask.Exception);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }));
        }
    }
}
