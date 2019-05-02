// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal sealed class ExtensionMethodTable
    {
        private readonly Dictionary<Type, MethodInfo[]> table = new Dictionary<Type, MethodInfo[]>();
        private ExtensionMethodSummary summary = new ExtensionMethodSummary();

        public ExtensionMethodSummary Summary
        {
            get { return summary; }
        }

        public bool ProcessType(Type type, Type accessContext, ScriptAccess defaultAccess)
        {
            Debug.Assert(type.IsSpecific());
            if (!table.ContainsKey(type) && type.HasExtensionMethods())
            {
                const BindingFlags bindFlags = BindingFlags.Public | BindingFlags.Static;
                table[type] = type.GetMethods(bindFlags).Where(method => IsScriptableExtensionMethod(method, accessContext, defaultAccess)).ToArray();
                RebuildSummary();
                return true;
            }

            return false;
        }

        public void RebuildSummary()
        {
            summary = new ExtensionMethodSummary(table);
        }

        private static bool IsScriptableExtensionMethod(MethodInfo method, Type accessContext, ScriptAccess defaultAccess)
        {
            return method.IsScriptable(accessContext, defaultAccess) && method.IsDefined(typeof(ExtensionAttribute), false);
        }
    }

    internal sealed class ExtensionMethodSummary
    {
        public ExtensionMethodSummary()
        {
            Types = ArrayHelpers.GetEmptyArray<Type>();
            Methods = ArrayHelpers.GetEmptyArray<MethodInfo>();
            MethodNames = ArrayHelpers.GetEmptyArray<string>();
        }

        public ExtensionMethodSummary(Dictionary<Type, MethodInfo[]> table)
        {
            Types = table.Keys.ToArray();
            Methods = table.SelectMany(pair => pair.Value).ToArray();
            MethodNames = Methods.Select(method => method.GetScriptName()).ToArray();
        }

        public Type[] Types { get; private set; }

        public MethodInfo[] Methods { get; private set; }

        public string[] MethodNames { get; private set; }
    }
}
