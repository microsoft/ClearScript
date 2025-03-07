// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal static class MemberMap
    {
        private static readonly MemberMapImpl<Field> fieldMap = new();
        private static readonly MemberMapImpl<Method> methodMap = new();
        private static readonly MemberMapImpl<Property> propertyMap = new();

        public static FieldInfo GetField(string name)
        {
            return fieldMap.GetMember(name);
        }

        public static FieldInfo[] GetFields(string[] names)
        {
            // ReSharper disable once CoVariantArrayConversion
            return fieldMap.GetMembers(names);
        }

        public static MethodInfo GetMethod(string name)
        {
            return methodMap.GetMember(name);
        }

        public static MethodInfo[] GetMethods(string[] names)
        {
            // ReSharper disable once CoVariantArrayConversion
            return methodMap.GetMembers(names);
        }

        public static PropertyInfo GetProperty(string name)
        {
            return propertyMap.GetMember(name);
        }

        public static PropertyInfo[] GetProperties(string[] names)
        {
            // ReSharper disable once CoVariantArrayConversion
            return propertyMap.GetMembers(names);
        }

        // ReSharper disable ClassNeverInstantiated.Local

        #region Nested type: Field

        private sealed class Field : FieldInfo
        {
            public Field(string name)
            {
                Name = name;
            }

            #region FieldInfo overrides

            public override FieldAttributes Attributes => FieldAttributes.Public;
                // This occurs during VB-based dynamic script item invocation. It was not
                // observed before script items gained an IReflect/IExpando implementation that
                // exposes script item properties as fields. Apparently VB's dynamic invocation
                // support not only recognizes IReflect/IExpando but actually favors it over
                // DynamicObject.

            public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

            public override Type FieldType => typeof(object);
                // This occurs during VB-based dynamic script item invocation. It was not
                // observed before script items gained an IReflect/IExpando implementation that
                // exposes script item properties as fields. Apparently VB's dynamic invocation
                // support not only recognizes IReflect/IExpando but actually favors it over
                // DynamicObject.

            public override Type DeclaringType => typeof(object);
                // This occurs during VB-based dynamic script item invocation. It was not
                // observed before script items gained an IReflect/IExpando implementation that
                // exposes script item properties as fields. Apparently VB's dynamic invocation
                // support not only recognizes IReflect/IExpando but actually favors it over
                // DynamicObject.

            public override string Name { get; }

            public override Type ReflectedType => throw new NotImplementedException();

            public override object GetValue(object obj)
            {
                // This occurs during VB-based dynamic script item invocation. It was not observed
                // before script items gained an IReflect/IExpando implementation that exposes
                // script item properties as fields. Apparently VB's dynamic invocation support not
                // only recognizes IReflect/IExpando but actually favors it over DynamicObject.

                if (obj is IReflect reflect)
                {
                    return reflect.InvokeMember(Name, BindingFlags.GetField, null, obj, ArrayHelpers.GetEmptyArray<object>(), null, CultureInfo.InvariantCulture, null);
                }

                throw new InvalidOperationException("Invalid field retrieval");
            }

            public override void SetValue(object obj, object value, BindingFlags invokeFlags, Binder binder, CultureInfo culture)
            {
                // This occurs during VB-based dynamic script item invocation. It was not observed
                // before script items gained an IReflect/IExpando implementation that exposes
                // script item properties as fields. Apparently VB's dynamic invocation support not
                // only recognizes IReflect/IExpando but actually favors it over DynamicObject.

                if (obj is IReflect reflect)
                {
                    reflect.InvokeMember(Name, BindingFlags.SetField, null, obj, new[] { value }, null, culture, null);
                    return;
                }

                throw new InvalidOperationException("Invalid field assignment");
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return ArrayHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Nested type: Method

        private sealed class Method : MethodInfo
        {
            public Method(string name)
            {
                Name = name;
            }

            #region MethodInfo overrides

            public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

            public override MethodAttributes Attributes => throw new NotImplementedException();

            public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

            public override Type DeclaringType => throw new NotImplementedException();

            public override string Name { get; }

            public override Type ReflectedType => throw new NotImplementedException();

            public override MethodInfo GetBaseDefinition()
            {
                throw new NotImplementedException();
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetParameters()
            {
                return ArrayHelpers.GetEmptyArray<ParameterInfo>();
            }

            public override object Invoke(object obj, BindingFlags invokeFlags, Binder binder, object[] args, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return ArrayHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Nested type: Property

        private sealed class Property : PropertyInfo
        {
            public Property(string name)
            {
                Name = name;
            }

            #region PropertyInfo overrides

            public override PropertyAttributes Attributes => throw new NotImplementedException();

            public override bool CanRead => throw new NotImplementedException();

            public override bool CanWrite => throw new NotImplementedException();

            public override Type PropertyType => throw new NotImplementedException();

            public override Type DeclaringType => throw new NotImplementedException();

            public override string Name { get; }

            public override Type ReflectedType => throw new NotImplementedException();

            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetIndexParameters()
            {
                return ArrayHelpers.GetEmptyArray<ParameterInfo>();
            }

            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override object GetValue(object obj, BindingFlags invokeFlags, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object obj, object value, BindingFlags invokeFlags, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return ArrayHelpers.GetEmptyArray<object>();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        // ReSharper restore ClassNeverInstantiated.Local

        #region Nested type: MemberMapBase

        private class MemberMapBase
        {
            protected const int CompactionThreshold = 1024 * 1024;
            protected static readonly TimeSpan CompactionInterval = TimeSpan.FromMinutes(5);
        }

        #endregion

        #region Nested type: MemberMapImpl<T>

        private sealed class MemberMapImpl<T> : MemberMapBase where T : MemberInfo
        {
            private readonly Dictionary<string, WeakReference<T>> map = new();
            private DateTime lastCompactionTime = DateTime.MinValue;

            public T GetMember(string name)
            {
                lock (map)
                {
                    var result = GetMemberInternal(name);
                    CompactIfNecessary();
                    return result;
                }
            }

            public T[] GetMembers(string[] names)
            {
                lock (map)
                {
                    var result = names.Select(GetMemberInternal).ToArray();
                    CompactIfNecessary();
                    return result;
                }
            }

            private T GetMemberInternal(string name)
            {
                T member;

                if (map.TryGetValue(name, out var weakRef))
                {
                    if (!weakRef.TryGetTarget(out member))
                    {
                        member = (T)typeof(T).CreateInstance(name);
                        weakRef.SetTarget(member);
                    }
                }
                else
                {
                    member = (T)typeof(T).CreateInstance(name);
                    map.Add(name, new WeakReference<T>(member));
                }

                return member;
            }

            private void CompactIfNecessary()
            {
                if (map.Count >= CompactionThreshold)
                {
                    var now = DateTime.UtcNow;
                    if ((lastCompactionTime + CompactionInterval) <= now)
                    {
                        map.Where(pair => !pair.Value.TryGetTarget(out _)).ToList().ForEach(pair => map.Remove(pair.Key));
                        lastCompactionTime = now;
                    }
                }
            }
        }

        #endregion
    }
}
