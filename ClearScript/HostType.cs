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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal class HostType : HostTarget, IScriptableObject
    {
        private readonly Type[] types;

        private HostType(Type type)
            : this(new[] { type })
        {
        }

        private HostType(Type[] types)
        {
            Debug.Assert((types != null) && (types.Length > 0));

            var nestingGroups = types.GroupBy(type => type.IsNested).ToIList();
            if (nestingGroups.Count != 1)
            {
                throw new NotSupportedException("Cannot create type wrapper for multiple unrelated types");
            }

            var nested = nestingGroups[0].Key;
            if (nested)
            {
                if (types.GroupBy(type => type.DeclaringType).Count() > 1)
                {
                    throw new NotSupportedException("Cannot create type wrapper for multiple unrelated types");
                }
            }
            else
            {
                if (types.GroupBy(type => type.GetLocator()).Count() > 1)
                {
                    throw new NotSupportedException("Cannot create type wrapper for multiple unrelated types");
                }
            }

            var specificTypes = types.Where(testType => testType.IsSpecific());
            if (specificTypes.Count() > 1)
            {
                throw new NotSupportedException("Cannot create type wrapper for multiple specific types");
            }

            this.types = types;
        }

        public static HostType Wrap(Type type)
        {
            return (type != null) ? new HostType(type) : null;
        }

        public static HostType Wrap(Type[] types)
        {
            return ((types != null) && (types.Length > 0)) ? new HostType(types) : null;
        }

        public Type[] Types
        {
            get { return types; }
        }

        public Type GetSpecificType()
        {
            var type = GetSpecificTypeNoThrow();
            if (type == null)
            {
                throw new InvalidOperationException(MiscHelpers.FormatInvariant("'{0}' requires type arguments", types[0].GetRootName()));
            }

            return type;
        }

        public Type GetTypeArg()
        {
            var type = GetSpecificType();
            if (type.IsStatic())
            {
                throw new InvalidOperationException(MiscHelpers.FormatInvariant("'{0}': static types cannot be used as type arguments", type.GetRootName()));
            }

            return type;
        }

        public Type GetTypeArgNoThrow()
        {
            var type = GetSpecificTypeNoThrow();
            return ((type == null) || type.IsStatic()) ? null : type;
        }

        private Type GetSpecificTypeNoThrow()
        {
            return types.FirstOrDefault(testType => testType.IsSpecific());
        }

        #region Object overrides

        public override string ToString()
        {
            var type = GetSpecificTypeNoThrow();
            if (type != null)
            {
                return MiscHelpers.FormatInvariant("HostType:{0}", type.GetFriendlyName());
            }

            var typeArgs = types[0].GetGenericArguments();
            var parentPrefix = string.Empty;
            if (types[0].IsNested)
            {
                var parentType = types[0].DeclaringType.MakeSpecificType(typeArgs);
                parentPrefix = parentType.GetFriendlyName() + ".";
            }

            return MiscHelpers.FormatInvariant((types.Length > 1) ? "HostTypeGroup:{0}{1}" : "GenericHostType:{0}{1}", parentPrefix, types[0].GetRootName());
        }

        #endregion

        #region HostTarget overrides

        public override Type Type
        {
            get { return GetSpecificTypeNoThrow() ?? types[0]; }
        }

        public override object Target
        {
            get { return this; }
        }

        public override object InvokeTarget
        {
            get
            {
                GetSpecificType();
                return null;
            }
        }

        public override object DynamicInvokeTarget
        {
            get { return GetSpecificType(); }
        }

        public override HostTargetFlags Flags
        {
            get
            {
                var type = GetSpecificTypeNoThrow();
                return (type != null) ? HostTargetFlags.AllowStaticMembers : HostTargetFlags.None;
            }
        }

        public override string[] GetAuxPropertyNames(BindingFlags bindFlags)
        {
            var type = GetSpecificTypeNoThrow();
            if (type != null)
            {
                return type.GetNestedTypes(bindFlags).Select(testType => testType.GetRootName()).Distinct().ToArray();
            }

            return MiscHelpers.GetEmptyArray<string>();
        }

        public override bool TryInvokeAuxMember(ScriptEngine engine, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            var type = GetSpecificTypeNoThrow();
            if (type != null)
            {
                var nestedTypes = type.GetNestedTypes(invokeFlags).Where(testType => testType.GetRootName() == name).ToIList();
                if (nestedTypes.Count > 0)
                {
                    var tempResult = Wrap(nestedTypes.Select(testType => testType.ApplyTypeArguments(type.GetGenericArguments())).ToArray());
                    if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
                    {
                        return tempResult.TryInvoke(engine, invokeFlags, args, bindArgs, out result);
                    }

                    result = tempResult;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvoke(ScriptEngine engine, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            if (!invokeFlags.HasFlag(BindingFlags.InvokeMethod) || (args.Length < 1))
            {
                result = null;
                return false;
            }

            if (!args.All(arg => arg is HostType))
            {
                throw new ArgumentException("Invalid generic type argument");
            }

            var templates = types.Where(type => !type.IsSpecific()).ToArray();
            var typeArgs = args.Cast<HostType>().Select(hostType => hostType.GetTypeArg()).ToArray();

            var template = templates.FirstOrDefault(testTemplate => testTemplate.GetGenericParamCount() == typeArgs.Length);
            if (template == null)
            {
                throw new TypeLoadException(MiscHelpers.FormatInvariant("Could not find a matching generic type definition for '{0}'", templates[0].GetRootName()));
            }

            result = Wrap(template.MakeSpecificType(typeArgs));
            return true;
        }

        #endregion

        #region IScriptableObject implementation

        void IScriptableObject.OnExposedToScriptCode(ScriptEngine engine)
        {
            if (engine != null)
            {
                var specificType = GetSpecificTypeNoThrow();
                if (specificType != null)
                {
                    engine.ProcessExtensionMethodType(specificType);
                }
            }
        }

        #endregion
    }
}
