// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region initialization

        private static HostItem Create(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            return TargetSupportsExpandoMembers(target, flags) ? new ExpandoHostItem(engine, target, flags) : new HostItem(engine, target, flags);
        }

        #endregion
    }
}
