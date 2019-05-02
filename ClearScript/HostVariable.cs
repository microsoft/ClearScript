// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Dynamic;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal interface IHostVariable
    {
        Type Type { get; }
        object Value { get; set; }
    }

    internal abstract class HostVariableBase : HostTarget
    {
        private static readonly string[] auxPropertyNames = { "out", "ref", "value" };

        public override string[] GetAuxPropertyNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            return auxPropertyNames;
        }
    }

    internal sealed class HostVariable<T> : HostVariableBase, IHostVariable
    {
        private T value;

        public HostVariable(T initValue)
        {
            if ((typeof(T) == typeof(Undefined)) || (typeof(T) == typeof(VoidResult)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if (typeof(HostItem).IsAssignableFrom(typeof(T)) || typeof(HostTarget).IsAssignableFrom(typeof(T)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if ((initValue is HostItem) || (initValue is HostTarget))
            {
                throw new NotSupportedException("Unsupported value type");
            }

            value = initValue;
        }

        public T Value
        {
            // Be careful when renaming or deleting this property; it is accessed by name in the
            // expression tree construction code in DelegateFactory.CreateComplexDelegate().

            get { return value; }
            set { this.value = value; }
        }

        #region Object overrides

        public override string ToString()
        {
            var objectName = value.GetFriendlyName(typeof(T));
            return MiscHelpers.FormatInvariant("HostVariable:{0}", objectName);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override object Target
        {
            get { return value; }
        }

        public override object InvokeTarget
        {
            get { return value; }
        }

        public override object DynamicInvokeTarget
        {
            get { return value; }
        }

        public override HostTargetFlags Flags
        {
            get { return HostTargetFlags.AllowInstanceMembers | HostTargetFlags.AllowExtensionMethods; }
        }

        public override bool TryInvokeAuxMember(IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            const BindingFlags getPropertyFlags =
                BindingFlags.GetField |
                BindingFlags.GetProperty;

            const BindingFlags setPropertyFlags =
                BindingFlags.SetProperty |
                BindingFlags.PutDispProperty |
                BindingFlags.PutRefDispProperty;

            if (name == "out")
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new OutArg<T>(this);
                    return true;
                }
            }
            else if (name == "ref")
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new RefArg<T>(this);
                    return true;
                }
            }
            else if (name == "value")
            {
                if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
                {
                    if (InvokeHelpers.TryInvokeObject(context, value, invokeFlags, args, bindArgs, typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)), out result))
                    {
                        return true;
                    }

                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        result = context.Engine.PrepareResult(value, ScriptMemberFlags.None, false);
                        return true;
                    }

                    result = null;
                    return false;
                }

                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = context.Engine.PrepareResult(value, ScriptMemberFlags.None, false);
                    return true;
                }

                if ((invokeFlags & setPropertyFlags) != 0)
                {
                    if (args.Length == 1)
                    {
                        result = context.Engine.PrepareResult(((IHostVariable)this).Value = args[0], typeof(T), ScriptMemberFlags.None, false);
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return typeof(T).GetInvocability(bindFlags, accessContext, defaultAccess, ignoreDynamic);
        }

        #endregion

        #region IHostVariable implementation

        object IHostVariable.Value
        {
            get { return value; }

            set
            {
                if (!typeof(T).IsAssignableFrom(ref value))
                {
                    throw new InvalidOperationException("Assignment invalid due to type mismatch");
                }

                if ((value is HostItem) || (value is HostTarget))
                {
                    throw new NotSupportedException("Unsupported value type");
                }

                this.value = (T)value;
            }
        }

        #endregion
    }
}
