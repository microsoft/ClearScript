// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.Util
{
    internal static class DateTimeHelpers
    {
        private static readonly DateTime unixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static double ToUnixMilliseconds(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime() - unixEpoch).TotalMilliseconds;
        }

        public static DateTime FromUnixMilliseconds(double value)
        {
            return unixEpoch + TimeSpan.FromMilliseconds(value);
        }

    }
}
