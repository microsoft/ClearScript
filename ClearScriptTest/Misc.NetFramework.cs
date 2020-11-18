// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.ClearScript.Util;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public static partial class TestUtil
    {
        public static void InvokeVBTestSub(string code, string extraDefinitions = null)
        {
            var options = new CompilerParameters { GenerateInMemory = true };
            options.ReferencedAssemblies.Add("ClearScript.Core.dll");
            options.ReferencedAssemblies.Add("ClearScript.V8.dll");
            options.ReferencedAssemblies.Add("ClearScript.Windows.dll");
            options.ReferencedAssemblies.Add("ClearScriptTest.dll");
            options.ReferencedAssemblies.Add("System.Runtime.dll");
            options.ReferencedAssemblies.Add(typeof(Enumerable).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(Assert).Assembly.Location);
            var results = new VBCodeProvider().CompileAssemblyFromSource(options, MiscHelpers.FormatInvariant(@"
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

            if (results.Errors.HasErrors)
            {
                var messageBuilder = new StringBuilder("Errors encountered during Visual Basic compilation:\n");
                foreach (var error in results.Errors)
                {
                    messageBuilder.Append(error);
                    messageBuilder.Append('\n');
                }

                throw new OperationCanceledException(messageBuilder.ToString());
            }

            results.CompiledAssembly.GetType("TestModule").InvokeMember("TestSub", BindingFlags.InvokeMethod, null, null, ArrayHelpers.GetEmptyArray<object>());
        }

        public static object InvokeVBTestFunction(string code, string extraDefinitions = null)
        {
            var options = new CompilerParameters { GenerateInMemory = true };
            options.ReferencedAssemblies.Add("ClearScript.Core.dll");
            options.ReferencedAssemblies.Add("ClearScript.V8.dll");
            options.ReferencedAssemblies.Add("ClearScript.Windows.dll");
            options.ReferencedAssemblies.Add("ClearScriptTest.dll");
            options.ReferencedAssemblies.Add("System.Runtime.dll");
            options.ReferencedAssemblies.Add(typeof(Enumerable).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(Assert).Assembly.Location);
            var results = new VBCodeProvider().CompileAssemblyFromSource(options, MiscHelpers.FormatInvariant(@"
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

            if (results.Errors.HasErrors)
            {
                var messageBuilder = new StringBuilder("Errors encountered during Visual Basic compilation:\n");
                foreach (var error in results.Errors)
                {
                    messageBuilder.Append(error);
                    messageBuilder.Append('\n');
                }

                throw new OperationCanceledException(messageBuilder.ToString());
            }

            return results.CompiledAssembly.GetType("TestModule").InvokeMember("TestFunction", BindingFlags.InvokeMethod, null, null, ArrayHelpers.GetEmptyArray<object>());
        }
    }
}
