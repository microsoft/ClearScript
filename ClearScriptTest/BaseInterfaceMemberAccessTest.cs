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
    public class BaseInterfaceMemberAccessTest : ClearScriptTest
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

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property()
        {
            testObject.BaseInterfaceProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testObject.BaseInterfaceProperty.Length"));
            engine.Execute("testObject.BaseInterfaceProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testObject.BaseInterfaceProperty.Length);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Null()
        {
            engine.Execute("testObject.BaseInterfaceProperty = null");
            Assert.IsNull(testObject.BaseInterfaceProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.BaseInterfaceProperty = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Scalar()
        {
            testObject.BaseInterfaceScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testObject.BaseInterfaceScalarProperty"));
            engine.Execute("testObject.BaseInterfaceScalarProperty = 4321");
            Assert.AreEqual(4321, testObject.BaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("testObject.BaseInterfaceScalarProperty = 54321"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.BaseInterfaceScalarProperty = TestEnum.Second"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Enum()
        {
            testObject.BaseInterfaceEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testObject.BaseInterfaceEnumProperty"));
            engine.Execute("testObject.BaseInterfaceEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testObject.BaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testObject.BaseInterfaceEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testObject.BaseInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.BaseInterfaceEnumProperty = 1"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Struct()
        {
            testObject.BaseInterfaceStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testObject.BaseInterfaceStructProperty"));
            engine.Execute("testObject.BaseInterfaceStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testObject.BaseInterfaceStructProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Property_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.BaseInterfaceStructProperty = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testObject.BaseInterfaceReadOnlyProperty, (int)engine.Evaluate("testObject.BaseInterfaceReadOnlyProperty"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.BaseInterfaceReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testObject.BaseInterfaceEvent.connect(function (sender, args) { sender.BaseInterfaceScalarProperty = args.Arg; })");
            testObject.BaseInterfaceFireEvent(5432);
            Assert.AreEqual(5432, testObject.BaseInterfaceScalarProperty);
            engine.Execute("connection.disconnect()");
            testObject.BaseInterfaceFireEvent(2345);
            Assert.AreEqual(5432, testObject.BaseInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method()
        {
            Assert.AreEqual(testObject.BaseInterfaceMethod("foo", 4), engine.Evaluate("testObject.BaseInterfaceMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_Generic()
        {
            Assert.AreEqual(testObject.BaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.BaseInterfaceMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testObject.BaseInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.BaseInterfaceMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_GenericExplicit()
        {
            Assert.AreEqual(testObject.BaseInterfaceMethod<TestEnum>(4), engine.Evaluate("testObject.BaseInterfaceMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Method_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceMethod(4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_BindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BaseInterfaceBindTestMethod(arg), engine.Evaluate("testObject.BaseInterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_BindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BaseInterfaceBindTestMethod(arg), engine.Evaluate("testObject.BaseInterfaceBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testObject.BaseInterfaceExtensionMethod("foo", 4), engine.Evaluate("testObject.BaseInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceExtensionMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testObject.BaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.BaseInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceExtensionMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testObject.BaseInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.BaseInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_GenericExplicit()
        {
            Assert.AreEqual(testObject.BaseInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.BaseInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionMethod_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.BaseInterfaceExtensionMethod(4)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionBindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BaseInterfaceExtensionBindTestMethod(arg), engine.Evaluate("testObject.BaseInterfaceExtensionBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_ExtensionBindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BaseInterfaceExtensionBindTestMethod(arg), engine.Evaluate("testObject.BaseInterfaceExtensionBindTestMethod(arg)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
