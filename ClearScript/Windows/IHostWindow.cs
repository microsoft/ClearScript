// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Windows
{
    /// <summary>
    /// Allows Windows Script engines to display dialogs within the host's user interface.
    /// </summary>
    /// <seealso cref="WindowsScriptEngine.HostWindow"/>
    public interface IHostWindow
    {
        /// <summary>
        /// Gets the handle of an owner window for displaying dialogs on behalf of script code.
        /// </summary>
        IntPtr OwnerHandle { get; }

        /// <summary>
        /// Enables or disables the host's modeless dialogs.
        /// </summary>
        /// <param name="enable"><c>True</c> to enable the host's modeless dialogs, <c>false</c> otherwise.</param>
        void EnableModeless(bool enable);
    }
}
