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
