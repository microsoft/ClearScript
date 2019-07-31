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
        IsTransient = 0x00000001
    }
}
