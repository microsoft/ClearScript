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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using Microsoft.ClearScript.JavaScript;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    public class JScriptEngineTest : ClearScriptTest
    {
        #region setup / teardown

        private JScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostObject()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", host);
            Assert.AreSame(host, engine.Evaluate("host"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void JScriptEngine_AddHostObject_Scalar()
        {
            engine.AddHostObject("value", 123);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostObject_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.AddHostObject("value", value);
            Assert.AreEqual(value, engine.Evaluate("value"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostObject_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.AddHostObject("date", date);
            Assert.AreEqual(date, engine.Evaluate("date"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostObject_GlobalMembers()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", HostItemFlags.GlobalMembers, host);
            Assert.IsInstanceOfType(engine.Evaluate("newObj()"), typeof(PropertyBag));

            engine.AddHostObject("test", HostItemFlags.GlobalMembers, this);
            engine.Execute("TestProperty = newObj()");
            Assert.IsInstanceOfType(TestProperty, typeof(PropertyBag));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void JScriptEngine_AddHostObject_DefaultAccess()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostObject_PrivateAccess()
        {
            engine.AddHostObject("test", HostItemFlags.PrivateAccess, this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddRestrictedHostObject_BaseClass()
        {
            var host = new ExtendedHostFunctions() as HostFunctions;
            engine.AddRestrictedHostObject("host", host);
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("host.type('System.Int32')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddRestrictedHostObject_Interface()
        {
            const double value = 123.45;
            engine.AddRestrictedHostObject("convertible", value as IConvertible);
            engine.AddHostObject("culture", CultureInfo.InvariantCulture);
            Assert.AreEqual(value, engine.Evaluate("convertible.ToDouble(culture)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_GlobalMembers()
        {
            engine.AddHostType("Guid", HostItemFlags.GlobalMembers, typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("NewGuid()"), typeof(Guid));

            engine.AddHostType("Test", HostItemFlags.GlobalMembers, GetType());
            engine.Execute("StaticTestProperty = NewGuid()");
            Assert.IsInstanceOfType(StaticTestProperty, typeof(Guid));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void JScriptEngine_AddHostType_DefaultAccess()
        {
            engine.AddHostType("Test", GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_PrivateAccess()
        {
            engine.AddHostType("Test", HostItemFlags.PrivateAccess, GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_Static()
        {
            engine.AddHostType("Enumerable", typeof(Enumerable));
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_OpenGeneric()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("List", typeof(List<>));
            engine.AddHostType("Guid", typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(List(Guid))"), typeof(List<Guid>));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_ByName()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", "System.Random");
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_ByNameWithAssembly()
        {
            engine.AddHostType("Enumerable", "System.Linq.Enumerable", "System.Core");
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_ByNameWithTypeArgs()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Dictionary", "System.Collections.Generic.Dictionary", typeof(string), typeof(int));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Dictionary)"), typeof(Dictionary<string, int>));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_DefaultName()
        {
            engine.AddHostType(typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new Random()"), typeof(Random));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddHostType_DefaultNameGeneric()
        {
            engine.AddHostType(typeof(List<int>));
            Assert.IsInstanceOfType(engine.Evaluate("new List()"), typeof(List<int>));

            engine.AddHostType(typeof(Dictionary<,>));
            engine.AddHostType(typeof(int));
            engine.AddHostType(typeof(double));
            Assert.IsInstanceOfType(engine.Evaluate("new Dictionary(Int32, Double, 100)"), typeof(Dictionary<int, double>));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, true, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_RetainDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, false, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DocumentInfo_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName), "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DocumentInfo_WithDocumentUri()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(@"c:\foo\bar\baz\" + documentName);
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentUri) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DocumentInfo_WithDocumentUri_Relative()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(documentName, UriKind.Relative);
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentUri) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DocumentInfo_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName) { Flags = DocumentFlags.IsTransient }, "Math.E * Math.PI"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Evaluate_DocumentInfo_RetainDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(new DocumentInfo(documentName) { Flags = DocumentFlags.None }, "Math.E * Math.PI"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute()
        {
            engine.Execute("epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, true, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, false, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DocumentInfo_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.Execute(new DocumentInfo(documentName), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DocumentInfo_WithDocumentUri()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(@"c:\foo\bar\baz\" + documentName);
            engine.Execute(new DocumentInfo(documentUri), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DocumentInfo_WithDocumentUri_Relative()
        {
            const string documentName = "DoTheMath";
            var documentUri = new Uri(documentName, UriKind.Relative);
            engine.Execute(new DocumentInfo(documentUri), "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DocumentInfo_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(new DocumentInfo(documentName) { Flags = DocumentFlags.IsTransient }, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Execute_DocumentInfo_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(new DocumentInfo(documentName) { Flags = DocumentFlags.None }, "epi = Math.E * Math.PI");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ExecuteCommand_EngineConvert()
        {
            Assert.AreEqual("[object Math]", engine.ExecuteCommand("Math"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ExecuteCommand_HostConvert()
        {
            var dateHostItem = HostItem.Wrap(engine, new DateTime(2007, 5, 22, 6, 15, 43));
            engine.AddHostObject("date", dateHostItem);
            Assert.AreEqual(dateHostItem.ToString(), engine.ExecuteCommand("date"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ExecuteCommand_var()
        {
            Assert.AreEqual("[undefined]", engine.ExecuteCommand("var x = 'foo'"));
            Assert.AreEqual("foo", engine.Script.x);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ExecuteCommand_HostVariable()
        {
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("[HostVariable:String]", engine.ExecuteCommand("host.newVar('foo')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Invoke_ScriptFunction()
        {
            engine.Execute("function foo(x) { return x * Math.PI; }");
            Assert.AreEqual(Math.E * Math.PI, engine.Invoke("foo", Math.E));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Invoke_HostDelegate()
        {
            engine.Script.foo = new Func<double, double>(x => x * Math.PI);
            Assert.AreEqual(Math.E * Math.PI, engine.Invoke("foo", Math.E));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Interrupt()
        {
            var checkpoint = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(state =>
            {
                checkpoint.WaitOne();
                engine.Interrupt();
            });

            engine.AddHostObject("checkpoint", checkpoint);
            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("checkpoint.Set(); while (true) { var foo = 'hello'; }"));
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void JScriptEngine_AccessContext_Default()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AccessContext_Private()
        {
            engine.AddHostObject("test", this);
            engine.AccessContext = GetType();
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ContinuationCallback()
        {
            engine.ContinuationCallback = () => false;
            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("while (true) { var foo = 'hello'; }"));
            engine.ContinuationCallback = null;
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_FileNameExtension()
        {
            Assert.AreEqual("js", engine.FileNameExtension);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property()
        {
            var host = new HostFunctions();
            engine.Script.host = host;
            Assert.AreSame(host, engine.Script.host);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Scalar()
        {
            const int value = 123;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.Script.date = date;
            Assert.AreEqual(date, engine.Script.date);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Index_ArrayItem()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Index_Property()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Method()
        {
            engine.Execute("function foo(x) { return x * x; }");
            Assert.AreEqual(25, engine.Script.foo(5));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Method_Intrinsic()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Script.eval("Math.E * Math.PI"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    Dim host As New HostFunctions
                    engine.Script.host = host
                    Assert.AreSame(host, engine.Script.host)
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Scalar_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    Dim value = 123
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Enum_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    Dim value = DayOfWeek.Wednesday
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Property_Struct_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    Dim value As New DateTime(2007, 5, 22, 6, 15, 43)
                    engine.Script.value = value
                    Assert.AreEqual(value, engine.Script.value)
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Index_ArrayItem_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine

                    Dim index = 5
                    engine.Execute(""foo = []"")

                    engine.Script.foo(index) = engine.Script.Math.PI
                    Assert.AreEqual(Math.PI, engine.Script.foo(index))
                    Assert.AreEqual(index + 1, engine.Evaluate(""foo.length""))

                    engine.Script.foo(index) = engine.Script.Math.E
                    Assert.AreEqual(Math.E, engine.Script.foo(index))
                    Assert.AreEqual(index + 1, engine.Evaluate(""foo.length""))

                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Index_Property_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine

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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Method_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    engine.Execute(""function foo(x) { return x * x; }"")
                    Assert.AreEqual(25, engine.Script.foo(5))
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Script_Method_Intrinsic_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    Assert.AreEqual(Math.E * Math.PI, engine.Script.eval(""Math.E * Math.PI""))
                End Using
            ");
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_CollectGarbage()
        {
            engine.Execute(@"x = []; for (i = 0; i < 1024 * 1024; i++) { x.push(x); }");
            engine.CollectGarbage(true);
            // can't test JScript GC effectiveness
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Random()"), typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Random(100)"), typeof(Random));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_Generic()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Collections.Generic.Dictionary(System.Int32, System.String)"), typeof(Dictionary<int, string>));
            Assert.IsInstanceOfType(engine.Evaluate("new System.Collections.Generic.Dictionary(System.Int32, System.String, 100)"), typeof(Dictionary<int, string>));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_GenericNested()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System.Core"));
            engine.AddHostObject("dict", new Dictionary<int, string> { { 12345, "foo" }, { 54321, "bar" } });
            Assert.IsInstanceOfType(engine.Evaluate("vc = new (System.Collections.Generic.Dictionary(System.Int32, System.String).ValueCollection)(dict)"), typeof(Dictionary<int, string>.ValueCollection));
            Assert.IsTrue((bool)engine.Evaluate("vc.SequenceEqual(dict.Values)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_Scalar()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(int), engine.Evaluate("new System.Int32"));
            Assert.AreEqual(default(int), engine.Evaluate("new System.Int32()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_Enum()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(DayOfWeek), engine.Evaluate("new System.DayOfWeek"));
            Assert.AreEqual(default(DayOfWeek), engine.Evaluate("new System.DayOfWeek()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_Struct()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            Assert.AreEqual(default(DateTime), engine.Evaluate("new System.DateTime"));
            Assert.AreEqual(default(DateTime), engine.Evaluate("new System.DateTime()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_new_NoMatch()
        {
            engine.AddHostObject("clr", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            TestUtil.AssertException<MissingMemberException>(() => engine.Execute("new System.Random('a')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_General()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_SyntaxError()
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
                    Assert.IsTrue(exception.Message.StartsWith("Expected", StringComparison.Ordinal));
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_ThrowNonError()
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
                    Assert.IsTrue(exception.ErrorDetails.Contains(" -> throw 123"));
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_ScriptError()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_HostException()
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
                    Assert.IsNotNull(exception.InnerException);

                    var hostException = exception.InnerException;
                    Assert.IsInstanceOfType(hostException, typeof(RuntimeBinderException));
                    TestUtil.AssertValidException(hostException);
                    Assert.IsNull(hostException.InnerException);

                    Assert.AreEqual(hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_IgnoredHostException()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_NestedScriptError()
        {
            using (var innerEngine = new JScriptEngine("inner", WindowsScriptEngineFlags.EnableDebugging))
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

                        Assert.AreEqual(hostException.Message, exception.Message);
                        throw;
                    }
                });
            }
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ErrorHandling_NestedHostException()
        {
            using (var innerEngine = new JScriptEngine("inner", WindowsScriptEngineFlags.EnableDebugging))
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

                        Assert.AreEqual(nestedHostException.Message, nestedException.Message);
                        Assert.AreEqual(hostException.Message, exception.Message);
                        throw;
                    }
                });
            }
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_CreateInstance()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo bar baz qux", engine.Evaluate("new testObject('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_CreateInstance_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("new testObject()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Invoke_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("testObject()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo-bar-baz-qux", engine.Evaluate("testObject.DynamicMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.DynamicMethod()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo.bar.baz.qux", engine.Evaluate("testObject.SomeField('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeField()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo:bar:baz:qux", engine.Evaluate("testObject.SomeProperty('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeProperty()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_DynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo;bar;baz;qux", engine.Evaluate("testObject.SomeMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_NonDynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.SomeMethod()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_InvokeMethod_NonDynamic()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("Super Bass-O-Matic '76", engine.Evaluate("testObject.ToString()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_StaticType_Field()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeField"), typeof(HostMethod));
            Assert.AreEqual(12345, engine.Evaluate("host.toStaticType(testObject).SomeField"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_StaticType_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeProperty"), typeof(HostMethod));
            Assert.AreEqual("Bogus", engine.Evaluate("host.toStaticType(testObject).SomeProperty"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_StaticType_Method()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("bar+baz+qux", engine.Evaluate("host.toStaticType(testObject).SomeMethod('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_StaticType_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            TestUtil.AssertException<NotSupportedException>(() => engine.Evaluate("host.toStaticType(testObject)('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.AreEqual(123, engine.Evaluate("testObject.foo = 123"));
            Assert.AreEqual(123, engine.Evaluate("testObject.foo"));
            Assert.IsTrue((bool)engine.Evaluate("delete testObject.foo"));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("delete testObject.foo"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Property_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.Zfoo"), typeof(Undefined));
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.Zfoo = 123"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Property_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo = function (x) { return x.length; }"), typeof(DynamicObject));
            Assert.AreEqual("floccinaucinihilipilification".Length, engine.Evaluate("testObject.foo('floccinaucinihilipilification')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Property_Invoke_Nested()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject.foo = testObject"), typeof(DynamicTestObject));
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject.foo('foo', 'bar', 'baz', 'qux')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Element()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Element_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 1, 2, 3, Math.PI)"), typeof(Undefined));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("host.setElement(testObject, 'bar', 1, 2, 3, Math.PI)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Element_Index()
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
            Assert.AreEqual("qux", engine.Evaluate("testObject('foo', 'bar', 'baz') = 'qux'"));
            Assert.AreEqual("qux", engine.Evaluate("testObject('foo', 'bar', 'baz')"));
            Assert.AreEqual("qux", engine.Evaluate("host.getElement(testObject, 'foo', 'bar', 'baz')"));
            Assert.IsInstanceOfType(engine.Evaluate("testObject('foo', 'bar', 'baz') = undefined"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testObject('foo', 'bar', 'baz')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, 'foo', 'bar', 'baz')"), typeof(Undefined));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DynamicHostObject_Convert()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            engine.AddHostType("int_t", typeof(int));
            engine.AddHostType("string_t", typeof(string));
            Assert.AreEqual(98765, engine.Evaluate("host.cast(int_t, testObject)"));
            Assert.AreEqual("Booyakasha!", engine.Evaluate("host.cast(string_t, testObject)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_HostIndexers()
        {
            engine.Script.testObject = new TestObject();

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(123)"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(123)"));
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.Item(123) = Math.PI"));
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.Item(123)"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.set(123, Math.E)"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.get(123)"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item('456')"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get('456')"));
            Assert.AreEqual(Math.Sqrt(2), engine.Evaluate("testObject.Item('456') = Math.sqrt(2)"));
            Assert.AreEqual(Math.Sqrt(2), engine.Evaluate("testObject.Item('456')"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.set('456', Math.sqrt(3))"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.get('456')"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(123, '456', 789.987, -0.12345)"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(123, '456', 789.987, -0.12345)"));
            Assert.AreEqual(Math.Sqrt(5), engine.Evaluate("testObject.Item(123, '456', 789.987, -0.12345) = Math.sqrt(5)"));
            Assert.AreEqual(Math.Sqrt(5), engine.Evaluate("testObject.Item(123, '456', 789.987, -0.12345)"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.set(123, '456', 789.987, -0.12345, Math.sqrt(7))"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.get(123, '456', 789.987, -0.12345)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_FormatCode()
        {
            try
            {
                engine.Execute("a", "\n\n\n     x = 3.a");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("(a:3:11)"));
            }

            engine.FormatCode = true;
            try
            {
                engine.Execute("b", "\n\n\n     x = 3.a");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("(b:0:6)"));
            }
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_GetStackTrace()
        {
            engine.AddHostObject("qux", new Func<object>(() => engine.GetStackTrace()));
            engine.Execute(@"
                function baz() { return qux(); }
                function bar() { return baz(); }
                function foo() { return bar(); }
            ");

            Assert.AreEqual("    at baz (Script:1:33) -> return qux()\n    at bar (Script:2:33) -> return baz()\n    at foo (Script:3:33) -> return bar()\n    at JScript global code (Script [2] [temp]:0:0) -> foo()", engine.Evaluate("foo()"));
            Assert.AreEqual("    at baz (Script:1:33) -> return qux()\n    at bar (Script:2:33) -> return baz()\n    at foo (Script:3:33) -> return bar()", engine.Script.foo());
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_StandardsMode()
        {
            // ReSharper disable AccessToDisposedClosure

            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("JSON"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode);

            Assert.AreEqual("{\"foo\":123,\"bar\":456.789}", engine.Evaluate("JSON.stringify({ foo: 123, bar: 456.789 })"));

            // ReSharper restore AccessToDisposedClosure
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_MarshalNullAsDispatch()
        {
            engine.Script.func = new Func<object>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalNullAsDispatch);

            engine.Script.func = new Func<object>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<string>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<bool?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<char?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<sbyte?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<byte?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<short?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<ushort?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<int?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<uint?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<long?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<ulong?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<float?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<double?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<decimal?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<Random>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
            engine.Script.func = new Func<DayOfWeek?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() === null"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_MarshalDecimalAsCurrency()
        {
            engine.Script.func = new Func<object>(() => 123.456M);
            Assert.AreEqual("number", engine.Evaluate("typeof(func())"));
            Assert.AreEqual(123.456 + 5, engine.Evaluate("func() + 5"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalDecimalAsCurrency);

            engine.Script.func = new Func<object>(() => 123.456M);
            Assert.AreEqual("number", engine.Evaluate("typeof(func())"));
            Assert.AreEqual(123.456 + 5, engine.Evaluate("func() + 5"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_MarshalArraysByValue()
        {
            var foo = new[] { DayOfWeek.Saturday, DayOfWeek.Friday, DayOfWeek.Thursday };

            engine.Script.foo = foo;
            Assert.AreSame(foo, engine.Evaluate("foo"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalArraysByValue);

            engine.Script.foo = foo;
            Assert.AreNotSame(foo, engine.Evaluate("foo"));
            engine.Execute("foo = new VBArray(foo)");

            Assert.AreEqual(foo.GetUpperBound(0), engine.Evaluate("foo.ubound(1)"));
            for (var index = 0; index < foo.Length; index++)
            {
                Assert.AreEqual(foo[index], engine.Evaluate("foo.getItem(" + index + ")"));
            }
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_MarshalArraysByValue_invokeMethod()
        {
            var args = new[] { Math.PI, Math.E };

            engine.Script.args = args;
            engine.Execute("function foo(a, b) { return a * b; }");
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("EngineInternal.invokeMethod(null, foo, args)"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalArraysByValue);

            engine.Script.args = args;
            engine.Execute("function foo(a, b) { return a * b; }");
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("EngineInternal.invokeMethod(null, foo, args)"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMObject_FileSystemObject()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMObject_FileSystemObject_NativeEnumerator()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                drives = fso.Drives;
                for (e = new Enumerator(drives); !e.atEnd(); e.moveNext()) {
                    list.Add(e.item().Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMObject_Dictionary()
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
                dict.Item('foo') = 987.654;
                dict.Item('bar') = 321;
                dict.Item('baz') = 'halloween';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get(Math.E)"));

            engine.Execute(@"
                dict.Key('qux') = 'foo';
                dict.Key(Math.PI) = 'bar';
                dict.Key(Math.E) = 'baz';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMType_FileSystemObject()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMType_FileSystemObject_NativeEnumerator()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                drives = fso.Drives;
                for (e = new Enumerator(drives); !e.atEnd(); e.moveNext()) {
                    list.Add(e.item().Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_COMType_Dictionary()
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
                dict.Item('foo') = 987.654;
                dict.Item('bar') = 321;
                dict.Item('baz') = 'halloween';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get(Math.E)"));

            engine.Execute(@"
                dict.Key('qux') = 'foo';
                dict.Key(Math.PI) = 'bar';
                dict.Key(Math.E) = 'baz';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMObject_FileSystemObject()
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

            Assert.AreEqual("[HostObject:IFileSystem3]", engine.ExecuteCommand("fso"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMObject_FileSystemObject_DirectAccess()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMObject("fso", HostItemFlags.DirectAccess, "Scripting.FileSystemObject");
            engine.Execute(@"
                drives = fso.Drives;
                for (e = new Enumerator(drives); !e.atEnd(); e.moveNext()) {
                    list.Add(e.item().Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));

            Assert.AreEqual("System.__ComObject", engine.ExecuteCommand("fso"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMObject_Dictionary()
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
                dict.Item('foo') = 987.654;
                dict.Item('bar') = 321;
                dict.Item('baz') = 'halloween';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get(Math.E)"));

            engine.Execute(@"
                dict.Key('qux') = 'foo';
                dict.Key(Math.PI) = 'bar';
                dict.Key(Math.E) = 'baz';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMType_FileSystemObject()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMType_Dictionary()
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
                dict.Item('foo') = 987.654;
                dict.Item('bar') = 321;
                dict.Item('baz') = 'halloween';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get(Math.E)"));

            engine.Execute(@"
                dict.Key('qux') = 'foo';
                dict.Key(Math.PI) = 'bar';
                dict.Key(Math.E) = 'baz';
            ");

            Assert.AreEqual(987.654, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(987.654, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(321, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("halloween", engine.Evaluate("dict.Item.get('baz')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_AddCOMType_XMLHTTP()
        {
            var status = 0;
            string data = null;

            var thread = new Thread(() =>
            {
                using (var testEngine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode))
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnableAutoHostVariables()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnableAutoHostVariables_Delegate()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Current()
        {
            // ReSharper disable AccessToDisposedClosure

            using (var innerEngine = new JScriptEngine())
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnableNullResultWrapping()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnableNullResultWrapping_String()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnableNullResultWrapping_Nullable()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DefaultProperty()
        {
            engine.Script.foo = new DefaultPropertyTestObject();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo('abc') = 123");
            Assert.AreEqual(123, engine.Evaluate("foo('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Item('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Item.get('abc')"));
            Assert.IsNull(engine.Evaluate("foo('def')"));

            engine.Execute("foo(DayOfWeek.Thursday) = 456");
            Assert.AreEqual(456, engine.Evaluate("foo(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Item(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Item.get(DayOfWeek.Thursday)"));
            Assert.IsNull(engine.Evaluate("foo(DayOfWeek.Friday)"));

            engine.Execute("foo.Item('def') = 987");
            Assert.AreEqual(987, engine.Evaluate("foo('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Item('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Item.get('def')"));
            Assert.IsNull(engine.Evaluate("foo('ghi')"));

            engine.Execute("foo.Item(DayOfWeek.Friday) = 654");
            Assert.AreEqual(654, engine.Evaluate("foo(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Item(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Item.get(DayOfWeek.Friday)"));
            Assert.IsNull(engine.Evaluate("foo(DayOfWeek.Saturday)"));

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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DefaultProperty_FieldTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Field('abc') = 123");
            Assert.AreEqual(123, engine.Evaluate("foo.Field('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Field.Item('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Field.Item.get('abc')"));
            Assert.IsNull(engine.Evaluate("foo.Field('def')"));

            engine.Execute("foo.Field(DayOfWeek.Thursday) = 456");
            Assert.AreEqual(456, engine.Evaluate("foo.Field(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Field.Item(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Field.Item.get(DayOfWeek.Thursday)"));
            Assert.IsNull(engine.Evaluate("foo.Field(DayOfWeek.Friday)"));

            engine.Execute("foo.Field.Item('def') = 987");
            Assert.AreEqual(987, engine.Evaluate("foo.Field('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Field.Item('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Field.Item.get('def')"));
            Assert.IsNull(engine.Evaluate("foo.Field('ghi')"));

            engine.Execute("foo.Field.Item(DayOfWeek.Friday) = 654");
            Assert.AreEqual(654, engine.Evaluate("foo.Field(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Field.Item(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Field.Item.get(DayOfWeek.Friday)"));
            Assert.IsNull(engine.Evaluate("foo.Field(DayOfWeek.Saturday)"));

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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DefaultProperty_PropertyTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Property('abc') = 123");
            Assert.AreEqual(123, engine.Evaluate("foo.Property('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Property.Item('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Property.Item.get('abc')"));
            Assert.IsNull(engine.Evaluate("foo.Property('def')"));

            engine.Execute("foo.Property(DayOfWeek.Thursday) = 456");
            Assert.AreEqual(456, engine.Evaluate("foo.Property(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Property.Item(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Property.Item.get(DayOfWeek.Thursday)"));
            Assert.IsNull(engine.Evaluate("foo.Property(DayOfWeek.Friday)"));

            engine.Execute("foo.Property.Item('def') = 987");
            Assert.AreEqual(987, engine.Evaluate("foo.Property('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Property.Item('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Property.Item.get('def')"));
            Assert.IsNull(engine.Evaluate("foo.Property('ghi')"));

            engine.Execute("foo.Property.Item(DayOfWeek.Friday) = 654");
            Assert.AreEqual(654, engine.Evaluate("foo.Property(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Property.Item(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Property.Item.get(DayOfWeek.Friday)"));
            Assert.IsNull(engine.Evaluate("foo.Property(DayOfWeek.Saturday)"));

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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DefaultProperty_MethodTunneling()
        {
            engine.Script.foo = new DefaultPropertyTestContainer();
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("foo.Method()('abc') = 123");
            Assert.AreEqual(123, engine.Evaluate("foo.Method()('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Method().Item('abc')"));
            Assert.AreEqual(123, engine.Evaluate("foo.Method().Item.get('abc')"));
            Assert.IsNull(engine.Evaluate("foo.Method()('def')"));

            engine.Execute("foo.Method()(DayOfWeek.Thursday) = 456");
            Assert.AreEqual(456, engine.Evaluate("foo.Method()(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Method().Item(DayOfWeek.Thursday)"));
            Assert.AreEqual(456, engine.Evaluate("foo.Method().Item.get(DayOfWeek.Thursday)"));
            Assert.IsNull(engine.Evaluate("foo.Method()(DayOfWeek.Friday)"));

            engine.Execute("foo.Method().Item('def') = 987");
            Assert.AreEqual(987, engine.Evaluate("foo.Method()('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Method().Item('def')"));
            Assert.AreEqual(987, engine.Evaluate("foo.Method().Item.get('def')"));
            Assert.IsNull(engine.Evaluate("foo.Method()('ghi')"));

            engine.Execute("foo.Method().Item(DayOfWeek.Friday) = 654");
            Assert.AreEqual(654, engine.Evaluate("foo.Method()(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Method().Item(DayOfWeek.Friday)"));
            Assert.AreEqual(654, engine.Evaluate("foo.Method().Item.get(DayOfWeek.Friday)"));
            Assert.IsNull(engine.Evaluate("foo.Method()(DayOfWeek.Saturday)"));

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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DefaultProperty_Indexer()
        {
            engine.Script.dict = new Dictionary<string, object> { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            engine.Execute("item = dict.Item");

            Assert.AreEqual(123, engine.Evaluate("item('abc')"));
            Assert.AreEqual(456, engine.Evaluate("item('def')"));
            Assert.AreEqual(789, engine.Evaluate("item('ghi')"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("item('jkl')"));

            engine.Execute("item('abc') = 'foo'");
            Assert.AreEqual("foo", engine.Evaluate("item('abc')"));
            Assert.AreEqual(456, engine.Evaluate("item('def')"));
            Assert.AreEqual(789, engine.Evaluate("item('ghi')"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("item('jkl')"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_PropertyAndMethodWithSameName()
        {
            engine.AddHostObject("lib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib", "System", "System.Core"));

            engine.Script.dict = new Dictionary<string, object> { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            Assert.AreEqual(3, engine.Evaluate("dict.Count"));
            Assert.AreEqual(3, engine.Evaluate("dict.Count()"));

            engine.Script.listDict = new ListDictionary { { "abc", 123 }, { "def", 456 }, { "ghi", 789 } };
            Assert.AreEqual(3, engine.Evaluate("listDict.Count"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Evaluate("listDict.Count()"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_NativeEnumerator()
        {
            var array = Enumerable.Range(0, 10).ToArray();
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var e = new Enumerator(array); !e.atEnd(); e.moveNext()) {
                        result += e.item();
                    }
                    return result;
                }
            ");
            Assert.AreEqual(array.Aggregate((current, next) => current + next), engine.Script.sum(array));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_NativeEnumerator_Generic()
        {
            var array = Enumerable.Range(0, 10).Select(value => (IConvertible)value).ToArray();
            engine.Script.culture = CultureInfo.InvariantCulture;
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var e = new Enumerator(array); !e.atEnd(); e.moveNext()) {
                        result += e.item().ToInt32(culture);
                    }
                    return result;
                }
            ");
            Assert.AreEqual(array.Aggregate((current, next) => Convert.ToInt32(current) + Convert.ToInt32(next)), engine.Script.sum(array));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_NativeEnumerator_NonGeneric()
        {
            var array = Enumerable.Range(0, 10).ToArray();
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var e = new Enumerator(array); !e.atEnd(); e.moveNext()) {
                        result += e.item();
                    }
                    return result;
                }
            ");
            Assert.AreEqual(array.Aggregate((current, next) => current + next), engine.Script.sum(HostObject.Wrap(array, typeof(IEnumerable))));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_NativeEnumerator_NonEnumerable()
        {
            engine.Execute(@"
                function sum(array) {
                    var result = 0;
                    for (var e = new Enumerator(array); !e.atEnd(); e.moveNext()) {
                        result += e.item();
                    }
                    return result;
                }
            ");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Script.sum(DayOfWeek.Monday));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ScriptObject()
        {
            var obj = engine.Evaluate("({})") as ScriptObject;
            Assert.IsNotNull(obj);
            Assert.AreSame(engine, obj.Engine);
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ArrayInvocability()
        {
            engine.Script.foo = Enumerable.Range(123, 5).ToArray();
            Assert.AreEqual(124, engine.Evaluate("foo(1)"));
            Assert.AreEqual(456, engine.Evaluate("foo(1) = 456"));

            engine.Script.foo = new IConvertible[] { "bar" };
            Assert.AreEqual("bar", engine.Evaluate("foo(0)"));
            Assert.AreEqual("baz", engine.Evaluate("foo(0) = 'baz'"));

            engine.Script.bar = new List<string>();
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("bar.Add(foo(0))"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_PropertyBagInvocability()
        {
            engine.Script.lib = new HostTypeCollection("mscorlib", "System", "System.Core");
            Assert.IsInstanceOfType(engine.Evaluate("lib('System')"), typeof(PropertyBag));
            Assert.IsInstanceOfType(engine.Evaluate("lib.System('Collections')"), typeof(PropertyBag));
            Assert.IsInstanceOfType(engine.Evaluate("lib('Bogus')"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("lib.System('Heinous')"), typeof(Undefined));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("lib('Bogus') = 123"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("lib.System('Heinous') = 456"));

            engine.Script.foo = new PropertyBag { { "Null", null } };
            Assert.IsNull(engine.Evaluate("foo.Null"));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("foo.Null(123)"));
            engine.Execute("foo(null) = 123");
            Assert.AreEqual(123, Convert.ToInt32(engine.Evaluate("foo(null)")));
            engine.Execute("foo(undefined) = 456");
            Assert.AreEqual(456, Convert.ToInt32(engine.Evaluate("foo(undefined)")));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EnforceAnonymousTypeAccess()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_UnderlyingObject()
        {
            engine.Execute("function Foo() {}; bar = new Foo();");

            var bar = (IWindowsScriptObject)(((ScriptObject)engine.Script)["bar"]);
            var underlyingObject = bar.GetUnderlyingObject();

            Assert.AreEqual("JScriptTypeInfo", Information.TypeName(underlyingObject));

            ((IDisposable)bar).Dispose();
            Assert.AreEqual(0, Marshal.ReleaseComObject(underlyingObject));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ScriptObjectMembers()
        {
            engine.Execute(@"
                function Foo() {
                    this.Qux = function (x) { this.Bar = x; };
                    this.Xuq = function () { return this.Baz; }
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_Nothing()
        {
            engine.Script.foo = new Func<object>(() => Nothing.Value);
            Assert.IsTrue((bool)engine.Evaluate("foo() == undefined"));
            Assert.IsFalse((bool)engine.Evaluate("foo() === undefined"));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_ExecuteDocument_Script()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EvaluateDocument_Script()
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

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_EvaluateDocument_Module_CommonJS()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.EvaluateDocument("JavaScript/LegacyCommonJS/Module", ModuleCategory.CommonJS));
        }

        [TestMethod, TestCategory("JScriptEngine")]
        public void JScriptEngine_DocumentSettings_EnforceRelativePrefix()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.EnforceRelativePrefix;
            TestUtil.AssertException<FileNotFoundException>(() => engine.EvaluateDocument("JavaScript/LegacyCommonJS/Module", ModuleCategory.CommonJS));
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

        private delegate string TestDelegate(string pre, ref string value, int post);

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
