using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.V8;

namespace MuslTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            var engine = new V8ScriptEngine();
            engine.AddHostType("Console", typeof(Console));
            engine.Evaluate("Console.WriteLine('Hello from JS')");
        }
    }
}
