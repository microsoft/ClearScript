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
        public static DocumentCategory Standard
        {
            get { return StandardModule.Instance; }
        }

        /// <summary>
        /// Gets the document category for <see href="http://wiki.commonjs.org/wiki/Modules">CommonJS</see> modules.
        /// </summary>
        public static DocumentCategory CommonJS
        {
            get { return CommonJSModule.Instance; }
        }

        #region Nested type: StandardModule

        private sealed class StandardModule : DocumentCategory
        {
            public static readonly StandardModule Instance = new StandardModule();

            private StandardModule()
            {
            }

            #region DocumentCategory overrides

            internal override string DefaultName
            {
                get { return "Module"; }
            }

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
            public static readonly CommonJSModule Instance = new CommonJSModule();

            private CommonJSModule()
            {
            }

            #region DocumentCategory overrides

            internal override string DefaultName
            {
                get { return "Module"; }
            }

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
