// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class ExtensionsTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Generator()
        {
            engine.Execute("foo = (function* () { yield 'This'; yield 'is'; yield 'not'; yield 'a'; yield 'drill!'; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_AsyncGenerator()
        {
            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo = (async function* () { await delay(1), yield 'This'; await delay(1), yield 'is'; await delay(1), yield 'not'; await delay(1), yield 'a'; await delay(1), yield 'drill!'; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_GenericObject()
        {
            engine.Execute("foo = { 'This': 1, 'is': 2, 'not': 3, 'a': 4, 'drill!': 5 }");
            TestUtil.AssertException<ArgumentException>(() => string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Execute("foo[Symbol.iterator] = function* () { for (const item of Object.keys(foo)) yield item; }");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo[Symbol.asyncIterator] = async function* () { for (const item of Object.keys(foo)) { await delay(1); yield item; } }");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Array()
        {
            engine.Execute("foo = [ 'This', 'is', 'not', 'a', 'drill!' ]");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Managed_Object()
        {
            engine.Global["bar"] = new PropertyBag { { "This", 1 }, { "is", 2 }, { "not", 3 }, { "a", 4 }, { "drill!", 5 } };
            TestUtil.AssertException<ArgumentException>(() => string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Execute("foo = (function* () { for (const item of Object.keys(bar)) yield item; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo = (async function* () { for (const item of Object.keys(bar)) { await delay(1); yield item; } })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Managed_Array()
        {
            engine.Global["bar"] = new[] { "This", "is", "not", "a", "drill!" };
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Global["bar"] = new object[] { "This", "is", "not", "a", "drill!" };
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Execute("foo = (function* () { for (const item of bar) yield item; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo = (async function* () { for (const item of bar) { await delay(1); yield item; } })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Managed_List()
        {
            engine.Global["bar"] = new List<string> { "This", "is", "not", "a", "drill!" };
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Global["bar"] = new List<object> { "This", "is", "not", "a", "drill!" };
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Execute("foo = (function* () { for (const item of bar) yield item; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo = (async function* () { for (const item of bar) { await delay(1); yield item; } })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        [TestMethod, TestCategory("Extensions")]
        public void Extension_JavaScript_ToAsyncEnumerable_Managed_ArrayList()
        {
            engine.Global["bar"] = new ArrayList { "This", "is", "not", "a", "drill!" };
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["bar"].ToAsyncEnumerable())));

            engine.Execute("foo = (function* () { for (const item of bar) yield item; })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));

            engine.Global["delay"] = new Func<int, object>(ms => Task.Delay(ms).ToPromise());
            engine.Execute("foo = (async function* () { for (const item of bar) { await delay(1); yield item; } })()");
            Assert.AreEqual("This is not a drill!", string.Join(" ", IterateAsyncEnumerable(engine.Global["foo"].ToAsyncEnumerable())));
        }

        // ReSharper restore InconsistentNaming

        #endregion

        #region miscellaneous

        private static IEnumerable<T> IterateAsyncEnumerable<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            while (asyncEnumerator.MoveNextAsync().AsTask().Result)
            {
                yield return asyncEnumerator.Current;
            }
        }

        #endregion
    }
}
