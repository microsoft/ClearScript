// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class ActivationHelpers
    {
        public static IntPtr CreateInstance<T>(string progID)
        {
            var clsid = CLSIDFromProgID(progID);
            var iid = typeof(T).GUID;
            HResult.Check(NativeMethods.CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out var pInterface));
            return pInterface;
        }

        private static Guid CLSIDFromProgID(string progID)
        {
            if (!Guid.TryParseExact(progID, "B", out var clsid))
            {
                HResult.Check(NativeMethods.CLSIDFromProgID(progID, out clsid));
            }

            return clsid;
        }
    }
}
