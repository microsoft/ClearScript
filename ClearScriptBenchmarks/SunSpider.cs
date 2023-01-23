// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace Microsoft.ClearScript.Test
{
    internal static class SunSpider
    {
        private const string version = "sunspider-1.0.2";
        private const string baseUrl = "https://raw.githubusercontent.com/WebKit/WebKit/main/Websites/webkit.org/perf/" + version + "/" + version + "/";

        private const int repeatCount = 10;
        private const string scriptBegin = "<script>";
        private const string scriptEnd = "</script>";

        private static bool gotCode;
        private static string testPrefix;
        private static string testContents;

        public static void RunSuite(ScriptEngine engine, bool quiet)
        {
            // download raw test code if necessary
            if (!gotCode)
            {
                if (!quiet) Console.Write("Downloading code... ");
                testPrefix = DownloadFileAsString("sunspider-test-prefix.js");
                testContents = DownloadFileAsString("sunspider-test-contents.js");
                if (!quiet) Console.WriteLine("Done");
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
            if (!quiet) Console.Write("Warming up... ");
            testIndices.ForEach(testIndex => RunTest(engine, mockDOM, testIndex));
            if (!quiet) Console.WriteLine("Done");

            // run main test
            var results = repeatIndices.Select(index => new Dictionary<string, int>()).ToArray();
            var timeSpans = new long[repeatCount];
            repeatIndices.ForEach(repeatIndex =>
            {
                if (!quiet) Console.Write("Running iteration {0}... ", repeatIndex + 1);
                var stopWatch = Stopwatch.StartNew();
                testIndices.ForEach(testIndex =>
                {
                    var name = (string)engine.Script.tests[testIndex];
                    results[repeatIndex][name] = RunTest(engine, mockDOM, testIndex);
                });
                timeSpans[repeatIndex] = stopWatch.ElapsedMilliseconds;
                if (!quiet) Console.WriteLine("Done");
            });

            // show results
            if (!quiet)
            {

            #if USE_RESULTS_PAGE

                var resultString = new StringBuilder("{\"v\":\"" + version + "\",");
                results[0].Keys.ToList().ForEach(name =>
                {
                    resultString.Append("\"" + name + "\":[");
                    resultString.Append(string.Join(",", repeatIndices.Select(repeatIndex => results[repeatIndex][name])));
                    resultString.Append("],");
                });
                resultString.Length -= 1;
                resultString.Append("}");
                Process.Start(new ProcessStartInfo((new Uri(baseUrl + "results.html?" + resultString)).AbsoluteUri) { UseShellExecute = true });

            #else // !USE_RESULTS_PAGE

                Console.WriteLine("\n--\nAverage iteration time: {0} ms\n--", timeSpans.Average());

            #endif // !USE_RESULTS_PAGE

            }
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
            using (var client = new HttpClient())
            {
                return client.GetStringAsync(baseUrl + name).Result;
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

            public object parent => this;

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
