// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript
{
    [Flags]
    internal enum HostTargetFlags
    {
        None = 0,
        AllowStaticMembers = 0x00000001,
        AllowInstanceMembers = 0x00000002,
        AllowExtensionMethods = 0x00000004
    }

    internal static class HostTargetFlagsHelpers
    {
        public static bool HasAllFlags(this HostTargetFlags value, HostTargetFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this HostTargetFlags value, HostTargetFlags flags) => (value & flags) != 0;
    }
}
