// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal class Nonexistent
    {
        public static readonly Nonexistent Value = new Nonexistent();

        private Nonexistent()
        {
        }
    }
}
