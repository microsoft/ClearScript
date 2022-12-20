// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ClearScript.Test
{
    public partial class V8ScriptEngineTest
    {
        #region test methods

        // ReSharper disable InconsistentNaming

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject_Iteration()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject_Iteration_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                var list = new ArrayList();

                engine.Script.host = new ExtendedHostFunctions();
                engine.Script.list = list;
                engine.Execute(@"
                    fso = host.newComObj('Scripting.FileSystemObject');
                    drives = fso.drives;
                    for (drive of drives) {
                        list.add(drive.path);
                    }
                ");

                var drives = DriveInfo.GetDrives();
                Assert.AreEqual(drives.Length, list.Count);
                Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject_Iteration_DisableTypeRestriction()
        {
            engine.DisableTypeRestriction = true;

            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_FileSystemObject_TypeLibEnums()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                fso = host.newComObj('Scripting.FileSystemObject');
                enums = host.typeLibEnums(fso);
            ");

            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.BinaryCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.BinaryCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.DatabaseCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.DatabaseCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.TextCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.TextCompare)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForAppending), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForAppending)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForReading), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForReading)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForWriting), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForWriting)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateFalse), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateFalse)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateMixed), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateMixed)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateTrue), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateTrue)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateUseDefault), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateUseDefault)"));

            engine.Execute(@"
                function writeFile(contents) {
                    var name = fso.GetTempName();
                    var path = fso.GetSpecialFolder(enums.Scripting.SpecialFolderConst.TemporaryFolder).Path + '\\' + name;
                    var stream = fso.OpenTextFile(path, enums.Scripting.IOMode.ForWriting, true, enums.Scripting.Tristate.TristateTrue);
                    stream.Write(contents);
                    stream.Close();
                    return path;
                }
            ");

            var contents = Guid.NewGuid().ToString();
            var path = engine.Script.writeFile(contents);
            Assert.IsTrue(new FileInfo(path).Length >= (contents.Length * 2));
            Assert.AreEqual(contents, File.ReadAllText(path));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMObject_Dictionary()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                dict = host.newComObj('Scripting.Dictionary');
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject_Iteration()
        {
            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject_Iteration_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                var list = new ArrayList();

                engine.Script.host = new ExtendedHostFunctions();
                engine.Script.list = list;
                engine.Execute(@"
                    FSO = host.comType('Scripting.FileSystemObject');
                    fso = host.newObj(FSO);
                    drives = fso.drives;
                    for (drive of drives) {
                        list.add(drive.path);
                    }
                ");

                var drives = DriveInfo.GetDrives();
                Assert.AreEqual(drives.Length, list.Count);
                Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject_Iteration_DisableTypeRestriction()
        {
            engine.DisableTypeRestriction = true;

            var list = new ArrayList();

            engine.Script.host = new ExtendedHostFunctions();
            engine.Script.list = list;
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_FileSystemObject_TypeLibEnums()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                FSO = host.comType('Scripting.FileSystemObject');
                fso = host.newObj(FSO);
                enums = host.typeLibEnums(fso);
            ");

            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.BinaryCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.BinaryCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.DatabaseCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.DatabaseCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.TextCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.TextCompare)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForAppending), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForAppending)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForReading), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForReading)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForWriting), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForWriting)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateFalse), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateFalse)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateMixed), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateMixed)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateTrue), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateTrue)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateUseDefault), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateUseDefault)"));

            engine.Execute(@"
                function writeFile(contents) {
                    var name = fso.GetTempName();
                    var path = fso.GetSpecialFolder(enums.Scripting.SpecialFolderConst.TemporaryFolder).Path + '\\' + name;
                    var stream = fso.OpenTextFile(path, enums.Scripting.IOMode.ForWriting, true, enums.Scripting.Tristate.TristateTrue);
                    stream.Write(contents);
                    stream.Close();
                    return path;
                }
            ");

            var contents = Guid.NewGuid().ToString();
            var path = engine.Script.writeFile(contents);
            Assert.IsTrue(new FileInfo(path).Length >= (contents.Length * 2));
            Assert.AreEqual(contents, File.ReadAllText(path));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_COMType_Dictionary()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.Execute(@"
                Dict = host.comType('Scripting.Dictionary');
                dict = host.newObj(Dict);
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMObject("fso", "Scripting.FileSystemObject");
            engine.Execute(@"
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject_Iteration()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMObject("fso", "Scripting.FileSystemObject");
            engine.Execute(@"
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject_Iteration_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                var list = new ArrayList();

                engine.Script.list = list;
                engine.AddCOMObject("fso", "Scripting.FileSystemObject");
                engine.Execute(@"
                    drives = fso.drives;
                    for (drive of drives) {
                        list.add(drive.path);
                    }
                ");

                var drives = DriveInfo.GetDrives();
                Assert.AreEqual(drives.Length, list.Count);
                Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject_Iteration_DisableTypeRestriction()
        {
            engine.DisableTypeRestriction = true;

            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMObject("fso", "Scripting.FileSystemObject");
            engine.Execute(@"
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_FileSystemObject_TypeLibEnums()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.AddCOMObject("fso", "Scripting.FileSystemObject");
            engine.Execute(@"
                enums = host.typeLibEnums(fso);
            ");

            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.BinaryCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.BinaryCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.DatabaseCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.DatabaseCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.TextCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.TextCompare)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForAppending), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForAppending)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForReading), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForReading)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForWriting), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForWriting)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateFalse), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateFalse)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateMixed), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateMixed)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateTrue), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateTrue)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateUseDefault), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateUseDefault)"));

            engine.Execute(@"
                function writeFile(contents) {
                    var name = fso.GetTempName();
                    var path = fso.GetSpecialFolder(enums.Scripting.SpecialFolderConst.TemporaryFolder).Path + '\\' + name;
                    var stream = fso.OpenTextFile(path, enums.Scripting.IOMode.ForWriting, true, enums.Scripting.Tristate.TristateTrue);
                    stream.Write(contents);
                    stream.Close();
                    return path;
                }
            ");

            var contents = Guid.NewGuid().ToString();
            var path = engine.Script.writeFile(contents);
            Assert.IsTrue(new FileInfo(path).Length >= (contents.Length * 2));
            Assert.AreEqual(contents, File.ReadAllText(path));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMObject_Dictionary()
        {
            engine.AddCOMObject("dict", new Guid("{ee09b103-97e0-11cf-978f-00a02463e06f}"));
            engine.Execute(@"
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMType("FSO", "Scripting.FileSystemObject");
            engine.Execute(@"
                fso = new FSO();
                drives = fso.Drives;
                e = drives.GetEnumerator();
                while (e.MoveNext()) {
                    list.Add(e.Current.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject_Iteration()
        {
            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMType("FSO", "Scripting.FileSystemObject");
            engine.Execute(@"
                fso = new FSO();
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject_Iteration_GlobalRenaming()
        {
            using (Scope.Create(() => HostSettings.CustomAttributeLoader, loader => HostSettings.CustomAttributeLoader = loader))
            {
                HostSettings.CustomAttributeLoader = new CamelCaseAttributeLoader();

                var list = new ArrayList();

                engine.Script.list = list;
                engine.AddCOMType("FSO", "Scripting.FileSystemObject");
                engine.Execute(@"
                    fso = new FSO();
                    drives = fso.drives;
                    for (drive of drives) {
                        list.add(drive.path);
                    }
                ");

                var drives = DriveInfo.GetDrives();
                Assert.AreEqual(drives.Length, list.Count);
                Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
            }
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject_Iteration_DisableTypeRestriction()
        {
            engine.DisableTypeRestriction = true;

            var list = new ArrayList();

            engine.Script.list = list;
            engine.AddCOMType("FSO", "Scripting.FileSystemObject");
            engine.Execute(@"
                fso = new FSO();
                drives = fso.Drives;
                for (drive of drives) {
                    list.Add(drive.Path);
                }
            ");

            var drives = DriveInfo.GetDrives();
            Assert.AreEqual(drives.Length, list.Count);
            Assert.IsTrue(drives.Select(drive => drive.Name.Substring(0, 2)).SequenceEqual(list.ToArray()));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_FileSystemObject_TypeLibEnums()
        {
            engine.Script.host = new ExtendedHostFunctions();
            engine.AddCOMType("FSO", "Scripting.FileSystemObject");
            engine.Execute(@"
                fso = new FSO();
                enums = host.typeLibEnums(fso);
            ");

            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.BinaryCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.BinaryCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.DatabaseCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.DatabaseCompare)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.CompareMethod.TextCompare), engine.Evaluate("host.toInt32(enums.Scripting.CompareMethod.TextCompare)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForAppending), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForAppending)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForReading), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForReading)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.IOMode.ForWriting), engine.Evaluate("host.toInt32(enums.Scripting.IOMode.ForWriting)"));

            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateFalse), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateFalse)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateMixed), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateMixed)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateTrue), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateTrue)"));
            Assert.AreEqual(Convert.ToInt32(Scripting.Tristate.TristateUseDefault), engine.Evaluate("host.toInt32(enums.Scripting.Tristate.TristateUseDefault)"));

            engine.Execute(@"
                function writeFile(contents) {
                    var name = fso.GetTempName();
                    var path = fso.GetSpecialFolder(enums.Scripting.SpecialFolderConst.TemporaryFolder).Path + '\\' + name;
                    var stream = fso.OpenTextFile(path, enums.Scripting.IOMode.ForWriting, true, enums.Scripting.Tristate.TristateTrue);
                    stream.Write(contents);
                    stream.Close();
                    return path;
                }
            ");

            var contents = Guid.NewGuid().ToString();
            var path = engine.Script.writeFile(contents);
            Assert.IsTrue(new FileInfo(path).Length >= (contents.Length * 2));
            Assert.AreEqual(contents, File.ReadAllText(path));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_Dictionary()
        {
            engine.AddCOMType("Dict", new Guid("{ee09b103-97e0-11cf-978f-00a02463e06f}"));
            engine.Execute(@"
                dict = new Dict();
                dict.Add('foo', Math.PI);
                dict.Add('bar', Math.E);
                dict.Add('baz', 'abc');
            ");

            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual(Math.PI, engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual(Math.E, engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual("abc", engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Item.set('foo', 'pushkin');
                dict.Item.set('bar', 'gogol');
                dict.Item.set('baz', Math.PI * Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('foo')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('foo')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item('bar')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get('bar')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item('baz')"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get('baz')"));

            engine.Execute(@"
                dict.Key.set('foo', 'qux');
                dict.Key.set('bar', Math.PI);
                dict.Key.set('baz', Math.E);
            ");

            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item('qux')"));
            Assert.AreEqual("pushkin", engine.Evaluate("dict.Item.get('qux')"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item(Math.PI)"));
            Assert.AreEqual("gogol", engine.Evaluate("dict.Item.get(Math.PI)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item(Math.E)"));
            Assert.AreEqual(Math.PI * Math.E, engine.Evaluate("dict.Item.get(Math.E)"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_AddCOMType_XMLHTTP()
        {
            var status = 0;
            string data = null;

            var thread = new Thread(() =>
            {
                using (var testEngine = new V8ScriptEngine(V8ScriptEngineFlags.EnableDebugging))
                {
                    testEngine.Script.onComplete = new Action<int, string>((xhrStatus, xhrData) =>
                    {
                        status = xhrStatus;
                        data = xhrData;
                        Dispatcher.ExitAllFrames();
                    });

                    Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    {
                        // ReSharper disable AccessToDisposedClosure

                        testEngine.AddCOMType("XMLHttpRequest", "MSXML2.XMLHTTP");
                        testEngine.Execute(@"
                            xhr = new XMLHttpRequest();
                            xhr.open('POST', 'http://httpbin.org/post', true);
                            xhr.onreadystatechange = function () {
                                if (xhr.readyState == 4) {
                                    onComplete(xhr.status, JSON.parse(xhr.responseText).data);
                                }
                            };
                            xhr.send('Hello, world!');
                        ");

                        // ReSharper restore AccessToDisposedClosure
                    }));

                    Dispatcher.Run();
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!thread.Join(TimeSpan.FromSeconds(5)))
            {
                Assert.Inconclusive("The Httpbin service request timed out");
            }

            Assert.AreEqual(200, status);
            Assert.AreEqual("Hello, world!", data);
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_Nothing()
        {
            engine.Script.foo = new Func<object>(() => Nothing.Value);
            Assert.IsTrue((bool)engine.Evaluate("foo() == undefined"));
            Assert.IsTrue((bool)engine.Evaluate("foo() === undefined"));
        }

        [TestMethod, TestCategory("V8ScriptEngine")]
        public void V8ScriptEngine_HeapExpansionMultiplier()
        {
            TestUtil.InvokeConsoleTest("V8ScriptEngine_HeapExpansionMultiplier");
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
