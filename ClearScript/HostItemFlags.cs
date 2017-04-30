// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Dynamic;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Defines options for exposing host resources to script code.
    /// </summary>
    [Flags]
    public enum HostItemFlags
    {
        /// <summary>
        /// Specifies that no options are selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the host resource's members are to be exposed as global items in the
        /// script engine's root namespace.
        /// </summary>
        GlobalMembers = 0x00000001,

        /// <summary>
        /// Specifies that the host resource's non-public members are to be exposed.
        /// </summary>
        PrivateAccess = 0x00000002,

        /// <summary>
        /// Specifies that the host resource's dynamic members are not to be exposed. This option
        /// applies only to objects that implement <see cref="IDynamicMetaObjectProvider"/>.
        /// </summary>
        HideDynamicMembers = 0x00000004,

        /// <summary>
        /// Specifies that the script engine is to be given direct access to the exposed object if
        /// possible. This option, when supported, suppresses marshaling and hands off the object
        /// for script access without the host's involvement. It is currently supported only for
        /// COM objects exposed in Windows Script engines.
        /// </summary>
        DirectAccess = 0x00000008
    }
}
