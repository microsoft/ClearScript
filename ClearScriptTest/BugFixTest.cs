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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
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
            // A V8 fatal error on a background thread may not kill the process, so a single run is
            // inconclusive. It will kill the V8 runtime, however, causing subsequent runs to fail.

            for (var iteration = 0; iteration < 16; iteration++)
            {
                var context = new PropertyBag();
                engine.AddHostObject("context", context);

                var startEvent = new ManualResetEventSlim(false);
                var thread = new Thread(() =>
                {
                    context["startEvent"] = startEvent;
                    context["counter"] = 0;

                    try
                    {
                        engine.Execute(
                        @"
                            for (var i = 0; i < 10000000; i++ )
                            {
                                context.counter++;
                                context.startEvent.Set();
                            }
                        ");
                    }
                    catch (ScriptInterruptedException)
                    {
                    }
                });

                thread.Start();
                startEvent.Wait();
                engine.Interrupt();
                thread.Join();

                var counter = (int)context["counter"];
                Assert.IsTrue((counter > 0) && (counter < 10000000));
            }
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
