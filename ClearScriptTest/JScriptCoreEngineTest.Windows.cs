// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.ClearScript.Windows.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class JScriptCoreEngineTest
    {
        #region test methods

        [TestMethod, TestCategory("JScriptCoreEngine")]
        public void JScriptCoreEngine_AddCOMType_XMLHTTP()
        {
            var status = 0;
            string data = null;

            var checkpoint = new ManualResetEventSlim();
            Dispatcher dispatcher = null;

            var thread = new Thread(() =>
            {
                using (var testEngine = new JScriptEngine(Windows.WindowsScriptEngineFlags.EnableDebugging | Windows.WindowsScriptEngineFlags.EnableStandardsMode, NullSyncInvoker.Instance))
                {
                    dispatcher = Dispatcher.CurrentDispatcher;
                    checkpoint.Set();

                    testEngine.Script.onComplete = new Action<int, string>((xhrStatus, xhrData) =>
                    {
                        status = xhrStatus;
                        data = xhrData;
                        Dispatcher.ExitAllFrames();
                    });

                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        // ReSharper disable AccessToDisposedClosure

                        testEngine.AddCOMType("XMLHttpRequest", "MSXML2.XMLHTTP");
                        testEngine.Execute($@"
                            xhr = new XMLHttpRequest();
                            xhr.open('POST', '{HttpBinUrl}/post', true);
                            xhr.onreadystatechange = function () {{
                                if (xhr.readyState == 4) {{
                                    onComplete(xhr.status, JSON.parse(xhr.responseText).data);
                                }}
                            }};
                            xhr.send('Hello, world!');
                        ");

                        // ReSharper restore AccessToDisposedClosure
                    }));

                    Dispatcher.Run();
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
