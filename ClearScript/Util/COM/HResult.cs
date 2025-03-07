// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util.COM
{
    internal static class HResult
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

        public static readonly int CO_E_CLASSSTRING = 0x800401F3U.ToSigned();
        public static readonly int REGDB_E_CLASSNOTREG = 0x80040154U.ToSigned();

        public static readonly int DISP_E_MEMBERNOTFOUND = 0x80020003U.ToSigned();
        public static readonly int DISP_E_UNKNOWNNAME = 0x80020006U.ToSigned();
        public static readonly int DISP_E_EXCEPTION = 0x80020009U.ToSigned();
        public static readonly int DISP_E_BADPARAMCOUNT = 0x8002000EU.ToSigned();
        public static readonly int SCRIPT_E_REPORTED = 0x80020101U.ToSigned();

        public static readonly int CLEARSCRIPT_E_HOSTEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xBAFF);
        public static readonly int CLEARSCRIPT_E_SCRIPTITEMEXCEPTION = MakeResult(SEVERITY_ERROR, FACILITY_URT, 0xB0FF);

        public const int ERROR_FILE_EXISTS = 80;
        public static readonly int WIN32_E_FILEEXISTS = MakeResult(SEVERITY_ERROR, FACILITY_WIN32, ERROR_FILE_EXISTS);

        // ReSharper restore InconsistentNaming

        public static void Check(uint result)
        {
            Check(result.ToSigned());
        }

        public static void Check(int result)
        {
            if (Succeeded(result) || !MiscHelpers.Try(out var exception, static result => Marshal.GetExceptionForHR(result), result))
            {
                return;
            }

            if (exception.HResult != result)
            {
                // WORKAROUND: In some .NET test environments, Marshal.GetExceptionForHR sometimes
                // converts COM error codes into unrelated exceptions that break critical features
                // such as double execution prevention (see BugFix_DoubleExecution_JScript et al).

                if (result == SCRIPT_E_REPORTED)
                {
                    throw new COMException("A script error has been reported", result);
                }

                if (result == CLEARSCRIPT_E_HOSTEXCEPTION)
                {
                    throw new COMException("A host exception has been reported", result);
                }
            }

            throw exception;
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
}
