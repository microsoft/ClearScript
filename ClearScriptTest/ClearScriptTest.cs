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
            DocumentLoader.Default.DiscardCachedDocuments();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var proxy = V8TestProxy.Create();
            var statistics = proxy.GetStatistics();

            for (var attempts = 0; attempts < 10; attempts++)
            {
                if ((statistics.ContextCount == 0UL) && (statistics.IsolateCount == 0UL))
                {
                    return;
                }

                Thread.Sleep(100);
                statistics = proxy.GetStatistics();
            }

            Assert.AreEqual(0UL, statistics.ContextCount, "Not all V8 contexts were destroyed.");
            Assert.AreEqual(0UL, statistics.IsolateCount, "Not all V8 isolates were destroyed.");
        }
    }
}
