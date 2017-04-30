// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;

namespace Microsoft.ClearScript.Test
{
    public static class ClearScriptBenchmarks
    {
    #if DEBUG
        private const string flavor = "Debug";
    #else
        private const string flavor = "Release";
    #endif

        public static void Main()
        {
            Console.Clear();
            Console.WriteLine("ClearScript Benchmarks ({0}, {1})\n", flavor, Environment.Is64BitProcess ? "64-bit" : "32-bit");

            while (true)
            {
                Console.WriteLine("1. SunSpider - JScript");
                Console.WriteLine("2. SunSpider - V8 (default)");
                Console.WriteLine("3. SunSpider - V8 (no GlobalMembers support)");
                Console.WriteLine("4. Exit");
                Console.WriteLine();

                var exit = false;

                while (true)
                {
                    Console.Write("-> ");
                    var input = Console.ReadLine();

                    int selection;
                    if (!int.TryParse(input, out selection))
                    {
                        Console.WriteLine("Invalid selection");
                        continue;
                    }

                    var done = false;

                    switch (selection)
                    {
                        case 1:
                            Run(() => new JScriptEngine(WindowsScriptEngineFlags.EnableStandardsMode), SunSpider.RunSuite);
                            done = true;
                            break;

                        case 2:
                            Run(() => new V8ScriptEngine(), SunSpider.RunSuite);
                            done = true;
                            break;

                        case 3:
                            Run(() => new V8ScriptEngine(V8ScriptEngineFlags.DisableGlobalMembers), SunSpider.RunSuite);
                            done = true;
                            break;

                        case 4:
                            done = true;
                            exit = true;
                            break;

                        default:
                            Console.WriteLine("Invalid selection");
                            break;
                    }

                    if (done)
                    {
                        Console.WriteLine();
                        break;
                    }
                }

                if (exit)
                {
                    break;
                }
            }
        }

        private static void Run(Func<ScriptEngine> engineFactory, Action<ScriptEngine> benchmark)
        {
            Console.WriteLine();
            using (var engine = engineFactory())
            {
                benchmark(engine);
            }
        }
    }
}
