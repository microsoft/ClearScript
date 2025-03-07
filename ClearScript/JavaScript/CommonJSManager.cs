// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    internal sealed class CommonJSManager
    {
        private readonly ScriptEngine engine;
        private readonly ScriptObject createModule;
        private readonly List<Module> moduleCache = new();
        private static readonly DocumentInfo createModuleInfo = new("CommonJS-createModule [internal]");

        public CommonJSManager(ScriptEngine engine)
        {
            this.engine = engine;
            if (((IJavaScriptEngine)engine).BaseLanguageVersion >= 5)
            {
                createModule = (ScriptObject)engine.Evaluate(createModuleInfo, @"
                    (function (id, uri, hostInitialize, hostRequire, initializeContext) {
                        'use strict';
                        var module = {}, exports = {}, context;
                        Object.defineProperty(module, 'id', { value: id });
                        if (uri) {
                            Object.defineProperty(module, 'uri', { value: uri });
                        }
                        module.exports = exports;
                        Object.defineProperty(module, 'meta', { get: function () {
                            return context || (context = initializeContext({}));
                        }});
                        hostInitialize(exports, function (moduleId) {
                            return hostRequire(moduleId);
                        });
                        return module;
                    }).valueOf()
                ");
            }
            else
            {
                createModule = (ScriptObject)engine.Evaluate(createModuleInfo, @"
                    (function (id, uri, hostInitialize, hostRequire, initializeContext, LegacyModule) {
                        'use strict';
                        var exports = {};
                        hostInitialize(exports, function (moduleId) {
                            return hostRequire(moduleId);
                        });
                        return new LegacyModule(id, uri, exports, {}, initializeContext);
                    }).valueOf()
                ");
            }
        }

        public int ModuleCacheSize => moduleCache.Count;

        public Module GetOrCreateModule(UniqueDocumentInfo documentInfo, string code)
        {
            var codeDigest = code.GetDigest();

            var cachedModule = GetCachedModule(documentInfo, codeDigest);
            if (cachedModule is not null)
            {
                return cachedModule;
            }

            return CacheModule(new Module(this, engine, documentInfo, codeDigest, code));
        }

        public Module GetOrCreateModule(UniqueDocumentInfo documentInfo, UIntPtr codeDigest, Func<object> evaluator)
        {
            var cachedModule = GetCachedModule(documentInfo, codeDigest);
            if (cachedModule is not null)
            {
                return cachedModule;
            }

            return CacheModule(new Module(this, engine, documentInfo, codeDigest, evaluator));
        }

        private Module GetCachedModule(UniqueDocumentInfo documentInfo, UIntPtr codeDigest)
        {
            for (var index = 0; index < moduleCache.Count; index++)
            {
                var cachedModule = moduleCache[index];
                if ((cachedModule.DocumentInfo.UniqueId == documentInfo.UniqueId) && (cachedModule.CodeDigest == codeDigest))
                {
                    moduleCache.RemoveAt(index);
                    moduleCache.Insert(0, cachedModule);
                    return cachedModule;
                }
            }

            return null;
        }

        private Module CacheModule(Module module)
        {
            var cachedModule = moduleCache.FirstOrDefault(testModule => (testModule.DocumentInfo.UniqueId == module.DocumentInfo.UniqueId) && (testModule.CodeDigest == module.CodeDigest));
            if (cachedModule is not null)
            {
                return cachedModule;
            }

            var maxModuleCacheSize = Math.Max(16, Convert.ToInt32(Math.Min(ModuleCategory.CommonJS.MaxCacheSize, int.MaxValue)));
            while (moduleCache.Count >= maxModuleCacheSize)
            {
                moduleCache.RemoveAt(moduleCache.Count - 1);
            }

            moduleCache.Insert(0, module);
            return module;
        }

        #region Nested type: Module

        public sealed class Module
        {
            private readonly CommonJSManager manager;
            private readonly ScriptEngine engine;

            private object module;
            private object exports;
            private object require;

            private ScriptObject function;
            private bool invoked;

            private const string codePrefix = "(function (module, exports, require) {\n";
            private const string codeSuffix = "\n}).valueOf()";

            public Module(CommonJSManager manager, ScriptEngine engine, UniqueDocumentInfo documentInfo, UIntPtr codeDigest, string code)
                : this(manager, engine, documentInfo, codeDigest, () => engine.ExecuteRaw(documentInfo, GetAugmentedCode(code), true))
            {
            }

            public Module(CommonJSManager manager, ScriptEngine engine, UniqueDocumentInfo documentInfo, UIntPtr codeDigest, Func<object> evaluator)
            {
                this.manager = manager;
                this.engine = engine;

                DocumentInfo = documentInfo;
                CodeDigest = codeDigest;
                Evaluator = evaluator;
            }

            public UniqueDocumentInfo DocumentInfo { get; }

            public UIntPtr CodeDigest { get; }

            public Func<object> Evaluator { get; set; }

            public static string GetAugmentedCode(string code)
            {
                return codePrefix + code + codeSuffix;
            }

            public object Process() => Process(out _);

            public object Process(out object marshaledExports)
            {
                if (module is null)
                {
                    var id = (DocumentInfo.Uri is not null) ? DocumentInfo.Uri.AbsoluteUri : DocumentInfo.UniqueName;
                    var uri = (DocumentInfo.Uri is not null) ? id : null;
                    Action<object, object> hostInitialize = Initialize;
                    Func<string, object> hostRequire = Require;
                    Func<ScriptObject, ScriptObject> initializeContext = InitializeContext;
                    module = manager.createModule.Invoke(false, id, uri, hostInitialize, hostRequire, initializeContext, typeof(CommonJSLegacyModule).ToHostType(engine));
                }

                if (function is null)
                {
                    function = (ScriptObject)engine.MarshalToHost(engine.ScriptInvoke(static self => self.Evaluator(), this), false);
                }

                object result;
                if (invoked)
                {
                    result = Undefined.Value;
                }
                else
                {
                    invoked = true;
                    result = function.Invoke(false, module, exports, require);
                    exports = (module is CommonJSLegacyModule legacyModule) ? legacyModule.exports : ((ScriptObject)module).GetProperty("exports");
                }

                marshaledExports = engine.MarshalToScript(exports);
                return result;
            }

            private void Initialize(object scriptExports, object scriptRequire)
            {
                exports = scriptExports;
                require = scriptRequire;
            }

            private object Require(string specifier)
            {
                var document = engine.DocumentSettings.LoadDocument(DocumentInfo.Info, specifier, ModuleCategory.CommonJS, null);

                if (document.Info.Category == ModuleCategory.CommonJS)
                {
                    var code = document.GetTextContents();
                    if (engine.FormatCode)
                    {
                        code = MiscHelpers.FormatCode(code);
                    }

                    var target = manager.GetOrCreateModule(document.Info.MakeUnique(engine), code);
                    target.Process();

                    return target.exports;
                }

                if (document.Info.Category == DocumentCategory.Json)
                {
                    return ((IJavaScriptEngine)engine).JsonModuleManager.GetOrCreateModule(document.Info.MakeUnique(engine), document.GetTextContents()).Result;
                }

                var uri = document.Info.Uri;
                var name = (uri is not null) ? (uri.IsFile ? uri.LocalPath : uri.AbsoluteUri) : document.Info.Name;
                throw new FileLoadException($"Unsupported document category '{document.Info.Category}'", name);
            }

            private ScriptObject InitializeContext(ScriptObject context)
            {
                var callback = DocumentInfo.ContextCallback ?? engine.DocumentSettings.ContextCallback;
                var properties = callback?.Invoke(DocumentInfo.Info);
                if (properties is not null)
                {
                    foreach (var pair in properties)
                    {
                        context.SetProperty(pair.Key, pair.Value);
                    }
                }

                return context;
            }
        }

        #endregion
    }
}
