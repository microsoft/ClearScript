// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.ClearScript.Util;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public class BaseTestArg
    {
    }

    public interface ITestArg
    {
    }

    public class TestArg : BaseTestArg, ITestArg
    {
    }

    public enum TestEnum : short
    {
        First,
        Second,
        Third,
        Fourth,
        Fifth
    }

    public class TestEventArgs<T> : EventArgs
    {
        public T Arg { get; set; }
    }

    public class NullResultWrappingTestObject<T>
    {
        private readonly T value;

        public NullResultWrappingTestObject(T value)
        {
            this.value = value;
        }

        public T Method(T arg)
        {
            return arg;
        }

        public Random Method(Random arg)
        {
            return arg;
        }

        public T Value { get { return value; } }

        public T NullValue { get { return default(T); } }

        [ScriptMember(ScriptMemberFlags.WrapNullResult)]
        public T WrappedNullValue { get { return NullValue; } }
    }

    public class DefaultPropertyTestObject
    {
        private readonly Dictionary<string, object> byName = new Dictionary<string, object>();
        private readonly Dictionary<DayOfWeek, object> byDay = new Dictionary<DayOfWeek, object>();

        public object this[string name]
        {
            get { return ((IDictionary)byName)[name]; }
            set { byName[name] = value; }
        }

        public object this[DayOfWeek day]
        {
            get { return ((IDictionary)byDay)[day]; }
            set { byDay[day] = value; }
        }

        [DispId(0)]
        public int Value { get; set; }
    }

    public class DefaultPropertyTestContainer
    {
        public readonly DefaultPropertyTestObject Field = new DefaultPropertyTestObject();

        public DefaultPropertyTestObject Property { get { return Field; } }

        public DefaultPropertyTestObject Method()
        {
            return Field;
        }
    }

    public static class TestUtil
    {
        public static void InvokeVBTestSub(string code, string extraDefinitions = null)
        {
            var options = new CompilerParameters { GenerateInMemory = true };
            options.ReferencedAssemblies.Add("ClearScript.dll");
            options.ReferencedAssemblies.Add("ClearScriptTest.dll");
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
            options.ReferencedAssemblies.Add("ClearScript.dll");
            options.ReferencedAssemblies.Add("ClearScriptTest.dll");
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

        public static void InvokeConsoleTest(string name)
        {
            var startInfo = new ProcessStartInfo("ClearScriptConsole.exe", "-t " + name)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                Assert.IsNotNull(process);
                process.WaitForExit();
                Assert.AreEqual(0, process.ExitCode, process.StandardError.ReadToEnd());
            }
        }

        public static void Iterate(Array array, Action<int[]> action)
        {
            array.Iterate(action);
        }

        public static string FormatInvariant(string format, params object[] args)
        {
            return MiscHelpers.FormatInvariant(format, args);
        }

        public static double CalcTestValue(Guid callerGuid, params object[] args)
        {
            var hashCode = args.Aggregate(callerGuid.GetHashCode(), (currentHashCode, value) => unchecked((currentHashCode * 31) + ((value != null) ? value.GetHashCode() : 0)));
            return hashCode * Math.E / Math.PI;
        }

        public static void AssertException<T>(Action action, bool checkScriptStackTrace = true) where T : Exception
        {
            Exception caughtException = null;
            var gotExpectedException = false;

            try
            {
                action();
            }
            catch (T exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                caughtException = exception;
                gotExpectedException = exception.GetBaseException() is T;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            var message = "Expected " + typeof(T).Name + " was not thrown.";
            if (caughtException != null)
            {
                message += " " + caughtException.GetType().Name + " was thrown instead.";
            }

            Assert.IsTrue(gotExpectedException, message);
        }

        public static void AssertValidException(Exception exception)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.StackTrace));
            Assert.IsFalse(exception.Message.Contains("COM"));

            if (!(exception is IOException))
            {
                Assert.IsFalse(exception.Message.Contains("HRESULT"));
                Assert.IsFalse(exception.Message.Contains("0x"));
            }
        }

        public static void AssertValidException(IScriptEngineException exception, bool checkScriptStackTrace = true)
        {
            AssertValidException((Exception)exception);
            if ((exception is ScriptEngineException) && !exception.IsFatal)
            {
                Assert.IsTrue(exception.ErrorDetails.StartsWith(exception.Message, StringComparison.Ordinal));
                if (checkScriptStackTrace)
                {
                    Assert.IsTrue(exception.ErrorDetails.Contains("\n    at "));
                }
            }
        }

        public static void AssertValidException(ScriptEngine engine, IScriptEngineException exception, bool checkScriptStackTrace = true)
        {
            AssertValidException(exception, checkScriptStackTrace);
            Assert.AreEqual(engine.Name, exception.EngineName);
        }

        private static void AssertValidExceptionChain(Exception exception, bool checkScriptStackTrace)
        {
            while (exception != null)
            {
                var scriptError = exception as IScriptEngineException;
                if (scriptError != null)
                {
                    AssertValidException(scriptError, checkScriptStackTrace);
                }
                else
                {
                    AssertValidException(exception);
                }

                exception = exception.InnerException;
            }
        }
    }
}
