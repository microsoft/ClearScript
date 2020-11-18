// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public partial class PropertyBagTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

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

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Property()
        {
            var host = new HostFunctions();
            var bag = new PropertyBag { { "host", host } };
            engine.AddHostObject("bag", bag);
            Assert.AreSame(host, engine.Evaluate("bag.host"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Property_Scalar()
        {
            const int value = 123;
            var bag = new PropertyBag { { "value", value } };
            engine.AddHostObject("bag", bag);
            Assert.AreEqual(value, engine.Evaluate("bag.value"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Property_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            var bag = new PropertyBag { { "date", date } };
            engine.AddHostObject("bag", bag);
            Assert.AreEqual(date, engine.Evaluate("bag.date"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Property_GlobalMembers()
        {
            var host = new HostFunctions();
            var bag = new PropertyBag { { "host", host } };
            engine.AddHostObject("bag", HostItemFlags.GlobalMembers, bag);
            Assert.AreSame(host, engine.Evaluate("host"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_HostDelegate()
        {
            var methodInvoked = false;
            Action method = () => methodInvoked = true;
            var bag = new PropertyBag { { "method", method } };
            engine.AddHostObject("bag", bag);
            engine.Execute("bag.method()");
            Assert.IsTrue(methodInvoked);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ScriptProperty()
        {
            engine.Execute("foo = { bar: false }");
            var bag = new PropertyBag { { "foo", engine.Script.foo } };
            engine.AddHostObject("bag", bag);
            engine.Execute("bag.foo.bar = true");
            Assert.IsTrue(engine.Script.foo.bar);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ScriptMethod()
        {
            engine.Execute("methodInvoked = false; function method() { methodInvoked = true }");
            var bag = new PropertyBag { { "method", engine.Script.method } };
            engine.AddHostObject("bag", bag);
            engine.Execute("bag.method()");
            Assert.IsTrue(engine.Script.methodInvoked);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_IScriptableObject_OnExpose()
        {
            var host = new HostFunctions();
            var bag = new PropertyBag { { "host", host } };
            engine.AddHostObject("bag", bag);
            Assert.AreSame(engine, host.GetEngine());
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_IScriptableObject_OnModify()
        {
            var bag = new PropertyBag();
            engine.AddHostObject("bag", bag);
            var host = new HostFunctions();
            bag.Add("host", host);
            Assert.AreSame(engine, host.GetEngine());
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Expando()
        {
            var bag = new PropertyBag();
            engine.AddHostObject("bag", bag);
            engine.Execute("bag.foo = 123");
            Assert.AreEqual(123, bag["foo"]);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Writable()
        {
            var bag = new PropertyBag { { "foo", false } };
            engine.AddHostObject("bag", bag);
            engine.Execute("bag.foo = true");
            Assert.IsTrue((bool)bag["foo"]);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Writable_Delete()
        {
            var bag = new PropertyBag { { "foo", false } };
            engine.AddHostObject("bag", bag);
            engine.Execute("delete bag.foo");
            Assert.IsFalse(bag.ContainsKey("foo"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ReadOnly()
        {
            var bag = new PropertyBag(true);
            bag.SetPropertyNoCheck("foo", false);
            engine.AddHostObject("bag", bag);
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("bag.foo = true"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ReadOnly_Delete()
        {
            var bag = new PropertyBag(true);
            bag.SetPropertyNoCheck("foo", false);
            engine.AddHostObject("bag", bag);
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("delete bag.foo"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ExternalModification_AddProperty()
        {
            var bag = new PropertyBag { { "foo", 123 } };
            engine.AddHostObject("bag", bag);
            Assert.AreEqual(123, engine.Evaluate("bag.foo"));
            bag.Add("bar", 456);
            Assert.AreEqual(456, engine.Evaluate("bag.bar"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ExternalModification_ChangeProperty()
        {
            var bag = new PropertyBag { { "foo", 123 } };
            engine.AddHostObject("bag", bag);
            Assert.AreEqual(123, engine.Evaluate("bag.foo"));
            bag["foo"] = 456;
            Assert.AreEqual(456, engine.Evaluate("bag.foo"));
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_ExternalModification_DeleteProperty()
        {
            var bag = new PropertyBag { { "foo", 123 }, { "bar", 456 } };
            engine.AddHostObject("bag", bag);
            Assert.AreEqual(456, engine.Evaluate("bag.bar"));
            bag.Remove("bar");
            Assert.AreSame(Undefined.Value, engine.Evaluate("bag.bar"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
