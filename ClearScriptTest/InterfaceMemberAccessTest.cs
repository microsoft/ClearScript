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
