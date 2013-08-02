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
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.BaseInterfaceReadOnlyProperty = 2"));
        }

        [TestMethod, TestCategory("BaseInterfaceMemberAccess")]
        public void BaseInterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testObject.BaseInterfaceEvent.connect(function(sender, args) { sender.BaseInterfaceScalarProperty = args.Arg; })");
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
