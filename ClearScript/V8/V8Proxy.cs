// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                    V8SplitProxyNative.InvokeNoThrow(instance => instance.V8SplitProxyManaged_SetMethodTable(V8SplitProxyManaged.MethodTable));
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

        private static IntPtr LoadNativeLibrary(string baseFileName, string prefix, string suffix32, string suffix64, string extension)
        {
            var suffix = Environment.Is64BitProcess ? suffix64 : suffix32;
            var fileName = prefix + baseFileName + suffix + extension;
            var messageBuilder = new StringBuilder();

            IntPtr hLibrary;

            var paths = GetDirPaths().Select(dirPath => Path.Combine(dirPath, deploymentDirName, fileName)).Distinct();
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

        private static IEnumerable<string> GetDirPaths()
        {
            // The assembly location may be empty if the host preloaded the assembly
            // from custom storage. Support for this scenario was requested on CodePlex.

            var location = typeof(V8Proxy).Assembly.Location;
            if (!string.IsNullOrWhiteSpace(location))
            {
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
