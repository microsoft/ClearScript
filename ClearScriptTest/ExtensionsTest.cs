// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class ExtensionsTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostType(typeof(Extensions));
            engine.AddHostType(typeof(JavaScriptExtensions));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_ToHostType()
        {
            engine.Script.ClrMath = typeof(Math).ToHostType(engine);
            Assert.AreEqual(Math.PI, engine.Evaluate("ClrMath.PI"));

            engine.Script.randomType = typeof(Random);
            Assert.IsInstanceOfType(engine.Evaluate("new (randomType.ToHostType())"), typeof(Random));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_ToRestrictedHostObject()
        {
            IConvertible convertible = 123;
            engine.Script.convertible = convertible.ToRestrictedHostObject(engine);
            Assert.AreEqual(TypeCode.Int32, engine.Evaluate("convertible.GetTypeCode()"));

            engine.AddHostType(typeof(IConvertible));
            Assert.AreEqual(TypeCode.Int32, engine.Evaluate("Extensions.ToRestrictedHostObject(IConvertible, 456).GetTypeCode()"));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise()
        {
            engine.Script.promise = Task.FromResult(Math.PI).ToPromise(engine);
            engine.Execute("(async function () { result = await promise; })()"); 
            Assert.AreEqual(Math.PI, engine.Script.result);

            engine.Script.task = Task.FromResult(Math.E);
            engine.Execute("(async function () { result = await task.ToPromise(); })()");
            Assert.AreEqual(Math.E, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise_Faulted()
        {
            const string message = "No task for you!";
            var task = Task.FromException<double>(new UnauthorizedAccessException(message));

            engine.Script.promise = task.ToPromise(engine);
            engine.Execute("(async function () { try { result = await promise; } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);

            engine.Script.task = task;
            engine.Execute("(async function () { try { result = await task.ToPromise(); } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise_Canceled()
        {
            var message = new TaskCanceledException().Message;
            var task = Task.FromCanceled<double>(new CancellationToken(true));

            engine.Script.promise = task.ToPromise(engine);
            engine.Execute("(async function () { try { result = await promise; } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);

            engine.Script.task = task;
            engine.Execute("(async function () { try { result = await task.ToPromise(); } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise_NoResult()
        {
            engine.Script.promise = RunAsTask(() => engine.Script.result = Math.PI).ToPromise(engine);
            engine.Execute("(async function () { await promise; })()");
            Assert.AreEqual(Math.PI, engine.Script.result);

            engine.Script.task = RunAsTask(() => engine.Script.result = Math.E);
            engine.Execute("(async function () { await task.ToPromise(); })()");
            Assert.AreEqual(Math.E, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise_NoResult_Faulted()
        {
            const string message = "No task for you!";
            var task = Task.FromException(new UnauthorizedAccessException(message));

            engine.Script.promise = task.ToPromise(engine);
            engine.Execute("(async function () { try { result = await promise; } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);

            engine.Script.task = task;
            engine.Execute("(async function () { try { result = await task.ToPromise(); } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToPromise_NoResult_Canceled()
        {
            var message = new TaskCanceledException().Message;
            var task = Task.FromCanceled(new CancellationToken(true));

            engine.Script.promise = task.ToPromise(engine);
            engine.Execute("(async function () { try { result = await promise; } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);

            engine.Script.task = task;
            engine.Execute("(async function () { try { result = await task.ToPromise(); } catch (exception) { result = exception.hostException.InnerException.InnerException.Message; } })()");
            Assert.AreEqual(message, engine.Script.result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToTask()
        {
            engine.AddHostType(typeof(Task));
            Assert.AreEqual(Math.PI, EvaluateAsync(@"(async function () { await Task.Delay(100).ToPromise(); return Math.PI; })()").Result);
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extensions_JavaScript_ToTask_Fail()
        {
            engine.AddHostType(typeof(Task));
            TestUtil.AssertException<ScriptEngineException>(() => EvaluateAsync(@"(async function () { await Task.Delay(100).ToPromise(); throw new Error('Unauthorized'); })()").Wait());
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private static Task RunAsTask(Action action)
        {
            var task = Task.Run(action);
            task.Wait();
            return task;
        }

        private async Task<object> EvaluateAsync(string code)
        {
            return await engine.Evaluate(code).ToTask();
        }

        #endregion
    }
}
