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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.ClearScript.Util;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.V8;
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
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class BugFixTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

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

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NullArgBinding()
        {
            var value = 123.456 as IConvertible;
            engine.AddRestrictedHostObject("value", value);
            Assert.AreEqual(value.ToString(null), engine.Evaluate("value.ToString(null)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NullArgBinding_Ambiguous()
        {
            engine.AddHostObject("lib", new HostTypeCollection("mscorlib"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("lib.System.Console.WriteLine(null)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DelegateConstructionSyntax()
        {
            engine.AddHostObject("lib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            var func = (Func<int, int>)engine.Evaluate("new (System.Func(System.Int32, System.Int32))(function (x) {return x * x; })");
            Assert.AreEqual(25, func(5));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DelegateConstructionSyntax_DoubleWrap()
        {
            engine.AddHostObject("lib", HostItemFlags.GlobalMembers, new HostTypeCollection("mscorlib"));
            engine.Execute("cb = new (System.Func(System.Int32, System.Int32))(function (x) {return x * x; })");
            var func = (Func<int, int>)engine.Evaluate("new (System.Func(System.Int32, System.Int32))(cb)");
            Assert.AreEqual(25, func(5));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScript_CaseInsensitivity()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            engine.Execute("abc = 1; ABC = 2; function foo() { return 3; } function FOO() { return 4; }");
            Assert.AreEqual(1, engine.Script.abc);
            Assert.AreEqual(2, engine.Script.ABC);
            Assert.AreEqual(3, engine.Script.foo());
            Assert.AreEqual(4, engine.Script.FOO());
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8_ScriptInterruptCrash()
        {
            // run the test several times to verify post-interrupt engine functionality

            for (var iteration = 0; iteration < 16; iteration++)
            {
                var context = new PropertyBag();
                engine.AddHostObject("context", context);

                using (var startEvent = new ManualResetEventSlim(false))
                {
                    context["startEvent"] = startEvent;

                    var interrupted = false;
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            engine.Execute("while (true) { context.startEvent.Set(); }");
                        }
                        catch (ScriptInterruptedException)
                        {
                            interrupted = true;
                        }
                    });

                    thread.Start();
                    startEvent.Wait();
                    engine.Interrupt();
                    thread.Join();
                    Assert.IsTrue(interrupted);
                }
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_InheritedInterfaceMethod()
        {
            var list = new List<int> { 123 };
            var enumerator = list.AsEnumerable().GetEnumerator();
            engine.AddRestrictedHostObject("enumerator", enumerator);
            Assert.IsTrue((bool)engine.Evaluate("enumerator.MoveNext()"));
            Assert.AreEqual(123, engine.Evaluate("enumerator.Current"));
            Assert.IsFalse((bool)engine.Evaluate("enumerator.MoveNext()"));
            engine.Execute("enumerator.Dispose()");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_EnumEquality()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));
            engine.AddHostType("BindingFlags", typeof(BindingFlags));
            Assert.IsTrue((bool)engine.Evaluate("DayOfWeek.Wednesday == DayOfWeek.Wednesday"));
            Assert.IsTrue((bool)engine.Evaluate("host.flags(BindingFlags.Public, BindingFlags.Instance) == host.flags(BindingFlags.Public, BindingFlags.Instance)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_FloatParameterBinding()
        {
            engine.AddHostType("Convert", typeof(Convert));
            engine.Script.list = new List<float>();
            const float floatPi = (float)Math.PI;
            engine.Execute("list.Add(Convert.ToSingle(Math.PI))");
            Assert.AreEqual(floatPi, engine.Script.list[0]);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_FloatParameterBinding_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_FloatParameterBinding();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_UInt32RoundTrip()
        {
            engine.AddHostType("UInt32", typeof(uint));
            Assert.AreEqual((long)uint.MaxValue, engine.Evaluate("UInt32.MaxValue"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_UInt32RoundTrip_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_UInt32RoundTrip();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AutoInt64FromDouble()
        {
            engine.AddHostType("Int32", typeof(int));
            Assert.AreEqual((long)int.MaxValue * 123 + 1, engine.Evaluate("Int32.MaxValue * 123 + 1"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_CompiledScriptResult()
        {
            engine.Script.host = new HostFunctions();
            using (var script = ((V8ScriptEngine)engine).Compile("host"))
            {
                Assert.IsInstanceOfType(((V8ScriptEngine)engine).Evaluate(script), typeof(HostFunctions));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_GenericDelegateConstructorWithArgument()
        {
            engine.AddHostType("Func", "System.Func");
            engine.AddHostType("StringT", typeof(string));
            engine.AddHostType("IntT", typeof(int));
            var func = (Func<string, int>)engine.Evaluate("new Func(StringT, IntT, function (s) { return s.length; })");
            const string testString = "floccinaucinihilipilification";
            Assert.AreEqual(testString.Length, func(testString));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_HostMethodAsArgument()
        {
            var testObject = new TestObject();
            engine.Script.testObject = testObject;
            engine.Script.expando = new ExpandoObject();
            engine.AddHostType("DateTime", typeof(DateTime));
            engine.Execute("expando.method = testObject.Method");
            Assert.AreEqual(testObject.Method("foo", 123), engine.Evaluate("expando.method('foo', 123)"));
            Assert.AreEqual(testObject.Method<DateTime>(456), engine.Evaluate("expando.method(DateTime, 456)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_HostIndexedPropertyAsArgument()
        {
            var testObject = new TestObject();
            engine.Script.testObject = testObject;
            engine.Script.expando = new ExpandoObject();
            engine.Execute("expando.property = testObject.Item");
            Assert.AreEqual("foo", engine.Evaluate("expando.property.set(123, 'foo')"));
            Assert.AreEqual("foo", engine.Evaluate("expando.property.get(123)"));
            Assert.AreEqual("foo", engine.Evaluate("expando.property(123)"));
            Assert.AreEqual(456, engine.Evaluate("expando.property.set('bar', 456)"));
            Assert.AreEqual(456, engine.Evaluate("expando.property.get('bar')"));
            Assert.AreEqual(456, engine.Evaluate("expando.property('bar')"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Nullable_Field()
        {
            engine.Script.test = new NullableTest();
            Assert.IsNull(engine.Evaluate("test.Field"));
            Assert.IsTrue((bool)engine.Evaluate("test.Field === null"));
            engine.Execute("test.Field = 123");
            Assert.AreEqual(123, engine.Evaluate("test.Field"));
            Assert.IsTrue((bool)engine.Evaluate("test.Field === 123"));
            engine.Execute("test.Field = null");
            Assert.IsNull(engine.Evaluate("test.Field"));
            Assert.IsTrue((bool)engine.Evaluate("test.Field === null"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Nullable_Property()
        {
            engine.Script.test = new NullableTest();
            Assert.IsNull(engine.Evaluate("test.Property"));
            Assert.IsTrue((bool)engine.Evaluate("test.Property === null"));
            engine.Execute("test.Property = 123");
            Assert.AreEqual(123, engine.Evaluate("test.Property"));
            Assert.IsTrue((bool)engine.Evaluate("test.Property === 123"));
            engine.Execute("test.Property = null");
            Assert.IsNull(engine.Evaluate("test.Property"));
            Assert.IsTrue((bool)engine.Evaluate("test.Property === null"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Nullable_ArgBinding()
        {
            var test = new NullableTest();
            engine.Script.test = test;
            Assert.IsNull(engine.Evaluate("test.Method(null)"));
            Assert.AreEqual(test.Method(5), engine.Evaluate("test.Method(5)"));
            Assert.AreEqual(test.Method(5.1), engine.Evaluate("test.Method(5.1)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_GlobalObjectCrash()
        {
            engine.AddHostObject("random", HostItemFlags.GlobalMembers, new Random());
            Assert.AreEqual("[object global]", engine.ExecuteCommand("this"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VariantClear()
        {
            // ReSharper disable RedundantAssignment

            var x = new object();
            var wr = new WeakReference(x);

            VariantClearTestHelper(x);
            x = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBCallToParameterlessVBScriptSub()
        {
            var options = new CompilerParameters { GenerateInMemory = true };
            options.ReferencedAssemblies.Add("ClearScript.dll");
            var results = new VBCodeProvider().CompileAssemblyFromSource(options, new[] { @"
                Module TestModule
                    Sub TestMethod
                        Using engine As New Microsoft.ClearScript.Windows.VBScriptEngine
                            engine.Execute(""sub main : end sub"")
                            engine.Script.main()
                        End Using
                    End Sub
                End Module
            "});

            results.CompiledAssembly.GetType("TestModule").InvokeMember("TestMethod", BindingFlags.InvokeMethod, null, null, MiscHelpers.GetEmptyArray<object>());
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private static void VariantClearTestHelper(object x)
        {
            using (var engine = new JScriptEngine())
            {
                engine.AddHostObject("x", x);
                engine.Evaluate("x");
            }
        }

        public class NullableTest
        {
            public int? Field;

            public double? Property;

            public object Method(int? value)
            {
                return value.HasValue ? (value + 1) : null;
            }

            public object Method(double? value)
            {
                return value.HasValue ? (value * 2.0) : null;
            }
        }

        #endregion
    }
}
