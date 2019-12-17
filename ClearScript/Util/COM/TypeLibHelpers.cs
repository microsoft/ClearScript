// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class TypeLibHelpers
    {
        public static string GetName(this ITypeLib typeLib)
        {
            return typeLib.GetMemberName(-1);
        }

        public static string GetMemberName(this ITypeLib typeLib, int index)
        {
            string name;
            string docString;
            int helpContext;
            string helpFile;
            typeLib.GetDocumentation(index, out name, out docString, out helpContext, out helpFile);
            return name;
        }
    }
}
