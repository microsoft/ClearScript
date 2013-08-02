// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;

namespace Microsoft.ClearScript.Util
{
    internal static class AssemblyHelpers
    {
        private static ConcurrentDictionary<string, string> table;

        static AssemblyHelpers()
        {
            LoadAssemblyTable();
            if (table != null)
            {
                AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => table.TryAdd(args.LoadedAssembly.GetName().Name, args.LoadedAssembly.FullName);
                AppDomain.CurrentDomain.GetAssemblies().ForEach(assembly => table.TryAdd(assembly.GetName().Name, assembly.FullName));
            }
        }

        public static string GetFullAssemblyName(string name)
        {
            string fullName;
            return ((table != null) && table.TryGetValue(name, out fullName)) ? fullName : name;
        }

        private static void LoadAssemblyTable()
        {
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            dirPath = Path.Combine(dirPath, "Microsoft", "ClearScript", Environment.Is64BitProcess ? "x64" : "x86", GetRuntimeVersionDirectoryName());
            Directory.CreateDirectory(dirPath);

            var filePath = Path.Combine(dirPath, "AssemblyTable.bin");
            if (File.Exists(filePath))
            {
                // ReSharper disable EmptyGeneralCatchClause

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var formatter = new BinaryFormatter();
                        table = (ConcurrentDictionary<string, string>)formatter.Deserialize(stream);
                    }
                }
                catch (Exception)
                {
                }

                // ReSharper restore EmptyGeneralCatchClause
            }

            if (table == null)
            {
                // ReSharper disable EmptyGeneralCatchClause

                BuildAssemblyTable();
                if (table != null)
                {
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                        {
                            var formatter = new BinaryFormatter();
                            formatter.Serialize(stream, table);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        private static void BuildAssemblyTable()
        {
            var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
            if (key != null)
            {
                var dirPath = Path.Combine((string)key.GetValue("InstallRoot"), GetRuntimeVersionDirectoryName());

                table = new ConcurrentDictionary<string, string>();
                foreach (var filePath in Directory.EnumerateFiles(dirPath, "*.dll", SearchOption.AllDirectories))
                {
                    // ReSharper disable EmptyGeneralCatchClause

                    try
                    {
                        var assemblyName = Assembly.ReflectionOnlyLoadFrom(filePath).GetName();
                        table.TryAdd(assemblyName.Name, assemblyName.FullName);
                    }
                    catch (Exception)
                    {
                    }

                    // ReSharper restore EmptyGeneralCatchClause
                }
            }
        }

        private static string GetRuntimeVersionDirectoryName()
        {
            return MiscHelpers.FormatInvariant("v{0}", Environment.Version.ToString(3));
        }
    }
}
