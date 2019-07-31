// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;
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
    public class ScriptAccessTest : ClearScriptTest
    {
        #region setup / teardown

        // ReSharper disable NotAccessedField.Local

        private static Guid[] guids;
        private ScriptEngine engine;
        private TestObject testObject;
        private ITestInterface testInterface;
        private BlockedTestObject blockedTestObject;
        private UnblockedTestObject unblockedTestObject;

        // ReSharper restore NotAccessedField.Local

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            guids = Enumerable.Range(0, 16).Select(index => Guid.NewGuid()).ToArray();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.AddHostObject("testObject", testInterface = testObject = new TestObject());
            engine.AddHostObject("testInterface", HostItem.Wrap(engine, testObject, typeof(ITestInterface)));
            engine.AddHostObject("blockedTestObject", blockedTestObject = new BlockedTestObject());
            engine.AddHostObject("unblockedTestObject", unblockedTestObject = new UnblockedTestObject());
            engine.AddHostType(typeof(BlockedTestObject));
            engine.AddHostType(typeof(UnblockedTestObject));
            engine.AddHostType("Guid", typeof(Guid));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            unblockedTestObject = null;
            blockedTestObject = null;
            testInterface = null;
            testObject = null;
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedEvent()
        {
            AssertBlockedMember("testObject", "BlockedEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedBaseEvent()
        {
            AssertBlockedMember("testObject", "BlockedBaseEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedOverriddenEvent()
        {
            AssertBlockedMember("testObject", "BlockedOverriddenEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedInterfaceEvent()
        {
            AssertMember("testObject", "BlockedInterfaceEvent");
            AssertBlockedMember("testInterface", "BlockedInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedExplicitInterfaceEvent()
        {
            AssertBlockedMember("testObject", "BlockedExplicitInterfaceEvent");
            AssertBlockedMember("testInterface", "BlockedExplicitInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedField()
        {
            AssertBlockedMember("testObject", "BlockedField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedBaseField()
        {
            AssertBlockedMember("testObject", "BlockedBaseField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedProperty()
        {
            AssertBlockedMember("testObject", "BlockedProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedBaseProperty()
        {
            AssertBlockedMember("testObject", "BlockedBaseProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedOverriddenProperty()
        {
            AssertBlockedMember("testObject", "BlockedOverriddenProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedInterfaceProperty()
        {
            AssertMember("testObject", "BlockedInterfaceProperty");
            AssertBlockedMember("testInterface", "BlockedInterfaceProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedExplicitInterfaceProperty()
        {
            AssertBlockedMember("testObject", "BlockedExplicitInterfaceEvent");
            AssertBlockedMember("testInterface", "BlockedExplicitInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedMethod()
        {
            AssertBlockedMember("testObject", "BlockedMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedBaseMethod()
        {
            AssertBlockedMember("testObject", "BlockedBaseMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedOverriddenMethod()
        {
            AssertBlockedMember("testObject", "BlockedOverriddenMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedInterfaceMethod()
        {
            AssertMember("testObject", "BlockedInterfaceMethod");
            AssertBlockedMember("testInterface", "BlockedInterfaceMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedExplicitInterfaceMethod()
        {
            AssertBlockedMember("testObject", "BlockedExplicitInterfaceMethod");
            AssertBlockedMember("testInterface", "BlockedExplicitInterfaceMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_BlockedOverloadedMethod()
        {
            // the property exists due to the overload, but invocation fails because the bound method is blocked
            Assert.AreNotSame(Undefined.Value, engine.Evaluate("testObject.BlockedOverloadedMethod"));
            TestUtil.AssertException<MissingMemberException>(() => engine.Execute("testObject.BlockedOverloadedMethod('foo')"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_ReadOnlyField_Read()
        {
            AssertMember("testObject", "ReadOnlyField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_ReadOnlyField_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.ReadOnlyField = 7"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_ReadOnlyProperty_Read()
        {
            AssertMember("testObject", "ReadOnlyProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_ReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.ReadOnlyProperty = Guid.NewGuid()"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedEvent()
        {
            AssertRenamedMember("testObject", "RenamedEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedBaseEvent()
        {
            AssertRenamedMember("testObject", "RenamedBaseEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedOverriddenEvent()
        {
            AssertRenamedMember("testObject", "RenamedOverriddenEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedInterfaceEvent()
        {
            AssertMember("testObject", "RenamedInterfaceEvent");
            AssertRenamedMember("testInterface", "RenamedInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedExplicitInterfaceEvent()
        {
            AssertBlockedMember("testObject", "RenamedExplicitInterfaceEvent");
            AssertRenamedMember("testInterface", "RenamedExplicitInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedField()
        {
            AssertRenamedMember("testObject", "RenamedField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedBaseField()
        {
            AssertRenamedMember("testObject", "RenamedBaseField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedProperty()
        {
            AssertRenamedMember("testObject", "RenamedProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedBaseProperty()
        {
            AssertRenamedMember("testObject", "RenamedBaseProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedOverriddenProperty()
        {
            AssertRenamedMember("testObject", "RenamedOverriddenProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedInterfaceProperty()
        {
            AssertMember("testObject", "RenamedInterfaceProperty");
            AssertRenamedMember("testInterface", "RenamedInterfaceProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedExplicitInterfaceProperty()
        {
            AssertBlockedMember("testObject", "RenamedExplicitInterfaceEvent");
            AssertRenamedMember("testInterface", "RenamedExplicitInterfaceEvent");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedMethod()
        {
            AssertRenamedMember("testObject", "RenamedMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedBaseMethod()
        {
            AssertRenamedMember("testObject", "RenamedBaseMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedOverriddenMethod()
        {
            AssertRenamedMember("testObject", "RenamedOverriddenMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedInterfaceMethod()
        {
            AssertMember("testObject", "RenamedInterfaceMethod");
            AssertRenamedMember("testInterface", "RenamedInterfaceMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedExplicitInterfaceMethod()
        {
            AssertBlockedMember("testObject", "RenamedExplicitInterfaceMethod");
            AssertRenamedMember("testInterface", "RenamedExplicitInterfaceMethod");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedOverloadedMethod()
        {
            // the property exists due to the overload, but invocation fails because the bound method is renamed
            Assert.AreNotSame(Undefined.Value, engine.Evaluate("testObject.RenamedOverloadedMethod"));
            TestUtil.AssertException<MissingMemberException>(() => engine.Execute("testObject.RenamedOverloadedMethod('foo')"));

            // the renamed property should work
            Assert.AreEqual(testObject.RenamedOverloadedMethod("blah"), engine.Evaluate("testObject.renamedOverloadedMethod('blah')"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedReadOnlyField_Read()
        {
            AssertRenamedMember("testObject", "RenamedReadOnlyField");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedReadOnlyField_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.renamedReadOnlyField = 7"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedReadOnlyProperty_Read()
        {
            AssertRenamedMember("testObject", "RenamedReadOnlyProperty");
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_RenamedReadOnlyProperty_Write()
        {
            TestUtil.AssertException<UnauthorizedAccessException>(() => engine.Execute("testObject.renamedReadOnlyProperty = Guid.NewGuid()"));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_ClassLevelBlocking()
        {
            AssertBlockedMember("blockedTestObject", "BlockedMethod");
            AssertMember("blockedTestObject", "UnblockedMethod");
            AssertMember("blockedTestObject", "ToString");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new BlockedTestObject.BlockedNestedType()"));
            Assert.IsInstanceOfType(engine.Evaluate("new BlockedTestObject.UnblockedNestedType()"), typeof(BlockedTestObject.UnblockedNestedType));
        }

        [TestMethod, TestCategory("ScriptAccess")]
        public void ScriptAccess_EngineLevelBlocking()
        {
            AssertBlockedMember("unblockedTestObject", "BlockedMethod");
            AssertMember("unblockedTestObject", "UnblockedMethod");
            AssertMember("unblockedTestObject", "ToString");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new UnblockedTestObject.BlockedNestedType()"));
            Assert.IsInstanceOfType(engine.Evaluate("new UnblockedTestObject.UnblockedNestedType()"), typeof(UnblockedTestObject.UnblockedNestedType));

            engine.DefaultAccess = ScriptAccess.None;

            AssertBlockedMember("unblockedTestObject", "BlockedMethod");
            AssertBlockedMember("unblockedTestObject", "UnblockedMethod");
            AssertBlockedMember("unblockedTestObject", "ToString");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new UnblockedTestObject.BlockedNestedType()"));
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new UnblockedTestObject.UnblockedNestedType()"));

            engine.DefaultAccess = ScriptAccess.Full;

            AssertBlockedMember("unblockedTestObject", "BlockedMethod");
            AssertMember("unblockedTestObject", "UnblockedMethod");
            AssertMember("unblockedTestObject", "ToString");
            TestUtil.AssertException<ScriptEngineException>(() => engine.Evaluate("new UnblockedTestObject.BlockedNestedType()"));
            Assert.IsInstanceOfType(engine.Evaluate("new UnblockedTestObject.UnblockedNestedType()"), typeof(UnblockedTestObject.UnblockedNestedType));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        // ReSharper disable ValueParameterNotUsed

        public class BaseObject
        {
            [NoScriptAccess] public event EventHandler BlockedBaseEvent { add {} remove {} }
            [NoScriptAccess] public virtual event EventHandler BlockedOverriddenEvent { add {} remove {} }

            [NoScriptAccess] public int BlockedBaseField = 1;

            [NoScriptAccess] public Guid BlockedBaseProperty { get { return guids[0]; } set {} }
            [NoScriptAccess] public virtual Guid BlockedOverriddenProperty { get { return guids[1]; } set {} }

            [NoScriptAccess] public double BlockedBaseMethod(object arg) { return TestUtil.CalcTestValue(new Guid("9bf4c32d-1394-4546-a150-eb162b3bbb5f"), "BlockedBaseMethod", arg); }
            [NoScriptAccess] public virtual double BlockedOverriddenMethod(object arg) { return TestUtil.CalcTestValue(new Guid("83aa905a-e17d-47fe-bd0f-d2350cfb92d6"), "BlockedOverriddenMethod", arg); }

            [ScriptMember("renamedBaseEvent")] public event EventHandler RenamedBaseEvent { add {} remove {} }
            [ScriptMember("renamedOverriddenEvent")] public virtual event EventHandler RenamedOverriddenEvent { add {} remove {} }

            [ScriptMember("renamedBaseField")] public int RenamedBaseField = 2;

            [ScriptMember("renamedBaseProperty")] public Guid RenamedBaseProperty { get { return guids[2]; } set {} }
            [ScriptMember("renamedOverriddenProperty")] public virtual Guid RenamedOverriddenProperty { get { return guids[3]; } set {} }

            [ScriptMember("renamedBaseMethod")] public double RenamedBaseMethod(object arg) { return TestUtil.CalcTestValue(new Guid("460c0b65-56e6-4174-9331-905d417df885"), "RenamedBaseMethod", arg); }
            [ScriptMember("renamedOverriddenMethod")] public virtual double RenamedOverriddenMethod(object arg) { return TestUtil.CalcTestValue(new Guid("051b3239-df62-4a14-967c-0291e02ad48f"), "RenamedOverriddenMethod", arg); }
        }

        public interface ITestInterface
        {
            [NoScriptAccess] event EventHandler BlockedInterfaceEvent;
            [NoScriptAccess] event EventHandler BlockedExplicitInterfaceEvent;

            [NoScriptAccess] Guid BlockedInterfaceProperty { get; set; }
            [NoScriptAccess] Guid BlockedExplicitInterfaceProperty { get; set; }

            [NoScriptAccess] double BlockedInterfaceMethod(object arg);
            [NoScriptAccess] double BlockedExplicitInterfaceMethod(object arg);
        
            [ScriptMember("renamedInterfaceEvent")] event EventHandler RenamedInterfaceEvent;
            [ScriptMember("renamedExplicitInterfaceEvent")] event EventHandler RenamedExplicitInterfaceEvent;

            [ScriptMember("renamedInterfaceProperty")] Guid RenamedInterfaceProperty { get; set; }
            [ScriptMember("renamedExplicitInterfaceProperty")] Guid RenamedExplicitInterfaceProperty { get; set; }

            [ScriptMember("renamedInterfaceMethod")] double RenamedInterfaceMethod(object arg);
            [ScriptMember("renamedExplicitInterfaceMethod")] double RenamedExplicitInterfaceMethod(object arg);
        }

        public sealed class TestObject : BaseObject, ITestInterface
        {
            [NoScriptAccess] public event EventHandler BlockedEvent { add {} remove {} }
            public override event EventHandler BlockedOverriddenEvent { add {} remove {} }
            public event EventHandler BlockedInterfaceEvent { add {} remove {} }
            event EventHandler ITestInterface.BlockedExplicitInterfaceEvent { add {} remove {} }

            [NoScriptAccess] public int BlockedField = 3;
            [ScriptMember(ScriptAccess.ReadOnly)] public int ReadOnlyField = 4;

            [NoScriptAccess] public Guid BlockedProperty { get { return guids[4]; } set {} }
            public override Guid BlockedOverriddenProperty { get { return guids[5]; } set {} }
            public Guid BlockedInterfaceProperty { get { return guids[6]; } set {} }
            Guid ITestInterface.BlockedExplicitInterfaceProperty { get { return guids[7]; } set {} }
            [ScriptMember(ScriptAccess.ReadOnly)] public Guid ReadOnlyProperty { get { return guids[8]; } set {} }

            [NoScriptAccess] public double BlockedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("9e890478-709c-4e42-be5f-c4e291572a17"), "BlockedMethod", arg); }
            public override double BlockedOverriddenMethod(object arg) { return TestUtil.CalcTestValue(new Guid("7ff033f5-f6f2-46cf-aba5-5adec6e210fd"), "BlockedOverriddenMethod", arg); }
            public double BlockedInterfaceMethod(object arg) { return TestUtil.CalcTestValue(new Guid("0eb4c085-808d-40ec-9117-214084222b75"), "BlockedInterfaceMethod", arg); }
            double ITestInterface.BlockedExplicitInterfaceMethod(object arg) { return TestUtil.CalcTestValue(new Guid("28318787-2195-46cf-8b57-a993c30b131f"), "BlockedExplicitInterfaceMethod", arg); }
            public double BlockedOverloadedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("47a9e14b-65f6-4202-9e6c-528f05d2fd08"), "BlockedOverloadedMethod 1", arg); }
            [NoScriptAccess] public double BlockedOverloadedMethod<T>(T arg) { return TestUtil.CalcTestValue(new Guid("60e54acc-0b47-4d0a-a29e-b6f420079095"), "BlockedOverloadedMethod 2", arg); }

            [ScriptMember("renamedEvent")] public event EventHandler RenamedEvent { add {} remove {} }
            public override event EventHandler RenamedOverriddenEvent { add {} remove {} }
            public event EventHandler RenamedInterfaceEvent { add {} remove {} }
            event EventHandler ITestInterface.RenamedExplicitInterfaceEvent { add {} remove {} }

            [ScriptMember("renamedField")] public int RenamedField = 5;
            [ScriptMember("renamedReadOnlyField", ScriptAccess.ReadOnly)] public int RenamedReadOnlyField = 6;

            [ScriptMember("renamedProperty")] public Guid RenamedProperty { get { return guids[9]; } set {} }
            public override Guid RenamedOverriddenProperty { get { return guids[10]; } set {} }
            public Guid RenamedInterfaceProperty { get { return guids[11]; } set {} }
            Guid ITestInterface.RenamedExplicitInterfaceProperty { get { return guids[12]; } set {} }
            [ScriptMember("renamedReadOnlyProperty", ScriptAccess.ReadOnly)] public Guid RenamedReadOnlyProperty { get { return guids[13]; } set {} }

            [ScriptMember("renamedMethod")] public double RenamedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("d6c05624-6357-4ef1-b54a-d7a03864a034"), "RenamedMethod", arg); }
            public override double RenamedOverriddenMethod(object arg) { return TestUtil.CalcTestValue(new Guid("77382ed3-feec-4d49-bd6e-d1d590827c35"), "RenamedOverriddenMethod", arg); }
            public double RenamedInterfaceMethod(object arg) { return TestUtil.CalcTestValue(new Guid("d3a7c549-1ee3-4890-b933-6b8ab6988e49"), "RenamedInterfaceMethod", arg); }
            double ITestInterface.RenamedExplicitInterfaceMethod(object arg) { return TestUtil.CalcTestValue(new Guid("d0505e09-e152-40e2-b279-59e980109d9c"), "RenamedExplicitInterfaceMethod", arg); }
            public double RenamedOverloadedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("c3b7cfd5-c870-422e-9bce-c361429c8c74"), "RenamedOverloadedMethod 1", arg); }
            [ScriptMember("renamedOverloadedMethod")] public double RenamedOverloadedMethod<T>(T arg) { return TestUtil.CalcTestValue(new Guid("8b79b6b1-8e54-4fff-9054-bfba76e092b7"), "RenamedOverloadedMethod 2", arg); }
        }

        [NoDefaultScriptAccess]
        public class BlockedTestObject
        {
            public double BlockedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("de3d88bf-a148-4bf2-ab67-b27a7ff6cf21"), "BlockedMethod", arg); }
            [ScriptMember(ScriptAccess.Full)] public double UnblockedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("ab0e94e7-445e-4fcf-8fe1-94a2c0724915"), "UnblockedMethod", arg); }
            public class BlockedNestedType {}
            [ScriptUsage(ScriptAccess.Full)] public class UnblockedNestedType {}
        }

        public class UnblockedTestObject
        {
            [NoScriptAccess] public double BlockedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("de3d88bf-a148-4bf2-ab67-b27a7ff6cf21"), "BlockedMethod", arg); }
            public double UnblockedMethod(object arg) { return TestUtil.CalcTestValue(new Guid("ab0e94e7-445e-4fcf-8fe1-94a2c0724915"), "UnblockedMethod", arg); }
            [NoScriptAccess] public class BlockedNestedType {}
            public class UnblockedNestedType {}
        }

        private void AssertBlockedMember(string objectName, string memberName)
        {
            var field = GetType().GetField(objectName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            Assert.IsNotNull(field.FieldType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(testMember => testMember.GetShortName() == memberName));
            Assert.AreSame(Undefined.Value, engine.Evaluate(objectName + "." + memberName));
        }

        private void AssertMember(string objectName, string memberName)
        {
            AssertMemberInternal(objectName, memberName, memberName);
        }

        private void AssertRenamedMember(string objectName, string memberName)
        {
            Assert.AreSame(Undefined.Value, engine.Evaluate(objectName + "." + memberName));

            var scriptMemberName = memberName.Replace("Renamed", "renamed");
            Assert.AreNotEqual(memberName, scriptMemberName);

            AssertMemberInternal(objectName, memberName, scriptMemberName);
        }

        private void AssertMemberInternal(string objectName, string memberName, string scriptMemberName)
        {
            var field = GetType().GetField(objectName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            var member = field.FieldType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(testMember => testMember.GetShortName() == memberName);
            Assert.IsNotNull(member);

            var target = field.GetValue(this);
            Assert.IsNotNull(target);

            var eventInfo = member as EventInfo;
            if (eventInfo != null)
            {
                var eventSource = engine.Evaluate(objectName + "." + scriptMemberName) as EventSource<EventHandler>;
                Assert.IsNotNull(eventSource);
                // ReSharper disable once PossibleNullReferenceException
                Assert.AreSame(target, eventSource.Source);
                Assert.AreSame(eventInfo, eventSource.EventInfo);
                return;
            }

            field = member as FieldInfo;
            if (field != null)
            {
                Assert.AreEqual(field.GetValue(target), engine.Evaluate(objectName + "." + scriptMemberName));
                return;
            }

            var property = member as PropertyInfo;
            if (property != null)
            {
                Assert.AreEqual(property.GetValue(target, ArrayHelpers.GetEmptyArray<object>()), engine.Evaluate(objectName + "." + scriptMemberName));
                return;
            }

            var method = member as MethodInfo;
            if (method != null)
            {
                switch (method.GetParameters().Length)
                {
                    case 0:
                        Assert.AreEqual(method.Invoke(target, ArrayHelpers.GetEmptyArray<object>()), engine.Evaluate(objectName + "." + scriptMemberName + "()"));
                        break;

                    case 1:
                        Assert.AreEqual(method.Invoke(target, new object[] { 98765 }), engine.Evaluate(objectName + "." + scriptMemberName + "(98765)"));
                        break;

                    default:
                        Assert.Fail("Unsupported method signature");
                        break;
                }

                return;
            }

            Assert.Fail("Unrecognized member");
        }

        // ReSharper restore ValueParameterNotUsed

        #endregion
    }
}
