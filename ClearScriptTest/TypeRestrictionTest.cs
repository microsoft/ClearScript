// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
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

            public TestObject Field = new();
            public ITestInterface InterfaceField = new TestObject();
            public object ObjectField = new TestObject();
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedField = new TestObject();

            public TestObject Property => Field;
            public ITestInterface InterfaceProperty => InterfaceField;
            public object ObjectProperty => ObjectField;
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedProperty => UnrestrictedField;

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
