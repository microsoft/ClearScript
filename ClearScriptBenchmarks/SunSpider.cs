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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace Microsoft.ClearScript.Test
{
    internal static class SunSpider
    {
        private const string version = "sunspider-0.9.1";
        private const string baseUrl = "http://www.webkit.org/perf/" + version + "/" + version + "/";

        private const int repeatCount = 10;
        private const string scriptBegin = "<script>";
        private const string scriptEnd = "</script>";

        private static bool gotCode;
        private static string testPrefix;
        private static string testContents;

        public static void RunSuite(ScriptEngine engine)
        {
            // download raw test code if necessary
            if (!gotCode)
            {
                Console.Write("Downloading code... ");
                testPrefix = DownloadFileAsString("sunspider-test-prefix.js");
                testContents = DownloadFileAsString("sunspider-test-contents.js");
                Console.WriteLine("Done");
                gotCode = true;
            }

            // set up dummy HTML DOM
            var mockDOM = new MockDOM();
            engine.AccessContext = typeof(SunSpider);
            engine.AddHostObject("document", mockDOM);
            engine.AddHostObject("window", mockDOM);
            engine.AddHostObject("parent", mockDOM);

            // load raw test code
            engine.Execute(testPrefix);
            engine.Execute(testContents);

            // initialize
            var testCount = (int)engine.Script.tests.length;
            var testIndices = Enumerable.Range(0, testCount).ToList();
            var repeatIndices = Enumerable.Range(0, repeatCount).ToList();

            // run warmup cycle
            Console.Write("Warming up... ");
            testIndices.ForEach(testIndex => RunTest(engine, mockDOM, testIndex));
            Console.WriteLine("Done");

            // run main test
            var results = repeatIndices.Select(index => new Dictionary<string, int>()).ToArray();
            repeatIndices.ForEach(repeatIndex =>
            {
                Console.Write("Running iteration {0}... ", repeatIndex + 1);
                testIndices.ForEach(testIndex =>
                {
                    var name = (string)engine.Script.tests[testIndex];
                    results[repeatIndex][name] = RunTest(engine, mockDOM, testIndex);
                });
                Console.WriteLine("Done");
            });

            // show results
            var resultString = new StringBuilder("{\"v\":\"" + version + "\",");
            results[0].Keys.ToList().ForEach(name =>
            {
                resultString.Append("\"" + name + "\":[");
                resultString.Append(string.Join(",", repeatIndices.Select(repeatIndex => results[repeatIndex][name])));
                resultString.Append("],");
            });
            resultString.Length -= 1;
            resultString.Append("}");
            Process.Start((new Uri(baseUrl + "results.html?" + resultString)).AbsoluteUri);
        }

        private static int RunTest(ScriptEngine engine, MockDOM mockDOM, int index)
        {
            // extract test script
            var name = (string)engine.Script.tests[index];
            var html = (string)engine.Script.testContents[index];
            var start = html.IndexOf(scriptBegin, StringComparison.OrdinalIgnoreCase) + scriptBegin.Length;
            var end = html.IndexOf(scriptEnd, StringComparison.OrdinalIgnoreCase);
            var script = html.Substring(start, end - start);

            // execute test
            var result = int.MinValue;
            mockDOM.RecordAction = value => result = value;
            engine.Execute(name, script);
            return result;
        }

        private static string DownloadFileAsString(string name)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(baseUrl + name);
            }
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local

        private class MockDOM
        {
            public Action<int> RecordAction { private get; set; }

            public object getElementById(string id)
            {
                return this;
            }

            public string innerHTML { get; set; }

            public object parent
            {
                get { return this; }
            }

            public void recordResult(int time)
            {
                RecordAction(time);
            }
        }

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming
    }
}
