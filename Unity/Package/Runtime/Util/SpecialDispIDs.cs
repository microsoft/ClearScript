// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal static class SpecialDispIDs
    {
        public const int Default = 0;
        public const int Unknown = -1;
        public const int StartEnum = Unknown;
        public const int PropertyPut = -3;
        public const int NewEnum = -4;
        public const int This = -613;
        public const int GetEnumerator = -1024 * 96;
        public const int NewAsyncEnum = -1024 * 256;
    }
}
