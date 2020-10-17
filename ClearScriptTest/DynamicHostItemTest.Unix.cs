// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class DynamicHostItemTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("DynamicHostItem")]
        public void DynamicHostItem_CrossEngine()
        {
            const string format = "{0} foo {1} bar {2} baz {3} qux {4} quux {5}";
            var args = new object[] { 1, 2, 3, 4, 5, 6 };
            engine.Script.mscorlib = new HostTypeCollection("mscorlib");

            using (var outerEngine = new V8ScriptEngine())
            {
                outerEngine.Script.inner = engine;
                outerEngine.Script.format = format;
                outerEngine.Script.args = args;
                Assert.AreEqual(string.Format(format, args), outerEngine.Evaluate("inner.Script.mscorlib.System.String.Format(format, args)"));
                Assert.AreEqual(string.Format(format, args), outerEngine.Evaluate("inner.Script.mscorlib['System'].String['Format'](format, args)"));
            }
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
