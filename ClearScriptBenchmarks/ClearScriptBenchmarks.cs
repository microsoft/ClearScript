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
