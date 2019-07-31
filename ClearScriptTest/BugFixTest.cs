// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
using System.Xml.Linq;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using UIAutomationClient;

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
    [DeploymentItem("ClearScriptConsole.exe")]
    [DeploymentItem("Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
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
            Assert.AreEqual("[object Object]", engine.ExecuteCommand("this"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VariantClear()
        {
            // ReSharper disable RedundantAssignment

            WeakReference wr = null;

            new Action(() =>
            {
                var x = new object();
                wr = new WeakReference(x);

                VariantClearTestHelper(x);
                x = null;
            })();

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
            WeakReference wr = null;

            new Action(() =>
            {
                // ReSharper disable RedundantAssignment

                object x = null;

                using (var tempEngine = new V8ScriptEngine())
                {
                    tempEngine.AddHostType("Action", typeof(Action));
                    x = tempEngine.Evaluate("action = new Action(function () {})");
                    wr = new WeakReference(tempEngine);
                }

                Assert.IsInstanceOfType(x, typeof(Action));
                TestUtil.AssertException<ObjectDisposedException>((Action)x);

                x = null;

                // ReSharper restore RedundantAssignment
            })();

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
            engine.AddHostObject("test", HostItemFlags.GlobalMembers, new ReadOnlyCollection<int>(new[] { 5, 4, 3, 2, 1 }));
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
            const double tolerance = .05;

            var constraints = new V8RuntimeConstraints
            {
                MaxNewSpaceSize = maxNewSpaceSize,
                MaxOldSpaceSize = maxOldSpaceSize
            };

            using (var tempEngine = new V8ScriptEngine(constraints))
            {
                Assert.AreEqual(Math.PI, tempEngine.Evaluate("Math.PI"));
                var expected = Convert.ToDouble(maxNewSpaceSize * 2 + maxOldSpaceSize);
                var actual = Convert.ToDouble(tempEngine.GetRuntimeHeapInfo().HeapSizeLimit / (1024 * 1024));
                var ratio = actual / expected;
                Assert.IsTrue((ratio >= 1 - tolerance) && (ratio <= 1 + tolerance));
            }

            constraints = new V8RuntimeConstraints
            {
                MaxNewSpaceSize = maxNewSpaceSize * 1024 * 1024,
                MaxOldSpaceSize = maxOldSpaceSize * 1024 * 1024
            };

            using (var tempEngine = new V8ScriptEngine(constraints))
            {
                Assert.AreEqual(Math.E, tempEngine.Evaluate("Math.E"));
                var expected = Convert.ToDouble(maxNewSpaceSize * 2 + maxOldSpaceSize);
                var actual = Convert.ToDouble(tempEngine.GetRuntimeHeapInfo().HeapSizeLimit / (1024 * 1024));
                var ratio = actual / expected;
                Assert.IsTrue((ratio >= 1 - tolerance) && (ratio <= 1 + tolerance));
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

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedInterrupt()
        {
            var context = new PropertyBag();
            engine.AddHostObject("context", context);

            using (var startEvent = new ManualResetEventSlim(false))
            {
                object result = null;
                var interruptedInner = false;
                var interruptedOuter = false;

                context["startEvent"] = startEvent;
                context["foo"] = new Action(() =>
                {
                    try
                    {
                        engine.Execute("while (true) { context.startEvent.Set(); }");
                    }
                    catch (ScriptInterruptedException)
                    {
                        interruptedInner = true;
                    }
                });

                var thread = new Thread(() =>
                {
                    try
                    {
                        result = engine.Evaluate("context.foo(); 123");
                    }
                    catch (ScriptInterruptedException)
                    {
                        interruptedOuter = true;
                    }
                });

                thread.Start();
                startEvent.Wait();
                engine.Interrupt();
                thread.Join();

                Assert.IsTrue(interruptedInner);
                Assert.IsFalse(interruptedOuter);
                Assert.AreEqual(123, result);
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedInterrupt_JScript()
        {
            engine.Dispose();
            try
            {
                using (var startEvent = new ManualResetEventSlim(false))
                {
                    object result = null;
                    var interruptedInner = false;
                    var interruptedOuter = false;

                    var thread = new Thread(() =>
                    {
                        using (engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging))
                        {
                            var context = new PropertyBag();
                            engine.AddHostObject("context", context);

                            // ReSharper disable once AccessToDisposedClosure
                            context["startEvent"] = startEvent;
                            context["foo"] = new Action(() =>
                            {
                                try
                                {
                                    engine.Execute("while (true) { context.startEvent.Set(); }");
                                }
                                catch (ScriptInterruptedException)
                                {
                                    interruptedInner = true;
                                }
                            });

                            try
                            {
                                result = engine.Evaluate("context.foo(); 123");
                            }
                            catch (ScriptInterruptedException)
                            {
                                interruptedOuter = true;
                            }
                        }
                    });

                    thread.Start();
                    startEvent.Wait();
                    engine.Interrupt();
                    thread.Join();

                    Assert.IsTrue(interruptedInner);
                    Assert.IsFalse(interruptedOuter);
                    Assert.AreEqual(123, result);
                }
            }
            finally
            {
                engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_PropertyBag_NativeEnumerator_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();

            var x = new PropertyBag();
            x["foo"] = 123;
            x["bar"] = "blah";
            engine.Script.x = x;

            var result = (string)engine.Evaluate(@"
                var result = '';
                for (var e = new Enumerator(x); !e.atEnd(); e.moveNext()) {
                    result += e.item().Value;
                }
                result
            ");

            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_PropertyBag_NativeEnumerator_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var x = new PropertyBag();
            x["foo"] = 123;
            x["bar"] = "blah";
            engine.Script.x = x;

            engine.Execute(@"
                function getResult(arg)
                    dim result
                    result = """"
                    for each item in arg
                        result = result & item.Value
                    next
                    getResult = result
                end function
            ");

            var result = (string)engine.Evaluate("getResult(x)");

            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_PropertyAccess()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);
            engine.Script.x = new { foo = 123 };
            Assert.AreEqual(123, engine.Evaluate("x.foo"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_MemberEnumeration()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);

            engine.Script.x = new { foo = 123, bar = "blah" };
            var result = (string)engine.Evaluate(@"
                var result = '';
                for (var i in x) {
                    if ((i == 'foo') || (i == 'bar')) {
                        result += x[i];
                    }
                }
                result
            ");

            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_MemberEnumeration_PropertyBag()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);

            var x = new PropertyBag();
            x["foo"] = 123;
            x["bar"] = "blah";
            engine.Script.x = x;

            var result = (string)engine.Evaluate(@"
                var result = '';
                for (var i in x) {
                    result += x[i];
                }
                result
            ");

            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_MemberEnumeration_Dynamic()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);

            dynamic x = new ExpandoObject();
            x.foo = 123;
            x.bar = "blah";
            engine.Script.x = x;

            var result = (string)engine.Evaluate(@"
                var result = '';
                for (var i in x) {
                    if ((i == 'foo') || (i == 'bar')) {
                        result += x[i];
                    }
                }
                result
            ");

            Assert.AreEqual(7, result.Length);
            Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
            Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_MemberDeletion_PropertyBag()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);

            var x = new PropertyBag();
            x["foo"] = 123;
            x["bar"] = "blah";
            engine.Script.x = x;

            Assert.AreEqual(123, engine.Evaluate("x.foo"));
            Assert.AreEqual("blah", engine.Evaluate("x.bar"));
            Assert.AreEqual(true, engine.Evaluate("delete x.foo"));
            Assert.IsInstanceOfType(engine.Evaluate("x.foo"), typeof(Undefined));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScriptStandardsMode_MemberDeletion_Dynamic()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode);

            dynamic x = new ExpandoObject();
            x.foo = 123;
            x.bar = "blah";
            engine.Script.x = x;

            Assert.AreEqual(123, engine.Evaluate("x.foo"));
            Assert.AreEqual("blah", engine.Evaluate("x.bar"));
            Assert.AreEqual(true, engine.Evaluate("delete x.foo"));
            Assert.IsInstanceOfType(engine.Evaluate("x.foo"), typeof(Undefined));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NumericArgConversion_Delegate()
        {
            engine.Script.host = new HostFunctions();
            engine.Script.sbyteFunc = new Func<sbyte, sbyte>(arg => arg);
            engine.Script.nullableSByteFunc = new Func<sbyte?, sbyte?>(arg => arg);
            engine.Script.floatFunc = new Func<float, float>(arg => arg);
            engine.Script.nullableFloatFunc = new Func<float?, float?>(arg => arg);
            engine.Script.doubleFunc = new Func<double, double>(arg => arg);
            engine.Script.nullableDoubleFunc = new Func<double?, double?>(arg => arg);
            engine.Script.decimalFunc = new Func<decimal, decimal>(arg => arg);
            engine.Script.nullableDecimalFunc = new Func<decimal?, decimal?>(arg => arg);

            Assert.AreEqual(123, engine.Evaluate("sbyteFunc(123)"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("sbyteFunc(234)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("sbyteFunc(123.5)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("sbyteFunc(Math.PI)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("sbyteFunc(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("nullableSByteFunc(123)"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("nullableSByteFunc(234)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("nullableSByteFunc(123.5)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("nullableSByteFunc(Math.PI)"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("nullableSByteFunc(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("nullableSByteFunc(null)"));

            Assert.AreEqual(123, engine.Evaluate("floatFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("floatFunc(123.5)"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("floatFunc(Math.PI)"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("floatFunc(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("nullableFloatFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("nullableFloatFunc(123.5)"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("nullableFloatFunc(Math.PI)"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("nullableFloatFunc(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("nullableFloatFunc(null)"));

            Assert.AreEqual(123, engine.Evaluate("doubleFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("doubleFunc(123.5)"));
            Assert.AreEqual(Math.PI, engine.Evaluate("doubleFunc(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("doubleFunc(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("nullableDoubleFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("nullableDoubleFunc(123.5)"));
            Assert.AreEqual(Math.PI, engine.Evaluate("nullableDoubleFunc(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("nullableDoubleFunc(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("nullableDoubleFunc(null)"));

            Assert.AreEqual(123, engine.Evaluate("decimalFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("decimalFunc(123.5)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("decimalFunc(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("decimalFunc(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("nullableDecimalFunc(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("nullableDecimalFunc(123.5)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("nullableDecimalFunc(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("nullableDecimalFunc(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("nullableDecimalFunc(null)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NumericArgConversion_Field()
        {
            engine.Script.host = new HostFunctions();
            engine.Script.test = new NumericArgConversionTest();

            Assert.AreEqual(123, engine.Evaluate("test.SByteField = 123; test.SByteField"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.SByteField = 234"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteField = 123.5"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteField = Math.PI"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteField = host.toDecimal(Math.PI)"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableSByteField = 123; test.NullableSByteField"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.NullableSByteField = 234"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteField = 123.5"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteField = Math.PI"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteField = host.toDecimal(Math.PI)"));
            Assert.IsNull(engine.Evaluate("test.NullableSByteField = null; test.NullableSByteField"));

            Assert.AreEqual(123, engine.Evaluate("test.FloatField = 123; test.FloatField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.FloatField = 123.5; test.FloatField"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.FloatField = Math.PI; test.FloatField"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.FloatField = host.toDecimal(Math.PI); test.FloatField"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableFloatField = 123; test.NullableFloatField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableFloatField = 123.5; test.NullableFloatField"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.NullableFloatField = Math.PI; test.NullableFloatField"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.NullableFloatField = host.toDecimal(Math.PI); test.NullableFloatField"));
            Assert.IsNull(engine.Evaluate("test.NullableFloatField = null; test.NullableFloatField"));

            Assert.AreEqual(123, engine.Evaluate("test.DoubleField = 123; test.DoubleField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.DoubleField = 123.5; test.DoubleField"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.DoubleField = Math.PI; test.DoubleField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DoubleField = host.toDecimal(Math.PI); test.DoubleField"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDoubleField = 123; test.NullableDoubleField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableDoubleField = 123.5; test.NullableDoubleField"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.NullableDoubleField = Math.PI; test.NullableDoubleField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDoubleField = host.toDecimal(Math.PI); test.NullableDoubleField"));
            Assert.IsNull(engine.Evaluate("test.NullableDoubleField = null; test.NullableDoubleField"));

            Assert.AreEqual(123, engine.Evaluate("test.DecimalField = 123; test.DecimalField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.DecimalField = 123.5; test.DecimalField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DecimalField = Math.PI; test.DecimalField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DecimalField = host.toDecimal(Math.PI); test.DecimalField"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDecimalField = 123; test.NullableDecimalField"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableDecimalField = 123.5; test.NullableDecimalField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDecimalField = Math.PI; test.NullableDecimalField"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDecimalField = host.toDecimal(Math.PI); test.NullableDecimalField"));
            Assert.IsNull(engine.Evaluate("test.NullableDecimalField = null; test.NullableDecimalField"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NumericArgConversion_Method()
        {
            engine.Script.host = new HostFunctions();
            engine.Script.test = new NumericArgConversionTest();

            Assert.AreEqual(123, engine.Evaluate("test.SByteMethod(123)"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.SByteMethod(234)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.SByteMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.SByteMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.SByteMethod(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableSByteMethod(123)"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.NullableSByteMethod(234)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableSByteMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableSByteMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableSByteMethod(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("test.NullableSByteMethod(null)"));

            Assert.AreEqual(123, engine.Evaluate("test.FloatMethod(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.FloatMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.FloatMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.FloatMethod(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableFloatMethod(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableFloatMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableFloatMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableFloatMethod(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("test.NullableFloatMethod(null)"));

            Assert.AreEqual(123, engine.Evaluate("test.DoubleMethod(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.DoubleMethod(123.5)"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.DoubleMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.DoubleMethod(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDoubleMethod(123)"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableDoubleMethod(123.5)"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.NullableDoubleMethod(Math.PI)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableDoubleMethod(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("test.NullableDoubleMethod(null)"));

            Assert.AreEqual(123, engine.Evaluate("test.DecimalMethod(123)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.DecimalMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.DecimalMethod(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DecimalMethod(host.toDecimal(Math.PI))"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDecimalMethod(123)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableDecimalMethod(123.5)"));
            TestUtil.AssertException<RuntimeBinderException>(() => engine.Execute("test.NullableDecimalMethod(Math.PI)"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDecimalMethod(host.toDecimal(Math.PI))"));
            Assert.IsNull(engine.Evaluate("test.NullableDecimalMethod(null)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NumericArgConversion_Property()
        {
            engine.Script.host = new HostFunctions();
            engine.Script.test = new NumericArgConversionTest();

            Assert.AreEqual(123, engine.Evaluate("test.SByteProperty = 123; test.SByteProperty"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.SByteProperty = 234"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteProperty = 123.5"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteProperty = Math.PI"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.SByteProperty = host.toDecimal(Math.PI)"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableSByteProperty = 123; test.NullableSByteProperty"));
            TestUtil.AssertException<OverflowException>(() => engine.Execute("test.NullableSByteProperty = 234"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteProperty = 123.5"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteProperty = Math.PI"));
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("test.NullableSByteProperty = host.toDecimal(Math.PI)"));
            Assert.IsNull(engine.Evaluate("test.NullableSByteProperty = null; test.NullableSByteProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.FloatProperty = 123; test.FloatProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.FloatProperty = 123.5; test.FloatProperty"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.FloatProperty = Math.PI; test.FloatProperty"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.FloatProperty = host.toDecimal(Math.PI); test.FloatProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableFloatProperty = 123; test.NullableFloatProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableFloatProperty = 123.5; test.NullableFloatProperty"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.NullableFloatProperty = Math.PI; test.NullableFloatProperty"));
            Assert.AreEqual((float)Math.PI, engine.Evaluate("test.NullableFloatProperty = host.toDecimal(Math.PI); test.NullableFloatProperty"));
            Assert.IsNull(engine.Evaluate("test.NullableFloatProperty = null; test.NullableFloatProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.DoubleProperty = 123; test.DoubleProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.DoubleProperty = 123.5; test.DoubleProperty"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.DoubleProperty = Math.PI; test.DoubleProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DoubleProperty = host.toDecimal(Math.PI); test.DoubleProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDoubleProperty = 123; test.NullableDoubleProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableDoubleProperty = 123.5; test.NullableDoubleProperty"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.NullableDoubleProperty = Math.PI; test.NullableDoubleProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDoubleProperty = host.toDecimal(Math.PI); test.NullableDoubleProperty"));
            Assert.IsNull(engine.Evaluate("test.NullableDoubleProperty = null; test.NullableDoubleProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.DecimalProperty = 123; test.DecimalProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.DecimalProperty = 123.5; test.DecimalProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DecimalProperty = Math.PI; test.DecimalProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.DecimalProperty = host.toDecimal(Math.PI); test.DecimalProperty"));

            Assert.AreEqual(123, engine.Evaluate("test.NullableDecimalProperty = 123; test.NullableDecimalProperty"));
            Assert.AreEqual(123.5f, engine.Evaluate("test.NullableDecimalProperty = 123.5; test.NullableDecimalProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDecimalProperty = Math.PI; test.NullableDecimalProperty"));
            Assert.AreEqual((double)(decimal)Math.PI, engine.Evaluate("test.NullableDecimalProperty = host.toDecimal(Math.PI); test.NullableDecimalProperty"));
            Assert.IsNull(engine.Evaluate("test.NullableDecimalProperty = null; test.NullableDecimalProperty"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_ScriptObjectInHostVariable()
        {
            engine.Script.host = new HostFunctions();
            var documentInfo = new DocumentInfo("Expression") { Flags = DocumentFlags.IsTransient };
            Assert.IsTrue(engine.Evaluate(documentInfo.MakeUnique(engine), "host.newVar({})", false).ToString().StartsWith("[HostVariable:", StringComparison.Ordinal));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_ScriptObjectInHostVariable_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_ScriptObjectInHostVariable();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8HostMethodClobbering()
        {
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
            TestUtil.AssertException<MissingMemberException>(() => engine.Execute("host.newObj = 123"));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8NativePropertyHiding()
        {
            var foo = new { toString = new Func<string>(() => "testValue") };
            engine.Script.foo = foo;
            Assert.AreEqual(foo.toString(), engine.Evaluate("foo.toString()"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8NativePropertyHiding_Method()
        {
            var foo = new HideNativeJavaScriptMethod();
            engine.Script.foo = foo;
            Assert.AreEqual(foo.toString(), engine.Evaluate("foo.toString()"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_EquatableComparison_DateTime()
        {
            engine.Script.x = new DateTime(2007, 5, 22);
            engine.Script.y = new DateTime(2007, 5, 22);
            engine.Script.z = new DateTime(2008, 5, 22);
            Assert.IsTrue((bool)engine.Evaluate("(x == y) && (x === y)"));
            Assert.IsFalse((bool)engine.Evaluate("(x != y) || (x !== y)"));
            Assert.IsTrue((bool)engine.Evaluate("(x != z) && (x !== z)"));
            Assert.IsFalse((bool)engine.Evaluate("(x == z) || (x === z)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_EquatableComparison_TimeSpan()
        {
            engine.Script.x = TimeSpan.FromSeconds(3.5);
            engine.Script.y = TimeSpan.FromSeconds(3.5);
            engine.Script.z = TimeSpan.FromSeconds(3.6);
            Assert.IsTrue((bool)engine.Evaluate("(x == y) && (x === y)"));
            Assert.IsFalse((bool)engine.Evaluate("(x != y) || (x !== y)"));
            Assert.IsTrue((bool)engine.Evaluate("(x != z) && (x !== z)"));
            Assert.IsFalse((bool)engine.Evaluate("(x == z) || (x === z)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_EquatableComparison_WrappedNumeric()
        {
            var host = new HostFunctions();
            engine.Script.x = host.toUInt16(25);
            engine.Script.y = host.toUInt16(25);
            engine.Script.z = host.toUInt16(26);
            Assert.IsTrue((bool)engine.Evaluate("(x == y) && (x === y)"));
            Assert.IsFalse((bool)engine.Evaluate("(x != y) || (x !== y)"));
            Assert.IsTrue((bool)engine.Evaluate("(x != z) && (x !== z)"));
            Assert.IsFalse((bool)engine.Evaluate("(x == z) || (x === z)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_PropertyPut()
        {
            using (var vbEngine = new VBScriptEngine())
            {
                vbEngine.Execute(@"
                    class Test
                        private myFoo
                        public property get foo
                            foo = myFoo
                        end property
                        public property let foo(value)
                            myFoo = value
                        end property
                    end class 
                    set myTest = new Test
                    myTest.foo = 123
                ");

                Assert.AreEqual(123, Convert.ToInt32(vbEngine.Script.myTest.foo));

                vbEngine.Script.myTest.foo = 456;
                Assert.AreEqual(456, Convert.ToInt32(vbEngine.Script.myTest.foo));

                vbEngine.Script.myTest.foo = "blah";
                Assert.AreEqual("blah", vbEngine.Script.myTest.foo);

                vbEngine.Script.myTest.foo = new DateTime(2007, 5, 22);
                Assert.AreEqual(new DateTime(2007, 5, 22), vbEngine.Script.myTest.foo);
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_PropertyPut_CrossEngine()
        {
            using (var vbEngine = new VBScriptEngine())
            {
                vbEngine.Execute(@"
                    class Test
                        private myFoo
                        public property get foo
                            foo = myFoo
                        end property
                        public property let foo(value)
                            myFoo = value
                        end property
                    end class 
                    set myTest = new Test
                    myTest.foo = 123
                ");

                engine.Script.test = vbEngine.Script.myTest;
                Assert.AreEqual(123, Convert.ToInt32(engine.Evaluate("test.foo")));

                engine.Execute("test.foo = 456");
                Assert.AreEqual(456, Convert.ToInt32(engine.Evaluate("test.foo")));

                engine.Execute("test.foo = \"blah\"");
                Assert.AreEqual("blah", engine.Evaluate("test.foo"));

                engine.AddHostObject("bar", new DateTime(2007, 5, 22));
                engine.Execute("test.foo = bar");
                Assert.AreEqual(new DateTime(2007, 5, 22), engine.Evaluate("test.foo"));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_PropertyPut_CrossEngine_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();
            BugFix_VBScript_PropertyPut_CrossEngine();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AddHostType_AssemblyQualifiedName()
        {
            engine.AddHostType("Random", typeof(Random).AssemblyQualifiedName);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution()
        {
            engine.Execute(@"
                var num = 0;
                function foo() {
                    num += 1;
                    throw new Error();
                }
            ");

            try
            {
                engine.Script.foo();
            }
            catch (ScriptEngineException)
            {
            }

            Assert.AreEqual(1, engine.Script.num);

            engine.Script.num = 0;
            dynamic foo = engine.Script.foo;

            try
            {
                foo();
            }
            catch (ScriptEngineException)
            {
            }

            Assert.AreEqual(1, engine.Script.num);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_DoubleExecution();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_CrossEngine()
        {
            using (var vbEngine = new VBScriptEngine())
            {
                vbEngine.Execute(@"
                    class Test
                        public sub foo
                            count = count + 1
                            err.raise 1, ""vb"", ""Bogus""
                        end sub
                    end class
                    set myTest = new Test
                    count = 0
                ");

                engine.Execute(@"
                    function foo() {
                        vbTest.foo();
                    }
                ");

                engine.Script.vbTest = vbEngine.Script.myTest;

                var message = string.Empty;
                try
                {
                    engine.Script.foo();
                }
                catch (ScriptEngineException exception)
                {
                    message = exception.Message;
                }

                Assert.AreEqual("Error: Bogus", message);
                Assert.AreEqual(1, vbEngine.Script.count);
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_CrossEngine_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            using (var vbEngine = new VBScriptEngine())
            {
                vbEngine.Execute(@"
                    class Test
                        public sub foo
                            count = count + 1
                            err.raise 1, ""vb"", ""Bogus""
                        end sub
                    end class
                    set myTest = new Test
                    count = 0
                ");

                engine.Execute(@"
                    sub foo
                        vbTest.foo
                    end sub
                ");

                engine.Script.vbTest = vbEngine.Script.myTest;

                var message = string.Empty;
                try
                {
                    engine.Script.foo();
                }
                catch (ScriptEngineException exception)
                {
                    message = exception.Message;
                }

                Assert.AreEqual("Bogus", message);
                Assert.AreEqual(1, vbEngine.Script.count);
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_Delegate()
        {
            // ReSharper disable EmptyGeneralCatchClause

            var num = 0;
            engine.Script.foo = new Action(() =>
            {
                // ReSharper disable AccessToModifiedClosure

                num += 1;
                throw new Exception();

                // ReSharper restore AccessToModifiedClosure
            });

            try
            {
                engine.Script.foo();
            }
            catch (ScriptEngineException)
            {
            }

            Assert.AreEqual(1, num);

            num = 0;
            dynamic foo = engine.Script.foo;

            try
            {
                foo();
            }
            catch (Exception)
            {
            }

            Assert.AreEqual(1, num);

            // ReSharper restore EmptyGeneralCatchClause
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_Delegate_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_DoubleExecution_Delegate();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_SharedV8RuntimeLeak()
        {
            using (var runtime = new V8Runtime())
            {
                using (runtime.CreateScriptEngine())
                {
                }

                runtime.CollectGarbage(true);
                var heapSize = runtime.GetHeapInfo().TotalHeapSize;

                for (var i = 0; i < 500; i++)
                {
                    using (runtime.CreateScriptEngine())
                    {
                    }

                    if ((i % 25) == 24)
                    {
                        runtime.CollectGarbage(true);
                    }
                }

                runtime.CollectGarbage(true);
                Assert.IsFalse(runtime.GetHeapInfo().TotalHeapSize > (heapSize * 1.75));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8ExceptionWhileInterrupting()
        {
            var context = new PropertyBag();
            engine.Script.context = context;

            var startEvent = new ManualResetEventSlim();
            var waitEvent = new ManualResetEventSlim();
            var tokenSource = new CancellationTokenSource();

            context["count"] = 0;
            context["startEvent"] = startEvent;
            context["waitForCancel"] = new Action(() => waitEvent.Wait(tokenSource.Token));

            var thread = new Thread(() =>
            {
                try
                {
                    engine.Execute(@"
                        context.count = 1;
                        context.startEvent.Set();
                        context.waitForCancel();
                        context.count = 2;
                    ");
                }
                catch (ScriptEngineException)
                {
                }
                catch (ScriptInterruptedException)
                {
                }
            });

            thread.Start();
            startEvent.Wait(Timeout.Infinite);

            tokenSource.Cancel();
            engine.Interrupt();
            thread.Join();

            Assert.AreEqual(1, context["count"]);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_InteropMethodCallWithInteropArg()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Automation", typeof(CUIAutomationClass));
            engine.AddHostType(typeof(UIA_PropertyIds));
            engine.AddHostType(typeof(UIA_ControlTypeIds));
            engine.AddHostType(typeof(IUIAutomationCondition));
            engine.Execute(@"
                    automation = new Automation();
                    condition1 = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId);
                    condition2 = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId);
                    condition3 = automation.CreatePropertyCondition(UIA_PropertyIds.UIA_ControlTypePropertyId, UIA_ControlTypeIds.UIA_CustomControlTypeId);
                    conditions = host.newArr(IUIAutomationCondition, 3);
                    conditions[0] = condition1;
                    conditions[1] = condition2;
                    conditions[2] = condition3;
                    andCondition = automation.CreateAndCondition(condition1, condition2);
                    andAndCondition = automation.CreateAndConditionFromArray(conditions);
                ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_InteropMethodCallWithInteropArg_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_InteropMethodCallWithInteropArg();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedType()
        {
            engine.AddHostType(typeof(NestedTypeTest));
            Assert.AreEqual(NestedTypeTest.NestedType.Foo, engine.Evaluate("NestedTypeTest.NestedType.Foo"));
            Assert.AreEqual(NestedTypeTest.NestedType.Bar, engine.Evaluate("NestedTypeTest.NestedType.Bar"));
            Assert.AreEqual(NestedTypeTest.NestedType.Baz, engine.Evaluate("NestedTypeTest.NestedType.Baz"));
            Assert.AreEqual(NestedTypeTest.NestedType.Qux, engine.Evaluate("NestedTypeTest.NestedType.Qux"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedType_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();

            engine.AddHostType(typeof(NestedTypeTest));
            Assert.AreEqual(NestedTypeTest.NestedType.Foo, engine.Evaluate("NestedTypeTest.NestedType.Foo"));
            Assert.AreEqual(NestedTypeTest.NestedType.Bar, engine.Evaluate("NestedTypeTest.NestedType.Bar"));
            Assert.AreEqual(NestedTypeTest.NestedType.Baz, engine.Evaluate("NestedTypeTest.NestedType.Baz"));
            Assert.AreEqual(NestedTypeTest.NestedType.Qux, engine.Evaluate("NestedTypeTest.NestedType.Qux"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedType_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostType(typeof(NestedTypeTest));
            Assert.AreEqual(NestedTypeTest.NestedType.Foo, engine.Evaluate("NestedTypeTest.NestedType.Foo"));
            Assert.AreEqual(NestedTypeTest.NestedType.Bar, engine.Evaluate("NestedTypeTest.NestedType.Bar"));
            Assert.AreEqual(NestedTypeTest.NestedType.Baz, engine.Evaluate("NestedTypeTest.NestedType.Baz"));
            Assert.AreEqual(NestedTypeTest.NestedType.Qux, engine.Evaluate("NestedTypeTest.NestedType.Qux"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NestedType_VBScript_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New VBScriptEngine
                    engine.AddHostType(GetType(BugFixTest.NestedTypeTest))
                    Assert.AreEqual(BugFixTest.NestedTypeTest.NestedType.Foo, engine.Evaluate(""NestedTypeTest.NestedType.Foo""))
                    Assert.AreEqual(BugFixTest.NestedTypeTest.NestedType.Bar, engine.Evaluate(""NestedTypeTest.NestedType.Bar""))
                    Assert.AreEqual(BugFixTest.NestedTypeTest.NestedType.Baz, engine.Evaluate(""NestedTypeTest.NestedType.Baz""))
                    Assert.AreEqual(BugFixTest.NestedTypeTest.NestedType.Qux, engine.Evaluate(""NestedTypeTest.NestedType.Qux""))
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8_Date_now()
        {
            var value = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var scriptValue = Convert.ToDouble(engine.Evaluate("Date.now()"));
            Assert.IsTrue(Math.Abs(scriptValue - value) < 25.0);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonEnumerablePropertyAccess()
        {
            engine.Script.dump = new Action<dynamic, string>((obj, value) =>
            {
                Assert.AreEqual(value, obj.message);
            });

            engine.Execute(@"
                message = 'hello';
                dump({ message: message }, message);
                message = 'world';
                dump(new Error(message), message);
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonEnumerablePropertyAccess_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();

            engine.Script.dump = new Action<dynamic, string>((obj, value) =>
            {
                Assert.AreEqual(value, obj.message);
            });

            engine.Execute(@"
                message = 'hello';
                dump({ message: message }, message);
                message = 'world';
                dump(new Error(message), message);
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonEnumerablePropertyAccess_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    engine.Script.dump = Sub(obj As Object, value As String)
                        Assert.AreEqual(value, obj.message)
                    End Sub
                    engine.Execute(
                        ""message = 'hello';"" & _
                        ""dump({ message: message }, message);"" & _
                        ""message = 'world';"" & _
                        ""dump(new Error(message), message);""
                    )
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonEnumerablePropertyAccess_JScript_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    engine.Script.dump = Sub(obj As Object, value As String)
                        Assert.AreEqual(value, obj.message)
                    End Sub
                    engine.Execute(
                        ""message = 'hello';"" & _
                        ""dump({ message: message }, message);"" & _
                        ""message = 'world';"" & _
                        ""dump(new Error(message), message);""
                    )
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonexistentPropertyAccess_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New V8ScriptEngine
                    engine.Script.dump = Sub(obj As Object, message As Object, stack As Object)
                        Assert.AreEqual(message, obj.message)
                        Assert.AreEqual(stack, obj.stack)
                    End Sub
                    engine.Execute(
                        ""message = 'hello';"" & _
                        ""stack = 'world';"" & _
                        ""dump({ message: message, stack: stack }, message, stack);"" & _
                        ""dump({ message: message }, message, undefined);""
                    )
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_NonexistentPropertyAccess_JScript_VB()
        {
            TestUtil.InvokeVBTestSub(@"
                Using engine As New JScriptEngine
                    engine.Script.dump = Sub(obj As Object, message As Object, stack As Object)
                        Assert.AreEqual(message, obj.message)
                        Assert.AreEqual(stack, obj.stack)
                    End Sub
                    engine.Execute(
                        ""message = 'hello';"" & _
                        ""stack = 'world';"" & _
                        ""dump({ message: message, stack: stack }, message, stack);"" & _
                        ""dump({ message: message }, message, undefined);""
                    )
                End Using
            ");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JsonDotNetSerialization()
        {
            var obj = engine.Evaluate("({foo:123,bar:'baz'})");
            Assert.AreEqual("{\"foo\":123,\"bar\":\"baz\"}", JsonConvert.SerializeObject(obj));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_RuntimeCompileException()
        {
            using (var runtime = new V8Runtime())
            {
                // ReSharper disable once AccessToDisposedClosure
                TestUtil.AssertException<ScriptEngineException>(() => runtime.Compile("function foo bar () {}"));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_PropertyAccessorScriptability()
        {
            engine.Script.testObject = new PropertyAccessorScriptability();
            engine.Execute("testObject.Foo = 123");
            Assert.AreEqual(123, engine.Evaluate("testObject.Foo"));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.Bar = 123"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("testObject.Bar"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_PropertyAccessorScriptability_Static()
        {
            engine.AddHostType("TestObject", typeof(PropertyAccessorScriptabilityStatic));
            engine.Execute("TestObject.Foo = 123");
            Assert.AreEqual(123, engine.Evaluate("TestObject.Foo"));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("TestObject.Bar = 123"));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate("TestObject.Bar"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_PublicOverrideInInternalClass()
        {
            engine.Script.foo = new PublicOverrideTest();
            Assert.AreEqual(789, engine.Evaluate("foo.AbstractMethod()"));
            Assert.AreEqual(456, engine.Evaluate("foo.VirtualMethod()"));
            Assert.AreEqual("baz", engine.Evaluate("foo.AbstractProperty"));
            Assert.AreEqual("bar", engine.Evaluate("foo.VirtualProperty"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_ImplicitConversionOperators()
        {
            engine.Script.doc = XDocument.Parse("<doc><foo>bar</foo><baz>qux</baz></doc>");
            engine.AddHostType(typeof(Enumerable));
            engine.AddHostType(typeof(XmlName));
            Assert.AreEqual(1, engine.Evaluate("doc.Root.Elements('foo').Count()"));
            Assert.AreEqual("<foo>bar</foo>", engine.Evaluate("doc.Root.Elements('foo').First().ToString()"));
            Assert.AreEqual(1, engine.Evaluate("doc.Root.Elements(new XmlName('foo')).Count()"));
            Assert.AreEqual("<foo>bar</foo>", engine.Evaluate("doc.Root.Elements(new XmlName('foo')).First().ToString()"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8StackLimitIntegerOverflow()
        {
            TestUtil.InvokeConsoleTest("BugFix_V8StackLimitIntegerOverflow");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_TextDigest()
        {
            const string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            var proxy = V8TestProxy.Create();
            for (var i = 0; i < 1024; i++)
            {
                var chars = new char[random.Next(8, 256)];
                for (var j = 0; j < chars.Length; j++)
                {
                    chars[j] = allChars[random.Next(allChars.Length - 1)];
                }

                var value = new string(chars);
                Assert.AreEqual(value.GetDigest(), proxy.GetNativeDigest(value));
            }
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

        [NoDefaultScriptAccess]
        public class PropertyAccessorScriptability
        {
            [ScriptMember]
            public int Foo { get; set; }

            [ScriptMember]
            public int Bar { [NoScriptAccess] get; [NoScriptAccess] set; }
        }

        [NoDefaultScriptAccess]
        public static class PropertyAccessorScriptabilityStatic
        {
            [ScriptMember]
            public static int Foo { get; set; }

            [ScriptMember]
            public static int Bar { [NoScriptAccess] get; [NoScriptAccess] set; }
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

        public class NumericArgConversionTest
        {
            public sbyte SByteField;
            public sbyte? NullableSByteField;
            public float FloatField;
            public float? NullableFloatField;
            public double DoubleField;
            public double? NullableDoubleField;
            public decimal DecimalField;
            public decimal? NullableDecimalField;

            public sbyte SByteMethod(sbyte arg) { return arg; }
            public sbyte? NullableSByteMethod(sbyte? arg) { return arg; }
            public float FloatMethod(float arg) { return arg; }
            public float? NullableFloatMethod(float? arg) { return arg; }
            public double DoubleMethod(double arg) { return arg; }
            public double? NullableDoubleMethod(double? arg) { return arg; }
            public decimal DecimalMethod(decimal arg) { return arg; }
            public decimal? NullableDecimalMethod(decimal? arg) { return arg; }

            public sbyte SByteProperty { get; set; }
            public sbyte? NullableSByteProperty { get; set; }
            public float FloatProperty { get; set; }
            public float? NullableFloatProperty { get; set; }
            public double DoubleProperty { get; set; }
            public double? NullableDoubleProperty { get; set; }
            public decimal DecimalProperty { get; set; }
            public decimal? NullableDecimalProperty { get; set; }
        }

        public class HideNativeJavaScriptMethod
        {
            // ReSharper disable InconsistentNaming

            public string toString()
            {
                return ToString();
            }

            // ReSharper restore InconsistentNaming
        }

        public class NestedTypeTest
        {
            public enum NestedType
            {
                Foo, Bar, Baz, Qux
            }
        }

        public abstract class PublicOverrideTestBase
        {
            public abstract int AbstractMethod();

            public virtual int VirtualMethod()
            {
                return 123;
            }

            public abstract string AbstractProperty { get; }

            public virtual string VirtualProperty
            {
                get { return "foo"; }
            }
        }

        internal class PublicOverrideTest : PublicOverrideTestBase
        {
            public override int AbstractMethod()
            {
                return 789;
            }

            public override int VirtualMethod()
            {
                return 456;
            }

            public override string AbstractProperty
            {
                get { return "baz"; }
            }

            public override string VirtualProperty
            {
                get { return "bar"; }
            }
        }

        public class XmlName
        {
            private readonly string name;

            public XmlName(string name)
            {
                this.name = name;
            }

            public static implicit operator XName(XmlName xmlName)
            {
                return xmlName.name;
            }
        }

        #endregion
    }
}
