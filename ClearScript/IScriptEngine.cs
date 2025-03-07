// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Represents a script engine.
    /// </summary>
    public interface IScriptEngine : IDisposable
    {
        /// <summary>
        /// Gets the name associated with the script engine instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the script engine's recommended file name extension for script files.
        /// </summary>
        string FileNameExtension { get; }

        /// <summary>
        /// Allows script code to access non-host resources.
        /// </summary>
        /// <remarks>
        /// By setting this property to a type you declare that script code running in the current
        /// script engine is to be treated as if it were part of that type's implementation. Doing
        /// so does not expose any host resources to script code, but it affects which host
        /// resources are importable and which members of exposed resources are accessible.
        /// </remarks>
        Type AccessContext { get; set; }

        /// <summary>
        /// Gets or sets the default script access setting for all members of exposed objects.
        /// </summary>
        /// <remarks>
        /// Use <c><see cref="DefaultScriptUsageAttribute"/></c>, <c><see cref="ScriptUsageAttribute"/></c>, or
        /// their subclasses to override this property for individual types and members. Note that
        /// this property has no effect on the method binding algorithm. If a script-based call is
        /// bound to a method that is blocked by this property, it will be rejected even if an
        /// overload exists that could receive the call.
        /// </remarks>
        ScriptAccess DefaultAccess { get; set; }

        /// <summary>
        /// Enables or disables access restrictions for anonymous types.
        /// </summary>
        /// <remarks>
        /// Anonymous types are
        /// <c><see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/internal">internal</see></c>
        /// and therefore accessible only within the same assembly, but ClearScript 5.5.3 and
        /// earlier permitted access to the properties of an object even if its type was
        /// internal. Newer versions strictly enforce <c><see cref="AccessContext"/></c>, but because
        /// anonymous types are particularly useful for scripting, ClearScript by default continues
        /// to expose their properties to external contexts. To override this behavior and enable
        /// normal access restrictions for anonymous types, set this property to <c>true</c>.
        /// </remarks>
        bool EnforceAnonymousTypeAccess { get; set; }

        /// <summary>
        /// Controls whether host objects provide access to the static members of their exposed types to script code.
        /// </summary>
        bool ExposeHostObjectStaticMembers { get; set; }

        /// <summary>
        /// Enables or disables extension method support.
        /// </summary>
        bool DisableExtensionMethods { get; set; }

        /// <summary>
        /// Enables or disables script code formatting.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine may format script code
        /// before executing or compiling it. This is intended to facilitate interactive debugging.
        /// The formatting operation currently includes stripping leading and trailing blank lines
        /// and removing global indentation.
        /// </remarks>
        bool FormatCode { get; set; }

        /// <summary>
        /// Controls whether script code is permitted to use reflection.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, script code running in the current script
        /// engine is permitted to use reflection. This affects
        /// <c><see cref="object.GetType">Object.GetType()</see></c>,
        /// <c><see cref="Exception.GetType">Exception.GetType()</see></c>,
        /// <c><see cref="Exception.TargetSite">Exception.TargetSite</see></c>,
        /// <c><see cref="Delegate.Method">Delegate.Method</see></c>,
        /// <c><see cref="HostFunctions.typeOf(object)"/></c> and <c><see cref="HostFunctions.typeOf{T}"/></c>.
        /// By default, any attempt to invoke these members from script code results in an
        /// exception.
        /// </remarks>
        bool AllowReflection { get; set; }

        /// <summary>
        /// Enables or disables type restriction for field, property, and method return values.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, script code running in the current script
        /// engine has access to the runtime types of all exposed host resources, which by default
        /// are restricted to their declared types. The default behavior is a general requirement
        /// for correct method binding, so setting this property to <c>true</c> is not recommended.
        /// </remarks>
        /// <c><seealso cref="ScriptMemberFlags.ExposeRuntimeType"/></c>
        bool DisableTypeRestriction { get; set; }

        /// <summary>
        /// Enables or disables type restriction for array and list elements retrieved by index.
        /// </summary>
        /// <remarks>
        /// In ClearScript 5.4.4 and earlier, indexed array and list elements were exempt from type
        /// restriction. ClearScript 5.4.5 introduced a breaking change to correct this, but you can
        /// set this property to <c>true</c> to restore the exemption if you have older script code
        /// that depends on it.
        /// </remarks>
        /// <c><seealso cref="DisableTypeRestriction"/></c>
        bool DisableListIndexTypeRestriction { get; set; }

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
        /// and other similar values. Instead, use <c><see cref="HostFunctions.isNull"/></c> or
        /// <c><see cref="object.Equals(object, object)"/></c> to perform such a comparison.
        /// </remarks>
        /// <c><seealso cref="ScriptMemberFlags.WrapNullResult"/></c>
        /// <c><seealso cref="HostFunctions.isNull"/></c>
        bool EnableNullResultWrapping { get; set; }

        /// <summary>
        /// Enables or disables floating point narrowing.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, no attempt is made to convert floating-point
        /// values imported from the script engine to the narrowest equivalent .NET representation.
        /// The default behavior is more likely to result in successful method binding in specific
        /// scenarios, so setting this property to <c>true</c> is not recommended.
        /// </remarks>
        bool DisableFloatNarrowing { get; set; }

        /// <summary>
        /// Enables or disables dynamic method binding.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine bypasses the default method
        /// binding algorithm and uses reflection-based method binding instead. This approach
        /// abandons support for generic type inference and other features, but it avoids engaging
        /// the dynamic infrastructure.
        /// </remarks>
        /// <c><seealso cref="UseReflectionBindFallback"/></c>
        bool DisableDynamicBinding { get; set; }

        /// <summary>
        /// Enables or disables the use of reflection-based method binding as a fallback.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this property is set to <c>true</c>, the script engine attempts to use
        /// reflection-based method binding when the default method binding algorithm fails. This
        /// approach reduces type safety, but it may be useful for running legacy scripts that rely
        /// on the specific behavior of reflection-based method binding.
        /// </para>
        /// <para>
        /// This property has no effect when <c><see cref="DisableDynamicBinding"/></c> is set to
        /// <c>true</c>.
        /// </para>
        /// </remarks>
        bool UseReflectionBindFallback { get; set; }

        /// <summary>
        /// Enables or disables automatic host variable tunneling for by-reference arguments to script functions and delegates.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the script engine replaces by-reference
        /// arguments to script functions and delegates with host variables, allowing script code
        /// to simulate output arguments if the script language does not support them natively.
        /// </remarks>
        /// <c><seealso cref="HostFunctions.newVar{T}(T)"/></c>
        bool EnableAutoHostVariables { get; set; }

        /// <summary>
        /// Gets or sets the script engine's undefined import value.
        /// </summary>
        /// <remarks>
        /// Some script languages support one or more special non-<c>null</c> values that represent
        /// nonexistent, missing, unknown, or undefined data. When such a value is marshaled to the
        /// host, the script engine maps it to the value of this property. The default value is
        /// <c><see cref="Undefined.Value">Undefined.Value</see></c>.
        /// </remarks>
        object UndefinedImportValue { get; set; }

        /// <summary>
        /// Gets or sets the script engine's null import value.
        /// </summary>
        /// <remarks>
        /// Some script languages support one or more special <c>null</c> values that represent
        /// empty or unassigned object references. When such a value is marshaled to the host, the
        /// script engine maps it to the value of this property. The default value is simply
        /// <c>null</c>.
        /// </remarks>
        object NullImportValue { get; set; }

        /// <summary>
        /// Gets or sets the script engine's null export value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When a null object reference is marshaled to script code, the script engine maps it to
        /// the value of this property. The default value is simply <c>null</c>, which corresponds
        /// to <c>null</c> or its closest equivalent in the script language. Other useful
        /// possibilities include
        /// <c><see cref="Undefined.Value">Undefined.Value</see></c> and
        /// <c><see href="https://microsoft.github.io/ClearScript/Reference/html/F_Microsoft_ClearScript_Windows_Nothing_Value.htm">Nothing.Value</see></c>.
        /// </para>
        /// <para>
        /// Note that <c><see cref="ScriptMemberFlags.WrapNullResult"/></c>,
        /// <c><see cref="EnableNullResultWrapping"/></c>, and
        /// <c><see href="https://microsoft.github.io/ClearScript/Reference/html/T_Microsoft_ClearScript_Windows_WindowsScriptEngineFlags.htm">MarshalNullAsDispatch</see></c>
        /// all take precedence over this property.
        /// </para>
        /// </remarks>
        object NullExportValue { get; set; }

        /// <summary>
        /// Gets or sets the script engine's void result export value.
        /// </summary>
        /// <remarks>
        /// Some script languages expect every subroutine call to return a value. When script code
        /// written in such a language invokes a host method that explicitly returns no value (such
        /// as a C#
        /// <c><see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/void">void</see></c>
        /// method), the script engine returns the value of this property as a dummy result. The
        /// default value is <c><see cref="VoidResult.Value">VoidResult.Value</see></c>.
        /// </remarks>
        object VoidResultValue { get; set; }

        /// <summary>
        /// Gets or sets a callback that can be used to halt script execution.
        /// </summary>
        /// <remarks>
        /// During script execution the script engine periodically invokes this callback to
        /// determine whether it should continue. If the callback returns <c>false</c>, the script
        /// engine terminates script execution and throws an exception.
        /// </remarks>
        ContinuationCallback ContinuationCallback { get; set; }

        /// <summary>
        /// Allows the host to access script resources dynamically.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It dynamically supports properties and methods that correspond to global
        /// script objects and functions.
        /// </remarks>
        dynamic Script { get; }

        /// <summary>
        /// Allows the host to access script resources.
        /// </summary>
        /// <remarks>
        /// The value of this property is an object that is bound to the script engine's root
        /// namespace. It allows you to access global script resources via the
        /// <c><see cref="ScriptObject"/></c> class interface. Doing so is likely to outperform
        /// dynamic access via <c><see cref="Script"/></c>.
        /// </remarks>
        ScriptObject Global { get; }

        /// <summary>
        /// Gets or sets the script engine's document settings.
        /// </summary>
        DocumentSettings DocumentSettings { get; set; }

        /// <summary>
        /// Gets or sets the script engine's custom attribute loader.
        /// </summary>
        /// <remarks>
        /// By default, all script engines use the
        /// <see cref="HostSettings.CustomAttributeLoader">global custom attribute loader</see>.
        /// </remarks>
        CustomAttributeLoader CustomAttributeLoader { get; set; }

        /// <summary>
        /// Allows the host to attach arbitrary data to the script engine.
        /// </summary>
        object HostData { get; set; }

        /// <summary>
        /// Exposes a host object to script code.
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="target">The object to expose.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddHostObject(string itemName, object target);

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
        ///         <c><see cref="HostFunctions.newObj{T}">HostFunctions.newObj(T)</see></c>.
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
        ///         as arguments. In addition, objects that implement <c><see cref="IList"/></c> expose
        ///         properties with numeric names that match their valid indices. This includes
        ///         one-dimensional host arrays and other collections. Multidimensional host arrays
        ///         do not expose functional indexers; you must use
        ///         <c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.array.getvalue">Array.GetValue</see></c>
        ///         and
        ///         <c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.array.setvalue">Array.SetValue</see></c>
        ///         instead.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Event</b></term>
        ///         <term><b>Property</b></term>
        ///         <description>
        ///         Events are exposed as read-only properties of type <c><see cref="EventSource{T}"/></c>.
        ///         </description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        void AddHostObject(string itemName, HostItemFlags flags, object target);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddRestrictedHostObject<T>(string itemName, T target);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddRestrictedHostObject<T>(string itemName, HostItemFlags flags, T target);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMObject(string itemName, string progID);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMObject(string itemName, string progID, string serverName);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMObject(string itemName, HostItemFlags flags, string progID);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMObject(string itemName, HostItemFlags flags, string progID, string serverName);

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code. The registered class is
        /// specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMObject(string itemName, Guid clsid);

        /// <summary>
        /// Creates a COM/ActiveX object on the specified server and exposes it to script code. The
        /// registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <param name="serverName">The name of the server on which to create the object.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMObject(string itemName, Guid clsid, string serverName);

        /// <summary>
        /// Creates a COM/ActiveX object and exposes it to script code with the specified options.
        /// The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the object.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to instantiate.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMObject(string itemName, HostItemFlags flags, Guid clsid, string serverName);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(Type type);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(HostItemFlags flags, Type type);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, Type type);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, HostItemFlags flags, Type type);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, string typeName, params Type[] typeArgs);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, HostItemFlags flags, string typeName, params Type[] typeArgs);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, string typeName, string assemblyName, params Type[] typeArgs);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostType(string itemName, HostItemFlags flags, string typeName, string assemblyName, params Type[] typeArgs);

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
        /// properties and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddHostTypes(params Type[] types);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMType(string itemName, string progID);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMType(string itemName, string progID, string serverName);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMType(string itemName, HostItemFlags flags, string progID);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </para>
        /// </remarks>
        void AddCOMType(string itemName, HostItemFlags flags, string progID, string serverName);

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code. The registered class is
        /// specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMType(string itemName, Guid clsid);

        /// <summary>
        /// Imports a COM/ActiveX type from the specified server and exposes it to script code. The
        /// registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <param name="serverName">The name of the server from which to import the type.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMType(string itemName, Guid clsid, string serverName);

        /// <summary>
        /// Imports a COM/ActiveX type and exposes it to script code with the specified options.
        /// The registered class is specified by class identifier (CLSID).
        /// </summary>
        /// <param name="itemName">A name for the new global script item that will represent the type.</param>
        /// <param name="flags">A value that selects options for the operation.</param>
        /// <param name="clsid">The class identifier (CLSID) of the registered class to import.</param>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMType(string itemName, HostItemFlags flags, Guid clsid);

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
        /// and methods, see <c><see cref="AddHostObject(string, HostItemFlags, object)"/></c>.
        /// </remarks>
        void AddCOMType(string itemName, HostItemFlags flags, Guid clsid, string serverName);

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
        void Execute(string code);

        /// <summary>
        /// Executes script code with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        void Execute(string documentName, string code);

        /// <summary>
        /// Executes script code with an associated document name, optionally discarding the document after execution.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        void Execute(string documentName, bool discard, string code);

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
        void Execute(DocumentInfo documentInfo, string code);

        /// <summary>
        /// Loads and executes a script document.
        /// </summary>
        /// <param name="specifier">A string specifying the document to be loaded and executed.</param>
        /// <remarks>
        /// In some script languages the distinction between statements and expressions is
        /// significant but ambiguous for certain syntactic elements. This method always
        /// interprets script code loaded from the specified document as a statement.
        /// </remarks>
        void ExecuteDocument(string specifier);

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
        void ExecuteDocument(string specifier, DocumentCategory category);

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
        void ExecuteDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback);

        /// <summary>
        /// Executes script code as a command.
        /// </summary>
        /// <param name="command">The script command to execute.</param>
        /// <returns>The command output.</returns>
        /// <remarks>
        /// This method is similar to <c><see cref="Evaluate(string)"/></c> but optimized for command
        /// consoles. The specified command must be limited to a single expression or statement.
        /// Script engines can override this method to customize command execution as well as the
        /// process of converting the result to a string for console output.
        /// </remarks>
        string ExecuteCommand(string command);

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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object Evaluate(string code);

        /// <summary>
        /// Evaluates script code with an associated document name.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object Evaluate(string documentName, string code);

        /// <summary>
        /// Evaluates script code with an associated document name, optionally discarding the document after execution.
        /// </summary>
        /// <param name="documentName">A document name for the script code. Currently, this name is used only as a label in presentation contexts such as debugger user interfaces.</param>
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
        ///         <term><c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.string">System.String</see></c></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Boolean</b></term>
        ///         <term><c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.boolean">System.Boolean</see></c></term>
        ///         <description>N/A</description>
        ///     </item>
        ///     <item>
        ///         <term><b>Number</b></term>
        ///         <term><c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.int32">System.Int32</see></c>&#xA0;or&#xA0;<c><see href="https://docs.microsoft.com/en-us/dotnet/api/system.double">System.Double</see></c></term>
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
        ///         <term><c><see cref="Undefined"/></c></term>
        ///         <description>
        ///         This represents JavaScript's
        ///         <c><see href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/undefined">undefined</see></c>,
        ///         VBScript's
        ///         <c><see href="https://docs.microsoft.com/en-us/previous-versions//f8tbc79x(v=vs.85)">Empty</see></c>,
        ///         etc.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><b>Void</b></term>
        ///         <term><c><see cref="VoidResult"/></c></term>
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
        ///         <term><c><see cref="ScriptObject"/></c></term>
        ///         <description>
        ///         This includes all native script objects that have no .NET representation. C#'s
        ///         <c><see href="https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#the-dynamic-type">dynamic</see></c>
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
        object Evaluate(string documentName, bool discard, string code);

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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object Evaluate(DocumentInfo documentInfo, string code);

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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object EvaluateDocument(string specifier);

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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object EvaluateDocument(string specifier, DocumentCategory category);

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
        /// <c><see cref="Evaluate(string, bool, string)"/></c>.
        /// </para>
        /// </remarks>
        object EvaluateDocument(string specifier, DocumentCategory category, DocumentContextCallback contextCallback);

        /// <summary>
        /// Invokes a global function or procedure.
        /// </summary>
        /// <param name="funcName">The name of the global function or procedure to invoke.</param>
        /// <param name="args">Optional invocation arguments.</param>
        /// <returns>The return value if a function was invoked, an undefined value otherwise.</returns>
        object Invoke(string funcName, params object[] args);

        /// <summary>
        /// Gets a string representation of the script call stack.
        /// </summary>
        /// <returns>The script call stack formatted as a string.</returns>
        /// <remarks>
        /// This method returns an empty string if the script engine is not executing script code.
        /// The stack trace text format is defined by the script engine.
        /// </remarks>
        string GetStackTrace();

        /// <summary>
        /// Interrupts script execution and causes the script engine to throw an exception.
        /// </summary>
        /// <remarks>
        /// This method can be called safely from any thread.
        /// </remarks>
        void Interrupt();

        /// <summary>
        /// Performs garbage collection.
        /// </summary>
        /// <param name="exhaustive"><c>True</c> to perform exhaustive garbage collection, <c>false</c> to favor speed over completeness.</param>
        void CollectGarbage(bool exhaustive);
    }
}
