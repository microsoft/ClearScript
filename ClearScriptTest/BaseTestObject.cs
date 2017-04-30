// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

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

        public double BaseMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("d5114f53-ca6a-4993-8117-7f8194088c08"), this, arg1.Length, arg2);
        }

        public double BaseMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("d18ddd76-a035-44b4-b18b-7c4c7d313c4f"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double BaseMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("53b0fe2a-6b7c-4516-93de-db706b1bf1bb"), this, typeof(T).Name.Length, arg);
        }

        public double BaseBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("c0f52143-a775-4b71-b206-a759285a35a5"), this, typeof(T), arg);
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

        public double BaseInterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("f58f24cd-1d08-4224-bf78-7dcdb01b733f"), this, arg1.Length, arg2);
        }

        public double BaseInterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("73cd3acf-3c94-4993-8786-70ba0f86f1a7"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        public double BaseInterfaceMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("28cf53e3-6422-4ac8-82d6-0d3d00e7bc1d"), this, typeof(T).Name.Length, arg);
        }

        public double BaseInterfaceBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("302f0d74-7ee8-4e0c-b383-7816d79a889b"), this, typeof(T), arg);
        }

        #endregion

        #region Implementation of IExplicitBaseTestInterface

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        int[] IExplicitBaseTestInterface.ExplicitBaseInterfaceProperty { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        short IExplicitBaseTestInterface.ExplicitBaseInterfaceScalarProperty { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        TestEnum IExplicitBaseTestInterface.ExplicitBaseInterfaceEnumProperty { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        TimeSpan IExplicitBaseTestInterface.ExplicitBaseInterfaceStructProperty { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        byte IExplicitBaseTestInterface.ExplicitBaseInterfaceReadOnlyProperty
        {
            get { return 17; }
        }

        private event EventHandler<TestEventArgs<short>> ExplicitBaseInterfaceEventImpl;
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        event EventHandler<TestEventArgs<short>> IExplicitBaseTestInterface.ExplicitBaseInterfaceEvent
        {
            add { ExplicitBaseInterfaceEventImpl += value; }
            remove { ExplicitBaseInterfaceEventImpl -= value; }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        void IExplicitBaseTestInterface.ExplicitBaseInterfaceFireEvent(short arg)
        {
            if (ExplicitBaseInterfaceEventImpl != null)
            {
                ExplicitBaseInterfaceEventImpl(this, new TestEventArgs<short> { Arg = arg });
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("354a43bc-7d15-4aeb-b4c9-c6e03893c5f2"), this, arg1.Length, arg2);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod<T>(string arg1, int arg2, T arg3)
        {
            return TestUtil.CalcTestValue(new Guid("b6496578-af16-4424-b16b-808de442d9e3"), this, arg1.Length, arg2, arg3.ToString().Length);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        double IExplicitBaseTestInterface.ExplicitBaseInterfaceMethod<T>(int arg)
        {
            return TestUtil.CalcTestValue(new Guid("b9810875-3ccf-400f-b106-d2869905f9bc"), this, typeof(T).Name.Length, arg);
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "This member requires explicit implementation for testing purposes.")]
        double IExplicitBaseTestInterface.ExplicitBaseInterfaceBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("5937707d-9158-4d72-986b-8eb13da5c079"), this, typeof(T), arg);
        }

        #endregion
    }

    public static class BaseTestObjectExtensions
    {
        public static double BaseExtensionMethod(this BaseTestObject self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("ffac885b-0e3b-4438-99e1-64f4d2c6f769"), self, arg1.Length, arg2);
        }

        public static double BaseExtensionMethod<T>(this BaseTestObject self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("6ee07aa3-4548-4d59-b7d6-77725bb2c900"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double BaseExtensionMethod<T>(this BaseTestObject self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("2db0feaf-8618-4676-a7ba-552a20853fcd"), self, typeof(T).Name.Length, arg);
        }

        public static double BaseExtensionBindTestMethod<T>(this BaseTestObject self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("fdef26a4-2155-4be5-a245-4810ae66c491"), self, typeof(T), arg);
        }
    }
}
