// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Test
{
    public interface IBaseTestInterface
    {
        int[] BaseInterfaceProperty { get; set; }
        short BaseInterfaceScalarProperty { get; set; }
        TestEnum BaseInterfaceEnumProperty { get; set; }
        TimeSpan BaseInterfaceStructProperty { get; set; }
        byte BaseInterfaceReadOnlyProperty { get; }

        event EventHandler<TestEventArgs<short>> BaseInterfaceEvent;
        void BaseInterfaceFireEvent(short arg);

        double BaseInterfaceMethod(string arg1, int arg2);
        double BaseInterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct;
        double BaseInterfaceMethod<T>(int arg) where T : struct;
        double BaseInterfaceBindTestMethod<T>(T arg);
    }

    public static class BaseTestInterfaceExtensions
    {
        public static double BaseInterfaceExtensionMethod(this IBaseTestInterface self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("4b62c135-85fe-40fd-94f3-b97a8961bd7e"), self, arg1.Length, arg2);
        }

        public static double BaseInterfaceExtensionMethod<T>(this IBaseTestInterface self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("25091ed2-88a2-4efb-aceb-b8c1c6aeb66e"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double BaseInterfaceExtensionMethod<T>(this IBaseTestInterface self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("589b2ea2-cec8-4ee2-9d25-684d8e3b8e2a"), self, typeof(T).Name.Length, arg);
        }

        public static double BaseInterfaceExtensionBindTestMethod<T>(this IBaseTestInterface self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("5fb43a52-2268-430c-9f90-5dfa06d98603"), self, typeof(T), arg);
        }
    }
}
