// 
// Copyright © Microsoft Corporation. All rights reserved.
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
using System.Runtime.CompilerServices;

namespace Microsoft.ClearScript.Test
{
    public class BaseTestObject : IBaseTestInterface, IExplicitBaseTestInterface
    {
        public int[] BaseField;
        public short BaseScalarField;
        public TestEnum BaseEnumField;
        public TimeSpan BaseStructField;

        public int[] BaseProperty { get; set; }
        public short BaseScalarProperty { get; set; }
        public TestEnum BaseEnumProperty { get; set; }
        public TimeSpan BaseStructProperty { get; set; }

        public byte BaseReadOnlyProperty
        {
            get { return 117; }
        }

        public event EventHandler<TestEventArgs<short>> BaseEvent;
        public void BaseFireEvent(short arg)
        {
            if (BaseEvent != null)
            {
                BaseEvent(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public double BaseMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public double BaseMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double BaseMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(this, typeof(T).Name.Length, arg);
        }

        #region Implementation of IBaseTestInterface

        public int[] BaseInterfaceProperty { get; set; }
        public short BaseInterfaceScalarProperty { get; set; }
        public TestEnum BaseInterfaceEnumProperty { get; set; }
        public TimeSpan BaseInterfaceStructProperty { get; set; }

        public byte BaseInterfaceReadOnlyProperty
        {
            get { return 73; }
        }

        public event EventHandler<TestEventArgs<short>> BaseInterfaceEvent;
        public void BaseInterfaceFireEvent(short arg)
        {
            if (BaseInterfaceEvent != null)
            {
                BaseInterfaceEvent(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public double BaseInterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public double BaseInterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double BaseInterfaceMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(this, typeof(T).Name.Length, arg);
        }

        #endregion

        #region Implementation of IExplicitBaseTestInterface

        int[] IExplicitBaseTestInterface.ExplicitBaseInterfaceProperty { get; set; }
        short IExplicitBaseTestInterface.ExplicitBaseInterfaceScalarProperty { get; set; }
        TestEnum IExplicitBaseTestInterface.ExplicitBaseInterfaceEnumProperty { get; set; }
        TimeSpan IExplicitBaseTestInterface.ExplicitBaseInterfaceStructProperty { get; set; }

        byte IExplicitBaseTestInterface.ExplicitBaseInterfaceReadOnlyProperty
        {
            get { return 17; }
        }

        private event EventHandler<TestEventArgs<short>> ExplicitBaseInterfaceEventImpl;
        event EventHandler<TestEventArgs<short>> IExplicitBaseTestInterface.ExplicitBaseInterfaceEvent
        {
            add { ExplicitBaseInterfaceEventImpl += value; }
            remove { ExplicitBaseInterfaceEventImpl -= value; }
        }

        void IExplicitBaseTestInterface.ExplicitBaseInterfaceFireEvent(short arg)
        {
            if (ExplicitBaseInterfaceEventImpl != null)
            {
                ExplicitBaseInterfaceEventImpl(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2);
        }

        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod<T>(string arg1, int arg2, T arg3)
        {
            return TestUtil.CalcTestValue(this, arg1.Length, arg2, arg3.ToString().Length);
        }

        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod<T>(int arg)
        {
            return TestUtil.CalcTestValue(this, typeof(T).Name.Length, arg);
        }

        #endregion
    }

    public static class BaseTestObjectExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static double BaseExtensionMethod(this BaseTestObject self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(self, arg1.Length, arg2);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static double BaseExtensionMethod<T>(this BaseTestObject self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double BaseExtensionMethod<T>(this BaseTestObject self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(self, typeof(T).Name.Length, arg);
        }
    }
}
