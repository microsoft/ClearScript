// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class HostVariableTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System", "System.Core"));
            engine.AddHostObject("host", new HostFunctions());
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

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out()
        {
            var value = new Random();
            var dict = new Dictionary<string, Random> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.Random); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreSame(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            var dict = new Dictionary<string, DayOfWeek> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.DayOfWeek); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Scalar()
        {
            const double value = Math.E;
            var dict = new Dictionary<string, Double> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.Double); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Struct()
        {
            var value = DateTime.UtcNow;
            var dict = new Dictionary<string, DateTime> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.DateTime); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref()
        {
            var inputValue = new Random();
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(Random));
            Assert.AreSame(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Enum()
        {
            const DayOfWeek inputValue = DayOfWeek.Thursday;
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(DayOfWeek));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Scalar()
        {
            const double inputValue = Math.PI;
            engine.AddHostObject("test", this);
            engine.Script.inValue = inputValue;
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(Double));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Struct()
        {
            var inputValue = DateTime.UtcNow;
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(DateTime));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        // ReSharper disable UnusedMember.Local

        public T TestMethod<T>(ref T value, T outValue = default(T))
        {
            var inValue = value;
            value = outValue;
            return inValue;
        }

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
