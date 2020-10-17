// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal static partial class MiscHelpers
    {
        #region miscellaneous

        public static bool IsX86InstructionSet()
        {
            SystemInfo info;
            try
            {
                NativeMethods.GetNativeSystemInfo(out info);
            }
            catch (EntryPointNotFoundException)
            {
                NativeMethods.GetSystemInfo(out info);
            }

            return
                ((info.ProcessorArchitecture == 0 /*PROCESSOR_ARCHITECTURE_INTEL*/) ||
                 (info.ProcessorArchitecture == 9 /*PROCESSOR_ARCHITECTURE_AMD64*/));
        }

        #endregion
    }
}
