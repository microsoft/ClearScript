// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.V8.FastProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class V8FastProxyTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;
        private TestObject testObject;
        private FastObject fastObject;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            PrepareEngine();
        }

        private void PrepareEngine()
        {
            engine.AddHostType(typeof(Helpers));
            engine.Script.testObject = testObject ?? (testObject = new TestObject());
            engine.Script.fastObject = fastObject ?? (fastObject = new FastObject());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Undefined()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.SetUndefined();
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === undefined")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(Undefined.Value);
            Assert.AreEqual(nameof(Undefined), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === undefined")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set(Undefined.Value);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === undefined")));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Null()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.SetNull();
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === null")));

            engine.NullExportValue = 123;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 123")));

            engine.NullExportValue = 456;
            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(null);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 456")));

            engine.EnableNullResultWrapping = true;
            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(null);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.IsNull(engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Boolean()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(true);
            Assert.IsTrue((bool)engine.Evaluate("testObject.value === true"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<bool>(false);
            Assert.IsTrue((bool)engine.Evaluate("testObject.value === false"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(true);
            Assert.AreEqual(nameof(Boolean), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.IsTrue((bool)engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(false);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.IsFalse((bool)engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Char()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set('q');
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 'q'.charCodeAt(0)")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<char>('r');
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 'r'.charCodeAt(0)")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>('s');
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((int)'s', engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>('t');
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual('t', engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_SByte()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(sbyte.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {sbyte.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<sbyte>(sbyte.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {sbyte.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((sbyte)123);
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(123, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>((sbyte)-123);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((sbyte)-123, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Byte()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(byte.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {byte.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<byte>(byte.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {byte.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((byte)124);
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(124, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>((byte)253);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((byte)253, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Int16()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(short.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {short.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<short>(short.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {short.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((short)1234);
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(1234, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>((short)4321);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((short)4321, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_UInt16()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(ushort.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {ushort.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<ushort>(ushort.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {ushort.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((ushort)2345);
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(2345, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>((ushort)5432);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((ushort)5432, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Int32()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(int.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {int.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<int>(int.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {int.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(12345);
            Assert.AreEqual(nameof(Int32), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(12345, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(54321);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(54321, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_UInt32()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(uint.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {uint.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<uint>(uint.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {uint.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(uint.MaxValue - 1);
            Assert.AreEqual(nameof(Int64), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((long)uint.MaxValue - 1, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(65432U);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(65432U, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Int64()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(-MiscHelpers.MaxInt64InDouble);
            Assert.AreEqual(nameof(Int64), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(-MiscHelpers.MaxInt64InDouble, engine.Evaluate("testObject.value"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.MarshalUnsafeInt64AsBigInt);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set(MiscHelpers.MaxInt64InDouble);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {MiscHelpers.MaxInt64InDouble}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<long>(-MiscHelpers.MaxInt64InDouble);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {-MiscHelpers.MaxInt64InDouble}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set(MiscHelpers.MaxInt64InDouble + 1);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {MiscHelpers.MaxInt64InDouble + 1}n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<long>(-MiscHelpers.MaxInt64InDouble - 1);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {-MiscHelpers.MaxInt64InDouble - 1}n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(654321L);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(654321L, engine.Evaluate("testObject.value"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.MarshalAllInt64AsBigInt);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set(12345678L);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 12345678n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<long>(87654321L);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 87654321n")));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_UInt64()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((ulong)MiscHelpers.MaxInt64InDouble);
            Assert.AreEqual(nameof(Int64), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(MiscHelpers.MaxInt64InDouble, engine.Evaluate("testObject.value"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.MarshalUnsafeInt64AsBigInt);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set((ulong)MiscHelpers.MaxInt64InDouble);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {MiscHelpers.MaxInt64InDouble}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set((ulong)MiscHelpers.MaxInt64InDouble + 1);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {MiscHelpers.MaxInt64InDouble + 1}n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(765432UL);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(765432UL, engine.Evaluate("testObject.value"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.MarshalAllInt64AsBigInt);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set(12345678UL);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 12345678n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<ulong>(87654321UL);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 87654321n")));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Single()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(float.MaxValue);
            Assert.AreEqual((double)float.MaxValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<float>(float.MinValue);
            Assert.AreEqual((double)float.MinValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(123.5F);
            Assert.AreEqual(nameof(Single), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(123.5F, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(321.5F);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(321.5F, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Double()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(double.MaxValue);
            Assert.AreEqual(double.MaxValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<double>(double.MinValue);
            Assert.AreEqual(double.MinValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(456.789D);
            Assert.AreEqual(nameof(Double), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(456.789D, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(987.654D);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(987.654D, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Decimal()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(decimal.MaxValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {decimal.MaxValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<decimal>(decimal.MinValue);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate($"testObject.value === {decimal.MinValue}")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(456.789M);
            Assert.AreEqual(nameof(Double), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(456.789D, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(987.654M);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(987.654M, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_String()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set("bogus");
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 'bogus'")));

            // ReSharper disable once RedundantCast
            testObject.GetProperty = static (in V8FastResult value) => value.Set((string)null);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === null")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<string>("heinous");
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 'heinous'")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<string>(null);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === null")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>("gnarly");
            Assert.AreEqual(nameof(String), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual("gnarly", engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>("bodacious");
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual("bodacious", engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_CharSpan()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set("bogus".AsSpan());
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 'bogus'")));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_DateTime()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set(DateTime.MinValue);
            Assert.AreEqual(DateTime.MinValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<DateTime>(DateTime.MaxValue);
            Assert.AreEqual(DateTime.MaxValue, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(new DateTime(2007, 5, 22, 6, 15, 43, DateTimeKind.Utc));
            Assert.AreEqual(nameof(DateTime), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(new DateTime(2007, 5, 22, 6, 15, 43, DateTimeKind.Utc), engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(new DateTime(1968, 6, 4, 17, 23, 48, DateTimeKind.Utc));
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(new DateTime(1968, 6, 4, 17, 23, 48, DateTimeKind.Utc), engine.Evaluate("testObject.value"));

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableDateTimeConversion);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set(new DateTime(1941, 8, 26, 9, 11, 25, DateTimeKind.Utc));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value.getTime() === new Date(Date.UTC(1941, 7, 26, 9, 11, 25)).getTime()")));
            Assert.AreEqual(new DateTime(1941, 8, 26, 9, 11, 25, DateTimeKind.Utc), engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>(new DateTime(2007, 5, 22, 6, 15, 43, DateTimeKind.Utc));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value.getTime() === new Date(Date.UTC(2007, 4, 22, 6, 15, 43)).getTime()")));
            Assert.AreEqual(new DateTime(2007, 5, 22, 6, 15, 43, DateTimeKind.Utc), engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>(new DateTime(1968, 6, 4, 17, 23, 48, DateTimeKind.Utc));
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual(new DateTime(1968, 6, 4, 17, 23, 48, DateTimeKind.Utc), engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_BigInteger()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set((BigInteger)123);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 123n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<BigInteger>(456);
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value === 456n")));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<object>((BigInteger)789);
            Assert.AreEqual(nameof(BigInteger), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((BigInteger)789, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<IComparable>((BigInteger)987);
            Assert.AreEqual(nameof(IComparable), engine.Evaluate("Helpers.GetTypeName(testObject.value)"));
            Assert.AreEqual((BigInteger)987, engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_V8Object()
        {
            {
                var testValue = engine.Evaluate("({ foo: 123, bar: 'baz' })");
                testObject.GetProperty = (in V8FastResult value) => value.Set(testValue);
                Assert.AreEqual(123, engine.Evaluate("testObject.value.foo"));
                Assert.AreEqual("baz", engine.Evaluate("testObject.value.bar"));

                testObject.GetProperty = (in V8FastResult value) => value.Set<object>(null);
                Assert.IsNull(engine.Evaluate("testObject.value"));
            }

            {
                var testValue = (ScriptObject)engine.Evaluate("({ foo: 456, bar: 'qux' })");
                testObject.GetProperty = (in V8FastResult value) => value.Set(testValue);
                Assert.AreEqual(456, engine.Evaluate("testObject.value.foo"));
                Assert.AreEqual("qux", engine.Evaluate("testObject.value.bar"));

                testObject.GetProperty = (in V8FastResult value) => value.Set<ScriptObject>(null);
                Assert.IsNull(engine.Evaluate("testObject.value"));
            }
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public async Task V8FastProxy_GetProperty_HostObject()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableTaskPromiseConversion);
            PrepareEngine();

            testObject.GetProperty = static (in V8FastResult value) => value.Set(new HostFunctions());
            Assert.IsInstanceOfType<HostFunctions>(engine.Evaluate("testObject.value"));

            testObject.GetProperty = (in V8FastResult value) => value.Set<HostFunctions>(null);
            Assert.IsNull(engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set(Task.FromResult(123));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("testObject.value.constructor.name === 'Promise'")));
            Assert.AreEqual(123, await (Task<object>)engine.Evaluate("testObject.value"));

            testObject.GetProperty = (in V8FastResult value) => value.Set<Task<int>>(null);
            Assert.IsNull(engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_Nullable()
        {
            testObject.GetProperty = static (in V8FastResult value) => value.Set((int?)123);
            Assert.AreEqual(123, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set((int?)null);
            Assert.IsNull(engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<int?>(123);
            Assert.AreEqual(123, engine.Evaluate("testObject.value"));

            testObject.GetProperty = static (in V8FastResult value) => value.Set<int?>(null);
            Assert.IsNull(engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_GetProperty_ResultAlreadySet()
        {
            testObject.GetProperty = static (in V8FastResult value) =>
            {
                value.Set(123);
                value.Set(456);
            };

            TestUtil.AssertException<InvalidOperationException>(() => engine.Evaluate("testObject.value"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Undefined()
        {
            {
                (bool Succeeded, Undefined Value) result = (false, null);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsTrue(value.TryGet(out IComparable _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = 123");
                Assert.IsFalse(result.Succeeded);

                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsFalse(value.IsTruthy);
                    Assert.IsTrue(value.IsFalsy);
                    Assert.IsTrue(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    Assert.IsFalse(value.TryGet(out IComparable _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = undefined");
                Assert.IsTrue(result.Succeeded);
                Assert.IsInstanceOfType(result.Value, typeof(Undefined));

                engine.Script.value = Undefined.Value.ToRestrictedHostObject<object>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.IsInstanceOfType(result.Value, typeof(Undefined));
            }

            {
                (bool Succeeded, object Value) result = (false, null);
                engine.UndefinedImportValue = null;
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsFalse(value.IsTruthy);
                    Assert.IsTrue(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.IsNull;
                };

                engine.Execute("testObject.value = undefined");
                Assert.IsTrue(result.Succeeded);
                Assert.IsNull(result.Value);

                engine.UndefinedImportValue = 123;
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = undefined");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(123, result.Value);
            }
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Null()
        {
            (bool Succeeded, object Value) result = (false, null);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsTruthy);
                Assert.IsTrue(value.IsFalsy);
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<char>(out char _));
                Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                Assert.IsFalse(value.TryGet<byte>(out byte _));
                Assert.IsFalse(value.TryGet<short>(out short _));
                Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                Assert.IsFalse(value.TryGet<int>(out int _));
                Assert.IsFalse(value.TryGet<uint>(out uint _));
                Assert.IsFalse(value.TryGet<long>(out long _));
                Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                Assert.IsFalse(value.TryGet<float>(out float _));
                Assert.IsFalse(value.TryGet<double>(out double _));
                Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                result.Succeeded = value.IsNull;
            };

            engine.Execute("testObject.value = null");
            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(result.Value);

            {
                engine.NullImportValue = Undefined.Value;
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsFalse(value.IsTruthy);
                    Assert.IsTrue(value.IsFalsy);
                    Assert.IsTrue(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    Assert.IsFalse(value.TryGet(out IComparable _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = null");
                Assert.IsTrue(result.Succeeded);
                Assert.IsInstanceOfType(result.Value, typeof(Undefined));

                engine.NullImportValue = 123;
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = null");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(123, result.Value);
            }
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Boolean()
        {
            (bool Succeeded, bool Value) result = (false, false);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<char>(out char _));
                Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                Assert.IsFalse(value.TryGet<byte>(out byte _));
                Assert.IsFalse(value.TryGet<short>(out short _));
                Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                Assert.IsFalse(value.TryGet<int>(out int _));
                Assert.IsFalse(value.TryGet<uint>(out uint _));
                Assert.IsFalse(value.TryGet<long>(out long _));
                Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                Assert.IsFalse(value.TryGet<float>(out float _));
                Assert.IsFalse(value.TryGet<double>(out double _));
                Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                Assert.IsTrue(value.TryGet(out IComparable _));

                result.Succeeded = value.TryGet<bool>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value, value.IsTruthy);
                    Assert.AreEqual(!result.Value, value.IsFalsy);
                }
            };

            engine.Execute("testObject.value = true");
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(result.Value);

            engine.Script.value = false.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Char() => V8FastProxy_SetProperty_Integer(char.MinValue, char.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_SByte() => V8FastProxy_SetProperty_Integer(sbyte.MinValue, sbyte.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Byte() => V8FastProxy_SetProperty_Integer(byte.MinValue, byte.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Int16() => V8FastProxy_SetProperty_Integer(short.MinValue, short.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_UInt16() => V8FastProxy_SetProperty_Integer(ushort.MinValue, ushort.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Int32() => V8FastProxy_SetProperty_Integer(int.MinValue, int.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_UInt32() => V8FastProxy_SetProperty_Integer(uint.MinValue, uint.MaxValue, static value => value);

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Int64()
        {
            (bool Succeeded, long Value) result = (false, 0);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<long>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value != 0, value.IsTruthy);
                    Assert.AreEqual(result.Value == 0, value.IsFalsy);
                }
            };

            engine.Script.value = 0.5D;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -MiscHelpers.MaxInt64InDoubleAsDouble - 1000000;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -MiscHelpers.MaxInt64InDoubleAsDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble + 1000000;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (BigInteger)long.MinValue - 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (BigInteger)long.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            engine.Script.value = (BigInteger)long.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = (BigInteger)long.MaxValue + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = long.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            engine.Script.value = long.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = (-MiscHelpers.MaxInt64InDoubleAsDouble - 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxInt64InDoubleAsDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = (MiscHelpers.MaxInt64InDoubleAsDouble + 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((decimal)long.MinValue - 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((decimal)long.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((decimal)long.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = ((decimal)long.MaxValue + 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)long.MinValue - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)long.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            engine.Script.value = ((BigInteger)long.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = ((BigInteger)long.MaxValue + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_UInt64()
        {
            (bool Succeeded, ulong Value) result = (false, 0);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<ulong>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value != 0, value.IsTruthy);
                    Assert.AreEqual(result.Value == 0, value.IsFalsy);
                }
            };

            engine.Script.value = 0.5D;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -1.0D;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = 0.0D;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((ulong)MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble + 1000000;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = BigInteger.MinusOne;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = BigInteger.Zero;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            engine.Script.value = (BigInteger)ulong.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MaxValue, result.Value);

            engine.Script.value = (BigInteger)ulong.MaxValue + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = 0UL.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            engine.Script.value = ulong.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MaxValue, result.Value);

            engine.Script.value = (-1.0D).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (0.0D).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((ulong)MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = (MiscHelpers.MaxInt64InDoubleAsDouble + 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-1.0M).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (0.0M).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((decimal)ulong.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MaxValue, result.Value);

            engine.Script.value = ((decimal)ulong.MaxValue + 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)ulong.MinValue - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)ulong.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MinValue, result.Value);

            engine.Script.value = ((BigInteger)ulong.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MaxValue, result.Value);

            engine.Script.value = ((BigInteger)ulong.MaxValue + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Single()
        {
            (bool Succeeded, float Value) result = (false, 0);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<float>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual((result.Value != 0) && !float.IsNaN(result.Value), value.IsTruthy);
                    Assert.AreEqual((result.Value == 0) || float.IsNaN(result.Value), value.IsFalsy);
                }
            };

            var bump = 1.0E25D;

            engine.Script.value = float.MinValue - bump;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (double)float.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = (double)float.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = float.MaxValue + bump;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -(BigInteger)MiscHelpers.MaxInt32InSingle - 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -(BigInteger)MiscHelpers.MaxInt32InSingle;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            engine.Script.value = (BigInteger)MiscHelpers.MaxInt32InSingle;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            engine.Script.value = (BigInteger)MiscHelpers.MaxInt32InSingle + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxInt32InSingle - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((long)-MiscHelpers.MaxInt32InSingle).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((uint)MiscHelpers.MaxInt32InSingle).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((ulong)MiscHelpers.MaxInt32InSingle + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((ulong)MiscHelpers.MaxInt32InSingle).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            engine.Script.value = float.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = float.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = decimal.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((float)decimal.MinValue, result.Value);

            engine.Script.value = decimal.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((float)decimal.MaxValue, result.Value);

            engine.Script.value = (float.MinValue - bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((double)float.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = ((double)float.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = (float.MaxValue + bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-(BigInteger)MiscHelpers.MaxInt32InSingle - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-(BigInteger)MiscHelpers.MaxInt32InSingle).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            engine.Script.value = ((BigInteger)MiscHelpers.MaxInt32InSingle).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt32InSingleAsSingle, result.Value);

            engine.Script.value = ((BigInteger)MiscHelpers.MaxInt32InSingle + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Double()
        {
            (bool Succeeded, double Value) result = (false, 0);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<double>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual((result.Value != 0) && !double.IsNaN(result.Value), value.IsTruthy);
                    Assert.AreEqual((result.Value == 0) || double.IsNaN(result.Value), value.IsFalsy);
                }
            };

            var bump = 1.0E25D;

            engine.Script.value = float.MinValue - bump;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue - bump, result.Value);

            engine.Script.value = (double)float.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = (double)float.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = float.MaxValue + bump;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue + bump, result.Value);

            engine.Script.value = -(BigInteger)MiscHelpers.MaxInt64InDouble - 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -(BigInteger)MiscHelpers.MaxInt64InDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            engine.Script.value = (BigInteger)MiscHelpers.MaxInt64InDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            engine.Script.value = (BigInteger)MiscHelpers.MaxInt64InDouble + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxInt64InDouble - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxInt64InDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((ulong)MiscHelpers.MaxInt64InDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            // ReSharper disable once RedundantCast
            engine.Script.value = ((ulong)MiscHelpers.MaxInt64InDouble + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = float.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = float.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = decimal.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((double)decimal.MinValue, result.Value);

            engine.Script.value = decimal.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((double)decimal.MaxValue, result.Value);

            engine.Script.value = (float.MinValue - bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue - bump, result.Value);

            engine.Script.value = ((double)float.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MinValue, result.Value);

            engine.Script.value = ((double)float.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue, result.Value);

            engine.Script.value = (float.MaxValue + bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(float.MaxValue + bump, result.Value);

            engine.Script.value = (-(BigInteger)MiscHelpers.MaxInt64InDouble - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-(BigInteger)MiscHelpers.MaxInt64InDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            engine.Script.value = ((BigInteger)MiscHelpers.MaxInt64InDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDoubleAsDouble, result.Value);

            engine.Script.value = ((BigInteger)MiscHelpers.MaxInt64InDouble + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Decimal()
        {
            (bool Succeeded, decimal Value) result = (false, 0);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<decimal>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value != 0, value.IsTruthy);
                    Assert.AreEqual(result.Value == 0, value.IsFalsy);
                }
            };

            var bump = 1.0E25D;

            engine.Script.value = (double)float.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (double)float.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = double.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = double.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (double)decimal.MinValue + bump;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)((double)decimal.MinValue + bump), result.Value);

            engine.Script.value = (double)decimal.MaxValue - bump;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)((double)decimal.MaxValue - bump), result.Value);

            engine.Script.value = -MiscHelpers.MaxBigIntegerInDecimal - 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -MiscHelpers.MaxBigIntegerInDecimal;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)-MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = MiscHelpers.MaxBigIntegerInDecimal;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = MiscHelpers.MaxBigIntegerInDecimal + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = long.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            engine.Script.value = long.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = ulong.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(ulong.MaxValue, result.Value);

            engine.Script.value = decimal.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(decimal.MinValue, result.Value);

            engine.Script.value = decimal.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(decimal.MaxValue, result.Value);

            engine.Script.value = ((double)float.MinValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((double)float.MaxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = double.MinValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = double.MaxValue.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((double)decimal.MinValue + bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)((double)decimal.MinValue + bump), result.Value);

            engine.Script.value = ((double)decimal.MaxValue - bump).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)((double)decimal.MaxValue - bump), result.Value);

            engine.Script.value = (-MiscHelpers.MaxBigIntegerInDecimal - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxBigIntegerInDecimal).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)-MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = MiscHelpers.MaxBigIntegerInDecimal.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((decimal)MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = (MiscHelpers.MaxBigIntegerInDecimal + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_String()
        {
            (bool Succeeded, string Value) result = (false, null);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<char>(out char _));
                Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                Assert.IsFalse(value.TryGet<byte>(out byte _));
                Assert.IsFalse(value.TryGet<short>(out short _));
                Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                Assert.IsFalse(value.TryGet<int>(out int _));
                Assert.IsFalse(value.TryGet<uint>(out uint _));
                Assert.IsFalse(value.TryGet<long>(out long _));
                Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                Assert.IsFalse(value.TryGet<float>(out float _));
                Assert.IsFalse(value.TryGet<double>(out double _));
                Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                Assert.IsTrue(value.TryGet(out IComparable _));

                result.Succeeded = value.TryGet<string>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(!string.IsNullOrEmpty(result.Value), value.IsTruthy);
                    Assert.AreEqual(string.IsNullOrEmpty(result.Value), value.IsFalsy);
                }
            };

            engine.Execute("testObject.value = 'foo'");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("foo", result.Value);

            engine.Script.value = "bar".ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("bar", result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_CharSpan()
        {
            (bool Succeeded, string Value) result = (false, null);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<char>(out char _));
                Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                Assert.IsFalse(value.TryGet<byte>(out byte _));
                Assert.IsFalse(value.TryGet<short>(out short _));
                Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                Assert.IsFalse(value.TryGet<int>(out int _));
                Assert.IsFalse(value.TryGet<uint>(out uint _));
                Assert.IsFalse(value.TryGet<long>(out long _));
                Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                Assert.IsFalse(value.TryGet<float>(out float _));
                Assert.IsFalse(value.TryGet<double>(out double _));
                Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                Assert.IsTrue(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                Assert.IsTrue(value.TryGet(out IComparable _));

                result.Succeeded = value.TryGet(out ReadOnlySpan<char> span);
                if (result.Succeeded)
                {
                    result.Value = span.ToString();
                    Assert.AreEqual(!string.IsNullOrEmpty(result.Value), value.IsTruthy);
                    Assert.AreEqual(string.IsNullOrEmpty(result.Value), value.IsFalsy);
                }
            };

            engine.Execute("testObject.value = 'foo'");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("foo", result.Value);

            engine.Script.value = "bar".ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual("bar", result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_DateTime()
        {
            (bool Succeeded, DateTime Value) result = (false, default);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<char>(out char _));
                Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                Assert.IsFalse(value.TryGet<byte>(out byte _));
                Assert.IsFalse(value.TryGet<short>(out short _));
                Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                Assert.IsFalse(value.TryGet<int>(out int _));
                Assert.IsFalse(value.TryGet<uint>(out uint _));
                Assert.IsFalse(value.TryGet<long>(out long _));
                Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                Assert.IsFalse(value.TryGet<float>(out float _));
                Assert.IsFalse(value.TryGet<double>(out double _));
                Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                Assert.IsTrue(value.TryGet(out IComparable _));
                result.Succeeded = value.TryGet<DateTime>(out result.Value);
            };

            var value = DateTime.UtcNow;

            engine.Script.value = value;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(value, result.Value);

            engine.Script.value = value.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(value, result.Value);

            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableDateTimeConversion);
            PrepareEngine();

            value = (DateTime)engine.Evaluate("new Date()");

            engine.Script.value = value;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(value, result.Value);

            engine.Script.value = value.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(value, result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_BigInteger()
        {
            (bool Succeeded, BigInteger Value) result = (false, default);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));

                result.Succeeded = value.TryGet<BigInteger>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(!result.Value.IsZero, value.IsTruthy);
                    Assert.AreEqual(result.Value.IsZero, value.IsFalsy);
                }
            };

            engine.Script.value = 0.5D;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -MiscHelpers.MaxInt64InDoubleAsDouble - 1000000;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = -MiscHelpers.MaxInt64InDoubleAsDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble + 1000000;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (BigInteger)long.MinValue - 1;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((BigInteger)long.MinValue - 1, result.Value);

            engine.Script.value = (BigInteger)long.MinValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MinValue, result.Value);

            engine.Script.value = (BigInteger)long.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(long.MaxValue, result.Value);

            engine.Script.value = (BigInteger)long.MaxValue + 1;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual((BigInteger)long.MaxValue + 1, result.Value);

            engine.Script.value = (-MiscHelpers.MaxInt64InDoubleAsDouble - 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxInt64InDoubleAsDouble).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = MiscHelpers.MaxInt64InDoubleAsDouble.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxInt64InDouble, result.Value);

            engine.Script.value = (MiscHelpers.MaxInt64InDoubleAsDouble + 1000000).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (-MiscHelpers.MaxBigIntegerInDecimalAsDecimal).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = MiscHelpers.MaxBigIntegerInDecimalAsDecimal.ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxBigIntegerInDecimal, result.Value);

            engine.Script.value = (-MiscHelpers.MaxBigIntegerInDecimal - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(-MiscHelpers.MaxBigIntegerInDecimal - 1, result.Value);

            engine.Script.value = (MiscHelpers.MaxBigIntegerInDecimal + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(MiscHelpers.MaxBigIntegerInDecimal + 1, result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_V8Object()
        {
            engine.Dispose();
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging | V8ScriptEngineFlags.EnableDateTimeConversion | V8ScriptEngineFlags.EnableTaskPromiseConversion);
            PrepareEngine();

            {
                (bool Succeeded, ScriptObject Value) result = (false, null);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = { foo: 123, bar: 'baz' }");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(123, result.Value["foo"]);
                Assert.AreEqual("baz", result.Value["bar"]);
            }

            {
                (bool Succeeded, DateTime Value) result = (false, default);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.TryGet<DateTime>(out result.Value);
                };

                engine.Execute("testObject.value = new Date()");
                Assert.IsTrue(result.Succeeded);
            }

            {
                (bool Succeeded, Task<object> Value) result = (false, null);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Execute("testObject.value = Promise.resolve({ foo: 123, bar: 'baz' })");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(123, ((ScriptObject)result.Value.Result)["foo"]);
                Assert.AreEqual("baz", ((ScriptObject)result.Value.Result)["bar"]);
            }
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_HostObject()
        {
            {
                (bool Succeeded, HostFunctions Value) result = (false, null);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                var value = new HostFunctions();
                engine.Script.value = value;
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(value, result.Value);
            }

            {
                (bool Succeeded, TimeSpan Value) result = (false, TimeSpan.Zero);
                testObject.SetProperty = (in V8FastArg value) =>
                {
                    Assert.IsTrue(value.IsTruthy);
                    Assert.IsFalse(value.IsFalsy);
                    Assert.IsFalse(value.IsUndefined);
                    Assert.IsFalse(value.IsNull);
                    Assert.IsFalse(value.TryGet(out Undefined _));
                    Assert.IsFalse(value.TryGet<bool>(out bool _));
                    Assert.IsFalse(value.TryGet<char>(out char _));
                    Assert.IsFalse(value.TryGet<sbyte>(out sbyte _));
                    Assert.IsFalse(value.TryGet<byte>(out byte _));
                    Assert.IsFalse(value.TryGet<short>(out short _));
                    Assert.IsFalse(value.TryGet<ushort>(out ushort _));
                    Assert.IsFalse(value.TryGet<int>(out int _));
                    Assert.IsFalse(value.TryGet<uint>(out uint _));
                    Assert.IsFalse(value.TryGet<long>(out long _));
                    Assert.IsFalse(value.TryGet<ulong>(out ulong _));
                    Assert.IsFalse(value.TryGet<float>(out float _));
                    Assert.IsFalse(value.TryGet<double>(out double _));
                    Assert.IsFalse(value.TryGet<decimal>(out decimal _));
                    Assert.IsFalse(value.TryGet<string>(out string _));
                    Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                    Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                    Assert.IsFalse(value.TryGet<BigInteger>(out BigInteger _));
                    result.Succeeded = value.TryGet(out result.Value);
                };

                engine.Script.value = TimeSpan.MaxValue;
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(TimeSpan.MaxValue, result.Value);
            }
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Nullable()
        {
            (bool Succeeded, TimeSpan? Value) result = (false, null);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.TryGet(out bool _));
                Assert.IsFalse(value.TryGet(out char _));
                Assert.IsFalse(value.TryGet(out sbyte _));
                Assert.IsFalse(value.TryGet(out byte _));
                Assert.IsFalse(value.TryGet(out short _));
                Assert.IsFalse(value.TryGet(out ushort _));
                Assert.IsFalse(value.TryGet(out int _));
                Assert.IsFalse(value.TryGet(out uint _));
                Assert.IsFalse(value.TryGet(out long _));
                Assert.IsFalse(value.TryGet(out ulong _));
                Assert.IsFalse(value.TryGet(out float _));
                Assert.IsFalse(value.TryGet(out double _));
                Assert.IsFalse(value.TryGet(out decimal _));
                Assert.IsFalse(value.TryGet(out string _));
                Assert.IsFalse(value.TryGet(out DateTime _));
                Assert.IsFalse(value.TryGet(out BigInteger _));

                result.Succeeded = value.TryGet(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value is not null, value.IsTruthy);
                    Assert.AreEqual(result.Value is null, value.IsFalsy);
                }
            };

            engine.Script.value = default(BindingFlags);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = TimeSpan.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(TimeSpan.MaxValue, result.Value);

            engine.Execute("testObject.value = null");
            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(result.Value);

            engine.Execute("testObject.value = undefined");
            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(result.Value);

            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.TryGet(out bool _));
                Assert.IsFalse(value.TryGet(out char _));
                Assert.IsFalse(value.TryGet(out sbyte _));
                Assert.IsFalse(value.TryGet(out byte _));
                Assert.IsFalse(value.TryGet(out short _));
                Assert.IsFalse(value.TryGet(out ushort _));
                Assert.IsFalse(value.TryGet(out int _));
                Assert.IsFalse(value.TryGet(out uint _));
                Assert.IsFalse(value.TryGet(out long _));
                Assert.IsFalse(value.TryGet(out ulong _));
                Assert.IsFalse(value.TryGet(out float _));
                Assert.IsFalse(value.TryGet(out double _));
                Assert.IsFalse(value.TryGet(out decimal _));
                Assert.IsFalse(value.TryGet(out string _));
                Assert.IsFalse(value.TryGet(out DateTime _));
                Assert.IsFalse(value.TryGet(out BigInteger _));

                result.Succeeded = value.TryGet<TimeSpan?>(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(result.Value is not null, value.IsTruthy);
                    Assert.AreEqual(result.Value is null, value.IsFalsy);
                }
            };

            engine.Script.value = default(BindingFlags);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = TimeSpan.MaxValue;
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(TimeSpan.MaxValue, result.Value);

            engine.Execute("testObject.value = null");
            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(result.Value);

            engine.Execute("testObject.value = undefined");
            Assert.IsTrue(result.Succeeded);
            Assert.IsNull(result.Value);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_SetProperty_Misc()
        {
            testObject.SetProperty = (in V8FastArg value) => Assert.IsTrue(value.GetBoolean());
            engine.Execute("testObject.value = true");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(' ', value.GetChar());
            engine.Execute("testObject.value = 32");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(1, value.GetSByte());
            engine.Execute("testObject.value = 1");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(2, value.GetByte());
            engine.Execute("testObject.value = 2");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(3, value.GetInt16());
            engine.Execute("testObject.value = 3");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(4, value.GetUInt16());
            engine.Execute("testObject.value = 4");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(5, value.GetInt32());
            engine.Execute("testObject.value = 5");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(6U, value.GetUInt32());
            engine.Execute("testObject.value = 6");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(7, value.GetInt64());
            engine.Execute("testObject.value = 7");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(8UL, value.GetUInt64());
            engine.Execute("testObject.value = 8");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(8.125f, value.GetSingle());
            engine.Execute("testObject.value = 8.125");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(9.25d, value.GetDouble());
            engine.Execute("testObject.value = 9.25");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(10.375m, value.GetDecimal());
            engine.Execute("testObject.value = 10.375");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual("blah", value.GetString());
            engine.Execute("testObject.value = 'blah'");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual("shoo", value.GetCharSpan().ToString());
            engine.Execute("testObject.value = 'shoo'");

            var dateTimeValue = DateTime.UtcNow;
            engine.Script.dateTimeValue = dateTimeValue;

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(dateTimeValue, value.GetDateTime());
            engine.Execute("testObject.value = dateTimeValue");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(1234567890, value.GetBigInteger());
            engine.Execute("testObject.value = 1234567890n");

            testObject.SetProperty = (in V8FastArg value) => Assert.AreEqual(123, value.Get<ScriptObject>().GetProperty("foo"));
            engine.Execute("testObject.value = { foo: 123 }");

            testObject.SetProperty = (in V8FastArg value) => Assert.IsNotNull(value.Get<TestObject>());
            engine.Execute("testObject.value = testObject");

            testObject.SetProperty = (in V8FastArg value) => Assert.IsNull(value.GetNullable<int>());
            engine.Execute("testObject.value = null");
            TestUtil.AssertException<ArgumentException>(() => engine.Execute("testObject.value = 'qux'"));
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_Args()
        {
            var tested = false;

            var dateTimeValue = DateTime.UtcNow;
            engine.Script.dateTimeValue = dateTimeValue;

            engine.Script.foo = new V8FastHostFunction(22, (bool asConstructor, in V8FastArgs args, in V8FastResult _) =>
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator

                V8FastHostFunction.VerifyFunctionCall(asConstructor);

                Assert.IsTrue(args.IsUndefined(0) && args.IsFalsy(0));
                Assert.IsTrue(args.IsNull(1) && args.IsFalsy(1));
                Assert.IsTrue(args.TryGet(2, out bool boolValue) && args.IsTruthy(2) && boolValue);
                Assert.IsTrue(args.TryGet(3, out char charValue) && args.IsTruthy(3) && (charValue == ' '));
                Assert.IsTrue(args.TryGet(4, out sbyte sbyteValue) && args.IsTruthy(4) && (sbyteValue == 1));
                Assert.IsTrue(args.TryGet(5, out byte byteValue) && args.IsTruthy(5) && (byteValue == 2));
                Assert.IsTrue(args.TryGet(6, out short shortValue) && args.IsTruthy(6) && (shortValue == 3));
                Assert.IsTrue(args.TryGet(7, out ushort ushortValue) && args.IsTruthy(7) && (ushortValue == 4));
                Assert.IsTrue(args.TryGet(8, out int intValue) && args.IsTruthy(8) && (intValue == 5));
                Assert.IsTrue(args.TryGet(9, out uint uintValue) && args.IsTruthy(9) && (uintValue == 6));
                Assert.IsTrue(args.TryGet(10, out long longValue) && args.IsTruthy(10) && (longValue == 7));
                Assert.IsTrue(args.TryGet(11, out ulong ulongValue) && args.IsTruthy(11) && (ulongValue == 8));
                Assert.IsTrue(args.TryGet(12, out float floatValue) && args.IsTruthy(12) && (floatValue == 8.125f));
                Assert.IsTrue(args.TryGet(13, out double doubleValue) && args.IsTruthy(13) && (doubleValue == 9.25d));
                Assert.IsTrue(args.TryGet(14, out decimal decimalValue) && args.IsTruthy(14) && (decimalValue == 10.375m));
                Assert.IsTrue(args.TryGet(15, out string stringValue) && args.IsTruthy(15) && (stringValue == "blah"));
                Assert.IsTrue(args.TryGet(16, out ReadOnlySpan<char> charSpanValue) && args.IsTruthy(16) && (charSpanValue.ToString() == "shoo"));
                Assert.IsTrue(args.TryGet(17, out DateTime tempDateTimeValue) && args.IsTruthy(17) && (tempDateTimeValue == dateTimeValue));
                Assert.IsTrue(args.TryGet(18, out BigInteger bigIntegerValue) && args.IsTruthy(18) && (bigIntegerValue == 1234567890));
                Assert.IsTrue(args.TryGet(19, out ScriptObject scriptObject) && args.IsTruthy(19) && ((int)scriptObject.GetProperty("foo") == 123));
                Assert.IsTrue(args.TryGet(20, out TestObject _) && args.IsTruthy(20));
                Assert.IsTrue(args.TryGet(21, out int? nullable) && args.IsFalsy(21) && (nullable is null));

                Assert.IsTrue(args.GetBoolean(2));
                Assert.AreEqual(' ', args.GetChar(3));
                Assert.AreEqual(1, args.GetSByte(4));
                Assert.AreEqual(2, args.GetByte(5));
                Assert.AreEqual(3, args.GetInt16(6));
                Assert.AreEqual(4, args.GetUInt16(7));
                Assert.AreEqual(5, args.GetInt32(8));
                Assert.AreEqual(6U, args.GetUInt32(9));
                Assert.AreEqual(7, args.GetInt64(10));
                Assert.AreEqual(8UL, args.GetUInt64(11));
                Assert.AreEqual(8.125f, args.GetSingle(12));
                Assert.AreEqual(9.25d, args.GetDouble(13));
                Assert.AreEqual(10.375m, args.GetDecimal(14));
                Assert.AreEqual("blah", args.GetString(15));
                Assert.AreEqual("shoo", args.GetCharSpan(16).ToString());
                Assert.AreEqual(dateTimeValue, args.GetDateTime(17));
                Assert.AreEqual(1234567890, args.GetBigInteger(18));
                Assert.AreEqual(123, args.Get<ScriptObject>(19).GetProperty("foo"));
                Assert.IsNotNull(args.Get<TestObject>(20));
                Assert.IsNull(args.GetNullable<int>(21));

                var failedAsExpected = false;
                try
                {
                    args.GetString(0);
                }
                catch (ArgumentException)
                {
                    failedAsExpected = true;
                }

                Assert.IsTrue(failedAsExpected);

                tested = true;

                // ReSharper restore CompareOfFloatsByEqualityOperator
            });

            engine.Execute(@"foo(
                undefined, null,
                true,
                32,
                1, 2, 3, 4, 5, 6, 7, 8,
                8.125, 9.25, 10.375,
                'blah', 'shoo',
                dateTimeValue,
                1234567890n,
                { foo: 123 },
                testObject,
                null
            )");

            Assert.IsTrue(tested);
        }

        [TestMethod, TestCategory("V8FastProxy")]
        public void V8FastProxy_FastObject()
        {
            fastObject.IntField = 123;
            fastObject.StringProperty = "foo";

            Assert.AreEqual(123, engine.Evaluate("fastObject.intFieldRO"));
            TestUtil.AssertException<NotSupportedException>(() => engine.Execute("fastObject.intFieldRO = 456"));
            Assert.AreEqual(123, engine.Evaluate("fastObject.intFieldRW"));
            Assert.AreEqual(456, engine.Evaluate("fastObject.intFieldRW = 456"));
            Assert.AreEqual(456, engine.Evaluate("fastObject.intFieldRW"));
            Assert.AreEqual(456, engine.Evaluate("fastObject.intFieldRO"));
            Assert.AreEqual(456, fastObject.IntField);
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("delete fastObject.intFieldRO")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("delete fastObject.intFieldRW")));

            Assert.AreEqual("foo", engine.Evaluate("fastObject.stringPropertyRO"));
            TestUtil.AssertException<NotSupportedException>(() => engine.Execute("fastObject.stringPropertyRO = 'bar'"));
            Assert.AreEqual("foo", engine.Evaluate("fastObject.stringPropertyRW"));
            Assert.AreEqual("bar", engine.Evaluate("fastObject.stringPropertyRW = 'bar'"));
            Assert.AreEqual("bar", engine.Evaluate("fastObject.stringPropertyRW"));
            Assert.AreEqual("bar", engine.Evaluate("fastObject.stringPropertyRO"));
            Assert.AreEqual("bar", fastObject.StringProperty);
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("delete fastObject.stringPropertyRO")));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("delete fastObject.stringPropertyRW")));

            Assert.AreEqual("456 bar 789 baz", engine.Evaluate("fastObject.method(789, 'baz')"));
            Assert.AreEqual("456 bar 987 qux", fastObject.Method(987, "qux".AsSpan()));
            TestUtil.AssertException<NotSupportedException>(() => engine.Execute("fastObject.method = 789"));
            Assert.IsFalse(Convert.ToBoolean(engine.Evaluate("delete fastObject.method")));

            Assert.IsInstanceOfType(engine.Evaluate("fastObject.baz"), typeof(Undefined));
            Assert.AreEqual(987, engine.Evaluate("fastObject.baz = 987"));
            Assert.AreEqual(987, engine.Evaluate("fastObject.baz"));

            Assert.IsInstanceOfType(engine.Evaluate("fastObject[4]"), typeof(Undefined));
            Assert.AreEqual(654, engine.Evaluate("fastObject[4] = 654"));
            Assert.AreEqual(654, engine.Evaluate("fastObject[4]"));

            Assert.AreEqual("4:654 intFieldRO:456 stringPropertyRO:bar baz:987 ", engine.Evaluate(@"
                let x = '';
                for (let i in fastObject) {
                    x += `${i}:${fastObject[i]} `;
                }
                x;
            "));

            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("delete fastObject.baz")));
            Assert.IsInstanceOfType(engine.Evaluate("fastObject.baz"), typeof(Undefined));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("delete fastObject.qux")));
            Assert.IsInstanceOfType(engine.Evaluate("fastObject.qux"), typeof(Undefined));

            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("delete fastObject[4]")));
            Assert.IsInstanceOfType(engine.Evaluate("fastObject[4]"), typeof(Undefined));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("delete fastObject[11]")));
            Assert.IsInstanceOfType(engine.Evaluate("fastObject[11]"), typeof(Undefined));

            Assert.AreEqual("9 8 7 6 5 4 3 2 1 0 ", engine.Evaluate(@"
                x = '';
                for (const i of fastObject) {
                    x += `${i} `;
                }
                x;
            "));

            Assert.AreEqual("a:100 a:101 a:102 a:103 a:104 a:105 a:106 a:107 a:108 a:109 ", engine.Evaluate(@"
                (async function() {
                    let x = '';
                    for await (const i of fastObject) {
                        x += `${i} `;
                    }
                    return x;
                })()
            ").ToTask().Result);
        }

        #endregion

        #region miscellaneous

        private void V8FastProxy_SetProperty_Integer<T>(T minValue, T maxValue, Func<T, double> toDouble) where T : struct
        {
            (bool Succeeded, T Value) result = (false, default);
            testObject.SetProperty = (in V8FastArg value) =>
            {
                Assert.IsFalse(value.IsUndefined);
                Assert.IsFalse(value.IsNull);
                Assert.IsFalse(value.TryGet(out Undefined _));
                Assert.IsFalse(value.TryGet<bool>(out bool _));
                Assert.IsFalse(value.TryGet<string>(out string _));
                Assert.IsFalse(value.TryGet(out ReadOnlySpan<char> _));
                Assert.IsFalse(value.TryGet<DateTime>(out DateTime _));
                Assert.IsTrue(value.TryGet(out IComparable _));

                result.Succeeded = value.TryGet(out result.Value);
                if (result.Succeeded)
                {
                    Assert.AreEqual(toDouble(result.Value) != 0, value.IsTruthy);
                    Assert.AreEqual(toDouble(result.Value) == 0, value.IsFalsy);
                }
            };

            engine.Script.value = toDouble(minValue) - 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = toDouble(minValue);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(minValue, result.Value);

            engine.Script.value = toDouble(maxValue);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(maxValue, result.Value);

            engine.Script.value = toDouble(maxValue) + 1;
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (BigInteger)(toDouble(minValue) - 1);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (BigInteger)toDouble(minValue);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(minValue, result.Value);

            engine.Script.value = (BigInteger)toDouble(maxValue);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(maxValue, result.Value);

            engine.Script.value = (BigInteger)(toDouble(maxValue) + 1);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = (toDouble(minValue) - 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = toDouble(minValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(minValue, result.Value);

            engine.Script.value = toDouble(maxValue).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(maxValue, result.Value);

            engine.Script.value = (toDouble(maxValue) + 1).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((decimal)(toDouble(minValue) - 1)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((decimal)toDouble(minValue)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(minValue, result.Value);

            engine.Script.value = ((decimal)toDouble(maxValue)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(maxValue, result.Value);

            engine.Script.value = ((decimal)(toDouble(maxValue) + 1)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)(toDouble(minValue) - 1)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            engine.Script.value = ((BigInteger)toDouble(minValue)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(minValue, result.Value);

            engine.Script.value = ((BigInteger)toDouble(maxValue)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(maxValue, result.Value);

            engine.Script.value = ((BigInteger)(toDouble(maxValue) + 1)).ToRestrictedHostObject<IComparable>(engine);
            engine.Execute("testObject.value = value");
            Assert.IsFalse(result.Succeeded);

            if (typeof(T) == typeof(char))
            {
                engine.Script.value = Unsafe.As<T, char>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, char>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(sbyte))
            {
                engine.Script.value = Unsafe.As<T, sbyte>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, sbyte>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(byte))
            {
                engine.Script.value = Unsafe.As<T, byte>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, byte>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(short))
            {
                engine.Script.value = Unsafe.As<T, short>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, short>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(ushort))
            {
                engine.Script.value = Unsafe.As<T, ushort>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, ushort>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(int))
            {
                engine.Script.value = Unsafe.As<T, int>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, int>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
            else if (typeof(T) == typeof(uint))
            {
                engine.Script.value = Unsafe.As<T, uint>(ref minValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(minValue, result.Value);

                engine.Script.value = Unsafe.As<T, uint>(ref maxValue).ToRestrictedHostObject<IComparable>(engine);
                engine.Execute("testObject.value = value");
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(maxValue, result.Value);
            }
        }

        public static class Helpers
        {
            public static string GetTypeName<T>(T _) => typeof(T).Name;
        }

        private sealed class TestObject : IV8FastHostObject
        {
            public delegate void RawGetProperty(in V8FastResult value);

            public delegate void RawSetProperty(in V8FastArg value);

            public RawGetProperty GetProperty { get; set; }

            public RawSetProperty SetProperty { get; set; }

            #region IV8FastHostObject implementation

            private static readonly Operations operations = new();

            IV8FastHostObjectOperations IV8FastHostObject.Operations => operations;

            #endregion

            #region Nested type: Operations

            private sealed class Operations : IV8FastHostObjectOperations
            {
                string IV8FastHostObjectOperations.GetFriendlyName(IV8FastHostObject instance) => null;

                void IV8FastHostObjectOperations.GetProperty(IV8FastHostObject instance, string name, in V8FastResult value, out bool isCacheable)
                {
                    isCacheable = false;
                    ((TestObject)instance).GetProperty?.Invoke(value);
                }

                void IV8FastHostObjectOperations.SetProperty(IV8FastHostObject instance, string name, in V8FastArg value)
                {
                    ((TestObject)instance).SetProperty?.Invoke(value);
                }

                V8FastHostPropertyFlags IV8FastHostObjectOperations.QueryProperty(IV8FastHostObject instance, string name)
                {
                    throw new NotImplementedException();
                }

                bool IV8FastHostObjectOperations.DeleteProperty(IV8FastHostObject instance, string name)
                {
                    throw new NotImplementedException();
                }

                IEnumerable<string> IV8FastHostObjectOperations.GetPropertyNames(IV8FastHostObject instance)
                {
                    throw new NotImplementedException();
                }

                void IV8FastHostObjectOperations.GetProperty(IV8FastHostObject instance, int index, in V8FastResult value)
                {
                    throw new NotImplementedException();
                }

                void IV8FastHostObjectOperations.SetProperty(IV8FastHostObject instance, int index, in V8FastArg value)
                {
                    throw new NotImplementedException();
                }

                V8FastHostPropertyFlags IV8FastHostObjectOperations.QueryProperty(IV8FastHostObject instance, int index)
                {
                    throw new NotImplementedException();
                }

                bool IV8FastHostObjectOperations.DeleteProperty(IV8FastHostObject instance, int index)
                {
                    throw new NotImplementedException();
                }

                IEnumerable<int> IV8FastHostObjectOperations.GetPropertyIndices(IV8FastHostObject instance)
                {
                    throw new NotImplementedException();
                }

                IV8FastEnumerator IV8FastHostObjectOperations.CreateEnumerator(IV8FastHostObject instance)
                {
                    throw new NotImplementedException();
                }

                IV8FastAsyncEnumerator IV8FastHostObjectOperations.CreateAsyncEnumerator(IV8FastHostObject instance)
                {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }

        private sealed class FastObject : V8FastHostDynamicObject<FastObject>
        {
            public int IntField;

            public string StringProperty { get; set; }

            private readonly int[] contents = Enumerable.Range(0, 10).Reverse().ToArray();

            private readonly string[] asyncContents = Enumerable.Range(100, 10).Select(static value => $"a:{value}").ToArray();

            public string Method(BigInteger intArg, in ReadOnlySpan<char> stringArg) => $"{IntField} {StringProperty} {intArg} {stringArg.ToString()}";

            static FastObject()
            {
                Configure(static configuration =>
                {
                    configuration.AddPropertyGetter("intFieldRO", static instance => ref instance.IntField, V8FastHostPropertyFlags.Enumerable);
                    configuration.AddPropertyAccessors("intFieldRW", static instance => ref instance.IntField);

                    configuration.AddPropertyGetter("stringPropertyRO",
                        static (FastObject instance, in V8FastResult value) => value.Set(instance.StringProperty), V8FastHostPropertyFlags.Enumerable);
                    configuration.AddPropertyAccessors("stringPropertyRW",
                        static (FastObject instance, in V8FastResult value) => value.Set(instance.StringProperty),
                        static (FastObject instance, in V8FastArg value) => instance.StringProperty = value.GetString());

                    configuration.AddMethodGetter("method", 2, 
                        static (FastObject instance, in V8FastArgs args, in V8FastResult result) => result.Set(instance.Method(
                            args.GetBigInteger(0, "intArg"),
                            args.GetCharSpan(1, "stringArg")
                        ))
                    );

                    configuration.EnumerateIndexedProperties = true;

                    configuration.SetEnumeratorFactory(static instance => instance.contents.GetEnumerator());
                    configuration.SetAsyncEnumeratorFactory(static instance => instance.asyncContents.AsAsyncEnumerable().GetAsyncEnumerator());
                });
            }
        }

        #endregion
    }
}
