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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

            LoadNativeLibrary();

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

        private static void LoadNativeLibrary()
        {
            var suffix = Environment.Is64BitProcess ? "x64" : "ia32";
            var fileName = "v8-" + suffix + ".dll";
            var messageBuilder = new StringBuilder();

            var paths = GetDirPaths().Select(dirPath => Path.Combine(dirPath, deploymentDirName, fileName)).Distinct();
            foreach (var path in paths)
            {
                var hLibrary = NativeMethods.LoadLibraryW(path);
                if (hLibrary != IntPtr.Zero)
                {
                    return;
                }

                var exception = new Win32Exception(Marshal.GetLastWin32Error());
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
