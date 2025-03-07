// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.JavaScript
{
    internal sealed class JsonModuleManager
    {
        private readonly ScriptEngine engine;
        private readonly List<Module> moduleCache = new();

        public JsonModuleManager(ScriptEngine engine)
        {
            this.engine = engine;
        }

        public int ModuleCacheSize => moduleCache.Count;

        public Module GetOrCreateModule(UniqueDocumentInfo documentInfo, string json)
        {
            var jsonDigest = json.GetDigest();

            var cachedModule = GetCachedModule(documentInfo, jsonDigest);
            if (cachedModule is not null)
            {
                return cachedModule;
            }

            return CacheModule(new Module(engine, documentInfo, jsonDigest, json));
        }

        private Module GetCachedModule(UniqueDocumentInfo documentInfo, UIntPtr jsonDigest)
        {
            for (var index = 0; index < moduleCache.Count; index++)
            {
                var cachedModule = moduleCache[index];
                if ((cachedModule.DocumentInfo.UniqueId == documentInfo.UniqueId) && (cachedModule.JsonDigest == jsonDigest))
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
            var cachedModule = moduleCache.FirstOrDefault(testModule => (testModule.DocumentInfo.UniqueId == module.DocumentInfo.UniqueId) && (testModule.JsonDigest == module.JsonDigest));
            if (cachedModule is not null)
            {
                return cachedModule;
            }

            var maxModuleCacheSize = Math.Max(16, Convert.ToInt32(Math.Min(DocumentCategory.Json.MaxCacheSize, int.MaxValue)));
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
            private readonly ScriptEngine engine;
            private readonly string json;

            private bool parsed;
            private object result;

            public Module(ScriptEngine engine, UniqueDocumentInfo documentInfo, UIntPtr jsonDigest, string json)
            {
                this.engine = engine;
                this.json = json;

                DocumentInfo = documentInfo;
                JsonDigest = jsonDigest;
            }

            public UniqueDocumentInfo DocumentInfo { get; }

            public UIntPtr JsonDigest { get; }

            public object Result
            {
                get
                {
                    if (!parsed)
                    {
                        parsed = true;
                        result = ((ScriptObject)engine.Global.GetProperty("EngineInternal")).InvokeMethod("parseJson", json);
                    }

                    return result;
                }
            }
        }

        #endregion
    }
}
