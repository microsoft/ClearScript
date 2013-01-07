// 
// Copyright © Microsoft Corporation. All rights reserved.
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
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    /// <summary>
    /// Provides optional script-callable utility functions.
    /// </summary>
    /// <remarks>
    /// Use <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see> to expose a
    /// <c>HostFunctions</c> instance to script code. Each instance can only be exposed in one
    /// script engine.
    /// </remarks>
    public class HostFunctions : IScriptableObject
    {
        private ScriptEngine engine;

        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <see cref="HostFunctions"/> instance.
        /// </summary>
        public HostFunctions()
        {
        }

        // ReSharper restore EmptyConstructor

        #region script-callable interface

        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Creates an empty host object.
        /// </summary>
        /// <returns>A new empty host object.</returns>
        /// <remarks>
        /// This function is provided for script languages that support "expando" functionality.
        /// It creates an object that supports dynamic property addition and removal. The host
        /// can manipulate it via the <see cref="IPropertyBag"/> interface.
        /// </remarks>
        /// <example>
        /// The following code creates an empty host object and adds several properties to it.
        /// It assumes that an instance of <see cref="HostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var item = host.newObj();
        /// item.label = "Widget";
        /// item.weight = 123.45;
        /// </code>
        /// </example>
        public PropertyBag newObj()
        {
            return new PropertyBag();
        }

        /// <summary>
        /// Creates a host object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="args">Optional constructor arguments.</param>
        /// <returns>A new host object of the specified type.</returns>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see
        /// <see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see>.
        /// </remarks>
        /// <example>
        /// The following code imports the <see cref="System.Random"/> class, creates an
        /// instance using the
        /// <see href="http://msdn.microsoft.com/en-us/library/ctssatww.aspx">Random(Int32)</see>
        /// constructor, and calls the <see cref="System.Random.NextDouble"/> method.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var RandomT = host.type("System.Random");
        /// var random = host.newObj(RandomT, 100);
        /// var value = random.NextDouble();
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public T newObj<T>(params object[] args)
        {
            return (T)typeof(T).CreateInstance(args);
        }

        /// <summary>
        /// Creates a host array.
        /// </summary>
        /// <typeparam name="T">The element type of the array to create.</typeparam>
        /// <param name="lengths">One or more integers representing the array dimension lengths.</param>
        /// <returns>A new host array.</returns>
        /// <remarks>
        /// For information about the mapping between host members and script-callable properties
        /// and methods, see
        /// <see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see>.
        /// </remarks>
        /// <example>
        /// The following code creates a 5x3 host array of strings.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var StringT = host.type("System.String");
        /// var array = host.newArr(StringT, 5, 3);
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public Array newArr<T>(params int[] lengths)
        {
            return Array.CreateInstance(typeof(T), lengths);
        }

        /// <summary>
        /// Creates a host variable of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of variable to create.</typeparam>
        /// <param name="initValue">An optional initial value for the variable.</param>
        /// <returns>A new host variable of the specified type.</returns>
        /// <remarks>
        /// A host variable is a strongly typed object that holds a value of the specified type.
        /// Host variables are useful for passing method arguments by reference. In addition to
        /// being generally interchangeable with their stored values, host variables support the
        /// following properties:
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Property</term>
        ///         <term>Access</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>value</c></term>
        ///         <term>read-write</term>
        ///         <description>The current value of the host variable.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>out</c></term>
        ///         <term>read-only</term>
        ///         <description>A reference to the host variable that can be passed as an <c><see href="http://msdn.microsoft.com/en-us/library/t3c3bfhx(VS.80).aspx">out</see></c> argument.</description>
        ///     </item>
        ///     <item>
        ///         <term><c>ref</c></term>
        ///         <term>read-only</term>
        ///         <description>A reference to the host variable that can be passed as a <c><see href="http://msdn.microsoft.com/en-us/library/14akc2c7(VS.80).aspx">ref</see></c> argument.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code demonstrates using a host variable to invoke a method with an
        /// <c>out</c> parameter.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// // import a dictionary type
        /// var StringT = host.type("System.String");
        /// var StringDictT = host.type("System.Collections.Generic.Dictionary", StringT, StringT);
        /// // create and populate a dictionary
        /// var dict = host.newObj(StringDictT);
        /// dict.Add("foo", "bar");
        /// dict.Add("baz", "qux");
        /// // look up a dictionary entry */
        /// var result = host.newVar(StringT);
        /// var found = dict.TryGetValue("baz", result.out);
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public object newVar<T>(T initValue = default(T))
        {
            return new HostVariable<T>(initValue);
        }

        /// <summary>
        /// Creates a delegate that invokes a script function.
        /// </summary>
        /// <typeparam name="T">The type of delegate to create.</typeparam>
        /// <param name="scriptFunc">The script function for which to create a delegate.</param>
        /// <returns>A new delegate that invokes the specified script function.</returns>
        /// <remarks>
        /// If the delegate signature includes parameters passed by reference, the corresponding
        /// arguments to the script function will be <see cref="newVar{T}">host variables</see>.
        /// The script function can set the value of an output argument by assigning the
        /// corresponding host variable's <c>value</c> property.
        /// </remarks>
        /// <example>
        /// The following code demonstrates delegating a callback to a script function.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// // create and populate an array of integers
        /// var EnumerableT = host.type("System.Linq.Enumerable", "System.Core");
        /// var array = EnumerableT.Range(1, 5).ToArray();
        /// // import the callback type required to call Array.ForEach
        /// var Int32T = host.type("System.Int32");
        /// var CallbackT = host.type("System.Action", Int32T);
        /// // use Array.ForEach to calculate a sum
        /// var sum = 0;
        /// var ArrayT = host.type("System.Array");
        /// ArrayT.ForEach(array, host.del(CallbackT, function (value) { sum += value; }));
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, string, object[])"/>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public T del<T>(object scriptFunc)
        {
            return DelegateFactory.CreateDelegate<T>(GetEngine(), scriptFunc);
        }

        /// <summary>
        /// Creates a delegate that invokes a script function and returns no value.
        /// </summary>
        /// <param name="argCount">The number of arguments to pass to the script function.</param>
        /// <param name="scriptFunc">The script function for which to create a delegate.</param>
        /// <returns>A new delegate that invokes the specified script function and returns no value.</returns>
        /// <remarks>
        /// This function creates a delegate that accepts <paramref name="argCount"/> arguments and
        /// returns no value. The type of all parameters is <see cref="System.Object"/>. Such a
        /// delegate is often useful in strongly typed contexts because of
        /// <see href="http://msdn.microsoft.com/en-us/library/ms173174(VS.80).aspx">contravariance</see>.
        /// </remarks>
        /// <example>
        /// The following code demonstrates delegating a callback to a script function.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// // create and populate an array of strings
        /// var StringT = host.type("System.String");
        /// var array = host.newArr(StringT, 3);
        /// array.SetValue("first", 0);
        /// array.SetValue("second", 1);
        /// array.SetValue("third", 2);
        /// // use Array.ForEach to generate console output
        /// var ArrayT = host.type("System.Array");
        /// var ConsoleT = host.type("System.Console");
        /// ArrayT.ForEach(array, host.proc(1, function (value) { ConsoleT.WriteLine(value); }));
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        /// <seealso cref="newArr{T}"/>
        public object proc(int argCount, object scriptFunc)
        {
            return DelegateFactory.CreateProc(GetEngine(), scriptFunc, argCount);
        }

        /// <summary>
        /// Creates a delegate that invokes a script function and returns a value of the specified type.
        /// </summary>
        /// <typeparam name="T">The return value type.</typeparam>
        /// <param name="argCount">The number of arguments to pass to the script function.</param>
        /// <param name="scriptFunc">The script function for which to create a delegate.</param>
        /// <returns>A new delegate that invokes the specified script function and returns a value of the specified type.</returns>
        /// <remarks>
        /// This function creates a delegate that accepts <paramref name="argCount"/> arguments and
        /// returns a value of the specified type. The type of all parameters is
        /// <see cref="System.Object"/>. Such a delegate is often useful in strongly typed contexts
        /// because of
        /// <see href="http://msdn.microsoft.com/en-us/library/ms173174(VS.80).aspx">contravariance</see>.
        /// </remarks>
        /// <example>
        /// The following code demonstrates delegating a callback to a script function.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// // create and populate an array of strings
        /// var StringT = host.type("System.String");
        /// var array = host.newArr(StringT, 3);
        /// array.SetValue("first", 0);
        /// array.SetValue("second", 1);
        /// array.SetValue("third", 2);
        /// // import LINQ extensions
        /// var EnumerableT = host.type("System.Linq.Enumerable", "System.Core");
        /// // use LINQ to create an array of modified strings
        /// var selector = host.func(StringT, 1, function (value) { return value.toUpperCase(); });
        /// array = array.Select(selector).ToArray();
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        /// <seealso cref="ExtendedHostFunctions.type(string, string, object[])"/>
        public object func<T>(int argCount, object scriptFunc)
        {
            return DelegateFactory.CreateFunc<T>(GetEngine(), scriptFunc, argCount);
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> for the specified host type. This version is invoked
        /// if the specified object can be used as a type argument.
        /// </summary>
        /// <typeparam name="T">The host type for which to get the <see cref="System.Type"/>.</typeparam>
        /// <returns>The <see cref="System.Type"/> for the specified host type.</returns>
        /// <remarks>
        /// This function is similar to the C#
        /// <c><see href="http://msdn.microsoft.com/en-us/library/58918ffs(VS.71).aspx">typeof</see></c>
        /// operator. It is overloaded with <see cref="typeOf(object)"/> and selected at runtime if
        /// <typeparamref name="T"/> can be used as a type argument.
        /// </remarks>
        /// <example>
        /// The following code retrieves the assembly-qualified name of a host type.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var StringT = host.type("System.String");
        /// var name = host.typeOf(StringT).AssemblyQualifiedName;
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public Type typeOf<T>()
        {
            return typeof(T);
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> for the specified host type. This version is invoked
        /// if the specified object cannot be used as a type argument.
        /// </summary>
        /// <param name="value">The host type for which to get the <see cref="System.Type"/>.</param>
        /// <returns>The <see cref="System.Type"/> for the specified host type.</returns>
        /// <remarks>
        /// This function is similar to the C#
        /// <c><see href="http://msdn.microsoft.com/en-us/library/58918ffs(VS.71).aspx">typeof</see></c>
        /// operator. It is overloaded with <see cref="typeOf{T}"/> and selected at runtime if
        /// <paramref name="value"/> cannot be used as a type argument. Note that this applies to
        /// some host types; examples are static types and overloaded generic type groups.
        /// </remarks>
        /// <example>
        /// The following code retrieves the assembly-qualified name of a host type.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var ConsoleT = host.type("System.Console");
        /// var name = host.typeOf(ConsoleT).AssemblyQualifiedName;
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public Type typeOf(object value)
        {
            var hostType = value as HostType;
            if (hostType == null)
            {
                throw new ArgumentException("Invalid host type", "value");
            }

            if (hostType.Types.Length > 1)
            {
                throw new ArgumentException(MiscHelpers.FormatInvariant("'{0}' does not identify a unique host type", hostType.Types[0].GetLocator()), "value");
            }

            return hostType.Types[0];
        }

        /// <summary>
        /// Determines whether an object is compatible with the specified host type.
        /// </summary>
        /// <typeparam name="T">The host type with which to test <paramref name="value"/> for compatibility.</typeparam>
        /// <param name="value">The object to test for compatibility with the specified host type.</param>
        /// <returns><c>True</c> if <paramref name="value"/> is compatible with the specified type, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This function is similar to the C#
        /// <c><see href="http://msdn.microsoft.com/en-us/library/scekt9xw(VS.71).aspx">is</see></c>
        /// operator.
        /// </remarks>
        /// <example>
        /// The following code defines a function that determines whether an object implements
        /// <see cref="System.IComparable"/>.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// function isComparable(value)
        /// {
        ///     var IComparableT = host.type("System.IComparable");
        ///     return host.isType(IComparableT, value);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public bool isType<T>(object value)
        {
            return value is T;
        }

        /// <summary>
        /// Casts an object to the specified host type, returning <c>null</c> if the cast fails.
        /// </summary>
        /// <typeparam name="T">The host type to which to cast <paramref name="value"/>.</typeparam>
        /// <param name="value">The object to cast to the specified host type.</param>
        /// <returns>The result of the cast if successful, <c>null</c> otherwise.</returns>
        /// <remarks>
        /// This function is similar to the C#
        /// <c><see href="http://msdn.microsoft.com/en-us/library/cscsdfbt(VS.71).aspx">as</see></c>
        /// operator.
        /// </remarks>
        /// <example>
        /// The following code defines a function that disposes an object if it implements
        /// <see cref="System.IDisposable"/>.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// function dispose(value)
        /// {
        ///     var IDisposableT = host.type("System.IDisposable");
        ///     var disposable = host.asType(IDisposableT, value);
        ///     if (disposable) {
        ///         disposable.Dispose();
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public object asType<T>(object value) where T : class
        {
            return HostItem.Wrap(GetEngine(), value as T, typeof(T));
        }

        /// <summary>
        /// Casts an object to the specified host type.
        /// </summary>
        /// <typeparam name="T">The host type to which to cast <paramref name="value"/>.</typeparam>
        /// <param name="value">The object to cast to the specified host type.</param>
        /// <returns>The result of the cast.</returns>
        /// <remarks>
        /// If the cast fails, this function throws an exception.
        /// </remarks>
        /// <example>
        /// The following code casts a floating-point value to a 32-bit integer.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var Int32T = host.type("System.Int32");
        /// var intValue = host.cast(Int32T, 12.5);
        /// </code>
        /// </example>
        /// <seealso cref="ExtendedHostFunctions.type(string, object[])"/>
        public object cast<T>(object value)
        {
            return HostItem.Wrap(GetEngine(), value.DynamicCast<T>(), typeof(T));
        }

        /// <summary>
        /// Determines whether an object is a host type. This version is invoked if the specified
        /// object cannot be used as a type argument.
        /// </summary>
        /// <param name="value">The object to test.</param>
        /// <returns><c>True</c> if <paramref name="value"/> is a host type, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This function is overloaded with <see cref="isTypeObj{T}"/> and selected at runtime if
        /// <paramref name="value"/> cannot be used as a type argument. Note that this applies to
        /// some host types; examples are static types and overloaded generic type groups.
        /// </remarks>
        public bool isTypeObj(object value)
        {
            return value is HostType;
        }

        // ReSharper disable UnusedTypeParameter

        /// <summary>
        /// Determines whether an object is a host type. This version is invoked if the specified
        /// object can be used as a type argument.
        /// </summary>
        /// <typeparam name="T">The host type (ignored).</typeparam>
        /// <returns><c>True</c>.</returns>
        /// <remarks>
        /// This function is overloaded with <see cref="isTypeObj(object)"/> and selected at
        /// runtime if <typeparamref name="T"/> can be used as a type argument. Because type
        /// arguments are always host types, this method ignores its type argument and always
        /// returns <c>true</c>.
        /// </remarks>
        public bool isTypeObj<T>()
        {
            return true;
        }

        // ReSharper restore UnusedTypeParameter

        // ReSharper restore InconsistentNaming

        #endregion

        internal ScriptEngine GetEngine()
        {
            if (engine == null)
            {
                throw new InvalidOperationException("Operation requires a script engine");
            }

            return engine;
        }

        #region IScriptableObject implementation

        // ReSharper disable ParameterHidesMember

        void IScriptableObject.OnExposedToScriptCode(ScriptEngine engine)
        {
            MiscHelpers.VerifyNonNullArgument(engine, "engine");

            if (this.engine == null)
            {
                this.engine = engine;
                return;
            }

            if (engine != this.engine)
            {
                throw new ArgumentException("Invalid script engine", "engine");
            }
        }

        // ReSharper restore ParameterHidesMember

        #endregion
    }

    /// <summary>
    /// Provides optional script-callable utility functions. This extended version allows script
    /// code to import host types.
    /// </summary>
    public class ExtendedHostFunctions : HostFunctions
    {
        // ReSharper disable EmptyConstructor

        /// <summary>
        /// Initializes a new <see cref="ExtendedHostFunctions"/> instance.
        /// </summary>
        public ExtendedHostFunctions()
        {
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
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="name"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code imports the
        /// <see href="http://msdn.microsoft.com/en-us/library/xfhwa508.aspx">Dictionary</see>
        /// generic type and uses it to create a string dictionary.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
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
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="name"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following code imports <see cref="System.Linq.Enumerable"/> and uses it to create
        /// an array of strings.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
        /// <code lang="JavaScript">
        /// var EnumerableT = host.type("System.Linq.Enumerable", "System.Core");
        /// var Int32T = host.type("System.Int32");
        /// var StringT = host.type("System.String");
        /// var SelectorT = host.type("System.Func", Int32T, StringT);
        /// var selector = host.del(SelectorT, function (num) { return StringT.Format("The number is {0}.", num); });
        /// var array = EnumerableT.Range(0, 5).Select(selector).ToArray();
        /// </code>
        /// </example>
        /// <seealso cref="type(string, object[])"/>
        public object type(string name, string assemblyName, params object[] hostTypeArgs)
        {
            return TypeHelpers.ImportType(name, assemblyName, true, hostTypeArgs);
        }

        /// <summary>
        /// Imports the host type for the specified <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> that specifies the host type to import.</param>
        /// <returns>The imported host type.</returns>
        /// <remarks>
        /// Host types are imported in the form of objects whose properties and methods are bound
        /// to the host type's static members and nested types. If <paramref name="type"/> refers
        /// to a generic type, the corresponding object will be invocable with type arguments to
        /// yield a specific type.
        /// <para>
        /// For more information about the mapping between host members and script-callable
        /// properties and methods, see
        /// <see cref="ScriptEngine.AddHostObject(string, HostItemFlags, object)">AddHostObject</see>.
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
        /// <see cref="System.Linq.Enumerable"/> to create an array of integers.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
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
        /// <see cref="System.Linq.Enumerable"/> to create an array of integers.
        /// It assumes that an instance of <see cref="ExtendedHostFunctions"/> is exposed under
        /// the name "host"
        /// (see <see cref="ScriptEngine.AddHostObject(string, object)">AddHostObject</see>).
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

        // ReSharper restore InconsistentNaming

        #endregion
    }
}
