// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;

namespace Microsoft.ClearScript.Util
{
    internal static class AssemblyTable
    {
        private static ConcurrentDictionary<string, string> table;

        static AssemblyTable()
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
            // ReSharper disable EmptyGeneralCatchClause

            string filePath = null;
            try
            {
                var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(dirPath))
                {
                    dirPath = Path.Combine(dirPath, "Microsoft", "ClearScript", Environment.Is64BitProcess ? "x64" : "x86", GetRuntimeVersionDirectoryName());
                    Directory.CreateDirectory(dirPath);

                    filePath = Path.Combine(dirPath, "AssemblyTable.bin");
                    if (File.Exists(filePath))
                    {
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
                    }
                }
            }
            catch (Exception)
            {
            }

            if (table == null)
            {
                BuildAssemblyTable();
                if ((table != null) && (filePath != null))
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
            }

            // ReSharper restore EmptyGeneralCatchClause
        }

        private static void BuildAssemblyTable()
        {
            // ReSharper disable EmptyGeneralCatchClause

            try
            {
                var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
                if (key != null)
                {
                    var dirPath = Path.Combine((string)key.GetValue("InstallRoot"), GetRuntimeVersionDirectoryName());

                    table = new ConcurrentDictionary<string, string>();
                    foreach (var filePath in Directory.EnumerateFiles(dirPath, "*.dll", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var assemblyName = Assembly.ReflectionOnlyLoadFrom(filePath).GetName();
                            table.TryAdd(assemblyName.Name, assemblyName.FullName);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            // ReSharper restore EmptyGeneralCatchClause
        }

        private static string GetRuntimeVersionDirectoryName()
        {
            return MiscHelpers.FormatInvariant("v{0}", Environment.Version.ToString(3));
        }
    }
}
