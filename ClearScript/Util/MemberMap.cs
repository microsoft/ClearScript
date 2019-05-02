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
        private static readonly MemberMapImpl<Field> fieldMap = new MemberMapImpl<Field>();
        private static readonly MemberMapImpl<Method> methodMap = new MemberMapImpl<Method>();
        private static readonly MemberMapImpl<Property> propertyMap = new MemberMapImpl<Property>();

        public static FieldInfo GetField(string name)
        {
            return fieldMap.GetMember(name);
        }

        public static FieldInfo[] GetFields(string[] names)
        {
            // ReSharper disable CoVariantArrayConversion
            return fieldMap.GetMembers(names);
            // ReSharper restore CoVariantArrayConversion
        }

        public static MethodInfo GetMethod(string name)
        {
            return methodMap.GetMember(name);
        }

        public static MethodInfo[] GetMethods(string[] names)
        {
            // ReSharper disable CoVariantArrayConversion
            return methodMap.GetMembers(names);
            // ReSharper restore CoVariantArrayConversion
        }

        public static PropertyInfo GetProperty(string name)
        {
            return propertyMap.GetMember(name);
        }

        public static PropertyInfo[] GetProperties(string[] names)
        {
            // ReSharper disable CoVariantArrayConversion
            return propertyMap.GetMembers(names);
            // ReSharper restore CoVariantArrayConversion
        }

        // ReSharper disable ClassNeverInstantiated.Local

        #region Nested type: Field

        private sealed class Field : FieldInfo
        {
            private readonly string name;

            public Field(string name)
            {
                this.name = name;
            }

            #region FieldInfo overrides

            public override FieldAttributes Attributes
            {
                get
                {
                    // This occurs during VB-based dynamic script item invocation. It was not
                    // observed before script items gained an IReflect/IExpando implementation that
                    // exposes script item properties as fields. Apparently VB's dynamic invocation
                    // support not only recognizes IReflect/IExpando but actually favors it over
                    // DynamicObject.

                    return FieldAttributes.Public;
                }
            }

            public override RuntimeFieldHandle FieldHandle
            {
                get { throw new NotImplementedException(); }
            }

            public override Type FieldType
            {
                get
                {
                    // This occurs during VB-based dynamic script item invocation. It was not
                    // observed before script items gained an IReflect/IExpando implementation that
                    // exposes script item properties as fields. Apparently VB's dynamic invocation
                    // support not only recognizes IReflect/IExpando but actually favors it over
                    // DynamicObject.

                    return typeof(object);
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    // This occurs during VB-based dynamic script item invocation. It was not
                    // observed before script items gained an IReflect/IExpando implementation that
                    // exposes script item properties as fields. Apparently VB's dynamic invocation
                    // support not only recognizes IReflect/IExpando but actually favors it over
                    // DynamicObject.

                    return typeof(object);
                }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

            public override object GetValue(object obj)
            {
                // This occurs during VB-based dynamic script item invocation. It was not observed
                // before script items gained an IReflect/IExpando implementation that exposes
                // script item properties as fields. Apparently VB's dynamic invocation support not
                // only recognizes IReflect/IExpando but actually favors it over DynamicObject.

                var reflect = obj as IReflect;
                if (reflect != null)
                {
                    return reflect.InvokeMember(name, BindingFlags.GetField, null, obj, ArrayHelpers.GetEmptyArray<object>(), null, CultureInfo.InvariantCulture, null);
                }

                throw new InvalidOperationException("Invalid field retrieval");
            }

            public override void SetValue(object obj, object value, BindingFlags invokeFlags, Binder binder, CultureInfo culture)
            {
                // This occurs during VB-based dynamic script item invocation. It was not observed
                // before script items gained an IReflect/IExpando implementation that exposes
                // script item properties as fields. Apparently VB's dynamic invocation support not
                // only recognizes IReflect/IExpando but actually favors it over DynamicObject.

                var reflect = obj as IReflect;
                if (reflect != null)
                {
                    reflect.InvokeMember(name, BindingFlags.SetField, null, obj, new[] { value }, null, culture, null);
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
            private readonly string name;

            public Method(string name)
            {
                this.name = name;
            }

            #region MethodInfo overrides

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get { throw new NotImplementedException(); }
            }

            public override MethodAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get { throw new NotImplementedException(); }
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

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
            private readonly string name;

            public Property(string name)
            {
                this.name = name;
            }

            #region PropertyInfo overrides

            public override PropertyAttributes Attributes
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanRead
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanWrite
            {
                get { throw new NotImplementedException(); }
            }

            public override Type PropertyType
            {
                get { throw new NotImplementedException(); }
            }

            public override Type DeclaringType
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { return name; }
            }

            public override Type ReflectedType
            {
                get { throw new NotImplementedException(); }
            }

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
            private readonly object dataLock = new object();
            private readonly Dictionary<string, WeakReference> map = new Dictionary<string, WeakReference>();
            private DateTime lastCompactionTime = DateTime.MinValue;

            public T GetMember(string name)
            {
                lock (dataLock)
                {
                    var result = GetMemberInternal(name);
                    CompactIfNecessary();
                    return result;
                }
            }

            public T[] GetMembers(string[] names)
            {
                lock (dataLock)
                {
                    var result = names.Select(GetMemberInternal).ToArray();
                    CompactIfNecessary();
                    return result;
                }
            }

            private T GetMemberInternal(string name)
            {
                T member;

                WeakReference weakRef;
                if (map.TryGetValue(name, out weakRef))
                {
                    member = weakRef.Target as T;
                    if (member == null)
                    {
                        member = (T)typeof(T).CreateInstance(name);
                        weakRef.Target = member;
                    }
                }
                else
                {
                    member = (T)typeof(T).CreateInstance(name);
                    map.Add(name, new WeakReference(member));
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
                        map.Where(pair => !pair.Value.IsAlive).ToList().ForEach(pair => map.Remove(pair.Key));
                        lastCompactionTime = now;
                    }
                }
            }
        }

        #endregion
    }
}
