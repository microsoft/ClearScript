using System;
using System.IO;

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
        File.Copy(file, dstFile);
    }

    foreach (string file in Directory.GetFiles(dstPath, "*.meta", SearchOption.AllDirectories))
    {
        if (!File.Exists(file[..^".meta".Length]))
            File.Delete(file);
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
