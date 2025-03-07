// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;
using Microsoft.CSharp.RuntimeBinder;
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
        public NullResultWrappingTestObject(T value)
        {
            Value = value;
        }

        public T Method(T arg)
        {
            return arg;
        }

        public Random Method(Random arg)
        {
            return arg;
        }

        public T Value { get; }

        public T NullValue => default;

        [ScriptMember(ScriptMemberFlags.WrapNullResult)]
        public T WrappedNullValue => NullValue;
    }

    public class DefaultPropertyTestObject
    {
        private readonly Dictionary<string, object> byName = new();
        private readonly Dictionary<DayOfWeek, object> byDay = new();

        public object this[string name]
        {
            get => ((IDictionary)byName)[name];
            set => byName[name] = value;
        }

        public object this[DayOfWeek day]
        {
            get => ((IDictionary)byDay)[day];
            set => byDay[day] = value;
        }

        [DispId(0)]
        public int Value { get; set; }
    }

    public class DefaultPropertyTestContainer
    {
        public readonly DefaultPropertyTestObject Field = new();

        public DefaultPropertyTestObject Property => Field;

        public DefaultPropertyTestObject Method()
        {
            return Field;
        }
    }

    public static partial class TestUtil
    {
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
            var hashCode = args.Aggregate(callerGuid.GetHashCode(), (currentHashCode, value) => unchecked((currentHashCode * 31) + (value?.GetHashCode() ?? 0)));
            return hashCode * Math.E / Math.PI;
        }

        public static void AssertException<TException>(Action action, bool checkScriptStackTrace = true) where TException : Exception
        {
            Exception caughtException = null;
            var gotExpectedException = false;

            try
            {
                action();
            }
            catch (TException exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                caughtException = exception;
                gotExpectedException = exception.GetBaseException() is TException;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            var message = "Expected " + typeof(TException).Name + " was not thrown.";
            if (caughtException is not null)
            {
                message += " " + caughtException.GetType().Name + " was thrown instead.";
            }

            Assert.IsTrue(gotExpectedException, message);
        }

        public static void AssertException<TArg, TException>(Action<TArg> action, in TArg arg, bool checkScriptStackTrace = true) where TException : Exception
        {
            Exception caughtException = null;
            var gotExpectedException = false;

            try
            {
                action(arg);
            }
            catch (TException exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                caughtException = exception;
                gotExpectedException = exception.GetBaseException() is TException;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            var message = "Expected " + typeof(TException).Name + " was not thrown.";
            if (caughtException is not null)
            {
                message += " " + caughtException.GetType().Name + " was thrown instead.";
            }

            Assert.IsTrue(gotExpectedException, message);
        }

        public static void AssertException<TArg, TException1, TException2>(Action<TArg> action, in TArg arg, bool checkScriptStackTrace = true) where TException1 : Exception where TException2 : Exception
        {
            Exception caughtException = null;
            var gotExpectedException = false;

            try
            {
                action(arg);
            }
            catch (TException1 exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (TException2 exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                caughtException = exception;
                gotExpectedException = (exception.GetBaseException() is TException1) || (exception.GetBaseException() is TException2);
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            var message = "Expected " + typeof(TException1).Name + " or " + typeof(TException2).Name + " was not thrown.";
            if (caughtException is not null)
            {
                message += " " + caughtException.GetType().Name + " was thrown instead.";
            }

            Assert.IsTrue(gotExpectedException, message);
        }

        public static void AssertException<TException1, TException2>(Action action, bool checkScriptStackTrace = true) where TException1 : Exception where TException2 : Exception
        {
            Exception caughtException = null;
            var gotExpectedException = false;

            try
            {
                action();
            }
            catch (TException1 exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (TException2 exception)
            {
                caughtException = exception;
                gotExpectedException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                caughtException = exception;
                gotExpectedException = (exception.GetBaseException() is TException1) || (exception.GetBaseException() is TException2);
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            var message = "Expected " + typeof(TException1).Name + " or " + typeof(TException2).Name + " was not thrown.";
            if (caughtException is not null)
            {
                message += " " + caughtException.GetType().Name + " was thrown instead.";
            }

            Assert.IsTrue(gotExpectedException, message);
        }

        public static void AssertMethodBindException(Action action, bool checkScriptStackTrace = true)
        {
            AssertException<RuntimeBinderException, MissingMethodException>(action, checkScriptStackTrace);
        }

        public static void AssertValidException(Exception exception)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.StackTrace));
            Assert.IsFalse(exception.Message.Contains("COM"));

            if (exception is not IOException)
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
            while (exception is not null)
            {
                if (exception is IScriptEngineException scriptError)
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

        public static string GetCOMObjectTypeName(object obj)
        {
            if (obj is IDispatch dispatch)
            {
                var typeInfo = dispatch.GetTypeInfo();
                if (typeInfo is not null)
                {
                    return typeInfo.GetName();
                }
            }

            return null;
        }
    }

    public static class TestUtil<TException> where TException : Exception
    {
        public static void AssertException(Action action, bool checkScriptStackTrace = true) => TestUtil.AssertException<TException>(action, checkScriptStackTrace);

        public static void AssertException<TArg>(Action<TArg> action, in TArg arg, bool checkScriptStackTrace = true) => TestUtil.AssertException<TArg, TException>(action, arg, checkScriptStackTrace);
    }

    public static class TestUtil<TException1, TException2> where TException1 : Exception where TException2 : Exception
    {
        public static void AssertException(Action action, bool checkScriptStackTrace = true) => TestUtil.AssertException<TException1, TException2>(action, checkScriptStackTrace);

        public static void AssertException<TArg>(Action<TArg> action, in TArg arg, bool checkScriptStackTrace = true) => TestUtil.AssertException<TArg, TException1, TException2>(action, arg, checkScriptStackTrace);
    }
}
