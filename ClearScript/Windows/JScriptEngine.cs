// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    // ReSharper disable once PartialTypeWithSinglePart

    /// <summary>
    /// Represents an instance of the JScript engine in a desktop environment.
    /// </summary>
    /// <remarks>
    /// Each Windows Script engine instance in a desktop environment has thread affinity and is
    /// bound to a <c><see cref="System.Windows.Threading.Dispatcher"/></c> during instantiation.
    /// Attempting to execute script code on a different thread results in an exception. Script
    /// delegates and event handlers are marshaled synchronously onto the correct thread.
    /// </remarks>
    public partial class JScriptEngine : WindowsScriptEngine, IJavaScriptEngine, Core.IJScriptEngine
    {
        #region data

        private CommonJSManager commonJSManager;
        private JsonModuleManager jsonDocumentManager;

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
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        public JScriptEngine(string name, WindowsScriptEngineFlags flags)
            : this("JScript", name, "js", flags)
        {
        }

        /// <summary>
        /// Initializes a new JScript engine instance with the specified programmatic
        /// identifier, name, list of supported file name extensions, and options.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the JScript engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected JScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(progID, name, fileNameExtensions, flags)
        {
            Execute(MiscHelpers.FormatInvariant("{0} [internal]", GetType().Name), Core.JScriptEngine.InitScript);
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

        internal override IDictionary<int, string> RuntimeErrorMap => Core.JScriptEngine.StaticRuntimeErrorMap;

        internal override IDictionary<int, string> SyntaxErrorMap => Core.JScriptEngine.StaticSyntaxErrorMap;

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            if (FormatCode)
            {
                code = MiscHelpers.FormatCode(code);
            }

            if (documentInfo.Category == ModuleCategory.CommonJS)
            {
                var module = CommonJSManager.GetOrCreateModule(documentInfo, code);
                return ScriptInvoke(static module => module.Process(), module);
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

        object IJavaScriptEngine.CreatePromiseForValueTask<T>(ValueTask<T> valueTask)
        {
            throw new NotImplementedException();
        }

        object IJavaScriptEngine.CreatePromiseForValueTask(ValueTask valueTask)
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
