// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
            // ReSharper disable once PossibleNullReferenceException
            return (type != typeof(Type)) && !type.FullName.StartsWith("System.Reflection.", StringComparison.Ordinal);
        }

        internal static void Test(HostTypeCollection typeCollection, string[] assemblyNames, Predicate<Type> filter, Func<Type, Type, bool> comparer)
        {
            // ReSharper disable CollectionNeverQueried.Local

            typeCollection = typeCollection ?? new HostTypeCollection(filter, assemblyNames);
            var allNodes = GetLeafNodes(typeCollection).OrderBy(hostType => hostType.Type.GetLocator()).ToArray();

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

            // ReSharper restore CollectionNeverQueried.Local
        }

        private static IEnumerable<Type> GetImportableTypes(string[] assemblyNames, Predicate<Type> filter)
        {
            var assemblies = assemblyNames.Select(assemblyName => Assembly.Load(AssemblyTable.GetFullAssemblyName(assemblyName)));
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
