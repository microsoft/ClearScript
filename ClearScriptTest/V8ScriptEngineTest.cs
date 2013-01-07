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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    public class V8ScriptEngineTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostObject()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", host);
            Assert.AreSame(host, engine.Evaluate("host"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void V8ScriptEngine_AddHostObject_Scalar()
        {
            engine.AddHostObject("value", 123);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostObject_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.AddHostObject("value", value);
            Assert.AreEqual(value, engine.Evaluate("value"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostObject_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.AddHostObject("date", date);
            Assert.AreEqual(date, engine.Evaluate("date"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostObject_GlobalMembers()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", HostItemFlags.GlobalMembers, host);
            Assert.IsInstanceOfType(engine.Evaluate("newObj()"), typeof(PropertyBag));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void V8ScriptEngine_AddHostObject_DefaultAccess()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostObject_PrivateAccess()
        {
            engine.AddHostObject("test", HostItemFlags.PrivateAccess, this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_GlobalMembers()
        {
            engine.AddHostType("Guid", HostItemFlags.GlobalMembers, typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("NewGuid()"), typeof(Guid));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void V8ScriptEngine_AddHostType_DefaultAccess()
        {
            engine.AddHostType("Test", GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_PrivateAccess()
        {
            engine.AddHostType("Test", HostItemFlags.PrivateAccess, GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_Static()
        {
            engine.AddHostType("Enumerable", typeof(Enumerable));
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_OpenGeneric()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("List", typeof(List<>));
            engine.AddHostType("Guid", typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(List(Guid))"), typeof(List<Guid>));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_ByName()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", "System.Random");
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_ByNameWithAssembly()
        {
            engine.AddHostType("Enumerable", "System.Linq.Enumerable", "System.Core");
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_ByNameWithTypeArgs()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Dictionary", "System.Collections.Generic.Dictionary", typeof(string), typeof(int));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Dictionary)"), typeof(Dictionary<string, int>));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, true, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_RetainDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, false, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute()
        {
            engine.Execute("epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, true, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, false, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ExecuteCommand_EngineConvert()
        {
            Assert.AreEqual("[object Math]", engine.ExecuteCommand("Math"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ExecuteCommand_HostConvert()
        {
            var dateHostItem = HostItem.Wrap(engine, new DateTime(2007, 5, 22, 6, 15, 43));
            engine.AddHostObject("date", dateHostItem);
            Assert.AreEqual(dateHostItem.ToString(), engine.ExecuteCommand("date"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Interrupt()
        {
            var checkpoint = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(state =>
            {
                checkpoint.WaitOne();
                engine.Interrupt();
            });

            engine.AddHostObject("checkpoint", checkpoint);

            var gotException = false;
            try
            {
                engine.Execute("checkpoint.Set(); while (true) { var foo = 'hello'; }");
            }
            catch (OperationCanceledException)
            {
                gotException = true;
            }

            Assert.IsTrue(gotException);
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void V8ScriptEngine_AccessContext_Default()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AccessContext_Private()
        {
            engine.AddHostObject("test", this);
            engine.AccessContext = GetType();
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ContinuationCallback()
        {
            engine.ContinuationCallback = () => false;

            var gotException = false;
            try
            {
                engine.Execute("while (true) { var foo = 'hello'; }");
            }
            catch (OperationCanceledException)
            {
                gotException = true;
            }

            Assert.IsTrue(gotException);

            engine.ContinuationCallback = null;
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_FileNameExtension()
        {
            Assert.AreEqual("js", engine.FileNameExtension);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property()
        {
            var host = new HostFunctions();
            engine.Script.host = host;
            Assert.AreSame(host, engine.Script.host);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Scalar()
        {
            const int value = 123;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.Script.date = date;
            Assert.AreEqual(date, engine.Script.date);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Index_ArrayItem()
        {
            const int index = 5;
            engine.Execute("foo = []");

            engine.Script.foo[index] = engine.Script.Math.PI;
            Assert.AreEqual(Math.PI, engine.Script.foo[index]);
            Assert.AreEqual(index + 1, engine.Evaluate("foo.length"));

            engine.Script.foo[index] = engine.Script.Math.E;
            Assert.AreEqual(Math.E, engine.Script.foo[index]);
            Assert.AreEqual(index + 1, engine.Evaluate("foo.length"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Index_Property()
        {
            const string name = "bar";
            engine.Execute("foo = {}");

            engine.Script.foo[name] = engine.Script.Math.PI;
            Assert.AreEqual(Math.PI, engine.Script.foo[name]);
            Assert.AreEqual(Math.PI, engine.Script.foo.bar);

            engine.Script.foo[name] = engine.Script.Math.E;
            Assert.AreEqual(Math.E, engine.Script.foo[name]);
            Assert.AreEqual(Math.E, engine.Script.foo.bar);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Method()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Script.eval("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Parallel()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));

            const int threadCount = 256;
            engine.AddHostObject("list", Enumerable.Range(0, threadCount).ToList());
            Assert.AreEqual(threadCount, engine.Evaluate("list.Count"));

            var startEvent = new ManualResetEventSlim(false);
            var stopEvent = new ManualResetEventSlim(false);
            engine.AddHostObject("stopEvent", stopEvent);

            ThreadStart body = () =>
            {
                // ReSharper disable AccessToDisposedClosure

                startEvent.Wait();
                engine.Execute("list.RemoveAt(0); if (list.Count == 0) { stopEvent.Set(); }");

                // ReSharper restore AccessToDisposedClosure
            };

            var threads = Enumerable.Range(0, threadCount).Select(index => new Thread(body)).ToArray();
            threads.ForEach(thread => thread.Start());

            startEvent.Set();
            stopEvent.Wait();
            Assert.AreEqual(0, engine.Evaluate("list.Count"));

            threads.ForEach(thread => thread.Join());
            startEvent.Dispose();
            stopEvent.Dispose();
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Random()"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Random(100)"), typeof(Random));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new_Generic()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Collections.Generic.Dictionary(System.Int32, System.String)"), typeof(Dictionary<int, string>));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Collections.Generic.Dictionary(System.Int32, System.String, 100)"), typeof(Dictionary<int, string>));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new_GenericInner()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System.Core"));
            engine.AddHostObject("dict", new Dictionary<int, string> { { 12345, "foo" }, { 54321, "bar" } });
            Assert.IsInstanceOfType(engine.Evaluate("vc = new (System.Collections.Generic.Dictionary(System.Int32, System.String).ValueCollection)(dict)"), typeof(Dictionary<int, string>.ValueCollection));
            Assert.IsTrue((bool)engine.Evaluate("vc.SequenceEqual(dict.Values)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new_Scalar()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(int), engine.Evaluate("new System.Int32"));
            Assert.AreEqual(default(int), engine.Evaluate("new System.Int32()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new_Enum()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(DayOfWeek), engine.Evaluate("new System.DayOfWeek"));
            Assert.AreEqual(default(DayOfWeek), engine.Evaluate("new System.DayOfWeek()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_new_Struct()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(DateTime), engine.Evaluate("new System.DateTime"));
            Assert.AreEqual(default(DateTime), engine.Evaluate("new System.DateTime()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(MissingMemberException), AllowDerivedTypes = true)]
        public void V8ScriptEngine_new_NoMatch()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.Evaluate("new System.Random('a')");
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        // ReSharper disable UnusedMember.Local

        private void PrivateMethod()
        {
        }

        private static void PrivateStaticMethod()
        {
        }

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
