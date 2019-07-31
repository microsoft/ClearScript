// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class MemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private TestObject testObject;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
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

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field()
        {
            testObject.Field = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testObject.Field.Length"));
            engine.Execute("testObject.Field = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testObject.Field.Length);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Null()
        {
            engine.Execute("testObject.Field = null");
            Assert.IsNull(testObject.Field);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.Field = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Scalar()
        {
            testObject.ScalarField = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testObject.ScalarField"));
            engine.Execute("testObject.ScalarField = 4321");
            Assert.AreEqual(4321, testObject.ScalarField);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("testObject.ScalarField = 54321"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.ScalarField = TestEnum.Second"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Enum()
        {
            testObject.EnumField = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testObject.EnumField"));
            engine.Execute("testObject.EnumField = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testObject.EnumField);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Enum_Zero()
        {
            engine.Execute("testObject.EnumField = 0");
            Assert.AreEqual((TestEnum)0, testObject.EnumField);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.EnumField = 1"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Struct()
        {
            testObject.StructField = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testObject.StructField"));
            engine.Execute("testObject.StructField = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testObject.StructField);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Field_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.StructField = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property()
        {
            testObject.Property = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testObject.Property.Length"));
            engine.Execute("testObject.Property = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testObject.Property.Length);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Null()
        {
            engine.Execute("testObject.Property = null");
            Assert.IsNull(testObject.Property);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.Property = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Scalar()
        {
            testObject.ScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testObject.ScalarProperty"));
            engine.Execute("testObject.ScalarProperty = 4321");
            Assert.AreEqual(4321, testObject.ScalarProperty);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("testObject.ScalarProperty = 54321"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.ScalarProperty = TestEnum.Second"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Enum()
        {
            testObject.EnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testObject.EnumProperty"));
            engine.Execute("testObject.EnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testObject.EnumProperty);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testObject.EnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testObject.EnumProperty);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.EnumProperty = 1"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Struct()
        {
            testObject.StructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testObject.StructProperty"));
            engine.Execute("testObject.StructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testObject.StructProperty);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.StructProperty = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Property_Blocked()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("host.proc(0, function () {}).Method"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testObject.ReadOnlyProperty, (int)engine.Evaluate("testObject.ReadOnlyProperty"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.ReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Event()
        {
            engine.Execute("var connection = testObject.Event.connect(function (sender, args) { sender.ScalarProperty = args.Arg; })");
            testObject.FireEvent(5432);
            Assert.AreEqual(5432, testObject.ScalarProperty);
            engine.Execute("connection.disconnect()");
            testObject.FireEvent(2345);
            Assert.AreEqual(5432, testObject.ScalarProperty);
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method()
        {
            Assert.AreEqual(testObject.Method("foo", 4), engine.Evaluate("testObject.Method('foo', 4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.Method('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_Generic()
        {
            Assert.AreEqual(testObject.Method("foo", 4, TestEnum.Second), engine.Evaluate("testObject.Method('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.Method('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testObject.Method("foo", 4, TestEnum.Second), engine.Evaluate("testObject.Method(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.Method(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GenericExplicit()
        {
            Assert.AreEqual(testObject.Method<TestEnum>(4), engine.Evaluate("testObject.Method(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.Method(4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GetType()
        {
            engine.AllowReflection = true;
            Assert.AreEqual(testObject.GetType(), engine.Evaluate("testObject.GetType()"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GetType_Blocked()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.GetType()"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GetType_Blocked_Exception()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute(@"
                try {
                    testObject.GetType();
                }
                catch (exception)
                {
                    exception.hostException.GetType();
                }
            "));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_Method_GetType_Blocked_Exception_Interface()
        {
            engine.AddHostType(typeof(_Exception));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute(@"
                try {
                    testObject.GetType();
                }
                catch (exception)
                {
                    host.cast(_Exception, exception.hostException).GetType();
                }
            "));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_BindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BindTestMethod(arg), engine.Evaluate("testObject.BindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_BindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.BindTestMethod(arg), engine.Evaluate("testObject.BindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testObject.ExtensionMethod("foo", 4), engine.Evaluate("testObject.ExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExtensionMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testObject.ExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExtensionMethod('foo', 4, testObject)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testObject.ExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_GenericExplicit()
        {
            Assert.AreEqual(testObject.ExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.ExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionMethod_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("testObject.ExtensionMethod(4)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionBindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.ExtensionBindTestMethod(arg), engine.Evaluate("testObject.ExtensionBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("MemberAccess")]
        public void MemberAccess_ExtensionBindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(testObject.ExtensionBindTestMethod(arg), engine.Evaluate("testObject.ExtensionBindTestMethod(arg)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
