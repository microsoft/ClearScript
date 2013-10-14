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
using System.Globalization;

namespace Microsoft.ClearScript.Test
{
    public sealed class TestObject : BaseTestObject, ITestInterface, IExplicitTestInterface
    {
        public int[] Field;
        public short ScalarField;
        public TestEnum EnumField;
        public TimeSpan StructField;

        public int[] Property { get; set; }
        public short ScalarProperty { get; set; }
        public TestEnum EnumProperty { get; set; }
        public TimeSpan StructProperty { get; set; }

        public byte ReadOnlyProperty
        {
            get { return 123; }
        }

        public event EventHandler<TestEventArgs<short>> Event;
        public void FireEvent(short arg)
        {
            if (Event != null)
            {
                Event(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        public double Method(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("f5b75d68-253b-4597-9464-8574f74750f5"), this, arg1.Length, arg2);
        }

        public double Method<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("823eeeea-48b3-4650-ba3c-077e47622b57"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double Method<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("ef3fafb5-680b-40ba-b9be-a0bb5ea0cba4"), this, typeof(T).Name.Length, arg);
        }

        public double BindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("06407870-c4dc-40f8-95ec-8d743c77c8b2"), this, typeof(T), arg);
        }

        private readonly Dictionary<string, object> dict = new Dictionary<string, object>();

        public object this[string key]
        {
            get { return dict[key]; }
            set { dict[key] = value; }
        }

        public object this[int index]
        {
            get { return dict[index.ToString(CultureInfo.InvariantCulture)]; }
            set { dict[index.ToString(CultureInfo.InvariantCulture)] = value; }
        }

        public object this[object i1, object i2, object i3, object i4]
        {
            get { return dict[string.Join(":", i1, i2, i3, i4)]; }
            set { dict[string.Join(":", i1, i2, i3, i4)] = value; }
        }

        #region Implementation of ITestInterface

        public int[] InterfaceProperty { get; set; }
        public short InterfaceScalarProperty { get; set; }
        public TestEnum InterfaceEnumProperty { get; set; }
        public TimeSpan InterfaceStructProperty { get; set; }

        public byte InterfaceReadOnlyProperty
        {
            get { return 17; }
        }

        public event EventHandler<TestEventArgs<short>> InterfaceEvent;
        public void InterfaceFireEvent(short arg)
        {
            if (InterfaceEvent != null)
            {
                InterfaceEvent(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        public double InterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("a545f94c-a791-47de-a292-b687fe6d0fc3"), this, arg1.Length, arg2);
        }

        public double InterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("216ef56a-aa88-46e2-93d9-bc5153ad2c9e"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double InterfaceMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("62a17d8c-2c9e-4dcd-b14f-c027c773c593"), this, typeof(T).Name.Length, arg);
        }

        public double InterfaceBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("43ce378d-5319-419c-bd62-908cd56dfd85"), this, typeof(T), arg);
        }

        #endregion

        #region Implementation of IExplicitTestInterface

        int[] IExplicitTestInterface.ExplicitInterfaceProperty { get; set; }
        short IExplicitTestInterface.ExplicitInterfaceScalarProperty { get; set; }
        TestEnum IExplicitTestInterface.ExplicitInterfaceEnumProperty { get; set; }
        TimeSpan IExplicitTestInterface.ExplicitInterfaceStructProperty { get; set; }

        byte IExplicitTestInterface.ExplicitInterfaceReadOnlyProperty
        {
            get { return 17; }
        }

        private event EventHandler<TestEventArgs<short>> ExplicitInterfaceEventImpl;
        event EventHandler<TestEventArgs<short>> IExplicitTestInterface.ExplicitInterfaceEvent
        {
            add { ExplicitInterfaceEventImpl += value; }
            remove { ExplicitInterfaceEventImpl -= value; }
        }

        void IExplicitTestInterface.ExplicitInterfaceFireEvent(short arg)
        {
            if (ExplicitInterfaceEventImpl != null)
            {
                ExplicitInterfaceEventImpl(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        double IExplicitTestInterface.ExplicitInterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("3f9634ff-d044-4ad5-96d7-bb2a42aa6aa5"), this, arg1.Length, arg2);
        }

        double IExplicitTestInterface.ExplicitInterfaceMethod<T>(string arg1, int arg2, T arg3)
        {
            return TestUtil.CalcTestValue(new Guid("7b46c24f-8bfa-4fc6-ae57-fcb136f9332e"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        double IExplicitTestInterface.ExplicitInterfaceMethod<T>(int arg)
        {
            return TestUtil.CalcTestValue(new Guid("d6aaecfe-952f-459b-9355-14a17cc66010"), this, typeof(T).Name.Length, arg);
        }

        double IExplicitTestInterface.ExplicitInterfaceBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("353d275f-aead-4f92-8035-1fd620a4a12e"), this, typeof(T), arg);
        }

        #endregion
    }

    public static class TestObjectExtensions
    {
        public static double ExtensionMethod(this TestObject self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("d78a86ce-310f-4d9e-bc3a-9f07dfa1d8e1"), self, arg1.Length, arg2);
        }

        public static double ExtensionMethod<T>(this TestObject self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("b3fad7c5-69fa-49d6-829f-16beb738352d"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double ExtensionMethod<T>(this TestObject self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("f1d7d8ec-998e-413c-9b65-06c396676a4b"), self, typeof(T).Name.Length, arg);
        }

        public static double ExtensionBindTestMethod<T>(this TestObject self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("613ce819-bc84-41c3-a3d6-4efa9a2a3b65"), self, typeof(T), arg);
        }
    }
}
