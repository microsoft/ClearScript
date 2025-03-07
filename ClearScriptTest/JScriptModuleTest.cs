// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Typos in test code are acceptable.")]
    public class JScriptModuleTest : ClearScriptTest
    {
        #region setup / teardown

        private JScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
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

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('JavaScript/LegacyCommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_ForeignExtension()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('JavaScript/LegacyCommonJS/Arithmetic/Arithmetic.bogus');
                return Arithmetic.BogusAdd(123, 456);
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Geometry")
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('GeometryWithPathlessImport');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_File_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Geometry")
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web_Nested()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Geometry/Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web_Disabled()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Geometry/Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web_PathlessImport()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(123 + 456, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('Arithmetic');
                return Arithmetic.Add(123, 456);
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web_PathlessImport_Nested()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Geometry"
            );

            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableWebLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('GeometryWithPathlessImport');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Web_PathlessImport_Disabled()
        {
            engine.DocumentSettings.SearchPath = string.Join(";",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Arithmetic",
                "https://raw.githubusercontent.com/microsoft/ClearScript/master/ClearScriptTest/JavaScript/LegacyCommonJS/Geometry"
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_SideEffects()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableAllLoading;
            engine.Execute("foo = {}");
            Assert.AreEqual(625, engine.EvaluateDocument("JavaScript/LegacyCommonJS/ModuleWithSideEffects.js", ModuleCategory.CommonJS));
            Assert.AreEqual(625, engine.Evaluate("foo.bar"));

            // re-evaluating a module is a no-op
            Assert.IsInstanceOfType(engine.EvaluateDocument("JavaScript/LegacyCommonJS/ModuleWithSideEffects.js", ModuleCategory.CommonJS), typeof(Undefined));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Module()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            dynamic first = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('JavaScript/LegacyCommonJS/Geometry/Geometry');
            ");

            Assert.IsInstanceOfType(first.Module.id, typeof(string));
            Assert.AreEqual(first.Module.id, first.Module.uri);

            dynamic second = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('" + (string)first.Module.id + @"');
            ");

            Assert.AreEqual(first.Module.id, second.Module.id);
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Context()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            engine.DocumentSettings.Loader = new CustomLoader();

            Assert.AreEqual(123, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return Geometry.Meta.foo;
            "));

            Assert.AreEqual(456.789, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return Geometry.Meta.bar;
            "));

            Assert.AreEqual("bogus", engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return Geometry.Meta.baz;
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return new Geometry.Meta.qux();
            "), typeof(Random));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJS/Geometry/Geometry');
                return Geometry.Meta.quux;
            "), typeof(Undefined));

            Assert.AreEqual(Math.PI, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('JavaScript/LegacyCommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Meta.foo;
            "));

            Assert.IsInstanceOfType(engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Arithmetic = require('JavaScript/LegacyCommonJS/Arithmetic/Arithmetic');
                return Arithmetic.Meta.bar;
            "), typeof(Undefined));

            engine.DocumentSettings.SearchPath = string.Join(";",
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Arithmetic"),
                Path.Combine(Directory.GetCurrentDirectory(), "JavaScript", "LegacyCommonJS", "Geometry")
            );

            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('GeometryWithPathlessImport');
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_OverrideExports()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;

            Assert.AreEqual(Math.PI, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('JavaScript/CommonJS/NewMath').PI;
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_SystemDocument()
        {
            engine.DocumentSettings.AddSystemDocument("test", ModuleCategory.CommonJS, @"
                exports.Add = function (a, b) {
                    return a + b;
                }
            ");

            dynamic add = engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                return require('test').Add
            ");

            Assert.AreEqual(579, add(123, 456));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_CircularReference()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading;
            Assert.AreEqual(25 * 25, engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, @"
                var Geometry = require('JavaScript/LegacyCommonJSWithCycles/Geometry/Geometry');
                return new Geometry.Square(25).getArea();
            "));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Json_Object()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            var result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Object.json')");
            Assert.AreEqual(2, result.PropertyNames.Count());
            Assert.AreEqual(123, result.GetProperty("foo"));
            Assert.AreEqual("baz", result.GetProperty("bar"));

            engine.DocumentSettings.AddSystemDocument("ObjectWithFunction.json", DocumentCategory.Json, "{ \"foo\": 123, \"bar\": \"baz\", \"qux\": function(){} }");
            result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('ObjectWithFunction.json')");
            Assert.AreEqual(3, result.PropertyNames.Count());
            Assert.AreEqual(123, result.GetProperty("foo"));
            Assert.AreEqual("baz", result.GetProperty("bar"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode);
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Object.json')");
            Assert.AreEqual(2, result.PropertyNames.Count());
            Assert.AreEqual(123, result.GetProperty("foo"));
            Assert.AreEqual("baz", result.GetProperty("bar"));

            engine.DocumentSettings.AddSystemDocument("ObjectWithFunction.json", DocumentCategory.Json, "{ \"foo\": 123, \"bar\": \"baz\", \"qux\": function(){} }");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('ObjectWithFunction.json')"));
        }

        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Json_Array()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            var result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Array.json')");
            Assert.AreEqual(4, result.PropertyIndices.Count());
            Assert.AreEqual(123, result.GetProperty(0));
            Assert.AreEqual("foo", result.GetProperty(1));
            Assert.AreEqual(4.56, result.GetProperty(2));
            Assert.AreEqual("bar", result.GetProperty(3));

            engine.DocumentSettings.AddSystemDocument("ArrayWithFunction.json", DocumentCategory.Json, "[ 123, \"foo\", 4.56, \"bar\", function(){} ]");
            result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('ArrayWithFunction.json')");
            Assert.AreEqual(5, result.PropertyIndices.Count());
            Assert.AreEqual(123, result.GetProperty(0));
            Assert.AreEqual("foo", result.GetProperty(1));
            Assert.AreEqual(4.56, result.GetProperty(2));
            Assert.AreEqual("bar", result.GetProperty(3));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode);
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            result = (ScriptObject)engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Array.json')");
            Assert.AreEqual(4, result.PropertyIndices.Count());
            Assert.AreEqual(123, result.GetProperty(0));
            Assert.AreEqual("foo", result.GetProperty(1));
            Assert.AreEqual(4.56, result.GetProperty(2));
            Assert.AreEqual("bar", result.GetProperty(3));

            engine.DocumentSettings.AddSystemDocument("ArrayWithFunction.json", DocumentCategory.Json, "[ 123, \"foo\", 4.56, \"bar\", function(){} ]");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('ArrayWithFunction.json')"));
        }


        [TestMethod, TestCategory("JScriptModule")]
        public void JScriptModule_CommonJS_Json_Malformed()
        {
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            // ReSharper disable once AccessToDisposedClosure
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Malformed.json')"));

            engine.Dispose();
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableStandardsMode);
            engine.DocumentSettings.AccessFlags = DocumentAccessFlags.EnableFileLoading | DocumentAccessFlags.AllowCategoryMismatch;
            engine.DocumentSettings.LoadCallback = (ref DocumentInfo info) =>
            {
                if (Path.GetExtension(info.Uri.AbsolutePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    info.Category = DocumentCategory.Json;
                }
            };

            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate(new DocumentInfo { Category = ModuleCategory.CommonJS }, "return require('JavaScript/Malformed.json')"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private sealed class CustomLoader : DocumentLoader
        {
            public override Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
            {
                return Default.LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback ?? CreateDocumentContext);
            }

            private static IDictionary<string, object> CreateDocumentContext(DocumentInfo info)
            {
                if (info.Uri is not null)
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

                    throw new UnauthorizedAccessException("Module context access is prohibited in this module");
                }

                return null;
            }
        }

        #endregion
    }
}
