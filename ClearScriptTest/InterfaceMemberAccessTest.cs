// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class InterfaceMemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private TestObject testObject;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", new ExtendedHostFunctions());
            engine.AddHostObject("mscorlib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.AddHostObject("ClearScriptTest", HostItemFlags.GlobalMembers, new HostTypeCollection("ClearScriptTest").GetNamespaceNode("Microsoft.ClearScript.Test"));
            engine.AddHostObject("testObject", testObject = new TestObject());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            testObject = null;
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property()
        {
            testObject.InterfaceProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testObject.InterfaceProperty.Length"));
            engine.Execute("testObject.InterfaceProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testObject.InterfaceProperty.Length);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Null()
        {
            engine.Execute("testObject.InterfaceProperty = null");
            Assert.IsNull(testObject.InterfaceProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.InterfaceProperty = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Scalar()
        {
            testObject.InterfaceScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testObject.InterfaceScalarProperty"));
            engine.Execute("testObject.InterfaceScalarProperty = 4321");
            Assert.AreEqual(4321, testObject.InterfaceScalarProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("testObject.InterfaceScalarProperty = 54321"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.InterfaceScalarProperty = TestEnum.Second"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Enum()
        {
            testObject.InterfaceEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testObject.InterfaceEnumProperty"));
            engine.Execute("testObject.InterfaceEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testObject.InterfaceEnumProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testObject.InterfaceEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testObject.InterfaceEnumProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.InterfaceEnumProperty = 1"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Struct()
        {
            testObject.InterfaceStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testObject.InterfaceStructProperty"));
            engine.Execute("testObject.InterfaceStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testObject.InterfaceStructProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Property_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.InterfaceStructProperty = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testObject.InterfaceReadOnlyProperty, (int)engine.Evaluate("testObject.InterfaceReadOnlyProperty"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.InterfaceReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testObject.InterfaceEvent.connect(function (sender, args) { sender.InterfaceScalarProperty = args.Arg; })");
            testObject.InterfaceFireEvent(5432);
            Assert.AreEqual(5432, testObject.InterfaceScalarProperty);
            engine.Execute("connection.disconnect()");
            testObject.InterfaceFireEvent(2345);
            Assert.AreEqual(5432, testObject.InterfaceScalarProperty);
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method()
        {
            Assert.AreEqual(testObject.InterfaceMethod("foo", 4), engine.Evaluate("testObject.InterfaceMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_Generic()
        {
            Assert.AreEqual(testObject.InterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.InterfaceMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testObject.InterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.InterfaceMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_GenericExplicit()
        {
            Assert.AreEqual(testObject.InterfaceMethod<TestEnum>(4), engine.Evaluate("testObject.InterfaceMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_Method_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceMethod(4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_BindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.InterfaceBindTestMethod(arg), engine.Evaluate("testObject.InterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_BindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.InterfaceBindTestMethod(arg), engine.Evaluate("testObject.InterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testObject.InterfaceExtensionMethod("foo", 4), engine.Evaluate("testObject.InterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceExtensionMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testObject.InterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.InterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceExtensionMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testObject.InterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.InterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_GenericExplicit()
        {
            Assert.AreEqual(testObject.InterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.InterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionMethod_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.InterfaceExtensionMethod(4)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionBindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.InterfaceExtensionBindTestMethod(arg), engine.Evaluate("testObject.InterfaceExtensionBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("InterfaceMemberAccess")]
        public void InterfaceMemberAccess_ExtensionBindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.InterfaceExtensionBindTestMethod(arg), engine.Evaluate("testObject.InterfaceExtensionBindTestMethod(arg)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
