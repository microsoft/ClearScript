// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;

namespace Microsoft.ClearScript.Test
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public sealed class DynamicTestObject : DynamicObject
    {
        private readonly Dictionary<string, object> memberMap = new Dictionary<string, object>();
        private readonly Dictionary<string, object> indexMap = new Dictionary<string, object>();

        public int SomeField = 12345;

        public string SomeProperty
        {
            get { return "Bogus"; }
        }

        public double SomeMethod()
        {
            return Math.PI;
        }

        public string SomeMethod(string unused, params object[] args)
        {
            return string.Join("+", args);
        }

        public bool DisableInvocation { get; set; }

        public bool DisableDynamicMembers { get; set; }

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            if (args.Length > 0)
            {
                result = string.Join(" ", args);
                return true;
            }

            return base.TryCreateInstance(binder, args, out result);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (!DisableInvocation && (args.Length > 0))
            {
                result = string.Join(",", args);
                return true;
            }

            return base.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if ((binder.Name == "DynamicMethod") && (args.Length > 0))
            {
                result = string.Join("-", args);
                return true;
            }

            if ((binder.Name == "SomeField") && (args.Length > 0))
            {
                result = string.Join(".", args);
                return true;
            }

            if ((binder.Name == "SomeProperty") && (args.Length > 0))
            {
                result = string.Join(":", args);
                return true;
            }

            if ((binder.Name == "SomeMethod") && (args.Length > 0))
            {
                result = string.Join(";", args);
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!DisableDynamicMembers && !binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                return memberMap.TryGetValue(binder.Name, out result);
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!DisableDynamicMembers && !binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                memberMap[binder.Name] = value;
                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (!DisableDynamicMembers && !binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                return memberMap.Remove(binder.Name);
            }

            return base.TryDeleteMember(binder);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.All(index => (index is int) || ((index is string) && !((string)index).StartsWith("Z", StringComparison.Ordinal))))
            {
                return indexMap.TryGetValue(string.Join(":", indexes), out result);
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.All(index => (index is int) || ((index is string) && !((string)index).StartsWith("Z", StringComparison.Ordinal))))
            {
                indexMap[string.Join(":", indexes)] = value;
                return true;
            }

            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            if (indexes.All(index => (index is int) || ((index is string) && !((string)index).StartsWith("Z", StringComparison.Ordinal))))
            {
                return indexMap.Remove(string.Join(":", indexes));
            }

            return base.TryDeleteIndex(binder, indexes);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == typeof(int))
            {
                result = 98765;
                return true;
            }

            if (binder.ReturnType == typeof(string))
            {
                result = "Booyakasha!";
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            foreach (var name in memberMap.Keys)
            {
                yield return name;
            }

            yield return "DynamicMethod";
            yield return "SomeField";
            yield return "SomeProperty";
            yield return "SomeMethod";
        }

        public override string ToString()
        {
            return "Super Bass-O-Matic '76";
        }
    }
}
