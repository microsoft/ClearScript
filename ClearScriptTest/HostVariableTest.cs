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
using System.Collections.Generic;
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
    public class HostVariableTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System", "System.Core"));
            engine.AddHostObject("host", new HostFunctions());
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

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out()
        {
            var value = new Random();
            var dict = new Dictionary<string, Random> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.Random); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreSame(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            var dict = new Dictionary<string, DayOfWeek> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.DayOfWeek); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Scalar()
        {
            const double value = Math.E;
            var dict = new Dictionary<string, Double> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.Double); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_out_Struct()
        {
            var value = DateTime.UtcNow;
            var dict = new Dictionary<string, DateTime> { { "key", value } };
            engine.AddHostObject("dict", dict);
            engine.Execute("var value = host.newVar(System.DateTime); var found = dict.TryGetValue('key', value.out); var result = value.value;");
            Assert.IsTrue(engine.Script.found);
            Assert.AreEqual(value, engine.Script.result);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref()
        {
            var inputValue = new Random();
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(Random));
            Assert.AreSame(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Enum()
        {
            const DayOfWeek inputValue = DayOfWeek.Thursday;
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(DayOfWeek));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Scalar()
        {
            const double inputValue = Math.PI;
            engine.AddHostObject("test", this);
            engine.Script.inValue = inputValue;
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(Double));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        [TestMethod, TestCategory("HostVariable")]
        public void HostVariable_ref_Struct()
        {
            var inputValue = DateTime.UtcNow;
            engine.AddHostObject("test", this);
            engine.AddHostObject("inValue", inputValue);
            engine.Execute("var value = host.newVar(inValue); var returnValue = test.TestMethod(value.ref); var result = value.value;");
            Assert.AreEqual(engine.Script.result, default(DateTime));
            Assert.AreEqual(inputValue, engine.Script.returnValue);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        // ReSharper disable UnusedMember.Local

        public T TestMethod<T>(ref T value, T outValue = default(T))
        {
            var inValue = value;
            value = outValue;
            return inValue;
        }

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
