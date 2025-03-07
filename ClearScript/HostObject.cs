// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostObject : HostTarget
    {
        #region data

        private readonly object target;
        private readonly Type type;
        private static readonly MethodInfo getNullWrapperGenericMethod = typeof(HostObject).GetMethod("GetNullWrapperGeneric", BindingFlags.NonPublic | BindingFlags.Static);

        #endregion

        #region constructors

        private HostObject(object target, Type type, bool isCanonicalRef)
        {
            if (!isCanonicalRef)
            {
                target = CanonicalRefTable.GetCanonicalRef(target);
            }

            if (type is null)
            {
                type = target.GetType();
            }

            if (type.IsUnknownCOMObject())
            {
                if (target is IEnumVARIANT enumVariant)
                {
                    target = new ScriptableEnumeratorOnEnumVariant(enumVariant);
                    type = typeof(IScriptableEnumerator);
                }
            }

            this.target = target;
            this.type = type;
        }

        #endregion

        #region wrappers

        public static HostObject Wrap(object target)
        {
            return Wrap(target, null);
        }

        public static HostObject Wrap(object target, Type type)
        {
            return Wrap(target, type, false);
        }

        public static HostObject Wrap(object target, Type type, bool isCanonicalRef)
        {
            return (target is not null) ? new HostObject(target, type, isCanonicalRef) : null;
        }

        public static object WrapResult(object result, Type type, bool wrapNull)
        {
            if ((result is IScriptMarshalWrapper) || (result is HostTarget))
            {
                return result;
            }

            if (result is null)
            {
                return wrapNull ? GetNullWrapper(type) : null;
            }

            if ((type == typeof(void)) || (type == typeof(object)) || type.IsNullable())
            {
                return result;
            }

            if ((type == result.GetType()) || (Type.GetTypeCode(type) != TypeCode.Object))
            {
                return result;
            }

            return Wrap(result, type);
        }

        #endregion

        #region internal members

        private static HostObject GetNullWrapper(Type type)
        {
            return (HostObject)getNullWrapperGenericMethod.MakeGenericMethod(type).Invoke(null, ArrayHelpers.GetEmptyArray<object>());
        }

        // ReSharper disable UnusedMember.Local

        private static HostObject GetNullWrapperGeneric<T>()
        {
            return NullWrapper<T>.Value;
        }

        // ReSharper restore UnusedMember.Local

        #endregion

        #region Object overrides

        public override string ToString()
        {
            if ((target is ScriptItem) && (typeof(ScriptItem).IsAssignableFrom(type)))
            {
                return "ScriptItem";
            }

            var objectName = target.GetFriendlyName(type);
            return MiscHelpers.FormatInvariant("HostObject:{0}", objectName);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type => type;

        public override object Target => target;

        public override object InvokeTarget => target;

        public override object DynamicInvokeTarget => target;

        public override HostTargetFlags GetFlags(IHostContext context)
        {
            var flags = HostTargetFlags.AllowInstanceMembers | HostTargetFlags.AllowExtensionMethods;
            if (context.Engine.ExposeHostObjectStaticMembers)
            {
                flags |= HostTargetFlags.AllowStaticMembers;
            }

            return flags;
        }

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return type.GetInvocability(context, bindFlags, ignoreDynamic);
        }

        #endregion

        #region Nested type: NullWrapper<T>

        // ReSharper disable UnusedMember.Local

        private static class NullWrapper<T>
        {
            public static readonly HostObject Value = new(null, typeof(T), true);
        }

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
