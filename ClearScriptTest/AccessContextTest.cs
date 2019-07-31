// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript.Util.Test;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    [DeploymentItem("v8-base-x64.dll")]
    [DeploymentItem("v8-base-ia32.dll")]
    [DeploymentItem("v8-libcpp-x64.dll")]
    [DeploymentItem("v8-libcpp-ia32.dll")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class AccessContextTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);

            engine.AddHostType(typeof(AccessContextTestBase));
            engine.AddHostType(typeof(AccessContextTestObject));

            engine.Script.dateTime = DateTime.Now;
            engine.Script.timeSpan = TimeSpan.Zero;
            engine.Script.testBase = new AccessContextTestBase();
            engine.Script.testObject = new AccessContextTestBase();
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

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_Constructors()
        {
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase()"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(123)"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase('foo')"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(dateTime)"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(timeSpan)"));

            engine.AccessContext = GetType();
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase()"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(123)"), typeof(AccessContextTestBase));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase('foo')"));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(dateTime)"), typeof(AccessContextTestBase));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(timeSpan)"));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase()"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(123)"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase('foo')"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(dateTime)"), typeof(AccessContextTestBase));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(timeSpan)"));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase()"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(123)"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase('foo')"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(dateTime)"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(timeSpan)"), typeof(AccessContextTestBase));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase()"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(123)"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase('foo')"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(dateTime)"), typeof(AccessContextTestBase));
            Assert.IsInstanceOfType(engine.Evaluate("new AccessContextTestBase(timeSpan)"), typeof(AccessContextTestBase));

            engine.AccessContext = null;
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase()"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(123)"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase('foo')"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(dateTime)"));
            TestUtil.AssertException<MissingMethodException>(() => engine.Evaluate("new AccessContextTestBase(timeSpan)"));
        }

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_Events()
        {
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(EventSource<EventHandler>));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(EventSource<EventHandler>));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(EventSource<EventHandler>));

            engine.AccessContext = null;
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalEvent"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateEvent"), typeof(Undefined));
        }

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_Fields()
        {
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.privateField"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.IsNull(engine.Evaluate("testBase.PublicField"));
            Assert.IsNull(engine.Evaluate("testBase.InternalField"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedField"), typeof(Undefined));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalField"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.privateField"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsNull(engine.Evaluate("testBase.PublicField"));
            Assert.IsNull(engine.Evaluate("testBase.InternalField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalField"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.privateField"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsNull(engine.Evaluate("testBase.PublicField"));
            Assert.IsNull(engine.Evaluate("testBase.InternalField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalField"));
            Assert.IsNull(engine.Evaluate("testBase.privateField"));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsNull(engine.Evaluate("testBase.PublicField"));
            Assert.IsNull(engine.Evaluate("testBase.InternalField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedField"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalField"));
            Assert.IsNull(engine.Evaluate("testBase.privateField"));

            engine.AccessContext = null;
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalField"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.privateField"), typeof(Undefined));
        }

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_Methods()
        {
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(HostMethod));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(HostMethod));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(HostMethod));

            engine.AccessContext = null;
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalMethod"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateMethod"), typeof(Undefined));
        }

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_Properties()
        {
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateProperty"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.IsNull(engine.Evaluate("testBase.PublicProperty"));
            Assert.IsNull(engine.Evaluate("testBase.InternalProperty"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedProperty"), typeof(Undefined));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalProperty"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateProperty"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsNull(engine.Evaluate("testBase.PublicProperty"));
            Assert.IsNull(engine.Evaluate("testBase.InternalProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalProperty"));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateProperty"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsNull(engine.Evaluate("testBase.PublicProperty"));
            Assert.IsNull(engine.Evaluate("testBase.InternalProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalProperty"));
            Assert.IsNull(engine.Evaluate("testBase.PrivateProperty"));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsNull(engine.Evaluate("testBase.PublicProperty"));
            Assert.IsNull(engine.Evaluate("testBase.InternalProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedProperty"));
            Assert.IsNull(engine.Evaluate("testBase.ProtectedInternalProperty"));
            Assert.IsNull(engine.Evaluate("testBase.PrivateProperty"));

            engine.AccessContext = null;
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PublicProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.InternalProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.ProtectedInternalProperty"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("testBase.PrivateProperty"), typeof(Undefined));
        }

        [TestMethod, TestCategory("AccessContest")]
        public void AccessContext_NestedTypes()
        {
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(Undefined));

            engine.AccessContext = GetType();
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestObject);
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(Undefined));

            engine.AccessContext = typeof(AccessContextTestBase);
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(HostType));

            engine.AccessContext = typeof(AccessContextTestBase.PublicNestedType);
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(HostType));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(HostType));

            engine.AccessContext = null;
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PublicNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.InternalNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.ProtectedInternalNestedType"), typeof(Undefined));
            Assert.IsInstanceOfType(engine.Evaluate("AccessContextTestBase.PrivateNestedType"), typeof(Undefined));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
