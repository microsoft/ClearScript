// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Test
{
    public interface IExplicitBaseTestInterface
    {
        int[] ExplicitBaseInterfaceProperty { get; set; }
        short ExplicitBaseInterfaceScalarProperty { get; set; }
        TestEnum ExplicitBaseInterfaceEnumProperty { get; set; }
        TimeSpan ExplicitBaseInterfaceStructProperty { get; set; }
        byte ExplicitBaseInterfaceReadOnlyProperty { get; }

        event EventHandler<TestEventArgs<short>> ExplicitBaseInterfaceEvent;
        void ExplicitBaseInterfaceFireEvent(short arg);

        double ExplicitBaseInterfaceMethod(string arg1, int arg2);
        double ExplicitBaseInterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct;
        double ExplicitBaseInterfaceMethod<T>(int arg) where T : struct;
        double ExplicitBaseInterfaceBindTestMethod<T>(T arg);
    }

    public static class ExplicitBaseTestInterfaceExtensions
    {
        public static double ExplicitBaseInterfaceExtensionMethod(this IExplicitBaseTestInterface self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("7cc1fa3e-6193-4914-9e0e-cff8a84e9beb"), self, arg1.Length, arg2);
        }

        public static double ExplicitBaseInterfaceExtensionMethod<T>(this IExplicitBaseTestInterface self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("5db749b9-bc1a-408c-a630-4c3aaa177a26"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double ExplicitBaseInterfaceExtensionMethod<T>(this IExplicitBaseTestInterface self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("ee25eedb-4a80-4db6-9c5e-5fe79178b9be"), self, typeof(T).Name.Length, arg);
        }

        public static double ExplicitBaseInterfaceExtensionBindTestMethod<T>(this IExplicitBaseTestInterface self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("a6815002-5517-43c3-94bc-282d53c32cb3"), self, typeof(T), arg);
        }
    }
}
