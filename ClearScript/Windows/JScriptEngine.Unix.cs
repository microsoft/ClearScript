// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Represents an instance of the JScript engine.
    /// </summary>
    public class JScriptEngine : WindowsScriptEngine, IJavaScriptEngine
    {
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
        protected JScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(progID, name, fileNameExtensions, flags)
        {
        }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        public override string FileNameExtension => "js";

        #endregion

        #region IJavaScriptEngine implementation

        uint IJavaScriptEngine.BaseLanguageVersion => 3;

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
