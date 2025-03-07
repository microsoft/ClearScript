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
        private readonly Dictionary<Type, MethodInfo[]> table = new();

        public ExtensionMethodSummary Summary { get; private set; } = new();

        public bool ProcessType(IHostContext context, Type type)
        {
            Debug.Assert(type.IsSpecific());
            if (!table.ContainsKey(type) && type.HasExtensionMethods(context))
            {
                const BindingFlags bindFlags = BindingFlags.Public | BindingFlags.Static;
                table[type] = type.GetMethods(bindFlags).Where(method => IsScriptableExtensionMethod(context, method)).ToArray();
                RebuildSummary(context);
                return true;
            }

            return false;
        }

        public void RebuildSummary(IHostContext context)
        {
            Summary = new ExtensionMethodSummary(context, table);
        }

        private static bool IsScriptableExtensionMethod(IHostContext context, MethodInfo method)
        {
            return method.IsScriptable(context) && method.HasCustomAttributes<ExtensionAttribute>(context, false);
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

        public ExtensionMethodSummary(IHostContext context, Dictionary<Type, MethodInfo[]> table)
        {
            Types = table.Keys.ToArray();
            Methods = table.SelectMany(pair => pair.Value).ToArray();
            MethodNames = Methods.Select(method => method.GetScriptName(context)).ToArray();
        }

        public Type[] Types { get; }

        public MethodInfo[] Methods { get; }

        public string[] MethodNames { get; }
    }
}
