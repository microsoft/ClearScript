// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.V8;
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

                using (var scriptEngine = new V8ScriptEngine())
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

                using (var scriptEngine = new V8ScriptEngine())
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
        // ReSharper restore InconsistentNaming

        #endregion
    }
}
