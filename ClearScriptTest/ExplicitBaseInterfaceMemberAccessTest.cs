// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
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
    public class ExplicitBaseInterfaceMemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private TestObject testObject;
        private IExplicitBaseTestInterface testInterface;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", new ExtendedHostFunctions());
            engine.AddHostObject("mscorlib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.AddHostObject("ClearScriptTest", HostItemFlags.GlobalMembers, new HostTypeCollection("ClearScriptTest").GetNamespaceNode("Microsoft.ClearScript.Test"));
            engine.AddHostObject("testObject", testInterface = testObject = new TestObject());
            engine.Execute("var testInterface = host.cast(IExplicitBaseTestInterface, testObject)");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            testInterface = null;
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property()
        {
            testInterface.ExplicitBaseInterfaceProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testInterface.ExplicitBaseInterfaceProperty.Length"));
            engine.Execute("testInterface.ExplicitBaseInterfaceProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testInterface.ExplicitBaseInterfaceProperty.Length);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Null()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceProperty = null");
            Assert.IsNull(testInterface.ExplicitBaseInterfaceProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceProperty = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar()
        {
            testInterface.ExplicitBaseInterfaceScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testInterface.ExplicitBaseInterfaceScalarProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = 4321");
            Assert.AreEqual(4321, testInterface.ExplicitBaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = 54321"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceScalarProperty = TestEnum.Second"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum()
        {
            testInterface.ExplicitBaseInterfaceEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testInterface.ExplicitBaseInterfaceEnumProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testInterface.ExplicitBaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testInterface.ExplicitBaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceEnumProperty = 1"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct()
        {
            testInterface.ExplicitBaseInterfaceStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testInterface.ExplicitBaseInterfaceStructProperty"));
            engine.Execute("testInterface.ExplicitBaseInterfaceStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testInterface.ExplicitBaseInterfaceStructProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceStructProperty = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceReadOnlyProperty, (int)engine.Evaluate("testInterface.ExplicitBaseInterfaceReadOnlyProperty"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testInterface.ExplicitBaseInterfaceEvent.connect(function (sender, args) { host.cast(IExplicitBaseTestInterface, sender).ExplicitBaseInterfaceScalarProperty = args.Arg; })");
            testInterface.ExplicitBaseInterfaceFireEvent(5432);
            Assert.AreEqual(5432, testInterface.ExplicitBaseInterfaceScalarProperty);
            engine.Execute("connection.disconnect()");
            testInterface.ExplicitBaseInterfaceFireEvent(2345);
            Assert.AreEqual(5432, testInterface.ExplicitBaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceMethod('foo', 4, testInterface)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericExplicitBase()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitBaseInterfaceMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_GenericExplicitBase_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceMethod(4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_BindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceBindTestMethod(arg), engine.Evaluate("testInterface.ExplicitBaseInterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_BindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceBindTestMethod(arg), engine.Evaluate("testInterface.ExplicitBaseInterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceExtensionMethod('foo', 4, testInterface)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase()
        {
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testInterface.ExplicitBaseInterfaceExtensionMethod(4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Scalar_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceScalarProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Enum_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceEnumProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Property_Struct_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceStructProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ReadOnlyProperty_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceReadOnlyProperty')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Event_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceEvent')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_Method_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitBaseInterfaceMethod')"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload_OnObject()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExplicitBaseInterfaceExtensionMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure_OnObject()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExplicitBaseInterfaceExtensionMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg_OnObject()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExplicitBaseInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitBaseInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.ExplicitBaseInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionMethod_GenericExplicitBase_MissingTypeArg_OnObject()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExplicitBaseInterfaceExtensionMethod(4)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionBindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionBindTestMethod(arg), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("ExplicitBaseInterfaceMemberAccess")]
        public void ExplicitBaseInterfaceMemberAccess_ExtensionBindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testInterface.ExplicitBaseInterfaceExtensionBindTestMethod(arg), engine.Evaluate("testInterface.ExplicitBaseInterfaceExtensionBindTestMethod(arg)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
