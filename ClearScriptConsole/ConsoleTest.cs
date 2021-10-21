// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    internal static class ConsoleTest
    {
        #region test methods

        public static unsafe void BugFix_V8StackLimitIntegerOverflow()
        {
            var threads = new List<Thread>();
            var exit = new ManualResetEventSlim();

            try
            {
                var done = new AutoResetEvent(false);
                Exception caughtException = null;
                var found = false;

                while (true)
                {
                    var thread = new Thread(() =>
                    {
                        var ptr = stackalloc byte[192 * 1024];
                        if ((ulong)ptr < (984 * 1024UL))
                        {
                            try
                            {
                                using (var engine = new V8ScriptEngine())
                                {
                                    Assert.AreEqual(Math.PI, engine.Evaluate("Math.PI"));
                                }

                                found = true;
                            }
                            catch (Exception exception)
                            {
                                caughtException = exception;
                            }
                        }

                        done.Set();
                        exit.Wait();

                    }, 256 * 1024);

                    threads.Add(thread);
                    thread.Start();
                    done.WaitOne();

                    if (caughtException != null)
                    {
                        throw new AssertFailedException("Exception thrown in worker thread", caughtException);
                    }

                    if (found)
                    {
                        break;
                    }
                }

                Assert.IsTrue(found);
            }
            finally
            {
                exit.Set();
                threads.ForEach(thread => thread.Join());
            }
        }

        public static void BugFix_MultipleAppDomains()
        {
            #pragma warning disable SYSLIB0024 // Creating and unloading AppDomains is not supported and throws an exception

            var domain1 = AppDomain.CreateDomain("domain1");
            var domain2 = AppDomain.CreateDomain("domain2");

            #pragma warning restore SYSLIB0024 // Creating and unloading AppDomains is not supported and throws an exception

            var obj1 = (MultiAppDomainTest)domain1.CreateInstanceAndUnwrap(Assembly.GetEntryAssembly().FullName, typeof(MultiAppDomainTest).FullName);
            var obj2 = (MultiAppDomainTest)domain2.CreateInstanceAndUnwrap(Assembly.GetEntryAssembly().FullName, typeof(MultiAppDomainTest).FullName);

            obj1.CreateEngine();
            obj2.CreateEngine();

            obj1.DisposeEngine();
            obj2.DisposeEngine();
        }

        public static void V8ScriptEngine_HeapExpansionMultiplier()
        {
            using (var engine = new V8ScriptEngine(new V8RuntimeConstraints { HeapExpansionMultiplier = 1.25 }))
            {
                engine.Execute(@"
                    let node = [];
                    for (let j = 0; j < 15; ++j) {
                        const offset = Math.round((Math.random() - 0.5) * 12345);
                        for (let i = 0; i < 10000000; ++i) {
                            node.push(i + offset);
                        }
                        const next = [];
                        next.push(node);
                        node = next;
                    }
                ");
            }
        }

        #endregion

        #region miscellaneous

        public class MultiAppDomainTest : MarshalByRefObject
        {
            private ScriptEngine engine;

            public void CreateEngine()
            {
                engine = new V8ScriptEngine();
            }

            public void DisposeEngine()
            {
                engine.Dispose();
            }
        }

        #endregion
    }
}
