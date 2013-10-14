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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Microsoft.ClearScript.Test
{
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
            if (args.Length > 0)
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
            if (!binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                return memberMap.TryGetValue(binder.Name, out result);
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                memberMap[binder.Name] = value;
                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (!binder.Name.StartsWith("Z", StringComparison.Ordinal))
            {
                return memberMap.Remove(binder.Name);
            }

            return base.TryDeleteMember(binder);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.All(index => (index is int) || (index is string)))
            {
                return indexMap.TryGetValue(string.Join(":", indexes), out result);
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.All(index => (index is int) || (index is string)))
            {
                indexMap[string.Join(":", indexes)] = value;
                return true;
            }

            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            if (indexes.All(index => (index is int) || (index is string)))
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
