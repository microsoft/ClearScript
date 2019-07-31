// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.ClearScript.Util;
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
    [DeploymentItem("v8-base-x64.dll")]
    [DeploymentItem("v8-base-ia32.dll")]
    [DeploymentItem("v8-libcpp-x64.dll")]
    [DeploymentItem("v8-libcpp-ia32.dll")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class CrossEngineTest : ClearScriptTest
    {
        #region setup / teardown

        private static readonly Type[] types =
        {
            typeof(V8ScriptEngine),
            typeof(JScriptEngine),
            typeof(VBScriptEngine)
        };

        private ScriptEngine[] engines;

        [TestInitialize]
        public void TestInitialize()
        {
            engines = types.Select(type => (ScriptEngine)type.CreateInstance()).ToArray();
            Iterate((engine, type) => engine.AddHostObject(type.Name, type.CreateInstance()));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Iterate((engine, type) => engine.Execute(type.Name + ".Dispose()"));
            engines.ForEach(engine => engine.Dispose());
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property()
        {
            var value = new Random();

            Iterate(JSOnly, All, (engine, type) =>
            {
                engine.Script.value = value;
                Assert.AreSame(value, engine.Evaluate(type.Name + ".Script.value = value"));
                Assert.AreSame(value, engine.Evaluate(type.Name + ".Script.value"));
            });

            Iterate(VBSOnly, All, (engine, type) =>
            {
                var innerEngine = (ScriptEngine)engine.Evaluate(type.Name);
                innerEngine.Script.value = value;
                Assert.AreSame(value, engine.Evaluate(type.Name + ".Script.value"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Scalar()
        {
            const int value = 123;

            Iterate(JSOnly, All, (engine, type) =>
            {
                engine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value = value"));
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });

            Iterate(VBSOnly, All, (engine, type) =>
            {
                var innerEngine = (ScriptEngine)engine.Evaluate(type.Name);
                innerEngine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Enum()
        {
            const DayOfWeek value = DayOfWeek.Thursday;

            Iterate(JSOnly, All, (engine, type) =>
            {
                engine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value = value"));
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });

            Iterate(VBSOnly, All, (engine, type) =>
            {
                var innerEngine = (ScriptEngine)engine.Evaluate(type.Name);
                innerEngine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Struct()
        {
            var value = new DateTime(2007, 5, 22, 6, 15, 43);

            Iterate(JSOnly, All, (engine, type) =>
            {
                engine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value = value"));
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });

            Iterate(VBSOnly, All, (engine, type) =>
            {
                var innerEngine = (ScriptEngine)engine.Evaluate(type.Name);
                innerEngine.Script.value = value;
                Assert.AreEqual(value, engine.Evaluate(type.Name + ".Script.value"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Index_ArrayItem()
        {
            Iterate(JSOnly, JSOnly, (engine, type) =>
            {
                engine.Execute(type.Name + ".Execute('foo = []')");
                Assert.AreEqual(Math.PI, engine.Evaluate(type.Name + ".Script.foo[4] = Math.PI"));
                Assert.AreEqual(Math.PI, engine.Evaluate(type.Name + ".Script.foo[4]"));
                Assert.AreEqual(5, engine.Evaluate(type.Name + ".Evaluate('foo.length')"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Index_Property()
        {
            Iterate(JSOnly, JSOnly, (engine, type) =>
            {
                engine.Execute(type.Name + ".Execute('foo = {}')");
                Assert.AreEqual(Math.E, engine.Evaluate(type.Name + ".Script.foo['bar'] = Math.E"));
                Assert.AreEqual(Math.E, engine.Evaluate(type.Name + ".Script.foo['bar']"));
                Assert.AreEqual(Math.E, engine.Evaluate(type.Name + ".Script.foo.bar"));
            });
        }

        [TestMethod, TestCategory("CrossEngine")]
        public void CrossEngine_Property_Method()
        {
            // WORKAROUND: Windows Script engines apparently refuse to evaluate or execute
            // non-enumerable external functions. This affects external script built-ins.
            // The workaround is to invoke the built-in from a custom function.

            Iterate(All, JSOnly, (engine, type) =>
            {
                engine.Execute(type.Name + ".Execute(\"function doEval(expr) { return eval(expr); }\")");
                Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(type.Name + ".Script.doEval(\"Math.E * Math.PI\")"));
            });

            Iterate(All, VBSOnly, (engine, type) =>
            {
                engine.Execute(type.Name + ".Execute(\"function doEval(expr) : doEval = eval(expr): end function\")");
                Assert.AreEqual(Math.E * Math.PI, engine.Evaluate(type.Name + ".Script.doEval(\"4 * atn(1) * exp(1)\")"));
            });
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private void Iterate(Action<ScriptEngine, Type> action)
        {
            Iterate(All, All, action);
        }

        private void Iterate(Func<ScriptEngine, bool> engineFilter, Func<Type, bool> typeFilter, Action<ScriptEngine, Type> action)
        {
            engines.Where(engineFilter).ForEach(engine => types.Where(typeFilter).ForEach(type => action(engine, type)));
        }

        private static bool All(ScriptEngine engine)
        {
            return true;
        }

        private static bool All(Type type)
        {
            return true;
        }

        private static bool JSOnly(ScriptEngine engine)
        {
            return (engine is V8ScriptEngine) || (engine is JScriptEngine);
        }

        private static bool JSOnly(Type type)
        {
            return (type == typeof(V8ScriptEngine)) || typeof(JScriptEngine).IsAssignableFrom(type);
        }

        private static bool VBSOnly(ScriptEngine engine)
        {
            return engine is VBScriptEngine;
        }

        private static bool VBSOnly(Type type)
        {
            return type == typeof(VBScriptEngine);
        }

        #endregion
    }
}
