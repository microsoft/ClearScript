// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

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
