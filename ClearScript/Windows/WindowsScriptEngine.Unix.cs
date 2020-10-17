// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.Util;
using System;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Provides the base implementation for all Windows Script engines.
    /// </summary>
    public abstract class WindowsScriptEngine : ScriptEngine
    {
        #region constructors

        // ReSharper disable UnusedParameter.Local

        /// <summary>
        /// Initializes a new Windows Script engine instance.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        [Obsolete("Use WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags) instead.")]
        protected WindowsScriptEngine(string progID, string name, WindowsScriptEngineFlags flags)
            : this(progID, name, null, flags)
        {
        }

        /// <summary>
        /// Initializes a new Windows Script engine instance with the specified list of supported file name extensions.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        protected WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(name, fileNameExtensions)
        {
        }

        // ReSharper restore UnusedParameter.Local

        #endregion

        #region public members

        /// <summary>
        /// Determines whether the calling thread has access to the current script engine.
        /// </summary>
        /// <returns><c>True</c> if the calling thread has access to the current script engine, <c>false</c> otherwise.</returns>
        public bool CheckAccess()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Enforces that the calling thread has access to the current script engine.
        /// </summary>
        public void VerifyAccess()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Gets or sets an interface that supports the display of dialogs on behalf of script code.
        /// </summary>
        public IHostWindow HostWindow { get; set; }

        #endregion

        #region ScriptEngine overrides

        /// <summary>
        /// Allows the host to access script resources directly.
        /// </summary>
        public override dynamic Script => throw new PlatformNotSupportedException();

        internal override IUniqueNameManager DocumentNameManager => throw new PlatformNotSupportedException();

        internal override HostItemCollateral HostItemCollateral => throw new PlatformNotSupportedException();

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        public override void CollectGarbage(bool exhaustive)
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Gets a string representation of the script call stack.
        /// </summary>
        /// <returns>The script call stack formatted as a string.</returns>
        public override string GetStackTrace()
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Interrupts script execution and causes the script engine to throw an exception.
        /// </summary>
        public override void Interrupt()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void AddHostItem(string itemName, HostItemFlags flags, object item)
        {
            throw new PlatformNotSupportedException();
        }

        internal override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            throw new PlatformNotSupportedException();
        }

        internal override object ExecuteRaw(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            throw new PlatformNotSupportedException();
        }

        internal override object MarshalToHost(object obj, bool preserveHostTarget)
        {
            throw new PlatformNotSupportedException();
        }

        internal override object MarshalToScript(object obj, HostItemFlags flags)
        {
            throw new PlatformNotSupportedException();
        }

        #endregion
    }
}
