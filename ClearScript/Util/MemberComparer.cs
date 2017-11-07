// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal sealed class MemberComparer<T> : EqualityComparer<T> where T : MemberInfo
    {
        private static readonly MemberComparer<T> instance = new MemberComparer<T>();

        public static MemberComparer<T> Instance { get { return instance; } }

        public override bool Equals(T x, T y)
        {
            // ReSharper disable PossibleNullReferenceException

            try
            {
                return (x.Module == y.Module) && (x.MetadataToken == y.MetadataToken);
            }
            catch (Exception)
            {
                return x == y;
            }

            // ReSharper restore PossibleNullReferenceException
        }

        public override int GetHashCode(T obj)
        {
            return (obj == null) ? 0 : obj.GetHashCode();
        }
    }
}
