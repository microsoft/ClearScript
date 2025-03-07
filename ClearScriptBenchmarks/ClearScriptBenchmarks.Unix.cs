// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.V8;

namespace Microsoft.ClearScript.Test
{
    public static class ClearScriptBenchmarks
    {
    #if DEBUG
        private const string flavor = "Debug";
    #else
        private const string flavor = "Release";
    #endif

        public static void Main(string[] args)
        {
            var choice = 0U;
            var burn = false;

            var argQueue = new Queue<string>(args);
            while (argQueue.TryDequeue(out var arg))
            {
                if ((arg == "-b") || (arg == "--burn"))
                {
                    burn = argQueue.TryDequeue(out var choiceString) && uint.TryParse(choiceString, out choice) && (choice >= 1) && (choice <= 2);
                }
                else if ((arg == "-d") || (arg == "--disable-background-work"))
                {
                    V8Settings.GlobalFlags |= V8GlobalFlags.DisableBackgroundWork;
                }
            }

            Console.Clear();
            if (!burn) Console.WriteLine("ClearScript Benchmarks ({0}, {1}, {2} {3})\n", RuntimeInformation.FrameworkDescription.Trim(), RuntimeInformation.OSDescription.Trim(), RuntimeInformation.ProcessArchitecture, flavor);

            var count = 0UL;

            while (true)
            {
                if (!burn)
                {
                    Console.WriteLine("1. SunSpider - V8 (default)");
                    Console.WriteLine("2. SunSpider - V8 (no GlobalMembers support)");
                    Console.WriteLine("3. Exit");
                    Console.WriteLine();
                }

                var exit = false;

                while (true)
                {
                    uint selection;
                    if (burn)
                    {
                        selection = choice;
                    }
                    else
                    {
                        Console.Write("-> ");
                        var input = Console.ReadLine();

                        if (!uint.TryParse(input, out selection))
                        {
                            Console.WriteLine("Invalid selection");
                            continue;
                        }
                    }

                    var done = false;

                    switch (selection)
                    {
                        case 1:
                            Run(() => new V8ScriptEngine(), SunSpider.RunSuite, burn);
                            done = true;
                            break;

                        case 2:
                            Run(() => new V8ScriptEngine(V8ScriptEngineFlags.DisableGlobalMembers), SunSpider.RunSuite, burn);
                            done = true;
                            break;

                        case 3:
                            done = true;
                            exit = true;
                            break;

                        default:
                            Console.WriteLine("Invalid selection");
                            break;
                    }

                    if (done)
                    {
                        if (!burn) Console.WriteLine();
                        count += 1;
                        break;
                    }
                }

                if (exit)
                {
                    break;
                }

                if (burn)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    using (var process = Process.GetCurrentProcess())
                    {
                        Console.WriteLine("{0:#,#} after {1:#,#} iterations", process.WorkingSet64, count);
                    }
                }
            }
        }

        private static void Run(Func<ScriptEngine> engineFactory, Action<ScriptEngine, bool> benchmark, bool quiet)
        {
            if (!quiet) Console.WriteLine();
            using (var engine = engineFactory())
            {
                benchmark(engine, quiet);
            }
        }
    }
}
