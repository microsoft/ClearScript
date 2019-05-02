// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
        private const string version = "sunspider-1.0.2";
        private const string baseUrl = "https://webkit.org/perf/" + version + "/" + version + "/";

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
            engine.Execute(@"
                function ClearScriptCleanup() {
                    delete Array.prototype.toJSONString;
                    delete Boolean.prototype.toJSONString;
                    delete Date.prototype.toJSONString;
                    delete Number.prototype.toJSONString;
                    delete Object.prototype.toJSONString;
                }
            ");

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
            engine.Script.ClearScriptCleanup();
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

        private sealed class MockDOM
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

            public object onerror { get; set; }
        }

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming
    }
}
