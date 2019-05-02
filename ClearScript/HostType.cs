// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class HostType : HostTarget, IScriptableObject
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

            if (type.IsUnknownCOMObject())
            {
                throw new InvalidOperationException("Unknown COM/ActiveX types cannot be used as type arguments");
            }

            return type;
        }

        public Type GetTypeArgNoThrow()
        {
            var type = GetSpecificTypeNoThrow();
            return ((type == null) || type.IsStatic() || type.IsUnknownCOMObject()) ? null : type;
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

        public override string[] GetAuxPropertyNames(IHostInvokeContext context, BindingFlags bindFlags)
        {
            var type = GetSpecificTypeNoThrow();
            if (type != null)
            {
                return type.GetScriptableNestedTypes(bindFlags, context.AccessContext, context.DefaultAccess).Select(testType => testType.GetRootName()).Distinct().ToArray();
            }

            return ArrayHelpers.GetEmptyArray<string>();
        }

        public override bool TryInvokeAuxMember(IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            var type = GetSpecificTypeNoThrow();
            if (type != null)
            {
                var nestedTypes = type.GetScriptableNestedTypes(invokeFlags, context.AccessContext, context.DefaultAccess).Where(testType => testType.GetRootName() == name).ToIList();
                if (nestedTypes.Count > 0)
                {
                    var tempResult = Wrap(nestedTypes.Select(testType => testType.ApplyTypeArguments(type.GetGenericArguments())).ToArray());
                    if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
                    {
                        if (tempResult.TryInvoke(context, invokeFlags, args, bindArgs, out result))
                        {
                            return true;
                        }

                        if (!invokeFlags.HasFlag(BindingFlags.GetField) && !invokeFlags.HasFlag(BindingFlags.GetProperty))
                        {
                            return false;
                        }
                    }

                    result = tempResult;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryInvoke(IHostInvokeContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
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

        public override Invocability GetInvocability(BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return Invocability.Delegate;
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
