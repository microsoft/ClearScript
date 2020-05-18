using Microsoft.ClearScript.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Test
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
					if (thisTask.IsCanceled) reject(thisTask);
					else if (thisTask.IsFaulted) reject(thisTask);
					else if (thisTask.IsCompleted) resolve(thisTask.Result);
					else reject(thisTask);
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
					if (thisTask.IsCanceled) reject(thisTask);
					else if (thisTask.IsFaulted) reject(thisTask);
					else if (thisTask.IsCompleted) resolve();
					else reject(thisTask);
				}, TaskContinuationOptions.ExecuteSynchronously);
			}));
		}


		/// <summary>
		/// Converts a JavaScript promise instance into a Task
		/// <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise">promise</see>
		/// </summary>
		/// <param name="promise">The promise to convert into a Task.</param>
		/// <returns>A Task that represents the Promise's asynchronous operation.</returns>
		public static Task<object> ToTask(this object promise)
		{
			var source = new TaskCompletionSource<object>();
			((dynamic)promise).then(
				new Action<object>(result => source.SetResult(result)),
				new Action<dynamic>(rejected =>
				{
					var t = rejected as Task;
					if (t != null)
					{
						if (t.IsCanceled) source.SetCanceled();
						else if (t.Exception != null) source.SetException(t.Exception);
					}
					else
					{
						source.SetException(new Exception(rejected.ToString()));
					}
				})
			);
			return source.Task;
		}

	}
}
