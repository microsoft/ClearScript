// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Test
{
    public static class StaticTestClass
    {
        public static int[] StaticField;
        public static short StaticScalarField;
        public static TestEnum StaticEnumField;
        public static TimeSpan StaticStructField;

        public static int[] StaticProperty { get; set; }
        public static short StaticScalarProperty { get; set; }
        public static TestEnum StaticEnumProperty { get; set; }
        public static TimeSpan StaticStructProperty { get; set; }

        public static byte StaticReadOnlyProperty
        {
            get { return 93; }
        }

        public static event EventHandler<TestEventArgs<short>> StaticEvent;
        public static void StaticFireEvent(short arg)
        {
            if (StaticEvent != null)
            {
                StaticEvent(typeof(StaticTestClass), new TestEventArgs<short> { Arg = arg });
            }
        }

        public static double StaticMethod(string arg1, int arg2)
        {
            return TestUtil.CalcTestValue(new Guid("e9bc20b4-3f24-4c99-b2dd-34d7e533bc10"), typeof(StaticTestClass), arg1.Length, arg2);
        }

        public static double StaticMethod<T>(string arg1, int arg2, T arg3) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("1f78dcb0-ecd9-4497-b67a-88684a5b3352"), typeof(StaticTestClass), arg1.Length, arg2, arg3.ToString().Length);
        }

        public static double StaticMethod<T>(int arg) where T : struct
        {
            return TestUtil.CalcTestValue(new Guid("6298f0a4-01c7-4600-870b-99917b92a4de"), typeof(StaticTestClass), typeof(T).Name.Length, arg);
        }

        public static double StaticBindTestMethod<T>(T arg)
        {
            return TestUtil.CalcTestValue(new Guid("d1e979f2-47cd-44fa-a21a-7b215f18bd67"), typeof(StaticTestClass), typeof(T), arg);
        }
    }
}
