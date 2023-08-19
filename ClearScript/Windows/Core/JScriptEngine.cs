// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows.Core
{
    internal interface IJScriptEngine
    {
    }

    // ReSharper disable once PartialTypeWithSinglePart

    /// <summary>
    /// Represents an instance of the JScript engine.
    /// </summary>
    /// <remarks>
    /// This class can be used in non-desktop environments such as server applications. An
    /// implementation of <c><see cref="ISyncInvoker"/></c> is required to enforce thread affinity.
    /// </remarks>
    public partial class JScriptEngine : WindowsScriptEngine, IJavaScriptEngine, IJScriptEngine
    {
        #region data

        internal static readonly Dictionary<int, string> StaticRuntimeErrorMap = new Dictionary<int, string>
        {
            // https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/1dk3k160(v=vs.84)
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

        internal static readonly Dictionary<int, string> StaticSyntaxErrorMap = new Dictionary<int, string>
        {
            // https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/scripting-articles/6bby3x2e(v=vs.84)
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
        private JsonModuleManager jsonDocumentManager;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new JScript engine instance.
        /// </summary>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public JScriptEngine(ISyncInvoker syncInvoker)
            : this(null, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public JScriptEngine(string name, ISyncInvoker syncInvoker)
            : this(name, WindowsScriptEngineFlags.None, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public JScriptEngine(WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : this(null, flags, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified name, options, and
        /// synchronous invoker.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        public JScriptEngine(string name, WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : this("JScript", name, "js", flags, syncInvoker)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, list of supported file name extensions, options, and synchronous
        /// invoker.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="syncInvoker">An object that enforces thread affinity for the instance.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>

        protected JScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags, ISyncInvoker syncInvoker)
            : base(progID, name, fileNameExtensions, flags, syncInvoker)
        {
            Execute(MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name), InitScript);
        }

        #endregion

        #region internal members

        internal CommonJSManager CommonJSManager => commonJSManager ?? (commonJSManager = new CommonJSManager(this));

        internal JsonModuleManager JsonModuleManager => jsonDocumentManager ?? (jsonDocumentManager = new JsonModuleManager(this));

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <c><see cref="JScriptEngine"/></c> instances return "js" for this property.
        /// </remarks>
        public override string FileNameExtension => "js";

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// <para>
        /// This method is similar to <c><see cref="ScriptEngine.Evaluate(string)"/></c> but optimized for
        /// command consoles. The specified command must be limited to a single expression or
        /// statement. Script engines can override this method to customize command execution as
        /// well as the process of converting the result to a string for console output.
        /// </para>
        /// <para>
        /// The <c><see cref="JScriptEngine"/></c> version of this method attempts to use
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Object/tostring">toString</see></c>
        /// to convert the return value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            var engineInternal = (ScriptObject)Global.GetProperty("EngineInternal");
            engineInternal.SetProperty("command", command);
            return base.ExecuteCommand("EngineInternal.getCommandResult(eval(EngineInternal.command))");
        }

        internal override IDictionary<int, string> RuntimeErrorMap => StaticRuntimeErrorMap;

        internal override IDictionary<int, string> SyntaxErrorMap => StaticSyntaxErrorMap;

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
                throw new NotSupportedException("The script engine cannot execute documents of type '" + documentInfo.Category + "'");
            }

            return base.Execute(documentInfo, code, evaluate);
        }

        #endregion

        #region IJavaScriptEngine implementation

        uint IJavaScriptEngine.BaseLanguageVersion => 3;

        CommonJSManager IJavaScriptEngine.CommonJSManager => CommonJSManager;

        JsonModuleManager IJavaScriptEngine.JsonModuleManager => JsonModuleManager;

        object IJavaScriptEngine.CreatePromiseForTask<T>(Task<T> task)
        {
            throw new NotImplementedException();
        }

        object IJavaScriptEngine.CreatePromiseForTask(Task task)
        {
            throw new NotImplementedException();
        }

        Task<object> IJavaScriptEngine.CreateTaskForPromise(ScriptObject promise)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
