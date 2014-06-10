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
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
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
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
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

            engine.AddHostObject("test", HostItemFlags.GlobalMembers, this);
            engine.Execute("TestProperty = newObj()");
            Assert.IsInstanceOfType(TestProperty, typeof(PropertyBag));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
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
        public void V8ScriptEngine_AddRestrictedHostObject_BaseClass()
        {
            var host = new ExtendedHostFunctions() as HostFunctions;
            engine.AddRestrictedHostObject("host", host);
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("host.type('System.Int32')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddRestrictedHostObject_Interface()
        {
            const double value = 123.45;
            engine.AddRestrictedHostObject("convertible", value as IConvertible);
            engine.AddHostObject("culture", CultureInfo.InvariantCulture);
            Assert.AreEqual(value, engine.Evaluate("convertible.ToDouble(culture)"));
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

            engine.AddHostType("Test", HostItemFlags.GlobalMembers, GetType());
            engine.Execute("StaticTestProperty = NewGuid()");
            Assert.IsInstanceOfType(StaticTestProperty, typeof(Guid));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
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
        public void V8ScriptEngine_ExecuteCommand_var()
        {
            Assert.AreEqual("[undefined]", engine.ExecuteCommand("var x = 'foo'"));
            Assert.AreEqual("foo", engine.Script.x);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ExecuteCommand_HostVariable()
        {
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("[HostVariable:String]", engine.ExecuteCommand("host.newVar('foo')"));
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

            // V8 can't interrupt code that accesses only native data
            engine.AddHostObject("test", new { foo = "bar" });

            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("checkpoint.Set(); while (true) { var foo = test.foo; }"));
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
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
            // V8 can't interrupt code that accesses only native data
            engine.AddHostObject("test", new { foo = "bar" });

            engine.ContinuationCallback = () => false;
            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("while (true) { var foo = test.foo; }"));
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
        public void V8ScriptEngine_CollectGarbage()
        {
            engine.Execute(@"x = []; for (i = 0; i < 1024 * 1024; i++) { x.push(x); }");
            var usedHeapSize = engine.GetRuntimeHeapInfo().UsedHeapSize;
            engine.CollectGarbage(true);
            Assert.IsTrue(usedHeapSize > engine.GetRuntimeHeapInfo().UsedHeapSize);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CollectGarbage_HostObject()
        {
            // ReSharper disable RedundantAssignment

            var x = new object();
            var wr = new WeakReference(x);
            engine.Script.x = x;

            x = null;
            engine.Script.x = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsTrue(wr.IsAlive);

            engine.CollectGarbage(true);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
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
        public void V8ScriptEngine_new_NoMatch()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            TestUtil.AssertException<MissingMemberException>(() => engine.Execute("new System.Random('a')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General()
        {
            using (var console = new StringWriter())
            {
                var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                engine.AddHostObject("host", new ExtendedHostFunctions());
                engine.AddHostObject("clr", clr);

                engine.Execute(generalScript);
                Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_Precompiled()
        {
            using (var script = engine.Compile(generalScript))
            {
                using (var console = new StringWriter())
                {
                    var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                    clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                    engine.AddHostObject("host", new ExtendedHostFunctions());
                    engine.AddHostObject("clr", clr);

                    engine.Evaluate(script);
                    Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));

                    console.GetStringBuilder().Clear();
                    Assert.AreEqual(string.Empty, console.ToString());

                    engine.Evaluate(script);
                    Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_Precompiled_Dual()
        {
            engine.Dispose();
            using (var runtime = new V8Runtime())
            {
                using (var script = runtime.Compile(generalScript))
                {
                    engine = runtime.CreateScriptEngine();
                    using (var console = new StringWriter())
                    {
                        var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                        clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                        engine.AddHostObject("host", new ExtendedHostFunctions());
                        engine.AddHostObject("clr", clr);

                        engine.Evaluate(script);
                        Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));

                        console.GetStringBuilder().Clear();
                        Assert.AreEqual(string.Empty, console.ToString());

                        engine.Evaluate(script);
                        Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
                    }

                    engine.Dispose();
                    engine = runtime.CreateScriptEngine();
                    using (var console = new StringWriter())
                    {
                        var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                        clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                        engine.AddHostObject("host", new ExtendedHostFunctions());
                        engine.AddHostObject("clr", clr);

                        engine.Evaluate(script);
                        Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));

                        console.GetStringBuilder().Clear();
                        Assert.AreEqual(string.Empty, console.ToString());

                        engine.Evaluate(script);
                        Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
                    }
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_SyntaxError()
        {
            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("function foo() { int c; }");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNull(exception.InnerException);
                    Assert.IsTrue(exception.Message.Contains("SyntaxError"));
                    Assert.IsTrue(exception.ErrorDetails.Contains(" -> "));
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_ThrowNonError()
        {
            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("(function () { throw 123; })()");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNull(exception.InnerException);
                    Assert.IsTrue(exception.Message.StartsWith("123", StringComparison.Ordinal));
                    Assert.IsTrue(exception.ErrorDetails.Contains(" -> "));
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_ScriptError()
        {
            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("foo = {}; foo();");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNull(exception.InnerException);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_HostException()
        {
            engine.AddHostObject("host", new HostFunctions());

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Evaluate("host.newObj(0)");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNotNull(exception.InnerException);

                    var hostException = exception.InnerException;
                    Assert.IsInstanceOfType(hostException, typeof(RuntimeBinderException));
                    TestUtil.AssertValidException(hostException);
                    Assert.IsNull(hostException.InnerException);

                    Assert.AreEqual("Error: " + hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_IgnoredHostException()
        {
            engine.AddHostObject("host", new HostFunctions());

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("try { host.newObj(null); } catch(ex) {} foo = {}; foo();");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNull(exception.InnerException);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_NestedScriptError()
        {
            var innerEngine = new V8ScriptEngine("inner", V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("engine", innerEngine);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("engine.Execute('foo = {}; foo();')");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNotNull(exception.InnerException);

                    var hostException = exception.InnerException;
                    Assert.IsInstanceOfType(hostException, typeof(TargetInvocationException));
                    TestUtil.AssertValidException(hostException);
                    Assert.IsNotNull(hostException.InnerException);

                    var nestedException = hostException.InnerException as ScriptEngineException;
                    Assert.IsNotNull(nestedException);
                    TestUtil.AssertValidException(innerEngine, nestedException);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.IsNull(nestedException.InnerException);

                    Assert.AreEqual("Error: " + hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_NestedHostException()
        {
            var innerEngine = new V8ScriptEngine("inner", V8ScriptEngineFlags.EnableDebugging);
            innerEngine.AddHostObject("host", new HostFunctions());
            engine.AddHostObject("engine", innerEngine);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("engine.Evaluate('host.newObj(0)')");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNotNull(exception.InnerException);

                    var hostException = exception.InnerException;
                    Assert.IsInstanceOfType(hostException, typeof(TargetInvocationException));
                    TestUtil.AssertValidException(hostException);
                    Assert.IsNotNull(hostException.InnerException);

                    var nestedException = hostException.InnerException as ScriptEngineException;
                    Assert.IsNotNull(nestedException);
                    TestUtil.AssertValidException(innerEngine, nestedException);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.IsNotNull(nestedException.InnerException);

                    var nestedHostException = nestedException.InnerException;
                    Assert.IsInstanceOfType(nestedHostException, typeof(RuntimeBinderException));
                    TestUtil.AssertValidException(nestedHostException);
                    Assert.IsNull(nestedHostException.InnerException);

                    Assert.AreEqual("Error: " + nestedHostException.Message, nestedException.Message);
                    Assert.AreEqual("Error: " + hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ResourceConstraints()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"x = []; while (true) { x.push(x); }";

            var constraints = new V8RuntimeConstraints
            {
                MaxYoungSpaceSize = limit,
                MaxOldSpaceSize = limit,
                MaxExecutableSize = limit
            };

            engine.Dispose();
            engine = new V8ScriptEngine(constraints);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute(code);
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    throw;
                }
            });

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.CollectGarbage(true);
                    engine.Execute("x = 5");
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ResourceConstraints_Dual()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"x = []; for (i = 0; i < 1024 * 1024; i++) { x.push(x); }";

            engine.Execute(code);
            engine.CollectGarbage(true);
            var usedHeapSize = engine.GetRuntimeHeapInfo().UsedHeapSize;

            var constraints = new V8RuntimeConstraints
            {
                MaxYoungSpaceSize = limit,
                MaxOldSpaceSize = limit,
                MaxExecutableSize = limit
            };

            engine.Dispose();
            engine = new V8ScriptEngine(constraints);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute(code);
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    throw;
                }
            });

            engine.CollectGarbage(true);
            Assert.IsTrue(usedHeapSize > engine.GetRuntimeHeapInfo().UsedHeapSize);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_CreateInstance()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo bar baz qux", engine.Evaluate("new testObject('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_CreateInstance_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("new testObject()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Invoke_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("testObject()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo-bar-baz-qux", engine.Evaluate("testObject.DynamicMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.DynamicMethod()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo.bar.baz.qux", engine.Evaluate("testObject.SomeField('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeField()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo:bar:baz:qux", engine.Evaluate("testObject.SomeProperty('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeProperty()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_DynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo;bar;baz;qux", engine.Evaluate("testObject.SomeMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_NonDynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.SomeMethod()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_InvokeMethod_NonDynamic()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("Super Bass-O-Matic '76", engine.Evaluate("testObject.ToString()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_StaticType_Field()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeField"), typeof(HostMethod));
            Assert.AreEqual(12345, engine.Evaluate("host.toStaticType(testObject).SomeField"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_StaticType_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeProperty"), typeof(HostMethod));
            Assert.AreEqual("Bogus", engine.Evaluate("host.toStaticType(testObject).SomeProperty"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_StaticType_Method()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("bar+baz+qux", engine.Evaluate("host.toStaticType(testObject).SomeMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.AreEqual(123, engine.Evaluate("testObject.foo = 123"));
            Assert.AreEqual(123, engine.Evaluate("testObject.foo"));
            Assert.IsTrue((bool)engine.Evaluate("delete testObject.foo"));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.IsFalse((bool)engine.Evaluate("delete testObject.foo"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Property_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.Zfoo"), typeof(Undefined));
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.Zfoo = 123"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Property_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo = function(x) { return x.length; }"), typeof(DynamicObject));
            Assert.AreEqual("floccinaucinihilipilification".Length, engine.Evaluate("testObject.foo('floccinaucinihilipilification')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Property_Invoke_Nested()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo = testObject"), typeof(DynamicTestObject));
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject.foo('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Element()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 1, 2, 3, 'foo')"), typeof(Undefined));
            Assert.AreEqual("bar", engine.Evaluate("host.setElement(testObject, 'bar', 1, 2, 3, 'foo')"));
            Assert.AreEqual("bar", engine.Evaluate("host.getElement(testObject, 1, 2, 3, 'foo')"));
            Assert.IsTrue((bool)engine.Evaluate("host.removeElement(testObject, 1, 2, 3, 'foo')"));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 1, 2, 3, 'foo')"), typeof(Undefined));
            Assert.IsFalse((bool)engine.Evaluate("host.removeElement(testObject, 1, 2, 3, 'foo')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Element_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 1, 2, 3, Math.PI)"), typeof(Undefined));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("host.setElement(testObject, 'bar', 1, 2, 3, Math.PI)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Element_Convert()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            engine.AddHostType("int_t", typeof(int));
            engine.AddHostType("string_t", typeof(string));
            Assert.AreEqual(98765, engine.Evaluate("host.cast(int_t, testObject)"));
            Assert.AreEqual("Booyakasha!", engine.Evaluate("host.cast(string_t, testObject)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_HostIndexers()
        {
            engine.Script.testObject = new TestObject();

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(123)"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(123)"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.set(123, Math.E)"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.get(123)"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item('456')"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get('456')"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.set('456', Math.sqrt(3))"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.get('456')"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(123, '456', 789.987, -0.12345)"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(123, '456', 789.987, -0.12345)"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.set(123, '456', 789.987, -0.12345, Math.sqrt(7))"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.get(123, '456', 789.987, -0.12345)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_FormatCode()
        {
            try
            {
                engine.Execute("a", "\n\n\n     x = 3.a");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains(" a:4:10 "));
            }

            engine.FormatCode = true;
            try
            {
                engine.Execute("b", "\n\n\n     x = 3.a");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains(" b:1:5 "));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_GetStackTrace()
        {
            engine.AddHostObject("qux", new Func<object>(() => engine.GetStackTrace()));
            engine.Execute(@"
                function baz() { return qux(); }
                function bar() { return baz(); }
                function foo() { return bar(); }
            ");

            Assert.AreEqual("    at baz (Script Document:2:41)\n    at bar (Script Document:3:41)\n    at foo (Script Document:4:41)\n    at Script Document [2] [temp]:1:1", engine.Evaluate("foo()"));
            Assert.AreEqual("    at baz (Script Document:2:41)\n    at bar (Script Document:3:41)\n    at foo (Script Document:4:41)", engine.Script.foo());
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage()
        {
            using (var runtime = new V8Runtime())
            {
                using (var engine1 = runtime.CreateScriptEngine())
                {
                    using (var engine2 = runtime.CreateScriptEngine())
                    {
                        var value = (UIntPtr)123456;
                        engine1.MaxRuntimeStackUsage = value;
                        Assert.AreEqual(value, engine1.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, engine2.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, runtime.MaxStackUsage);

                        value = (UIntPtr)654321;
                        runtime.MaxStackUsage = value;
                        Assert.AreEqual(value, engine1.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, engine2.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, runtime.MaxStackUsage);
                    }
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_ScriptOnly()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(16 * 1024);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("(function () { arguments.callee(); })()"), false);
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_HostBounce()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(16 * 1024);
            dynamic foo = engine.Evaluate("(function () { arguments.callee(); })");
            engine.Script.bar = new Action(() => foo());
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("bar()"), false);
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_Alternating()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(16 * 1024);
            dynamic foo = engine.Evaluate("(function () { bar(); })");
            engine.Script.bar = new Action(() => foo());
            TestUtil.AssertException<ScriptEngineException>(() => foo(), false);
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_Expansion()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(16 * 1024);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("count = 0; (function () { count++; arguments.callee(); })()"), false);
            var count1 = engine.Script.count;
            engine.MaxRuntimeStackUsage = (UIntPtr)(64 * 1024);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("count = 0; (function () { count++; arguments.callee(); })()"), false);
            var count2 = engine.Script.count;
            Assert.IsTrue(count2 >= (count1 * 2));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private const string generalScript =
        @"
            System = clr.System;

            TestObject = host.type('Microsoft.ClearScript.Test.GeneralTestObject', 'ClearScriptTest');
            tlist = host.newObj(System.Collections.Generic.List(TestObject));
            tlist.Add(host.newObj(TestObject, 'Eóin', 20));
            tlist.Add(host.newObj(TestObject, 'Shane', 16));
            tlist.Add(host.newObj(TestObject, 'Cillian', 8));
            tlist.Add(host.newObj(TestObject, 'Sasha', 6));
            tlist.Add(host.newObj(TestObject, 'Brian', 3));

            olist = host.newObj(System.Collections.Generic.List(System.Object));
            olist.Add({ name: 'Brian', age: 3 });
            olist.Add({ name: 'Sasha', age: 6 });
            olist.Add({ name: 'Cillian', age: 8 });
            olist.Add({ name: 'Shane', age: 16 });
            olist.Add({ name: 'Eóin', age: 20 });

            dict = host.newObj(System.Collections.Generic.Dictionary(System.String, System.String));
            dict.Add('foo', 'bar');
            dict.Add('baz', 'qux');
            value = host.newVar(System.String);
            result = dict.TryGetValue('foo', value.out);

            bag = host.newObj();
            bag.method = function (x) { System.Console.WriteLine(x * x); };
            bag.proc = host.del(System.Action(System.Object), bag.method);

            expando = host.newObj(System.Dynamic.ExpandoObject);
            expandoCollection = host.cast(System.Collections.Generic.ICollection(System.Collections.Generic.KeyValuePair(System.String, System.Object)), expando);

            function onChange(s, e) {
                System.Console.WriteLine('Property changed: {0}; new value: {1}', e.PropertyName, s[e.PropertyName]);
            };
            function onStaticChange(s, e) {
                System.Console.WriteLine('Property changed: {0}; new value: {1} (static event)', e.PropertyName, e.PropertyValue);
            };
            eventCookie = tlist.Item(0).Change.connect(onChange);
            staticEventCookie = TestObject.StaticChange.connect(onStaticChange);
            tlist.Item(0).Name = 'Jerry';
            tlist.Item(1).Name = 'Ellis';
            tlist.Item(0).Name = 'Eóin';
            tlist.Item(1).Name = 'Shane';
            eventCookie.disconnect();
            staticEventCookie.disconnect();
            tlist.Item(0).Name = 'Jerry';
            tlist.Item(1).Name = 'Ellis';
            tlist.Item(0).Name = 'Eóin';
            tlist.Item(1).Name = 'Shane';
        ";

        private const string generalScriptOutput =
        @"
            Property changed: Name; new value: Jerry
            Property changed: Name; new value: Jerry (static event)
            Property changed: Name; new value: Ellis (static event)
            Property changed: Name; new value: Eóin
            Property changed: Name; new value: Eóin (static event)
            Property changed: Name; new value: Shane (static event)
        ";

        public object TestProperty { get; set; }

        public static object StaticTestProperty { get; set; }

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
