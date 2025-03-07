// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.ClearScript.Properties;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8.ICUData;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8
{
    internal abstract partial class V8Proxy : IDisposable
    {
        private static readonly object dataLock = new();

        private static IntPtr hNativeAssembly;
        private static ulong splitImplCount;
        private static bool triedToLoadNativeAssembly;
        private static bool loadedNativeAssembly;

        internal static void OnEntityHolderCreated()
        {
            lock (dataLock)
            {
                if (++splitImplCount == 1)
                {
                    V8SplitProxyManaged.Initialize();
                }

                if (!triedToLoadNativeAssembly)
                {
                    triedToLoadNativeAssembly = true;

                    var nativeVersion = string.Empty;
                    var gotNativeVersion = false;

                    try
                    {
                        hNativeAssembly = LoadNativeAssembly();
                        loadedNativeAssembly = true;
                    }
                    catch
                    {
                        gotNativeVersion = MiscHelpers.Try(out nativeVersion, static () => V8SplitProxyNative.GetVersion());
                        if (!gotNativeVersion)
                        {
                            throw;
                        }
                    }

                    if (!gotNativeVersion)
                    {
                        nativeVersion = V8SplitProxyNative.GetVersion();
                    }

                    if (nativeVersion != ClearScriptVersion.Informational)
                    {
                        throw new InvalidOperationException($"V8 native assembly: loaded version {nativeVersion} does not match required version {ClearScriptVersion.Informational}");
                    }

                    InitializeICU();
                }
            }
        }

        internal static void OnEntityHolderDestroyed()
        {
            lock (dataLock)
            {
                if (--splitImplCount < 1)
                {
                    V8SplitProxyManaged.Teardown();

                    if (loadedNativeAssembly)
                    {
                        FreeLibrary(hNativeAssembly);
                        hNativeAssembly = IntPtr.Zero;
                        loadedNativeAssembly = false;
                    }
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
            var messageBuilder = new StringBuilder();

            var paths = GetDirPaths(platform, architecture).Select(dirPath => Path.Combine(dirPath, fileName)).Distinct();
            foreach (var path in paths)
            {
                var hLibrary = LoadLibrary(path);
                if (hLibrary != IntPtr.Zero)
                {
                    return hLibrary;
                }

                messageBuilder.AppendInvariant("\n{0}: {1}", path, GetLoadLibraryErrorMessage().ToNonBlank("Unknown error"));
            }

            var message = MiscHelpers.FormatInvariant("Cannot load ClearScript V8 library. Load failure information for {0}:{1}", fileName, messageBuilder);
            throw new TypeLoadException(message);
        }

        private static unsafe void InitializeICU()
        {
            using (var stream = typeof(V8ICUData).Assembly.GetManifestResourceStream(V8ICUData.ResourceName))
            {
                var bytes = new byte[stream.Length];

                var length = stream.Read(bytes, 0, bytes.Length);
                Debug.Assert(length == bytes.Length);

                fixed (byte* pBytes = bytes)
                {
                    var pICUData = (IntPtr)pBytes;
                    V8SplitProxyNative.InvokeNoThrow(static (instance, ctx) => instance.V8Environment_InitializeICU(ctx.pICUData, Convert.ToUInt32(ctx.length)), (pICUData, length));
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
                if ((platform is not null) && (architecture is not null))
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

            searchPath = HostSettings.AuxiliarySearchPath;
            if (!string.IsNullOrWhiteSpace(searchPath))
            {
                foreach (var dirPath in searchPath.SplitSearchPath())
                {
                    yield return dirPath;
                }
            }
        }

        #region IDisposable implementation

        public abstract void Dispose();

        #endregion
    }
}
