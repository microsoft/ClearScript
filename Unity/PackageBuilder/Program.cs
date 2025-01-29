using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

try
{
    string srcPath = @"..\..\..\..\..\ClearScript";
    string dstPath = @"..\..\..\..\Package\Runtime";

    foreach (string file in Directory.GetFiles(dstPath, "*.cs", SearchOption.AllDirectories))
    {
        File.Delete(file);
    }

    foreach (string file in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
    {
        if (file.Contains(@"\Windows\")
            || file.Contains(@"\AssemblyInfo.") && !file.Contains(".Core.")
            || file.Contains(@"\ICUData\")
            || file.Contains(".Net5.")
            || file.Contains(".NetCore.")
            || file.Contains(".NetFramework.")
            || file.Contains(".UWP.")
            || file.Contains(".Windows."))
        {
            continue;
        }

        string dstFile = string.Concat(dstPath, file.AsSpan(srcPath.Length));
        Directory.CreateDirectory(Path.GetDirectoryName(dstFile)!);

        if (file.EndsWith("V8SplitProxyManaged.cs"))
        {
            using var reader = new StreamReader(file);
            using var writer = new StreamWriter(dstFile);
            writer.NewLine = "\n";
            var methods = new Dictionary<string, string>();

            while (true)
            {
                string? line = reader.ReadLine();

                if (line == null)
                    break;

                {
                    Match match = Regex.Match(line, @"\bGetMethodPair<(\w+)>\((\w+)\)");

                    if (match.Success)
                    {
                        string delegateType = match.Groups[1].Value;
                        string method = match.Groups[2].Value;
                        methods.Add(method, delegateType);
                    }
                }

                {
                    Match match = Regex.Match(line, @"^(\s*)private static \w+ (\w+)\([^)]*\)");

                    if (match.Success)
                    {
                        string method = match.Groups[2].Value;

                        if (methods.TryGetValue(method, out string? delegateType))
                        {
                            writer.Write(match.Groups[1].Value);
                            writer.Write("[AOT.MonoPInvokeCallback(typeof(");
                            writer.Write(delegateType);
                            writer.WriteLine("))]");
                        }
                    }
                }

                writer.WriteLine(line);
            }
        }
        else
        {
            File.Copy(file, dstFile);
        }
    }

    foreach (string metaFile in Directory.GetFiles(dstPath, "*.meta", SearchOption.AllDirectories))
    {
        string fileOrFolder = metaFile[..^".meta".Length];

        if (File.Exists(fileOrFolder))
            continue;

        if (Directory.Exists(fileOrFolder))
        {
            string[] files = Directory.GetFiles(fileOrFolder, "", SearchOption.AllDirectories);

            if (files.Any(i => !i.EndsWith(".meta")))
                continue;

            // Delete meta files of directories that contain nothing but meta files. These directories
            // will then become empty and eligible for deletion in the next step.
        }

        File.Delete(metaFile);
    }

    foreach (string folder in Directory.GetDirectories(dstPath, "", SearchOption.AllDirectories))
    {
        try
        {
            if (Directory.GetFiles(folder, "", SearchOption.AllDirectories).Length == 0)
                Directory.Delete(folder, true);
        }
        catch (DirectoryNotFoundException) { }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Console.ReadKey(true);
    throw;
}
