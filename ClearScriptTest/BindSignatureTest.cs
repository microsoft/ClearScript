// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    [TestClass]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test classes use TestCleanupAttribute for deterministic teardown.")]
    public class BindSignatureTest : ClearScriptTest
    {
        #region test methods

        [TestMethod, TestCategory("BindSignature")]
        public void BindSignature_General()
        {
            var typeArgs1 = new[]
            {
                typeof(Random),
                typeof(string),
                typeof(Dictionary<string, DateTime>)
            };

            var typeArgs2 = new[]
            {
                typeof(Random),
                typeof(string),
                typeof(Dictionary<string, TimeSpan>)
            };

            var args1 = new object[]
            {
                int.MinValue,
                Math.E,
                "blah",
                new OutArg<DateTime>(DateTime.MinValue),
                new RefArg<TimeSpan>(TimeSpan.MinValue),
                DayOfWeek.Sunday
            };

            var args2 = new object[]
            {
                int.MaxValue,
                Math.PI,
                "meh",
                new OutArg<DateTime>(DateTime.MaxValue),
                new RefArg<TimeSpan>(TimeSpan.MaxValue),
                DayOfWeek.Saturday
            };

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(ClearScriptTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, new HostVariable<string>(null), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, new HostVariable<string>(null), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostObject.Wrap("baz"), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostObject.Wrap("baz"), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, new HostVariable<string>(null), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, new HostVariable<string>("baz"), "foo", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, new HostVariable<string>("baz"), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostObject.Wrap("qux"), "foo", typeArgs1, args1);
                AssertEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "bar", typeArgs1, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs2, args1);
                AssertNotEqual(sig1, sig2);
            }

            {
                var sig1 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args1);
                var sig2 = new BindSignature(typeof(BindSignatureTest), BindingFlags.Instance | BindingFlags.Public, HostType.Wrap(typeof(string)), "foo", typeArgs1, args2);
                AssertEqual(sig1, sig2);
            }
        }

        #endregion

        #region miscellaneous

        private static void AssertEqual(BindSignature sig1, BindSignature sig2)
        {
            Assert.AreEqual(sig1, sig2);
            Assert.AreEqual(sig1.GetHashCode(), sig2.GetHashCode());
        }

        private static void AssertNotEqual(BindSignature sig1, BindSignature sig2)
        {
            Assert.AreNotEqual(sig1, sig2);
            Assert.AreNotEqual(sig1.GetHashCode(), sig2.GetHashCode());
        }

        #endregion
    }
}
