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
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal class HostObject : HostTarget
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
            return (HostObject)getNullWrapperGenericMethod.MakeGenericMethod(type).Invoke(null, MiscHelpers.GetEmptyArray<object>());
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
