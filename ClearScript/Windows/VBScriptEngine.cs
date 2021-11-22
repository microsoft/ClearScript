// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an instance of the VBScript engine in a desktop environment.
    /// </summary>
    /// <remarks>
    /// Each Windows Script engine instance in a desktop environment has thread affinity and is
    /// bound to a <see cref="System.Windows.Threading.Dispatcher"/> during instantiation.
    /// Attempting to execute script code on a different thread results in an exception. Script
    /// delegates and event handlers are marshaled synchronously onto the correct thread.
    /// </remarks>
    public class VBScriptEngine : WindowsScriptEngine, IVBScriptEngineTag
    {
        #region constructors

        /// <summary>
        /// Initializes a new VBScript engine instance.
        /// </summary>
        public VBScriptEngine()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        public VBScriptEngine(string name)
            : this(name, WindowsScriptEngineFlags.None)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified name and options.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public VBScriptEngine(string name, WindowsScriptEngineFlags flags)
            : this("VBScript", name, "vbs", flags)
        {
        }

        /// <summary>
        /// Initializes a new VBScript engine instance with the specified programmatic
        /// identifier, name, list of supported file name extensions, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the VBScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected VBScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(progID, name, fileNameExtensions, flags)
        {
            Execute(MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name), Core.VBScriptEngine.InitScript);
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        /// <remarks>
        /// <see cref="VBScriptEngine"/> instances return "vbs" for this property.
        /// </remarks>
        public override string FileNameExtension => "vbs";

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
        /// The <see cref="VBScriptEngine"/> version of this method supports both expressions and
        /// statements. If the specified command begins with "eval " (not case-sensitive), the
        /// engine executes the remainder as an expression and attempts to use
        /// <see href="https://docs.microsoft.com/en-us/previous-versions//0zk841e9(v=vs.85)">CStr</see>
        /// to convert the result value. Otherwise, it executes the command as a statement and does
        /// not return a value.
        /// </para>
        /// </remarks>
        public override string ExecuteCommand(string command)
        {
            var trimmedCommand = command.Trim();
            if (trimmedCommand.StartsWith("eval ", StringComparison.OrdinalIgnoreCase))
            {
                var expression = MiscHelpers.FormatInvariant("EngineInternal.getCommandResult({0})", trimmedCommand.Substring(5));
                var documentInfo = new DocumentInfo("Expression") { Flags = DocumentFlags.IsTransient };
                return GetCommandResultString(Evaluate(documentInfo.MakeUnique(this), expression, false));
            }

            Execute("Command", true, trimmedCommand);
            return null;
        }

        internal override IDictionary<int, string> RuntimeErrorMap => Core.VBScriptEngine.StaticRuntimeErrorMap;

        internal override IDictionary<int, string> SyntaxErrorMap => Core.VBScriptEngine.StaticSyntaxErrorMap;

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category != DocumentCategory.Script)
            {
                throw new NotSupportedException("The script engine cannot execute documents of type '" + documentInfo.Category + "'");
            }

            return base.Execute(documentInfo, code, evaluate);
        }

        #endregion
    }
}
