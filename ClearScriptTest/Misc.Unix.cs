// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public static partial class TestUtil
    {
        public static void InvokeVBTestSub(string code, string extraDefinitions = null)
        {
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(V8ScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(WindowsScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ClearScriptTest).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.VisualBasic.Core").Location)
            };

            var syntaxTree = VisualBasicSyntaxTree.ParseText(MiscHelpers.FormatInvariant(@"
                Imports System
                Imports System.Linq
                Imports System.Runtime.InteropServices
                Imports Microsoft.ClearScript
                Imports Microsoft.ClearScript.Test
                Imports Microsoft.ClearScript.V8
                Imports Microsoft.ClearScript.Windows
                Imports Microsoft.VisualStudio.TestTools.UnitTesting
                {1}
                Module TestModule
                    Sub TestSub
                        {0}
                    End Sub
                End Module
            ", code, extraDefinitions ?? string.Empty));

            var compilation = VisualBasicCompilation.Create(
                "VBTest_" + Guid.NewGuid().ToString(),
                new[] { syntaxTree },
                references,
                new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var messageBuilder = new StringBuilder("Errors encountered during Visual Basic compilation:\n");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        messageBuilder.Append(diagnostic);
                        messageBuilder.Append('\n');
                    }

                    throw new OperationCanceledException(messageBuilder.ToString());
                }

                stream.Seek(0, SeekOrigin.Begin);
                AssemblyLoadContext.Default.LoadFromStream(stream).GetType("TestModule").InvokeMember("TestSub", BindingFlags.InvokeMethod, null, null, ArrayHelpers.GetEmptyArray<object>());
            }
        }

        public static object InvokeVBTestFunction(string code, string extraDefinitions = null)
        {
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(V8ScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(WindowsScriptEngine).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ClearScriptTest).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("Microsoft.VisualBasic.Core").Location)
            };

            var syntaxTree = VisualBasicSyntaxTree.ParseText(MiscHelpers.FormatInvariant(@"
                Imports System
                Imports System.Linq
                Imports System.Runtime.InteropServices
                Imports Microsoft.ClearScript
                Imports Microsoft.ClearScript.Test
                Imports Microsoft.ClearScript.V8
                Imports Microsoft.ClearScript.Windows
                Imports Microsoft.VisualStudio.TestTools.UnitTesting
                {1}
                Module TestModule
                    Function TestFunction
                        {0}
                    End Function
                End Module
            ", code, extraDefinitions ?? string.Empty));

            var compilation = VisualBasicCompilation.Create(
                "VBTest_" + Guid.NewGuid().ToString(),
                new[] { syntaxTree },
                references,
                new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    var messageBuilder = new StringBuilder("Errors encountered during Visual Basic compilation:\n");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        messageBuilder.Append(diagnostic);
                        messageBuilder.Append('\n');
                    }

                    throw new OperationCanceledException(messageBuilder.ToString());
                }

                stream.Seek(0, SeekOrigin.Begin);
                return AssemblyLoadContext.Default.LoadFromStream(stream).GetType("TestModule").InvokeMember("TestFunction", BindingFlags.InvokeMethod, null, null, ArrayHelpers.GetEmptyArray<object>());
            }
        }
    }
}

namespace Microsoft.ClearScript.Windows
{
    public class JScriptEngine : Core.JScriptEngine
    {
        public JScriptEngine()
            : this(WindowsScriptEngineFlags.None)
        {
        }

        public JScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        public JScriptEngine(string name, WindowsScriptEngineFlags flags)
            : base(name, flags, NullSyncInvoker.Instance)
        {
        }
    }

    public class VBScriptEngine : Core.VBScriptEngine
    {
        public VBScriptEngine()
            : this(WindowsScriptEngineFlags.None)
        {
        }

        public VBScriptEngine(WindowsScriptEngineFlags flags)
            : this(null, flags)
        {
        }

        public VBScriptEngine(string name, WindowsScriptEngineFlags flags)
            : base(name, flags, NullSyncInvoker.Instance)
        {
        }
    }
}
