// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;

namespace Microsoft.ClearScript.Util
{
    internal static class BitwiseHelpers
    {
        #region bitwise operations

        public static byte And(this byte value, byte mask)
        {
            var tempValue = value;
            tempValue &= mask;
            return tempValue;
        }

        public static byte Or(this byte value, byte mask)
        {
            var tempValue = value;
            tempValue |= mask;
            return tempValue;
        }

        public static byte Xor(this byte value, byte mask)
        {
            var tempValue = value;
            tempValue ^= mask;
            return tempValue;
        }

        public static bool Has(this byte value, byte mask)
        {
            return value.And(mask) != 0;
        }

        public static bool HasAll(this byte value, byte mask)
        {
            return value.And(mask) == mask;
        }

        #endregion

        #region signed/unsigned conversion

        public static short ToSigned(this ushort value)
        {
            return BitConverter.ToInt16(BitConverter.GetBytes(value), 0);
        }

        public static int ToSigned(this uint value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static long ToSigned(this ulong value)
        {
            return BitConverter.ToInt64(BitConverter.GetBytes(value), 0);
        }

        public static ushort ToUnsigned(this short value)
        {
            return BitConverter.ToUInt16(BitConverter.GetBytes(value), 0);
        }

        public static uint ToUnsigned(this int value)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        }

        public static ulong ToUnsigned(this long value)
        {
            return BitConverter.ToUInt64(BitConverter.GetBytes(value), 0);
        }

        #endregion

        #region network byte order support

        public static ushort ToHostUInt16(this byte[] bytes, int index = 0)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, index)).ToUnsigned();
        }

        public static uint ToHostUInt32(this byte[] bytes, int index = 0)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, index)).ToUnsigned();
        }

        public static ulong ToHostUInt64(this byte[] bytes, int index = 0)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, index)).ToUnsigned();
        }

        public static byte[] ToNetworkBytes(this ushort value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value.ToSigned()));
        }

        public static byte[] ToNetworkBytes(this uint value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value.ToSigned()));
        }

        public static byte[] ToNetworkBytes(this ulong value)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value.ToSigned()));
        }

        #endregion
    }
}
