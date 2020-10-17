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
        public static string GetFullAssemblyName(string name)
        {
            return HostSettings.UseAssemblyTable ? AssemblyTableImpl.GetFullAssemblyNameImpl(name) : AssemblyHelpers.GetFullAssemblyName(name);
        }

        #region Nested type: AssemblyTableImpl

        internal static class AssemblyTableImpl
        {
            private static ConcurrentDictionary<string, string> table;

            static AssemblyTableImpl()
            {
                LoadAssemblyTable();
                if (table != null)
                {
                    AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => table.TryAdd(args.LoadedAssembly.GetName().Name, args.LoadedAssembly.FullName);
                    AppDomain.CurrentDomain.GetAssemblies().ForEach(assembly => table.TryAdd(assembly.GetName().Name, assembly.FullName));
                }
            }

            public static string GetFullAssemblyNameImpl(string name)
            {
                return ((table != null) && table.TryGetValue(name, out var fullName)) ? fullName : AssemblyHelpers.GetFullAssemblyName(name);
            }

            private static void LoadAssemblyTable()
            {
                if (!ReadAssemblyTable() && BuildAssemblyTable())
                {
                    WriteAssemblyTable();
                }
            }

            private static bool ReadAssemblyTable()
            {
                if (ReadAssemblyTable(MiscHelpers.GetLocalDataRootPath(out var usingAppPath)))
                {
                    return true;
                }

                return !usingAppPath && ReadAssemblyTable(MiscHelpers.GetLocalDataRootPath(AppDomain.CurrentDomain.BaseDirectory));
            }

            private static bool ReadAssemblyTable(string rootPath)
            {
                var succeeded = MiscHelpers.Try(() =>
                {
                    var filePath = GetFilePath(rootPath);
                    if (File.Exists(filePath))
                    {
                        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            var formatter = new BinaryFormatter();
                            table = (ConcurrentDictionary<string, string>)formatter.Deserialize(stream);
                        }
                    }
                });

                return succeeded && (table != null);
            }

            private static bool BuildAssemblyTable()
            {
                var succeeded = MiscHelpers.Try(() =>
                {
                    var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework");
                    if (key != null)
                    {
                        var dirPath = Path.Combine((string)key.GetValue("InstallRoot"), GetRuntimeVersionDirectoryName());

                        table = new ConcurrentDictionary<string, string>();
                        foreach (var filePath in Directory.EnumerateFiles(dirPath, "*.dll", SearchOption.AllDirectories))
                        {
                            var path = filePath;
                            MiscHelpers.Try(() =>
                            {
                                var assemblyName = Assembly.ReflectionOnlyLoadFrom(path).GetName();
                                table.TryAdd(assemblyName.Name, assemblyName.FullName);
                            });
                        }
                    }
                });

                return succeeded && (table != null);
            }

            private static void WriteAssemblyTable()
            {
                if (!WriteAssemblyTable(MiscHelpers.GetLocalDataRootPath(out var usingAppPath)) && !usingAppPath)
                {
                    WriteAssemblyTable(MiscHelpers.GetLocalDataRootPath(AppDomain.CurrentDomain.BaseDirectory));
                }
            }

            private static bool WriteAssemblyTable(string rootPath)
            {
                return MiscHelpers.Try(() =>
                {
                    var filePath = GetFilePath(rootPath);
                    using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(stream, table);
                    }
                });
            }

            private static string GetFilePath(string rootPath)
            {
                var dirPath = Path.Combine(rootPath, GetRuntimeVersionDirectoryName());
                Directory.CreateDirectory(dirPath);

                return Path.Combine(dirPath, "AssemblyTable.bin");
            }

            private static string GetRuntimeVersionDirectoryName()
            {
                return MiscHelpers.FormatInvariant("v{0}", Environment.Version.ToString(3));
            }
        }

        #endregion
    }
}
