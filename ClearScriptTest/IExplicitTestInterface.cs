// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Test
{
    public interface IExplicitTestInterface
    {
        int[] ExplicitInterfaceProperty { get; set; }
        short ExplicitInterfaceScalarProperty { get; set; }
        TestEnum ExplicitInterfaceEnumProperty { get; set; }
        TimeSpan ExplicitInterfaceStructProperty { get; set; }
        byte ExplicitInterfaceReadOnlyProperty { get; }

        event EventHandler<TestEventArgs<short>> ExplicitInterfaceEvent;
        void ExplicitInterfaceFireEvent(short arg);

        double ExplicitInterfaceMethod(string arg1, int arg2);
        double ExplicitInterfaceMethod<T>(string arg1, int arg2, T arg3) where T : struct;
        double ExplicitInterfaceMethod<T>(int arg) where T : struct;
        double ExplicitInterfaceBindTestMethod<T>(T arg);
    }

    public static class ExplicitTestInterfaceExtensions
    {
        public static double ExplicitInterfaceExtensionMethod(this IExplicitTestInterface self, string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("d758e3b9-52e3-46b5-ae5c-4d0b89bddc78"), self, arg1.Length, arg2);
        }

        public static double ExplicitInterfaceExtensionMethod<T>(this IExplicitTestInterface self, string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("185c7082-c55d-435f-b3eb-418f1c27617c"), self, arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double ExplicitInterfaceExtensionMethod<T>(this IExplicitTestInterface self, int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("768b93c3-0a86-4e45-92ab-4d37613d4e09"), self, typeof(T).Name.Length, arg);
        }

        public static double ExplicitInterfaceExtensionBindTestMethod<T>(this IExplicitTestInterface self, T arg)
        {
            return TestUtil.CalcTestValue(new Guid("35c244d4-1473-46ce-a9cf-d633034c967d"), self, typeof(T), arg);
        }
    }
}
