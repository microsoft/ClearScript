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
        private HostType(Type type)
            : this(new[] { type })
        {
        }

        private HostType(Type[] types)
        {
            Debug.Assert((types is not null) && (types.Length > 0));

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

            Types = types;
        }

        public static HostType Wrap(Type type)
        {
            return (type is not null) ? new HostType(type) : null;
        }

        public static HostType Wrap(Type[] types)
        {
            return ((types is not null) && (types.Length > 0)) ? new HostType(types) : null;
        }

        public Type[] Types { get; }

        public Type GetSpecificType()
        {
            var type = GetSpecificTypeNoThrow();
            if (type is null)
            {
                throw new InvalidOperationException(MiscHelpers.FormatInvariant("'{0}' requires type arguments", Types[0].GetRootName()));
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
            return ((type is null) || type.IsStatic() || type.IsUnknownCOMObject()) ? null : type;
        }

        private Type GetSpecificTypeNoThrow()
        {
            return Types.FirstOrDefault(testType => testType.IsSpecific());
        }

        #region Object overrides

        public override string ToString()
        {
            var type = GetSpecificTypeNoThrow();
            if (type is not null)
            {
                return MiscHelpers.FormatInvariant("HostType:{0}", type.GetFriendlyName());
            }

            var typeArgs = Types[0].GetGenericArguments();
            var parentPrefix = string.Empty;
            if (Types[0].IsNested)
            {
                var parentType = Types[0].DeclaringType.MakeSpecificType(typeArgs);
                parentPrefix = parentType.GetFriendlyName() + ".";
            }

            return MiscHelpers.FormatInvariant((Types.Length > 1) ? "HostTypeGroup:{0}{1}" : "GenericHostType:{0}{1}", parentPrefix, Types[0].GetRootName());
        }

        #endregion

        #region HostTarget overrides

        public override Type Type => GetSpecificTypeNoThrow() ?? Types[0];

        public override object Target => this;

        public override object InvokeTarget
        {
            get
            {
                GetSpecificType();
                return null;
            }
        }

        public override object DynamicInvokeTarget => GetSpecificType();

        public override HostTargetFlags GetFlags(IHostContext context)
        {
            var type = GetSpecificTypeNoThrow();
            return (type is not null) ? HostTargetFlags.AllowStaticMembers : HostTargetFlags.None;
        }

        public override string[] GetAuxPropertyNames(IHostContext context, BindingFlags bindFlags)
        {
            var type = GetSpecificTypeNoThrow();
            if (type is not null)
            {
                return type.GetScriptableNestedTypes(context, bindFlags).Select(testType => testType.GetRootName()).Distinct().ToArray();
            }

            return ArrayHelpers.GetEmptyArray<string>();
        }

        public override bool TryInvokeAuxMember(IHostContext context, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            var type = GetSpecificTypeNoThrow();
            if (type is not null)
            {
                var nestedTypes = type.GetScriptableNestedTypes(context, invokeFlags).Where(testType => string.Equals(testType.GetRootName(), name, invokeFlags.GetMemberNameComparison())).ToIList();
                if (nestedTypes.Count > 0)
                {
                    var tempResult = Wrap(nestedTypes.Select(testType => testType.ApplyTypeArguments(type.GetGenericArguments())).ToArray());
                    if (invokeFlags.HasAllFlags(BindingFlags.InvokeMethod))
                    {
                        if (tempResult.TryInvoke(context, invokeFlags, args, bindArgs, out result))
                        {
                            return true;
                        }

                        if (!invokeFlags.HasAllFlags(BindingFlags.GetField) && !invokeFlags.HasAllFlags(BindingFlags.GetProperty))
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

        public override bool TryInvoke(IHostContext context, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            if (!invokeFlags.HasAllFlags(BindingFlags.InvokeMethod) || (args.Length < 1))
            {
                result = null;
                return false;
            }

            if (!args.All(arg => arg is HostType))
            {
                throw new ArgumentException("Invalid generic type argument", nameof(args));
            }

            var templates = Types.Where(type => !type.IsSpecific()).ToArray();
            var typeArgs = args.Cast<HostType>().Select(hostType => hostType.GetTypeArg()).ToArray();

            var template = templates.FirstOrDefault(testTemplate => testTemplate.GetGenericParamCount() == typeArgs.Length);
            if (template is null)
            {
                throw new TypeLoadException(MiscHelpers.FormatInvariant("Could not find a matching generic type definition for '{0}'", templates[0].GetRootName()));
            }

            result = Wrap(template.MakeSpecificType(typeArgs));
            return true;
        }

        public override Invocability GetInvocability(IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return Invocability.Delegate;
        }

        #endregion

        #region IScriptableObject implementation

        void IScriptableObject.OnExposedToScriptCode(ScriptEngine engine)
        {
            if (engine is not null)
            {
                var specificType = GetSpecificTypeNoThrow();
                if (specificType is not null)
                {
                    engine.ProcessExtensionMethodType(specificType);
                }
            }
        }

        #endregion
    }
}
