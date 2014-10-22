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
