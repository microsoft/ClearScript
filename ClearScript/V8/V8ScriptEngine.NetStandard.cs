// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    public sealed partial class V8ScriptEngine
    {
        #region internal members

        private void CompletePromise<T>(ValueTask<T> valueTask, object resolve, object reject)
        {
            Func<T> getResult = () => valueTask.Result;
            var engineInternal = (ScriptObject)script.GetProperty("EngineInternal");
            engineInternal.InvokeMethod("completePromiseWithResult", getResult, resolve, reject);
        }

        private void CompletePromise(ValueTask valueTask, object resolve, object reject)
        {
            Action wait = () => WaitForValueTask(valueTask);
            var engineInternal = (ScriptObject)script.GetProperty("EngineInternal");
            engineInternal.InvokeMethod("completePromise", wait, resolve, reject);
        }

        private static void WaitForValueTask(ValueTask valueTask)
        {
            if (valueTask.IsCompletedSuccessfully)
            {
                return;
            }

            if (valueTask.IsCanceled)
            {
                throw new TaskCanceledException();
            }

            valueTask.AsTask().Wait();
        }

        partial void TryConvertValueTaskToPromise(object obj, Action<object> setResult)
        {
            if (obj.GetType().IsAssignableToGenericType(typeof(ValueTask<>), out var typeArgs))
            {
                setResult(typeof(ValueTaskConverter<>).MakeSpecificType(typeArgs).InvokeMember("ToPromise", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new[] { obj, this }));
            }
            else if (obj is ValueTask valueTask)
            {
                setResult(valueTask.ToPromise(this));
            }
        }

        #endregion

        #region IJavaScriptEngine implementation

        object IJavaScriptEngine.CreatePromiseForValueTask<T>(ValueTask<T> valueTask)
        {
            if (valueTask.IsCompleted)
            {
                return CreatePromise((resolve, reject) => CompletePromise(valueTask, resolve, reject));
            }

            return ((IJavaScriptEngine)this).CreatePromiseForTask(valueTask.AsTask());
        }

        object IJavaScriptEngine.CreatePromiseForValueTask(ValueTask valueTask)
        {
            if (valueTask.IsCompleted)
            {
                return CreatePromise((resolve, reject) => CompletePromise(valueTask, resolve, reject));
            }

            return ((IJavaScriptEngine)this).CreatePromiseForTask(valueTask.AsTask());
        }

        #endregion

        #region Nested type: ValueTaskConverter

        private static class ValueTaskConverter<T>
        {
            // ReSharper disable UnusedMember.Local

            public static object ToPromise(ValueTask<T> valueTask, V8ScriptEngine engine)
            {
                return valueTask.ToPromise(engine);
            }

            // ReSharper restore UnusedMember.Local
        }

        #endregion
    }
}
