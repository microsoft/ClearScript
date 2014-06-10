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
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class VBScriptEngineTest : ClearScriptTest
    {
        #region setup / teardown

        private VBScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
            engine.Execute("function pi : pi = 4 * atn(1) : end function");
            engine.Execute("function e : e = exp(1) : end function");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostObject()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", host);
            Assert.AreSame(host, engine.Evaluate("host"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VBScriptEngine_AddHostObject_Scalar()
        {
            engine.AddHostObject("value", 123);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostObject_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.AddHostObject("value", value);
            Assert.AreEqual(value, engine.Evaluate("value"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostObject_Struct()
        {
            var date = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.AddHostObject("date", date);
            Assert.AreEqual(date, engine.Evaluate("date"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostObject_GlobalMembers()
        {
            var host = new HostFunctions();
            engine.AddHostObject("host", HostItemFlags.GlobalMembers, host);
            Assert.IsInstanceOfType(engine.Evaluate("newObj()"), typeof(PropertyBag));

            engine.AddHostObject("test", HostItemFlags.GlobalMembers, this);
            engine.Execute("TestProperty = newObj()");
            Assert.IsInstanceOfType(TestProperty, typeof(PropertyBag));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void VBScriptEngine_AddHostObject_DefaultAccess()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostObject_PrivateAccess()
        {
            engine.AddHostObject("test", HostItemFlags.PrivateAccess, this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddRestrictedHostObject_BaseClass()
        {
            var host = new ExtendedHostFunctions() as HostFunctions;
            engine.AddRestrictedHostObject("host", host);
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj()"), typeof(PropertyBag));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("host.type(\"System.Int32\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddRestrictedHostObject_Interface()
        {
            const double value = 123.45;
            engine.AddRestrictedHostObject("convertible", value as IConvertible);
            engine.AddHostObject("culture", CultureInfo.InvariantCulture);
            Assert.AreEqual(value, engine.Evaluate("convertible.ToDouble(culture)"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", typeof(Random));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_GlobalMembers()
        {
            engine.AddHostType("Guid", HostItemFlags.GlobalMembers, typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("NewGuid()"), typeof(Guid));

            engine.AddHostType("Test", HostItemFlags.GlobalMembers, GetType());
            engine.Execute("StaticTestProperty = NewGuid()");
            Assert.IsInstanceOfType(StaticTestProperty, typeof(Guid));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void VBScriptEngine_AddHostType_DefaultAccess()
        {
            engine.AddHostType("Test", GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_PrivateAccess()
        {
            engine.AddHostType("Test", HostItemFlags.PrivateAccess, GetType());
            engine.Execute("Test.PrivateStaticMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_Static()
        {
            engine.AddHostType("Enumerable", typeof(Enumerable));
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_OpenGeneric()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("List", typeof(List<>));
            engine.AddHostType("Guid", typeof(Guid));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(List(Guid))"), typeof(List<Guid>));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_ByName()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Random", "System.Random");
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Random)"), typeof(Random));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_ByNameWithAssembly()
        {
            engine.AddHostType("Enumerable", "System.Linq.Enumerable", "System.Core");
            Assert.IsInstanceOfType(engine.Evaluate("Enumerable.Range(0, 5).ToArray()"), typeof(int[]));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AddHostType_ByNameWithTypeArgs()
        {
            engine.AddHostObject("host", new HostFunctions());
            engine.AddHostType("Dictionary", "System.Collections.Generic.Dictionary", typeof(string), typeof(int));
            Assert.IsInstanceOfType(engine.Evaluate("host.newObj(Dictionary)"), typeof(Dictionary<string, int>));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Evaluate()
        {
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("e * pi"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Evaluate_Array()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var lengths = new[] { 3, 5, 7 };
            var formatParams = string.Join(", ", Enumerable.Range(0, lengths.Length).Select(position => "{" + position + "}"));

            var hosts = Array.CreateInstance(typeof(object), lengths);
            hosts.Iterate(indices => hosts.SetValue(new HostFunctions(), indices));
            engine.AddHostObject("hostArray", hosts);

            engine.Execute(MiscHelpers.FormatInvariant("dim hosts(" + formatParams + ")", lengths.Select(length => (object)(length - 1)).ToArray()));
            hosts.Iterate(indices => engine.Execute(MiscHelpers.FormatInvariant("set hosts(" + formatParams + ") = hostArray.GetValue(" + formatParams + ")", indices.Select(index => (object)index).ToArray())));
            hosts.Iterate(indices => Assert.AreSame(hosts.GetValue(indices), engine.Evaluate(MiscHelpers.FormatInvariant("hosts(" + formatParams + ")", indices.Select(index => (object)index).ToArray()))));

            var result = engine.Evaluate("hosts");
            Assert.IsInstanceOfType(result, typeof(object[,,]));
            var hostArray = (object[,,])result;
            hosts.Iterate(indices => Assert.AreSame(hosts.GetValue(indices), hostArray.GetValue(indices)));

            // ReSharper restore ImplicitlyCapturedClosure
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Evaluate_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, "e * pi"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Evaluate_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, true, "e * pi"));
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Evaluate_RetainDocument()
        {
            const string documentName = "DoTheMath";
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(documentName, false, "e * pi"));
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Execute()
        {
            engine.Execute("epi = e * pi");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Execute_WithDocumentName()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, "epi = e * pi");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Execute_DiscardDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, true, "epi = e * pi");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsFalse(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Execute_RetainDocument()
        {
            const string documentName = "DoTheMath";
            engine.Execute(documentName, false, "epi = e * pi");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.epi);
            Assert.IsTrue(engine.GetDebugDocumentNames().Any(name => name.StartsWith(documentName, StringComparison.Ordinal)));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ExecuteCommand_EngineConvert()
        {
            Assert.AreEqual("[ScriptObject:EngineInternalImpl]", engine.ExecuteCommand("eval EngineInternal"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ExecuteCommand_HostConvert()
        {
            var dateHostItem = HostItem.Wrap(engine, new DateTime(2007, 5, 22, 6, 15, 43));
            engine.AddHostObject("date", dateHostItem);
            Assert.AreEqual(dateHostItem.ToString(), engine.ExecuteCommand("eval date"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ExecuteCommand_HostVariable()
        {
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("[HostVariable:String]", engine.ExecuteCommand("eval host.newVar(\"foo\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Interrupt()
        {
            var checkpoint = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(state =>
            {
                checkpoint.WaitOne();
                engine.Interrupt();
            });

            engine.AddHostObject("checkpoint", checkpoint);
            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("call checkpoint.Set() : while true : foo = \"hello\" : wend"));
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("e * pi"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        [ExpectedException(typeof(ScriptEngineException))]
        public void VBScriptEngine_AccessContext_Default()
        {
            engine.AddHostObject("test", this);
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_AccessContext_Private()
        {
            engine.AddHostObject("test", this);
            engine.AccessContext = GetType();
            engine.Execute("test.PrivateMethod()");
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ContinuationCallback()
        {
            engine.ContinuationCallback = () => false;
            TestUtil.AssertException<OperationCanceledException>(() => engine.Execute("while true : foo = \"hello\" : wend"));
            engine.ContinuationCallback = null;
            Assert.AreEqual(Math.E * Math.PI, engine.Evaluate("e * pi"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_FileNameExtension()
        {
            Assert.AreEqual("vbs", engine.FileNameExtension);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Variable()
        {
            var host = new HostFunctions();
            engine.Script.host = host;
            Assert.AreSame(host, engine.Script.host);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Variable_Scalar()
        {
            const int value = 123;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Variable_Enum()
        {
            const DayOfWeek value = DayOfWeek.Wednesday;
            engine.Script.value = value;
            Assert.AreEqual(value, engine.Script.value);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Array()
        {
            // ReSharper disable ImplicitlyCapturedClosure

            var lengths = new[] { 3, 5, 7 };
            var formatParams = string.Join(", ", Enumerable.Range(0, lengths.Length).Select(position => "{" + position + "}"));

            var hosts = Array.CreateInstance(typeof(object), lengths);
            hosts.Iterate(indices => hosts.SetValue(new HostFunctions(), indices));
            engine.Script.hostArray = hosts;

            engine.Execute(MiscHelpers.FormatInvariant("dim hosts(" + formatParams + ")", lengths.Select(length => (object)(length - 1)).ToArray()));
            hosts.Iterate(indices => engine.Execute(MiscHelpers.FormatInvariant("set hosts(" + formatParams + ") = hostArray.GetValue(" + formatParams + ")", indices.Select(index => (object)index).ToArray())));
            hosts.Iterate(indices => Assert.AreSame(hosts.GetValue(indices), engine.Script.hosts.GetValue(indices)));

            var result = engine.Script.hosts;
            Assert.IsInstanceOfType(result, typeof(object[,,]));
            var hostArray = (object[,,])result;
            hosts.Iterate(indices => Assert.AreSame(hosts.GetValue(indices), hostArray.GetValue(indices)));

            // ReSharper restore ImplicitlyCapturedClosure
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Variable_Struct()
        {
            var stamp = new DateTime(2007, 5, 22, 6, 15, 43);
            engine.Script.stamp = stamp;
            Assert.AreEqual(stamp, engine.Script.stamp);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Function()
        {
            engine.Execute("function test(x, y) : test = x * y : end function");
            Assert.AreEqual(Math.E * Math.PI, engine.Script.test(Math.E, Math.PI));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_Script_Sub()
        {
            var callbackInvoked = false;
            Action callback = () => callbackInvoked = true;
            engine.Execute("sub test(x) : call x() : end sub");
            engine.Script.test(callback);
            Assert.IsTrue(callbackInvoked);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_CollectGarbage()
        {
            // VBScript doesn't support GC
            engine.CollectGarbage(true);
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_General()
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

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ErrorHandling_ScriptError()
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

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ErrorHandling_HostException()
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

                    Assert.AreEqual(hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ErrorHandling_IgnoredHostException()
        {
            engine.AddHostObject("host", new HostFunctions());

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("on error resume next : host.newObj(null) : on error goto 0 : foo = \"foo\" : foo()");
                }
                catch (ScriptEngineException exception)
                {
                    TestUtil.AssertValidException(engine, exception);
                    Assert.IsNull(exception.InnerException);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ErrorHandling_NestedScriptError()
        {
            var innerEngine = new JScriptEngine("inner", WindowsScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("engine", innerEngine);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("engine.Execute(\"foo = {}; foo();\")");
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

                    Assert.AreEqual(hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_ErrorHandling_NestedHostException()
        {
            var innerEngine = new JScriptEngine("inner", WindowsScriptEngineFlags.EnableDebugging);
            innerEngine.AddHostObject("host", new HostFunctions());
            engine.AddHostObject("engine", innerEngine);

            TestUtil.AssertException<ScriptEngineException>(() =>
            {
                try
                {
                    engine.Execute("engine.Evaluate(\"host.newObj(0)\")");
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

                    Assert.AreEqual(nestedHostException.Message, nestedException.Message);
                    Assert.AreEqual(hostException.Message, exception.Message);
                    throw;
                }
            });
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_CreateInstance()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("foo bar baz qux", engine.Evaluate("host.newObj(testObject, \"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_CreateInstance_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("host.newObj(testObject)"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Invoke_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("testObject()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo-bar-baz-qux", engine.Evaluate("testObject.DynamicMethod(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.DynamicMethod()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo.bar.baz.qux", engine.Evaluate("testObject.SomeField(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_FieldOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeField()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo:bar:baz:qux", engine.Evaluate("testObject.SomeProperty(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_PropertyOverride_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            TestUtil.AssertException<MissingMemberException>(() => engine.Evaluate("testObject.SomeProperty()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_DynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("foo;bar;baz;qux", engine.Evaluate("testObject.SomeMethod(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_NonDynamicOverload()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.SomeMethod()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_InvokeMethod_NonDynamic()
        {
            engine.Script.testObject = new DynamicTestObject();
            Assert.AreEqual("Super Bass-O-Matic '76", engine.Evaluate("testObject.ToString()"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_StaticType_Field()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeField"), typeof(HostMethod));
            Assert.AreEqual(12345, engine.Evaluate("host.toStaticType(testObject).SomeField"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_StaticType_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("testObject.SomeProperty"), typeof(HostMethod));
            Assert.AreEqual("Bogus", engine.Evaluate("host.toStaticType(testObject).SomeProperty"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_StaticType_Method()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.AreEqual("bar+baz+qux", engine.Evaluate("host.toStaticType(testObject).SomeMethod(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Property()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getProperty(testObject, \"foo\")"), typeof(Undefined));
            Assert.AreEqual(123, engine.Evaluate("host.setProperty(testObject, \"foo\", clng(123))"));
            Assert.AreEqual(123, engine.Evaluate("testObject.foo"));
            Assert.IsTrue((bool)engine.Evaluate("host.removeProperty(testObject, \"foo\")"));
            Assert.IsInstanceOfType(engine.Evaluate("host.getProperty(testObject, \"foo\")"), typeof(Undefined));
            Assert.IsFalse((bool)engine.Evaluate("host.removeProperty(testObject, \"foo\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Property_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getProperty(testObject, \"Zfoo\")"), typeof(Undefined));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("host.setProperty(testObject, \"Zfoo\", clng(123))"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Property_Invoke()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            engine.Execute("function secret(x) : secret = len(x) : end function");
            Assert.IsInstanceOfType(engine.Evaluate("host.getProperty(testObject, \"foo\")"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.setProperty(testObject, \"foo\", GetRef(\"secret\"))"), typeof(DynamicObject));
            Assert.AreEqual("floccinaucinihilipilification".Length, engine.Evaluate("testObject.foo(\"floccinaucinihilipilification\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Property_Invoke_Nested()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getProperty(testObject, \"foo\")"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("host.setProperty(testObject, \"foo\", testObject)"), typeof(DynamicObject));
            Assert.AreEqual("foo,bar,baz,qux", engine.Evaluate("testObject.foo(\"foo\", \"bar\", \"baz\", \"qux\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Element()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, clng(1), clng(2), clng(3), \"foo\")"), typeof(Undefined));
            Assert.AreEqual("bar", engine.Evaluate("host.setElement(testObject, \"bar\", clng(1), clng(2), clng(3), \"foo\")"));
            Assert.AreEqual("bar", engine.Evaluate("host.getElement(testObject, clng(1), clng(2), clng(3), \"foo\")"));
            Assert.IsTrue((bool)engine.Evaluate("host.removeElement(testObject, clng(1), clng(2), clng(3), \"foo\")"));
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, clng(1), clng(2), clng(3), \"foo\")"), typeof(Undefined));
            Assert.IsFalse((bool)engine.Evaluate("host.removeElement(testObject, clng(1), clng(2), clng(3), \"foo\")"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Element_Fail()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            Assert.IsInstanceOfType(engine.Evaluate("host.getElement(testObject, clng(1), clng(2), clng(3), pi)"), typeof(Undefined));
            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("host.setElement(testObject, \"bar\", clng(1), clng(2), clng(3), pi)"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_DynamicHostObject_Element_Convert()
        {
            engine.Script.testObject = new DynamicTestObject();
            engine.Script.host = new HostFunctions();
            engine.AddHostType("int_t", typeof(int));
            engine.AddHostType("string_t", typeof(string));
            Assert.AreEqual(98765, engine.Evaluate("host.cast(int_t, testObject)"));
            Assert.AreEqual("Booyakasha!", engine.Evaluate("host.cast(string_t, testObject)"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_HostIndexers()
        {
            engine.Script.testObject = new TestObject();

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(clng(123))"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(clng(123))"));
            engine.Execute("testObject.Item(clng(123)) = pi");
            Assert.AreEqual(Math.PI, engine.Evaluate("testObject.Item(clng(123))"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.set(clng(123), e)"));
            Assert.AreEqual(Math.E, engine.Evaluate("testObject.Item.get(clng(123))"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(\"456\")"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(\"456\")"));
            engine.Execute("testObject.Item(\"456\") = sqr(2)");
            Assert.AreEqual(Math.Sqrt(2), engine.Evaluate("testObject.Item(\"456\")"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.set(\"456\", sqr(3))"));
            Assert.AreEqual(Math.Sqrt(3), engine.Evaluate("testObject.Item.get(\"456\")"));

            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item(clng(123), \"456\", 789.987, -0.12345)"));
            TestUtil.AssertException<KeyNotFoundException>(() => engine.Evaluate("testObject.Item.get(clng(123), \"456\", 789.987, -0.12345)"));
            engine.Execute("testObject.Item(clng(123), \"456\", 789.987, -0.12345) = sqr(5)");
            Assert.AreEqual(Math.Sqrt(5), engine.Evaluate("testObject.Item(clng(123), \"456\", 789.987, -0.12345)"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.set(clng(123), \"456\", 789.987, -0.12345, sqr(7))"));
            Assert.AreEqual(Math.Sqrt(7), engine.Evaluate("testObject.Item.get(clng(123), \"456\", 789.987, -0.12345)"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_FormatCode()
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

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_GetStackTrace()
        {
            engine.AddHostObject("qux", new Func<object>(() => engine.GetStackTrace()));
            engine.Execute(@"
                function baz():baz = qux():end function
                function bar():bar = baz():end function
                function foo():foo = bar():end function
            ");

            Assert.AreEqual("    at baz (Script Document [3]:1:31) -> baz = qux()\n    at bar (Script Document [3]:2:31) -> bar = baz()\n    at foo (Script Document [3]:3:31) -> foo = bar()\n    at VBScript global code (Script Document [4] [temp]:0:0) -> foo()", engine.Evaluate("foo()"));
            Assert.AreEqual("    at baz (Script Document [3]:1:31) -> baz = qux()\n    at bar (Script Document [3]:2:31) -> bar = baz()\n    at foo (Script Document [3]:3:31) -> foo = bar()", engine.Script.foo());
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_StandardsMode()
        {
            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode);

            // Standards Mode shouldn't affect VBScriptEngine at all
            engine.Execute("function pi : pi = 4 * atn(1) : end function");
            engine.Execute("function e : e = exp(1) : end function");
            VBScriptEngine_Execute();
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_MarshalNullAsDispatch()
        {
            engine.Script.func = new Func<object>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));

            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalNullAsDispatch);

            engine.Script.func = new Func<object>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<string>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<bool?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<char?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<sbyte?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<byte?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<short?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<ushort?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<int?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<uint?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<long?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<ulong?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<float?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<double?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<decimal?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("IsNull(func())"));
            engine.Script.func = new Func<Random>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() is nothing"));
            engine.Script.func = new Func<DayOfWeek?>(() => null);
            Assert.IsTrue((bool)engine.Evaluate("func() is nothing"));
        }

        [TestMethod, TestCategory("VBScriptEngine")]
        public void VBScriptEngine_MarshalDecimalAsCurrency()
        {
            // ReSharper disable AccessToDisposedClosure

            engine.Script.func = new Func<object>(() => 123.456M);
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("TypeName(func())"));

            engine.Dispose();
            engine = new VBScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.MarshalDecimalAsCurrency);

            engine.Script.func = new Func<object>(() => 123.456M);
            Assert.AreEqual("Currency", engine.Evaluate("TypeName(func())"));
            Assert.AreEqual(123.456M + 5, engine.Evaluate("func() + 5"));

            // ReSharper restore AccessToDisposedClosure
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private const string generalScript =
        @"
            set System = clr.System

            set TestObject = host.type(""Microsoft.ClearScript.Test.GeneralTestObject"", ""ClearScriptTest"")
            set tlist = host.newObj(System.Collections.Generic.List(TestObject))
            call tlist.Add(host.newObj(TestObject, ""Eóin"", 20))
            call tlist.Add(host.newObj(TestObject, ""Shane"", 16))
            call tlist.Add(host.newObj(TestObject, ""Cillian"", 8))
            call tlist.Add(host.newObj(TestObject, ""Sasha"", 6))
            call tlist.Add(host.newObj(TestObject, ""Brian"", 3))

            class VBTestObject
               public name
               public age
            end class

            function createTestObject(name, age)
               dim testObject
               set testObject = new VBTestObject
               testObject.name = name
               testObject.age = age
               set createTestObject = testObject
            end function

            set olist = host.newObj(System.Collections.Generic.List(System.Object))
            call olist.Add(createTestObject(""Brian"", 3))
            call olist.Add(createTestObject(""Sasha"", 6))
            call olist.Add(createTestObject(""Cillian"", 8))
            call olist.Add(createTestObject(""Shane"", 16))
            call olist.Add(createTestObject(""Eóin"", 20))

            set dict = host.newObj(System.Collections.Generic.Dictionary(System.String, System.String))
            call dict.Add(""foo"", ""bar"")
            call dict.Add(""baz"", ""qux"")
            set value = host.newVar(System.String)
            result = dict.TryGetValue(""foo"", value.out)

            set expando = host.newObj(System.Dynamic.ExpandoObject)
            set expandoCollection = host.cast(System.Collections.Generic.ICollection(System.Collections.Generic.KeyValuePair(System.String, System.Object)), expando)

            set onEventRef = GetRef(""onEvent"")
            sub onEvent(s, e)
                call System.Console.WriteLine(""Property changed: {0}; new value: {1}"", e.PropertyName, eval(""s."" + e.PropertyName))
            end sub

            set onStaticEventRef = GetRef(""onStaticEvent"")
            sub onStaticEvent(s, e)
                call System.Console.WriteLine(""Property changed: {0}; new value: {1} (static event)"", e.PropertyName, e.PropertyValue)
            end sub

            set eventCookie = tlist.Item(0).Change.connect(onEventRef)
            set staticEventCookie = TestObject.StaticChange.connect(onStaticEventRef)
            tlist.Item(0).Name = ""Jerry""
            tlist.Item(1).Name = ""Ellis""
            tlist.Item(0).Name = ""Eóin""
            tlist.Item(1).Name = ""Shane""

            call eventCookie.disconnect()
            call staticEventCookie.disconnect()
            tlist.Item(0).Name = ""Jerry""
            tlist.Item(1).Name = ""Ellis""
            tlist.Item(0).Name = ""Eóin""
            tlist.Item(1).Name = ""Shane""
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
