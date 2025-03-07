// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.Util
{
    internal static partial class TypeHelpers
    {
        private static readonly string[] importDenyList =
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

        private static readonly ConcurrentDictionary<Tuple<Type, BindingFlags, Type, ScriptAccess, bool>, Invocability> invocabilityMap = new();

        private static readonly NumericTypes[] numericConversions =
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions

            // IMPORTANT: maintain NumericType order

            /* None */      NumericTypes.None,
            /* Char */      NumericTypes.UInt16 | NumericTypes.Int32 | NumericTypes.UInt32 | NumericTypes.Int64 | NumericTypes.UInt64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr | NumericTypes.UIntPtr,
            /* SByte */     NumericTypes.Int16 | NumericTypes.Int32 | NumericTypes.Int64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr,
            /* Byte */      NumericTypes.Int16 | NumericTypes.UInt16 | NumericTypes.Int32 | NumericTypes.UInt32 | NumericTypes.Int64 | NumericTypes.UInt64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr | NumericTypes.UIntPtr,
            /* Int16 */     NumericTypes.Int32 | NumericTypes.Int64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr,
            /* UInt16 */    NumericTypes.Int32 | NumericTypes.UInt32 | NumericTypes.Int64 | NumericTypes.UInt64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr | NumericTypes.UIntPtr,
            /* Int32 */     NumericTypes.Int64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.IntPtr,
            /* UInt32 */    NumericTypes.Int64 | NumericTypes.UInt64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal | NumericTypes.UIntPtr,
            /* Int64 */     NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal,
            /* UInt64 */    NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal,
            /* IntPtr */    NumericTypes.Int64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal,
            /* UIntPtr */   NumericTypes.UInt64 | NumericTypes.Single | NumericTypes.Double | NumericTypes.Decimal,
            /* Single */    NumericTypes.Double,
            /* Double */    NumericTypes.None,
            /* Decimal */   NumericTypes.None
        };

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsSpecific(this Type type)
        {
            return !type.IsGenericParameter && !type.ContainsGenericParameters;
        }

        public static bool IsCompilerGenerated(this Type type, IHostContext context)
        {
            return type.HasCustomAttributes<CompilerGeneratedAttribute>(context, false);
        }

        public static bool IsFlagsEnum(this Type type, IHostContext context)
        {
            return type.IsEnum && type.HasCustomAttributes<FlagsAttribute>(context, false);
        }

        public static bool IsImportable(this Type type, IHostContext context)
        {
            if (!type.IsNested && !type.IsSpecialName && !type.IsCompilerGenerated(context))
            {
                var locator = type.GetLocator();
                return !importDenyList.Contains(locator) && IsValidLocator(locator);
            }

            return false;
        }

        public static bool IsAnonymous(this Type type, IHostContext context)
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

            if (!type.IsCompilerGenerated(context))
            {
                return false;
            }

            return true;
        }

        public static bool IsIntegral(this Type type)
        {
            return
                (type == typeof(char)) ||
                (type == typeof(sbyte)) ||
                (type == typeof(byte)) ||
                (type == typeof(short)) ||
                (type == typeof(ushort)) ||
                (type == typeof(int)) ||
                (type == typeof(uint)) ||
                (type == typeof(long)) ||
                (type == typeof(ulong)) ||
                (type == typeof(IntPtr)) ||
                (type == typeof(UIntPtr));
        }

        public static bool IsFloatingPoint(this Type type)
        {
            return
                (type == typeof(float)) ||
                (type == typeof(double)) ||
                (type == typeof(decimal));
        }

        public static bool IsNumeric(this Type type, out bool isIntegral)
        {
            return (isIntegral = type.IsIntegral()) || type.IsFloatingPoint();
        }

        public static bool IsNumeric(this Type type)
        {
            return type.IsNumeric(out _);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsNullableNumeric(this Type type)
        {
            return
                (type == typeof(char?)) ||
                (type == typeof(sbyte?)) ||
                (type == typeof(byte?)) ||
                (type == typeof(short?)) ||
                (type == typeof(ushort?)) ||
                (type == typeof(int?)) ||
                (type == typeof(uint?)) ||
                (type == typeof(long?)) ||
                (type == typeof(ulong?)) ||
                (type == typeof(IntPtr?)) ||
                (type == typeof(UIntPtr?)) ||
                (type == typeof(float?)) ||
                (type == typeof(double?)) ||
                (type == typeof(decimal?));
        }

        public static bool IsUnknownCOMObject(this Type type)
        {
            return type.IsCOMObject && (type.GetInterfaces().Length < 1);
        }

        public static bool IsAssignableFromValue(this Type type, ref object value)
        {
            return type.IsAssignableFromValueInternal(ref value, null, null);
        }

        public static bool IsAssignableToGenericType(this Type type, Type genericTypeDefinition, out Type[] typeArgs)
        {
            Debug.Assert(genericTypeDefinition.IsGenericTypeDefinition);

            for (var testType = type; testType is not null; testType = testType.BaseType)
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

        public static bool IsImplicitlyConvertibleFromValue(this Type type, Type sourceType, ref object value)
        {
            return IsImplicitlyConvertibleFromValueInternal(type, sourceType, type, ref value) || IsImplicitlyConvertibleFromValueInternal(sourceType, sourceType, type, ref value);
        }

        public static bool IsNumericallyConvertibleFrom(this Type type, Type valueType)
        {
            return numericConversions[(int)valueType.GetNumericType()].HasAllFlags(GetNumericTypes(type.GetNumericType()));
        }

        public static bool HasExtensionMethods(this Type type, IHostContext context)
        {
            return type.HasCustomAttributes<ExtensionAttribute>(context, false);
        }

        public static bool EqualsOrDeclares(this Type type, Type thatType)
        {
            for (; thatType is not null; thatType = thatType.DeclaringType)
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
            for (; type is not null; type = type.DeclaringType)
            {
                if ((type == thatType) || type.IsSubclassOf(thatType))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFriendOf(this Type type, IHostContext context, Type thatType)
        {
            return type.Assembly.IsFriendOf(context, thatType.Assembly);
        }

        public static bool IsCOMVisible(this Type type, IHostContext context)
        {
            var attribute = type.GetOrLoadCustomAttribute<ComVisibleAttribute>(context, false);
            if (attribute is not null)
            {
                return attribute.Value;
            }

            attribute = type.Assembly.GetOrLoadCustomAttribute<ComVisibleAttribute>(context, false);
            if (attribute is not null)
            {
                return attribute.Value;
            }

            return false;
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
            if (type.IsUnknownCOMObject())
            {
                var clsid = type.GUID;
                if (HResult.Succeeded(NativeMethods.ProgIDFromCLSID(ref clsid, out var progID)))
                {
                    return progID;
                }
            }

            return type.GetFriendlyName(GetRootName);
        }

        public static string GetFullFriendlyName(this Type type)
        {
            if (type.IsUnknownCOMObject())
            {
                var clsid = type.GUID;
                if (HResult.Succeeded(NativeMethods.ProgIDFromCLSID(ref clsid, out var progID)))
                {
                    return progID;
                }
            }

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

        public static IEnumerable<EventInfo> GetScriptableEvents(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            var events = type.GetEvents(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                events = events.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableEvents(context, bindFlags)));
            }

            return events.Where(eventInfo => eventInfo.IsScriptable(context));
        }

        public static EventInfo GetScriptableEvent(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            try
            {
                var eventInfo = type.GetScriptableEventInternal(context, name, bindFlags);
                if (eventInfo is not null)
                {
                    return eventInfo;
                }
            }
            catch (AmbiguousMatchException)
            {
            }

            return type.GetScriptableEvents(context, bindFlags).FirstOrDefault(eventInfo => string.Equals(eventInfo.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
        }

        public static IEnumerable<FieldInfo> GetScriptableFields(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            return type.GetFields(bindFlags).Where(field => field.IsScriptable(context));
        }

        public static FieldInfo GetScriptableField(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            var candidate = type.GetField(name, bindFlags);
            if ((candidate is not null) && candidate.IsScriptable(context) && string.Equals(candidate.GetScriptName(context), name, bindFlags.GetMemberNameComparison()))
            {
                return candidate;
            }

            return type.GetScriptableFields(context, bindFlags).FirstOrDefault(field => string.Equals(field.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
        }

        public static IEnumerable<MethodInfo> GetScriptableMethods(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            var methods = type.GetMethods(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                methods = methods.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableMethods(context, bindFlags)));
                methods = methods.Concat(typeof(object).GetScriptableMethods(context, bindFlags));
            }

            return methods.Where(method => method.IsScriptable(context));
        }

        public static IEnumerable<MethodInfo> GetScriptableMethods(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            return type.GetScriptableMethods(context, bindFlags).Where(method => string.Equals(method.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
        }

        public static IEnumerable<PropertyInfo> GetScriptableProperties(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            var properties = type.GetProperties(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                properties = properties.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableProperties(context, bindFlags)));
            }

            return properties.Where(property => property.IsScriptable(context));
        }

        public static IEnumerable<PropertyInfo> GetScriptableDefaultProperties(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            if (type.IsArray)
            {
                var property = typeof(IList<>).MakeSpecificType(type.GetElementType()).GetProperty("Item");
                return (property is not null) ? property.ToEnumerable() : ArrayHelpers.GetEmptyArray<PropertyInfo>();
            }

            var properties = type.GetProperties(bindFlags).AsEnumerable();
            if (type.IsInterface)
            {
                properties = properties.Concat(type.GetInterfaces().SelectMany(interfaceType => interfaceType.GetScriptableProperties(context, bindFlags)));
            }

            var defaultMembers = type.GetDefaultMembers();
            return properties.Where(property => property.IsScriptable(context) && (defaultMembers.Contains(property) || property.IsDispID(context, SpecialDispIDs.Default)));
        }

        public static IEnumerable<PropertyInfo> GetScriptableProperties(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            return type.GetScriptableProperties(context, bindFlags).Where(property => string.Equals(property.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
        }

        public static PropertyInfo GetScriptableProperty(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            var candidates = type.GetProperty(name, bindFlags)?.ToEnumerable() ?? Enumerable.Empty<PropertyInfo>();
            if (type.IsInterface)
            {
                candidates = candidates.Concat(type.GetInterfaces().Select(interfaceType => interfaceType.GetScriptableProperty(context, name, bindFlags)));
            }

            try
            {
                // ReSharper disable once RedundantEnumerableCastCall
                return candidates.OfType<PropertyInfo>().SingleOrDefault(property => (property.GetIndexParameters().Length < 1) && property.IsScriptable(context) && string.Equals(property.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
            }
            catch (InvalidOperationException exception)
            {
                throw new AmbiguousMatchException($"Multiple matches found for property name '{name}'", exception);
            }
        }

        public static PropertyInfo GetScriptableProperty(this Type type, IHostContext context, string name, BindingFlags bindFlags, object[] args, object[] bindArgs)
        {
            if (bindArgs.Length < 1)
            {
                try
                {
                    var property = type.GetScriptableProperty(context, name, bindFlags);
                    if (property is not null)
                    {
                        return property;
                    }
                }
                catch (AmbiguousMatchException)
                {
                }
            }

            var candidates = type.GetScriptableProperties(context, name, bindFlags).Distinct(PropertySignatureComparer.Instance).ToArray();
            return BindToMember(context, candidates, bindFlags, args, bindArgs);
        }

        public static PropertyInfo GetScriptableDefaultProperty(this Type type, IHostContext context, BindingFlags bindFlags, object[] args, object[] bindArgs)
        {
            var candidates = type.GetScriptableDefaultProperties(context, bindFlags).Distinct(PropertySignatureComparer.Instance).ToArray();
            return BindToMember(context, candidates, bindFlags, args, bindArgs);
        }

        public static IEnumerable<Type> GetScriptableNestedTypes(this Type type, IHostContext context, BindingFlags bindFlags)
        {
            return type.GetNestedTypes(bindFlags).Where(nestedType => nestedType.IsScriptable(context));
        }

        public static Invocability GetInvocability(this Type type, IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            return invocabilityMap.GetOrAdd(Tuple.Create(type, bindFlags, context.AccessContext, context.DefaultAccess, ignoreDynamic), _ => GetInvocabilityInternal(type, context, bindFlags, ignoreDynamic));
        }

        public static object CreateInstance(this Type type, params object[] args)
        {
            return type.CreateInstance(BindingFlags.Public, args);
        }

        public static object CreateInstance(this Type type, BindingFlags flags, params object[] args)
        {
            return type.InvokeMember(string.Empty, BindingFlags.CreateInstance | BindingFlags.Instance | (flags & ~BindingFlags.Static), null, null, args, CultureInfo.InvariantCulture);
        }

        public static object CreateInstance(this Type type, IHostContext context, HostTarget target, object[] args, object[] bindArgs)
        {
            if (type.IsCOMObject)
            {
                return type.CreateInstance(args);
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var signature = new BindSignature(context.AccessContext, flags, target, type.TypeHandle.Value.ToString(), ArrayHelpers.GetEmptyArray<Type>(), bindArgs);
            if (context.Engine.TryGetCachedConstructorBindResult(signature, out var boundConstructor))
            {
                return InvokeHelpers.InvokeConstructor(context, boundConstructor, args);
            }

            var constructors = type.GetConstructors(flags);
            if (type.IsValueType && (args.Length < 1) && !constructors.Any(testConstructor => testConstructor.GetParameters().Length < 1))
            {
                return type.CreateInstance();
            }

            var candidates = constructors.Where(testConstructor => testConstructor.IsAccessible(context) && !testConstructor.IsBlockedFromScript(context, context.DefaultAccess)).ToArray();
            if (candidates.Length < 1)
            {
                throw new MissingMethodException(MiscHelpers.FormatInvariant("Type '{0}' has no constructor that matches the specified arguments", type.GetFullFriendlyName()));
            }

            var constructor = BindToMember(context, candidates, flags, args, bindArgs);
            if (constructor is null)
            {
                throw new MissingMethodException(MiscHelpers.FormatInvariant("Type '{0}' has no constructor that matches the specified arguments", type.GetFullFriendlyName()));
            }

            context.Engine.CacheConstructorBindResult(signature, constructor);
            return InvokeHelpers.InvokeConstructor(context, constructor, args);
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

        public static T BindToMember<T>(IHostContext context, T[] candidates, BindingFlags bindFlags, object[] args, object[] bindArgs) where T : MethodBase
        {
            T result = null;

            if (candidates.Length > 0)
            {
                var bindCandidates = GetBindCandidates(context, candidates, args, bindArgs.Select(GetBindArgType).ToArray()).ToArray();
                result = SelectBindCandidate(bindCandidates);
            }

            return result;
        }

        public static PropertyInfo BindToMember(IHostContext context, PropertyInfo[] candidates, BindingFlags bindFlags, object[] args, object[] bindArgs)
        {
            PropertyInfo result = null;

            if (candidates.Length > 0)
            {
                var bindCandidates = GetBindCandidates(context, candidates, args, bindArgs.Select(GetBindArgType).ToArray()).ToArray();
                result = SelectBindCandidate(bindCandidates);

                if (result is null)
                {
                    // the default binder fails to bind to some COM properties because of by-ref parameter types
                    if (candidates.Length == 1)
                    {
                        var parameters = candidates[0].GetIndexParameters();
                        if ((bindArgs.Length == parameters.Length) || ((bindArgs.Length > 0) && (parameters.Length >= bindArgs.Length)))
                        {
                            result = candidates[0];
                        }
                    }
                }
            }

            return result;
        }

        public static HostType ImportType(string typeName, string assemblyName, bool useAssemblyName, object[] hostTypeArgs)
        {
            if (!IsValidLocator(typeName))
            {
                throw new ArgumentException("Invalid type name", nameof(typeName));
            }

            if (useAssemblyName && string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ArgumentException("Invalid assembly name", nameof(assemblyName));
            }

            if (!hostTypeArgs.All(arg => arg is HostType))
            {
                throw new ArgumentException("Invalid generic type argument", nameof(hostTypeArgs));
            }

            var typeArgs = hostTypeArgs.Cast<HostType>().Select(hostType => hostType.GetTypeArg()).ToArray();
            return ImportType(typeName, assemblyName, useAssemblyName, typeArgs);
        }

        public static HostType ImportType(string typeName, string assemblyName, bool useAssemblyName, Type[] typeArgs)
        {
            const int maxTypeArgCount = 16;

            if ((typeArgs is not null) && (typeArgs.Length > 0))
            {
                var template = ImportType(typeName, assemblyName, useAssemblyName, typeArgs.Length);
                if (template is null)
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
                if (type is null)
                {
                    throw new TypeLoadException(MiscHelpers.FormatInvariant("Could not find a specific type or generic type definition for '{0}'", typeName));
                }

                return HostType.Wrap(type);
            }

            if (type is null)
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
            catch (FileLoadException)
            {
            }

            return ((type is not null) && useAssemblyName && (type.AssemblyQualifiedName != assemblyQualifiedName)) ? null : type;
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

        private static EventInfo GetScriptableEventInternal(this Type type, IHostContext context, string name, BindingFlags bindFlags)
        {
            var candidates = type.GetEvent(name, bindFlags)?.ToEnumerable() ?? Enumerable.Empty<EventInfo>();
            if (type.IsInterface)
            {
                candidates = candidates.Concat(type.GetInterfaces().Select(interfaceType => interfaceType.GetScriptableEventInternal(context, name, bindFlags)));
            }

            try
            {
                // ReSharper disable once RedundantEnumerableCastCall
                return candidates.OfType<EventInfo>().SingleOrDefault(eventInfo => eventInfo.IsScriptable(context) && string.Equals(eventInfo.GetScriptName(context), name, bindFlags.GetMemberNameComparison()));
            }
            catch (InvalidOperationException exception)
            {
                throw new AmbiguousMatchException($"Multiple matches found for event name '{name}'", exception);
            }
        }

        private static Invocability GetInvocabilityInternal(Type type, IHostContext context, BindingFlags bindFlags, bool ignoreDynamic)
        {
            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return Invocability.Delegate;
            }

            if (!ignoreDynamic && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                return Invocability.Dynamic;
            }

            if (type.GetScriptableDefaultProperties(context, bindFlags).Any())
            {
                return Invocability.DefaultProperty;
            }

            return Invocability.None;
        }

        private static bool IsValidLocator(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.All(IsValidLocatorChar);
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

        private static bool IsBindableFromArg(this Type type, object value, Type valueType, out BindArgCost cost)
        {
            cost = new BindArgCost();
            return type.IsAssignableFromValueInternal(ref value, valueType, cost);
        }

        private static bool IsAssignableFromValueInternal(this Type type, ref object value, Type valueType, BindArgCost cost)
        {
            var typeIsByRef = type.IsByRef;
            if (typeIsByRef)
            {
                type = type.GetElementType();
            }

            var valueIsByRef = (valueType is not null) && valueType.IsByRef;
            if (valueIsByRef)
            {
                valueType = valueType.GetElementType();
            }

            if ((cost is not null) && (typeIsByRef != valueIsByRef))
            {
                cost.Flags |= BindArgFlags.ByRefMismatch;
            }

            if ((value is null) && (valueType is null))
            {
                if (type.IsNullable())
                {
                    if (cost is not null)
                    {
                        cost.NumericType = Nullable.GetUnderlyingType(type).GetNumericType();
                    }

                    return true;
                }

                return !type.IsValueType;
            }

            if (valueType is null)
            {
                valueType = value.GetType();
            }

            if (valueType == type)
            {
                return true;
            }

            if (type.IsAssignableFrom(valueType))
            {
                if (cost is not null)
                {
                    if (type.IsNullable())
                    {
                        cost.Flags |= BindArgFlags.NullableTransition;
                        cost.NumericType = Nullable.GetUnderlyingType(type).GetNumericType();
                    }
                    else if (TypeNode.TryGetUpcastCount(valueType, type, out var count))
                    {
                        cost.UpcastCount = count;
                    }
                }

                return true;
            }

            if (type.IsImplicitlyConvertibleFromValue(valueType, ref value))
            {
                if (cost is not null)
                {
                    cost.Flags |= BindArgFlags.ImplicitConversion;
                }

                return true;
            }

            if (value is null)
            {
                return false;
            }

            if (type.IsNullable())
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType.IsAssignableFromValueInternal(ref value, valueType, cost))
                {
                    if (cost is not null)
                    {
                        cost.Flags |= BindArgFlags.NullableTransition;
                        cost.NumericType = underlyingType.GetNumericType();
                    }

                    return true;
                }

                return false;
            }

            if (!type.IsValueType)
            {
                if (type.IsInterface && type.IsImport && valueType.IsCOMObject)
                {
                    var result = false;
                    var pUnknown = Marshal.GetIUnknownForObject(value);

                    var iid = type.GUID;
                    if (iid != Guid.Empty)
                    {
                        if (HResult.Succeeded(Marshal.QueryInterface(pUnknown, ref iid, out var pInterface)))
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
                return Enum.GetUnderlyingType(type).IsAssignableFromValueInternal(ref value, valueType, cost) && value.IsZero();
            }

            if (valueType.IsEnum)
            {
                return false;
            }

            if (type.IsNumeric(out var typeIsIntegral))
            {
                if (typeIsIntegral)
                {
                    if (!valueType.IsIntegral() && !value.IsWholeNumber())
                    {
                        return false;
                    }
                }
                else
                {
                    if (!valueType.IsNumeric(out var valueTypeIsIntegral))
                    {
                        return false;
                    }

                    // special case for method binding only

                    if ((cost is not null) && !valueTypeIsIntegral && !type.IsNumericallyConvertibleFrom(valueType))
                    {
                        return false;
                    }
                }

                var tempValue = value;
                if (MiscHelpers.Try(out var tempResult, static ctx => Convert.ChangeType(ctx.tempValue, ctx.type), (tempValue, type)))
                {
                    if (cost is not null)
                    {
                        cost.Flags |= BindArgFlags.NumericConversion;
                        cost.NumericType = type.GetNumericType();
                    }

                    value = tempResult;
                    return true;
                }
            }

            return false;
        }

        private static bool IsImplicitlyConvertibleFromValueInternal(Type definingType, Type sourceType, Type targetType, ref object value)
        {
            foreach (var converter in definingType.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == "op_Implicit"))
            {
                var parameters = converter.GetParameters();
                if ((parameters.Length == 1) && parameters[0].ParameterType.IsAssignableFrom(sourceType) && targetType.IsAssignableFrom(converter.ReturnType))
                {
                    var args = new[] { value };
                    if (MiscHelpers.Try(out var result, static ctx => ctx.converter.Invoke(null, ctx.args), (converter, args)))
                    {
                        value = result;
                        return true;
                    }
                }
            }

            return false;
        }

        private static NumericType GetNumericType(this Type type)
        {
            if (type == typeof(char)) return NumericType.Char;
            if (type == typeof(sbyte)) return NumericType.SByte;
            if (type == typeof(byte)) return NumericType.Byte;
            if (type == typeof(short)) return NumericType.Int16;
            if (type == typeof(ushort)) return NumericType.UInt16;
            if (type == typeof(int)) return NumericType.Int32;
            if (type == typeof(uint)) return NumericType.UInt32;
            if (type == typeof(long)) return NumericType.Int64;
            if (type == typeof(ulong)) return NumericType.UInt64;
            if (type == typeof(IntPtr)) return NumericType.IntPtr;
            if (type == typeof(UIntPtr)) return NumericType.UIntPtr;
            if (type == typeof(float)) return NumericType.Single;
            if (type == typeof(double)) return NumericType.Double;
            if (type == typeof(decimal)) return NumericType.Decimal;
            return NumericType.None;
        }

        private static NumericTypes GetNumericTypes(NumericType numericType)
        {
            return (numericType == NumericType.None) ? NumericTypes.None : (NumericTypes)(1 << (int)numericType);
        }

        private static Type GetBindArgType(object bindArg)
        {
            if (bindArg is HostTarget hostTarget)
            {
                return hostTarget.Type;
            }

            if (bindArg is not null)
            {
                return bindArg.GetType();
            }

            return null;
        }

        private static IEnumerable<BindCandidate<T>> GetBindCandidates<T>(IHostContext context, T[] candidates, object[] args, Type[] argTypes) where T : MethodBase
        {
            return GetBindCandidates(context, candidates, candidate => candidate.GetParameters(), args, argTypes);
        }

        private static IEnumerable<BindCandidate<PropertyInfo>> GetBindCandidates(IHostContext context, PropertyInfo[] candidates, object[] args, Type[] argTypes)
        {
            return GetBindCandidates(context, candidates, candidate => candidate.GetIndexParameters(), args, argTypes);
        }

        private static IEnumerable<BindCandidate<T>> GetBindCandidates<T>(IHostContext context, T[] candidates, Func<T, ParameterInfo[]> getParameters, object[] args, Type[] argTypes) where T : MemberInfo
        {
            foreach (var candidate in candidates)
            {
                if (MiscHelpers.Try(out var bindCandidate, static ctx => BindCandidate<T>.TryCreateInstance(ctx.context, ctx.candidate, ctx.getParameters(ctx.candidate), ctx.args, ctx.argTypes), (context, candidate, getParameters, args, argTypes)) && (bindCandidate is not null))
                {
                    yield return bindCandidate;
                }
            }
        }

        private static T SelectBindCandidate<T>(BindCandidate<T>[] bindCandidates) where T : MemberInfo
        {
            if (bindCandidates.Length < 1)
            {
                return null;
            }

            if (bindCandidates.Length < 2)
            {
                return bindCandidates[0].Candidate;
            }

            Array.Sort(bindCandidates, Comparer<BindCandidate<T>>.Default);

            if (bindCandidates[0].CompareTo(bindCandidates[1]) == 0)
            {
                throw new AmbiguousMatchException("Ambiguous match found for the specified arguments");
            }

            return bindCandidates[0].Candidate;
        }

        #region Nested type: NumericType

        private enum NumericType
        {
            // IMPORTANT: maintain order and numeric mapping

            None,
            Char,
            SByte,
            Byte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            IntPtr,
            UIntPtr,
            Single,
            Double,
            Decimal
        }

        #endregion

        #region Nested type: NumericTypes

        [Flags]
        public enum NumericTypes
        {
            // ReSharper disable UnusedMember.Local

            None = 0,
            Char = 1 << NumericType.Char,
            SByte = 1 << NumericType.SByte,
            Byte = 1 << NumericType.Byte,
            Int16 = 1 << NumericType.Int16,
            UInt16 = 1 << NumericType.UInt16,
            Int32 = 1 << NumericType.Int32,
            UInt32 = 1 << NumericType.UInt32,
            Int64 = 1 << NumericType.Int64,
            UInt64 = 1 << NumericType.UInt64,
            IntPtr = 1 << NumericType.IntPtr,
            UIntPtr = 1 << NumericType.UIntPtr,
            Single = 1 << NumericType.Single,
            Double = 1 << NumericType.Double,
            Decimal = 1 << NumericType.Decimal

            // ReSharper restore UnusedMember.Local
        }

        public static bool HasAllFlags(this NumericTypes value, NumericTypes flags) => (value & flags) == flags;
        public static bool HasAnyFlag(this NumericTypes value, NumericTypes flags) => (value & flags) != 0;

        #endregion

        #region Nested type: TypeNode

        private sealed class TypeNode
        {
            private static readonly ConcurrentDictionary<Type, TypeNode> typeNodeMap = new();

            private readonly Type type;

            private readonly TypeNode baseNode;

            private readonly IReadOnlyCollection<TypeNode> interfaceNodes;

            public static bool TryGetUpcastCount(Type sourceType, Type targetType, out uint count)
            {
                count = uint.MaxValue;

                if (targetType != typeof(object))
                {
                    GetOrCreate(sourceType).GetUpcastCountInternal(targetType, 0U, ref count);
                    return count < uint.MaxValue;
                }

                return true;
            }

            private TypeNode(Type type, TypeNode baseNode, IReadOnlyCollection<TypeNode> interfaceNodes)
            {
                this.type = type;
                this.baseNode = baseNode;
                this.interfaceNodes = interfaceNodes;
            }

            private static TypeNode GetOrCreate(Type type) => typeNodeMap.GetOrAdd(type, Create);

            private static TypeNode Create(Type type)
            {
                TypeNode baseNode = null;
                var redundantInterfaces = new List<Type>();

                var baseType = type.BaseType;
                if (baseType is not null)
                {
                    redundantInterfaces.AddRange(baseType.GetInterfaces());
                    baseNode = GetOrCreate(baseType);
                }

                var allInterfaces = type.GetInterfaces();
                foreach (var interfaceType in allInterfaces)
                {
                    redundantInterfaces.AddRange(interfaceType.GetInterfaces());
                }

                var interfaces = allInterfaces.Except(redundantInterfaces.Distinct());
                return new TypeNode(type, baseNode, interfaces.Select(GetOrCreate).ToArray());
            }

            private void GetUpcastCountInternal(Type targetType, uint count, ref uint lowestCount)
            {
                if (type == targetType)
                {
                    if (count < lowestCount)
                    {
                        lowestCount = count;
                    }
                }
                else
                {
                    baseNode?.GetUpcastCountInternal(targetType, count + 1, ref lowestCount);
                    if (targetType.IsInterface)
                    {
                        foreach (var interfaceNode in interfaceNodes)
                        {
                            interfaceNode.GetUpcastCountInternal(targetType, count + 1, ref lowestCount);
                        }
                    }
                }
            }
        }

        #endregion

        #region Nested type: PropertySignatureComparer

        private sealed class PropertySignatureComparer : EqualityComparer<PropertyInfo>
        {
            public static readonly PropertySignatureComparer Instance = new();

            public override bool Equals(PropertyInfo first, PropertyInfo second)
            {
                var firstParamTypes = first.GetIndexParameters().Select(param => param.ParameterType);
                var secondParamTypes = second.GetIndexParameters().Select(param => param.ParameterType);
                return firstParamTypes.SequenceEqual(secondParamTypes);
            }

            public override int GetHashCode(PropertyInfo property)
            {
                var hashCode = 0;

                var parameters = property.GetIndexParameters();
                foreach (var param in parameters)
                {
                    hashCode = unchecked((hashCode * 31) + param.ParameterType.GetHashCode());
                }

                return hashCode;
            }
        }

        #endregion

        #region Nested Type: BindArgFlag

        private enum BindArgFlag
        {
            // IMPORTANT: ascending cost order 

            IsParamArrayArg,
            NullableTransition,
            NumericConversion,
            ImplicitConversion,
            ByRefMismatch
        }

        #endregion

        #region Nested type: BindArgFlags

        [Flags]
        private enum BindArgFlags : uint
        {
            IsParamArrayArg = 1 << BindArgFlag.IsParamArrayArg,
            NullableTransition = 1 << BindArgFlag.NullableTransition,
            NumericConversion = 1 << BindArgFlag.NumericConversion,
            ImplicitConversion = 1 << BindArgFlag.ImplicitConversion,
            ByRefMismatch = 1 << BindArgFlag.ByRefMismatch
        }

        #endregion

        #region Nested type: BindArgCost

        private sealed class BindArgCost : IComparable<BindArgCost>
        {
            public BindArgFlags Flags;
            public NumericType NumericType;
            public uint UpcastCount;

            public int CompareTo(BindArgCost other)
            {
                var result = Flags.CompareTo(other.Flags);
                if (result != 0)
                {
                    return result;
                }

                if ((NumericType != NumericType.None) && (other.NumericType != NumericType.None))
                {
                    result = NumericType.CompareTo(other.NumericType);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                return UpcastCount.CompareTo(other.UpcastCount);
            }
        }

        #endregion

        #region Nested type: BindCandidate

        private sealed class BindCandidate<T> : IComparable<BindCandidate<T>> where T : MemberInfo
        {
            private readonly int defaultArgCount;
            private readonly bool paramArray;
            private readonly List<BindArgCost> argCosts;

            public T Candidate { get; }

            private BindCandidate(T candidate, int defaultArgCount, bool paramArray, List<BindArgCost> argCosts)
            {
                Candidate = candidate;
                this.defaultArgCount = defaultArgCount;
                this.paramArray = paramArray;
                this.argCosts = argCosts;
            }

            public int CompareTo(BindCandidate<T> other)
            {
                Debug.Assert(argCosts.Count == other.argCosts.Count);
                int result;

                for (var index = 0; index < argCosts.Count; index++)
                {
                    result = argCosts[index].CompareTo(other.argCosts[index]);
                    if (result != 0)
                    {
                        return result;
                    }
                }

                result = defaultArgCount.CompareTo(other.defaultArgCount);
                if (result != 0)
                {
                    return result;
                }

                result = paramArray.CompareTo(other.paramArray);
                if (result != 0)
                {
                    return result;
                }

                var declType = Candidate.DeclaringType;
                var otherDeclType = other.Candidate.DeclaringType;

                if (otherDeclType == declType)
                {
                    return 0;
                }

                return TypeNode.TryGetUpcastCount(Candidate.DeclaringType, other.Candidate.DeclaringType, out _) ? -1 : 1;
            }

            public static BindCandidate<T> TryCreateInstance(IHostContext context, T candidate, ParameterInfo[] parameters, object[] args, Type[] argTypes)
            {
                var defaultArgCount = 0;
                var paramArray = false;
                var argCosts = new List<BindArgCost>();

                var paramIndex = 0;
                var argIndex = 0;

                Type paramType = null;
                BindArgCost cost;

                for (; paramIndex < parameters.Length; paramIndex++)
                {
                    var param = parameters[paramIndex];
                    paramType = param.ParameterType;

                    if ((paramIndex == (parameters.Length - 1)) && paramType.IsArray && CustomAttributes.Has<ParamArrayAttribute>(context, param, false))
                    {
                        paramArray = true;
                        break;
                    }

                    if (argIndex >= args.Length)
                    {
                        if (!param.IsOptional && !param.HasDefaultValue)
                        {
                            return null;
                        }

                        ++defaultArgCount;
                        continue;
                    }

                    if ((args[argIndex] is Missing) && (param.IsOptional || param.HasDefaultValue))
                    {
                        ++defaultArgCount;
                        ++argIndex;
                        continue;
                    }

                    if (!paramType.IsBindableFromArg(args[argIndex], argTypes[argIndex], out cost))
                    {
                        return null;
                    }

                    argCosts.Add(cost);
                    ++argIndex;
                }

                if (paramArray)
                {
                    if (argIndex >= args.Length)
                    {
                        return new BindCandidate<T>(candidate, defaultArgCount, true, argCosts);
                    }

                    if ((argIndex == (args.Length - 1)) && paramType.IsBindableFromArg(args[argIndex], argTypes[argIndex], out cost))
                    {
                        argCosts.Add(cost);
                        return new BindCandidate<T>(candidate, defaultArgCount, true, argCosts);
                    }

                    paramType = paramType.GetElementType();
                    for (; argIndex < args.Length; argIndex++)
                    {
                        if (!paramType.IsBindableFromArg(args[argIndex], argTypes[argIndex], out cost))
                        {
                            return null;
                        }

                        cost.Flags |= BindArgFlags.IsParamArrayArg;
                        argCosts.Add(cost);
                    }
                }
                else if (argIndex < args.Length)
                {
                    return null;
                }

                return new BindCandidate<T>(candidate, defaultArgCount, paramArray, argCosts);
            }
        }

        #endregion
    }
}
