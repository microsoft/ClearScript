// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static partial class MiscHelpers
    {
        #region miscellaneous

        public static bool PlatformIsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool ProcessorArchitectureIsIntel()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X64:
                case Architecture.X86:
                    return true;

                default:
                    return false;
            }
        }

        public static bool ProcessorArchitectureIsArm()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm:
                case Architecture.Arm64:
                    return true;

                default:
                    return false;
            }
        }

        #endregion
    }
}
