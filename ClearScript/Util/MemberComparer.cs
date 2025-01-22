// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal sealed class MemberComparer<T> : EqualityComparer<T> where T : MemberInfo
    {
        public static readonly MemberComparer<T> Instance = new MemberComparer<T>();

        private MemberComparer()
        {
        }

        public override bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return MiscHelpers.Try(out var result, () => UnsafeEquals(x, y)) && result;
        }

        public override int GetHashCode(T obj)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return ReferenceEquals(obj, null) ? 0 : obj.GetHashCode();
        }

        private static bool UnsafeEquals(T x, T y)
        {
            return (x.Module == y.Module) && (x.MetadataToken == y.MetadataToken);
        }
    }
}
