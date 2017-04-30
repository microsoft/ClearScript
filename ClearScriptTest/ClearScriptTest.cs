// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public class ClearScriptTest
    {
        public TestContext TestContext { get; set; }

        public void BaseTestCleanup()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var proxy = V8TestProxy.Create();
            var counters = proxy.GetCounters();

            for (var attempts = 0; attempts < 10; attempts++)
            {
                if ((counters.ContextCount == 0UL) && (counters.IsolateCount == 0UL))
                {
                    return;
                }

                Thread.Sleep(100);
                counters = proxy.GetCounters();
            }

            Assert.AreEqual(0UL, counters.ContextCount, "Not all V8 contexts were destroyed.");
            Assert.AreEqual(0UL, counters.IsolateCount, "Not all V8 isolates were destroyed.");
        }
    }
}
