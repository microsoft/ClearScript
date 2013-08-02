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
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class RawCOMHelpers
    {
        public static IntPtr CreateInstance<T>(string progID)
        {
            IntPtr pInterface;
            var clsid = CLSIDFromProgID(progID);
            var iid = typeof(T).GUID;
            HResult.Check(NativeMethods.CoCreateInstance(ref clsid, IntPtr.Zero, 1, ref iid, out pInterface));
            return pInterface;
        }

        public static IntPtr QueryInterface<T>(IntPtr pUnknown)
        {
            IntPtr pInterface;
            var iid = typeof(T).GUID;
            HResult.Check(Marshal.QueryInterface(pUnknown, ref iid, out pInterface));
            return pInterface;
        }

        public static IntPtr QueryInterfaceNoThrow<T>(IntPtr pUnknown)
        {
            IntPtr pInterface;
            var iid = typeof(T).GUID;
            var result = Marshal.QueryInterface(pUnknown, ref iid, out pInterface);
            return (result == HResult.S_OK) ? pInterface : IntPtr.Zero;
        }

        public static T GetMethodDelegate<T>(IntPtr pInterface, int methodIndex) where T : class
        {
            var pVTable = Marshal.ReadIntPtr(pInterface);
            var pMethod = Marshal.ReadIntPtr(pVTable + methodIndex * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer(pMethod, typeof(T)) as T;
        }

        public static void ReleaseAndEmpty(ref IntPtr pInterface)
        {
            if (pInterface != IntPtr.Zero)
            {
                Marshal.Release(pInterface);
                pInterface = IntPtr.Zero;
            }
        }

        private static Guid CLSIDFromProgID(string progID)
        {
            Guid clsid;
            if (!Guid.TryParseExact(progID, "B", out clsid))
            {
                HResult.Check(NativeMethods.CLSIDFromProgID(progID, out clsid));
            }

            return clsid;
        }

        #region Nested type: NativeMethods

        private static class NativeMethods
        {
            [DllImport("ole32.dll", ExactSpelling = true)]
            public static extern uint CoCreateInstance(
                [In] ref Guid clsid,
                [In] IntPtr pOuter,
                [In] uint clsContext,
                [In] ref Guid iid,
                [Out] out IntPtr pInterface
            );

            [DllImport("ole32.dll", ExactSpelling = true)]
            public static extern uint CLSIDFromProgID(
                [In] [MarshalAs(UnmanagedType.LPWStr)] string progID,
                [Out] out Guid clsid
            );
        }

        #endregion

        #region Nested type: HResult

        public static class HResult
        {
            // ReSharper disable InconsistentNaming

            public const int SEVERITY_SUCCESS = 0;
            public const int SEVERITY_ERROR = 1;

            public const int FACILITY_NULL = 0;
            public const int FACILITY_RPC = 1;
            public const int FACILITY_DISPATCH = 2;
            public const int FACILITY_STORAGE = 3;
            public const int FACILITY_ITF = 4;
            public const int FACILITY_WIN32 = 7;
            public const int FACILITY_WINDOWS = 8;
            public const int FACILITY_CONTROL = 10;
            public const int FACILITY_INTERNET = 12;
            public const int FACILITY_URT = 19;

            public const int S_OK = 0;
            public const int S_FALSE = 1;

            public static readonly int E_NOINTERFACE = MiscHelpers.UnsignedAsSigned(0x80004002U);
            public static readonly int E_ABORT = MiscHelpers.UnsignedAsSigned(0x80004004U);
            public static readonly int E_INVALIDARG = MiscHelpers.UnsignedAsSigned(0x80070057U);

            public static readonly int DISP_E_MEMBERNOTFOUND = MiscHelpers.UnsignedAsSigned(0x80020003U);
            public static readonly int SCRIPT_E_REPORTED = MiscHelpers.UnsignedAsSigned(0x80020101U);

            public static readonly int CLEARSCRIPT_E_HOSTEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xBAFF);
            public static readonly int CLEARSCRIPT_E_SCRIPTITEMEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xB0FF);

            // ReSharper restore InconsistentNaming

            public static void Check(uint result)
            {
                Check(MiscHelpers.UnsignedAsSigned(result));
            }

            public static void Check(int result)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            public static int GetSeverity(uint result)
            {
                return GetSeverity(MiscHelpers.UnsignedAsSigned(result));
            }

            public static int GetSeverity(int result)
            {
                return (result >> 31) & 0x1;
            }

            public static int GetFacility(uint result)
            {
                return GetFacility(MiscHelpers.UnsignedAsSigned(result));
            }

            public static int GetFacility(int result)
            {
                return (result >> 16) & 0x1FFF;
            }

            public static int GetCode(uint result)
            {
                return GetCode(MiscHelpers.UnsignedAsSigned(result));
            }

            public static int GetCode(int result)
            {
                return result & 0xFFFF;
            }

            public static int MakeResult(int severity, int facility, int code)
            {
                return MiscHelpers.UnsignedAsSigned((uint)(code & 0xFFFF) | ((uint)(facility & 0x1FFF) << 16) | ((uint)(severity & 0x1) << 31));
            }
        }

        #endregion
    }
}
