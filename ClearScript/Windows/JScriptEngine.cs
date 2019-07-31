// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an instance of the JScript engine.
    /// </summary>
    public class JScriptEngine : WindowsScriptEngine, IJavaScriptEngine
    {
        #region data

        private static readonly Dictionary<int, string> runtimeErrorMap = new Dictionary<int, string>
        {
            // https://msdn.microsoft.com/en-us/library/1dk3k160(v=vs.84).aspx
            { 5029, "Array length must be a finite positive integer" },
            { 5030, "Array length must be assigned a finite positive number" },
            { 5028, "Array or arguments object expected" },
            { 5010, "Boolean expected" },
            { 5003, "Cannot assign to a function result" },
            { 5000, "Cannot assign to 'this'" },
            { 5034, "Circular reference in value argument not supported" },
            { 5006, "Date object expected" },
            { 5015, "Enumerator object expected" },
            { 5022, "Exception thrown and not caught" },
            { 5020, "Expected ')' in regular expression" },
            { 5019, "Expected ']' in regular expression" },
            { 5023, "Function does not have a valid prototype object" },
            { 5002, "Function expected" },
            { 5008, "Illegal assignment" },
            { 5021, "Invalid range in character set" },
            { 5035, "Invalid replacer argument" },
            { 5014, "JScript object expected" },
            { 5001, "Number expected" },
            { 5007, "Object expected" },
            { 5012, "Object member expected" },
            { 5016, "Regular Expression object expected" },
            { 5005, "String expected" },
            { 5017, "Syntax error in regular expression" },
            { 5026, "The number of fractional digits is out of range" },
            { 5027, "The precision is out of range" },
            { 5025, "The URI to be decoded is not a valid encoding" },
            { 5024, "The URI to be encoded contains an invalid character" },
            { 5009, "Undefined identifier" },
            { 5018, "Unexpected quantifier" },
            { 5013, "VBArray expected" }
        };

        private static readonly Dictionary<int, string> syntaxErrorMap = new Dictionary<int, string>
        {
            // https://msdn.microsoft.com/en-us/library/6bby3x2e(v=vs.84).aspx
            { 1019, "Can't have 'break' outside of loop" },
            { 1020, "Can't have 'continue' outside of loop" },
            { 1030, "Conditional compilation is turned off" },
            { 1027, "'default' can only appear once in a 'switch' statement" },
            { 1005, "Expected '('" },
            { 1006, "Expected ')'" },
            { 1012, "Expected '/'" },
            { 1003, "Expected ':'" },
            { 1004, "Expected ';'" },
            { 1032, "Expected '@'" },
            { 1029, "Expected '@end'" },
            { 1007, "Expected ']'" },
            { 1008, "Expected '{'" },
            { 1009, "Expected '}'" },
            { 1011, "Expected '='" },
            { 1033, "Expected 'catch'" },
            { 1031, "Expected constant" },
            { 1023, "Expected hexadecimal digit" },
            { 1010, "Expected identifier" },
            { 1028, "Expected identifier, string or number" },
            { 1024, "Expected 'while'" },
            { 1014, "Invalid character" },
            { 1026, "Label not found" },
            { 1025, "Label redefined" },
            { 1018, "'return' statement outside of function" },
            { 1002, "Syntax error" },
            { 1035, "Throw must be followed by an expression on the same source line" },
            { 1016, "Unterminated comment" },
            { 1015, "Unterminated string constant" }
        };

        private CommonJSManager commonJSManager;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new JScript engine instance.
        /// </summary>
        public JScriptEngine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public JScriptEngine(string name)
            : this(name, WindowsScriptEngineFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(string name, WindowsScriptEngineFlags flags)
            : this("JScript", name, flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected JScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
            : this(progID, name, "js", flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, list of supported file name extensions, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected JScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(progID, name, fileNameExtensions, flags)
        {
            Execute(
                MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name),
                @"
                    EngineInternal = (function () {

                        function convertArgs(args) {
                            var result = [];
                            if (args.GetValue) {
                                var count = args.Length;
                                for (var i = 0; i < count; i++) {
                                    result.push(args[i]);
                                }
                            }
                            else {
                                args = new VBArray(args);
                                var count = args.ubound(1) + 1;
                                for (var i = 0; i < count; i++) {
                                    result.push(args.getItem(i));
                                }
                            }
                            return result;
                        }

                        function construct(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) {
                            return new this(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
                        }

                        return {

                            getCommandResult: function (value) {
                                if (value != null) {
                                    if ((typeof(value) == 'object') || (typeof(value) == 'function')) {
                                        if (typeof(value.toString) == 'function') {
                                            return value.toString();
                                        }
                                    }
                                }
                                return value;
                            },

                            invokeConstructor: function (constructor, args) {
                                if (typeof(constructor) != 'function') {
                                    throw new Error('Function expected');
                                }
                                return construct.apply(constructor, convertArgs(args));
                            },

                            invokeMethod: function (target, method, args) {
                                if (typeof(method) != 'function') {
                                    throw new Error('Function expected');
                                }
                                return method.apply(target, convertArgs(args));
                            }
                        };
                    })();
                "
            );
        }

        #endregion

        #region internal members

        private CommonJSManager CommonJSManager
        {
            get
            {
                if (commonJSManager == null)
                {
                    commonJSManager = new CommonJSManager(this);
                }

                return commonJSManager;
            }
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="JScriptEngine"/> instances return "js" for this property.
        /// </remarks>
        public override string FileNameExtension
        {
            get { return "js"; }
        }

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// <para>
        /// This method is similar to <see cref="ScriptEngine.Evaluate(string)"/> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
        /// </para>
        /// <para>
        /// The <see cref="JScriptEngine"/> version of this method attempts to use
        /// <see href="http://msdn.microsoft.com/en-us/library/k6xhc6yc(VS.85).aspx">toString</see>
        /// to convert the return value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            Script.EngineInternal.command = command;
            return base.ExecuteCommand("EngineInternal.getCommandResult(eval(EngineInternal.command))");
        }

        internal override IDictionary<int, string> RuntimeErrorMap
        {
            get { return runtimeErrorMap; }
        }

        internal override IDictionary<int, string> SyntaxErrorMap
        {
            get { return syntaxErrorMap; }
        }

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                var module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                return ScriptInvoke(() => module.Process());
            }

            if (documentInfo.Category != DocumentCategory.Script)
            {
                throw new NotSupportedException("Engine cannot execute documents of type '" + documentInfo.Category + "'");
            }

            return base.Execute(documentInfo, code, evaluate);
        }

        #endregion

        #region IJavaScriptEngine implementation

        uint IJavaScriptEngine.BaseLanguageVersion
        {
            get { return 3; }
        }

        #endregion
    }
}
