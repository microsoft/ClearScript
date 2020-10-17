// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.ClearScript.V8;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    public class BadV8DeploymentTest : ClearScriptTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("BadV8Deployment")]
        public void BadV8Deployment_NoNativeLibrary()
        {
            GC.Collect();

            V8Proxy.RunWithDeploymentDir("BadV8Deployment_NoNativeLibrary", () =>
            {
                var moduleNotFoundException = new Win32Exception(126 /*ERROR_MOD_NOT_FOUND*/);
                TypeLoadException caughtException = null;

                try
                {
                    using (new V8ScriptEngine())
                    {
                    }
                }
                catch (TypeLoadException exception)
                {
                    caughtException = exception;
                }
                catch (TargetInvocationException exception)
                {
                    if (exception.InnerException is TypeLoadException typeLoadException)
                    {
                        caughtException = typeLoadException;
                    }
                    else
                    {
                        throw;
                    }
                }

                Assert.IsNotNull(caughtException);
                Assert.IsTrue(caughtException.Message.Contains(moduleNotFoundException.Message));
            });
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
