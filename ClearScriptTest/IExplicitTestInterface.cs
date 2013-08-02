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
