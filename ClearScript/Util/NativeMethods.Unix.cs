// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.Util
{
    internal static class NativeMethods
    {
        public static int CLSIDFromProgID(string progID, out Guid clsid)
        {
            clsid = Guid.Empty;
            return HResult.CO_E_CLASSSTRING;
        }

        public static int ProgIDFromCLSID(ref Guid clsid, out string progID)
        {
            progID = null;
            return HResult.REGDB_E_CLASSNOTREG;
        }

        public static int CoCreateInstance(ref Guid clsid, IntPtr pOuter, uint clsContext, ref Guid iid, out IntPtr pInterface)
        {
            pInterface = IntPtr.Zero;
            return HResult.REGDB_E_CLASSNOTREG;
        }

        public static void VariantInit(IntPtr pVariant)
        {
            throw new PlatformNotSupportedException();
        }

        public static uint VariantClear(IntPtr pVariant)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
