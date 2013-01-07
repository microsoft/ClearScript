// 
// Copyright © Microsoft Corporation. All rights reserved.
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
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    public class ExplicitInterfaceMemberAccessTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private TestObject testObject;
        private IExplicitTestInterface testInterface;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", new ExtendedHostFunctions());
            engine.AddHostObject("mscorlib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.AddHostObject("ClearScriptTest", HostItemFlags.GlobalMembers, new HostTypeCollection("ClearScriptTest").GetNamespaceNode("Microsoft.ClearScript.Test"));
            engine.AddHostObject("testObject", testInterface = testObject = new TestObject());
            engine.Execute("var testInterface = host.cast(IExplicitTestInterface, testObject)");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            testInterface = null;
            engine.Dispose();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property()
        {
            testInterface.ExplicitInterfaceProperty = Enumerable.Range(0, 10).ToArray();
            Assert.AreEqual(10, engine.Evaluate("testInterface.ExplicitInterfaceProperty.Length"));
            engine.Execute("testInterface.ExplicitInterfaceProperty = host.newArr(System.Int32, 5)");
            Assert.AreEqual(5, testInterface.ExplicitInterfaceProperty.Length);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Null()
        {
            engine.Execute("testInterface.ExplicitInterfaceProperty = null");
            Assert.IsNull(testInterface.ExplicitInterfaceProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitInterfaceMemberAccess_Property_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitInterfaceProperty = host.newArr(System.Double, 5)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Scalar()
        {
            testInterface.ExplicitInterfaceScalarProperty = 12345;
            Assert.AreEqual(12345, engine.Evaluate("testInterface.ExplicitInterfaceScalarProperty"));
            engine.Execute("testInterface.ExplicitInterfaceScalarProperty = 4321");
            Assert.AreEqual(4321, testInterface.ExplicitInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(OverflowException))]
        public void ExplicitInterfaceMemberAccess_Property_Scalar_Overflow()
        {
            engine.Execute("testInterface.ExplicitInterfaceScalarProperty = 54321");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitInterfaceMemberAccess_Property_Scalar_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitInterfaceScalarProperty = TestEnum.Second");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Enum()
        {
            testInterface.ExplicitInterfaceEnumProperty = TestEnum.Second;
            Assert.AreEqual(TestEnum.Second, engine.Evaluate("testInterface.ExplicitInterfaceEnumProperty"));
            engine.Execute("testInterface.ExplicitInterfaceEnumProperty = TestEnum.Third");
            Assert.AreEqual(TestEnum.Third, testInterface.ExplicitInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Enum_Zero()
        {
            engine.Execute("testInterface.ExplicitInterfaceEnumProperty = 0");
            Assert.AreEqual((TestEnum)0, testInterface.ExplicitInterfaceEnumProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitInterfaceMemberAccess_Property_Enum_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitInterfaceEnumProperty = 1");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Struct()
        {
            testInterface.ExplicitInterfaceStructProperty = TimeSpan.FromDays(5);
            Assert.AreEqual(TimeSpan.FromDays(5), engine.Evaluate("testInterface.ExplicitInterfaceStructProperty"));
            engine.Execute("testInterface.ExplicitInterfaceStructProperty = System.TimeSpan.FromSeconds(25)");
            Assert.AreEqual(TimeSpan.FromSeconds(25), testInterface.ExplicitInterfaceStructProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitInterfaceMemberAccess_Property_Struct_BadAssignment()
        {
            engine.Execute("testInterface.ExplicitInterfaceStructProperty = System.DateTime.Now");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ReadOnlyProperty()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceReadOnlyProperty, (int)engine.Evaluate("testInterface.ExplicitInterfaceReadOnlyProperty"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitInterfaceMemberAccess_ReadOnlyProperty_Write()
        {
            engine.Execute("testInterface.ExplicitInterfaceReadOnlyProperty = 2");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Event()
        {
            engine.Execute("var connection = testInterface.ExplicitInterfaceEvent.connect(function(sender, args) { host.cast(IExplicitTestInterface, sender).ExplicitInterfaceScalarProperty = args.Arg; })");
            testInterface.ExplicitInterfaceFireEvent(5432);
            Assert.AreEqual(5432, testInterface.ExplicitInterfaceScalarProperty);
            engine.Execute("connection.disconnect()");
            testInterface.ExplicitInterfaceFireEvent(2345);
            Assert.AreEqual(5432, testInterface.ExplicitInterfaceScalarProperty);
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Method()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceMethod("foo", 4), engine.Evaluate("testInterface.ExplicitInterfaceMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_Method_NoMatchingOverload()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Method_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitInterfaceMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_Method_Generic_TypeArgConstraintFailure()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceMethod('foo', 4, testInterface)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Method_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitInterfaceMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_Method_GenericRedundant_MismatchedTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Method_GenericExplicit()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitInterfaceMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_Method_GenericExplicit_MissingTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceMethod(4)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceExtensionMethod("foo", 4), engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_Generic()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod('foo', 4, testInterface)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericRedundant()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericExplicit()
        {
            Assert.AreEqual(testInterface.ExplicitInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericExplicit_MissingTypeArg()
        {
            engine.Evaluate("testInterface.ExplicitInterfaceExtensionMethod(4)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceProperty')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Scalar_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceScalarProperty')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Enum_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceEnumProperty')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Property_Struct_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceStructProperty')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ReadOnlyProperty_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceReadOnlyProperty')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Event_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceEvent')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_Method_OnObject()
        {
            Assert.IsTrue((bool)engine.Evaluate("(function (name) { return !(name in testObject) && (name in testInterface); })('ExplicitInterfaceMethod')"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitInterfaceExtensionMethod("foo", 4), engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod('foo', 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_NoMatchingOverload_OnObject()
        {
            engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod('foo', TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_Generic_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod('foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_Generic_TypeArgConstraintFailure_OnObject()
        {
            engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod('foo', 4, testObject)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericRedundant_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitInterfaceExtensionMethod("foo", 4, TestEnum.Second), engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod(TestEnum, 'foo', 4, TestEnum.Second)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericRedundant_MismatchedTypeArg_OnObject()
        {
            engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod(System.Int32, 'foo', 4, TestEnum.Second)");
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericExplicit_OnObject()
        {
            Assert.AreEqual(testObject.ExplicitInterfaceExtensionMethod<TestEnum>(4), engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod(TestEnum, 4)"));
        }

        [TestMethod, TestCategory("ExplicitInterfaceMemberAccess")]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ExplicitInterfaceMemberAccess_ExtensionMethod_GenericExplicit_MissingTypeArg_OnObject()
        {
            engine.Evaluate("testObject.ExplicitInterfaceExtensionMethod(4)");
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
