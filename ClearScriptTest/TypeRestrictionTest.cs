// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
    public class TypeRestrictionTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.Script.testContainer = new TestContainer();
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

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Field()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Field)"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceField)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectField)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedField)"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceField)"));
        }

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Property()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Property)"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceProperty)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectProperty)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedProperty)"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceProperty)"));
        }

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Method()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Method())"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceMethod())"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectMethod())"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedMethod())"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceMethod())"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        public interface ITestInterface
        {
        }

        public class TestObject : ITestInterface
        {
        }

        public class TestContainer
        {
            public const int ObjectTestValue = 123456;
            public const string InterfaceTestValue = "fooBARbazQUX";

            public TestObject Field = new TestObject();
            public ITestInterface InterfaceField = new TestObject();
            public object ObjectField = new TestObject();
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedField = new TestObject();

            public TestObject Property { get { return Field; } }
            public ITestInterface InterfaceProperty { get { return InterfaceField; } }
            public object ObjectProperty { get { return ObjectField; } }
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedProperty { get { return UnrestrictedField; } }

            public TestObject Method() { return Property; }
            public ITestInterface InterfaceMethod() { return InterfaceProperty; }
            public object ObjectMethod() { return ObjectProperty; }
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedMethod() { return UnrestrictedProperty; }

            public int TestMethod(TestObject testObject)
            {
                return ObjectTestValue;
            }

            public string TestMethod(ITestInterface testInterface)
            {
                return InterfaceTestValue;
            }
        }

        #endregion
    }
}
