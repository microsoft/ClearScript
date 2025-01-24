// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal static class SpecialMemberNames
    {
        public static readonly string Default = MiscHelpers.GetDispIDName(SpecialDispIDs.Default);
        public static readonly string NewEnum = MiscHelpers.GetDispIDName(SpecialDispIDs.NewEnum);
        public static readonly string NewAsyncEnum = MiscHelpers.GetDispIDName(SpecialDispIDs.NewAsyncEnum);
    }
}
