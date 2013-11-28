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
using System.Linq;
using Microsoft.ClearScript.Util;
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

    public static class TestUtil
    {
        public static double CalcTestValue(Guid callerGuid, params object[] args)
        {
            var hashCode = args.Aggregate(callerGuid.GetHashCode(), (currentHashCode, value) => unchecked((currentHashCode * 31) + ((value != null) ? value.GetHashCode() : 0)));
            return hashCode * Math.E / Math.PI;
        }

        public static void AssertException<T>(Action action, bool checkScriptStackTrace = true) where T : Exception
        {
            var gotException = false;

            try
            {
                action();
            }
            catch (T exception)
            {
                gotException = true;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }
            catch (Exception exception)
            {
                gotException = exception.GetBaseException() is T;
                AssertValidExceptionChain(exception, checkScriptStackTrace);
            }

            Assert.IsTrue(gotException, "Expected " + typeof(T).Name + " was not thrown.");
        }

        public static void AssertValidException(Exception exception)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.Message));
            Assert.IsFalse(exception.Message.Contains("COM"));
            Assert.IsFalse(exception.Message.Contains("HRESULT"));
            Assert.IsFalse(exception.Message.Contains("0x"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(exception.StackTrace));
        }

        public static void AssertValidException(IScriptEngineException exception, bool checkScriptStackTrace = true)
        {
            AssertValidException((Exception)exception);
            if ((exception is ScriptEngineException) && !exception.IsFatal && (exception.HResult != RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION))
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
