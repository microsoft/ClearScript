// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostMethod : HostTarget
    {
        private readonly HostItem target;
        private readonly string name;

        public HostMethod(HostItem target, string name)
        {
            this.target = target;
            this.name = name;
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("HostMethod:{0}", name);
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

        public override bool TryInvoke(IHostContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = target.InvokeMember(name, invokeFlags, args, bindArgs, null, true);
            return true;
        }

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return Invocability.Delegate;
        }

        #endregion
    }
}
