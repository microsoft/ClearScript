// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.JavaScript
{
    /// <exclude/>
    public sealed class CommonJSLegacyModule
    {
        private readonly ScriptObject context;
        private readonly Func<ScriptObject, ScriptObject> initializeContext;
        private ScriptObject initializedContext;

        /// <exclude/>
        public CommonJSLegacyModule(string id, string uri, object exports, ScriptObject context, Func<ScriptObject, ScriptObject> initializeContext)
        {
            this.context = context;
            this.initializeContext = initializeContext;

            this.id = id;
            this.uri = uri;
            this.exports = exports;
        }

        // ReSharper disable InconsistentNaming

        /// <exclude/>
        public string id { get; private set; }

        /// <exclude/>
        public string uri { get; private set; }

        /// <exclude/>
        public object exports { get; set; }

        /// <exclude/>
        public object meta
        {
            get { return GetInitializedContext(); }
        }

        // ReSharper restore InconsistentNaming

        private ScriptObject GetInitializedContext()
        {
            if (initializedContext == null)
            {
                initializedContext = initializeContext(context);
            }

            return initializedContext;
        }
    }
}
