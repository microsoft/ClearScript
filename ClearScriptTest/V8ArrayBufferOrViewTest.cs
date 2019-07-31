// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.ClearScript.JavaScript;
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
    public class V8ArrayBufferOrViewTest : ClearScriptTest
    {
        #region setup / teardown

        private V8ScriptEngine engine;

        [TestInitialize]
        public void TestInitialize()
        {
            engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging);
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

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_None()
        {
            Assert.IsNull(engine.Evaluate("({})") as IArrayBuffer);
            Assert.IsNull(engine.Evaluate("({})") as IArrayBufferView);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_ArrayBuffer()
        {
            var arrayBuffer = (IArrayBuffer)engine.Evaluate("new ArrayBuffer(123456)");
            Assert.AreEqual(123456UL, arrayBuffer.Size);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_ArrayBuffer_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(1024));
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((IArrayBuffer)engine.Script.typedArray.buffer).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((IArrayBuffer)engine.Script.typedArray.buffer).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_ArrayBuffer_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(1024));
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_ArrayBuffer_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => unchecked((byte)index)).ToArray();

            engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(1024))");
            Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(1024))");
                Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((IArrayBuffer)engine.Script.typedArray.buffer).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IArrayBuffer)engine.Script.typedArray.buffer).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IArrayBuffer)engine.Script.typedArray.buffer).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_DataView()
        {
            var dataView = (IDataView)engine.Evaluate("new DataView(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, dataView.ArrayBuffer.Size);
            Assert.AreEqual(128UL, dataView.Offset);
            Assert.AreEqual(1024UL, dataView.Size);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_DataView_GetBytes()
        {
            engine.Execute(@"
                dataView = new DataView(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    dataView.setUint8(i, i);
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((IDataView)engine.Script.dataView).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((IDataView)engine.Script.dataView).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_DataView_ReadBytes()
        {
            engine.Execute(@"
                dataView = new DataView(new ArrayBuffer(1024));
                for (var i = 0; i < 1024; i++) {
                    dataView.setUint8(i, i);
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IDataView)engine.Script.dataView).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IDataView)engine.Script.dataView).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_DataView_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => unchecked((byte)index)).ToArray();

            engine.Execute("dataView = new DataView(new ArrayBuffer(1024))");
            Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(1024))");
                Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((IDataView)engine.Script.dataView).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IDataView)engine.Script.dataView).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((IDataView)engine.Script.dataView).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array()
        {
            var typedArray = (ITypedArray<byte>)engine.Evaluate("new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(1024UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => unchecked((byte)index)).ToArray();

            engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_Read()
        {
            engine.Execute(@"
                typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)index)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => unchecked((byte)index)).ToArray();

            engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray()
        {
            var typedArray = (ITypedArray<byte>)engine.Evaluate("new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(1024UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (byte)(Math.Min(index, 255))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (byte)(Math.Min(index, 255))).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => (byte)(Math.Min(index, 255))).ToArray();

            engine.Execute("typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_ToArray()
        {
            engine.Execute(@"
                typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (byte)(Math.Min(index, 255))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<byte>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_Read()
        {
            engine.Execute(@"
                typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (byte)(Math.Min(index, 255))).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint8ClampedArray_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => (byte)(Math.Min(index, 255))).ToArray();

            engine.Execute("typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint8ClampedArray(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<byte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<byte>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array()
        {
            var typedArray = (ITypedArray<sbyte>)engine.Evaluate("new Int8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(1024UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)(index - 512))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<sbyte>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<sbyte>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((byte)(index - 512))).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => (byte)(Math.Min(index, 255))).ToArray();

            engine.Execute("typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((sbyte)(index - 512))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<sbyte>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<sbyte>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_Read()
        {
            engine.Execute(@"
                typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => unchecked((sbyte)(index - 512))).ToArray();

            var readValues = new sbyte[512];
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new sbyte[512];
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int8Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => unchecked((sbyte)(index - 512))).ToArray();

            engine.Execute("typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new sbyte[512];
            Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int8Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new sbyte[512];
                Assert.AreEqual(256UL, ((ITypedArray<sbyte>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<sbyte>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array()
        {
            {
                var typedArray = (ITypedArray<ushort>)engine.Evaluate("new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
                Assert.AreEqual(128UL, typedArray.Offset);
                Assert.AreEqual(2048UL, typedArray.Size);
                Assert.AreEqual(1024UL, typedArray.Length);
            }
            {
                var typedArray = (ITypedArray<char>)engine.Evaluate("new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
                Assert.AreEqual(128UL, typedArray.Offset);
                Assert.AreEqual(2048UL, typedArray.Size);
                Assert.AreEqual(1024UL, typedArray.Length);
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            {
                var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((ushort)index)).ToArray();
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ushort>)engine.Script.typedArray).GetBytes()));
                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ushort>)engine.Script.typedArray).GetBytes()));
                }
            }
            {
                var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((char)index)).ToArray();
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<char>)engine.Script.typedArray).GetBytes()));
                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<char>)engine.Script.typedArray).GetBytes()));
                }
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            {
                var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((ushort)index)).ToArray();

                var readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    readValues = new byte[512];
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
            }
            {
                var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((char)index)).ToArray();

                var readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256).Take(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    readValues = new byte[512];
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256).Take(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_WriteBytes()
        {
            {
                var testValues = Enumerable.Range(0, 256).SelectMany(index => BitConverter.GetBytes((ushort)index)).ToArray();

                engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                var readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                    readValues = new byte[512];
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
            }
            {
                var testValues = Enumerable.Range(0, 256).SelectMany(index => BitConverter.GetBytes((char)index)).ToArray();

                engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                var readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                    readValues = new byte[512];
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            {
                var testValues = Enumerable.Range(0, 1024).Select(index => (ushort)index).ToArray();
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ushort>)engine.Script.typedArray).ToArray()));
                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<ushort>)engine.Script.typedArray).ToArray()));
                }
            }
            {
                var testValues = Enumerable.Range(0, 1024).Select(index => (char)index).ToArray();
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<char>)engine.Script.typedArray).ToArray()));
                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<char>)engine.Script.typedArray).ToArray()));
                }
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_Read()
        {
            engine.Execute(@"
                typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            {
                var testValues = Enumerable.Range(0, 1024).Select(index => (ushort)index).ToArray();

                var readValues = new ushort[512];
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    readValues = new ushort[512];
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
            }
            {
                var testValues = Enumerable.Range(0, 1024).Select(index => (char)index).ToArray();

                var readValues = new char[512];
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    readValues = new char[512];
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint16Array_Write()
        {
            {
                var testValues = Enumerable.Range(0, 512).Select(index => (ushort)index).ToArray();

                engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                var readValues = new ushort[512];
                Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                    readValues = new ushort[512];
                    Assert.AreEqual(256UL, ((ITypedArray<ushort>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<ushort>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
            }
            {
                var testValues = Enumerable.Range(0, 512).Select(index => (char)index).ToArray();

                engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                var readValues = new char[512];
                Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

                using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
                {
                    engine.Execute("typedArray = new Uint16Array(new ArrayBuffer(123456), 128, 1024)");
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                    readValues = new char[512];
                    Assert.AreEqual(256UL, ((ITypedArray<char>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                    Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
                }

                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
                TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<char>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array()
        {
            var typedArray = (ITypedArray<short>)engine.Evaluate("new Int16Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(2048UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((short)(index - 512))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<short>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<short>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((short)(index - 512))).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 256).SelectMany(index => BitConverter.GetBytes((short)(index - 512))).ToArray();

            engine.Execute("typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (short)(index - 512)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<short>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<short>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_Read()
        {
            engine.Execute(@"
                typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i - 512;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (short)(index - 512)).ToArray();

            var readValues = new short[512];
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new short[512];
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int16Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => (short)(index - 512)).ToArray();

            engine.Execute("typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new short[512];
            Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int16Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new short[512];
                Assert.AreEqual(256UL, ((ITypedArray<short>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<short>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array()
        {
            var typedArray = (ITypedArray<uint>)engine.Evaluate("new Uint32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(4096UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((uint)index)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<uint>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<uint>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes((uint)index)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 128).SelectMany(index => BitConverter.GetBytes((uint)index)).ToArray();

            engine.Execute("typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (uint)index).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<uint>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<uint>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_Read()
        {
            engine.Execute(@"
                typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => (uint)index).ToArray();

            var readValues = new uint[512];
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new uint[512];
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Uint32Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => (uint)index).ToArray();

            engine.Execute("typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new uint[512];
            Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Uint32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new uint[512];
                Assert.AreEqual(256UL, ((ITypedArray<uint>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<uint>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array()
        {
            var typedArray = (ITypedArray<int>)engine.Evaluate("new Int32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(4096UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(BitConverter.GetBytes).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<int>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<int>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(BitConverter.GetBytes).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 128).SelectMany(BitConverter.GetBytes).ToArray();

            engine.Execute("typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<int>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<int>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_Read()
        {
            engine.Execute(@"
                typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).ToArray();

            var readValues = new int[512];
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new int[512];
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Int32Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).ToArray();

            engine.Execute("typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new int[512];
            Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Int32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new int[512];
                Assert.AreEqual(256UL, ((ITypedArray<int>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<int>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array()
        {
            var typedArray = (ITypedArray<float>)engine.Evaluate("new Float32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(4096UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes(Convert.ToSingle(index * Math.PI))).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<float>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<float>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes(Convert.ToSingle(index * Math.PI))).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 128).SelectMany(index => BitConverter.GetBytes(Convert.ToSingle(index * Math.PI))).ToArray();

            engine.Execute("typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToSingle(index * Math.PI)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<float>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<float>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_Read()
        {
            engine.Execute(@"
                typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => Convert.ToSingle(index * Math.PI)).ToArray();

            var readValues = new float[512];
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new float[512];
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float32Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => Convert.ToSingle(index * Math.PI)).ToArray();

            engine.Execute("typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new float[512];
            Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Float32Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new float[512];
                Assert.AreEqual(256UL, ((ITypedArray<float>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<float>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array()
        {
            var typedArray = (ITypedArray<double>)engine.Evaluate("new Float64Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(123456UL, typedArray.ArrayBuffer.Size);
            Assert.AreEqual(128UL, typedArray.Offset);
            Assert.AreEqual(8192UL, typedArray.Size);
            Assert.AreEqual(1024UL, typedArray.Length);
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_GetBytes()
        {
            engine.Execute(@"
                typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes(index * Math.PI)).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<double>)engine.Script.typedArray).GetBytes()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<double>)engine.Script.typedArray).GetBytes()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_ReadBytes()
        {
            engine.Execute(@"
                typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).SelectMany(index => BitConverter.GetBytes(index * Math.PI)).ToArray();

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_WriteBytes()
        {
            var testValues = Enumerable.Range(0, 64).SelectMany(index => BitConverter.GetBytes(index * Math.PI)).ToArray();

            engine.Execute("typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

            var readValues = new byte[512];
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).WriteBytes(testValues, 256, 16384, 128));

                readValues = new byte[512];
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).ReadBytes(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).WriteBytes(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).WriteBytes(testValues, 16384, 512, 0));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_ToArray()
        {
            engine.Execute(@"
                typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => index * Math.PI).ToArray();
            Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<double>)engine.Script.typedArray).ToArray()));
            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                Assert.IsTrue(testValues.SequenceEqual(((ITypedArray<double>)engine.Script.typedArray).ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_Read()
        {
            engine.Execute(@"
                typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024);
                for (var i = 0; i < 1024; i++) {
                    typedArray[i] = i * Math.PI;
                }
            ");

            var testValues = Enumerable.Range(0, 1024).Select(index => index * Math.PI).ToArray();

            var readValues = new double[512];
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                readValues = new double[512];
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(128).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).Read(16384, 1024, readValues, 0));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).Read(0, 1024, readValues, 16384));
        }

        [TestMethod, TestCategory("V8ArrayBufferOrView")]
        public void V8ArrayBufferOrView_Float64Array_Write()
        {
            var testValues = Enumerable.Range(0, 512).Select(index => index * Math.PI).ToArray();

            engine.Execute("typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024)");
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

            var readValues = new double[512];
            Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
            Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));

            using (Scope.Create(() => MiscHelpers.Exchange(ref UnmanagedMemoryHelpers.DisableMarshalCopy, true), value => UnmanagedMemoryHelpers.DisableMarshalCopy = value))
            {
                engine.Execute("typedArray = new Float64Array(new ArrayBuffer(123456), 128, 1024)");
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Write(testValues, 256, 16384, 128));

                readValues = new double[512];
                Assert.AreEqual(256UL, ((ITypedArray<double>)engine.Script.typedArray).Read(128, 16384, readValues, 256));
                Assert.IsTrue(testValues.Skip(256).Take(256).SequenceEqual(readValues.Skip(256)));
            }

            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).Write(testValues, 0, 512, 16384));
            TestUtil.AssertException<ArgumentOutOfRangeException>(() => ((ITypedArray<double>)engine.Script.typedArray).Write(testValues, 16384, 512, 0));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
