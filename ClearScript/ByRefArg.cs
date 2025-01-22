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
            get => target.Value;
            set => target.Value = value;
        }

        #region HostTarget overrides

        public override Type Type => target.Type;

        public override object Target => this;

        public override object InvokeTarget => target.InvokeTarget;

        public override object DynamicInvokeTarget => target.DynamicInvokeTarget;

        public override HostTargetFlags GetFlags(IHostContext context)
        {
            return target.GetFlags(context);
        }

        public override string[] GetAuxMethodNames(IHostContext context, BindingFlags bindFlags)
        {
            return target.GetAuxMethodNames(context, bindFlags);
        }

        public override string[] GetAuxPropertyNames(IHostContext context, BindingFlags bindFlags)
        {
            return target.GetAuxPropertyNames(context, bindFlags);
        }

        public override bool TryInvokeAuxMember(IHostContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvokeAuxMember(context, name, invokeFlags, args, bindArgs, out result);
        }

        public override bool TryInvoke(IHostContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvoke(context, invokeFlags, args, bindArgs, out result);
        }

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return target.GetInvocability(context, bindFlags, ignoreDynamic);
        }

        #endregion

        #region IByRefArg implementation

        object IByRefArg.Value
        {
            get => target.Value;
            set => ((IHostVariable)target).Value = value;
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
