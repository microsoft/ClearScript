// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class ScriptMethod : HostTarget, IReflect
    {
        private readonly ScriptItem target;
        private readonly string name;

        public ScriptMethod(ScriptItem target, string name)
        {
            this.target = target;
            this.name = name;
        }

        public object Invoke(params object[] args)
        {
            return target.InvokeMethod(name, args);
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("ScriptMethod:{0}", name);
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

        public override bool TryInvoke(IHostInvokeContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            result = target.InvokeMethod(name, args);
            return true;
        }

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return Invocability.Delegate;
        }

        #endregion

        #region IReflect implementation

        MethodInfo IReflect.GetMethod(string methodName, BindingFlags bindFlags, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        MethodInfo IReflect.GetMethod(string methodName, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        FieldInfo IReflect.GetField(string fieldName, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        PropertyInfo IReflect.GetProperty(string propertyName, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        PropertyInfo IReflect.GetProperty(string propertyName, BindingFlags bindFlags, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        MemberInfo[] IReflect.GetMember(string memberName, BindingFlags bindFlags)
        {
            // This occurs during VB-based dynamic script item invocation. It was not observed
            // before script items gained an IReflect/IExpando implementation that exposes
            // script item properties as fields. Apparently VB's dynamic invocation support not
            // only recognizes IReflect/IExpando but actually favors it over DynamicObject.

            return typeof(ScriptMethod).GetMember(MiscHelpers.EnsureNonBlank(memberName, "Invoke"), bindFlags);
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        object IReflect.InvokeMember(string memberName, BindingFlags invokeFlags, Binder binder, object invokeTarget, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        Type IReflect.UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
