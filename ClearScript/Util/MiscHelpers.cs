// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.Properties;
using INVOKEKIND = System.Runtime.InteropServices.ComTypes.INVOKEKIND;
using TYPEFLAGS = System.Runtime.InteropServices.ComTypes.TYPEFLAGS;

namespace Microsoft.ClearScript.Util
{
    internal static class MiscHelpers
    {
        #region COM helpers

        private static readonly Regex dispIDNameRegex = new(@"^\[DISPID=(-?[0-9]+)\]$");

        public static object CreateCOMObject(string progID, string serverName)
        {
            return Activator.CreateInstance(GetCOMType(progID, serverName));
        }

        public static object CreateCOMObject(Guid clsid, string serverName)
        {
            return Activator.CreateInstance(GetCOMType(clsid, serverName));
        }

        public static bool TryCreateCOMObject<T>(string progID, string serverName, out T obj) where T : class
        {
            if (!TryGetCOMType(progID, serverName, out var type))
            {
                obj = null;
                return false;
            }

            return Try(out obj, () => Activator.CreateInstance(type) as T) && (obj is not null);
        }

        public static bool TryCreateCOMObject<T>(Guid clsid, string serverName, out T obj) where T : class
        {
            if (!TryGetCOMType(clsid, serverName, out var type))
            {
                obj = null;
                return false;
            }

            return Try(out obj, () => Activator.CreateInstance(type) as T) && (obj is not null);
        }

        public static Type GetCOMType(string progID, string serverName)
        {
            VerifyNonBlankArgument(progID, nameof(progID), "Invalid programmatic identifier (ProgID)");

            if (!TryGetCOMType(progID, serverName, out var type))
            {
                throw new TypeLoadException(FormatInvariant("Could not find a registered class for '{0}'", progID));
            }

            return type;
        }

        public static Type GetCOMType(Guid clsid, string serverName)
        {
            if (!TryGetCOMType(clsid, serverName, out var type))
            {
                throw new TypeLoadException(FormatInvariant("Could not find a registered class for '{0}'", clsid.ToString("B")));
            }

            return type;
        }

        public static bool TryGetCOMType(string progID, string serverName, out Type type)
        {
            type = Guid.TryParseExact(progID, "B", out var clsid) ? Type.GetTypeFromCLSID(clsid, serverName) : Type.GetTypeFromProgID(progID, serverName);
            return type is not null;
        }

        public static bool TryGetCOMType(Guid clsid, string serverName, out Type type)
        {
            type = Type.GetTypeFromCLSID(clsid, serverName);
            return type is not null;
        }

        public static string GetDispIDName(int dispid)
        {
            return FormatInvariant("[DISPID={0}]", dispid);
        }

        public static bool IsDispIDName(this string name, out int dispid)
        {
            var match = dispIDNameRegex.Match(name);
            if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out dispid))
            {
                return true;
            }

