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
    public class StaticStaticMemberAccessTest : ClearScriptTest
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
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("StaticTestClass.StaticReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("StaticMemberAccess")]
        public void StaticMemberAccess_Event()
        {
            engine.Execute("var connection = StaticTestClass.StaticEvent.connect(function(sender, args) { host.type(sender).StaticScalarProperty = args.Arg; })");
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
