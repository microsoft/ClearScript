// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

using System;
using System.Globalization;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal class ScriptMethod : HostTarget, IReflect
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
