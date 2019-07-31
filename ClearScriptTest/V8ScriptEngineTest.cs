// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.ClearScript.JavaScript;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    [DeploymentItem("JavaScript", "JavaScript")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
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
            BaseTestCleanup();
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
        public void V8ScriptEngine_AddHostObject_GlobalMembers_Overwrite()
        {
            const int fooFirst = 123;
            const int fooSecond = 456;
            const int barSecond = 789;
            engine.AddHostObject("bar", HostItemFlags.GlobalMembers, new { second = barSecond });
            engine.AddHostObject("foo", HostItemFlags.GlobalMembers, new { second = fooSecond });
            engine.AddHostObject("foo", HostItemFlags.GlobalMembers, new { first = fooFirst });
            Assert.AreEqual(fooFirst, engine.Evaluate("first"));
            Assert.AreEqual(barSecond, engine.Evaluate("second"));
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
        public void V8ScriptEngine_AddHostType_DefaultName()
        {
            engine.AddHostType(typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new Random()"), typeof(Random));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddHostType_DefaultNameGeneric()
        {
            engine.AddHostType(typeof(List<int>));
            Assert.IsInstanceOfType(engine.Evaluate("new List()"), typeof(List<int>));

            engine.AddHostType(typeof(Dictionary<,>));
            engine.AddHostType(typeof(int));
            engine.AddHostType(typeof(double));
            Assert.IsInstanceOfType(engine.Evaluate("new Dictionary(Int32, Double, 100)"), typeof(Dictionary<int, double>));
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
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, true, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, false, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DocumentInfo_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName), "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DocumentInfo_WithDocumentUri()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(@"c:\foo\bar\baz\" + documentName);
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentUri) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DocumentInfo_WithDocumentUri_Relative()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(documentName, UriKind.Relative);
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentUri) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DocumentInfo_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName) { Flags = DocumentFlags.IsTransient }, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Evaluate_DocumentInfo_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
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
            engine.EnableDocumentNameTracking();
            engine.Execute(documentName, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            engine.Execute(documentName, true, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            engine.Execute(documentName, false, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DocumentInfo_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            engine.Execute(new DocumentInfo(documentName), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DocumentInfo_WithDocumentUri()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(@"c:\foo\bar\baz\" + documentName);
            engine.EnableDocumentNameTracking();
            engine.Execute(new DocumentInfo(documentUri), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DocumentInfo_WithDocumentUri_Relative()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(documentName, UriKind.Relative);
            engine.EnableDocumentNameTracking();
            engine.Execute(new DocumentInfo(documentUri), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DocumentInfo_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            engine.Execute(new DocumentInfo(documentName) { Flags = DocumentFlags.IsTransient }, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_DocumentInfo_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.EnableDocumentNameTracking();
            engine.Execute(new DocumentInfo(documentName) { Flags = DocumentFlags.None }, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Execute_CompiledScript()
        {
            using (var script = engine.Compile("epi = Math.E * Math.PI"))
            {
                engine.Execute(script);
                Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            }
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
        public void V8ScriptEngine_Invoke_ScriptFunction()
        {
            engine.Execute("function foo(x) { return x * Math.PI; }");
            Assert.AreEqual(Math.E * Math.PI, engine.Invoke("foo", Math.E));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Invoke_HostDelegate()
        {
            engine.Script.foo = new Func<double, double>(x => x * Math.PI);
            Assert.AreEqual(Math.E * Math.PI, engine.Invoke("foo", Math.E));
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

            TestUtil.AssertException<OperationCanceledException>(() =>
            {
                try
                {
                    engine.Execute("checkpoint.Set(); while (true) { var foo = test.foo; }");
                }
                catch (ScriptInterruptedException exception)
                {
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });

            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Interrupt_AwaitDebuggerAndPauseOnStart()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);

            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.Sleep(1000);
                engine.Interrupt();
            });

            TestUtil.AssertException<OperationCanceledException>(() =>
            {
                try
                {
                    engine.Evaluate("Math.E * Math.PI");
                }
                catch (ScriptInterruptedException exception)
                {
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });

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
        public void V8ScriptEngine_ContinuationCallback_AwaitDebuggerAndPauseOnStart()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart);

            engine.ContinuationCallback = () => false;
            TestUtil.AssertException<OperationCanceledException>(() => engine.Evaluate("Math.E * Math.PI"));
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
            engine.Execute("function foo(x) { return x * x; }");
            Assert.AreEqual(25, engine.Script.foo(5));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Method_Intrinsic()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Script.eval("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    Dim host As New HostFunctions
                    engine.Script.host = host
                    Assert.AreSame(host, engine.Script.host)
                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Scalar_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    Dim value = 123
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Enum_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    Dim value = DayOfWeek.Wednesday
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Property_Struct_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    Dim value As New DateTime(2007, 5, 22, 6, 15, 43)
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Index_ArrayItem_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine

                    Dim index = 5
                    engine.Execute(""foo = []"")

                    engine.Script.foo(index) = engine.Script.Math.PI
                    rem Assert.AreEqual(Math.PI, engine.Script.foo(index))
                    rem Assert.AreEqual(index + 1, engine.Evaluate(""foo.length""))

                    rem engine.Script.foo(index) = engine.Script.Math.E
                    rem Assert.AreEqual(Math.E, engine.Script.foo(index))
                    rem Assert.AreEqual(index + 1, engine.Evaluate(""foo.length""))

                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Index_Property_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine

                    Dim name = ""bar""
                    engine.Execute(""foo = {}"")

                    engine.Script.foo(name) = engine.Script.Math.PI
                    Assert.AreEqual(Math.PI, engine.Script.foo(name))
                    Assert.AreEqual(Math.PI, engine.Script.foo.bar)

                    engine.Script.foo(name) = engine.Script.Math.E
                    Assert.AreEqual(Math.E, engine.Script.foo(name))
                    Assert.AreEqual(Math.E, engine.Script.foo.bar)

                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Method_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    engine.Execute(""function foo(x) { return x * x; }"")
                    Assert.AreEqual(25, engine.Script.foo(5))
                End Using
            ");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Script_Method_Intrinsic_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    Assert.AreEqual(Math.E * Math.PI, engine.Script.eval(""Math.E * Math.PI""))
                End Using
            ");
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

            WeakReference wr = null;

            new Action(() =>
            {
                var x = new object();
                wr = new WeakReference(x);
                engine.Script.x = x;

                x = null;
                engine.Script.x = null;
            })();

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
        public void V8ScriptEngine_new_GenericNested()
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
        public void V8ScriptEngine_General_Precompiled_Execute()
        {
            using (var script = engine.Compile(generalScript))
            {
                using (var console = new StringWriter())
                {
                    var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                    clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                    engine.AddHostObject("host", new ExtendedHostFunctions());
                    engine.AddHostObject("clr", clr);

                    engine.Execute(script);
                    Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));

                    console.GetStringBuilder().Clear();
                    Assert.AreEqual(string.Empty, console.ToString());

                    engine.Execute(script);
                    Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_ParserCache()
        {
            #pragma warning disable CS0618 // Type or member is obsolete (V8CacheKind.Parser)

            engine.Dispose();
            engine = new V8ScriptEngine(); // default engine enables debugging, which disables caching (in older V8 versions)

            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Parser, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Parser, cacheBytes, out cacheAccepted))
            {
                Assert.IsTrue(cacheAccepted);
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

            #pragma warning restore CS0618 // Type or member is obsolete (V8CacheKind.Parser)
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_ParserCache_BadData()
        {
            #pragma warning disable CS0618 // Type or member is obsolete (V8CacheKind.Parser)

            engine.Dispose();
            engine = new V8ScriptEngine(); // default engine enables debugging, which disables caching (in older V8 versions)

            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Parser, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            cacheBytes = cacheBytes.Take(cacheBytes.Length - 1).ToArray();

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Parser, cacheBytes, out cacheAccepted))
            {
                Assert.IsFalse(cacheAccepted);
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

            #pragma warning restore CS0618 // Type or member is obsolete (V8CacheKind.Parser)
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_ParserCache_DebuggingEnabled()
        {
            #pragma warning disable CS0618 // Type or member is obsolete (V8CacheKind.Parser)

            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Parser, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Parser, cacheBytes, out cacheAccepted))
            {
                Assert.IsTrue(cacheAccepted);
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

            #pragma warning restore CS0618 // Type or member is obsolete (V8CacheKind.Parser)
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_General_CodeCache()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(); // default engine enables debugging, which disables caching (in older V8 versions)

            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Code, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Code, cacheBytes, out cacheAccepted))
            {
                Assert.IsTrue(cacheAccepted);
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
        public void V8ScriptEngine_General_CodeCache_BadData()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(); // default engine enables debugging, which disables caching (in older V8 versions)

            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Code, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            cacheBytes = cacheBytes.Take(cacheBytes.Length - 1).ToArray();

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Code, cacheBytes, out cacheAccepted))
            {
                Assert.IsFalse(cacheAccepted);
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
        public void V8ScriptEngine_General_CodeCache_DebuggingEnabled()
        {
            byte[] cacheBytes;
            using (var tempEngine = new V8ScriptEngine())
            {
                using (tempEngine.Compile(generalScript, V8CacheKind.Code, out cacheBytes))
                {
                }
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 2000); // typical size is ~4K

            bool cacheAccepted;
            using (var script = engine.Compile(generalScript, V8CacheKind.Code, cacheBytes, out cacheAccepted))
            {
                Assert.IsTrue(cacheAccepted);
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
                    Assert.IsNotNull(exception.ScriptException);
                    Assert.AreEqual("SyntaxError", exception.ScriptException.constructor.name);
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
                    Assert.AreEqual(123, exception.ScriptException);
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
                    Assert.AreEqual("TypeError", exception.ScriptException.constructor.name);
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
                    engine.Evaluate("host.proc(0)");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.AreEqual("Error", exception.ScriptException.constructor.name);
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
                    Assert.AreEqual("TypeError", exception.ScriptException.constructor.name);
                    Assert.IsNull(exception.InnerException);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_NestedSyntaxError()
        {
            engine.AddHostObject("engine", engine);
            engine.Execute("good.js", "function bar() { engine.Execute('bad.js', 'function foo() { int c; }'); }");

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Script.bar();
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.AreEqual("Error", exception.ScriptException.constructor.name);
                    Assert.IsNotNull(exception.InnerException);

                    var hostException = exception.InnerException;
                    Assert.IsInstanceOfType(hostException, typeof(TargetInvocationException));
                    TestUtil.AssertValidException(hostException);
                    Assert.IsNotNull(hostException.InnerException);

                    var nestedException = hostException.InnerException as ScriptEngineException;
                    Assert.IsNotNull(nestedException);
                    // ReSharper disable once AccessToDisposedClosure
                    TestUtil.AssertValidException(engine, nestedException);
                    // ReSharper disable once PossibleNullReferenceException
                    Assert.IsNull(nestedException.InnerException);
                    Assert.IsTrue(nestedException.ErrorDetails.Contains("at bad.js:1:22 -> "));
                    Assert.IsTrue(nestedException.ErrorDetails.Contains("at bar (good.js:1:25)"));

                    Assert.AreEqual("Error: " + hostException.GetBaseException().Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_NestedScriptError()
        {
            using (var innerEngine = new V8ScriptEngine("inner", V8ScriptEngineFlags.EnableDebugging))
            {
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
                        Assert.AreEqual("Error", exception.ScriptException.constructor.name);
                        Assert.IsNotNull(exception.InnerException);

                        var hostException = exception.InnerException;
                        Assert.IsInstanceOfType(hostException, typeof(TargetInvocationException));
                        TestUtil.AssertValidException(hostException);
                        Assert.IsNotNull(hostException.InnerException);

                        var nestedException = hostException.InnerException as ScriptEngineException;
                        Assert.IsNotNull(nestedException);
                        // ReSharper disable once AccessToDisposedClosure
                        TestUtil.AssertValidException(innerEngine, nestedException);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.IsNull(nestedException.InnerException);

                        Assert.AreEqual("Error: " + hostException.GetBaseException().Message, exception.Message);
                        throw;
                    }
                });
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ErrorHandling_NestedHostException()
        {
            using (var innerEngine = new V8ScriptEngine("inner", V8ScriptEngineFlags.EnableDebugging))
            {
                innerEngine.AddHostObject("host", new HostFunctions());
                engine.AddHostObject("engine", innerEngine);

                TestUtil.AssertException<ScriptEngineException>(() =>
                {
                    try
                    {
                        engine.Execute("engine.Evaluate('host.proc(0)')");
                    }
                    catch (ScriptEngineException exception)
                    {
                        TestUtil.AssertValidException(engine, exception);
                        Assert.AreEqual("Error", exception.ScriptException.constructor.name);
                        Assert.IsNotNull(exception.InnerException);

                        var hostException = exception.InnerException;
                        Assert.IsInstanceOfType(hostException, typeof(TargetInvocationException));
                        TestUtil.AssertValidException(hostException);
                        Assert.IsNotNull(hostException.InnerException);

                        var nestedException = hostException.InnerException as ScriptEngineException;
                        Assert.IsNotNull(nestedException);
                        // ReSharper disable once AccessToDisposedClosure
                        TestUtil.AssertValidException(innerEngine, nestedException);
                        // ReSharper disable once PossibleNullReferenceException
                        Assert.IsNotNull(nestedException.InnerException);

                        var nestedHostException = nestedException.InnerException;
                        Assert.IsInstanceOfType(nestedHostException, typeof(RuntimeBinderException));
                        TestUtil.AssertValidException(nestedHostException);
                        Assert.IsNull(nestedHostException.InnerException);

                        Assert.AreEqual("Error: " + nestedHostException.Message, nestedException.Message);
                        Assert.AreEqual("Error: " + hostException.GetBaseException().Message, exception.Message);
                        throw;
                    }
                });
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeHeapSize()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"x = []; while (true) { x.push(x); }";

            engine.MaxRuntimeHeapSize = (UIntPtr)limit;

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute(code);
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    Assert.IsNull(exception.ScriptException);
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
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeHeapSize_Recovery()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"x = []; while (true) { x.push(x); }";

            engine.MaxRuntimeHeapSize = (UIntPtr)limit;

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute(code);
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });

            engine.MaxRuntimeHeapSize = (UIntPtr)(limit * 64);
            engine.Execute("x = 5");
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeHeapSize_Dual()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"x = []; for (i = 0; i < 16 * 1024 * 1024; i++) { x.push(x); }";

            engine.Execute(code);
            engine.CollectGarbage(true);
            var usedHeapSize = engine.GetRuntimeHeapInfo().UsedHeapSize;

            engine.Dispose();
            engine = new V8ScriptEngine { MaxRuntimeHeapSize = (UIntPtr)limit };

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute(code);
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });

            engine.CollectGarbage(true);
            Assert.IsTrue(usedHeapSize > engine.GetRuntimeHeapInfo().UsedHeapSize);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeHeapSize_ShortBursts()
        {
            const int limit = 4 * 1024 * 1024;
            const string code = @"for (i = 0; i < 1024 * 1024; i++) { x.push(x); }";

            engine.MaxRuntimeHeapSize = (UIntPtr)limit;
            engine.RuntimeHeapSizeSampleInterval = TimeSpan.FromMilliseconds(30000);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("x = []");
                    using (var script = engine.Compile(code))
                    {
                        while (true)
                        {
                            engine.Evaluate(script);
                        }
                    }
                }
                catch (ScriptEngineException exception)
                {
                    Assert.IsTrue(exception.IsFatal);
                    Assert.IsNull(exception.ScriptException);
                    throw;
                }
            });
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
        public void V8ScriptEngine_DynamicHostObject_StaticType_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("host.toStaticType(testObject)('foo', 'bar', 'baz', 'qux')"));
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
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo = function (x) { return x.length; }"), typeof(DynamicObject));
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
        public void V8ScriptEngine_DynamicHostObject_Element_Index()
        {
            engine.Script.testObject = new DynamicTestObject { DisableInvocation = true, DisableDynamicMembers = true };
            engine.Script.host = new HostFunctions();

            Assert.IsInstanceOfType(engine.Evaluate("testObject[123]"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 123)"), typeof(Undefined));
            Assert.AreEqual(456, engine.Evaluate("testObject[123] = 456"));
            Assert.AreEqual(456, engine.Evaluate("testObject[123]"));
            Assert.AreEqual(456, engine.Evaluate("host.getElement(testObject, 123)"));
            Assert.IsTrue((bool)engine.Evaluate("delete testObject[123]"));
            Assert.IsInstanceOfType(engine.Evaluate("testObject[123]"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 123)"), typeof(Undefined));

            Assert.IsInstanceOfType(engine.Evaluate("testObject['foo']"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 'foo')"), typeof(Undefined));
            Assert.AreEqual("bar", engine.Evaluate("testObject['foo'] = 'bar'"));
            Assert.AreEqual("bar", engine.Evaluate("testObject['foo']"));
            Assert.AreEqual("bar", engine.Evaluate("host.getElement(testObject, 'foo')"));
            Assert.IsTrue((bool)engine.Evaluate("delete testObject['foo']"));
            Assert.IsInstanceOfType(engine.Evaluate("testObject['foo']"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 'foo')"), typeof(Undefined));

            Assert.IsInstanceOfType(engine.Evaluate("testObject('foo', 'bar', 'baz')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 'foo', 'bar', 'baz')"), typeof(Undefined));
            Assert.AreEqual("qux", engine.Evaluate("host.setElement(testObject, 'qux', 'foo', 'bar', 'baz')"));
            Assert.AreEqual("qux", engine.Evaluate("testObject('foo', 'bar', 'baz')"));
            Assert.AreEqual("qux", engine.Evaluate("host.getElement(testObject, 'foo', 'bar', 'baz')"));
            Assert.IsInstanceOfType(engine.Evaluate("host.setElement(testObject, undefined, 'foo', 'bar', 'baz')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject('foo', 'bar', 'baz')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 'foo', 'bar', 'baz')"), typeof(Undefined));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DynamicHostObject_Convert()
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

            Assert.IsTrue(((string)engine.Evaluate("foo()")).EndsWith("    at baz (Script:2:41)\n    at bar (Script:3:41)\n    at foo (Script:4:41)\n    at Script [2] [temp]:1:1", StringComparison.Ordinal));
            Assert.IsTrue(((string)engine.Script.foo()).EndsWith("    at baz (Script:2:41)\n    at bar (Script:3:41)\n    at foo (Script:4:41)", StringComparison.Ordinal));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeHeapSize_Plumbing()
        {
            using (var runtime = new V8Runtime())
            {
                using (var engine1 = runtime.CreateScriptEngine())
                {
                    using (var engine2 = runtime.CreateScriptEngine())
                    {
                        var value = (UIntPtr)123456;
                        engine1.MaxRuntimeHeapSize = value;
                        Assert.AreEqual(value, engine1.MaxRuntimeHeapSize);
                        Assert.AreEqual(value, engine2.MaxRuntimeHeapSize);
                        Assert.AreEqual(value, runtime.MaxHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, engine1.MaxRuntimeStackUsage);
                        Assert.AreEqual(UIntPtr.Zero, engine2.MaxRuntimeStackUsage);
                        Assert.AreEqual(UIntPtr.Zero, runtime.MaxStackUsage);

                        value = (UIntPtr)654321;
                        runtime.MaxHeapSize = value;
                        Assert.AreEqual(value, engine1.MaxRuntimeHeapSize);
                        Assert.AreEqual(value, engine2.MaxRuntimeHeapSize);
                        Assert.AreEqual(value, runtime.MaxHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, engine1.MaxRuntimeStackUsage);
                        Assert.AreEqual(UIntPtr.Zero, engine2.MaxRuntimeStackUsage);
                        Assert.AreEqual(UIntPtr.Zero, runtime.MaxStackUsage);
                    }
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_RuntimeHeapSizeSampleInterval_Plumbing()
        {
            using (var runtime = new V8Runtime())
            {
                using (var engine1 = runtime.CreateScriptEngine())
                {
                    using (var engine2 = runtime.CreateScriptEngine())
                    {
                        var value = TimeSpan.FromMilliseconds(123456789.0);
                        engine1.RuntimeHeapSizeSampleInterval = value;
                        Assert.AreEqual(value, engine1.RuntimeHeapSizeSampleInterval);
                        Assert.AreEqual(value, engine2.RuntimeHeapSizeSampleInterval);
                        Assert.AreEqual(value, runtime.HeapSizeSampleInterval);

                        value = TimeSpan.FromMilliseconds(987654321.0);
                        runtime.HeapSizeSampleInterval = value;
                        Assert.AreEqual(value, engine1.RuntimeHeapSizeSampleInterval);
                        Assert.AreEqual(value, engine2.RuntimeHeapSizeSampleInterval);
                        Assert.AreEqual(value, runtime.HeapSizeSampleInterval);
                    }
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_Plumbing()
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
                        Assert.AreEqual(UIntPtr.Zero, engine1.MaxRuntimeHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, engine2.MaxRuntimeHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, runtime.MaxHeapSize);

                        value = (UIntPtr)654321;
                        runtime.MaxStackUsage = value;
                        Assert.AreEqual(value, engine1.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, engine2.MaxRuntimeStackUsage);
                        Assert.AreEqual(value, runtime.MaxStackUsage);
                        Assert.AreEqual(UIntPtr.Zero, engine1.MaxRuntimeHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, engine2.MaxRuntimeHeapSize);
                        Assert.AreEqual(UIntPtr.Zero, runtime.MaxHeapSize);
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
            engine.MaxRuntimeStackUsage = (UIntPtr)(32 * 1024);
            dynamic foo = engine.Evaluate("(function () { arguments.callee(); })");
            engine.Script.bar = new Action(() => foo());
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("bar()"), false);
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_Alternating()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(32 * 1024);
            dynamic foo = engine.Evaluate("(function () { bar(); })");
            engine.Script.bar = new Action(() => foo());
            TestUtil.AssertException<ScriptEngineException>(() => foo(), false);
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_MaxRuntimeStackUsage_Expansion()
        {
            engine.MaxRuntimeStackUsage = (UIntPtr)(32 * 1024);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("count = 0; (function () { count++; arguments.callee(); })()"), false);
            var count1 = engine.Script.count;
            engine.MaxRuntimeStackUsage = (UIntPtr)(64 * 1024);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("count = 0; (function () { count++; arguments.callee(); })()"), false);
            var count2 = engine.Script.count;
            Assert.IsTrue(count2 >= (count1 * 2));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_Dictionary()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                dict = host.newComObj('Scripting.Dictionary');
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_Dictionary()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                Dict = host.comType('Scripting.Dictionary');
                dict = host.newObj(Dict);
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMObject("fso", "Scripting.FileSystemObject");
            engine.Execute(@"
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_Dictionary()
        {
            engine.AddCOMObject("dict", new Guid("{ee09b103-97e0-11cf-978f-00a02463e06f}"));
            engine.Execute(@"
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMType("FSO", "Scripting.FileSystemObject");
            engine.Execute(@"
                fso = new FSO();
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_Dictionary()
        {
            engine.AddCOMType("Dict", new Guid("{ee09b103-97e0-11cf-978f-00a02463e06f}"));
            engine.Execute(@"
                dict = new Dict();
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_XMLHTTP()
        {
            var status = 0;
            string data = null;

            var thread = new Thread(() =>
            {
                using (var testEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
                {
                    testEngine.Script.onComplete = new Action<int, string>((xhrStatus, xhrData) =>
                    {
                        status = xhrStatus;
                        data = xhrData;
                        Dispatcher.ExitAllFrames();
                    });

                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        // ReSharper disable AccessToDisposedClosure

                        testEngine.AddCOMType("XMLHttpRequest", "MSXML2.XMLHTTP");
                        testEngine.Execute(@"
                            xhr = new XMLHttpRequest();
                            xhr.open('POST', 'http://httpbin.org/post', true);
                            xhr.onreadystatechange = function () {
                                if (xhr.readyState == 4) {
                                    onComplete(xhr.status, JSON.parse(xhr.responseText).data);
                                }
                            };
                            xhr.send('Hello, world!');
                        ");

                        // ReSharper restore AccessToDisposedClosure
                    }));

                    Dispatcher.Run();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            Assert.AreEqual(200, status);
            Assert.AreEqual("Hello, world!", data);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnableAutoHostVariables()
        {
            const string pre = "123";
            var value = "foo";
            const int post = 456;

            engine.Execute("function foo(a, x, b) { var y = x; x = a + 'bar' + b; return y; }");
            Assert.AreEqual("foo", engine.Script.foo(pre, ref value, post));
            Assert.AreEqual("foo", value);  // JavaScript doesn't support output parameters

            engine.EnableAutoHostVariables = true;
            engine.Execute("function foo(a, x, b) { var y = x.value; x.value = a + 'bar' + b; return y; }");
            Assert.AreEqual("foo", engine.Script.foo(pre, ref value, post));
            Assert.AreEqual("123bar456", value);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnableAutoHostVariables_Delegate()
        {
            const string pre = "123";
            var value = "foo";
            const int post = 456;

            engine.Execute("function foo(a, x, b) { var y = x; x = a + 'bar' + b; return y; }");
            var del = DelegateFactory.CreateDelegate<TestDelegate>(engine, engine.Evaluate("foo"));
            Assert.AreEqual("foo", del(pre, ref value, post));
            Assert.AreEqual("foo", value);  // JavaScript doesn't support output parameters

            engine.EnableAutoHostVariables = true;
            engine.Execute("function foo(a, x, b) { var y = x.value; x.value = a + 'bar' + b; return y; }");
            del = DelegateFactory.CreateDelegate<TestDelegate>(engine, engine.Evaluate("foo"));
            Assert.AreEqual("foo", del(pre, ref value, post));
            Assert.AreEqual("123bar456", value);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ExceptionMarshaling()
        {
            Exception exception = new IOException("something awful happened");
            engine.AddRestrictedHostObject("exception", exception);

            engine.Script.foo = new Action(() => { throw exception; });

            engine.Execute(@"
                function bar() {
                    try {
                        foo();
                        return false;
                    }
                    catch (ex) {
                        return ex.hostException.GetBaseException() === exception;
                    }
                }
            ");

            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("bar()")));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Current()
        {
            // ReSharper disable AccessToDisposedClosure

            using (var innerEngine = new V8ScriptEngine())
            {
                engine.Script.test = new Action(() =>
                {
                    innerEngine.Script.test = new Action(() => Assert.AreSame(innerEngine, ScriptEngine.Current));
                    Assert.AreSame(engine, ScriptEngine.Current);
                    innerEngine.Execute("test()");
                    innerEngine.Script.test();
                    Assert.AreSame(engine, ScriptEngine.Current);
                });

                Assert.IsNull(ScriptEngine.Current);
                engine.Execute("test()");
                engine.Script.test();
                Assert.IsNull(ScriptEngine.Current);
            }

            // ReSharper restore AccessToDisposedClosure
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnableNullResultWrapping()
        {
            var testValue = new[] { 1, 2, 3, 4, 5 };
            engine.Script.host = new HostFunctions();
            engine.Script.foo = new NullResultWrappingTestObject<int[]>(testValue);

            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.Value === null")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.Value)")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("foo.NullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.NullValue)")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.WrappedNullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.WrappedNullValue)")));

            Assert.AreSame(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = true;
            Assert.AreSame(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = false;
            Assert.AreSame(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnableNullResultWrapping_String()
        {
            const string testValue = "bar";
            engine.Script.host = new HostFunctions();
            engine.Script.foo = new NullResultWrappingTestObject<string>(testValue);

            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.Value === null")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.Value)")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("foo.NullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.NullValue)")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.WrappedNullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.WrappedNullValue)")));

            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = true;
            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = false;
            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnableNullResultWrapping_Nullable()
        {
            int? testValue = 12345;
            engine.Script.host = new HostFunctions();
            engine.Script.foo = new NullResultWrappingTestObject<int?>(testValue);

            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.Value === null")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.Value)")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("foo.NullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.NullValue)")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("foo.WrappedNullValue === null")));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("host.isNull(foo.WrappedNullValue)")));

            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = true;
            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.NullValue)"));

            engine.EnableNullResultWrapping = false;
            Assert.AreEqual(testValue, engine.Evaluate("foo.Method(foo.Value)"));
            Assert.IsNull(engine.Evaluate("foo.Method(foo.WrappedNullValue)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("foo.Method(foo.NullValue)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DefaultProperty()
        {
            engine.Script.foo = new DefaultPropertyTestObject();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Item.set('ghi', 321)");
            Assert.AreEqual(321, engine.Evaluate("foo('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Item('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Item.get('ghi')"));
            Assert.IsNull(engine.Evaluate("foo('jkl')"));

            engine.Execute("foo.Item.set(DayOfWeek.Saturday, -123)");
            Assert.AreEqual(-123, engine.Evaluate("foo(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Item(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Item.get(DayOfWeek.Saturday)"));
            Assert.IsNull(engine.Evaluate("foo(DayOfWeek.Sunday)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DefaultProperty_FieldTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Field.Item.set('ghi', 321)");
            Assert.AreEqual(321, engine.Evaluate("foo.Field('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Field.Item('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Field.Item.get('ghi')"));
            Assert.IsNull(engine.Evaluate("foo.Field('jkl')"));

            engine.Execute("foo.Field.Item.set(DayOfWeek.Saturday, -123)");
            Assert.AreEqual(-123, engine.Evaluate("foo.Field(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Field.Item(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Field.Item.get(DayOfWeek.Saturday)"));
            Assert.IsNull(engine.Evaluate("foo.Field(DayOfWeek.Sunday)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DefaultProperty_PropertyTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Property.Item.set('ghi', 321)");
            Assert.AreEqual(321, engine.Evaluate("foo.Property('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Property.Item('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Property.Item.get('ghi')"));
            Assert.IsNull(engine.Evaluate("foo.Property('jkl')"));

            engine.Execute("foo.Property.Item.set(DayOfWeek.Saturday, -123)");
            Assert.AreEqual(-123, engine.Evaluate("foo.Property(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Property.Item(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Property.Item.get(DayOfWeek.Saturday)"));
            Assert.IsNull(engine.Evaluate("foo.Property(DayOfWeek.Sunday)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DefaultProperty_MethodTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Method().Item.set('ghi', 321)");
            Assert.AreEqual(321, engine.Evaluate("foo.Method()('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Method().Item('ghi')"));
            Assert.AreEqual(321, engine.Evaluate("foo.Method().Item.get('ghi')"));
            Assert.IsNull(engine.Evaluate("foo.Method()('jkl')"));

            engine.Execute("foo.Method().Item.set(DayOfWeek.Saturday, -123)");
            Assert.AreEqual(-123, engine.Evaluate("foo.Method()(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Method().Item(DayOfWeek.Saturday)"));
            Assert.AreEqual(-123, engine.Evaluate("foo.Method().Item.get(DayOfWeek.Saturday)"));
            Assert.IsNull(engine.Evaluate("foo.Method()(DayOfWeek.Sunday)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DefaultProperty_Indexer()
        {
            engine.Script.dict = new Dictionary<string, object> { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            engine.Execute("item = dict.Item");

            Assert.AreEqual(123, engine.Evaluate("item('abc')"));
            Assert.AreEqual(456, engine.Evaluate("item('def')"));
            Assert.AreEqual(789, engine.Evaluate("item('ghi')"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("item('jkl')"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_PropertyAndMethodWithSameName()
        {
            engine.AddHostObject("lib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System", "System.Core"));

            engine.Script.dict = new Dictionary<string, object> { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            Assert.AreEqual(3, engine.Evaluate("dict.Count"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("dict.Count()"));

            engine.Script.listDict = new ListDictionary { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            Assert.AreEqual(3, engine.Evaluate("listDict.Count"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("listDict.Count()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_toFunction_Delegate()
        {
            engine.Script.foo = new Func<int, double>(arg => arg * Math.PI);
            Assert.AreEqual(123 * Math.PI, engine.Evaluate("foo(123)"));
            Assert.AreEqual("function", engine.Evaluate("typeof foo.toFunction"));
            Assert.AreEqual("function", engine.Evaluate("typeof foo.toFunction()"));
            Assert.AreEqual(456 * Math.PI, engine.Evaluate("foo.toFunction()(456)"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new foo()"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new (foo.toFunction())()"));

            engine.Script.bar = new VarArgDelegate((pre, args) => args.Aggregate((int)pre, (value, arg) => value + (int)arg));
            Assert.AreEqual(3330, engine.Evaluate("bar(123, 456, 789, 987, 654, 321)"));
            Assert.AreEqual("function", engine.Evaluate("typeof bar.toFunction"));
            Assert.AreEqual("function", engine.Evaluate("typeof bar.toFunction()"));
            Assert.AreEqual(2934, engine.Evaluate("bar.toFunction()(135, 579, 975, 531, 135, 579)"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new bar()"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new (bar.toFunction())()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_toFunction_Method()
        {
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("function", engine.Evaluate("typeof host.newObj.toFunction"));
            Assert.AreEqual("function", engine.Evaluate("typeof host.newObj.toFunction()"));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj.toFunction()()"), typeof(PropertyBag));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new host.newObj()"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new (host.newObj.toFunction())()"));

            engine.AddHostType(typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random, 100)"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj.toFunction()(Random, 100)"), typeof(Random));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_toFunction_Type()
        {
            engine.AddHostType(typeof(Random));
            Assert.AreEqual("function", engine.Evaluate("typeof Random.toFunction"));
            Assert.AreEqual("function", engine.Evaluate("typeof Random.toFunction()"));
            Assert.IsInstanceOfType(engine.Evaluate("new Random()"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new Random(100)"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new (Random.toFunction())()"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new (Random.toFunction())(100)"), typeof(Random));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("Random(100)"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("(Random.toFunction())(100)"));

            engine.AddHostType(typeof(Dictionary<,>));
            engine.AddHostType(typeof(int));
            Assert.AreEqual("function", engine.Evaluate("typeof Dictionary.toFunction"));
            Assert.AreEqual("function", engine.Evaluate("typeof Dictionary.toFunction()"));
            Assert.IsInstanceOfType(engine.Evaluate("Dictionary(Int32, Int32)"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("Dictionary.toFunction()(Int32, Int32)"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("new Dictionary(Int32, Int32, 100)"), typeof(Dictionary<int, int>));
            Assert.IsInstanceOfType(engine.Evaluate("new (Dictionary.toFunction())(Int32, Int32, 100)"), typeof(Dictionary<int, int>));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_toFunction_None()
        {
            engine.Script.foo = new Random();
            Assert.IsInstanceOfType(engine.Evaluate("foo"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("foo.toFunction"), typeof(Undefined));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_NativeEnumerator()
        {
            var array = Enumerable.Range(0, 10).ToArray();
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var item of array) {
                        result += item;
                    }
                    return result;
                }
            ");

            // run test several times to verify workaround for V8 optimizer bug
            for (var i = 0; i < 64; i++)
            {
                Assert.AreEqual(array.Aggregate((current, next) => current + next), engine.Script.sum(array));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_NativeEnumerator_Generic()
        {
            var array = Enumerable.Range(0, 10).Select(value => (IConvertible)value).ToArray();
            engine.Script.culture = CultureInfo.InvariantCulture;
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var item of array) {
                        result += item.ToInt32(culture);
                    }
                    return result;
                }
            ");

            // run test several times to verify workaround for V8 optimizer bug
            for (var i = 0; i < 64; i++)
            {
                Assert.AreEqual(array.Aggregate((current, next) => Convert.ToInt32(current) + Convert.ToInt32(next)), engine.Script.sum(array));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_NativeEnumerator_NonGeneric()
        {
            var array = Enumerable.Range(0, 10).ToArray();
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var item of array) {
                        result += item;
                    }
                    return result;
                }
            ");

            // run test several times to verify workaround for V8 optimizer bug
            for (var i = 0; i < 64; i++)
            {
                Assert.AreEqual(array.Aggregate((current, next) => current + next), engine.Script.sum(HostObject.Wrap(array, typeof(IEnumerable))));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_NativeEnumerator_NonEnumerable()
        {
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var item of array) {
                        result += item;
                    }
                    return result;
                }
            ");

            // run test several times to verify workaround for V8 optimizer bug
            for (var i = 0; i < 64; i++)
            {
                TestUtil.AssertException<NotSupportedException>(() => engine.Script.sum(DayOfWeek.Monday));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_SuppressInstanceMethodEnumeration()
        {
            engine.Script.foo = Enumerable.Range(0, 25).ToArray();
            Assert.AreEqual("ToString", engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'ToString' })"));
            Assert.IsInstanceOfType(engine.Evaluate("foo.ToString"), typeof(HostMethod));
            Assert.AreEqual("System.Int32[]", engine.Evaluate("foo.ToString()"));

            engine.SuppressInstanceMethodEnumeration = true;
            Assert.IsInstanceOfType(engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'ToString' })"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("foo.ToString"), typeof(HostMethod));
            Assert.AreEqual("System.Int32[]", engine.Evaluate("foo.ToString()"));

            engine.SuppressInstanceMethodEnumeration = false;
            Assert.AreEqual("ToString", engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'ToString' })"));
            Assert.IsInstanceOfType(engine.Evaluate("foo.ToString"), typeof(HostMethod));
            Assert.AreEqual("System.Int32[]", engine.Evaluate("foo.ToString()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_SuppressExtensionMethodEnumeration()
        {
            engine.AddHostType(typeof(Enumerable));
            engine.Script.foo = Enumerable.Range(0, 25).ToArray();
            Assert.AreEqual("Count", engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'Count' })"));
            Assert.IsInstanceOfType(engine.Evaluate("foo.Count"), typeof(HostMethod));
            Assert.AreEqual(25, engine.Evaluate("foo.Count()"));

            engine.SuppressExtensionMethodEnumeration = true;
            Assert.IsInstanceOfType(engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'Count' })"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("foo.Count"), typeof(HostMethod));
            Assert.AreEqual(25, engine.Evaluate("foo.Count()"));

            engine.SuppressExtensionMethodEnumeration = false;
            Assert.AreEqual("Count", engine.Evaluate("Object.keys(foo).find(function (key) { return key == 'Count' })"));
            Assert.IsInstanceOfType(engine.Evaluate("foo.Count"), typeof(HostMethod));
            Assert.AreEqual(25, engine.Evaluate("foo.Count()"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ScriptObject()
        {
            var obj = engine.Evaluate("({})") as ScriptObject;
            Assert.IsNotNull(obj);
            Assert.AreSame(engine, obj.Engine);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DateTimeConversion()
        {
            engine.Script.now = DateTime.Now;
            Assert.AreEqual("HostObject", engine.Evaluate("now.constructor.name"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableDateTimeConversion);
            var utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            var now = DateTime.Now;
            engine.Script.now = now;
            Assert.AreEqual("Date", engine.Evaluate("now.constructor.name"));
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - utcEpoch).TotalMilliseconds - Convert.ToDouble(engine.Evaluate("now.valueOf()"))) <= 1.0);

            var utcNow = DateTime.UtcNow;
            engine.Script.now = utcNow;
            Assert.AreEqual("Date", engine.Evaluate("now.constructor.name"));
            Assert.IsTrue(Math.Abs((utcNow - utcEpoch).TotalMilliseconds - Convert.ToDouble(engine.Evaluate("now.valueOf()"))) <= 1.0);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DateTimeConversion_FromScript()
        {
            Assert.IsInstanceOfType(engine.Evaluate("new Date(Date.now())"), typeof(ScriptObject));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableDateTimeConversion);
            var utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            var utcNowObj = engine.Evaluate("now = new Date(Date.now())");
            Assert.IsInstanceOfType(utcNowObj, typeof(DateTime));
            Assert.AreEqual(DateTimeKind.Utc, ((DateTime)utcNowObj).Kind);
            Assert.IsTrue(Math.Abs(((DateTime)utcNowObj - utcEpoch).TotalMilliseconds - Convert.ToDouble(engine.Evaluate("now.valueOf()"))) <= 1.0);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_typeof()
        {
            engine.Script.foo = new Random();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));
            Assert.AreEqual("function", engine.Evaluate("typeof foo.ToString"));

            engine.Script.foo = Enumerable.Range(0, 5).ToArray();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new ArrayList();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new BitArray(100);
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new Hashtable();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new Queue();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new SortedList();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new Stack();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));

            engine.Script.foo = new List<string>();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));
            Assert.AreEqual("function", engine.Evaluate("typeof foo.Item"));

            engine.Script.foo = new ExpandoObject();
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("object", engine.Evaluate("typeof foo"));
            Assert.AreEqual("object", engine.Evaluate("typeof host.toStaticType(foo)"));
        }
        
        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ArrayInvocability()
        {
            engine.Script.foo = Enumerable.Range(123, 5).ToArray();
            Assert.AreEqual(124, engine.Evaluate("foo(1)"));

            engine.Script.foo = new IConvertible[] { "bar" };
            Assert.AreEqual("bar", engine.Evaluate("foo(0)"));

            engine.Script.bar = new List<string>();
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("bar.Add(foo(0))"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_PropertyBagInvocability()
        {
            engine.Script.lib = new HostTypeCollection("mscorlib", "System", "System.Core");
            Assert.IsInstanceOfType(engine.Evaluate("lib('System')"), typeof(PropertyBag));
            Assert.IsInstanceOfType(engine.Evaluate("lib.System('Collections')"), typeof(PropertyBag));
            Assert.IsInstanceOfType(engine.Evaluate("lib('Bogus')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("lib.System('Heinous')"), typeof(Undefined));

            engine.Script.foo = new PropertyBag { { "Null", null } };
            Assert.IsNull(engine.Evaluate("foo.Null"));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("foo.Null(123)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EnforceAnonymousTypeAccess()
        {
            engine.Script.foo = new { bar = 123, baz = "qux" };
            Assert.AreEqual(123, engine.Evaluate("foo.bar"));
            Assert.AreEqual("qux", engine.Evaluate("foo.baz"));

            engine.EnforceAnonymousTypeAccess = true;
            Assert.IsInstanceOfType(engine.Evaluate("foo.bar"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("foo.baz"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.AreEqual(123, engine.Evaluate("foo.bar"));
            Assert.AreEqual("qux", engine.Evaluate("foo.baz"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ScriptObjectMembers()
        {
            engine.Execute(@"
                function Foo() {
                    this.Qux = x => this.Bar = x;
                    this.Xuq = () => this.Baz;
                }
            ");

            var foo = (ScriptObject)engine.Evaluate("new Foo");

            foo.SetProperty("Bar", 123);
            Assert.AreEqual(123, foo.GetProperty("Bar"));

            foo["Baz"] = "abc";
            Assert.AreEqual("abc", foo.GetProperty("Baz"));

            foo.InvokeMethod("Qux", DayOfWeek.Wednesday);
            Assert.AreEqual(DayOfWeek.Wednesday, foo.GetProperty("Bar"));

            foo["Baz"] = BindingFlags.ExactBinding;
            Assert.AreEqual(BindingFlags.ExactBinding, foo.InvokeMethod("Xuq"));

            foo[1] = new HostFunctions();
            Assert.IsInstanceOfType(foo[1], typeof(HostFunctions));
            Assert.IsInstanceOfType(foo[2], typeof(Undefined));

            var names = foo.PropertyNames.ToArray();
            Assert.AreEqual(4, names.Length);
            Assert.IsTrue(names.Contains("Bar"));
            Assert.IsTrue(names.Contains("Baz"));
            Assert.IsTrue(names.Contains("Qux"));
            Assert.IsTrue(names.Contains("Xuq"));

            var indices = foo.PropertyIndices.ToArray();
            Assert.AreEqual(1, indices.Length);
            Assert.IsTrue(indices.Contains(1));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Nothing()
        {
            engine.Script.foo = new Func<object>(() => Nothing.Value);
            Assert.IsTrue((bool)engine.Evaluate("foo() == undefined"));
            Assert.IsTrue((bool)engine.Evaluate("foo() === undefined"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CpuProfileSampleInterval_Plumbing()
        {
            using (var runtime = new V8Runtime())
            {
                using (var engine1 = runtime.CreateScriptEngine())
                {
                    using (var engine2 = runtime.CreateScriptEngine())
                    {
                        var value = 123456789U;
                        engine1.CpuProfileSampleInterval = value;
                        Assert.AreEqual(value, engine1.CpuProfileSampleInterval);
                        Assert.AreEqual(value, engine2.CpuProfileSampleInterval);
                        Assert.AreEqual(value, runtime.CpuProfileSampleInterval);

                        value = 987654321U;
                        runtime.CpuProfileSampleInterval = value;
                        Assert.AreEqual(value, engine1.CpuProfileSampleInterval);
                        Assert.AreEqual(value, engine2.CpuProfileSampleInterval);
                        Assert.AreEqual(value, runtime.CpuProfileSampleInterval);
                    }
                }
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CpuProfile()
        {
            const string name = "foo";
            engine.BeginCpuProfile(name, V8CpuProfileFlags.EnableSampleCollection);
            engine.Execute(CreateCpuProfileTestScript());
            var profile = engine.EndCpuProfile(name);

            Assert.AreEqual(engine.Name + ":" + name, profile.Name);
            Assert.IsTrue(profile.StartTimestamp > 0);
            Assert.IsTrue(profile.EndTimestamp > 0);
            Assert.IsNotNull(profile.RootNode);
            Assert.IsNotNull(profile.Samples);
            Assert.IsTrue(profile.Samples.Count > 0);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CpuProfile_Json()
        {
            const string name = "foo";
            engine.BeginCpuProfile(name, V8CpuProfileFlags.EnableSampleCollection);
            engine.Execute(CreateCpuProfileTestScript());
            var profile = engine.EndCpuProfile(name);

            var json = profile.ToJson();
            var result = JsonConvert.DeserializeObject<JObject>(json);

            Assert.IsInstanceOfType(result["nodes"], typeof(JArray));
            Assert.IsInstanceOfType(result["startTime"], typeof(JValue));
            Assert.IsInstanceOfType(result["endTime"], typeof(JValue));
            Assert.IsInstanceOfType(result["samples"], typeof(JArray));
            Assert.IsInstanceOfType(result["timeDeltas"], typeof(JArray));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ExecuteDocument_Script()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            using (var console = new StringWriter())
            {
                var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                engine.AddHostObject("host", new ExtendedHostFunctions());
                engine.AddHostObject("clr", clr);

                engine.ExecuteDocument("JavaScript/General.js");
                Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EvaluateDocument_Script()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            using (var console = new StringWriter())
            {
                var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                engine.AddHostObject("host", new ExtendedHostFunctions());
                engine.AddHostObject("clr", clr);

                Assert.AreEqual((int)Math.Round(Math.Sin(Math.PI) * 1000e16), engine.EvaluateDocument("JavaScript/General.js"));
                Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CompileDocument_Script()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            using (var console = new StringWriter())
            {
                var clr = new HostTypeCollection(type => type != typeof(Console), "mscorlib", "System", "System.Core");
                clr.GetNamespaceNode("System").SetPropertyNoCheck("Console", console);

                engine.AddHostObject("host", new ExtendedHostFunctions());
                engine.AddHostObject("clr", clr);

                var script = engine.CompileDocument("JavaScript/General.js");
                Assert.AreEqual((int)Math.Round(Math.Sin(Math.PI) * 1000e16), engine.Evaluate(script));
                Assert.AreEqual(MiscHelpers.FormatCode(generalScriptOutput), console.ToString().Replace("\r\n", "\n"));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EvaluateDocument_Module_Standard()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.EvaluateDocument("JavaScript/StandardModule/Module.js", ModuleCategory.Standard));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CompileDocument_Module_Standard()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            var module = engine.CompileDocument("JavaScript/StandardModule/Module.js", ModuleCategory.Standard);
            Assert.AreEqual(25 * 25, engine.Evaluate(module));

            // re-evaluating a module is a no-op
            Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_EvaluateDocument_Module_CommonJS()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.EvaluateDocument("JavaScript/CommonJS/Module.js", ModuleCategory.CommonJS));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_CompileDocument_Module_CommonJS()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            var module = engine.CompileDocument("JavaScript/CommonJS/Module.js", ModuleCategory.CommonJS);
            Assert.AreEqual(25 * 25, engine.Evaluate(module));

            // re-evaluating a module is a no-op
            Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_DocumentSettings_EnforceRelativePrefix()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.EnforceRelativePrefix;
            TestUtil.AssertException<FileNotFoundException>(() => engine.EvaluateDocument("JavaScript/CommonJS/Module.js", ModuleCategory.CommonJS));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_ScriptCaching()
        {
            Assert.AreEqual(1UL, engine.GetRuntimeStatistics().ScriptCount);

            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
            Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
            Assert.AreEqual(3UL, engine.GetRuntimeStatistics().ScriptCount);

            var info = new DocumentInfo("Test");

            Assert.AreEqual(Math.E, engine.Evaluate(info, "Math.E"));
            Assert.AreEqual(Math.E, engine.Evaluate(info, "Math.E"));
            Assert.AreEqual(4UL, engine.GetRuntimeStatistics().ScriptCount);

            Assert.AreEqual(Math.PI, engine.Evaluate(info, "Math.PI"));
            Assert.AreEqual(Math.PI, engine.Evaluate(info, "Math.PI"));
            Assert.AreEqual(5UL, engine.GetRuntimeStatistics().ScriptCount);

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 10; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI, testEngine.Evaluate(info, "Math.PI"));
                        Assert.AreEqual(Math.E, testEngine.Evaluate(info, "Math.E"));
                        Assert.AreEqual((i < 1) ? 3UL : 0UL, testEngine.GetStatistics().ScriptCount);
                    }
                }

                Assert.AreEqual(3UL, runtime.GetStatistics().ScriptCount);
            }

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 300; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI + i, testEngine.Evaluate(info, "Math.PI" + "+" + i));
                    }
                }

                Assert.AreEqual(301UL, runtime.GetStatistics().ScriptCount);
                Assert.AreEqual(256UL, runtime.GetStatistics().ScriptCacheSize);
            }
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

        private static string CreateCpuProfileTestScript()
        {
            var builder = new StringBuilder();

            builder.Append(@"
                function loop() {
                    for (var i = 0; i < 10000000; i++) {
                        for (var j = 0; j < 10000000; j++) {
                            if (Math.random() > 0.999 && Math.random() > 0.999) {
                                return i + '-' + j;
                            }
                        }
                    }
                }
                (function () {");

            builder.AppendLine();
            AppendCpuProfileTestSequence(builder, 4, MiscHelpers.CreateSeededRandom(), new List<int>());
            builder.Append(@"                })()");
            builder.AppendLine();

            return builder.ToString();
        }

        private static void AppendCpuProfileTestSequence(StringBuilder builder, int count, Random random, List<int> indices)
        {
            const string separator = "_";
            var indent = new string(Enumerable.Repeat(' ', indices.Count * 4 + 20).ToArray());

            count = (count < 0) ? random.Next(4) : count;
            count = (indices.Count >= 4) ? 0 : count;

            for (var index = 0; index < count; index++)
            {
                builder.AppendFormat("{0}function f{1}{2}() {{", indent, separator, string.Join(separator, indices.Concat(index.ToEnumerable())));
                builder.AppendLine();

                AppendCpuProfileTestSequence(builder, -1, random, indices.Concat(index.ToEnumerable()).ToList());

                builder.AppendFormat("{0}}}", indent);
                builder.AppendLine();
            }

            builder.AppendFormat("{0}return {1}loop();", indent, string.Join(string.Empty, Enumerable.Range(0, count).Select(index => "f" + separator + string.Join(separator, indices.Concat(index.ToEnumerable())) + "() + '-' + ")));
            builder.AppendLine();
        }

        public object TestProperty { get; set; }

        public static object StaticTestProperty { get; set; }

        // ReSharper disable UnusedMember.Local

        private void PrivateMethod()
        {
        }

        private static void PrivateStaticMethod()
        {
        }

        private delegate string TestDelegate(string pre, ref string value, int post);

        public delegate object VarArgDelegate(object pre, params object[] args);

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
