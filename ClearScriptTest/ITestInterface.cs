// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Test
{
    public interface ITestInterface
    {
        int[] InterfaceProperty { get; set; }
        short InterfaceScalarProperty { get; set; }
        TestEnum InterfaceEnumProperty { get; set; }
        TimeSpan InterfaceStructProperty { get; set; }
        byte InterfaceReadOnlyProperty { get; }

        event EventHandler<TestEventArgs<short>> InterfaceEvent;
        void InterfaceFireEvent(short arg);

        double InterfaceMethod(string arg1, int arg2);
        double InterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct;
        double InterfaceMethod<T>(int arg) where T : struct;
        double InterfaceBindTestMethod<T>(T arg);
    }

    public static class TestInterfaceExtensions
    {
        public static double InterfaceExtensionMethod(this ITestInterface self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("59c058fc-86c0-49ec-a686-1eda84a902a2"), self, arg1.Length, arg2);
        }

        public static double InterfaceExtensionMethod<T>(this ITestInterface self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("b7de0341-a6ed-475f-83b8-9ce3fa3cbe38"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double InterfaceExtensionMethod<T>(this ITestInterface self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("9101e026-40d9-43cc-a52d-c5264f168e28"), self, typeof(T).Name.Length, arg);
        }

        public static double InterfaceExtensionBindTestMethod<T>(this ITestInterface self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("84e8f577-58c3-42c5-aed3-ca0f62ccb291"), self, typeof(T), arg);
        }
    }
}