            dispid = 0;
            return false;
        }

        #endregion

        #region argument helpers

        public static void VerifyNonNullArgument(object value, string name)
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void VerifyNonBlankArgument(string value, string name, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(message, name);
            }
        }

        #endregion

        #region string helpers

        private static readonly char[] searchPathSeparators = { ';' };

        public static string ToNonNull(this string input, string alternate = "[null]")
        {
            Debug.Assert(alternate is not null);
            return input ?? alternate;
        }

        public static string ToNonEmpty(this string input, string alternate)
        {
            Debug.Assert(!string.IsNullOrEmpty(alternate));
            return string.IsNullOrEmpty(input) ? alternate : input;
        }

        public static string ToNonBlank(this string input, string alternate)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(alternate));
            return string.IsNullOrWhiteSpace(input) ? alternate : input;
        }

        public static string FormatInvariant(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static StringBuilder AppendInvariant(this StringBuilder builder, string format, params object[] args)
        {
            return builder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }

        public static string FormatCode(string code)
        {
            var lines = (code ?? string.Empty).Replace("\r\n", "\n").Split('\n');

            lines = lines.SkipWhile(string.IsNullOrWhiteSpace).Reverse().SkipWhile(string.IsNullOrWhiteSpace).Reverse().ToArray();
            if (lines.Length > 0)
            {
                var firstLine = lines[0];
                for (var indentLength = firstLine.TakeWhile(char.IsWhiteSpace).Count(); indentLength > 0; indentLength--)
                {
                    var indent = firstLine.Substring(0, indentLength);
                    if (lines.Skip(1).All(line => string.IsNullOrWhiteSpace(line) || line.StartsWith(indent, StringComparison.Ordinal)))
                    {
                        lines = lines.Select(line => string.IsNullOrWhiteSpace(line) ? string.Empty : line.Substring(indent.Length)).ToArray();
                        break;
                    }
                }
            }

            return string.Join("\n", lines) + '\n';
        }

        public static string GetUrlOrPath(Uri uri, string alternate)
        {
            Debug.Assert(alternate is not null);

            if (uri is null)
            {
                return alternate;
            }

            if (!uri.IsAbsoluteUri)
            {
                return uri.ToString();
            }

            if (uri.IsFile)
            {
                return uri.LocalPath;
            }

            return uri.AbsoluteUri;
        }

        public static string ToQuotedJson(this string value)
        {
            var builder = new StringBuilder();
            builder.Append('\"');

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\"':
                        builder.Append("\\\"");
                        break;

                    case '\\':
                        builder.Append("\\\\");
                        break;

                    default:
                        builder.Append(ch);
                        break;
                }
            }

            builder.Append('\"');
            return builder.ToString();
        }

        public static UIntPtr GetDigest(this string code)
        {
            return (UIntPtr.Size == 4) ? (UIntPtr)code.GetDigestAsUInt32() : (UIntPtr)code.GetDigestAsUInt64();
        }

        public static uint GetDigestAsUInt32(this string code)
        {
            var digest = 2166136261U;
            const uint prime = 16777619U;

            unchecked
            {
                var bytes = Encoding.Unicode.GetBytes(code);
                for (var index = 0; index < bytes.Length; index++)
                {
                    digest ^= bytes[index];
                    digest *= prime;
                }
            }

            return digest;
        }

        public static ulong GetDigestAsUInt64(this string code)
        {
            var digest = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;

            var bytes = Encoding.Unicode.GetBytes(code);
            for (var index = 0; index < bytes.Length; index++)
            {
                digest ^= bytes[index];
                digest *= prime;
            }

            return digest;
        }

        public static IEnumerable<string> SplitSearchPath(this string searchPath)
        {
            return searchPath.Split(searchPathSeparators, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public static StringComparison GetMemberNameComparison(this BindingFlags bindFlags)
        {
            return bindFlags.HasAllFlags(BindingFlags.IgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }

        public static StringComparer GetMemberNameComparer(this BindingFlags bindFlags)
        {
            return bindFlags.HasAllFlags(BindingFlags.IgnoreCase) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        }

        #endregion

        #region enum helpers

        public static bool HasAllFlags(this BindingFlags value, BindingFlags flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this BindingFlags value, BindingFlags flags) => (value & flags) != 0;

        public static bool HasAllFlags(this ParameterAttributes value, ParameterAttributes flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this ParameterAttributes value, ParameterAttributes flags) => (value & flags) != 0;

        public static bool HasAllFlags(this TYPEFLAGS value, TYPEFLAGS flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this TYPEFLAGS value, TYPEFLAGS flags) => (value & flags) != 0;

        public static bool HasAllFlags(this INVOKEKIND value, INVOKEKIND flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this INVOKEKIND value, INVOKEKIND flags) => (value & flags) != 0;

        #endregion

        #region numeric index helpers

        public static bool TryGetNumericIndex(object arg, out int index)
        {
            if (arg is not null)
            {
                switch (Type.GetTypeCode(arg.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                        index = Convert.ToInt32(arg);
                        return true;
                }
            }

            index = -1;
            return false;
        }

        public static bool TryGetNumericIndex(object arg, out long index)
        {
            if (arg is not null)
            {
                switch (Type.GetTypeCode(arg.GetType()))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                        index = Convert.ToInt64(arg);
                        return true;
                }
            }

            index = -1;
            return false;
        }

        #endregion

        #region simplified exception handling

        public static bool Try(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Try<TArg>(Action<TArg> action, in TArg arg)
        {
            try
            {
                action(arg);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Try<TResult>(out TResult result, Func<TResult> func)
        {
            try
            {
                result = func();
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static bool Try<TArg, TResult>(out TResult result, Func<TArg, TResult> func, in TArg arg)
        {
            try
            {
                result = func(arg);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        public static async Task<bool> TryAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryAsync<T>(Holder<T> holder, Task<T> task)
        {
            try
            {
                holder.Value = await task.ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region primitive marshaling

        public const float MaxInt32InSingleAsSingle = 16777215.0F;
        public const double MaxInt32InSingleAsDouble = 16777215.0D;
        public const decimal MaxInt32InSingleAsDecimal = 16777215.0M;
        public const int MaxInt32InSingle = (1 << 24) - 1;

        public const double MaxInt64InDoubleAsDouble = 9007199254740991.0D;
        public const decimal MaxInt64InDoubleAsDecimal = 9007199254740991.0M;
        public const long MaxInt64InDouble = (1L << 53) - 1;

        public static readonly decimal MaxBigIntegerInDecimalAsDecimal = 79228162514264337593543950335.0M;
        public static readonly BigInteger MaxBigIntegerInDecimal = (new BigInteger(1) << 96) - 1;

        public static bool TryMarshalPrimitiveToHost(object obj, bool disableFloatNarrowing, out object result)
        {
            if (obj is IConvertible convertible)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.String:
                    case TypeCode.Boolean:
                        result = obj;
                        return true;

                    case TypeCode.Double:
                        result = MarshalDoubleToHost(convertible.ToDouble(CultureInfo.InvariantCulture), disableFloatNarrowing);
                        return true;

                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Char:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Decimal:
                        result = obj;
                        return true;
                }
            }

            result = null;
            return false;
        }

        public static object MarshalDoubleToHost(double value, bool disableFloatNarrowing)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            var truncatedValue = Math.Truncate(value);
            if (truncatedValue == value)
            {
                if (Math.Abs(value) <= MaxInt64InDoubleAsDouble)
                {
                    var longValue = (long)truncatedValue;
                    if ((longValue >= int.MinValue) && (longValue <= int.MaxValue))
                    {
                        return (int)longValue;
                    }

                    return longValue;
                }
            }
            else if (!disableFloatNarrowing)
            {
                var floatValue = (float)value;
                if (value == floatValue)
                {
                    return floatValue;
                }
            }

            return value;

            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        #endregion

        #region miscellaneous

        public static T Exchange<T>(ref T target, T value)
        {
            var oldValue = target;
            target = value;
            return oldValue;
        }

        public static void QueueNativeCallback(INativeCallback callback)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                using (callback)
                {
                    Try(callback.Invoke);
                }

                // The above code appears to be problematic on some .NET runtimes, intermittently
                // triggering premature finalization of the callback. That can lead to a crash if
                // the callback's finalizer ends up racing against its Dispose method. The call
                // below should prevent this condition in all cases.
                //
                // UPDATE: The observed behavior is actually documented. As Dispose is invoked via
                // the callback's only reference, the callback may become eligible for finalization
                // during the call. Typically, Dispose invokes GC.SuppressFinalize just before
                // exiting, which, in addition to canceling finalization, extends the object's
                // lifetime until Dispose has done its job. The callback here is unusual in that it
                // requires finalization regardless of disposal, so the correct fix is for Dispose
                // to invoke GC.KeepAlive as its final step. The original fix is retained here for
                // regression avoidance.

                GC.KeepAlive(callback);
            });
        }

        public static Random CreateSeededRandom()
        {
            return new Random(Convert.ToUInt32(DateTime.Now.Ticks.ToUnsigned() & 0x00000000FFFFFFFFUL).ToSigned());
        }

        public static async Task<IDisposable> CreateLockScopeAsync(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return Scope.Create(null, () => semaphore.Release());
        }

        public static byte[] ReadToEnd(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string GetTextContents(this Document document)
        {
            if (document is StringDocument stringDocument)
            {
                return stringDocument.StringContents;
            }

            using (var reader = new StreamReader(document.Contents, document.Encoding ?? Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static void AssertUnreachable()
        {
            Debug.Assert(false, "Entered code block presumed unreachable.");
        }

        public static string GetLocalDataRootPath(out bool usingAppPath)
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory;
                usingAppPath = true;
            }
            else
            {
                usingAppPath = false;
            }

            return GetLocalDataRootPath(basePath);
        }

        public static string GetLocalDataRootPath(string basePath)
        {
            var path = Path.Combine(basePath, "Microsoft", "ClearScript", ClearScriptVersion.Triad, Environment.Is64BitProcess ? "x64" : "x86");

            if (Try(out var fullPath, () => Path.GetFullPath(path)))
            {
                return fullPath;
            }

            return path;
        }

        public static bool PlatformIsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool PlatformIsLinux()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }

        public static bool PlatformIsOSX()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        }

        public static bool ProcessorArchitectureIsIntel()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X64:
                case Architecture.X86:
                    return true;

                default:
                    return false;
            }
        }

        public static bool ProcessorArchitectureIsArm()
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm:
                case Architecture.Arm64:
                    return true;

                default:
                    return false;
            }
        }

        public static object GetObjectForVariant(IntPtr pVariant)
        {
            var result = Marshal.GetObjectForNativeVariant(pVariant);

            if ((result is null) && (Marshal.ReadInt16(pVariant) == (short)VarEnum.VT_BSTR))
            {
                return string.Empty;
            }

            return result;
        }

        #endregion
    }
}
