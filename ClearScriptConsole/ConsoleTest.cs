using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    internal static class ConsoleTest
    {
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
                        var pointer = stackalloc byte[192 * 1024];
                        if ((ulong)pointer < (984 * 1024UL))
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
    }
}
