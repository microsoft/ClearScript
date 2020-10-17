// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region initialization

        private static HostItem Create(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            return new HostItem(engine, target, flags);
        }

        #endregion

        #region ICustomQueryInterface implementation

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr pInterface)
        {
            pInterface = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }

        #endregion
    }
}
