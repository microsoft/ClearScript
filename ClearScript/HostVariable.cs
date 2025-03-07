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

    internal abstract class HostVariable : HostTarget
    {
        private static readonly string[] auxPropertyNames = { "out", "ref", "value" };

        public override string[] GetAuxPropertyNames(IHostContext context, BindingFlags bindFlags)
        {
            return auxPropertyNames;
        }
    }

    internal sealed class HostVariable<T> : HostVariable, IHostVariable
    {
        public HostVariable(T initValue)
        {
            if ((typeof(T) == typeof(Undefined)) || (typeof(T) == typeof(VoidResult)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if (typeof(IHostItem).IsAssignableFrom(typeof(T)) || typeof(HostTarget).IsAssignableFrom(typeof(T)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if ((initValue is IHostItem) || (initValue is HostTarget))
            {
                throw new NotSupportedException("Unsupported value type");
            }

            Value = initValue;
        }

        public T Value { get; set; }
            // Be careful when renaming or deleting this property; it is accessed by name in the
            // expression tree construction code in DelegateFactory.CreateComplexDelegate().

        #region Object overrides

        public override string ToString()
        {
            var objectName = Value.GetFriendlyName(typeof(T));
            return MiscHelpers.FormatInvariant("HostVariable:{0}", objectName);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type => typeof(T);

        public override object Target => Value;

        public override object InvokeTarget => Value;

        public override object DynamicInvokeTarget => Value;

        public override HostTargetFlags GetFlags(IHostContext context)
        {
            var flags = HostTargetFlags.AllowInstanceMembers | HostTargetFlags.AllowExtensionMethods;
            if (context.Engine.ExposeHostObjectStaticMembers)
            {
                flags |= HostTargetFlags.AllowStaticMembers;
            }

            return flags;
        }

        public override bool TryInvokeAuxMember(IHostContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            const BindingFlags getPropertyFlags =
                BindingFlags.GetField |
                BindingFlags.GetProperty;

            const BindingFlags setPropertyFlags =
                BindingFlags.SetProperty |
                BindingFlags.PutDispProperty |
                BindingFlags.PutRefDispProperty;

            if (string.Equals(name, "out", invokeFlags.GetMemberNameComparison()))
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new OutArg<T>(this);
                    return true;
                }
            }
            else if (string.Equals(name, "ref", invokeFlags.GetMemberNameComparison()))
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new RefArg<T>(this);
                    return true;
                }
            }
            else if (string.Equals(name, "value", invokeFlags.GetMemberNameComparison()))
            {
                if (invokeFlags.HasAllFlags(BindingFlags.InvokeMethod))
                {
                    if (InvokeHelpers.TryInvokeObject(context, Value, invokeFlags, args, bindArgs, typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)), out result))
                    {
                        return true;
                    }

                    if (invokeFlags.HasAllFlags(BindingFlags.GetField) && (args.Length < 1))
                    {
                        result = context.Engine.PrepareResult(Value, ScriptMemberFlags.None, false);
                        return true;
                    }

                    result = null;
                    return false;
                }

                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = context.Engine.PrepareResult(Value, ScriptMemberFlags.None, false);
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

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return typeof(T).GetInvocability(context, bindFlags, ignoreDynamic);
        }

        #endregion

        #region IHostVariable implementation

        object IHostVariable.Value
        {
            get => Value;

            set
            {
                if (!typeof(T).IsAssignableFromValue(ref value))
                {
                    throw new InvalidOperationException("Assignment invalid due to type mismatch");
                }

                if ((value is IHostItem) || (value is HostTarget))
                {
                    throw new NotSupportedException("Unsupported value type");
                }

                Value = (T)value;
            }
        }

        #endregion
    }
}
