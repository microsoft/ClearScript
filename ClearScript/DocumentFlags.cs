// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines document attributes.
    /// </summary>
    [Flags]
    public enum DocumentFlags
    {
        /// <summary>
        /// Indicates that no attributes are present.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the document is temporary and can be discarded after use. Only Windows
        /// Script engines honor this attribute.
        /// </summary>
        IsTransient = 0x00000001,

        /// <summary>
        /// Specifies that the script engine is to wait for a debugger connection and schedule a
        /// pause before executing the first line of the document. Windows Script engines do not
        /// honor this attribute. For it to be effective, debugging features must be enabled, a
        /// debugger must not already be connected, and the script engine must not already have
        /// waited for a debugger connection.
        /// </summary>
        AwaitDebuggerAndPause = 0x00000002
    }

    internal static class DocumentFlagsHelpers
    {
        public static bool HasAllFlags(this DocumentFlags value, DocumentFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this DocumentFlags value, DocumentFlags flags) => (value & flags) != 0;
    }
}
