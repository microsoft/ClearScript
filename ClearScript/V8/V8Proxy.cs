// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8Proxy : IDisposable
    {
        private static readonly object mapLock = new object();
        private static readonly Dictionary<Type, Type> map = new Dictionary<Type, Type>();
        private static Assembly assembly;

        protected static T CreateImpl<T>(params object[] args) where T : V8Proxy
        {
            Type implType;
            lock (mapLock)
            {
                var type = typeof(T);
                if (!map.TryGetValue(type, out implType))
                {
                    implType = GetImplType(type);
                    map.Add(type, implType);
                }
            }

            return (T)Activator.CreateInstance(implType, args);
        }

        private static Type GetImplType(Type type)
        {
            var name = type.GetFullRootName();

            var implType = GetAssembly().GetType(name + "Impl");
            if (implType == null)
            {
                throw new TypeLoadException("Cannot find " + name + " implementation type in V8 interface assembly.");
            }

            return implType;
        }

        private static Assembly GetAssembly()
        {
            if (assembly == null)
            {
                assembly = LoadAssembly();
            }

            return assembly;
        }

        private static Assembly LoadAssembly()
        {
            try
            {
                return Assembly.Load("ClearScriptV8");
            }
            catch (FileNotFoundException)
            {
            }

            var hBaseLibrary = LoadNativeLibrary("v8-base");
            try
            {
                var hLibrary = LoadNativeLibrary("v8");
                try
                {
                    var suffix = Environment.Is64BitProcess ? "64" : "32";
                    var fileName = "ClearScriptV8-" + suffix + ".dll";
                    var messageBuilder = new StringBuilder();

                    var paths = GetDirPaths().Select(dirPath => Path.Combine(dirPath, deploymentDirName, fileName)).Distinct();
                    foreach (var path in paths)
                    {
                        try
                        {
                            return Assembly.LoadFrom(path);
                        }
                        catch (Exception exception)
                        {
                            messageBuilder.AppendInvariant("\n{0}: {1}", path, MiscHelpers.EnsureNonBlank(exception.Message, "Unknown error"));
                        }
                    }

                    var message = MiscHelpers.FormatInvariant("Cannot load V8 interface assembly. Load failure information for {0}:{1}", fileName, messageBuilder);
                    throw new TypeLoadException(message);
                }
                finally
                {
                    NativeMethods.FreeLibrary(hLibrary);
                }
            }
            finally
            {
                NativeMethods.FreeLibrary(hBaseLibrary);
            }
        }

        private static IntPtr LoadNativeLibrary(string baseFileName)
        {
            var suffix = Environment.Is64BitProcess ? "-x64" : "-ia32";
            var fileName = baseFileName + suffix + ".dll";
            var messageBuilder = new StringBuilder();

            var paths = GetDirPaths().Select(dirPath => Path.Combine(dirPath, deploymentDirName, fileName)).Distinct();
            foreach (var path in paths)
            {
                var hLibrary = NativeMethods.LoadLibraryW(path);
                if (hLibrary != IntPtr.Zero)
                {
                    return hLibrary;
                }

                var exception = new Win32Exception();
                messageBuilder.AppendInvariant("\n{0}: {1}", path, MiscHelpers.EnsureNonBlank(exception.Message, "Unknown error"));
            }

            var message = MiscHelpers.FormatInvariant("Cannot load V8 interface assembly. Load failure information for {0}:{1}", fileName, messageBuilder);
            throw new TypeLoadException(message);
        }

        private static IEnumerable<string> GetDirPaths()
        {
            // The assembly location may be empty if the the host preloaded the assembly
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
                foreach (var dirPath in searchPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
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
            lock (mapLock)
            {
                map.Clear();
                assembly = null;
            }

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
