// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.





using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("ClearScript Library")]
[assembly: AssemblyProduct("ClearScript")]
[assembly: AssemblyCopyright("(c) Microsoft Corporation")]
[assembly: InternalsVisibleTo("ClearScriptV8-32")]
[assembly: InternalsVisibleTo("ClearScriptV8-64")]
[assembly: InternalsVisibleTo("ClearScriptTest")]

[assembly: ComVisible(false)]
[assembly: AssemblyVersion("6.0.1.0")]
[assembly: AssemblyFileVersion("6.0.1.0")]

namespace Microsoft.ClearScript.Properties
{
    internal static class ClearScriptVersion
    {
        public const string Value = "6.0.1.0";
        public const string Triad = "6.0.1";
    }
}
