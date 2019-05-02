// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;
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

        private HostObject(object target, Type type)
        {
            this.target = CanonicalRefTable.GetCanonicalRef(target);
            this.type = type ?? target.GetType();
        }

        #endregion

        #region wrappers

        public static HostObject Wrap(object target)
        {
            return Wrap(target, null);
        }

        public static HostObject Wrap(object target, Type type)
        {
            return (target != null) ? new HostObject(target, type) : null;
        }

        public static object WrapResult(object result, Type type, bool wrapNull)
        {
            if ((result is HostItem) || (result is HostTarget))
            {
                return result;
            }

            if (result == null)
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

        public override Type Type
        {
            get { return type; }
        }

        public override object Target
        {
            get { return target; }
        }

        public override object InvokeTarget
        {
            get { return target; }
        }

        public override object DynamicInvokeTarget
        {
            get { return target; }
        }

        public override HostTargetFlags Flags
        {
            get { return HostTargetFlags.AllowInstanceMembers | HostTargetFlags.AllowExtensionMethods; }
        }

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return type.GetInvocability(bindFlags, accessContext, defaultAccess, ignoreDynamic);
        }

        #endregion

        #region Nested type: NullWrapper<T>

        // ReSharper disable UnusedMember.Local

        private static class NullWrapper<T>
        {
            private static readonly HostObject value = new HostObject(null, typeof(T));

            public static HostObject Value
            {
                get { return value; }
            }
        }

        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
