// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UIAutomationClient;

namespace Microsoft.ClearScript.Test
{
    public partial class BugFixTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

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
        public void BugFix_FloatParameterBinding_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_FloatParameterBinding();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_UInt32RoundTrip_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_UInt32RoundTrip();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_SetProperty_JScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new JScriptEngine();

            WeakReference wr = null;

            new Action(() =>
            {
                object x = Guid.NewGuid();
                wr = new WeakReference(x);

                var result = x.ToString();
                engine.Script.x = x;

                x = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                Assert.AreEqual(result, engine.Evaluate("x.ToString()"));

                engine.Script.x = null;
                engine.CollectGarbage(true);
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_InvokeMethod_JScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new JScriptEngine();

            WeakReference wr1 = null;
            WeakReference wr2 = null;

            new Action(() =>
            {
                object x1 = Guid.NewGuid();
                wr1 = new WeakReference(x1);
                object x2 = Guid.NewGuid();
                wr2 = new WeakReference(x2);

                engine.Execute("function foo(x1, x2) { return x1.ToString() + x2.ToString(); }");
                Assert.AreEqual(x1.ToString() + x2, engine.Script.foo(x1, x2));

                engine.CollectGarbage(true);
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_TooManyDebugApplications()
        {
            var engines = Enumerable.Range(0, 2048).Select(_ => new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging)).ToArray();
            Array.ForEach(engines, tempEngine => tempEngine.Dispose());
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
        public void BugFix_PropertyBag_Iteration_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();

            var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
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
        public void BugFix_PropertyBag_Iteration_JScript_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                engine.Dispose();
                engine = new JScriptEngine();

                var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
                engine.Script.x = x;

                var result = (string)engine.Evaluate(@"
                    var result = '';
                    for (var e = new Enumerator(x); !e.atEnd(); e.moveNext()) {
                        result += e.item().value;
                    }
                    result
                ");

                Assert.AreEqual(7, result.Length);
                Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
                Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
            }
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

            var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
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

            var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
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
        public void BugFix_ScriptObjectInHostVariable_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_ScriptObjectInHostVariable();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_DoubleExecution();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DoubleExecution_Delegate_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_DoubleExecution_Delegate();
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
        public void BugFix_InteropMethodCallWithInteropArg_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            BugFix_InteropMethodCallWithInteropArg();
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_JScript_TargetInvocationException()
        {
            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.Script.foo = new Action(() => throw new Exception("bar"));
            engine.Execute("function test() { foo(); }");

            try
            {
                engine.Invoke("test");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("bar\n"));
                Assert.IsTrue(exception.ErrorDetails.Contains("(Script:0:18) -> foo"));
            }
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
        public void BugFix_IDispatchExArgLeak_SetProperty_VBScript()
        {
            // ReSharper disable RedundantAssignment

            engine.Dispose();
            engine = new VBScriptEngine();

            WeakReference wr = null;

            new Action(() =>
            {
                object x = Guid.NewGuid();
                wr = new WeakReference(x);

                var result = x.ToString();
                engine.Script.x = x;

                x = null;
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();

                Assert.AreEqual(result, engine.Evaluate("x.ToString()"));

                engine.Script.x = null;
                engine.CollectGarbage(true);
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr.IsAlive);

            // ReSharper restore RedundantAssignment
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_GetProperty_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            WeakReference wr1 = null;
            WeakReference wr2 = null;

            new Action(() =>
            {
                object x1 = Guid.NewGuid();
                wr1 = new WeakReference(x1);
                object x2 = Guid.NewGuid();
                wr2 = new WeakReference(x2);

                engine.Execute(@"
                    class MyClass
                        public property get foo(x1, x2)
                            foo = x1.ToString() & x2.ToString()
                        end property
                    end class
                    set bar = new MyClass
                ");

                var bar = (DynamicObject)engine.Script.bar;
                var args = new[] { "foo", HostItem.Wrap(engine, x1), HostItem.Wrap(engine, x2) };
                Assert.IsTrue(bar.GetMetaObject(Expression.Constant(bar)).TryGetIndex(args, out var result));
                Assert.AreEqual(x1.ToString() + x2, result);
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_IDispatchExArgLeak_InvokeMethod_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            WeakReference wr1 = null;
            WeakReference wr2 = null;

            new Action(() =>
            {
                object x1 = Guid.NewGuid();
                wr1 = new WeakReference(x1);
                object x2 = Guid.NewGuid();
                wr2 = new WeakReference(x2);

                engine.Execute("function foo(x1, x2):foo = x1.ToString() & x2.ToString():end function");
                Assert.AreEqual(x1.ToString() + x2, engine.Script.foo(x1, x2));

                engine.CollectGarbage(true);
            })();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            Assert.IsFalse(wr1.IsAlive);
            Assert.IsFalse(wr2.IsAlive);
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
        public void BugFix_PropertyBag_Iteration_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
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
        public void BugFix_PropertyBag_Iteration_VBScript_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                engine.Dispose();
                engine = new VBScriptEngine();

                var x = new PropertyBag { ["foo"] = 123, ["bar"] = "blah" };
                engine.Script.x = x;

                engine.Execute(@"
                    function getResult(arg)
                        dim result
                        result = """"
                        for each item in arg
                            result = result & item.value
                        next
                        getResult = result
                    end function
                ");

                var result = (string)engine.Evaluate("getResult(x)");

                Assert.AreEqual(7, result.Length);
                Assert.IsTrue(result.IndexOf("123", StringComparison.Ordinal) >= 0);
                Assert.IsTrue(result.IndexOf("blah", StringComparison.Ordinal) >= 0);
            }
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
        public void BugFix_WindowsScriptItem_SetPropertyLeak()
        {
            WeakReference wr = null;

            var proc = new Action(() =>
            {
                using (var vbs = new VBScriptEngine())
                {
                    wr = new WeakReference(vbs);

                    vbs.AddHostType(typeof(Console));
                    vbs.Script["foo"] = "bar";
                }
            });

            proc();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.IsFalse(wr.IsAlive);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AmbiguousIndexer_ADODB()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            var recordSet = new ADODB.Recordset();
            recordSet.Fields._Append("foo", ADODB.DataTypeEnum.adVarChar, 20);
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
        public void BugFix_VBScript_DefaultPropertyEmulation()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.AddHostObject("foo", new Variable { Value = 1, IsValid = true });
            engine.AddHostObject("bar", new Variable { Value = 2, IsValid = false });
            engine.Script.baz = new Variable { Value = 3, IsValid = false };

            engine.Execute(@"
                class CQux
                    private v
                    public sub Class_Initialize
                        v = 123
                    end sub
                    public default property get Value
                        Value = v
                    end property
                end class
                class CQuux
                end class
                set qux = new CQux
                set quux = new CQuux
            ");

            engine.Execute("foo = 456");
            Assert.AreEqual(456, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = bar");
            Assert.AreEqual(2, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = foo + 789");
            Assert.AreEqual(791, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = bar + baz");
            Assert.AreEqual(5, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = qux");
            Assert.AreEqual(123, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = bar + qux");
            Assert.AreEqual(125, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            engine.Execute("foo = bar + baz + qux");
            Assert.AreEqual(128, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));

            TestUtil.AssertException<ArgumentException>(() => engine.Execute("foo = quux"));
            Assert.AreEqual(128, engine.Evaluate("foo.Value"));
            Assert.AreEqual(true, engine.Evaluate("foo.IsValid"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.AddHostObject("foo", new Variable());
            engine.Execute("sub test\nfoo = \"xyz\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at test (Script:1:0) -> foo = \"xyz\""));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails_NoDebugger()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostObject("foo", new Variable());
            engine.Execute("sub test\nfoo = \"xyz\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at ([unknown]:1:0)"));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails_DirectAccess()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.AddHostObject("foo", HostItemFlags.DirectAccess, new Variable());
            engine.Execute("sub test\nfoo = \"xyz\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at test (Script:1:0) -> foo = \"xyz\""));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails_DirectAccess_NoDebugger()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostObject("foo", HostItemFlags.DirectAccess, new Variable());
            engine.Execute("sub test\nfoo = \"xyz\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at ([unknown]:1:0)"));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails_DirectAccess_ReadOnlyProperty()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.AddHostObject("foo", HostItemFlags.DirectAccess, new Variable());
            engine.Execute("sub test\nfoo.Description = \"bogus\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at test (Script:1:0) -> foo.Description = \"bogus\""));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_ErrorDetails_DirectAccess_ReadOnlyProperty_NoDebugger()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostObject("foo", HostItemFlags.DirectAccess, new Variable());
            engine.Execute("sub test\nfoo.Description = \"bogus\"\nend sub");

            try
            {
                engine.Invoke("test");
                Assert.Fail();
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("at ([unknown]:1:0)"));
            }
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_VBScript_TargetInvocationException()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);

            engine.Script.foo = new Action(() => throw new Exception("bar"));
            engine.Execute("sub test\nfoo\nend sub");

            try
            {
                engine.Invoke("test");
            }
            catch (ScriptEngineException exception)
            {
                Assert.IsTrue(exception.ErrorDetails.Contains("bar\n    at test (Script:1:0) -> foo"));
            }
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
        public void BugFix_V8ArrayBufferLeak()
        {
            TestUtil.InvokeConsoleTest("BugFix_V8ArrayBufferLeak");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DefaultArgs_Indexer_JScript()
        {
            engine.Dispose();
            engine = new JScriptEngine();
            engine.Script.test = new DefaultArgsTestObject();

            engine.Execute("test.Item.set(Math.PI)");
            Assert.AreEqual(Math.PI, engine.Evaluate("test.Item()"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.Item.get()"));
            engine.Execute("test.Item.set(456, Math.E)");
            Assert.AreEqual(Math.E, engine.Evaluate("test.Item(456)"));
            Assert.AreEqual(Math.E, engine.Evaluate("test.Item.get(456)"));
            engine.Execute("test.Item.set(789, 'bar', Math.PI * Math.E)");
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("test.Item(789, 'bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("test.Item.get(789, 'bar')"));

            engine.Execute("test.Item = Math.sqrt(Math.PI)");
            Assert.AreEqual(Math.Sqrt(Math.PI), engine.Evaluate("test.Item()"));
            Assert.AreEqual(Math.Sqrt(Math.PI), engine.Evaluate("test.Item.get()"));
            engine.Execute("test.Item(456) = Math.sqrt(Math.E)");
            Assert.AreEqual(Math.Sqrt(Math.E), engine.Evaluate("test.Item(456)"));
            Assert.AreEqual(Math.Sqrt(Math.E), engine.Evaluate("test.Item.get(456)"));
            engine.Execute("test.Item(789, 'bar') = Math.sqrt(Math.PI * Math.E)");
            Assert.AreEqual(Math.Sqrt(Math.PI * Math.E), engine.Evaluate("test.Item(789, 'bar')"));
            Assert.AreEqual(Math.Sqrt(Math.PI * Math.E), engine.Evaluate("test.Item.get(789, 'bar')"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_DefaultArgs_Indexer_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();
            engine.Script.test = new DefaultArgsTestObject();
            engine.Script.pi = Math.PI;
            engine.Script.e = Math.E;

            engine.Execute("test.Item.set pi");
            Assert.AreEqual(Math.PI, engine.Evaluate("test.Item()"));
            Assert.AreEqual(Math.PI, engine.Evaluate("test.Item.get()"));
            engine.Execute("test.Item.set 456, e");
            Assert.AreEqual(Math.E, engine.Evaluate("test.Item(456)"));
            Assert.AreEqual(Math.E, engine.Evaluate("test.Item.get(456)"));
            engine.Execute("test.Item.set 789, \"bar\", pi * e");
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("test.Item(789, \"bar\")"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("test.Item.get(789, \"bar\")"));

            engine.Execute("test.Item = sqr(pi)");
            Assert.AreEqual(Math.Sqrt(Math.PI), engine.Evaluate("test.Item()"));
            Assert.AreEqual(Math.Sqrt(Math.PI), engine.Evaluate("test.Item.get()"));
            engine.Execute("test.Item(456) = sqr(e)");
            Assert.AreEqual(Math.Sqrt(Math.E), engine.Evaluate("test.Item(456)"));
            Assert.AreEqual(Math.Sqrt(Math.E), engine.Evaluate("test.Item.get(456)"));
            engine.Execute("test.Item(789, \"bar\") = sqr(pi * e)");
            Assert.AreEqual(Math.Sqrt(Math.PI * Math.E), engine.Evaluate("test.Item(789, \"bar\")"));
            Assert.AreEqual(Math.Sqrt(Math.PI * Math.E), engine.Evaluate("test.Item.get(789, \"bar\")"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_MultidimensionalArray_VBScript()
        {
            engine.Dispose();
            engine = new VBScriptEngine();
            engine.AddHostObject("host", new HostFunctions());

            engine.Script.x = new int[2];
            engine.Script.y = new int[3, 4];

            Assert.AreEqual(0, engine.Evaluate("x(1)"));
            engine.Execute("x(1) = 123");
            Assert.AreEqual(123, engine.Evaluate("x(1)"));

            Assert.AreEqual(0, engine.Evaluate("y(2, 3)"));
            engine.Execute("y(2, 3) = 456");
            Assert.AreEqual(456, engine.Evaluate("y(2, 3)"));

            engine.Execute(@"
                x = host.newVar(x)
                x(1) = 0
                y = host.newVar(y)
                y(2, 3) = 0
            ");

            Assert.AreEqual(0, engine.Evaluate("x(1)"));
            engine.Execute("x(1) = 123");
            Assert.AreEqual(123, engine.Evaluate("x(1)"));

            Assert.AreEqual(0, engine.Evaluate("y(2, 3)"));
            engine.Execute("y(2, 3) = 456");
            Assert.AreEqual(456, engine.Evaluate("y(2, 3)"));
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_XMLDOM_Enumeration_JScript()
        {
            var document = new MSXML2.DOMDocument60Class();
            document.loadXML(@"
                <document>
                    <page id=""123""/>
                    <separator/>
                    <page id=""456""/>
                    <page id=""789""/>
                    <page id=""987""/>
                    <separator/>
                    <page id=""654""/>
                    <page id=""321""/>
                    <page id=""135""/>
                    <separator/>
                    <page id=""246""/>
                    <page id=""357""/>
                    <page id=""468""/>
                    <separator/>
                    <page id=""579""/>
                </document>
            ");

            engine.Dispose();
            engine = new JScriptEngine();

            engine.AddHostObject("document", document);
            engine.Execute(@"
                allPages = document.getElementsByTagName('page');
                count = 0;
                for (var e = new Enumerator(allPages); !e.atEnd(); e.moveNext()) {
                    ++count;
                }
            ");

            Assert.AreEqual(11, engine.Script.count);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_XMLDOM_Enumeration_VBScript()
        {
            var document = new MSXML2.DOMDocument60Class();
            document.loadXML(@"
                <document>
                    <page id=""123""/>
                    <separator/>
                    <page id=""456""/>
                    <page id=""789""/>
                    <page id=""987""/>
                    <separator/>
                    <page id=""654""/>
                    <page id=""321""/>
                    <page id=""135""/>
                    <separator/>
                    <page id=""246""/>
                    <page id=""357""/>
                    <page id=""468""/>
                    <separator/>
                    <page id=""579""/>
                </document>
            ");

            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostObject("document", document);
            engine.Execute(@"
                set allPages = document.getElementsByTagName(""page"")
                count = 0
                for each page in allPages
                    count = count + 1
                next
            ");

            Assert.AreEqual(11, engine.Script.count);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_SparseArgumentBinding()
        {
            engine.Dispose();
            engine = new VBScriptEngine();

            engine.Script.test = new SparseArgumentTest();
            engine.UseReflectionBindFallback = true;

            Assert.AreEqual("'foo' 'bar' 789 456", engine.Evaluate("test.go(\"foo\", \"bar\", 789)"));
            Assert.AreEqual("'foo' 'bar' 123 456", engine.Evaluate("test.go(\"foo\", \"bar\")"));
            Assert.AreEqual("'foo' 'xyz' 789 456", engine.Evaluate("test.go(\"foo\", , 789)"));
            Assert.AreEqual("'foo' 'xyz' 123 789", engine.Evaluate("test.go(\"foo\", , , 789)"));
            Assert.AreEqual("'foo' 'xyz' 123 456", engine.Evaluate("test.go(\"foo\")"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        public sealed class SparseArgumentTest
        {
            public string Go(string first, string second = "xyz", int third = 123, int fourth = 456)
            {
                return $"'{first}' '{second}' {third} {fourth}";
            }
        }

        private static void VariantClearTestHelper(object x)
        {
            using (var engine = new JScriptEngine())
            {
                engine.AddHostObject("x", x);
                engine.Evaluate("x");
            }
        }

        private sealed class CamelCaseAttributeLoader : CustomAttributeLoader
        {
            public override T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
            {
                if (typeof(T) == typeof(ScriptMemberAttribute) && (resource is MemberInfo member))
                {
                    var name = char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1);
                    return new[] { new ScriptMemberAttribute(name) } as T[];
                }

                return base.LoadCustomAttributes<T>(resource, inherit);
            }
        }

        #endregion
    }
}
