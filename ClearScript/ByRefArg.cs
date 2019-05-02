// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal interface IByRefArg
    {
        Type Type { get; }
        object Value { get; set; }
    }

    internal interface IOutArg : IByRefArg
    {
    }

    internal interface IRefArg : IByRefArg
    {
    }

    internal abstract class ByRefArg<T> : HostTarget, IByRefArg
    {
        private readonly HostVariable<T> target;

        protected ByRefArg(HostVariable<T> target)
        {
            this.target = target;
        }

        public T Value
        {
            get { return target.Value; }
            set { target.Value = value; }
        }

        #region HostTarget overrides

        public override Type Type
        {
            get { return target.Type; }
        }

        public override object Target
        {
            get { return this; }
        }

        public override object InvokeTarget
        {
            get { return target.InvokeTarget; }
        }

        public override object DynamicInvokeTarget
        {
            get { return target.DynamicInvokeTarget; }
        }

        public override HostTargetFlags Flags
        {
            get { return target.Flags; }
        }

        public override string[] GetAuxMethodNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return target.GetAuxMethodNames(context, bindFlags);
        }

        public override string[] GetAuxPropertyNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return target.GetAuxPropertyNames(context, bindFlags);
        }

        public override bool TryInvokeAuxMember(IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvokeAuxMember(context, name, invokeFlags, args, bindArgs, out result);
        }

        public override bool TryInvoke(IHostInvokeContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvoke(context, invokeFlags, args, bindArgs, out result);
        }

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return target.GetInvocability(bindFlags, accessContext, defaultAccess, ignoreDynamic);
        }

        #endregion

        #region IByRefArg implementation

        object IByRefArg.Value
        {
            get { return target.Value; }
            set { ((IHostVariable)target).Value = value; }
        }

        #endregion
    }

    internal sealed class OutArg<T> : ByRefArg<T>, IOutArg
    {
        public OutArg(HostVariable<T> target)
            : base(target)
        {
        }

        public OutArg(T initValue)
            : this(new HostVariable<T>(initValue))
        {
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("out {0}", Type.GetFriendlyName());
        }

        #endregion
    }

    internal sealed class RefArg<T> : ByRefArg<T>, IRefArg
    {
        public RefArg(HostVariable<T> target)
            : base(target)
        {
        }

        public RefArg(T initValue)
            : this(new HostVariable<T>(initValue))
        {
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("ref {0}", Type.GetFriendlyName());
        }

        #endregion
    }
}
