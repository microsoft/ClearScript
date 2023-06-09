// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.ClearScript.Windows.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class VBScriptCoreEngineTest
    {
        #region test methods

        [TestMethod, TestCategory("VBScriptCoreEngine")]
        public void VBScriptCoreEngine_AddCOMType_XMLHTTP()
        {
            var status = 0;
            string data = null;

            var checkpoint = new ManualResetEventSlim();
            Dispatcher dispatcher = null;

            var thread = new Thread(() =>
            {
                using (var testEngine = new VBScriptEngine(Windows.WindowsScriptEngineFlags.EnableDebugging, NullSyncInvoker.Instance))
                {
                    using (var helperEngine = new JScriptEngine(Windows.WindowsScriptEngineFlags.EnableStandardsMode, NullSyncInvoker.Instance))
                    {
                        // ReSharper disable AccessToDisposedClosure

                        dispatcher = Dispatcher.CurrentDispatcher;
                        checkpoint.Set();

                        testEngine.Script.onComplete = new Action<int, string>((xhrStatus, xhrData) =>
                        {
                            status = xhrStatus;
                            data = xhrData;
                            Dispatcher.ExitAllFrames();
                        });

                        testEngine.Script.getData = new Func<string, string>(responseText =>
                            helperEngine.Script.JSON.parse(responseText).data
                        );

                        dispatcher.BeginInvoke(new Action(() =>
                        {
                            testEngine.AddCOMType("XMLHttpRequest", "MSXML2.XMLHTTP");
                            testEngine.Script.host = new HostFunctions();
                            testEngine.Execute($@"
                                sub onreadystatechange
                                    if xhr.readyState = 4 then
                                        call onComplete(xhr.status, getData(xhr.responseText))
                                    end if
                                end sub
                                xhr = host.newObj(XMLHttpRequest)
                                call xhr.open(""POST"", ""{HttpBinUrl}/post"", true)
                                xhr.onreadystatechange = GetRef(""onreadystatechange"")
                                call xhr.send(""Hello, world!"")
                            ");
                        }));

                        Dispatcher.Run();

                        // ReSharper restore AccessToDisposedClosure
                    }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            checkpoint.Wait();

            if (!thread.Join(TimeSpan.FromSeconds(10)))
            {
                dispatcher.Invoke(Dispatcher.ExitAllFrames);
                thread.Join();
                Assert.Inconclusive("The Httpbin service request timed out");
            }

            Assert.AreEqual(200, status);
            Assert.AreEqual("Hello, world!", data);
        }

        #endregion
    }
}
