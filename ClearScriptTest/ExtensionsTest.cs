// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    [DeploymentItem("v8-base-x64.dll")]
    [DeploymentItem("v8-base-ia32.dll")]
    [DeploymentItem("v8-libcpp-x64.dll")]
    [DeploymentItem("v8-libcpp-ia32.dll")]
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
        public void Extensions_JavaScript_ToPromise_NoResult()
        {
            engine.Script.promise = RunAsTask(() => engine.Script.result = Math.PI).ToPromise(engine);
            engine.Execute("(async function () { await promise; })()");
            Assert.AreEqual(Math.PI, engine.Script.result);

            engine.Script.task = RunAsTask(() => engine.Script.result = Math.E);
            engine.Execute("(async function () { await task.ToPromise(); })()");
            Assert.AreEqual(Math.E, engine.Script.result);
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

        #endregion
    }
}
