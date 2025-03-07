// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using Microsoft.Win32;

namespace Microsoft.ClearScript.Util
{
    internal static partial class ObjectHelpers
    {
        private static bool GetPrimaryInteropAssembly(Guid libid, int major, int minor, out string name, out string codeBase)
        {
            name = null;
            codeBase = null;

            using (var containerKey = Registry.ClassesRoot.OpenSubKey("TypeLib", false))
            {
                if (containerKey is not null)
                {
                    var typeLibName = "{" + libid.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
                    using (var typeLibKey = containerKey.OpenSubKey(typeLibName))
                    {
                        if (typeLibKey is not null)
                        {
                            var versionName = major.ToString("x", CultureInfo.InvariantCulture) + "." + minor.ToString("x", CultureInfo.InvariantCulture);
                            using (var versionKey = typeLibKey.OpenSubKey(versionName, false))
                            {
                                if (versionKey is not null)
                                {
                                    name = (string)versionKey.GetValue("PrimaryInteropAssemblyName");
                                    codeBase = (string)versionKey.GetValue("PrimaryInteropAssemblyCodeBase");
                                }
                            }
                        }
                    }
                }
            }

            return name is not null;
        }
    }
}
