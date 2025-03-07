// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class BugFixTest
    {
        #region test methods

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

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_MultipleAppDomains()
        {
            TestUtil.InvokeConsoleTest("BugFix_MultipleAppDomains");
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_XMLDOM_Enumeration_Attributes_JScript()
        {
            var document = new MSXML2.DOMDocument60Class();
            document.loadXML(@"
                <document>
                    <page id=""123""/>
                    <separator/>
                    <page id=""456""/>
                    <page id=""789""/>
                    <page id=""987""/>
                    <separator/>
                    <page id=""654""/>
                    <page id=""321""/>
                    <page id=""135""/>
                    <separator/>
                    <page id=""246""/>
                    <page id=""357""/>
                    <page id=""468""/>
                    <separator/>
                    <page id=""579""/>
                </document>
            ");

            engine.Dispose();
            engine = new JScriptEngine();

            engine.AddHostObject("document", document);
            engine.Execute(@"
                allPages = document.getElementsByTagName('page');
                total = 0;
                for (var e = new Enumerator(allPages); !e.atEnd(); e.moveNext()) {
                    total = total + parseInt(e.item().getAttribute('id'))
                }
            ");

            Assert.AreEqual(5115, engine.Script.total);
        }

        [TestMethod, TestCategory("BugFix")]
        public void BugFix_XMLDOM_Enumeration_Attributes_VBScript()
        {
            var document = new MSXML2.DOMDocument60Class();
            document.loadXML(@"
                <document>
                    <page id=""123""/>
                    <separator/>
                    <page id=""456""/>
                    <page id=""789""/>
                    <page id=""987""/>
                    <separator/>
                    <page id=""654""/>
                    <page id=""321""/>
                    <page id=""135""/>
                    <separator/>
                    <page id=""246""/>
                    <page id=""357""/>
                    <page id=""468""/>
                    <separator/>
                    <page id=""579""/>
                </document>
            ");

            engine.Dispose();
            engine = new VBScriptEngine();

            engine.AddHostObject("document", document);
            engine.Execute(@"
                set allPages = document.getElementsByTagName(""page"")
                total = 0
                for each page in allPages
                    total = total + CInt(page.getAttribute(""id""))
                next
            ");

            Assert.AreEqual(5115, engine.Script.total);
        }

        #endregion
    }
}
