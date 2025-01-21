// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Windows.Threading;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Provides the base implementation for all Windows Script engines in a desktop environment.
    /// </summary>
    /// <remarks>
    /// Each Windows Script engine instance in a desktop environment has thread affinity and is
    /// bound to a <c><see cref="System.Windows.Threading.Dispatcher"/></c> during instantiation.
    /// Attempting to execute script code on a different thread results in an exception. Script
    /// delegates and event handlers are marshaled synchronously onto the correct thread.
    /// </remarks>
    public abstract class WindowsScriptEngine : Core.WindowsScriptEngine
    {
        #region constructors

        /// <summary>
        /// Initializes a new Windows Script engine instance with the specified list of supported file name extensions.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the Windows Script engine class.</param>
        /// <param name="name">A name to associate with the instance. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{F414C260-6AC0-11CF-B6D1-00AA00BBBB58}").
        /// </remarks>
        protected WindowsScriptEngine(string progID, string name, string fileNameExtensions, WindowsScriptEngineFlags flags)
            : base(progID, name, fileNameExtensions, flags, new DispatcherSyncInvoker())
        {
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets the <c><see cref="System.Windows.Threading.Dispatcher"/></c> associated with the current script engine.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get
            {
                VerifyNotDisposed();
                return ((DispatcherSyncInvoker)SyncInvoker).Dispatcher;
            }
        }

        #endregion
    }
}
