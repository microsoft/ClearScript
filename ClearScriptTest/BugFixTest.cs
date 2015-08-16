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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.CSharp.RuntimeBinder;
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
            BaseTestCleanup();
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
        public void BugFix_V8ScriptInterruptCrash()
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
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine
                    engine.Execute(""sub main : end sub"")
                    engine.Script.main()
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBDynamicArgMarshaling_Numeric()
        {
            var result = TestUtil.InvokeVBTestFunction(@"
                Using engine As New V8ScriptEngine
                    engine.Execute(""data = [5, 4, 'qux', 2, 1]; function getElement(i1, i2, i3) { return data[i1 + i2 - i3]; }"")
                    TestFunction = engine.Script.getElement(CShort(1), CLng(99), CStr(98))
                End Using
            ");

            Assert.AreEqual("qux", result);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBDynamicArgMarshaling_String()
        {
            var result = TestUtil.InvokeVBTestFunction(@"
                Using engine As New V8ScriptEngine
                    engine.Execute(""data = { foo26: 123, bar97: 456.789 }; function getElement(i1, i2) { return data[i1 + i2]; }"")
                    TestFunction = engine.Script.getElement(""bar"", CLng(97))
                End Using
            ");

            Assert.AreEqual(456.789, result);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AmbiguousAttribute()
        {
            engine.Script.foo = new AmbiguousAttributeTest();
            engine.Execute("foo.Foo()");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_CoreBindCache()
        {
            HostItem.ResetCoreBindCache();

            engine.Dispose();
            for (var i = 0; i < 10; i++)
            {
                using (engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
                {
                    engine.Script.host = new HostFunctions();
                    Assert.AreEqual(' ', engine.Evaluate("host.toChar(32)"));
                    Assert.AreEqual('A', engine.Evaluate("host.toChar(65)"));
                    Assert.AreEqual(0.0F, engine.Evaluate("host.toSingle(0)"));
                    Assert.AreEqual(1.0F, engine.Evaluate("host.toSingle(1)"));
                }
            }

            Assert.AreEqual(2L, HostItem.GetCoreBindCount());
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_StringWithNullChar()
        {
            const string value = "abc\0def";
            const string value2 = "ghi\0jkl";
            dynamic func = engine.Evaluate("(function (x) { return x.length; }).valueOf()");
            Assert.AreEqual(value.Length, func(value));
            Assert.AreEqual(value, engine.Evaluate(@"'abc\0def'"));
            dynamic obj = engine.Evaluate(@"({ 'abc\0def': 'ghi\0jkl' })");
            Assert.IsTrue(((DynamicObject)obj).GetDynamicMemberNames().Contains(value));
            Assert.AreEqual(value2, obj[value]);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8ScriptItemWeakBinding()
        {
            // This test verifies that V8 script items no longer prevent their isolates from being
            // destroyed. Previously it exhausted address space and crashed in 32-bit mode.

            for (var i = 0; i < 128; i++)
            {
                using (var tempEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
                {
                    tempEngine.Evaluate("(function () {}).valueOf()");
                }
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8ScriptItemDispose()
        {
            dynamic item1;
            using (var tempEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
            {
                item1 = tempEngine.Evaluate("(function () { return 123; }).valueOf()");
                Assert.AreEqual(123, item1());

                dynamic item2 = tempEngine.Evaluate("(function () { return 456; }).valueOf()");
                using (item2 as IDisposable)
                {
                    Assert.AreEqual(456, item2());
                   
                }
            }

            using (item1 as IDisposable)
            {
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8CompiledScriptWeakBinding()
        {
            // This test verifies that V8 compiled scripts no longer prevent their isolates from
            // being destroyed. Previously it exhausted address space and crashed in 32-bit mode.

            for (var i = 0; i < 128; i++)
            {
                using (var tempEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
                {
                    tempEngine.Compile("(function () {}).valueOf()");
                }
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8CompiledScriptDispose()
        {
            V8Script script1;
            using (var tempEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
            {
                script1 = tempEngine.Compile("(function () { return 123; })()");
                Assert.AreEqual(123, tempEngine.Evaluate(script1));

                V8Script script2 = tempEngine.Compile("(function () { return 456; })()");
                using (script2)
                {
                    Assert.AreEqual(456, tempEngine.Evaluate(script2));

                }
            }

            using (script1)
            {
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8ArrayBufferAllocator()
        {
            engine.Execute("buffer = new ArrayBuffer(12345)");
            Assert.AreEqual(12345, engine.Script.buffer.byteLength);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_SetProperty_JScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new JScriptEngine();

            object x = Guid.NewGuid();
            var wr = new WeakReference(x);

            new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                var result = x.ToString();
                engine.Script.x = x;

                x = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                Assert.AreEqual(result, engine.Evaluate("x.ToString()"));

                engine.Script.x = null;
                engine.CollectGarbage(true);

                // ReSharper restore AccessToModifiedClosure
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_SetProperty_VBScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new VBScriptEngine();

            object x = Guid.NewGuid();
            var wr = new WeakReference(x);

            new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                var result = x.ToString();
                engine.Script.x = x;

                x = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                Assert.AreEqual(result, engine.Evaluate("x.ToString()"));

                engine.Script.x = null;
                engine.CollectGarbage(true);

                // ReSharper restore AccessToModifiedClosure
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_GetProperty_VBScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new VBScriptEngine();

            object x1 = Guid.NewGuid();
            var wr1 = new WeakReference(x1);
            object x2 = Guid.NewGuid();
            var wr2 = new WeakReference(x2);

            new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                engine.Execute(@"
                    class MyClass
                        public property get foo(x1, x2)
                            foo = x1.ToString() & x2.ToString()
                        end property
                    end class
                    set bar = new MyClass
                ");

                object result;
                var bar = (DynamicObject)engine.Script.bar;
                var args = new[] { "foo", HostItem.Wrap(engine, x1), HostItem.Wrap(engine, x2) };
                Assert.IsTrue(bar.GetMetaObject(Expression.Constant(bar)).TryGetIndex(args, out result));
                Assert.AreEqual(x1.ToString() + x2, result);

                // ReSharper restore AccessToModifiedClosure
            })();

            x1 = null;
            x2 = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_InvokeMethod_JScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new JScriptEngine();

            object x1 = Guid.NewGuid();
            var wr1 = new WeakReference(x1);
            object x2 = Guid.NewGuid();
            var wr2 = new WeakReference(x2);

            new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                engine.Execute("function foo(x1, x2) { return x1.ToString() + x2.ToString(); }");
                Assert.AreEqual(x1.ToString() + x2, engine.Script.foo(x1, x2));

                engine.CollectGarbage(true);

                // ReSharper restore AccessToModifiedClosure
            })();

            x1 = null;
            x2 = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_InvokeMethod_VBScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new VBScriptEngine();

            object x1 = Guid.NewGuid();
            var wr1 = new WeakReference(x1);
            object x2 = Guid.NewGuid();
            var wr2 = new WeakReference(x2);

            new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                engine.Execute("function foo(x1, x2):foo = x1.ToString() & x2.ToString():end function");
                Assert.AreEqual(x1.ToString() + x2, engine.Script.foo(x1, x2));

                engine.CollectGarbage(true);

                // ReSharper restore AccessToModifiedClosure
            })();

            x1 = null;
            x2 = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8CachedObjectLeak()
        {
            object x = null;
            WeakReference wr = null;

            new Action(() =>
            {
                using (var tempEngine = new V8ScriptEngine())
                {
                    tempEngine.AddHostType("Action", typeof(Action));
                    x = tempEngine.Evaluate("action = new Action(function () {})");
                    wr = new WeakReference(tempEngine);
                }
            })();

            Assert.IsInstanceOfType(x, typeof(Action));
            TestUtil.AssertException<ObjectDisposedException>((Action)x);

            x = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Resurrection_V8ScriptEngine()
        {
            for (var index = 0; index < 256; index++)
            {
                // ReSharper disable UnusedVariable

                var wrapper = new ResurrectionTestWrapper(new V8ScriptEngine());
                GC.Collect();

                // ReSharper restore UnusedVariable
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Resurrection_V8Runtime()
        {
            for (var index = 0; index < 256; index++)
            {
                // ReSharper disable UnusedVariable

                var wrapper = new ResurrectionTestWrapper(new V8Runtime());
                GC.Collect();

                // ReSharper restore UnusedVariable
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Resurrection_V8Script()
        {
            for (var index = 0; index < 256; index++)
            {
                // ReSharper disable UnusedVariable

                var tempEngine = new V8ScriptEngine();
                var wrapper = new ResurrectionTestWrapper(tempEngine.Compile("function foo() {}"));
                GC.Collect();

                // ReSharper restore UnusedVariable
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_Resurrection_V8ScriptItem()
        {
            for (var index = 0; index < 256; index++)
            {
                // ReSharper disable UnusedVariable

                var tempEngine = new V8ScriptEngine();
                var wrapper = new ResurrectionTestWrapper(tempEngine.Script.Math);
                GC.Collect();

                // ReSharper restore UnusedVariable
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8GlobalMembers_ReadOnlyPropertyCrash()
        {
            // this test is for a crash that occurred only on debug V8 builds
            engine.AddHostObject("bag", HostItemFlags.GlobalMembers, new PropertyBag());
            engine.AddHostObject("test", HostItemFlags.GlobalMembers, new { foo = 123 });
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("foo = 456"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8GlobalMembers_ReadOnlyPropertyCrash_Index()
        {
            // this test is for a crash that occurred only on debug V8 builds
            engine.AddHostObject("bag", HostItemFlags.GlobalMembers, new PropertyBag());
            engine.AddHostObject("test", HostItemFlags.GlobalMembers, new ReadOnlyCollection<int>(new [] { 5, 4, 3, 2, 1 }));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Execute("this[2] = 123"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8GlobalMembers_NativeFunctionHiding()
        {
            engine.Execute("function toString() { return 'ABC'; }");
            engine.AddHostObject("bag", HostItemFlags.GlobalMembers, new PropertyBag());
            Assert.AreEqual("ABC", engine.Evaluate("toString()"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var a = new object();
            var b = new object();
            var c = new object();
            var x = new object();
            var y = new object();
            var z = new object();

            engine.Script.a = a;
            engine.Script.b = b;
            engine.Script.c = c;
            engine.Script.x = x;
            engine.Script.y = y;
            engine.Script.z = z;

            engine.Execute("sub test(i, j, k) : i = x : j = y : k = z : end sub");
            engine.Script.test(ref a, out b, ref c);

            Assert.AreSame(x, a);
            Assert.AreSame(y, b);
            Assert.AreSame(z, c);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_Scalar()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var a = 123;
            var b = 456;
            var c = 789;
            const int x = 987;
            const int y = 654;
            const int z = 321;

            engine.Script.a = a;
            engine.Script.b = b;
            engine.Script.c = c;
            engine.Script.x = x;
            engine.Script.y = y;
            engine.Script.z = z;

            engine.Execute("sub test(i, j, k) : i = x : j = y : k = z : end sub");
            engine.Script.test(ref a, out b, ref c);

            Assert.AreEqual(x, a);
            Assert.AreEqual(y, b);
            Assert.AreEqual(z, c);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_Enum()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var a = DayOfWeek.Monday;
            var b = DayOfWeek.Tuesday;
            var c = DayOfWeek.Wednesday;
            const DayOfWeek x = DayOfWeek.Sunday;
            const DayOfWeek y = DayOfWeek.Saturday;
            const DayOfWeek z = DayOfWeek.Friday;

            engine.Script.a = a;
            engine.Script.b = b;
            engine.Script.c = c;
            engine.Script.x = x;
            engine.Script.y = y;
            engine.Script.z = z;

            engine.Execute("sub test(i, j, k) : i = x : j = y : k = z : end sub");
            engine.Script.test(ref a, out b, ref c);

            Assert.AreEqual(x, a);
            Assert.AreEqual(y, b);
            Assert.AreEqual(z, c);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_Struct()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var random = new Random();
            var a = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);
            var b = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);
            var c = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);
            var x = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);
            var y = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);
            var z = TimeSpan.FromMilliseconds(random.NextDouble() * 1000);

            engine.Script.a = a;
            engine.Script.b = b;
            engine.Script.c = c;
            engine.Script.x = x;
            engine.Script.y = y;
            engine.Script.z = z;

            engine.Execute("sub test(i, j, k) : i = x : j = y : k = z : end sub");
            engine.Script.test(ref a, out b, ref c);

            Assert.AreEqual(x, a);
            Assert.AreEqual(y, b);
            Assert.AreEqual(z, c);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine

                    Dim a = New Object
                    Dim b = New Object
                    Dim c = New Object
                    Dim x = New Object
                    Dim y = New Object
                    Dim z = New Object

                    engine.Script.a = a
                    engine.Script.b = b
                    engine.Script.c = c
                    engine.Script.x = x
                    engine.Script.y = y
                    engine.Script.z = z

                    engine.Execute(""sub test(i, j, k) : i = x : j = y : k = z : end sub"")
                    engine.Script.test(a, b, c)

                    Assert.AreSame(x, a)
                    Assert.AreSame(y, b)
                    Assert.AreSame(z, c)

                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_VB_Scalar()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine

                    Dim a = 123
                    Dim b = 456
                    Dim c = 789
                    Dim x = 987
                    Dim y = 654
                    Dim z = 321

                    engine.Script.a = a
                    engine.Script.b = b
                    engine.Script.c = c
                    engine.Script.x = x
                    engine.Script.y = y
                    engine.Script.z = z

                    engine.Execute(""sub test(i, j, k) : i = x : j = y : k = z : end sub"")
                    engine.Script.test(a, b, c)

                    Assert.AreEqual(x, a)
                    Assert.AreEqual(y, b)
                    Assert.AreEqual(z, c)

                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_VB_Enum()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine

                    Dim a = DayOfWeek.Monday
                    Dim b = DayOfWeek.Tuesday
                    Dim c = DayOfWeek.Wednesday
                    Dim x = DayOfWeek.Sunday
                    Dim y = DayOfWeek.Saturday
                    Dim z = DayOfWeek.Friday

                    engine.Script.a = a
                    engine.Script.b = b
                    engine.Script.c = c
                    engine.Script.x = x
                    engine.Script.y = y
                    engine.Script.z = z

                    engine.Execute(""sub test(i, j, k) : i = x : j = y : k = z : end sub"")
                    engine.Script.test(a, b, c)

                    Assert.AreEqual(x, a)
                    Assert.AreEqual(y, b)
                    Assert.AreEqual(z, c)

                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScriptItemArgsByRef_VB_Struct()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine

                    Dim random = New Random
                    Dim a = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)
                    Dim b = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)
                    Dim c = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)
                    Dim x = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)
                    Dim y = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)
                    Dim z = TimeSpan.FromMilliseconds(random.NextDouble() * 1000)

                    engine.Script.a = a
                    engine.Script.b = b
                    engine.Script.c = c
                    engine.Script.x = x
                    engine.Script.y = y
                    engine.Script.z = z

                    engine.Execute(""sub test(i, j, k) : i = x : j = y : k = z : end sub"")
                    engine.Script.test(a, b, c)

                    Assert.AreEqual(x, a)
                    Assert.AreEqual(y, b)
                    Assert.AreEqual(z, c)

                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_CallHostObjectFunctionAsConstructor()
        {
            engine.Script.random = new Random();
            engine.AddHostType("Random", typeof(Random));
            var result = engine.Evaluate(@"
                (function () {
                    var x = new Random().NextDouble();
                    try {
                        return new random.constructor();
                    }
                    catch (ex) {
                        return new Random().NextDouble() * x;
                    }
                    return false;
                })()
            ");
            Assert.IsInstanceOfType(result, typeof(double));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_HostItemCachingForHostVariables()
        {
            var foo = new HostFunctions().newVar(new object());
            engine.Script.foo1 = foo;
            engine.Script.foo2 = foo;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("foo1 === foo2")));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_MetaScriptItem_GetDynamicMemberNames()
        {
            var dmop = (IDynamicMetaObjectProvider)engine.Evaluate("({ foo: 123, bar: 456, baz: 789 })");
            var dmo = dmop.GetMetaObject(Expression.Constant(dmop));
            var names = dmo.GetDynamicMemberNames().ToArray();
            Assert.IsTrue(names.Contains("foo"));
            Assert.IsTrue(names.Contains("bar"));
            Assert.IsTrue(names.Contains("baz"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AmbiguousIndexer()
        {
            IAmbiguousIndexer indexer = new AmbiguousIndexer();
            engine.AddRestrictedHostObject("indexer", indexer);
            engine.AddHostType("DayOfWeek", typeof(DayOfWeek));

            engine.Execute("indexer.Item.set(123, 456)");
            Assert.AreEqual(456, engine.Evaluate("indexer.Item(123)"));
            Assert.IsNull(engine.Evaluate("indexer.Item(789)"));

            engine.Execute("indexer.Item.set(DayOfWeek.Thursday, DayOfWeek.Sunday)");
            Assert.AreEqual(DayOfWeek.Sunday, engine.Evaluate("indexer.Item(DayOfWeek.Thursday)"));
            Assert.IsNull(engine.Evaluate("indexer.Item(DayOfWeek.Tuesday)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AmbiguousIndexer_ADODB()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            var recordSet = new ADODB.Recordset();
            recordSet.Fields.Append("foo", ADODB.DataTypeEnum.adVarChar, 20);
            recordSet.Open(Missing.Value, Missing.Value, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic, 0);
            recordSet.AddNew(Missing.Value, Missing.Value);
            recordSet.Fields["foo"].Value = "bar";

            engine.AddHostObject("recordSet", recordSet);
            Assert.AreEqual("bar", engine.Evaluate("recordSet.Fields.Item(\"foo\").Value"));

            engine.Execute("recordSet.Fields.Item(\"foo\").Value = \"qux\"");
            Assert.AreEqual("qux", engine.Evaluate("recordSet.Fields.Item(\"foo\").Value"));

            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("recordSet.Fields.Item(\"baz\")"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_InaccessiblePropertyAccessors()
        {
            engine.Script.foo = new InaccessiblePropertyAccessors();

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.NoGetter"));
            Assert.AreEqual(123, engine.Evaluate("foo.NoGetter = 123"));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.PrivateGetter"));
            Assert.AreEqual(456, engine.Evaluate("foo.PrivateGetter = 456"));

            Assert.AreEqual(456, engine.Evaluate("foo.NoSetter"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.NoSetter = 789"));

            Assert.AreEqual(456, engine.Evaluate("foo.PrivateSetter"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.PrivateSetter = 789"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_InaccessiblePropertyAccessors_Static()
        {
            engine.AddHostType("foo", typeof(InaccessiblePropertyAccessorsStatic));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.NoGetter"));
            Assert.AreEqual(123, engine.Evaluate("foo.NoGetter = 123"));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.PrivateGetter"));
            Assert.AreEqual(456, engine.Evaluate("foo.PrivateGetter = 456"));

            Assert.AreEqual(456, engine.Evaluate("foo.NoSetter"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.NoSetter = 789"));

            Assert.AreEqual(456, engine.Evaluate("foo.PrivateSetter"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("foo.PrivateSetter = 789"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8RuntimeConstraintScale()
        {
            const int maxNewSpaceSize = 16;
            const int maxOldSpaceSize = 512;

            var constraints = new V8RuntimeConstraints
            {
                MaxNewSpaceSize = maxNewSpaceSize,
                MaxOldSpaceSize = maxOldSpaceSize
            };

            using (var tempEngine = new V8ScriptEngine(constraints))
            {
                Assert.AreEqual(Math.PI, tempEngine.Evaluate("Math.PI"));
                Assert.AreEqual(Convert.ToUInt64(maxNewSpaceSize * 4 + maxOldSpaceSize), tempEngine.GetRuntimeHeapInfo().HeapSizeLimit / (1024 * 1024));
            }

            constraints = new V8RuntimeConstraints
            {
                MaxNewSpaceSize = maxNewSpaceSize * 1024 * 1024,
                MaxOldSpaceSize = maxOldSpaceSize * 1024 * 1024
            };

            using (var tempEngine = new V8ScriptEngine(constraints))
            {
                Assert.AreEqual(Math.E, tempEngine.Evaluate("Math.E"));
                Assert.AreEqual(Convert.ToUInt64(maxNewSpaceSize * 4 + maxOldSpaceSize), tempEngine.GetRuntimeHeapInfo().HeapSizeLimit / (1024 * 1024));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_TooManyDebugApplications()
        {
            var engines = Enumerable.Range(0, 2048).Select(index => new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging)).ToArray();
            Array.ForEach(engines, tempEngine => tempEngine.Dispose());
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AssemblyTableClass()
        {
            Assert.IsTrue(typeof(AssemblyTable).IsStatic());
            Assert.IsNotNull(typeof(AssemblyTable).TypeInitializer);

            var methods = typeof(AssemblyTable).GetMethods(BindingFlags.Static | BindingFlags.Public);
            Assert.AreEqual(1, methods.Length);
            Assert.AreEqual("GetFullAssemblyName", methods[0].Name);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AssemblyHelpersClass()
        {
            Assert.IsTrue(typeof(AssemblyHelpers).IsStatic());
            Assert.IsNull(typeof(AssemblyHelpers).TypeInitializer);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_FinalizerHang_V8ScriptItem()
        {
            engine.Script.foo = new Action<object>(arg => {});
            engine.Script.bar = new Action(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

            engine.Execute(@"
                for (var i = 0; i < 100; i++) {
                    foo({ index: i });
                }
                bar();
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_FinalizerHang_V8Script()
        {
            engine.Script.bar = new Action(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

            for (var index = 0; index < 100; index++)
            {
                ((V8ScriptEngine)engine).Compile("function foo() { return " + index + "; }");
            }

            engine.Execute("bar()");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_FinalizerHang_V8ScriptEngine()
        {
            engine.Dispose();

            using (var runtime = new V8Runtime(V8RuntimeFlags.EnableDebugging))
            {
                engine = runtime.CreateScriptEngine();
                engine.Script.bar = new Action(() =>
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });

                for (var index = 0; index < 100; index++)
                {
                    runtime.CreateScriptEngine();
                }

                engine.Execute("bar()");
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DynamicMethodArgs()
        {
            engine.Script.foo = new DynamicMethodArgTest();
            Assert.AreEqual("123 456.789 hello", engine.Evaluate("foo.RunTest(123, 456.789, 'hello')"));
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
                return value + 1;
            }

            public object Method(double? value)
            {
                return value * 2.0;
            }
        }

        public abstract class AmbiguousAttributeTestBase
        {
            public abstract object this[int index] { get; }

            public abstract object this[string key] { get; }
        }

        public class AmbiguousAttributeTest : AmbiguousAttributeTestBase
        {
            public override object this[int index] { get { return null; } }

            public override object this[string key] { get { return null; } }

            public void Foo() {}
        }

        public class ResurrectionTestWrapper
        {
            private readonly IDisposable target;

            public ResurrectionTestWrapper(IDisposable target)
            {
                this.target = target;
            }

            ~ResurrectionTestWrapper()
            {
                target.Dispose();
            }
        }

        public interface IAmbiguousIndexerBase1
        {
            object this[int i] { get; set; }
        }

        public interface IAmbiguousIndexerBase2
        {
            object this[int i] { get; set; }
            object this[DayOfWeek d] { get; set; }
        }

        public interface IAmbiguousIndexer : IAmbiguousIndexerBase1, IAmbiguousIndexerBase2
        {
            new object this[int i] { get; set; }
        }

        public class AmbiguousIndexer : IAmbiguousIndexer
        {
            private readonly IDictionary byInteger = new ListDictionary();
            private readonly IDictionary byDayOfWeek = new ListDictionary();

            public object this[int key]
            {
                get { return byInteger[key]; }
                set { byInteger[key] = value; }
            }

            public object this[DayOfWeek key]
            {
                get { return byDayOfWeek[key]; }
                set { byDayOfWeek[key] = value; }
            }
        }

        public class InaccessiblePropertyAccessors
        {
            // ReSharper disable UnusedMember.Local

            private int dummy;

            public int NoGetter
            {
                set { dummy = value; }
            }

            public int PrivateGetter
            {
                private get { return dummy; }
                set { dummy = value; }
            }

            public int NoSetter
            {
                get { return dummy; }
            }

            public int PrivateSetter
            {
                get { return dummy; }
                private set { dummy = value; }
            }

            // ReSharper restore UnusedMember.Local
        }

        public static class InaccessiblePropertyAccessorsStatic
        {
            // ReSharper disable UnusedMember.Local

            private static int dummy = 12345;

            public static int NoGetter
            {
                set { dummy = value; }
            }

            public static int PrivateGetter
            {
                private get { return dummy; }
                set { dummy = value; }
            }

            public static int NoSetter
            {
                get { return dummy; }
            }

            public static int PrivateSetter
            {
                get { return dummy; }
                private set { dummy = value; }
            }

            // ReSharper restore UnusedMember.Local
        }

        public class DynamicMethodArgTest : DynamicObject
        {
            public override IEnumerable<string> GetDynamicMemberNames()
            {
                yield return "RunTest";
            }

            // ReSharper disable RedundantOverridenMember

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                // this override is redundant, but required for the test
                return base.TryGetMember(binder, out result);
            }

            // ReSharper restore RedundantOverridenMember

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (binder.Name == "RunTest")
                {
                    result = string.Join(" ", args);
                    return true;
                }

                return base.TryInvokeMember(binder, args, out result);
            }
        }

        #endregion
    }
}
