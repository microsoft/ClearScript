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
    public class StaticMemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", new ExtendedHostFunctions());
            engine.AddHostObject("mscorlib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.AddHostObject("ClearScriptTest", HostItemFlags.GlobalMembers, new HostTypeCollection("ClearScriptTest").GetNamespaceNode("Microsoft.ClearScript.Test"));
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

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field()
        {
            StaticTestClass.StaticField = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("StaticTestClass.StaticField.Length"));
            engine.Execute("StaticTestClass.StaticField = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, StaticTestClass.StaticField.Length);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Null()
        {
            engine.Execute("StaticTestClass.StaticField = null");
            Assert.IsNull(StaticTestClass.StaticField);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticField = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Scalar()
        {
            StaticTestClass.StaticScalarField = 12345;
            Assert.AreEqual(12345, engine.Evaluate("StaticTestClass.StaticScalarField"));
            engine.Execute("StaticTestClass.StaticScalarField = 4321");
            Assert.AreEqual(4321, StaticTestClass.StaticScalarField);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("StaticTestClass.StaticScalarField = 54321"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticScalarField = TestEnum.Second"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Enum()
        {
            StaticTestClass.StaticEnumField = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("StaticTestClass.StaticEnumField"));
            engine.Execute("StaticTestClass.StaticEnumField = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, StaticTestClass.StaticEnumField);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Enum_Zero()
        {
            engine.Execute("StaticTestClass.StaticEnumField = 0");
            Assert.AreEqual((TestEnum)0, StaticTestClass.StaticEnumField);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticEnumField = 1"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Struct()
        {
            StaticTestClass.StaticStructField = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("StaticTestClass.StaticStructField"));
            engine.Execute("StaticTestClass.StaticStructField = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), StaticTestClass.StaticStructField);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Field_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticStructField = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property()
        {
            StaticTestClass.StaticProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("StaticTestClass.StaticProperty.Length"));
            engine.Execute("StaticTestClass.StaticProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, StaticTestClass.StaticProperty.Length);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Null()
        {
            engine.Execute("StaticTestClass.StaticProperty = null");
            Assert.IsNull(StaticTestClass.StaticProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticProperty = host.newArr(System.Double, 5)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Scalar()
        {
            StaticTestClass.StaticScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("StaticTestClass.StaticScalarProperty"));
            engine.Execute("StaticTestClass.StaticScalarProperty = 4321");
            Assert.AreEqual(4321, StaticTestClass.StaticScalarProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Scalar_Overflow()
        {
            TestUtil.AssertException<OverflowException>(() => engine.Execute("StaticTestClass.StaticScalarProperty = 54321"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Scalar_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticScalarProperty = TestEnum.Second"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Enum()
        {
            StaticTestClass.StaticEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("StaticTestClass.StaticEnumProperty"));
            engine.Execute("StaticTestClass.StaticEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, StaticTestClass.StaticEnumProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("StaticTestClass.StaticEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, StaticTestClass.StaticEnumProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Enum_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticEnumProperty = 1"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Struct()
        {
            StaticTestClass.StaticStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("StaticTestClass.StaticStructProperty"));
            engine.Execute("StaticTestClass.StaticStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), StaticTestClass.StaticStructProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Property_Struct_BadAssignment()
        {
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticStructProperty = System.DateTime.Now"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(StaticTestClass.StaticReadOnlyProperty, (int)engine.Evaluate("StaticTestClass.StaticReadOnlyProperty"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("StaticTestClass.StaticReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Event()
        {
            engine.Execute("var connection = StaticTestClass.StaticEvent.connect(function (sender, args) { host.type(sender).StaticScalarProperty = args.Arg; })");
            StaticTestClass.StaticFireEvent(5432);
            Assert.AreEqual(5432, StaticTestClass.StaticScalarProperty);
            engine.Execute("connection.disconnect()");
            StaticTestClass.StaticFireEvent(2345);
            Assert.AreEqual(5432, StaticTestClass.StaticScalarProperty);
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method()
        {
            Assert.AreEqual(StaticTestClass.StaticMethod("foo", 4), engine.Evaluate("StaticTestClass.StaticMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_NoMatchingOverload()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("StaticTestClass.StaticMethod('foo', TestEnum.Second)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_Generic()
        {
            Assert.AreEqual(StaticTestClass.StaticMethod("foo", 4, TestEnum.Second), engine.Evaluate("StaticTestClass.StaticMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("StaticTestClass.StaticMethod('foo', 4, StaticTestClass)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(StaticTestClass.StaticMethod("foo", 4, TestEnum.Second), engine.Evaluate("StaticTestClass.StaticMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("StaticTestClass.StaticMethod(System.Int32, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_GenericExplicit()
        {
            Assert.AreEqual(StaticTestClass.StaticMethod<TestEnum>(4), engine.Evaluate("StaticTestClass.StaticMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Method_GenericExplicit_MissingTypeArg()
        {
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("StaticTestClass.StaticMethod(4)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_BindTestMethod_BaseClass()
        {
            var arg = new TestArg() as BaseTestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(StaticTestClass.StaticBindTestMethod(arg), engine.Evaluate("StaticTestClass.StaticBindTestMethod(arg)"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_BindTestMethod_Interface()
        {
            var arg = new TestArg() as ITestArg;
            engine.AddRestrictedHostObject("arg", arg);
            Assert.AreEqual(StaticTestClass.StaticBindTestMethod(arg), engine.Evaluate("StaticTestClass.StaticBindTestMethod(arg)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
