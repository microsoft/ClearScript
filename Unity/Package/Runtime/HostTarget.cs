// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal abstract class HostTarget
    {
        public abstract Type Type { get; }

        public abstract object Target { get; }

        public abstract object InvokeTarget { get; }

        public abstract object DynamicInvokeTarget { get; }

        public abstract HostTargetFlags GetFlags(IHostContext context);

        public virtual string[] GetAuxMethodNames(IHostContext context, BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<string>();
        }

        public virtual string[] GetAuxPropertyNames(IHostContext context, BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<string>();
        }

        public virtual bool TryInvokeAuxMember(IHostContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvoke(IHostContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = null;
            return false;
        }

        public abstract Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic);
    }
}
