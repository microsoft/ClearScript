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
        private readonly Dictionary<string, uint> map = new();

        #region IUniqueNameManager implementation

        public string GetUniqueName(string inputName, string alternate)
        {
            lock (map)
            {
                var nonBlankName = inputName.ToNonBlank(alternate);

                map.TryGetValue(nonBlankName, out var count);

                map[nonBlankName] = ++count;
                return (count < 2) ? nonBlankName : string.Concat(nonBlankName, " [", count, "]");
            }
        }

        #endregion
    }

    internal sealed class UniqueFileNameManager : IUniqueNameManager
    {
        private readonly Dictionary<string, uint> map = new();

        #region IUniqueNameManager implementation

        public string GetUniqueName(string inputName, string alternate)
        {
            lock (map)
            {
                var nonBlankName = Path.GetFileNameWithoutExtension(inputName).ToNonBlank(alternate);
                var extension = Path.GetExtension(inputName);

                map.TryGetValue(nonBlankName, out var count);

                map[nonBlankName] = ++count;
                return (count < 2) ? string.Concat(nonBlankName, extension) : string.Concat(nonBlankName, " [", count, "]", extension);
            }
        }

        #endregion
    }
}
