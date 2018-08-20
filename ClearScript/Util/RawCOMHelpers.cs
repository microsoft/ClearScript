// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class RawCOMHelpers
    {
        public static readonly int VariantSize = sizeof(ushort) * 4 + IntPtr.Size * 2;

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

            public static readonly int E_NOINTERFACE = 0x80004002U.ToSigned();
            public static readonly int E_ABORT = 0x80004004U.ToSigned();
            public static readonly int E_FAIL = 0x80004005U.ToSigned();
            public static readonly int E_INVALIDARG = 0x80070057U.ToSigned();

            public static readonly int DISP_E_UNKNOWNNAME = 0x80020006U.ToSigned();
            public static readonly int DISP_E_MEMBERNOTFOUND = 0x80020003U.ToSigned();
            public static readonly int SCRIPT_E_REPORTED = 0x80020101U.ToSigned();

            public static readonly int CLEARSCRIPT_E_HOSTEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xBAFF);
            public static readonly int CLEARSCRIPT_E_SCRIPTITEMEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xB0FF);

            // ReSharper restore InconsistentNaming

            public static void Check(uint result)
            {
                Check(result.ToSigned());
            }

            public static void Check(int result)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            public static bool Succeeded(uint result)
            {
                return GetSeverity(result) == SEVERITY_SUCCESS;
            }

            public static bool Succeeded(int result)
            {
                return GetSeverity(result) == SEVERITY_SUCCESS;
            }

            public static int GetSeverity(uint result)
            {
                return GetSeverity(result.ToSigned());
            }

            public static int GetSeverity(int result)
            {
                return (result >> 31) & 0x1;
            }

            public static int GetFacility(uint result)
            {
                return GetFacility(result.ToSigned());
            }

            public static int GetFacility(int result)
            {
                return (result >> 16) & 0x1FFF;
            }

            public static int GetCode(uint result)
            {
                return GetCode(result.ToSigned());
            }

            public static int GetCode(int result)
            {
                return result & 0xFFFF;
            }

            public static int MakeResult(int severity, int facility, int code)
            {
                return ((uint)(code & 0xFFFF) | ((uint)(facility & 0x1FFF) << 16) | ((uint)(severity & 0x1) << 31)).ToSigned();
            }
        }

        #endregion
    }
}
