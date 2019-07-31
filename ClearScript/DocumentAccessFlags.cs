// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines document access options.
    /// </summary>
    [Flags]
    public enum DocumentAccessFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that documents may be loaded from the file system.
        /// </summary>
        EnableFileLoading = 0x00000001,

        /// <summary>
        /// Specifies that documents may be downloaded from the Web.
        /// </summary>
        EnableWebLoading = 0x00000002,

        /// <summary>
        /// Specifies that documents may be loaded from any location.
        /// </summary>
        EnableAllLoading = EnableFileLoading | EnableWebLoading,

        /// <summary>
        /// Specifies that a document path must begin with a segment of "." or ".." to be
        /// considered a relative path. By default, any path that is not explicitly a top-level
        /// or root path is eligible.
        /// </summary>
        EnforceRelativePrefix = 0x00000004
    }
}
