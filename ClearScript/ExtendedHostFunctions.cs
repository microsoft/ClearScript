// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides optional script-callable utility functions. This extended version allows script
    /// code to import host types.
    /// </summary>
    public class ExtendedHostFunctions : HostFunctions
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <c><see cref="ExtendedHostFunctions"/></c> instance.
        /// </summary>
        public ExtendedHostFunctions()
        {
            // the help file builder (SHFB) insists on an empty constructor here
        }

        // ReSharper restore EmptyConstructor

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Imports a host type by name.
        /// </summary>
        /// <param name="name">The fully qualified name of the host type to import.</param>
        /// <param name="hostTypeArgs">Optional generic type arguments.</param>
        /// <returns>The imported host type.</returns>
        /// <remarks>
        /// <para>
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="name"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <c><see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see></c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code imports the
        /// <c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2">Dictionary</see></c>
        /// generic type and uses it to create a string dictionary.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var DictT = host.type("System.Collections.Generic.Dictionary");
        /// var StringT = host.type("System.String");
        /// var dict = host.newObj(DictT(StringT, StringT));
        /// </code>
        /// Another way to create a string dictionary is to import the specific type directly.
        /// <code lang="JavaScript">
        /// var StringT = host.type("System.String");
        /// var StringDictT = host.type("System.Collections.Generic.Dictionary", StringT, StringT);
        /// var dict = host.newObj(StringDictT);
        /// </code>
        /// </example>
        public object type(string name, params object[] hostTypeArgs)
        {
            return TypeHelpers.ImportType(name, null, false, hostTypeArgs);
        }

        /// <summary>
        /// Imports a host type by name from the specified assembly.
        /// </summary>
        /// <param name="name">The fully qualified name of the host type to import.</param>
        /// <param name="assemblyName">The name of the assembly that contains the host type to import.</param>
        /// <param name="hostTypeArgs">Optional generic type arguments.</param>
        /// <returns>The imported host type.</returns>
        /// <remarks>
        /// <para>
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="name"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <c><see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see></c>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code imports <c><see cref="System.Linq.Enumerable"/></c> and uses it to create
        /// an array of strings.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var EnumerableT = host.type("System.Linq.Enumerable", "System.Core");
        /// var Int32T = host.type("System.Int32");
        /// var StringT = host.type("System.String");
        /// var SelectorT = host.type("System.Func", Int32T, StringT);
        /// var selector = host.del(SelectorT, function (num) { return StringT.Format("The number is {0}.", num); });
        /// var array = EnumerableT.Range(0, 5).Select(selector).ToArray();
        /// </code>
        /// </example>
        /// <c><seealso cref="type(string, object[])"/></c>
        public object type(string name, string assemblyName, params object[] hostTypeArgs)
        {
            return TypeHelpers.ImportType(name, assemblyName, true, hostTypeArgs);
        }

        /// <summary>
        /// Imports the host type for the specified <c><see cref="System.Type"/></c>.
        /// </summary>
        /// <param name="type">The <c><see cref="System.Type"/></c> that specifies the host type to import.</param>
        /// <returns>The imported host type.</returns>
        /// <remarks>
        /// <para>
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="type"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <c><see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see></c>.
        /// </para>
        /// </remarks>
        public object type(Type type)
        {
            return HostType.Wrap(type);
        }

        /// <summary>
        /// Imports the host array type for the specified element type.
        /// </summary>
        /// <typeparam name="T">The element type for the host array type to import.</typeparam>
        /// <param name="rank">The number of dimensions for the host array type to import.</param>
        /// <returns>The imported host array type.</returns>
        public object arrType<T>(int rank = 1)
        {
            return HostType.Wrap(typeof(T).MakeArrayType(rank));
        }

        /// <summary>
        /// Imports types from one or more host assemblies.
        /// </summary>
        /// <param name="assemblyNames">The names of the assemblies that contain the types to import.</param>
        /// <returns>The imported host type collection.</returns>
        /// <remarks>
        /// Host type collections provide convenient scriptable access to all the types defined in one
        /// or more host assemblies. They are hierarchical collections where leaf nodes represent types
        /// and parent nodes represent namespaces. For example, if an assembly contains a type named
        /// "Acme.Gadgets.Button", the corresponding collection will have a property named "Acme" whose
        /// value is an object with a property named "Gadgets" whose value is an object with a property
        /// named "Button" whose value represents the <c>Acme.Gadgets.Button</c> host type.
        /// </remarks>
        /// <example>
        /// The following code imports types from several core assemblies and uses
        /// <c><see cref="System.Linq.Enumerable"/></c> to create an array of integers.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var clr = host.lib("mscorlib", "System", "System.Core");
        /// var array = clr.System.Linq.Enumerable.Range(0, 5).ToArray();
        /// </code>
        /// </example>
        public HostTypeCollection lib(params string[] assemblyNames)
        {
            return lib(null, assemblyNames);
        }

        /// <summary>
        /// Imports types from one or more host assemblies and merges them with an existing host type collection.
        /// </summary>
        /// <param name="collection">The host type collection with which to merge types from the specified assemblies.</param>
        /// <param name="assemblyNames">The names of the assemblies that contain the types to import.</param>
        /// <returns>A host type collection: <paramref name="collection"/> if it is not <c>null</c>, a new host type collection otherwise.</returns>
        /// <remarks>
        /// Host type collections provide convenient scriptable access to all the types defined in one
        /// or more host assemblies. They are hierarchical collections where leaf nodes represent types
        /// and parent nodes represent namespaces. For example, if an assembly contains a type named
        /// "Acme.Gadgets.Button", the corresponding collection will have a property named "Acme" whose
        /// value is an object with a property named "Gadgets" whose value is an object with a property
        /// named "Button" whose value represents the <c>Acme.Gadgets.Button</c> host type.
        /// </remarks>
        /// <example>
        /// The following code imports types from several core assemblies and uses
        /// <c><see cref="System.Linq.Enumerable"/></c> to create an array of integers.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var clr = host.lib("mscorlib");
        /// host.lib(clr, "System");
        /// host.lib(clr, "System.Core");
        /// var array = clr.System.Linq.Enumerable.Range(0, 5).ToArray();
        /// </code>
        /// </example>
        public HostTypeCollection lib(HostTypeCollection collection, params string[] assemblyNames)
        {
            var target = collection ?? new HostTypeCollection();
            Array.ForEach(assemblyNames, target.AddAssembly);
            return target;
        }

        /// <summary>
        /// Imports a COM/ActiveX type.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to import.</param>
        /// <param name="serverName">An optional name that specifies the server from which to import the type.</param>
        /// <returns>The imported COM/ActiveX type.</returns>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </remarks>
        /// <example>
        /// The following code imports the
        /// <c><see href="https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/x4k5wbx4(v=vs.84)">Scripting.Dictionary</see></c>
        /// class and uses it to create and populate an instance.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var DictT = host.comType('Scripting.Dictionary');
        /// var dict = host.newObj(DictT);
        /// dict.Add('foo', 123);
        /// dict.Add('bar', 456.789);
        /// dict.Add('baz', 'abc');
        /// </code>
        /// </example>
        public object comType(string progID, string serverName = null)
        {
            return HostType.Wrap(MiscHelpers.GetCOMType(progID, serverName));
        }

        // ReSharper disable GrammarMistakeInComment

        /// <summary>
        /// Creates a COM/ActiveX object of the specified type.
        /// </summary>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to instantiate.</param>
        /// <param name="serverName">An optional name that specifies the server on which to create the object.</param>
        /// <returns>A new COM/ActiveX object of the specified type.</returns>
        /// <remarks>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </remarks>
        /// <example>
        /// The following code creates a 
        /// <c><see href="https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/windows-scripting/6kxy1a51(v=vs.84)">Scripting.FileSystemObject</see></c>
        /// instance and uses it to list the drives on the local machine.
        /// It assumes that an instance of <c><see cref="ExtendedHostFunctions"/></c> is exposed under
        /// the name "host"
        /// (see <c><see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see></c>).
        /// <code lang="JavaScript">
        /// var fso = host.newComObj('Scripting.FileSystemObject');
        /// var ConsoleT = host.type('System.Console');
        /// for (en = fso.Drives.GetEnumerator(); en.MoveNext();) {
        ///     ConsoleT.WriteLine(en.Current.Path);
        /// }
        /// </code>
        /// </example>
        public object newComObj(string progID, string serverName = null)
        {
            return MiscHelpers.CreateCOMObject(progID, serverName);
        }

        // ReSharper restore GrammarMistakeInComment

        /// <summary>
        /// Imports enumerations defined within or referenced from a COM/ActiveX type library.
        /// </summary>
        /// <typeparam name="T">The imported type whose parent library is to be searched for relevant enumerations.</typeparam>
        /// <param name="obj">An instance of the representative type.</param>
        /// <param name="collection">An optional host type collection with which to merge the imported enumerations.</param>
        /// <returns>A host type collection: <paramref name="collection"/> if it is not <c>null</c>, a new host type collection otherwise.</returns>
        public HostTypeCollection typeLibEnums<T>(T obj, HostTypeCollection collection = null) where T : class
        {
            MiscHelpers.VerifyNonNullArgument(obj, nameof(obj));
            if (collection is null)
            {
                collection = new HostTypeCollection();
            }

            var type = typeof(T);
            if (type.IsUnknownCOMObject())
            {
                if (obj is IDispatch dispatch)
                {
                    var typeInfo = dispatch.GetTypeInfo();
                    if (typeInfo is not null)
                    {
                        typeInfo.GetContainingTypeLib().GetReferencedEnums().ForEach(collection.AddEnumTypeInfo);
                        return collection;
                    }
                }
            }
            else if (type.IsImport && (type.Assembly.GetOrLoadCustomAttribute<ImportedFromTypeLibAttribute>(ScriptEngine.Current, false) is not null))
            {
                type.Assembly.GetReferencedEnums().ForEach(collection.AddType);
                return collection;
            }

            throw new ArgumentException("The object type is not of an imported (COM/ActiveX) type", nameof(obj));
        }

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
