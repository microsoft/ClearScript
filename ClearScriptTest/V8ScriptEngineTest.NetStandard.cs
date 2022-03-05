// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Microsoft.ClearScript.Test
{
    // ReSharper disable once PartialTypeWithSinglePart

    public partial class V8ScriptEngineTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ValueTaskPromiseConversion()
        {
            engine.Script.value = new ValueTask<string>("foo");
            Assert.AreEqual("HostObject", engine.Evaluate("value.constructor.name"));
            Assert.IsInstanceOfType(engine.Evaluate("Promise.resolve(123)"), typeof(ScriptObject));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableTaskPromiseConversion | V8ScriptEngineFlags.EnableValueTaskPromiseConversion);

            engine.Script.value = new ValueTask<string>("bar");
            Assert.AreEqual("Promise", engine.Evaluate("value.constructor.name"));
            Assert.IsInstanceOfType(engine.Evaluate("Promise.resolve(123)"), typeof(Task));

            var task = new Func<Task<object>>(async () => await (Task<object>)engine.Evaluate("Promise.resolve(123)"))();
            Assert.AreEqual(123, task.Result);

            engine.Script.promise = new ValueTask<int>(456);
            engine.Execute("promise.then(value => result = value);");
            Assert.AreEqual(456, engine.Script.result);

            var cancelSource = new CancellationTokenSource();
            cancelSource.Cancel();
            engine.Script.promise = new ValueTask<string>(Task<string>.Factory.StartNew(() => "baz", cancelSource.Token));
            Thread.Sleep(250);
            engine.Execute("promise.then(value => result = value, value => error = value);");
            Assert.IsInstanceOfType(engine.Script.error.hostException.GetBaseException(), typeof(TaskCanceledException));

            cancelSource = new CancellationTokenSource();
            engine.Script.promise = new ValueTask<double>(Task<double>.Factory.StartNew(() => throw new AmbiguousImplementationException(), cancelSource.Token));
            Thread.Sleep(250);
            engine.Execute("promise.then(value => result = value, value => error = value);");
            Assert.IsInstanceOfType(engine.Script.error.hostException.GetBaseException(), typeof(AmbiguousImplementationException));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ValueTaskPromiseConversion_NoResult()
        {
            engine.Script.value = new ValueTask(Task.CompletedTask);
            Assert.AreEqual("HostObject", engine.Evaluate("value.constructor.name"));
            Assert.IsInstanceOfType(engine.Evaluate("Promise.resolve(123)"), typeof(ScriptObject));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableTaskPromiseConversion | V8ScriptEngineFlags.EnableValueTaskPromiseConversion);

            engine.Script.value = new ValueTask(Task.CompletedTask);
            Assert.AreEqual("Promise", engine.Evaluate("value.constructor.name"));
            Assert.IsInstanceOfType(engine.Evaluate("Promise.resolve(123)"), typeof(Task));

            var task = new Func<Task<object>>(async () => await (Task<object>)engine.Evaluate("Promise.resolve(123)"))();
            Assert.AreEqual(123, task.Result);

            engine.Script.promise = new ValueTask(Task.CompletedTask);
            engine.Execute("promise.then(value => result = value);");
            Assert.IsInstanceOfType(engine.Script.result, typeof(Undefined));

            var cancelSource = new CancellationTokenSource();
            cancelSource.Cancel();
            engine.Script.promise = new ValueTask(Task.Factory.StartNew(() => {}, cancelSource.Token));
            Thread.Sleep(250);
            engine.Execute("promise.then(value => result = value, value => error = value);");
            Assert.IsInstanceOfType(engine.Script.error.hostException.GetBaseException(), typeof(TaskCanceledException));

            cancelSource = new CancellationTokenSource();
            engine.Script.promise = new ValueTask(Task.Factory.StartNew(() => throw new AmbiguousImplementationException(), cancelSource.Token));
            Thread.Sleep(250);
            engine.Execute("promise.then(value => result = value, value => error = value);");
            Assert.IsInstanceOfType(engine.Script.error.hostException.GetBaseException(), typeof(AmbiguousImplementationException));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_NativeEnumerator_Disposal_AsyncSource()
        {
            var source = TestEnumerable.CreateAsync("foo", "bar", "baz");

            engine.Script.done = new ManualResetEventSlim();
            engine.AddRestrictedHostObject("source", source);
            engine.Execute(@"
                result = '';
                (async function () {
                    for await (let item of source) {
                        result += item;
                    }
                    done.Set();
                })();
            ");
            engine.Script.done.Wait();

            Assert.AreEqual("foobarbaz", engine.Script.result);
            Assert.AreEqual(1, ((TestEnumerable.IDisposableEnumeratorFactory)source).DisposedEnumeratorCount);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AsyncIteration_AsyncEnumerable()
        {
            static async IAsyncEnumerable<object> GetItems()
            {
                await Task.Delay(10);
                yield return 123;
                await Task.Delay(10);
                yield return "blah";
            }

            engine.Script.done = new ManualResetEventSlim();
            engine.Script.enumerable = GetItems();
            engine.Execute(@"
                result = '';
                (async function () {
                    for await (var item of enumerable) {
                        result += item;
                    }
                    done.Set();
                })();
            ");
            engine.Script.done.Wait();

            var result = (string)engine.Script.result;
            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AsyncIteration_AsyncEnumerable_Exception()
        {
            const string errorMessage = "Well, this is bogus!";
            static async IAsyncEnumerable<object> GetItems()
            {
                await Task.Delay(10);
                yield return 123;
                await Task.Delay(10);
                yield return "blah";
                throw new InvalidOperationException(errorMessage);
            }

            engine.Script.done = new ManualResetEventSlim();
            engine.Script.enumerable = GetItems();
            engine.Execute(@"
                result = '';
                (async function () {
                    try {
                        for await (var item of enumerable) {
                            result += item;
                        }
                    }
                    catch (error) {
                        errorMessage = error.message;
                        throw error;
                    }
                    finally {
                        done.Set();
                    }
                })();
            ");
            engine.Script.done.Wait();

            var result = (string)engine.Script.result;
            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
            Assert.AreEqual(errorMessage, engine.Script.errorMessage);
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
