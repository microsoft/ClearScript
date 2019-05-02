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

        public override Type Type
        {
            get { return typeof(void); }
        }

        public override object Target
        {
            get { return this; }
        }

        public override object InvokeTarget
        {
            get { return null; }
        }

        public override object DynamicInvokeTarget
        {
            get { return null; }
        }

        public override HostTargetFlags Flags
        {
            get { return HostTargetFlags.None; }
        }

        public override string[] GetAuxMethodNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return auxMethodNames;
        }

        public override bool TryInvokeAuxMember(IHostInvokeContext context, string memberName, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (memberName == "get")
                {
                    result = target.InvokeMember(name, BindingFlags.GetProperty, args, bindArgs, null, true);
                    return true;
                }

                if (memberName == "set")
                {
                    result = target.InvokeMember(name, BindingFlags.SetProperty, args, bindArgs, null, true);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvoke(IHostInvokeContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = target.InvokeMember(name, invokeFlags.HasFlag(BindingFlags.SetField) ? BindingFlags.SetProperty : BindingFlags.GetProperty, args, bindArgs, null, true);
            return true;
        }

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return Invocability.Delegate;
        }

        #endregion
    }
}
