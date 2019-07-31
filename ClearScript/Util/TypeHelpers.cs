// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.Util
{
    internal static class TypeHelpers
    {
        private static readonly string[] importBlackList =
        {
            // ReSharper disable StringLiteralTypo

            "FXAssembly",
            "ThisAssembly",
            "AssemblyRef",
            "SRETW",
            "MatchState",
            "__DynamicallyInvokableAttribute"

            // ReSharper restore StringLiteralTypo
        };

        private static readonly HashSet<Type> nullableNumericTypes = new HashSet<Type>
        {
            typeof(char?),
            typeof(sbyte?),
            typeof(byte?),
            typeof(short?),
            typeof(ushort?),
            typeof(int?),
            typeof(uint?),
            typeof(long?),
            typeof(ulong?),
            typeof(float?),
            typeof(double?),
            typeof(decimal?)
        };

        private static readonly ConcurrentDictionary<Tuple<Type, BindingFlags, Type, ScriptAccess, bool>, Invocability> invocabilityMap = new ConcurrentDictionary<Tuple<Type, BindingFlags, Type, ScriptAccess, bool>, Invocability>();

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsSpecific(this Type type)
        {
            return !type.IsGenericParameter && !type.ContainsGenericParameters;
        }

        public static bool IsCompilerGenerated(this Type type)
        {
            return type.IsDefined(typeof(CompilerGeneratedAttribute), false);
        }

        public static bool IsFlagsEnum(this Type type)
        {
            return type.IsEnum && type.IsDefined(typeof(FlagsAttribute), false);
        }

        public static bool IsImportable(this Type type)
        {
            if (!type.IsNested && !type.IsSpecialName && !type.IsCompilerGenerated())
            {
                var locator = type.GetLocator();
                return !importBlackList.Contains(locator) && IsValidLocator(locator);
            }

            return false;
        }

        public static bool IsAnonymous(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            if ((type.Attributes & TypeAttributes.VisibilityMask) != TypeAttributes.NotPublic)
            {
                return false;
            }

            var name = type.Name;

            if (!name.StartsWith("<>", StringComparison.Ordinal) && !name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!name.Contains("AnonymousType"))
            {
                return false;
            }

            if (!Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false))
            {
                return false;
            }

            return true;
        }

        public static bool IsIntegral(this Type type)
        {
            return
                (type == typeof(sbyte)) ||
                (type == typeof(byte)) ||
                (type == typeof(short)) ||
                (type == typeof(ushort)) ||
                (type == typeof(char)) ||
                (type == typeof(int)) ||
                (type == typeof(uint)) ||
                (type == typeof(long)) ||
                (type == typeof(ulong));
        }

        public static bool IsFloatingPoint(this Type type)
        {
            return
                (type == typeof(float)) ||
                (type == typeof(double));
        }

        public static bool IsNumeric(this Type type, out bool isIntegral)
        {
            isIntegral = type.IsIntegral();
            return
                isIntegral ||
                type.IsFloatingPoint() ||
                type == typeof(decimal);
        }

        public static bool IsNumeric(this Type type)
        {
            bool isIntegral;
            return type.IsNumeric(out isIntegral);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsNullableNumeric(this Type type)
        {
            return nullableNumericTypes.Contains(type);
        }

        public static bool IsUnknownCOMObject(this Type type)
        {
            return type.IsCOMObject && (type.GetInterfaces().Length < 1);
        }

        public static bool IsAssignableFrom(this Type type, ref object value)
        {
            var isByRef = false;
            if (type.IsByRef)
            {
                type = type.GetElementType();
                isByRef = true;
            }

            if (type.IsNullable())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return (value == null) || (Nullable.GetUnderlyingType(type).IsAssignableFrom(ref value));
            }

            if (value == null)
            {
                // ReSharper disable once PossibleNullReferenceException
                return !type.IsValueType;
            }

            var valueType = value.GetType();
            if (valueType == type)
            {
                return true;
            }

            if (!isByRef && type.IsImplicitlyConvertibleFrom(valueType, ref value))
            {
                return true;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!type.IsValueType)
            {
                if (type.IsAssignableFrom(valueType))
                {
                    return true;
                }

                if (type.IsInterface && type.IsImport && valueType.IsCOMObject)
                {
                    var result = false;
                    var pUnknown = Marshal.GetIUnknownForObject(value);

                    var iid = type.GUID;
                    if (iid != Guid.Empty)
                    {
                        IntPtr pInterface;
                        if (RawCOMHelpers.HResult.Succeeded(Marshal.QueryInterface(pUnknown, ref iid, out pInterface)))
                        {
                            Marshal.Release(pInterface);
                            result = true;
                        }
                    }

                    Marshal.Release(pUnknown);
                    return result;
                }

                return false;
            }

            if (!valueType.IsValueType)
            {
                return false;
            }

            if (type.IsEnum)
            {
                return Enum.GetUnderlyingType(type).IsAssignableFrom(ref value) && (value.DynamicCast<int>() == 0);
            }

            if (valueType.IsEnum)
            {
                return false;
            }

            bool typeIsIntegral;
            if (type.IsNumeric(out typeIsIntegral))
            {
                if (typeIsIntegral)
                {
                    if (!valueType.IsIntegral())
                    {
                        return false;
                    }
                }
                else if (!valueType.IsNumeric())
                {
                    return false;
                }

                value = Convert.ChangeType(value, type);
                return true;
            }

            return false;
        }

        public static bool IsAssignableToGenericType(this Type type, Type genericTypeDefinition, out Type[] typeArgs)
        {
            Debug.Assert(genericTypeDefinition.IsGenericTypeDefinition);

            for (var testType = type; testType != null; testType = testType.BaseType)
            {
                if (testType.IsGenericType && (testType.GetGenericTypeDefinition() == genericTypeDefinition))
                {
                    typeArgs = testType.GetGenericArguments();
                    return true;
                }
            }

            var matches = type.GetInterfaces().Where(testType => testType.IsGenericType && (testType.GetGenericTypeDefinition() == genericTypeDefinition)).ToArray();
            if (matches.Length == 1)
            {
                typeArgs = matches[0].GetGenericArguments();
                return true;
            }

            typeArgs = null;
            return false;
        }

        public static bool HasExtensionMethods(this Type type)
        {
            return type.IsDefined(typeof(ExtensionAttribute), false);
        }

        public static bool EqualsOrDeclares(this Type type, Type thatType)
        {
            for (; thatType != null; thatType = thatType.DeclaringType)
            {
                if (thatType == type)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFamilyOf(this Type type, Type thatType)
        {
            for (; type != null; type = type.DeclaringType)
            {
                if ((type == thatType) || type.IsSubclassOf(thatType))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFriendOf(this Type type, Type thatType)
        {
            return type.Assembly.IsFriendOf(thatType.Assembly);
        }

        public static string GetRootName(this Type type)
        {
            return StripGenericSuffix(type.Name);
        }

        public static string GetFullRootName(this Type type)
        {
            return StripGenericSuffix(type.FullName);
        }

        public static string GetFriendlyName(this Type type)
        {
            return type.GetFriendlyName(GetRootName);
        }

        public static string GetFullFriendlyName(this Type type)
        {
            return type.GetFriendlyName(GetFullRootName);
        }

        public static string GetLocator(this Type type)
        {
            Debug.Assert(!type.IsNested);
            return type.GetFullRootName();
        }

        public static int GetGenericParamCount(this Type type)
        {
            return type.GetGenericArguments().Count(typeArg => typeArg.IsGenericParameter);
        }

        public static IEnumerable<EventInfo> GetScriptableEvents(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            var events = type.GetEvents(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                events = events.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableEvents(bindFlags, accessContext, defaultAccess)));
            }

            return events.Where(eventInfo => eventInfo.IsScriptable(accessContext, defaultAccess));
        }

        public static EventInfo GetScriptableEvent(this Type type, string name, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetScriptableEvents(bindFlags, accessContext, defaultAccess).FirstOrDefault(eventInfo => eventInfo.GetScriptName() == name);
        }

        public static IEnumerable<FieldInfo> GetScriptableFields(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetFields(bindFlags).Where(field => field.IsScriptable(accessContext, defaultAccess));
        }

        public static FieldInfo GetScriptableField(this Type type, string name, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetScriptableFields(bindFlags, accessContext, defaultAccess).FirstOrDefault(field => field.GetScriptName() == name);
        }

        public static IEnumerable<MethodInfo> GetScriptableMethods(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            var methods = type.GetMethods(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                methods = methods.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableMethods(bindFlags, accessContext, defaultAccess)));
                methods = methods.Concat(typeof(object).GetScriptableMethods(bindFlags, accessContext, defaultAccess));
            }

            return methods.Where(method => method.IsScriptable(accessContext, defaultAccess));
        }

        public static IEnumerable<MethodInfo> GetScriptableMethods(this Type type, string name, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetScriptableMethods(bindFlags, accessContext, defaultAccess).Where(method => method.GetScriptName() == name);
        }

        public static IEnumerable<PropertyInfo> GetScriptableProperties(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            var properties = type.GetProperties(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                properties = properties.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableProperties(bindFlags, accessContext, defaultAccess)));
            }

            return properties.Where(property => property.IsScriptable(accessContext, defaultAccess));
        }

        public static IEnumerable<PropertyInfo> GetScriptableDefaultProperties(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            if (type.IsArray)
            {
                var property = typeof(IList<>).MakeSpecificType(type.GetElementType()).GetProperty("Item");
                return (property != null) ? property.ToEnumerable() : ArrayHelpers.GetEmptyArray<PropertyInfo>();
            }

            var properties = type.GetProperties(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                properties = properties.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableProperties(bindFlags, accessContext, defaultAccess)));
            }

            var defaultMembers = type.GetDefaultMembers();
            return properties.Where(property => property.IsScriptable(accessContext, defaultAccess) && (defaultMembers.Contains(property) || property.IsDispID(SpecialDispIDs.Default)));
        }

        public static IEnumerable<PropertyInfo> GetScriptableProperties(this Type type, string name, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetScriptableProperties(bindFlags, accessContext, defaultAccess).Where(property => property.GetScriptName() == name);
        }

        public static PropertyInfo GetScriptableProperty(this Type type, string name, BindingFlags bindFlags, object[] bindArgs, Type accessContext, ScriptAccess defaultAccess)
        {
            var candidates = type.GetScriptableProperties(name, bindFlags, accessContext, defaultAccess).Distinct(PropertySignatureComparer.Instance).ToArray();
            return SelectProperty(candidates, bindFlags, bindArgs);
        }

        public static PropertyInfo GetScriptableDefaultProperty(this Type type, BindingFlags bindFlags, object[] bindArgs, Type accessContext, ScriptAccess defaultAccess)
        {
            var candidates = type.GetScriptableDefaultProperties(bindFlags, accessContext, defaultAccess).Distinct(PropertySignatureComparer.Instance).ToArray();
            return SelectProperty(candidates, bindFlags, bindArgs);
        }

        public static IEnumerable<Type> GetScriptableNestedTypes(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess)
        {
            return type.GetNestedTypes(bindFlags).Where(nestedType => nestedType.IsScriptable(accessContext, defaultAccess));
        }

        public static Invocability GetInvocability(this Type type, BindingFlags bindFlags, Type accessContext, ScriptAccess defaultAccess, bool ignoreDynamic)
        {
            return invocabilityMap.GetOrAdd(Tuple.Create(type, bindFlags, accessContext, defaultAccess, ignoreDynamic), GetInvocabilityInternal);
        }

        public static object CreateInstance(this Type type, params object[] args)
        {
            return type.CreateInstance(BindingFlags.Public, args);
        }

        public static object CreateInstance(this Type type, BindingFlags flags, params object[] args)
        {
            return type.InvokeMember(null, BindingFlags.CreateInstance | BindingFlags.Instance | (flags & ~BindingFlags.Static), null, null, args, CultureInfo.InvariantCulture);
        }

        public static object CreateInstance(this Type type, Type accessContext, ScriptAccess defaultAccess, params object[] args)
        {
            // ReSharper disable CoVariantArrayConversion
            // ReSharper disable PossibleNullReferenceException

            if (type.IsCOMObject || (type.IsValueType && (args.Length < 1)))
            {
                return type.CreateInstance(args);
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var candidates = type.GetConstructors(flags).Where(testConstructor => testConstructor.IsAccessible(accessContext) && !testConstructor.IsBlockedFromScript(defaultAccess)).ToArray();
            if (candidates.Length < 1)
            {
                throw new MissingMethodException(MiscHelpers.FormatInvariant("Type '{0}' has no constructor that matches the specified arguments", type.GetFullFriendlyName()));
            }

            ConstructorInfo constructor = null;

            try
            {
                object state;
                constructor = Type.DefaultBinder.BindToMethod(flags, candidates, ref args, null, null, null, out state) as ConstructorInfo;
            }
            catch (MissingMethodException)
            {
            }

            if (constructor == null)
            {
                throw new MissingMethodException(MiscHelpers.FormatInvariant("Type '{0}' has no constructor that matches the specified arguments", type.GetFullFriendlyName()));
            }

            return constructor.Invoke(args);

            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore CoVariantArrayConversion
        }

        public static Type MakeSpecificType(this Type template, params Type[] typeArgs)
        {
            Debug.Assert(template.GetGenericParamCount() <= typeArgs.Length);
            return template.ApplyTypeArguments(typeArgs);
        }

        public static Type ApplyTypeArguments(this Type type, params Type[] typeArgs)
        {
            if (!type.IsSpecific())
            {
                Debug.Assert(typeArgs.All(typeArg => typeArg.IsSpecific()));

                var finalTypeArgs = (Type[])type.GetGenericArguments().Clone();
                for (int finalIndex = 0, index = 0; finalIndex < finalTypeArgs.Length; finalIndex++)
                {
                    if (finalTypeArgs[finalIndex].IsGenericParameter)
                    {
                        finalTypeArgs[finalIndex] = typeArgs[index++];
                        if (index >= typeArgs.Length)
                        {
                            break;
                        }
                    }
                }

                return type.GetGenericTypeDefinition().MakeGenericType(finalTypeArgs);
            }

            return type;
        }

        public static bool IsValidLocator(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.All(IsValidLocatorChar);
        }

        public static HostType ImportType(string typeName, string assemblyName, bool useAssemblyName, object[] hostTypeArgs)
        {
            if (!IsValidLocator(typeName))
            {
                throw new ArgumentException("Invalid type name", "typeName");
            }

            if (useAssemblyName && string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentException("Invalid assembly name", "assemblyName");
            }

            if (!hostTypeArgs.All(arg => arg is HostType))
            {
                throw new ArgumentException("Invalid generic type argument");
            }

            var typeArgs = hostTypeArgs.Cast<HostType>().Select(hostType => hostType.GetTypeArg()).ToArray();
            return ImportType(typeName, assemblyName, useAssemblyName, typeArgs);
        }

        public static HostType ImportType(string typeName, string assemblyName, bool useAssemblyName, Type[] typeArgs)
        {
            const int maxTypeArgCount = 16;

            if ((typeArgs != null) && (typeArgs.Length > 0))
            {
                var template = ImportType(typeName, assemblyName, useAssemblyName, typeArgs.Length);
                if (template == null)
                {
                    throw new TypeLoadException(MiscHelpers.FormatInvariant("Could not find a matching generic type definition for '{0}'", typeName));
                }

                return HostType.Wrap(template.MakeSpecificType(typeArgs));
            }

            var type = ImportType(typeName, assemblyName, useAssemblyName, 0);

            // ReSharper disable RedundantEnumerableCastCall

            // the OfType<Type>() call is not redundant; it filters out null elements
            var counts = Enumerable.Range(1, maxTypeArgCount);
            var templates = counts.Select(count => ImportType(typeName, assemblyName, useAssemblyName, count)).OfType<Type>().ToArray();

            // ReSharper restore RedundantEnumerableCastCall

            if (templates.Length < 1)
            {
                if (type == null)
                {
                    throw new TypeLoadException(MiscHelpers.FormatInvariant("Could not find a specific type or generic type definition for '{0}'", typeName));
                }

                return HostType.Wrap(type);
            }

            if (type == null)
            {
                return HostType.Wrap(templates);
            }

            return HostType.Wrap(type.ToEnumerable().Concat(templates).ToArray());
        }

        private static Type ImportType(string typeName, string assemblyName, bool useAssemblyName, int typeArgCount)
        {
            var assemblyQualifiedName = GetFullTypeName(typeName, assemblyName, useAssemblyName, typeArgCount);

            Type type = null;
            try
            {
                type = Type.GetType(assemblyQualifiedName);
            }
            catch (ArgumentException)
            {
            }
            catch (TypeLoadException)
            {
            }

            return ((type != null) && useAssemblyName && (type.AssemblyQualifiedName != assemblyQualifiedName)) ? null : type;
        }

        private static string GetFriendlyName(this Type type, Func<Type, string> getBaseName)
        {
            Debug.Assert(type.IsSpecific());
            if (type.IsArray)
            {
                var commas = new string(Enumerable.Repeat(',', type.GetArrayRank() - 1).ToArray());
                return MiscHelpers.FormatInvariant("{0}[{1}]", type.GetElementType().GetFriendlyName(getBaseName), commas);
            }

            var typeArgs = type.GetGenericArguments();
            var parentPrefix = string.Empty;
            if (type.IsNested)
            {
                var parentType = type.DeclaringType.MakeSpecificType(typeArgs);
                parentPrefix = parentType.GetFriendlyName(getBaseName) + ".";
                typeArgs = typeArgs.Skip(parentType.GetGenericArguments().Length).ToArray();
                getBaseName = GetRootName;
            }

            if (typeArgs.Length < 1)
            {
                return MiscHelpers.FormatInvariant("{0}{1}", parentPrefix, getBaseName(type));
            }

            var name = getBaseName(type.GetGenericTypeDefinition());
            var paramList = string.Join(",", typeArgs.Select(typeArg => typeArg.GetFriendlyName(getBaseName)));
            return MiscHelpers.FormatInvariant("{0}{1}<{2}>", parentPrefix, name, paramList);
        }

        private static string GetFullTypeName(string name, string assemblyName, bool useAssemblyName, int typeArgCount)
        {
            var fullTypeName = name;

            if (typeArgCount > 0)
            {
                fullTypeName += MiscHelpers.FormatInvariant("`{0}", typeArgCount);
            }

            if (useAssemblyName)
            {
                fullTypeName += MiscHelpers.FormatInvariant(", {0}", AssemblyTable.GetFullAssemblyName(assemblyName));
            }

            return fullTypeName;
        }

        private static Invocability GetInvocabilityInternal(Tuple<Type, BindingFlags, Type, ScriptAccess, bool> args)
        {
            if (typeof(Delegate).IsAssignableFrom(args.Item1))
            {
                return Invocability.Delegate;
            }

            if (!args.Item5 && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(args.Item1))
            {
                return Invocability.Dynamic;
            }

            if (args.Item1.GetScriptableDefaultProperties(args.Item2, args.Item3, args.Item4).Any())
            {
                return Invocability.DefaultProperty;
            }

            return Invocability.None;
        }

        private static bool IsValidLocatorChar(char ch)
        {
            return char.IsLetterOrDigit(ch) || (ch == '_') || (ch == '.');
        }

        private static string StripGenericSuffix(string name)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));
            var index = name.LastIndexOf('`');
            return (index >= 0) ? name.Substring(0, index) : name;
        }

        private static Type GetPropertyIndexType(object bindArg)
        {
            var hostTarget = bindArg as HostTarget;
            if (hostTarget != null)
            {
                return hostTarget.Type;
            }

            if (bindArg != null)
            {
                return bindArg.GetType();
            }

            throw new InvalidOperationException("Property index value must not be null");
        }

        private static PropertyInfo SelectProperty(PropertyInfo[] candidates, BindingFlags bindFlags, object[] bindArgs)
        {
            if (candidates.Length < 1)
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            var result = Type.DefaultBinder.SelectProperty(bindFlags, candidates, null, bindArgs.Select(GetPropertyIndexType).ToArray(), null);
            if (result != null)
            {
                return result;
            }

            // the default binder fails to bind to some COM properties because of by-ref parameter types
            if (candidates.Length == 1)
            {
                var parameters = candidates[0].GetIndexParameters();
                if ((bindArgs.Length == parameters.Length) || ((bindArgs.Length > 0) && (parameters.Length >= bindArgs.Length)))
                {
                    return candidates[0];
                }
            }

            return null;
        }

        private static bool IsImplicitlyConvertibleFrom(this Type type, Type sourceType, ref object value)
        {
            return IsImplicitlyConvertibleInternal(type, sourceType, type, ref value) || IsImplicitlyConvertibleInternal(sourceType, sourceType, type, ref value);
        }

        private static bool IsImplicitlyConvertibleInternal(Type definingType, Type sourceType, Type targetType, ref object value)
        {
            foreach (var converter in definingType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == "op_Implicit"))
            {
                var parameters = converter.GetParameters();
                if ((parameters.Length == 1) && parameters[0].ParameterType.IsAssignableFrom(sourceType) && targetType.IsAssignableFrom(converter.ReturnType))
                {
                    value = converter.Invoke(null, new [] { value });
                    return true;
                }
            }

            return false;
        }

        #region Nested type: PropertySignatureComparer

        private sealed class PropertySignatureComparer : IEqualityComparer<PropertyInfo>
        {
            private static readonly PropertySignatureComparer instance = new PropertySignatureComparer();

            public static PropertySignatureComparer Instance { get { return instance; } }

            #region IEqualityComparer<PropertyInfo> implementation

            public bool Equals(PropertyInfo first, PropertyInfo second)
            {
                // ReSharper disable PossibleNullReferenceException

                var firstParamTypes = first.GetIndexParameters().Select(param => param.ParameterType);
                var secondParamTypes = second.GetIndexParameters().Select(param => param.ParameterType);
                return firstParamTypes.SequenceEqual(secondParamTypes);

                // ReSharper restore PossibleNullReferenceException
            }

            public int GetHashCode(PropertyInfo property)
            {
                var hashCode = 0;

                var parameters = property.GetIndexParameters();
                foreach (var param in parameters)
                {
                    hashCode = unchecked((hashCode * 31) + param.ParameterType.GetHashCode());
                }

                return hashCode;
            }

            #endregion
        }

        #endregion
    }
}
