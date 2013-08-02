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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class HostTypeCollectionTest : ClearScriptTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("HostTypeCollection")]
        public void HostTypeCollection_SingleAssembly()
        {
            Test(null, new[] { "mscorlib" }, null, (first, second) => first == second);
        }

        [TestMethod, TestCategory("HostTypeCollection")]
        public void HostTypeCollection_MultiAssembly()
        {
            Test(null, new[] { "mscorlib", "System", "System.Core" }, null, (first, second) => first.FullName == second.FullName);
        }

        [TestMethod, TestCategory("HostTypeCollection")]
        public void HostTypeCollection_SingleAssembly_Filtered()
        {
            Test(null, new[] { "mscorlib" }, ReflectionFilter, (first, second) => first == second);
        }

        [TestMethod, TestCategory("HostTypeCollection")]
        public void HostTypeCollection_MultiAssembly_Filtered()
        {
            Test(null, new[] { "mscorlib", "System", "System.Core" }, ReflectionFilter, (first, second) => first.FullName == second.FullName);
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private static readonly Predicate<Type> defaultFilter = type => true;

        private static bool ReflectionFilter(Type type)
        {
            return (type != typeof(Type)) && !type.FullName.StartsWith("System.Reflection.", StringComparison.Ordinal);
        }

        internal static void Test(HostTypeCollection typeCollection, string[] assemblyNames, Predicate<Type> filter, Func<Type, Type, bool> comparer)
        {
            typeCollection = typeCollection ?? new HostTypeCollection(filter, assemblyNames);
            var allNodes = GetLeafNodes(typeCollection).OrderBy(hostType => hostType.Type.GetLocator());

            var visitedNodes = new SortedDictionary<string, HostType>();
            foreach (var type in GetImportableTypes(assemblyNames, filter))
            {
                var currentType = type;
                var locator = currentType.GetLocator();

                var segments = locator.Split('.');
                var hostType = (HostType)segments.Aggregate((object)typeCollection, (node, segment) => ((PropertyBag)node)[segment]);

                Assert.IsTrue(hostType.Types.All(testType => testType.GetLocator() == locator));
                Assert.AreEqual(1, hostType.Types.Count(testType => comparer(testType, currentType)));
                visitedNodes[locator] = hostType;
            }

            Assert.IsTrue(allNodes.SequenceEqual(visitedNodes.Values));
            if (filter != null)
            {
                Assert.IsTrue(allNodes.All(hostType => hostType.Types.All(type => filter(type))));
            }
        }

        private static IEnumerable<Type> GetImportableTypes(string[] assemblyNames, Predicate<Type> filter)
        {
            var assemblies = assemblyNames.Select(assemblyName => Assembly.Load(AssemblyHelpers.GetFullAssemblyName(assemblyName)));
            var activeFilter = filter ?? defaultFilter;
            return assemblies.SelectMany(assembly => assembly.GetTypes().Where(type => type.IsImportable() && activeFilter(type)));
        }

        private static IEnumerable<HostType> GetLeafNodes(PropertyBag container)
        {
            foreach (var childNode in container.Values)
            {
                var childContainer = childNode as PropertyBag;
                if (childContainer == null)
                {
                    yield return (HostType)childNode;
                }
                else
                {
                    foreach (var leafNode in GetLeafNodes(childContainer))
                    {
                        yield return leafNode;
                    }
                }
            }
        }

        #endregion
    }
}
