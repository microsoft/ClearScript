// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostIndexedProperty : HostTarget
    {
        private static readonly string[] auxMethodNames = { "get", "set" };

        private readonly HostItem target;
        private readonly string name;

        public HostIndexedProperty(HostItem target, string name)
        {
            this.target = target;
            this.name = name;
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("HostIndexedProperty:{0}", name);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type => typeof(void);

        public override object Target => this;

        public override object InvokeTarget => null;

        public override object DynamicInvokeTarget => null;

        public override HostTargetFlags GetFlags(IHostContext context)
        {
            return HostTargetFlags.None;
        }

        public override string[] GetAuxMethodNames(IHostContext context, BindingFlags bindFlags)
        {
            return auxMethodNames;
        }

        public override bool TryInvokeAuxMember(IHostContext context, string memberName, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            if (invokeFlags.HasAllFlags(BindingFlags.InvokeMethod))
            {
                if (string.Equals(memberName, "get", invokeFlags.GetMemberNameComparison()))
                {
                    result = target.InvokeMember(name, BindingFlags.GetProperty | BindingFlags.SuppressChangeType, args, bindArgs, null, true);
                    return true;
                }

                if (string.Equals(memberName, "set", invokeFlags.GetMemberNameComparison()))
                {
                    result = target.InvokeMember(name, BindingFlags.SetProperty | BindingFlags.SuppressChangeType, args, bindArgs, null, true);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvoke(IHostContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = target.InvokeMember(name, (invokeFlags.HasAllFlags(BindingFlags.SetField) ? BindingFlags.SetProperty : BindingFlags.GetProperty) | BindingFlags.SuppressChangeType, args, bindArgs, null, true);
            return true;
        }

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return Invocability.Delegate;
        }

        #endregion
    }
}
