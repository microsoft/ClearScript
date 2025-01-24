// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;

namespace Microsoft.ClearScript
{
    internal class HostTargetMemberData
    {
        public string[] TypeEventNames;
        public string[] TypeFieldNames;
        public string[] TypeMethodNames;
        public string[] TypePropertyNames;

        public string[] AllFieldNames;
        public string[] AllMethodNames;
        public string[] OwnMethodNames;
        public string[] AllPropertyNames;
        public string[] AllMemberNames;

        public FieldInfo[] AllFields;
        public MethodInfo[] AllMethods;
        public PropertyInfo[] AllProperties;

        public object EnumerationSettingsToken;
        public ExtensionMethodSummary ExtensionMethodSummary;
        public Invocability? TargetInvocability;
    }

    internal class HostTargetMemberDataWithContext : HostTargetMemberData
    {
        public readonly CustomAttributeLoader CustomAttributeLoader;
        public readonly Type AccessContext;
        public readonly ScriptAccess DefaultAccess;
        public readonly HostTargetFlags TargetFlags;

        public HostTargetMemberDataWithContext(CustomAttributeLoader customAttributeLoader, Type accessContext, ScriptAccess defaultAccess, HostTargetFlags targetFlags)
        {
            CustomAttributeLoader = customAttributeLoader;
            AccessContext = accessContext;
            DefaultAccess = defaultAccess;
            TargetFlags = targetFlags;
        }
    }
}
