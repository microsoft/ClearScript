// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    internal sealed class CommonJSManager
    {
        private readonly ScriptEngine engine;
        private readonly ScriptObject createModule;

        private readonly List<Module> moduleCache = new List<Module>();
        private const int maxModuleCacheSize = 1024;

        private static readonly DocumentInfo createModuleInfo = new DocumentInfo("CommonJS-createModule [internal]");

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

        public int ModuleCacheSize
        {
            get { return moduleCache.Count; }
        }

        public Module GetOrCreateModule(UniqueDocumentInfo documentInfo, string code)
        {
            var codeDigest = code.GetDigest();

            var cachedModule = GetCachedModule(documentInfo, codeDigest);
            if (cachedModule != null)
            {
                return cachedModule;
            }

            return CacheModule(new Module(this, engine, documentInfo, codeDigest, code));
        }

        public Module GetOrCreateModule(UniqueDocumentInfo documentInfo, UIntPtr codeDigest, Func<object> evaluator)
        {
            var cachedModule = GetCachedModule(documentInfo, codeDigest);
            if (cachedModule != null)
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
            if (cachedModule != null)
            {
                return cachedModule;
            }

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

            public UniqueDocumentInfo DocumentInfo { get; private set; }

            public UIntPtr CodeDigest { get; private set; }

            public Func<object> Evaluator { get; set; }

            public static string GetAugmentedCode(string code)
            {
                return codePrefix + code + codeSuffix;
            }

            public object Process()
            {
                if (module == null)
                {
                    var id = (DocumentInfo.Uri != null) ? DocumentInfo.Uri.AbsoluteUri : DocumentInfo.UniqueName;
                    var uri = (DocumentInfo.Uri != null) ? id : null;
                    Action<object, object> hostInitialize = Initialize;
                    Func<string, object> hostRequire = Require;
                    Func<ScriptObject, ScriptObject> initializeContext = InitializeContext;
                    module = manager.createModule.Invoke(false, id, uri, hostInitialize, hostRequire, initializeContext, typeof(CommonJSLegacyModule).ToHostType(engine));
                }

                if (function == null)
                {
                    function = (ScriptObject)engine.MarshalToHost(engine.ScriptInvoke(() => Evaluator()), false);
                }

                if (!invoked)
                {
                    invoked = true;
                    var result = function.Invoke(false, module, exports, require);
                    exports = ((dynamic)module).exports;
                    return result;
                }

                return Undefined.Value;
            }

            private void Initialize(object scriptExports, object scriptRequire)
            {
                exports = scriptExports;
                require = scriptRequire;
            }

            private object Require(string specifier)
            {
                var settings = engine.DocumentSettings;
                var document = settings.Loader.LoadDocument(settings, DocumentInfo.Info, specifier, ModuleCategory.CommonJS, null);

                var code = document.GetTextContents();
                if (engine.FormatCode)
                {
                    code = MiscHelpers.FormatCode(code);
                }

                var target = manager.GetOrCreateModule(document.Info.MakeUnique(engine), code);
                target.Process();

                return target.exports;
            }

            private ScriptObject InitializeContext(ScriptObject context)
            {
                var callback = DocumentInfo.ContextCallback ?? engine.DocumentSettings.ContextCallback;
                if (callback != null)
                {
                    var properties = callback(DocumentInfo.Info);
                    if (properties != null)
                    {
                        foreach (var pair in properties)
                        {
                            context.SetProperty(pair.Key, pair.Value);
                        }
                    }
                }

                return context;
            }
        }

        #endregion
    }
}
