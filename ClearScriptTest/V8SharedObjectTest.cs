// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class V8SharedObjectTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;
        private V8ScriptEngine otherEngine;

        [TestInitialize]
        public void TestInitialize()
        {
            otherEngine = new V8ScriptEngine();
            otherEngine.Execute(@"
                arrayBuffer = new SharedArrayBuffer(1024);
                array = new Uint8Array(arrayBuffer);
                for (let index = 0; index < 1024; ++index) {
                    array[index] = index & 0xFF;
                }
            ");

            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
            engine.Script.arrayBuffer = otherEngine.Script.arrayBuffer;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            otherEngine.Dispose();
            engine.Dispose();
            BaseTestCleanup();
        }

        #endregion

        #region test methods

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_ArrayBuffer()
        {
            Assert.AreEqual("[object SharedArrayBuffer]", engine.ExecuteCommand("arrayBuffer"));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("arrayBuffer instanceof SharedArrayBuffer")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((IArrayBuffer)engine.Script.arrayBuffer).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_DataView()
        {
            otherEngine.Execute("dataView = new DataView(arrayBuffer)");
            engine.Script.dataView = otherEngine.Script.dataView;
            Assert.AreEqual("[object DataView]", engine.ExecuteCommand("dataView"));
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("dataView instanceof DataView")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((IDataView)engine.Script.dataView).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Uint8Array()
        {
            otherEngine.Execute("array = new Uint8Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Uint8Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Uint8ClampedArray()
        {
            otherEngine.Execute("array = new Uint8ClampedArray(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Uint8ClampedArray")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Int8Array()
        {
            otherEngine.Execute("array = new Int8Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Int8Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<sbyte>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Uint16Array()
        {
            otherEngine.Execute("array = new Uint16Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Uint16Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ushort>)engine.Script.array).GetBytes()));
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<char>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Int16Array()
        {
            otherEngine.Execute("array = new Int16Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Int16Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<short>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Uint32Array()
        {
            otherEngine.Execute("array = new Uint32Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Uint32Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<uint>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Int32Array()
        {
            otherEngine.Execute("array = new Int32Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Int32Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<int>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_BigUint64Array()
        {
            otherEngine.Execute("array = new BigUint64Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof BigUint64Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ulong>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_BigInt64Array()
        {
            otherEngine.Execute("array = new BigInt64Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof BigInt64Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<long>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Float32Array()
        {
            otherEngine.Execute("array = new Float32Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Float32Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<float>)engine.Script.array).GetBytes()));
        }

        [TestMethod, TestCategory("V8SharedObject")]
        public void V8SharedObject_Float64Array()
        {
            otherEngine.Execute("array = new Float64Array(arrayBuffer)");
            engine.Script.array = otherEngine.Script.array;
            Assert.IsTrue(Convert.ToBoolean(engine.Evaluate("array instanceof Float64Array")));
            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToByte(index & 0xFF)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<double>)engine.Script.array).GetBytes()));
        }

        #endregion
    }
}
