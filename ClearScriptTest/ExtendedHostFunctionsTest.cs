// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class ExtendedHostFunctionsTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;
        private ExtendedHostFunctions host;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("host", host = new ExtendedHostFunctions());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_arrType()
        {
            VerifyArrType<Random>();
            VerifyArrType<Random>(3);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_arrType_Enum()
        {
            VerifyArrType<DayOfWeek>();
            VerifyArrType<DayOfWeek>(3);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_arrType_Scalar()
        {
            VerifyArrType<double>();
            VerifyArrType<double>(3);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_arrType_Struct()
        {
            VerifyArrType<DateTime>();
            VerifyArrType<DateTime>(3);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ExtendedHostFunctions_arrType_BadRank()
        {
            VerifyArrType<Random>(0);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_lib_SingleAssembly()
        {
            var assemblyNames = new[] { "mscorlib" };
            HostTypeCollectionTest.Test(host.lib(assemblyNames), assemblyNames, null, (first, second) => first == second);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_lib_MultiAssembly()
        {
            var assemblyNames = new[] { "mscorlib", "System", "System.Core" };
            HostTypeCollectionTest.Test(host.lib(assemblyNames), assemblyNames, null, (first, second) => first.FullName == second.FullName);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_lib_Merge()
        {
            var assemblyNames = new[] { "mscorlib", "System", "System.Core" };
            var typeCollection = assemblyNames.Aggregate(new HostTypeCollection(), (tempTypeCollection, assemblyName) => host.lib(tempTypeCollection, assemblyName));
            HostTypeCollectionTest.Test(typeCollection, assemblyNames, null, (first, second) => first.FullName == second.FullName);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type()
        {
            var hostType = (HostType)host.type("System.Random");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Random), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_Generic()
        {
            var hostType = (HostType)host.type("System.Collections.Generic.Dictionary");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Dictionary<,>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_GenericWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var intHostType = host.type("System.Int32");
            var hostType = (HostType)host.type("System.Collections.Generic.Dictionary", stringHostType, intHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Dictionary<string, int>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_Overloaded()
        {
            const string locator = "System.Action";
            var hostType = (HostType)host.type(locator);
            Assert.IsTrue(hostType.Types.Length > 1);
            Assert.IsTrue(hostType.Types.All(type => type.GetLocator() == locator));
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_OverloadedWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var intHostType = host.type("System.Int32");
            var hostType = (HostType)host.type("System.Func", stringHostType, intHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Func<string, int>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_PartialAssemblyName()
        {
            var hostType = (HostType)host.type("System.Linq.Enumerable", "System.Core");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Enumerable), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_PartialAssemblyName_Generic()
        {
            var hostType = (HostType)host.type("System.Collections.Generic.HashSet", "System.Core");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(HashSet<>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_PartialAssemblyName_GenericWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var hostType = (HostType)host.type("System.Collections.Generic.HashSet", "System.Core", stringHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(HashSet<string>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_PartialAssemblyName_Overloaded()
        {
            const string locator = "System.Action";
            var hostType = (HostType)host.type(locator, "System.Core");
            Assert.IsTrue(hostType.Types.Length > 1);
            Assert.IsTrue(hostType.Types.All(type => type.GetLocator() == locator));
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_PartialAssemblyName_OverloadedWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var intHostType = host.type("System.Int32");
            var hostType = (HostType)host.type("System.Func", "System.Core", stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, intHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Func<string, string, string, string, string, string, string, string, string, int>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FullAssemblyName()
        {
            var hostType = (HostType)host.type("System.Linq.Enumerable", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Enumerable), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FullAssemblyName_Generic()
        {
            var hostType = (HostType)host.type("System.Collections.Generic.HashSet", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(HashSet<>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FullAssemblyName_GenericWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var hostType = (HostType)host.type("System.Collections.Generic.HashSet", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", stringHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(HashSet<string>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FullAssemblyName_Overloaded()
        {
            const string locator = "System.Action";
            var hostType = (HostType)host.type(locator, "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.IsTrue(hostType.Types.Length > 1);
            Assert.IsTrue(hostType.Types.All(type => type.GetLocator() == locator));
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FullAssemblyName_OverloadedWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var intHostType = host.type("System.Int32");
            var hostType = (HostType)host.type("System.Func", "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, stringHostType, intHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(Func<string, string, string, string, string, string, string, string, string, int>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem()
        {
            var hostType = (HostType)host.type("Microsoft.ClearScript.ScriptEngine");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(ScriptEngine), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem_Generic()
        {
            var hostType = (HostType)host.type("Microsoft.ClearScript.OutArg");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(OutArg<>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem_GenericWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var hostType = (HostType)host.type("Microsoft.ClearScript.OutArg", stringHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(OutArg<string>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem_PartialAssemblyName()
        {
            var hostType = (HostType)host.type("Microsoft.ClearScript.ScriptEngine", "ClearScript");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(ScriptEngine), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem_PartialAssemblyName_Generic()
        {
            var hostType = (HostType)host.type("Microsoft.ClearScript.OutArg", "ClearScript");
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(OutArg<>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_NonSystem_PartialAssemblyName_GenericWithTypeArgs()
        {
            var stringHostType = host.type("System.String");
            var hostType = (HostType)host.type("Microsoft.ClearScript.OutArg", "ClearScript", stringHostType);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(typeof(OutArg<string>), hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        public void ExtendedHostFunctions_type_FromType()
        {
            var type = typeof(Dictionary<string, int>);
            var hostType = (HostType)host.type(type);
            Assert.AreEqual(1, hostType.Types.Length);
            Assert.AreEqual(type, hostType.Type);
        }

        [TestMethod, TestCategory("ExtendedHostFunctions")]
        [ExpectedException(typeof(TypeLoadException))]
        public void ExtendedHostFunctions_type_BadAssemblyName()
        {
            host.type("Microsoft.ClearScript.ScriptEngine", "Bogus");
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private void VerifyArrType<T>()
        {
            var hostType = (HostType)host.arrType<T>();
            Assert.AreEqual(1, hostType.Types.Length);

            var type = hostType.Types[0];
            Assert.IsTrue(type.IsArray);
            Assert.AreEqual(1, type.GetArrayRank());
        }

        private void VerifyArrType<T>(int rank)
        {
            var hostType = (HostType)host.arrType<T>(rank);
            Assert.AreEqual(1, hostType.Types.Length);

            var type = hostType.Types[0];
            Assert.IsTrue(type.IsArray);
            Assert.AreEqual(rank, type.GetArrayRank());
        }

        #endregion
    }
}
