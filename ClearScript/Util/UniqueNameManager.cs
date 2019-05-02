// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.ClearScript.Util
{
    internal interface IUniqueNameManager
    {
        string GetUniqueName(string inputName, string alternate);
    }

    internal sealed class UniqueNameManager : IUniqueNameManager
    {
        private readonly object mapLock = new object();
        private readonly Dictionary<string, uint> map = new Dictionary<string, uint>();

        #region IUniqueNameManager implementation

        public string GetUniqueName(string inputName, string alternate)
        {
            lock (mapLock)
            {
                var nonBlankName = MiscHelpers.EnsureNonBlank(inputName, alternate);

                uint count;
                map.TryGetValue(nonBlankName, out count);

                map[nonBlankName] = ++count;
                return (count < 2) ? nonBlankName : string.Concat(nonBlankName, " [", count, "]");
            }
        }

        #endregion
    }

    internal sealed class UniqueFileNameManager : IUniqueNameManager
    {
        private readonly object mapLock = new object();
        private readonly Dictionary<string, uint> map = new Dictionary<string, uint>();

        #region IUniqueNameManager implementation

        public string GetUniqueName(string inputName, string alternate)
        {
            lock (mapLock)
            {
                var nonBlankName = MiscHelpers.EnsureNonBlank(Path.GetFileNameWithoutExtension(inputName), alternate);
                var extension = Path.GetExtension(inputName);

                uint count;
                map.TryGetValue(nonBlankName, out count);

                map[nonBlankName] = ++count;
                return (count < 2) ? string.Concat(nonBlankName, extension) : string.Concat(nonBlankName, " [", count, "]", extension);
            }
        }

        #endregion
    }
}
