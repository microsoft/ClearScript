// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Threading;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class PropertyBagTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_MultiEngine()
        {
            var bag = new PropertyBag();
            engine.AddHostObject("bag", bag);

            Action innerTest = () =>
            {
                // The Visual Studio 2013 debugging stack fails to release the script engine
                // properly, resulting in test failure. Visual Studio 2012 does not have this bug.

                using (var scriptEngine = new VBScriptEngine())
                {
                    scriptEngine.AddHostObject("bag", bag);
                    Assert.AreEqual(2, bag.EngineCount);
                }
            };

            innerTest();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Assert.AreEqual(1, bag.EngineCount);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_MultiEngine_Property()
        {
            var outerBag = new PropertyBag();
            engine.AddHostObject("bag", outerBag);

            var innerBag = new PropertyBag();
            Action innerTest = () =>
            {
                // The Visual Studio 2013 debugging stack fails to release the script engine
                // properly, resulting in test failure. Visual Studio 2012 does not have this bug.

                using (var scriptEngine = new VBScriptEngine())
                {
                    scriptEngine.AddHostObject("bag", outerBag);
                    outerBag.Add("innerBag", innerBag);
                    Assert.AreEqual(2, innerBag.EngineCount);
                }
            };

            innerTest();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Assert.AreEqual(1, innerBag.EngineCount);
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_MultiEngine_HostFunctions()
        {
            using (var scriptEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
            {
                const string code = "bag.host.func(0, function () { return bag.func(); })";
                var bag = new PropertyBag
                {
                    { "host", new HostFunctions() },
                    { "func", new Func<object>(() => ScriptEngine.Current) }
                };

                engine.AddHostObject("bag", bag);
                scriptEngine.AddHostObject("bag", bag);

                var func = (Func<object>)engine.Evaluate(code);
                Assert.AreSame(engine, func());

                func = (Func<object>)scriptEngine.Evaluate(code);
                Assert.AreSame(scriptEngine, func());
            }
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_MultiEngine_Parallel()
        {
            // This is a torture test for ConcurrentWeakSet and general engine teardown/cleanup.
            // It has exposed some very tricky engine bugs.

            var bag = new PropertyBag();
            engine.AddHostObject("bag", bag);

            var threadCount = Environment.Is64BitProcess ? 1024 : 256;
            var engineCount = 0;

            // 32-bit V8 starts failing requests to create new contexts rather quickly. This is
            // because each V8 isolate requires (among other things) a 32MB address space
            // reservation. 64-bit V8 reserves much larger blocks but benefits from the enormous
            // available address space.

            var maxV8Count = Environment.Is64BitProcess ? 256 : 16;
            var maxJScriptCount = (threadCount - maxV8Count) / 2;

            var startEvent = new ManualResetEventSlim(false);
            var checkpointEvent = new ManualResetEventSlim(false);
            var continueEvent = new ManualResetEventSlim(false);
            var stopEvent = new ManualResetEventSlim(false);

            ParameterizedThreadStart body = arg =>
            {
                // ReSharper disable AccessToDisposedClosure

                var index = (int)arg;
                startEvent.Wait();

                ScriptEngine scriptEngine;
                if (index < maxV8Count)
                {
                    scriptEngine = new V8ScriptEngine();
                }
                else if (index < (maxV8Count + maxJScriptCount))
                {
                    scriptEngine = new JScriptEngine();
                }
                else
                {
                    scriptEngine = new VBScriptEngine();
                }

                scriptEngine.AddHostObject("bag", bag);
                if (Interlocked.Increment(ref engineCount) == threadCount)
                {
                    checkpointEvent.Set();
                }

                continueEvent.Wait();

                scriptEngine.Dispose();
                if (Interlocked.Decrement(ref engineCount) == 0)
                {
                    stopEvent.Set();
                }

                // ReSharper restore AccessToDisposedClosure
            };

            var threads = Enumerable.Range(0, threadCount).Select(index => new Thread(body)).ToArray();
            threads.ForEach((thread, index) => thread.Start(index));

            startEvent.Set();
            checkpointEvent.Wait();
            Assert.AreEqual(threadCount + 1, bag.EngineCount);

            continueEvent.Set();
            stopEvent.Wait();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Assert.AreEqual(1, bag.EngineCount);

            Array.ForEach(threads, thread => thread.Join());
            startEvent.Dispose();
            checkpointEvent.Dispose();
            continueEvent.Dispose();
            stopEvent.Dispose();
        }

        [TestMethod, TestCategory("PropertyBag")]
        public void PropertyBag_Concurrent_JScript()
        {
            var bag = new ConcurrentPropertyBag();
            engine.AddHostObject("bag", bag);

            var threadCount = Environment.Is64BitProcess ? 512 : 16;
            var engineCount = 0;

            var startEvent = new ManualResetEventSlim(false);
            var checkpointEvent = new ManualResetEventSlim(false);
            var continueEvent = new ManualResetEventSlim(false);
            var stopEvent = new ManualResetEventSlim(false);

            ParameterizedThreadStart body = arg =>
            {
                // ReSharper disable AccessToDisposedClosure

                var index = (int)arg;
                startEvent.Wait();

                var scriptEngine = new Windows.Core.JScriptEngine(Windows.Core.NullSyncInvoker.Instance);

                scriptEngine.AddHostObject("bag", bag);
                scriptEngine.Global["index"] = index;

                scriptEngine.Execute("bag['foo' + index] = index");
                Assert.AreEqual(index, scriptEngine.Evaluate("bag['foo' + index]"));

                if (Interlocked.Increment(ref engineCount) == threadCount)
                {
                    checkpointEvent.Set();
                }

                continueEvent.Wait();

                scriptEngine.Dispose();
                if (Interlocked.Decrement(ref engineCount) == 0)
                {
                    stopEvent.Set();
                }

                // ReSharper restore AccessToDisposedClosure
            };

            var threads = Enumerable.Range(0, threadCount).Select(index => new Thread(body)).ToArray();
            threads.ForEach((thread, index) => thread.Start(index));

            startEvent.Set();
            checkpointEvent.Wait();
            Assert.AreEqual(threadCount + 1, bag.EngineCount);

            continueEvent.Set();
            stopEvent.Wait();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Assert.AreEqual(1, bag.EngineCount);

            Array.ForEach(threads, thread => thread.Join());
            startEvent.Dispose();
            checkpointEvent.Dispose();
            continueEvent.Dispose();
            stopEvent.Dispose();
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
