// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides the base implementation for all script engines.
    /// </summary>
    public abstract class ScriptEngine : IDisposable
    {
        #region data

        private Type accessContext;
        private ScriptAccess defaultAccess;
        private bool enforceAnonymousTypeAccess;
        private bool exposeHostObjectStaticMembers;

        private DocumentSettings documentSettings;
        private readonly DocumentSettings defaultDocumentSettings = new DocumentSettings();

        private static readonly IUniqueNameManager nameManager = new UniqueNameManager();
        private static readonly object nullHostObjectProxy = new object();
        [ThreadStatic] private static ScriptEngine currentEngine;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new script engine instance.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        [Obsolete("Use ScriptEngine(string name, string fileNameExtensions) instead.")]
        protected ScriptEngine(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new script engine instance with the specified list of supported file name extensions.
        /// </summary>
        /// <param name="name">A name to associate with the instance. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="fileNameExtensions">A semicolon-delimited list of supported file name extensions.</param>
        protected ScriptEngine(string name, string fileNameExtensions)
        {
            Name = nameManager.GetUniqueName(name, GetType().GetRootName());
            defaultDocumentSettings.FileNameExtensions = fileNameExtensions;
            extensionMethodTable = realExtensionMethodTable = new ExtensionMethodTable();
        }

        #endregion

        #region public members

        /// <summary>
        /// Gets the name associated with the script engine instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the script engine that is invoking a host member on the current thread.
        /// </summary>
        /// <remarks>
        /// If multiple script engines are invoking host members on the current thread, this
        /// property gets the one responsible for the most deeply nested invocation. If no script
        /// engines are invoking host members on the current thread, this property returns
        /// <c>null</c>.
        /// </remarks>
        public static ScriptEngine Current => currentEngine;

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        public abstract string FileNameExtension { get; }

        /// <summary>
        /// Allows script code to access non-public host resources.
        /// </summary>
        /// <remarks>
        /// By setting this property to a type you declare that script code running in the current
        /// script engine is to be treated as if it were part of that type's implementation. Doing
        /// so does not expose any host resources to script code, but it affects which host
        /// resources are importable and which members of exposed resources are accessible.
        /// </remarks>
        public Type AccessContext
        {
            get => accessContext;

            set
            {
                accessContext = value;
                OnAccessSettingsChanged();
            }
        }

        /// <summary>
        /// Gets or sets the default script access setting for all members of exposed objects.
        /// </summary>
        /// <remarks>
        /// Use <see cref="DefaultScriptUsageAttribute"/>, <see cref="ScriptUsageAttribute"/>, or
        /// their subclasses to override this property for individual types and members. Note that
        /// this property has no effect on the method binding algorithm. If a script-based call is
        /// bound to a method that is blocked by this property, it will be rejected even if an
        /// overload exists that could receive the call.
        /// </remarks>
        public ScriptAccess DefaultAccess
        {
            get => defaultAccess;

            set
            {
                defaultAccess = value;
                OnAccessSettingsChanged();
            }
        }

        /// <summary>
        /// Enables or disables access restrictions for anonymous types.
        /// </summary>
        /// <remarks>
        /// Anonymous types are
        /// <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/internal">internal</see>
        /// and therefore accessible only within the same assembly, but ClearScript 5.5.3 and
        /// earlier permitted access to the public properties of an object even if its type was
        /// internal. Newer versions strictly enforce <see cref="AccessContext"/>, but because
        /// anonymous types are particularly useful for scripting, ClearScript by default continues
        /// to expose their properties to external contexts. To override this behavior and enable
        /// normal access restrictions for anonymous types, set this property to <c>true</c>.
        /// </remarks>
        public bool EnforceAnonymousTypeAccess
        {
            get => enforceAnonymousTypeAccess;

            set
            {
                enforceAnonymousTypeAccess = value;
                OnAccessSettingsChanged();
            }
        }

        /// <summary>
        /// Controls whether host objects provide access to the static members of their exposed types to script code.
        /// </summary>
        public bool ExposeHostObjectStaticMembers
        {
            get => exposeHostObjectStaticMembers;

            set
            {
                exposeHostObjectStaticMembers = value;
                OnAccessSettingsChanged();
            }
        }

        /// <summary>
        /// Enables or disables extension method support.
        /// </summary>
        public bool DisableExtensionMethods
        {
            get => extensionMethodTable == emptyExtensionMethodTable;

            set
            {
                var newExtensionMethodTable = value ? emptyExtensionMethodTable : realExtensionMethodTable;
                if (newExtensionMethodTable != extensionMethodTable)
                {
                    ScriptInvoke(() =>
                    {
                        if (newExtensionMethodTable != extensionMethodTable)
                        {
                            extensionMethodTable = newExtensionMethodTable;
                            bindCache.Clear();
                            OnAccessSettingsChanged();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Enables or disables script code formatting.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine may format script code
        /// before executing or compiling it. This is intended to facilitate interactive debugging.
        /// The formatting operation currently includes stripping leading and trailing blank lines
        /// and removing global indentation.
        /// </remarks>
        public bool FormatCode { get; set; }

        /// <summary>
        /// Controls whether script code is permitted to use reflection.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, script code running in the current script
        /// engine is permitted to use reflection. This affects
        /// <see cref="System.Object.GetType">Object.GetType()</see>,
        /// <see cref="System.Exception.GetType">Exception.GetType()</see>,
        /// <see cref="System.Delegate.Method">Delegate.Method</see>,
        /// <see cref="HostFunctions.typeOf(object)"/> and <see cref="HostFunctions.typeOf{T}"/>.
        /// By default, any attempt to invoke these members from script code results in an
        /// exception.
        /// </remarks>
        public bool AllowReflection { get; set; }

        /// <summary>
        /// Enables or disables type restriction for field, property, and method return values.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, script code running in the current script
        /// engine has access to the runtime types of all exposed host resources, which by default
        /// are restricted to their declared types. The default behavior is a general requirement
        /// for correct method binding, so setting this property to <c>true</c> is not recommended.
        /// </remarks>
        /// <seealso cref="ScriptMemberFlags.ExposeRuntimeType"/>
        public bool DisableTypeRestriction { get; set; }

        /// <summary>
        /// Enables or disables type restriction for array and list elements retrieved by index.
        /// </summary>
        /// <remarks>
        /// In ClearScript 5.4.4 and earlier, indexed array and list elements were exempt from type
        /// restriction. ClearScript 5.4.5 introduced a breaking change to correct this, but you can
        /// set this property to <c>true</c> to restore the exemption if you have older script code
        /// that depends on it.
        /// </remarks>
        /// <seealso cref="DisableTypeRestriction"/>
        public bool DisableListIndexTypeRestriction { get; set; }

        /// <summary>
        /// Enables or disables <c>null</c> wrapping for field, property, and method return values.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, all field, property, and method return values
        /// are marshaled with full .NET type information even if they are <c>null</c>. Note that
        /// such values will always fail equality comparison with JavaScript's
        /// <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/null">null</see></c>,
        /// VBScript's
        /// <c><see href="https://docs.microsoft.com/en-us/previous-versions//f8tbc79x(v=vs.85)">Nothing</see></c>,
        /// and other similar values. Instead, use <see cref="HostFunctions.isNull"/> or
        /// <see cref="object.Equals(object, object)"/> to perform such a comparison.
        /// </remarks>
        /// <seealso cref="ScriptMemberFlags.WrapNullResult"/>
        /// <seealso cref="HostFunctions.isNull"/>
        public bool EnableNullResultWrapping { get; set; }

        /// <summary>
        /// Enables or disables floating point narrowing.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, no attempt is made to convert floating-point
        /// values imported from the script engine to the narrowest equivalent .NET representation.
        /// The default behavior is more likely to result in successful method binding in specific
        /// scenarios, so setting this property to <c>true</c> is not recommended.
        /// </remarks>
        public bool DisableFloatNarrowing { get; set; }

        /// <summary>
        /// Enables or disables the use of reflection-based method binding as a fallback.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine attempts to use
        /// reflection-based method binding when the default method binding algorithm fails. This
        /// approach reduces type safety, but it may be useful for running legacy scripts that rely
        /// on the specific behavior of reflection-based method binding.
        /// </remarks>
        public bool UseReflectionBindFallback { get; set; }

        /// <summary>
        /// Enables or disables automatic host variable tunneling for by-reference arguments to script functions and delegates.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine replaces by-reference
        /// arguments to script functions and delegates with host variables, allowing script code
        /// to simulate output arguments if the script language does not support them natively.
        /// </remarks>
        /// <seealso cref="HostFunctions.newVar{T}(T)"/>
        public bool EnableAutoHostVariables { get; set; }

        /// <summary>
        /// Gets or sets the engine's undefined import value.
        /// </summary>
        /// <remarks>
        /// Some script languages support one or more special non-<c>null</c> values that represent
        /// nonexistent, missing, unknown, or undefined data. When such a value is marshaled to the
        /// host, the script engine maps it to the value of this property. The default value is
        /// <see cref="Undefined.Value"/>.
        /// </remarks>
        public object UndefinedImportValue { get; set; } = Undefined.Value;

        /// <summary>
        /// Gets or sets a callback that can be used to halt script execution.
        /// </summary>
        /// <remarks>
        /// During script execution the script engine periodically invokes this callback to
        /// determine whether it should continue. If the callback returns <c>false</c>, the script
        /// engine terminates script execution and throws an exception.
        /// </remarks>
        public ContinuationCallback ContinuationCallback { get; set; }

        /// <summary>
        /// Allows the host to access script resources dynamically.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It dynamically supports properties and methods that correspond to global
        /// script objects and functions.
        /// </remarks>
        public abstract dynamic Script { get; }

        /// <summary>
        /// Allows the host to access script resources.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It allows you to access global script resources via the
        /// <see cref="ScriptObject"/> class interface. Doing so is likely to perform better than
        /// dynamic access via <see cref="Script"/>.
        /// </remarks>
        public abstract ScriptObject Global { get; }

        /// <summary>
        /// Gets or sets the script engine's document settings.
        /// </summary>
        public DocumentSettings DocumentSettings
        {
            get => documentSettings ?? defaultDocumentSettings;
            set => documentSettings = value;
        }

        /// <summary>
        /// Exposes a host object to script code.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="target">The object to expose.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddHostObject(string itemName, object target)
        {
            AddHostObject(itemName, HostItemFlags.None, target);
        }

        /// <summary>
        /// Exposes a host object to script code with the specified options.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="target">The object to expose.</param>
        /// <remarks>
        /// <para>
        /// Once a host object is exposed to script code, its members are accessible via the script
        /// language's native syntax for member access. The following table provides details about
        /// the mapping between host members and script-accessible properties and methods.
        /// </para>
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Member&#xA0;Type</term>
        ///         <term>Exposed&#xA0;As</term>
        ///         <description>Remarks</description>
        ///     </listheader>
        ///     <item>
        ///         <term><b>Constructor</b></term>
        ///         <term>N/A</term>
        ///         <description>
        ///         To invoke a constructor from script code, call
        ///         <see cref="HostFunctions.newObj{T}">HostFunctions.newObj(T)</see>.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Property/Field</b></term>
        ///         <term><b>Property</b></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Method</b></term>
        ///         <term><b>Method</b></term>
        ///         <description>
        ///         Overloaded host methods are merged into a single script-callable method. At
        ///         runtime the correct host method is selected based on the argument types.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Generic&#xA0;Method</b></term>
        ///         <term><b>Method</b></term>
        ///         <description>
        ///         The ClearScript library supports dynamic C#-like type inference when invoking
        ///         generic methods. However, some methods require explicit type arguments. To call
        ///         such a method from script code, you must place the required number of
        ///         <see cref="AddHostType(string, HostItemFlags, Type)">host type objects</see>
        ///         at the beginning of the argument list. Doing so for methods that do not require
        ///         explicit type arguments is optional.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Extension&#xA0;Method</b></term>
        ///         <term><b>Method</b></term>
        ///         <description>
        ///         Extension methods are available if the type that implements them has been
        ///         exposed in the current script engine.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Indexer</b></term>
        ///         <term><b>Property</b></term>
        ///         <description>
        ///         Indexers appear as properties named "Item" that accept one or more index values
        ///         as arguments. In addition, objects that implement <see cref="IList"/> expose
        ///         properties with numeric names that match their valid indices. This includes
        ///         one-dimensional host arrays and other collections. Multidimensional host arrays
        ///         do not expose functional indexers; you must use
        ///         <see href="https://docs.microsoft.com/en-us/dotnet/api/system.array.getvalue">Array.GetValue</see>
        ///         and
        ///         <see href="https://docs.microsoft.com/en-us/dotnet/api/system.array.setvalue">Array.SetValue</see>
        ///         instead.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Event</b></term>
        ///         <term><b>Property</b></term>
        ///         <description>
        ///         Events are exposed as read-only properties of type <see cref="EventSource{T}"/>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public void AddHostObject(string itemName, HostItemFlags flags, object target)
        {
            MiscHelpers.VerifyNonNullArgument(target, nameof(target));
            AddHostItem(itemName, flags, target);
        }

        /// <summary>
        /// Exposes a host object to script code with the specified type restriction.
        /// </summary>
        /// <typeparam name="T">The type whose members are to be made accessible from script code.</typeparam>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="target">The object to expose.</param>
        /// <remarks>
        /// <para>
        /// This method can be used to restrict script access to the members of a particular
        /// interface or base class.
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddRestrictedHostObject<T>(string itemName, T target)
        {
            AddRestrictedHostObject(itemName, HostItemFlags.None, target);
        }

        /// <summary>
        /// Exposes a host object to script code with the specified type restriction and options.
        /// </summary>
        /// <typeparam name="T">The type whose members are to be made accessible from script code.</typeparam>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="target">The object to expose.</param>
        /// <remarks>
        /// <para>
        /// This method can be used to restrict script access to the members of a particular
        /// interface or base class.
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddRestrictedHostObject<T>(string itemName, HostItemFlags flags, T target)
        {
            AddHostItem(itemName, flags, HostItem.Wrap(this, target, typeof(T)));
        }

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code. The registered class is
        /// specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to instantiate.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMObject(string itemName, string progID)
        {
            AddCOMObject(itemName, HostItemFlags.None, progID);
        }

        /// <summary>
        /// Creates a COM/ActiveX object on the specified server and exposes it to script code. The
        /// registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to instantiate.</param>
        /// <param name="serverName">The name of the server on which to create the object.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMObject(string itemName, string progID, string serverName)
        {
            AddCOMObject(itemName, HostItemFlags.None, progID, serverName);
        }

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code with the specified options.
        /// The registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to instantiate.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMObject(string itemName, HostItemFlags flags, string progID)
        {
            AddCOMObject(itemName, flags, progID, null);
        }

        /// <summary>
        /// Creates a COM/ActiveX object on the specified server and exposes it to script code with
        /// the specified options. The registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to instantiate.</param>
        /// <param name="serverName">The name of the server on which to create the object.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMObject(string itemName, HostItemFlags flags, string progID, string serverName)
        {
            AddHostItem(itemName, flags, MiscHelpers.CreateCOMObject(progID, serverName));
        }

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code. The registered class is
        /// specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMObject(string itemName, Guid clsid)
        {
            AddCOMObject(itemName, HostItemFlags.None, clsid);
        }

        /// <summary>
        /// Creates a COM/ActiveX object on the specified server and exposes it to script code. The
        /// registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <param name="serverName">The name of the server on which to create the object.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMObject(string itemName, Guid clsid, string serverName)
        {
            AddCOMObject(itemName, HostItemFlags.None, clsid, serverName);
        }

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code with the specified options.
        /// The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid)
        {
            AddCOMObject(itemName, flags, clsid, null);
        }

        /// <summary>
        /// Creates a COM/ActiveX object on the specified server and exposes it to script code with
        /// the specified options. The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <param name="serverName">The name of the server on which to create the object.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid, string serverName)
        {
            AddHostItem(itemName, flags, MiscHelpers.CreateCOMObject(clsid, serverName));
        }

        /// <summary>
        /// Exposes a host type to script code with a default name.
        /// </summary>
        /// <param name="type">The type to expose.</param>
        /// <remarks>
        /// <para>
        /// This method uses <paramref name="type"/>'s name for the new global script item that
        /// will represent it.
        /// </para>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(Type type)
        {
            AddHostType(HostItemFlags.None, type);
        }

        /// <summary>
        /// Exposes a host type to script code with a default name and the specified options.
        /// </summary>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="type">The type to expose.</param>
        /// <remarks>
        /// <para>
        /// This method uses <paramref name="type"/>'s name for the new global script item that
        /// will represent it.
        /// </para>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(HostItemFlags flags, Type type)
        {
            AddHostType(type.GetRootName(), flags, type);
        }

        /// <summary>
        /// Exposes a host type to script code.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="type">The type to expose.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, Type type)
        {
            AddHostType(itemName, HostItemFlags.None, type);
        }

        /// <summary>
        /// Exposes a host type to script code with the specified options.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="type">The type to expose.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, HostItemFlags flags, Type type)
        {
            MiscHelpers.VerifyNonNullArgument(type, nameof(type));
            AddHostItem(itemName, flags, HostType.Wrap(type));
        }

        /// <summary>
        /// Exposes a host type to script code. The type is specified by name.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="typeName">The fully qualified name of the type to expose.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, string typeName, params Type[] typeArgs)
        {
            AddHostType(itemName, HostItemFlags.None, typeName, typeArgs);
        }

        /// <summary>
        /// Exposes a host type to script code with the specified options. The type is specified by name.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="typeName">The fully qualified name of the type to expose.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, HostItemFlags flags, string typeName, params Type[] typeArgs)
        {
            AddHostItem(itemName, flags, TypeHelpers.ImportType(typeName, null, false, typeArgs));
        }

        /// <summary>
        /// Exposes a host type to script code. The type is specified by type name and assembly name.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="typeName">The fully qualified name of the type to expose.</param>
        /// <param name="assemblyName">The name of the assembly that contains the type to expose.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddHostType(itemName, HostItemFlags.None, typeName, assemblyName, typeArgs);
        }

        /// <summary>
        /// Exposes a host type to script code with the specified options. The type is specified by
        /// type name and assembly name.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="typeName">The fully qualified name of the type to expose.</param>
        /// <param name="assemblyName">The name of the assembly that contains the type to expose.</param>
        /// <param name="typeArgs">Optional generic type arguments.</param>
        /// <remarks>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostType(string itemName, HostItemFlags flags, string typeName, string assemblyName, params Type[] typeArgs)
        {
            AddHostItem(itemName, flags, TypeHelpers.ImportType(typeName, assemblyName, true, typeArgs));
        }

        /// <summary>
        /// Exposes host types to script code.
        /// </summary>
        /// <param name="types">The types to expose.</param>
        /// <remarks>
        /// <para>
        /// This method uses each specified type's name for the new global script item that will
        /// represent it.
        /// </para>
        /// <para>
        /// Host types are exposed to script code in the form of objects whose properties and
        /// methods are bound to the type's static members and nested types. If the type has
        /// generic parameters, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// </para>
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddHostTypes(params Type[] types)
        {
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type != null)
                    {
                        AddHostType(type);
                    }
                }
            }
        }

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code. The registered class is
        /// specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to import.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMType(string itemName, string progID)
        {
            AddCOMType(itemName, HostItemFlags.None, progID);
        }

        /// <summary>
        /// Imports a COM/ActiveX type from the specified server and exposes it to script code. The
        /// registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to import.</param>
        /// <param name="serverName">The name of the server from which to import the type.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMType(string itemName, string progID, string serverName)
        {
            AddCOMType(itemName, HostItemFlags.None, progID, serverName);
        }

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code with the specified options.
        /// The registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to import.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMType(string itemName, HostItemFlags flags, string progID)
        {
            AddCOMType(itemName, flags, progID, null);
        }

        /// <summary>
        /// Imports a COM/ActiveX type from the specified server and exposes it to script code with
        /// the specified options. The registered class is specified by programmatic identifier (ProgID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="progID">The programmatic identifier (ProgID) of the registered class to import.</param>
        /// <param name="serverName">The name of the server from which to import the type.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="progID"/> argument can be a class identifier (CLSID) in standard
        /// GUID format with braces (e.g., "{0D43FE01-F093-11CF-8940-00A0C9054228}").
        /// </para>
        /// <para>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </para>
        /// </remarks>
        public void AddCOMType(string itemName, HostItemFlags flags, string progID, string serverName)
        {
            AddHostItem(itemName, flags, HostType.Wrap(MiscHelpers.GetCOMType(progID, serverName)));
        }

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code. The registered class is
        /// specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMType(string itemName, Guid clsid)
        {
            AddCOMType(itemName, HostItemFlags.None, clsid);
        }

        /// <summary>
        /// Imports a COM/ActiveX type from the specified server and exposes it to script code. The
        /// registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <param name="serverName">The name of the server from which to import the type.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMType(string itemName, Guid clsid, string serverName)
        {
            AddCOMType(itemName, HostItemFlags.None, clsid, serverName);
        }

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code with the specified options.
        /// The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMType(string itemName, HostItemFlags flags, Guid clsid)
        {
            AddCOMType(itemName, flags, clsid, null);
        }

        /// <summary>
        /// Imports a COM/ActiveX type from the specified server and exposes it to script code with
        /// the specified options. The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <param name="serverName">The name of the server from which to import the type.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <see cref="AddHostObject(string, HostItemFlags, object)"/>.
        /// </remarks>
        public void AddCOMType(string itemName, HostItemFlags flags, Guid clsid, string serverName)
        {
            AddHostItem(itemName, flags, HostType.Wrap(MiscHelpers.GetCOMType(clsid, serverName)));
        }

        /// <summary>
        /// Executes script code.
        /// </summary>
        /// <param name="code">The script code to execute.</param>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as a statement.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with an automatically selected name. This document will not be discarded
        /// after execution.
        /// </para>
        /// </remarks>
        public void Execute(string code)
        {
            Execute(null, code);
        }

        /// <summary>
        /// Executes script code with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to execute.</param>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as a statement.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with the specified name. This document will not be discarded after execution.
        /// </para>
        /// </remarks>
        public void Execute(string documentName, string code)
        {
            Execute(documentName, false, code);
        }

        /// <summary>
        /// Executes script code with an associated document name, optionally discarding the document after execution.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="discard"><c>True</c> to discard the script document after execution, <c>false</c> otherwise.</param>
        /// <param name="code">The script code to execute.</param>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as a statement.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with the specified name. Discarding this document removes it from view but
        /// has no effect on the script engine. Only Windows Script engines honor
        /// <paramref name="discard"/>.
        /// </para>
        /// </remarks>
        public void Execute(string documentName, bool discard, string code)
        {
            Execute(new DocumentInfo(documentName) { Flags = discard ? DocumentFlags.IsTransient : DocumentFlags.None }, code);
        }

        /// <summary>
        /// Executes script code with the specified document meta-information.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to execute.</param>
        /// <remarks>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as a statement.
        /// </remarks>
        public void Execute(DocumentInfo documentInfo, string code)
        {
            Execute(documentInfo.MakeUnique(this), code, false);
        }

        /// <summary>
        /// Loads and executes a script document.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and executed.</param>
        /// <remarks>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as a statement.
        /// </remarks>
        public void ExecuteDocument(string specifier)
        {
            ExecuteDocument(specifier, null);
        }

        /// <summary>
        /// Loads and executes a document with the specified category.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and executed.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <remarks>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as a statement.
        /// </remarks>
        public void ExecuteDocument(string specifier, DocumentCategory category)
        {
            ExecuteDocument(specifier, category, null);
        }

        /// <summary>
        /// Loads and executes a document with the specified category and context callback.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and executed.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <remarks>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as a statement.
        /// </remarks>
        public void ExecuteDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            Execute(document.Info, document.GetTextContents());
        }

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// This method is similar to <see cref="Evaluate(string)"/> but optimized for command
        /// consoles. The specified command must be limited to a single expression or statement.
        /// Script engines can override this method to customize command execution as well as the
        /// process of converting the result to a string for console output.
        /// </remarks>
        public virtual string ExecuteCommand(string command)
        {
            var documentInfo = new DocumentInfo("Command") { Flags = DocumentFlags.IsTransient };
            return GetCommandResultString(Evaluate(documentInfo.MakeUnique(this), command, false));
        }

        /// <summary>
        /// Evaluates script code.
        /// </summary>
        /// <param name="code">The script code to evaluate.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as an expression.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with an automatically selected name. This document will be discarded after
        /// execution.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object Evaluate(string code)
        {
            return Evaluate(null, code);
        }

        /// <summary>
        /// Evaluates script code with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="code">The script code to evaluate.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as an expression.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with the specified name. This document will be discarded after execution.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object Evaluate(string documentName, string code)
        {
            return Evaluate(documentName, true, code);
        }

        /// <summary>
        /// Evaluates script code with an associated document name, optionally discarding the document after execution.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
        /// <param name="discard"><c>True</c> to discard the script document after execution, <c>false</c> otherwise.</param>
        /// <param name="code">The script code to evaluate.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as an expression.
        /// </para>
        /// <para>
        /// If a debugger is attached, it will present the specified script code to the user as a
        /// document with the specified name. Discarding this document removes it from view but
        /// has no effect on the script engine. Only Windows Script engines honor
        /// <paramref name="discard"/>.
        /// </para>
        /// <para>
        /// The following table summarizes the types of result values that script code can return.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Type</term>
        ///         <term>Returned&#xA0;As</term>
        ///         <description>Remarks</description>
        ///     </listheader>
        ///     <item>
        ///         <term><b>String</b></term>
        ///         <term><see href="https://docs.microsoft.com/en-us/dotnet/api/system.string">System.String</see></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Boolean</b></term>
        ///         <term><see href="https://docs.microsoft.com/en-us/dotnet/api/system.boolean">System.Boolean</see></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Number</b></term>
        ///         <term><see href="https://docs.microsoft.com/en-us/dotnet/api/system.int32">System.Int32</see>&#xA0;or&#xA0;<see href="https://docs.microsoft.com/en-us/dotnet/api/system.double">System.Double</see></term>
        ///         <description>
        ///         Other numeric types are possible. The exact conversions between script and .NET
        ///         numeric types are defined by the script engine.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Null&#xA0;Reference</b></term>
        ///         <term><c>null</c></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Undefined</b></term>
        ///         <term><see cref="Undefined"/></term>
        ///         <description>
        ///         This represents JavaScript's
        ///         <see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see>,
        ///         VBScript's
        ///         <see href="https://docs.microsoft.com/en-us/previous-versions//f8tbc79x(v=vs.85)">Empty</see>,
        ///         etc.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Void</b></term>
        ///         <term><see cref="VoidResult"/></term>
        ///         <description>
        ///         This is returned when script code forwards the result of a host method that returns no value.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Host&#xA0;Object</b></term>
        ///         <term>Native&#xA0;.NET&#xA0;type</term>
        ///         <description>
        ///         This includes all .NET types not mentioned above, including value types (enums,
        ///         structs, etc.), and instances of all other classes. Script code can only create
        ///         these objects by invoking a host method or constructor. They are returned to
        ///         the host in their native .NET form.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Script&#xA0;Object</b></term>
        ///         <term><see cref="ScriptObject"/></term>
        ///         <description>
        ///         This includes all native script objects that have no .NET representation. C#'s
        ///         <see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#the-dynamic-type">dynamic</see>
        ///         keyword provides a convenient way to access them.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term>Other</term>
        ///         <term>Unspecified</term>
        ///         <description>
        ///         This includes host types and other ClearScript-specific objects intended for
        ///         script code use only. It may also include language-specific values that the
        ///         ClearScript library does not support. 
        ///         </description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public object Evaluate(string documentName, bool discard, string code)
        {
            return Evaluate(new DocumentInfo(documentName) { Flags = discard ? DocumentFlags.IsTransient : DocumentFlags.None }, code);
        }

        /// <summary>
        /// Evaluates script code with the specified document meta-information.
        /// </summary>
        /// <param name="documentInfo">A structure containing meta-information for the script document.</param>
        /// <param name="code">The script code to evaluate.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets the specified script code as an expression.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object Evaluate(DocumentInfo documentInfo, string code)
        {
            return Evaluate(documentInfo.MakeUnique(this, DocumentFlags.IsTransient), code, true);
        }

        /// <summary>
        /// Loads and evaluates a script document.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and evaluated.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as an expression.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object EvaluateDocument(string specifier)
        {
            return EvaluateDocument(specifier, null);
        }

        /// <summary>
        /// Loads and evaluates a document with the specified category.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and evaluated.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as an expression.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object EvaluateDocument(string specifier, DocumentCategory category)
        {
            return EvaluateDocument(specifier, category, null);
        }

        /// <summary>
        /// Loads and evaluates a document with the specified category and context callback.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and evaluated.</param>
        /// <param name="category">An optional category for the requested document.</param>
        /// <param name="contextCallback">An optional context callback for the requested document.</param>
        /// <returns>The result value.</returns>
        /// <remarks>
        /// <para>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as an expression.
        /// </para>
        /// <para>
        /// For information about the types of result values that script code can return, see
        /// <see cref="Evaluate(string, bool, string)"/>.
        /// </para>
        /// </remarks>
        public object EvaluateDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
        {
            MiscHelpers.VerifyNonBlankArgument(specifier, nameof(specifier), "Invalid document specifier");
            var document = DocumentSettings.LoadDocument(null, specifier, category, contextCallback);
            return Evaluate(document.Info, document.GetTextContents());
        }

        /// <summary>
        /// Invokes a global function or procedure.
        /// </summary>
        /// <param name="funcName">The name of the global function or procedure to invoke.</param>
        /// <param name="args">Optional invocation arguments.</param>
        /// <returns>The return value if a function was invoked, an undefined value otherwise.</returns>
        public object Invoke(string funcName, params object[] args)
        {
            MiscHelpers.VerifyNonBlankArgument(funcName, nameof(funcName), "Invalid function name");
            return ((IDynamic)Script).InvokeMethod(funcName, args ?? ArrayHelpers.GetEmptyArray<object>());
        }

        /// <summary>
        /// Gets a string representation of the script call stack.
        /// </summary>
        /// <returns>The script call stack formatted as a string.</returns>
        /// <remarks>
        /// This method returns an empty string if the script engine is not executing script code.
        /// The stack trace text format is defined by the script engine.
        /// </remarks>
        public abstract string GetStackTrace();

        /// <summary>
        /// Interrupts script execution and causes the script engine to throw an exception.
        /// </summary>
        /// <remarks>
        /// This method can be called safely from any thread.
        /// </remarks>
        public abstract void Interrupt();

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        public abstract void CollectGarbage(bool exhaustive);

        #endregion

        #region internal members

        internal abstract IUniqueNameManager DocumentNameManager { get; }

        internal virtual bool EnumerateInstanceMethods => true;

        internal virtual bool EnumerateExtensionMethods => EnumerateInstanceMethods;

        internal virtual bool UseCaseInsensitiveMemberBinding => false;

        internal abstract void AddHostItem(string itemName, HostItemFlags flags, object item);

        internal object PrepareResult<T>(T result, ScriptMemberFlags flags, bool isListIndexResult)
        {
            return PrepareResult(result, typeof(T), flags, isListIndexResult);
        }

        internal virtual object PrepareResult(object result, Type type, ScriptMemberFlags flags, bool isListIndexResult)
        {
            var wrapNull = flags.HasFlag(ScriptMemberFlags.WrapNullResult) || EnableNullResultWrapping;
            if (wrapNull && (result == null))
            {
                return HostObject.WrapResult(null, type, true);
            }

            if (!flags.HasFlag(ScriptMemberFlags.ExposeRuntimeType) && !DisableTypeRestriction && (!isListIndexResult || !DisableListIndexTypeRestriction))
            {
                return HostObject.WrapResult(result, type, wrapNull);
            }

            return result;
        }

        internal abstract object MarshalToScript(object obj, HostItemFlags flags);

        internal object MarshalToScript(object obj)
        {
            var hostItem = obj as HostItem;
            return MarshalToScript(obj, hostItem?.Flags ?? HostItemFlags.None);
        }

        internal object[] MarshalToScript(object[] args)
        {
            return args.Select(MarshalToScript).ToArray();
        }

        internal abstract object MarshalToHost(object obj, bool preserveHostTarget);

        internal object[] MarshalToHost(object[] args, bool preserveHostTargets)
        {
            return args.Select(arg => MarshalToHost(arg, preserveHostTargets)).ToArray();
        }

        internal abstract object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate);

        internal abstract object ExecuteRaw(UniqueDocumentInfo documentInfo, string code, bool evaluate);

        internal object Evaluate(UniqueDocumentInfo documentInfo, string code, bool marshalResult)
        {
            var result = Execute(documentInfo, code, true);
            if (marshalResult)
            {
                result = MarshalToHost(result, false);
            }

            return result;
        }

        internal string GetCommandResultString(object result)
        {
            if (result is HostItem hostItem)
            {
                if (hostItem.Target is IHostVariable)
                {
                    return result.ToString();
                }
            }

            var marshaledResult = MarshalToHost(result, false);

            if (marshaledResult is VoidResult)
            {
                return null;
            }

            if (marshaledResult == null)
            {
                return "[null]";
            }

            if (marshaledResult is Undefined)
            {
                return marshaledResult.ToString();
            }

            if (marshaledResult is ScriptItem)
            {
                return "[ScriptObject]";
            }

            return result.ToString();
        }

        internal void RequestInterrupt()
        {
            // Some script engines don't support IActiveScript::InterruptScriptThread(). This
            // method provides an alternate mechanism based on IActiveScriptSiteInterruptPoll.

            var tempScriptFrame = CurrentScriptFrame;
            if (tempScriptFrame != null)
            {
                tempScriptFrame.InterruptRequested = true;
            }
        }

        internal void CheckReflection()
        {
            if (!AllowReflection)
            {
                throw new UnauthorizedAccessException("Use of reflection is prohibited in this script engine");
            }
        }

        internal virtual void OnAccessSettingsChanged()
        {
        }

        #endregion

        #region host-side invocation

        internal virtual void HostInvoke(Action action)
        {
            action();
        }

        internal virtual T HostInvoke<T>(Func<T> func)
        {
            return func();
        }

        #endregion

        #region script-side invocation

        internal ScriptFrame CurrentScriptFrame { get; private set; }

        internal IDisposable CreateEngineScope()
        {
            return Scope.Create(() => MiscHelpers.Exchange(ref currentEngine, this), previousEngine => currentEngine = previousEngine);
        }

        internal virtual void ScriptInvoke(Action action)
        {
            using (CreateEngineScope())
            {
                ScriptInvokeInternal(action);
            }
        }

        internal virtual T ScriptInvoke<T>(Func<T> func)
        {
            using (CreateEngineScope())
            {
                return ScriptInvokeInternal(func);
            }
        }

        internal void ScriptInvokeInternal(Action action)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                action();
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal T ScriptInvokeInternal<T>(Func<T> func)
        {
            var previousScriptFrame = CurrentScriptFrame;
            CurrentScriptFrame = new ScriptFrame();

            try
            {
                return func();
            }
            finally
            {
                CurrentScriptFrame = previousScriptFrame;
            }
        }

        internal void ThrowScriptError()
        {
            if (CurrentScriptFrame != null)
            {
                ThrowScriptError(CurrentScriptFrame.ScriptError);
            }
        }

        internal static void ThrowScriptError(IScriptEngineException scriptError)
        {
            if (scriptError != null)
            {
                if (scriptError is ScriptInterruptedException)
                {
                    throw new ScriptInterruptedException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptException, scriptError.InnerException);
                }

                throw new ScriptEngineException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptException, scriptError.InnerException);
            }
        }

        #endregion

        #region synchronized invocation

        internal virtual void SyncInvoke(Action action)
        {
            action();
        }

        internal virtual T SyncInvoke<T>(Func<T> func)
        {
            return func();
        }

        #endregion

        #region enumeration settings

        internal object EnumerationSettingsToken { get; private set; } = new object();

        internal void OnEnumerationSettingsChanged()
        {
            EnumerationSettingsToken = new object();
        }

        #endregion

        #region extension method table

        private static readonly ExtensionMethodTable emptyExtensionMethodTable = new ExtensionMethodTable();

        private readonly ExtensionMethodTable realExtensionMethodTable;
        private ExtensionMethodTable extensionMethodTable;

        internal void ProcessExtensionMethodType(Type type)
        {
            if (extensionMethodTable != emptyExtensionMethodTable)
            {
                if (extensionMethodTable.ProcessType(type, AccessContext, DefaultAccess))
                {
                    bindCache.Clear();
                }
            }
        }

        internal ExtensionMethodSummary ExtensionMethodSummary => extensionMethodTable.Summary;

        internal void RebuildExtensionMethodSummary()
        {
            if (extensionMethodTable != emptyExtensionMethodTable)
            {
                extensionMethodTable.RebuildSummary();
            }
        }

        #endregion

        #region bind cache

        private readonly Dictionary<BindSignature, object> bindCache = new Dictionary<BindSignature, object>();

        internal void CacheBindResult(BindSignature signature, object result)
        {
            bindCache.Add(signature, result);
        }

        internal bool TryGetCachedBindResult(BindSignature signature, out object result)
        {
            return bindCache.TryGetValue(signature, out result);
        }

        #endregion

        #region host item cache

        private readonly ConditionalWeakTable<object, List<WeakReference>> hostObjectHostItemCache = new ConditionalWeakTable<object, List<WeakReference>>();
        private readonly ConditionalWeakTable<Type, List<WeakReference>> hostTypeHostItemCache = new ConditionalWeakTable<Type, List<WeakReference>>();

        internal HostItem GetOrCreateHostItem(HostTarget target, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            if (target is HostObject hostObject)
            {
                return GetOrCreateHostItemForHostObject(hostObject, hostObject.Target, flags, createHostItem);
            }

            if (target is HostType hostType)
            {
                return GetOrCreateHostItemForHostType(hostType, flags, createHostItem);
            }

            if (target is HostMethod hostMethod)
            {
                return GetOrCreateHostItemForHostObject(hostMethod, hostMethod, flags, createHostItem);
            }

            if (target is HostVariableBase hostVariable)
            {
                return GetOrCreateHostItemForHostObject(hostVariable, hostVariable, flags, createHostItem);
            }

            if (target is HostIndexedProperty hostIndexedProperty)
            {
                return GetOrCreateHostItemForHostObject(hostIndexedProperty, hostIndexedProperty, flags, createHostItem);
            }

            return CreateHostItem(target, flags, createHostItem, null);
        }

        private HostItem GetOrCreateHostItemForHostObject(HostTarget hostTarget, object target, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            var cacheEntry = hostObjectHostItemCache.GetOrCreateValue(target ?? nullHostObjectProxy);

            List<WeakReference> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                var hostItem = weakRef.Target as HostItem;
                if (hostItem == null)
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if ((hostItem.Target.Type == hostTarget.Type) && (hostItem.Flags == flags))
                    {
                        return hostItem;
                    }

                    if (activeWeakRefs == null)
                    {
                        activeWeakRefs = new List<WeakReference>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs != null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            return CreateHostItem(hostTarget, flags, createHostItem, cacheEntry);
        }

        private HostItem GetOrCreateHostItemForHostType(HostType hostType, HostItemFlags flags, HostItem.CreateFunc createHostItem)
        {
            if (hostType.Types.Length != 1)
            {
                return CreateHostItem(hostType, flags, createHostItem, null);
            }

            var cacheEntry = hostTypeHostItemCache.GetOrCreateValue(hostType.Types[0]);

            List<WeakReference> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                var hostItem = weakRef.Target as HostItem;
                if (hostItem == null)
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if (hostItem.Flags == flags)
                    {
                        return hostItem;
                    }

                    if (activeWeakRefs == null)
                    {
                        activeWeakRefs = new List<WeakReference>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs != null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            return CreateHostItem(hostType, flags, createHostItem, cacheEntry);
        }

        private HostItem CreateHostItem(HostTarget hostTarget, HostItemFlags flags, HostItem.CreateFunc createHostItem, List<WeakReference> cacheEntry)
        {
            var newHostItem = createHostItem(this, hostTarget, flags);

            if (cacheEntry != null)
            {
                cacheEntry.Add(new WeakReference(newHostItem));
            }

            if (hostTarget.Target is IScriptableObject scriptableObject)
            {
                scriptableObject.OnExposedToScriptCode(this);
            }

            return newHostItem;
        }

        #endregion

        #region host item collateral

        internal abstract HostItemCollateral HostItemCollateral { get; }

        #endregion

        #region shared host target member data

        internal readonly HostTargetMemberData SharedHostMethodMemberData = new HostTargetMemberData();
        internal readonly HostTargetMemberData SharedHostIndexedPropertyMemberData = new HostTargetMemberData();
        internal readonly HostTargetMemberData SharedScriptMethodMemberData = new HostTargetMemberData();

        private readonly ConditionalWeakTable<Type, List<WeakReference>> sharedHostObjectMemberDataCache = new ConditionalWeakTable<Type, List<WeakReference>>();

        internal HostTargetMemberData GetSharedHostObjectMemberData(HostObject target, Type targetAccessContext, ScriptAccess targetDefaultAccess, HostTargetFlags targetFlags)
        {
            var cacheEntry = sharedHostObjectMemberDataCache.GetOrCreateValue(target.Type);

            List<WeakReference> activeWeakRefs = null;
            var staleWeakRefCount = 0;

            foreach (var weakRef in cacheEntry)
            {
                var memberData = weakRef.Target as HostTargetMemberDataWithContext;
                if (memberData == null)
                {
                    staleWeakRefCount++;
                }
                else
                {
                    if ((memberData.AccessContext == targetAccessContext) && (memberData.DefaultAccess == targetDefaultAccess) && (memberData.TargetFlags == targetFlags))
                    {
                        return memberData;
                    }

                    if (activeWeakRefs == null)
                    {
                        activeWeakRefs = new List<WeakReference>(cacheEntry.Count);
                    }

                    activeWeakRefs.Add(weakRef);
                }
            }

            if (staleWeakRefCount > 4)
            {
                cacheEntry.Clear();
                if (activeWeakRefs != null)
                {
                    cacheEntry.Capacity = activeWeakRefs.Count + 1;
                    cacheEntry.AddRange(activeWeakRefs);
                }
            }

            var newMemberData = new HostTargetMemberDataWithContext(targetAccessContext, targetDefaultAccess, targetFlags);
            cacheEntry.Add(new WeakReference(newMemberData));
            return newMemberData;
        }

        #endregion

        #region event connections

        private readonly EventConnectionMap eventConnectionMap = new EventConnectionMap();

        internal EventConnection<T> CreateEventConnection<T>(object source, EventInfo eventInfo, Delegate handler)
        {
            return eventConnectionMap.Create<T>(this, source, eventInfo, handler);
        }

        internal void BreakEventConnection(IEventConnection connection)
        {
            eventConnectionMap.Break(connection);
        }

        private void BreakAllEventConnections()
        {
            eventConnectionMap.Dispose();
        }

        #endregion

        #region disposal / finalization

        /// <summary>
        /// Releases all resources used by the script engine.
        /// </summary>
        /// <remarks>
        /// Call <c>Dispose()</c> when you are finished using the script engine. <c>Dispose()</c>
        /// leaves the script engine in an unusable state. After calling <c>Dispose()</c>, you must
        /// release all references to the script engine so the garbage collector can reclaim the
        /// memory that the script engine was occupying.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the script engine and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>True</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// This method is called by the public <see cref="Dispose()"/> method and the
        /// <see cref="Finalize">Finalize</see> method. <see cref="Dispose()"/> invokes the
        /// protected <c>Dispose(Boolean)</c> method with the <paramref name="disposing"/>
        /// parameter set to <c>true</c>. <see cref="Finalize">Finalize</see> invokes
        /// <c>Dispose(Boolean)</c> with <paramref name="disposing"/> set to <c>false</c>.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                BreakAllEventConnections();
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the script engine is reclaimed by garbage collection.
        /// </summary>
        /// <remarks>
        /// This method overrides <see cref="System.Object.Finalize"/>. Application code should not
        /// call this method; an object's <c>Finalize()</c> method is automatically invoked during
        /// garbage collection, unless finalization by the garbage collector has been disabled by a
        /// call to <see cref="System.GC.SuppressFinalize"/>.
        /// </remarks>
        ~ScriptEngine()
        {
            Dispose(false);
        }

        #endregion

        #region Nested type: ScriptFrame

        internal sealed class ScriptFrame
        {
            public Exception HostException { get; set; }

            public IScriptEngineException ScriptError { get; set; }

            public IScriptEngineException PendingScriptError { get; set; }

            public bool InterruptRequested { get; set; }
        }

        #endregion

        #region Nested type: EventConnectionMap

        private sealed class EventConnectionMap : IDisposable
        {
            private readonly HashSet<IEventConnection> map = new HashSet<IEventConnection>();
            private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

            internal EventConnection<T> Create<T>(ScriptEngine engine, object source, EventInfo eventInfo, Delegate handler)
            {
                var connection = new EventConnection<T>(engine, source, eventInfo, handler);

                if (!disposedFlag.IsSet)
                {
                    lock (map)
                    {
                        map.Add(connection);
                    }
                }

                return connection;
            }

            internal void Break(IEventConnection connection)
            {
                var mustBreak = true;

                if (!disposedFlag.IsSet)
                {
                    lock (map)
                    {
                        mustBreak = map.Remove(connection);
                    }
                }

                if (mustBreak)
                {
                    connection.Break();
                }
            }

            public void Dispose()
            {
                if (disposedFlag.Set())
                {
                    var connections = new List<IEventConnection>();

                    lock (map)
                    {
                        connections.AddRange(map);
                    }

                    connections.ForEach(connection => connection.Break());
                }
            }
        }

        #endregion
    }
}
