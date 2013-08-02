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
