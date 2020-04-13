using System;
using System.ComponentModel;
using System.IO;
using Microsoft.ClearScript.Util.COM;
using Microsoft.ClearScript.V8;

namespace Microsoft.ClearScript.Util.IJW
{
    internal static class IJWHostLibrary
    {
        public static IntPtr Load()
        {
            IntPtr hLibrary;

            var usingAppPath = false;
            if (!MiscHelpers.Try(out hLibrary, () => Load(MiscHelpers.GetLocalDataRootPath(out usingAppPath))) && !usingAppPath)
            {
                hLibrary = Load(MiscHelpers.GetLocalDataRootPath(AppDomain.CurrentDomain.BaseDirectory));
            }

            return hLibrary;
        }

        private static IntPtr Load(string rootPath)
        {
            var dirPath = Path.Combine(rootPath, "IJW");
            var filePath = Path.Combine(dirPath, "ijwhost.dll");

            if (!File.Exists(filePath))
            {
                var suffix = Environment.Is64BitProcess ? "64" : "32";
                using (var resourceStream = typeof(V8Proxy).Assembly.GetManifestResourceStream("Microsoft.ClearScript.Util.IJW.ijwhost-" + suffix + ".dll"))
                {
                    if (resourceStream == null)
                    {
                        throw new TypeLoadException("Could not access embedded mixed assembly support library");
                    }

                    Directory.CreateDirectory(dirPath);

                    Stream fileStream = null;
                    try
                    {
                        fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                    }
                    catch (IOException exception)
                    {
                        if (exception.HResult != HResult.WIN32_E_FILEEXISTS)
                        {
                            throw;
                        }
                    }

                    if (fileStream != null)
                    {
                        using (fileStream)
                        {
                            resourceStream.CopyTo(fileStream);
                        }
                    }
                }
            }

            var hLibrary =  NativeMethods.LoadLibraryW(filePath);
            if (hLibrary != IntPtr.Zero)
            {
                return hLibrary;
            }

            throw new Win32Exception();
        }
    }
}
