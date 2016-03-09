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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [DeploymentItem("ClearScriptV8-64.dll")]
    [DeploymentItem("ClearScriptV8-32.dll")]
    [DeploymentItem("v8-x64.dll")]
    [DeploymentItem("v8-ia32.dll")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class HostListTest : ClearScriptTest
    {
        #region setup / teardown

        private ScriptEngine engine;

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

        [TestMethod, TestCategory("HostList")]
        public void HostList_ArrayList()
        {
            var list = new ArrayList { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            engine.Script.list = list;
            Assert.AreEqual(3, engine.Evaluate("list.Count"));
            Assert.AreEqual(DayOfWeek.Tuesday, engine.Evaluate("list[1]"));
            Assert.AreEqual("[HostObject:DayOfWeek]", engine.ExecuteCommand("list[2]"));
            engine.Execute("list[1] = list[2]");
            Assert.AreEqual(DayOfWeek.Wednesday, engine.Evaluate("list[1]"));
        }

        [TestMethod, TestCategory("HostList")]
        public void HostList_List()
        {
            var list = new List<IConvertible> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            engine.Script.list = list;
            Assert.AreEqual(3, engine.Evaluate("list.Count"));
            Assert.AreEqual(DayOfWeek.Tuesday, engine.Evaluate("list[1]"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list[2]"));
            engine.Execute("list[1] = list[2]");
            Assert.AreEqual(DayOfWeek.Wednesday, engine.Evaluate("list[1]"));
        }

        [TestMethod, TestCategory("HostList")]
        public void HostList_Custom()
        {
            var list = new BogusCustomList { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            engine.Script.list = list;
            Assert.AreEqual(3, engine.Evaluate("list.Count"));
            Assert.AreEqual(DayOfWeek.Tuesday, engine.Evaluate("list[1]"));
            Assert.AreEqual("[HostObject:DayOfWeek]", engine.ExecuteCommand("list[2]"));
            engine.Execute("list[1] = list[2]");
            Assert.AreEqual(DayOfWeek.Wednesday, engine.Evaluate("list[1]"));
        }

        [TestMethod, TestCategory("HostList")]
        public void HostList_CustomGeneric()
        {
            var list = new BogusCustomList<IConvertible> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            engine.Script.list = list;
            Assert.AreEqual(3, engine.Evaluate("list.Count"));
            Assert.AreEqual(DayOfWeek.Tuesday, engine.Evaluate("list[1]"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list[2]"));
            engine.Execute("list[1] = list[2]");
            Assert.AreEqual(DayOfWeek.Wednesday, engine.Evaluate("list[1]"));
        }

        [TestMethod, TestCategory("HostList")]
        public void HostList_TypeRestriction()
        {
            var list = new List<IConvertible> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
            engine.Script.list = list;
            Assert.AreEqual(3, engine.Evaluate("list.Count"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list[2]"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list.Item(2)"));
            engine.DisableListIndexTypeRestriction = true;
            Assert.AreEqual("[HostObject:DayOfWeek]", engine.ExecuteCommand("list[2]"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list.Item(2)"));
            engine.DisableListIndexTypeRestriction = false;
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list[2]"));
            Assert.AreEqual("[HostObject:IConvertible]", engine.ExecuteCommand("list.Item(2)"));
            engine.DisableTypeRestriction = true;
            Assert.AreEqual("[HostObject:DayOfWeek]", engine.ExecuteCommand("list[2]"));
            Assert.AreEqual("[HostObject:DayOfWeek]", engine.ExecuteCommand("list.Item(2)"));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        public interface IBogus
        {
        }

        public interface ICustomList : IList
        {
        }

        public interface ICustomList<T> : IList<T>
        {
        }

        public interface IBogusCustomList : IBogus, ICustomList
        {
        }

        public interface IBogusCustomList<T> : IBogus, ICustomList<T>
        {
        }

        public class BogusCustomListBase : IBogusCustomList
        {
            private readonly IList list = new ArrayList();

            public IEnumerator GetEnumerator()
            {
                return list.GetEnumerator();
            }

            public void CopyTo(Array array, int index)
            {
                list.CopyTo(array, index);
            }

            public int Count
            {
                get { return list.Count; }
            }

            public object SyncRoot
            {
                get { return list.SyncRoot; }
            }

            public bool IsSynchronized
            {
                get { return list.IsSynchronized; }
            }

            public int Add(object value)
            {
                return list.Add(value);
            }

            public bool Contains(object value)
            {
                return list.Contains(value);
            }

            public void Clear()
            {
                list.Clear();
            }

            public int IndexOf(object value)
            {
                return list.IndexOf(value);
            }

            public void Insert(int index, object value)
            {
                list.Insert(index, value);
            }

            public void Remove(object value)
            {
                list.Remove(value);
            }

            public void RemoveAt(int index)
            {
                list.RemoveAt(index);
            }

            public object this[int index]
            {
                get { return list[index]; }
                set { list[index] = value; }
            }

            public bool IsReadOnly
            {
                get { return list.IsReadOnly; }
            }

            public bool IsFixedSize
            {
                get { return list.IsFixedSize; }
            }
        }

        public class BogusCustomListBase<T> : IBogusCustomList<T>
        {
            private readonly IList<T> list = new List<T>();

            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)list).GetEnumerator();
            }

            public void Add(T item)
            {
                list.Add(item);
            }

            public void Clear()
            {
                list.Clear();
            }

            public bool Contains(T item)
            {
                return list.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                list.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return list.Remove(item);
            }

            public int Count
            {
                get { return list.Count; }
            }

            public bool IsReadOnly
            {
                get { return list.IsReadOnly; }
            }

            public int IndexOf(T item)
            {
                return list.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                list.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                list.RemoveAt(index);
            }

            public T this[int index]
            {
                get { return list[index]; }
                set { list[index] = value; }
            }
        }

        public class BogusCustomList : BogusCustomListBase
        {
        }

        public class BogusCustomList<T> : BogusCustomListBase<T>
        {
        }

        #endregion
    }
}
