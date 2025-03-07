// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a scriptable collection of host types.
    /// </summary>
    /// <remarks>
    /// Host type collections provide convenient scriptable access to all the types defined in one
    /// or more host assemblies. They are hierarchical collections where leaf nodes represent types
    /// and parent nodes represent namespaces. For example, if an assembly contains a type named
    /// "Acme.Gadgets.Button", the corresponding collection will have a property named "Acme" whose
    /// value is an object with a property named "Gadgets" whose value is an object with a property
    /// named "Button" whose value represents the <c>Acme.Gadgets.Button</c> host type. Use
    /// <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c> to expose a host
    /// type collection to script code.
    /// </remarks>
    public class HostTypeCollection : PropertyBag
    {
        private static readonly Predicate<Type> defaultFilter = _ => true;
        private static readonly TypeComparer typeComparer = new();

        /// <summary>
        /// Initializes a new host type collection.
        /// </summary>
        public HostTypeCollection()
            : base(true)
        {
        }

        /// <summary>
        /// Initializes a new host type collection with types from one or more assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(params Assembly[] assemblies)
            : base(true)
        {
            MiscHelpers.VerifyNonNullArgument(assemblies, nameof(assemblies));
            Array.ForEach(assemblies, AddAssembly);
        }

        /// <summary>
        /// Initializes a new host type collection with types from one or more assemblies. The
        /// assemblies are specified by name.
        /// </summary>
        /// <param name="assemblyNames">The names of the assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(params string[] assemblyNames)
            : base(true)
        {
            MiscHelpers.VerifyNonNullArgument(assemblyNames, nameof(assemblyNames));
            Array.ForEach(assemblyNames, AddAssembly);
        }

        /// <summary>
        /// Initializes a new host type collection with selected types from one or more assemblies.
        /// </summary>
        /// <param name="filter">A filter for selecting the types to add.</param>
        /// <param name="assemblies">The assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(Predicate<Type> filter, params Assembly[] assemblies)
        {
            MiscHelpers.VerifyNonNullArgument(assemblies, nameof(assemblies));
            Array.ForEach(assemblies, assembly => AddAssembly(assembly, filter));
        }

        /// <summary>
        /// Initializes a new host type collection with selected types from one or more assemblies.
        /// The assemblies are specified by name.
        /// </summary>
        /// <param name="filter">A filter for selecting the types to add.</param>
        /// <param name="assemblyNames">The names of the assemblies that contain the types with which to initialize the collection.</param>
        public HostTypeCollection(Predicate<Type> filter, params string[] assemblyNames)
        {
            MiscHelpers.VerifyNonNullArgument(assemblyNames, nameof(assemblyNames));
            Array.ForEach(assemblyNames, assemblyName => AddAssembly(assemblyName, filter));
        }

        /// <summary>
        /// Adds types from an assembly to a host type collection.
        /// </summary>
        /// <param name="assembly">The assembly that contains the types to add.</param>
        public void AddAssembly(Assembly assembly)
        {
            MiscHelpers.VerifyNonNullArgument(assembly, nameof(assembly));
            assembly.GetAllTypes().Where(type => type.IsImportable(null)).ForEach(AddType);
        }

        /// <summary>
        /// Adds types from an assembly to a host type collection. The assembly is specified by name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the types to add.</param>
        public void AddAssembly(string assemblyName)
        {
            MiscHelpers.VerifyNonBlankArgument(assemblyName, nameof(assemblyName), "Invalid assembly name");
            AddAssembly(Assembly.Load(AssemblyTable.GetFullAssemblyName(assemblyName)));
        }

        /// <summary>
        /// Adds selected types from an assembly to a host type collection.
        /// </summary>
        /// <param name="assembly">The assembly that contains the types to add.</param>
        /// <param name="filter">A filter for selecting the types to add.</param>
        public void AddAssembly(Assembly assembly, Predicate<Type> filter)
        {
            MiscHelpers.VerifyNonNullArgument(assembly, nameof(assembly));
            var activeFilter = filter ?? defaultFilter;
            assembly.GetAllTypes().Where(type => type.IsImportable(null) && activeFilter(type)).ForEach(AddType);
        }

        /// <summary>
        /// Adds selected types from an assembly to a host type collection. The assembly is
        /// specified by name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that contains the types to add.</param>
        /// <param name="filter">A filter for selecting the types to add.</param>
        public void AddAssembly(string assemblyName, Predicate<Type> filter)
        {
            MiscHelpers.VerifyNonBlankArgument(assemblyName, nameof(assemblyName), "Invalid assembly name");
            AddAssembly(Assembly.Load(AssemblyTable.GetFullAssemblyName(assemblyName)), filter);
        }

        /// <summary>
        /// Adds a type to a host type collection.
        /// </summary>
        /// <param name="type">The type to add.</param>
        public void AddType(Type type)
        {
            MiscHelpers.VerifyNonNullArgument(type, nameof(type));
            AddType(HostType.Wrap(type));
        }

        /// <summary>
        /// Adds a type to a host type collection. The type is specified by name.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to add.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        public void AddType(string typeName, params Type[] typeArgs)
        {
            AddType(TypeHelpers.ImportType(typeName, null, false, typeArgs));
        }

        /// <summary>
        /// Adds a type to a host type collection. The type is specified by type name and assembly name.
        /// </summary>
        /// <param name="typeName">The fully qualified name of the type to add.</param>
        /// <param name="assemblyName">The name of the assembly that contains the type to add.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        public void AddType(string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddType(TypeHelpers.ImportType(typeName, assemblyName, true, typeArgs));
        }

        /// <summary>
        /// Locates a namespace within a host type collection.
        /// </summary>
        /// <param name="name">The full name of the namespace to locate.</param>
        /// <returns>The node that represents the namespace if it was found, <c>null</c> otherwise.</returns>
        public PropertyBag GetNamespaceNode(string name)
        {
            MiscHelpers.VerifyNonNullArgument(name, nameof(name));

            PropertyBag namespaceNode = this;

            var segments = name.Split('.');
            foreach (var segment in segments)
            {
                if (!namespaceNode.TryGetValue(segment, out var node))
                {
                    return null;
                }

                namespaceNode = node as PropertyBag;
                if (namespaceNode is null)
                {
                    return null;
                }
            }

            return namespaceNode;
        }

        internal void AddEnumTypeInfo(ITypeInfo typeInfo)
        {
            AddEnumTypeInfoInternal(typeInfo);
        }

        private PropertyBag AddEnumTypeInfoInternal(ITypeInfo typeInfo)
        {
            using (var attrScope = typeInfo.CreateAttrScope())
            {
                if (attrScope.Value.typekind == TYPEKIND.TKIND_ALIAS)
                {
                    typeInfo.GetRefTypeInfo(unchecked((int)attrScope.Value.tdescAlias.lpValue.ToInt64()), out var refTypeInfo);

                    var node = AddEnumTypeInfoInternal(refTypeInfo);
                    if (node is not null)
                    {
                        var locator = typeInfo.GetManagedName();

                        var segments = locator.Split('.');
                        if (segments.Length > 0)
                        {
                            var namespaceNode = GetOrCreateNamespaceNode(locator);
                            if (namespaceNode is not null)
                            {
                                namespaceNode.SetPropertyNoCheck(segments.Last(), node);
                                return node;
                            }
                        }
                    }
                }
                else if (attrScope.Value.typekind == TYPEKIND.TKIND_ENUM)
                {
                    var node = GetOrCreateEnumTypeInfoNode(typeInfo);
                    if (node is not null)
                    {
                        var count = attrScope.Value.cVars;
                        for (var index = 0; index < count; index++)
                        {
                            using (var varDescScope = typeInfo.CreateVarDescScope(index))
                            {
                                if (varDescScope.Value.varkind == VARKIND.VAR_CONST)
                                {
                                    var name = typeInfo.GetMemberName(varDescScope.Value.memid);
                                    node.SetPropertyNoCheck(name, MiscHelpers.GetObjectForVariant(varDescScope.Value.desc.lpvarValue));
                                }
                            }
                        }

                        return node;
                    }
                }
            }

            return null;
        }

        private PropertyBag GetOrCreateEnumTypeInfoNode(ITypeInfo typeInfo)
        {
            var locator = typeInfo.GetManagedName();

            var segments = locator.Split('.');
            if (segments.Length < 1)
            {
                return null;
            }

            PropertyBag enumTypeInfoNode = this;
            foreach (var segment in segments)
            {
                PropertyBag innerNode;

                if (!enumTypeInfoNode.TryGetValue(segment, out var node))
                {
                    innerNode = new PropertyBag(true);
                    enumTypeInfoNode.SetPropertyNoCheck(segment, innerNode);
                }
                else
                {
                    innerNode = node as PropertyBag;
                    if (innerNode is null)
                    {
                        throw new OperationCanceledException(MiscHelpers.FormatInvariant("Enumeration conflicts with '{0}' at '{1}'", node.GetFriendlyName(), locator));
                    }
                }

                enumTypeInfoNode = innerNode;
            }

            return enumTypeInfoNode;
        }

        private void AddType(HostType hostType)
        {
            MiscHelpers.VerifyNonNullArgument(hostType, nameof(hostType));
            foreach (var type in hostType.Types)
            {
                var namespaceNode = GetOrCreateNamespaceNode(type);
                if (namespaceNode is not null)
                {
                    AddTypeToNamespaceNode(namespaceNode, type);
                }
            }
        }

        private PropertyBag GetOrCreateNamespaceNode(Type type)
        {
            return GetOrCreateNamespaceNode(type.GetLocator());
        }

        private PropertyBag GetOrCreateNamespaceNode(string locator)
        {
            var segments = locator.Split('.');
            if (segments.Length < 1)
            {
                return null;
            }

            PropertyBag namespaceNode = this;
            foreach (var segment in segments.Take(segments.Length - 1))
            {
                PropertyBag innerNode;

                if (!namespaceNode.TryGetValue(segment, out var node))
                {
                    innerNode = new PropertyBag(true);
                    namespaceNode.SetPropertyNoCheck(segment, innerNode);
                }
                else
                {
                    innerNode = node as PropertyBag;
                    if (innerNode is null)
                    {
                        throw new OperationCanceledException(MiscHelpers.FormatInvariant("Namespace conflicts with '{0}' at '{1}'", node.GetFriendlyName(), locator));
                    }
                }

                namespaceNode = innerNode;
            }

            return namespaceNode;
        }

        private static void AddTypeToNamespaceNode(PropertyBag node, Type type)
        {
            var name = type.GetRootName();
            if (!node.TryGetValue(name, out var value))
            {
                node.SetPropertyNoCheck(name, HostType.Wrap(type));
                return;
            }

            if (value is HostType hostType)
            {
                var types = type.ToEnumerable().Concat(hostType.Types).ToArray();

                var groups = types.GroupBy(testType => testType.GetGenericParamCount()).ToIList();
                if (groups.Any(group => group.Count() > 1))
                {
                    types = groups.Select(ResolveTypeConflict).ToArray();
                }

                node.SetPropertyNoCheck(name, HostType.Wrap(types));
                return;
            }

            if (value is PropertyBag)
            {
                throw new OperationCanceledException(MiscHelpers.FormatInvariant("Type conflicts with namespace at '{0}'", type.GetLocator()));
            }

            throw new OperationCanceledException(MiscHelpers.FormatInvariant("Type conflicts with '{0}' at '{1}'", value.GetFriendlyName(), type.GetLocator()));
        }

        private static Type ResolveTypeConflict(IEnumerable<Type> types)
        {
            var typeList = types.Distinct(typeComparer).ToIList();
            return typeList.SingleOrDefault(type => type.IsPublic) ?? typeList[0];
        }

        #region Nested type : TypeComparer

        private sealed class TypeComparer : EqualityComparer<Type>
        {
            public override bool Equals(Type x, Type y) => (x == y) || (x.AssemblyQualifiedName == y.AssemblyQualifiedName);

            public override int GetHashCode(Type type) => type.AssemblyQualifiedName.GetHashCode();
        }

        #endregion
    }
}
