// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using Microsoft.ClearScript.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class BugFixTest
    {
        [TestMethod, TestCategory("BugFix")]
        public void BugFix_AssemblyTableClass()
        {
            Assert.IsTrue(typeof(AssemblyTable.AssemblyTableImpl).IsStatic());
            Assert.IsNotNull(typeof(AssemblyTable.AssemblyTableImpl).TypeInitializer);

            var members = typeof(AssemblyTable.AssemblyTableImpl).GetMembers(BindingFlags.Static | BindingFlags.Public);
            Assert.AreEqual(1, members.Length);
            Assert.AreEqual(MemberTypes.Method, members[0].MemberType);
            Assert.AreEqual("GetFullAssemblyNameImpl", members[0].Name);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_V8StackLimitIntegerOverflow()
        {
            TestUtil.InvokeConsoleTest("BugFix_V8StackLimitIntegerOverflow");
        }
    }
}
