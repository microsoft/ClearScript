// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy : IDisposable
    {
        private static readonly object dataLock = new object();

        private static IntPtr hNativeAssembly;
        private static ulong splitImplCount;

        internal static bool OnEntityHolderCreated()
        {
            lock (dataLock)
            {
                if (hNativeAssembly == IntPtr.Zero)
                {
                    hNativeAssembly = LoadNativeAssembly();
                    InitializeICU();
                }

                ++splitImplCount;
                return true;
            }
        }

        internal static void OnEntityHolderDestroyed()
        {
            lock (dataLock)
            {
                if (--splitImplCount < 1)
                {
                    FreeLibrary(hNativeAssembly);
                    hNativeAssembly = IntPtr.Zero;
                }
            }
        }

        private static IntPtr LoadNativeAssembly()
        {
            string platform;
            string architecture;
            string extension;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = "win";
                extension = "dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = "linux";
                extension = "so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = "osx";
                extension = "dylib";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform");
            }

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                architecture = "x64";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                architecture = "x86";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
            {
                architecture = "arm";
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                architecture = "arm64";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported process architecture");
            }

            return LoadNativeLibrary("ClearScriptV8", platform, architecture, extension);
        }

        private static IntPtr LoadNativeLibrary(string baseName, string platform, string architecture, string extension)
        {
            var fileName = $"{baseName}.{platform}-{architecture}.{extension}";

            IntPtr hLibrary;
            var messageBuilder = new StringBuilder();

            var paths = GetDirPaths(platform, architecture).Select(dirPath => Path.Combine(dirPath, deploymentDirName, fileName)).Distinct();
            foreach (var path in paths)
            {
                hLibrary = LoadLibrary(path);
                if (hLibrary != IntPtr.Zero)
                {
                    return hLibrary;
                }

                messageBuilder.AppendInvariant("\n{0}: {1}", path, MiscHelpers.EnsureNonBlank(GetLoadLibraryErrorMessage(), "Unknown error"));
            }

            if (string.IsNullOrEmpty(deploymentDirName))
            {
                var systemPath = Path.Combine(Environment.SystemDirectory, fileName);
                hLibrary = LoadLibrary(systemPath);
                if (hLibrary != IntPtr.Zero)
                {
                    return hLibrary;
                }

                messageBuilder.AppendInvariant("\n{0}: {1}", systemPath, MiscHelpers.EnsureNonBlank(GetLoadLibraryErrorMessage(), "Unknown error"));
            }

            var message = MiscHelpers.FormatInvariant("Cannot load ClearScript V8 library. Load failure information for {0}:{1}", fileName, messageBuilder);
            throw new TypeLoadException(message);
        }

        private static void InitializeICU()
        {
            var paths = GetDirPaths(null, null).Select(dirPath => Path.Combine(dirPath, deploymentDirName, "ClearScriptV8.ICU.dat")).Distinct();
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    V8SplitProxyNative.InvokeNoThrow(instance => instance.V8Environment_InitializeICU(path));
                    return;
                }
            }
        }

        private static IEnumerable<string> GetDirPaths(string platform, string architecture)
        {
            // The assembly location may be empty if the host preloaded the assembly
            // from custom storage. Support for this scenario was requested on CodePlex.

            var location = typeof(V8Proxy).Assembly.Location;
            if (!string.IsNullOrWhiteSpace(location))
            {
                if ((platform != null) && (architecture != null))
                {
                    yield return Path.Combine(Path.GetDirectoryName(location), "runtimes", $"{platform}-{architecture}", "native");
                }

                yield return Path.GetDirectoryName(location);
            }

            var appDomain = AppDomain.CurrentDomain;
            yield return appDomain.BaseDirectory;

            var searchPath = appDomain.RelativeSearchPath;
            if (!string.IsNullOrWhiteSpace(searchPath))
            {
                foreach (var dirPath in searchPath.SplitSearchPath())
                {
                    yield return dirPath;
                }
            }
        }

        #region IDisposable implementation (abstract)

        public abstract void Dispose();

        #endregion

        #region unit test support

        private static string deploymentDirName = string.Empty;

        internal static void RunWithDeploymentDir(string name, Action action)
        {
            deploymentDirName = name;
            try
            {
                action();
            }
            finally
            {
                deploymentDirName = string.Empty;
            }
        }

        #endregion
    }
}
