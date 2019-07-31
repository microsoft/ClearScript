// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
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
    [DeploymentItem("JavaScript", "JavaScript")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class V8ModuleTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports | V8ScriptEngineFlags.EnableDebugging);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'JavaScript/StandardModule/Arithmetic/Arithmetic.js';
                Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_MixedImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'Arithmetic.js';
                Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_PathlessImport_MixedImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'GeometryWithDynamicImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'GeometryWithPathlessImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "StandardModule", "Geometry")
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_DynamicImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Arithmetic = await import('JavaScript/StandardModule/Arithmetic/Arithmetic.js');
                        result = Arithmetic.Add(123, 456);
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(123 + 456, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_DynamicImport_MixedImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('JavaScript/StandardModule/Geometry/Geometry.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(25 * 25, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_DynamicImport_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(25 * 25, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_DynamicImport_Disabled()
        {
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsNotInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("throw caughtException"));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_File_FileNameExtensions()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic/Arithmetic.js';
                Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_MixedImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'Arithmetic.js';
                Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_PathlessImport_MixedImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'GeometryWithDynamicImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'GeometryWithPathlessImport.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry"
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'Geometry.js';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_DynamicImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Arithmetic = await import('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Arithmetic/Arithmetic.js');
                        result = Arithmetic.Add(123, 456);
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(123 + 456, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_DynamicImport_MixedImport()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/Geometry.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(25 * 25, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_DynamicImport_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            Assert.AreEqual(25 * 25, engine.Script.result);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_DynamicImport_Disabled()
        {
            engine.Evaluate(@"
                (async function () {
                    try {
                        let Geometry = await import('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js');
                        result = new Geometry.Square(25).Area;
                    }
                    catch (exception) {
                        caughtException = exception;
                    }
                })();
            ");

            Assert.IsNotInstanceOfType(engine.Script.caughtException, typeof(Undefined));
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("throw caughtException"));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Web_FileNameExtensions()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/StandardModule/Geometry/Geometry';
                new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Compilation()
        {
            var module = engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            ");

            using (module)
            {
                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                Assert.AreEqual(25 * 25, engine.Evaluate(module));

                // re-evaluating a module is a no-op
                Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Compilation_CodeCache()
        {
            var code = @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            ";

            byte[] cacheBytes;
            using (engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, code, V8CacheKind.Code, out cacheBytes))
            {
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 350); // typical size is ~700

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports | V8ScriptEngineFlags.EnableDebugging);

            bool cacheAccepted;
            var module = engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, code, V8CacheKind.Code, cacheBytes, out cacheAccepted);
            Assert.IsTrue(cacheAccepted);

            using (module)
            {
                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                Assert.AreEqual(25 * 25, engine.Evaluate(module));

                // re-evaluating a module is a no-op
                Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Compilation_Runtime()
        {
            engine.Dispose();

            using (var runtime = new V8Runtime(V8RuntimeFlags.EnableDebugging))
            {
                var module = runtime.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                    import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                    new Geometry.Square(25).Area;
                ");

                using (module)
                {
                    engine = runtime.CreateScriptEngine();

                    engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                    Assert.AreEqual(25 * 25, engine.Evaluate(module));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
                }
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Compilation_Runtime_CodeCache()
        {
            var code = @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Square(25).Area;
            ";

            byte[] cacheBytes;
            using (engine.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, code, V8CacheKind.Code, out cacheBytes))
            {
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 350); // typical size is ~700

            engine.Dispose();

            using (var runtime = new V8Runtime(V8RuntimeFlags.EnableDebugging))
            {
                engine = runtime.CreateScriptEngine();

                bool cacheAccepted;
                var module = runtime.Compile(new DocumentInfo { Category = ModuleCategory.Standard }, code, V8CacheKind.Code, cacheBytes, out cacheAccepted);
                Assert.IsTrue(cacheAccepted);

                using (module)
                {
                    engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                    Assert.AreEqual(25 * 25, engine.Evaluate(module));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
                }
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_SideEffects()
        {
            using (var runtime = new V8Runtime())
            {
                runtime.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                var module = runtime.CompileDocument("JavaScript/StandardModule/ModuleWithSideEffects.js", ModuleCategory.Standard);

                using (var testEngine = runtime.CreateScriptEngine())
                {
                    testEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                    testEngine.Execute("foo = {}");
                    Assert.AreEqual(625, testEngine.Evaluate(module));
                    Assert.AreEqual(625, testEngine.Evaluate("foo.bar"));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(testEngine.Evaluate(module), typeof(Undefined));
                }

                using (var testEngine = runtime.CreateScriptEngine())
                {
                    testEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                    testEngine.Execute("foo = {}");
                    Assert.AreEqual(625, testEngine.Evaluate(module));
                    Assert.AreEqual(625, testEngine.Evaluate("foo.bar"));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(testEngine.Evaluate(module), typeof(Undefined));
                }
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Caching()
        {
            Assert.AreEqual(0UL, engine.GetRuntimeStatistics().ModuleCount);

            var info = new DocumentInfo { Category = ModuleCategory.Standard };

            Assert.AreEqual(Math.PI, engine.Evaluate(info, "Math.PI"));
            Assert.AreEqual(Math.PI, engine.Evaluate(info, "Math.PI"));
            Assert.AreEqual(2UL, engine.GetRuntimeStatistics().ModuleCount);

            info = new DocumentInfo("Test") { Category = ModuleCategory.Standard };

            Assert.AreEqual(Math.E, engine.Evaluate(info, "Math.E"));
            Assert.IsInstanceOfType(engine.Evaluate(info, "Math.E"), typeof(Undefined));
            Assert.AreEqual(3UL, engine.GetRuntimeStatistics().ModuleCount);

            Assert.AreEqual(Math.PI, engine.Evaluate(info, "Math.PI"));
            Assert.IsInstanceOfType(engine.Evaluate(info, "Math.PI"), typeof(Undefined));
            Assert.AreEqual(4UL, engine.GetRuntimeStatistics().ModuleCount);

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 10; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI, testEngine.Evaluate(info, "Math.PI"));
                        Assert.AreEqual(Math.E, testEngine.Evaluate(info, "Math.E"));
                        Assert.AreEqual(2UL, testEngine.GetStatistics().ModuleCount);
                    }
                }

                Assert.AreEqual(20UL, runtime.GetStatistics().ModuleCount);
            }

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 300; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI, testEngine.Evaluate(info, "Math.PI"));
                        Assert.IsInstanceOfType(testEngine.Evaluate(info, "Math.PI"), typeof(Undefined));
                    }
                }

                Assert.AreEqual(300UL, runtime.GetStatistics().ModuleCount);
            }

            using (var runtime = new V8Runtime())
            {
                using (var testEngine = runtime.CreateScriptEngine())
                {
                    for (var i = 0; i < 300; i++)
                    {
                        Assert.AreEqual(Math.PI + i, testEngine.Evaluate(info, "Math.PI" + "+" + i));
                    }

                    Assert.AreEqual(300UL, testEngine.GetStatistics().ModuleCount);
                    Assert.AreEqual(300UL, testEngine.GetStatistics().ModuleCacheSize);
                }

                Assert.AreEqual(300UL, runtime.GetStatistics().ModuleCount);
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_Standard_Context()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.DocumentSettings.Loader = new CustomLoader();

            Assert.AreEqual(123, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                Geometry.Meta.foo
            "));

            Assert.AreEqual(456.789, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                Geometry.Meta.bar
            "));

            Assert.AreEqual("bogus", engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                Geometry.Meta.baz
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                new Geometry.Meta.qux()
            "), typeof(Random));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/Geometry.js';
                Geometry.Meta.quux
            "), typeof(Undefined));

            Assert.AreEqual(Math.PI, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'JavaScript/StandardModule/Arithmetic/Arithmetic.js';
                Arithmetic.Meta.foo
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Arithmetic from 'JavaScript/StandardModule/Arithmetic/Arithmetic.js';
                Arithmetic.Meta.bar
            "), typeof(Undefined));

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute(new DocumentInfo { Category = ModuleCategory.Standard }, @"
                import * as Geometry from 'JavaScript/StandardModule/Geometry/GeometryWithDynamicImport.js';
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('JavaScript/CommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('GeometryWithPathlessImport');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_File_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Geometry")
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('GeometryWithPathlessImport');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Web_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/CommonJS/Geometry"
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('Geometry');
                return new Geometry.Square(25).Area;
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Compilation()
        {
            var module = engine.Compile(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            ");

            using (module)
            {
                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                Assert.AreEqual(25 * 25, engine.Evaluate(module));

                // re-evaluating a module is a no-op
                Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Compilation_CodeCache()
        {
            var code = @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            ";

            byte[] cacheBytes;
            using (engine.Compile(new DocumentInfo { Category = ModuleCategory.CommonJS }, code, V8CacheKind.Code, out cacheBytes))
            {
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 350); // typical size is ~700

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDynamicModuleImports | V8ScriptEngineFlags.EnableDebugging);

            bool cacheAccepted;
            var module = engine.Compile(new DocumentInfo { Category = ModuleCategory.CommonJS }, code, V8CacheKind.Code, cacheBytes, out cacheAccepted);
            Assert.IsTrue(cacheAccepted);

            using (module)
            {
                engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                Assert.AreEqual(25 * 25, engine.Evaluate(module));

                // re-evaluating a module is a no-op
                Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Compilation_Runtime_CodeCache()
        {
            var code = @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Square(25).Area;
            ";

            byte[] cacheBytes;
            using (engine.Compile(new DocumentInfo { Category = ModuleCategory.CommonJS }, code, V8CacheKind.Code, out cacheBytes))
            {
            }

            Assert.IsNotNull(cacheBytes);
            Assert.IsTrue(cacheBytes.Length > 350); // typical size is ~700

            engine.Dispose();

            using (var runtime = new V8Runtime(V8RuntimeFlags.EnableDebugging))
            {
                engine = runtime.CreateScriptEngine();

                bool cacheAccepted;
                var module = runtime.Compile(new DocumentInfo { Category = ModuleCategory.CommonJS }, code, V8CacheKind.Code, cacheBytes, out cacheAccepted);
                Assert.IsTrue(cacheAccepted);

                using (module)
                {
                    engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
                    Assert.AreEqual(25 * 25, engine.Evaluate(module));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(engine.Evaluate(module), typeof(Undefined));
                }
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_SideEffects()
        {
            using (var runtime = new V8Runtime())
            {
                runtime.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                var module = runtime.CompileDocument("JavaScript/CommonJS/ModuleWithSideEffects.js", ModuleCategory.CommonJS);

                using (var testEngine = runtime.CreateScriptEngine())
                {
                    testEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                    testEngine.Execute("foo = {}");
                    Assert.AreEqual(625, testEngine.Evaluate(module));
                    Assert.AreEqual(625, testEngine.Evaluate("foo.bar"));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(testEngine.Evaluate(module), typeof(Undefined));
                }

                using (var testEngine = runtime.CreateScriptEngine())
                {
                    testEngine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
                    testEngine.Execute("foo = {}");
                    Assert.AreEqual(625, testEngine.Evaluate(module));
                    Assert.AreEqual(625, testEngine.Evaluate("foo.bar"));

                    // re-evaluating a module is a no-op
                    Assert.IsInstanceOfType(testEngine.Evaluate(module), typeof(Undefined));
                }
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Caching()
        {
            Assert.AreEqual(1UL, engine.GetRuntimeStatistics().ScriptCount);

            var info = new DocumentInfo { Category = ModuleCategory.CommonJS };

            Assert.AreEqual(Math.PI, engine.Evaluate(info, "return Math.PI"));
            Assert.AreEqual(Math.PI, engine.Evaluate(info, "return Math.PI"));
            Assert.AreEqual(4UL, engine.GetRuntimeStatistics().ScriptCount);

            info = new DocumentInfo("Test") { Category = ModuleCategory.CommonJS };

            Assert.AreEqual(Math.E, engine.Evaluate(info, "return Math.E"));
            Assert.IsInstanceOfType(engine.Evaluate(info, "return Math.E"), typeof(Undefined));
            Assert.AreEqual(5UL, engine.GetRuntimeStatistics().ScriptCount);

            Assert.AreEqual(Math.PI, engine.Evaluate(info, "return Math.PI"));
            Assert.IsInstanceOfType(engine.Evaluate(info, "return Math.PI"), typeof(Undefined));
            Assert.AreEqual(6UL, engine.GetRuntimeStatistics().ScriptCount);

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 10; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI, testEngine.Evaluate(info, "return Math.PI"));
                        Assert.AreEqual(Math.E, testEngine.Evaluate(info, "return Math.E"));
                        Assert.AreEqual((i < 1) ? 4UL : 0UL, testEngine.GetStatistics().ScriptCount);
                    }
                }

                Assert.AreEqual(4UL, runtime.GetStatistics().ScriptCount);
            }

            using (var runtime = new V8Runtime())
            {
                for (var i = 0; i < 300; i++)
                {
                    using (var testEngine = runtime.CreateScriptEngine())
                    {
                        Assert.AreEqual(Math.PI, testEngine.Evaluate(info, "return Math.PI"));
                        Assert.IsInstanceOfType(testEngine.Evaluate(info, "return Math.PI"), typeof(Undefined));
                    }
                }

                Assert.AreEqual(3UL, runtime.GetStatistics().ScriptCount);
            }

            using (var runtime = new V8Runtime())
            {
                using (var testEngine = runtime.CreateScriptEngine())
                {
                    for (var i = 0; i < 300; i++)
                    {
                        Assert.AreEqual(Math.PI + i, testEngine.Evaluate(info, "return Math.PI" + "+" + i + ";"));
                    }

                    Assert.AreEqual(302UL, testEngine.GetStatistics().ScriptCount);
                    Assert.AreEqual(300, testEngine.GetStatistics().CommonJSModuleCacheSize);
                }

                Assert.AreEqual(302UL, runtime.GetStatistics().ScriptCount);
            }
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Module()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            dynamic first = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('JavaScript/CommonJS/Geometry/Geometry');
            ");

            Assert.IsInstanceOfType(first.Module.id, typeof(string));
            Assert.AreEqual(first.Module.id, first.Module.uri);

            dynamic second = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('" + (string)first.Module.id + @"');
            ");

            Assert.AreEqual(first.Module.id, second.Module.id);
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_Context()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.DocumentSettings.ContextCallback = CustomLoader.CreateDocumentContext;

            Assert.AreEqual(123, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return Geometry.Meta.foo;
            "));

            Assert.AreEqual(456.789, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return Geometry.Meta.bar;
            "));

            Assert.AreEqual("bogus", engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return Geometry.Meta.baz;
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return new Geometry.Meta.qux();
            "), typeof(Random));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('JavaScript/CommonJS/Geometry/Geometry');
                return Geometry.Meta.quux;
            "), typeof(Undefined));

            Assert.AreEqual(Math.PI, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('JavaScript/CommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Meta.foo;
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Arithmetic = require('JavaScript/CommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Meta.bar;
            "), typeof(Undefined));

            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "CommonJS", "Geometry")
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                let Geometry = require('GeometryWithPathlessImport');
            "));
        }

        [TestMethod, TestCategory("V8Module")]
        public void V8Module_CommonJS_OverrideExports()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            Assert.AreEqual(Math.PI, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('JavaScript/CommonJS/NewMath').PI;
            "));
        }

        #endregion

        #region miscellaneous

        private sealed class CustomLoader : DocumentLoader
        {
            public override Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                return Default.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback ?? CreateDocumentContext);
            }

            public static IDictionary<string, object> CreateDocumentContext(DocumentInfo info)
            {
                if (info.Uri != null)
                {
                    var name = Path.GetFileName(info.Uri.AbsolutePath);

                    if (name.Equals("Geometry.js", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Dictionary<string, object>
                        {
                            { "foo", 123 },
                            { "bar", 456.789 },
                            { "baz", "bogus" },
                            { "qux", typeof(Random).ToHostType() }
                        };
                    }

                    if (name.Equals("Arithmetic.js", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Dictionary<string, object>
                        {
                            { "foo", Math.PI }
                        };
                    }

                    throw new UnauthorizedAccessException("Module context access is prohibited in this module.");
                }

                return null;
            }
        }

        #endregion
    }
}
