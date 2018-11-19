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

        public abstract HostTargetFlags Flags { get; }

        public virtual string[] GetAuxMethodNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<string>();
        }

        public virtual string[] GetAuxPropertyNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<string>();
        }

        public virtual bool TryInvokeAuxMember(IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvoke(IHostInvokeContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = null;
            return false;
        }

        public abstract Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic);
    }
}
