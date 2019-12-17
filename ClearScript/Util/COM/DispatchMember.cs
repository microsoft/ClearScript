// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.ClearScript.Util.COM
{
    internal sealed class DispatchMember
    {
        public string Name { get; private set; }

        public int DispID { get; private set; }

        public DispatchFlags DispatchFlags { get; private set; }

        private DispatchMember(string name, int dispid)
        {
            Name = name;
            DispID = dispid;
        }

        public DispatchMember(string name, int dispid, INVOKEKIND invokeKind)
            : this(name, dispid)
        {
            if (invokeKind.HasFlag(INVOKEKIND.INVOKE_FUNC))
            {
                DispatchFlags |= DispatchFlags.Method;
            }

            if (invokeKind.HasFlag(INVOKEKIND.INVOKE_PROPERTYGET))
            {
                DispatchFlags |= DispatchFlags.PropertyGet;
            }

            if (invokeKind.HasFlag(INVOKEKIND.INVOKE_PROPERTYPUT))
            {
                DispatchFlags |= DispatchFlags.PropertyPut;
            }

            if (invokeKind.HasFlag(INVOKEKIND.INVOKE_PROPERTYPUTREF))
            {
                DispatchFlags |= DispatchFlags.PropertyPutRef;
            }
        }

        public DispatchMember(string name, int dispid, DispatchPropFlags flags)
            : this(name, dispid)
        {
            if (flags.HasFlag(DispatchPropFlags.CanCall))
            {
                DispatchFlags |= DispatchFlags.Method;
            }

            if (flags.HasFlag(DispatchPropFlags.CanGet))
            {
                DispatchFlags |= DispatchFlags.PropertyGet;
            }

            if (flags.HasFlag(DispatchPropFlags.CanPut))
            {
                DispatchFlags |= DispatchFlags.PropertyPut;
            }

            if (flags.HasFlag(DispatchPropFlags.CanPutRef))
            {
                DispatchFlags |= DispatchFlags.PropertyPutRef;
            }
        }

        public static DispatchMember Merge(int dispid, IEnumerable<DispatchMember> group)
        {
            var members = group.ToArray();
            if (members.Length < 1)
            {
                return null;
            }

            var result = new DispatchMember(members[0].Name, dispid);
            foreach (var member in members)
            {
                Debug.Assert(member.Name == result.Name);
                Debug.Assert(member.DispID == result.DispID);
                result.DispatchFlags |= member.DispatchFlags;
            }

            return result;
        }
    }
}
