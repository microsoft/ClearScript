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
