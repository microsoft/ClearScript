// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.JavaScript
{
    /// <summary>
    /// Defines document categories for JavaScript modules.
    /// </summary>
    public static class ModuleCategory
    {
        /// <summary>
        /// Gets the document category for standard <see href="https://www.ecma-international.org/ecma-262/6.0/#sec-modules">ECMAScript 6</see> modules.
        /// </summary>
        public static DocumentCategory Standard => StandardModule.Instance;

        /// <summary>
        /// Gets the document category for <see href="http://wiki.commonjs.org/wiki/Modules">CommonJS</see> modules.
        /// </summary>
        public static DocumentCategory CommonJS => CommonJSModule.Instance;

        #region Nested type: StandardModule

        private sealed class StandardModule : DocumentCategory
        {
            public static readonly StandardModule Instance = new();

            private StandardModule()
            {
            }

            #region DocumentCategory overrides

            internal override DocumentKind Kind => DocumentKind.JavaScriptModule;

            internal override string DefaultName => "Module";

            #endregion

            #region Object overrides

            public override string ToString()
            {
                return "ECMAScript Module";
            }

            #endregion
        }

        #endregion

        #region Nested type: CommonJSModule

        private sealed class CommonJSModule : DocumentCategory
        {
            public static readonly CommonJSModule Instance = new();

            private CommonJSModule()
            {
            }

            #region DocumentCategory overrides

            internal override DocumentKind Kind => DocumentKind.CommonJSModule;

            internal override string DefaultName => "Module";

            #endregion

            #region Object overrides

            public override string ToString()
            {
                return "CommonJS Module";
            }

            #endregion
        }

        #endregion
    }
}
