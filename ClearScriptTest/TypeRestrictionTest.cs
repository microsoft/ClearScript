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

using System.Diagnostics.CodeAnalysis;
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
    public class TypeRestrictionTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.Script.testContainer = new TestContainer();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Field()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Field)"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceField)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectField)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedField)"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceField)"));
        }

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Property()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Property)"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceProperty)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectProperty)"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedProperty)"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceProperty)"));
        }

        [TestMethod, TestCategory("TypeRestriction")]
        public void TypeRestriction_Method()
        {
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.Method())"));
            Assert.AreEqual(TestContainer.InterfaceTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceMethod())"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.ObjectMethod())"));
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.UnrestrictedMethod())"));

            engine.DisableTypeRestriction = true;
            Assert.AreEqual(TestContainer.ObjectTestValue, engine.Evaluate("testContainer.TestMethod(testContainer.InterfaceMethod())"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        public interface ITestInterface
        {
        }

        public class TestObject : ITestInterface
        {
        }

        public class TestContainer
        {
            public const int ObjectTestValue = 123456;
            public const string InterfaceTestValue = "fooBARbazQUX";

            public TestObject Field = new TestObject();
            public ITestInterface InterfaceField = new TestObject();
            public object ObjectField = new TestObject();
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedField = new TestObject();

            public TestObject Property { get { return Field; } }
            public ITestInterface InterfaceProperty { get { return InterfaceField; } }
            public object ObjectProperty { get { return ObjectField; } }
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedProperty { get { return UnrestrictedField; } }

            public TestObject Method() { return Property; }
            public ITestInterface InterfaceMethod() { return InterfaceProperty; }
            public object ObjectMethod() { return ObjectProperty; }
            [ScriptMember(ScriptMemberFlags.ExposeRuntimeType)] public ITestInterface UnrestrictedMethod() { return UnrestrictedProperty; }

            public int TestMethod(TestObject testObject)
            {
                return ObjectTestValue;
            }

            public string TestMethod(ITestInterface testInterface)
            {
                return InterfaceTestValue;
            }
        }

        #endregion
    }
}
