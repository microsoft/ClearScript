// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
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
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class DynamicHostItemTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
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

        [TestMethod, TestCategory("DynamicHostItem")]
        public void DynamicHostItem_StaticMethod()
        {
            const string format = "{0} foo {1} bar {2} baz {3} qux {4} quux {5}";
            var args = new object[] { 1, 2, 3, 4, 5, 6 };
            engine.Script.mscorlib = new HostTypeCollection("mscorlib");
            Assert.AreEqual(string.Format(format, args), engine.Script.mscorlib.System.String.Format(format, args));
            Assert.AreEqual(string.Format(format, args), engine.Script.mscorlib["System"].String["Format"](format, args));
        }

        [TestMethod, TestCategory("DynamicHostItem")]
        public void DynamicHostItem_Conversion()
        {
            var hostTypeCollection = new HostTypeCollection("mscorlib");
            engine.Script.mscorlib = hostTypeCollection;
            Assert.AreEqual(((PropertyBag)hostTypeCollection["System"]).Keys.Count, ((PropertyBag)engine.Script.mscorlib.System).Keys.Count);
            Assert.AreEqual(((PropertyBag)hostTypeCollection["System"]).Keys.Count, ((PropertyBag)engine.Script["mscorlib"]["System"]).Keys.Count);
        }

        [TestMethod, TestCategory("DynamicHostItem")]
        public void DynamicHostItem_Nonexistent()
        {
            engine.Script.mscorlib = new HostTypeCollection("mscorlib");
            Assert.IsInstanceOfType(engine.Script.mscorlib.BogusNonexistentProperty, typeof(Undefined));
            Assert.IsInstanceOfType(engine.Script["mscorlib"]["BogusNonexistentProperty"], typeof(Undefined));
        }

        [TestMethod, TestCategory("DynamicHostItem")]
        public void DynamicHostItem_CrossEngine()
        {
            const string format = "{0} foo {1} bar {2} baz {3} qux {4} quux {5}";
            var args = new object[] { 1, 2, 3, 4, 5, 6 };
            engine.Script.mscorlib = new HostTypeCollection("mscorlib");

            using (var outerEngine = new JScriptEngine())
            {
                outerEngine.Script.inner = engine;
                outerEngine.Script.format = format;
                outerEngine.Script.args = args;
                Assert.AreEqual(string.Format(format, args), outerEngine.Evaluate("inner.Script.mscorlib.System.String.Format(format, args)"));
                Assert.AreEqual(string.Format(format, args), outerEngine.Evaluate("inner.Script.mscorlib['System'].String['Format'](format, args)"));
            }
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
