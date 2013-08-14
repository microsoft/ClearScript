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
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.ClearScript.Util
{
    internal static class MiscHelpers
    {
        public static object CreateCOMObject(string progID)
        {
            return Activator.CreateInstance(Type.GetTypeFromProgID(progID));
        }

        public static void VerifyNonNullArgument(object value, string name)
        {
            if (value == null)
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

        public static string EnsureNonBlank(string input, string alternate)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(alternate));
            return string.IsNullOrWhiteSpace(input) ? alternate : input;
        }

        public static string FormatInvariant(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
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

        public static bool TryGetIndex(object arg, out int index)
        {
            if (arg != null)
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

        public static bool TryGetIndex(object arg, out long index)
        {
            if (arg != null)
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

        public static int UnsignedAsSigned(uint value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static uint SignedAsUnsigned(int value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }

        public static bool TryMarshalPrimitiveToHost(object obj, out object result)
        {
            var convertible = obj as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.String:
                    case TypeCode.Boolean:
                        result = obj;
                        return true;

                    case TypeCode.Double:
                    case TypeCode.Single:
                        result = MarshalDoubleToHost(convertible.ToDouble(CultureInfo.InvariantCulture));
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
                    case TypeCode.Decimal:
                        result = obj;
                        return true;
                }
            }

            result = null;
            return false;
        }

        public static object MarshalDoubleToHost(double value)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator

            if (Math.Round(value) == value)
            {
                const double maxIntInDouble = (1L << 53) - 1;
                if (Math.Abs(value) <= maxIntInDouble)
                {
                    var longValue = Convert.ToInt64(value);
                    if ((longValue >= int.MinValue) && (longValue <= int.MaxValue))
                    {
                        return (int)longValue;
                    }

                    return longValue;
                }
            }
            else
            {
                var floatArg = Convert.ToSingle(value);
                if (value == floatArg)
                {
                    return floatArg;
                }
            }

            return value;

            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static T[] GetEmptyArray<T>()
        {
            return EmptyArray<T>.Value;
        }

        #region Nested type: EmptyArray

        private static class EmptyArray<T>
        {
            private static readonly T[] value = new T[0];

            public static T[] Value
            {
                get { return value; }
            }
        }

        #endregion
    }
}
