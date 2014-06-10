// 
// Copyright (c) Microsoft Corporation. All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.Expando;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal partial class HostItem : DynamicObject, IExpando, IDynamic, IScriptMarshalWrapper
    {
        #region data

        private readonly ScriptEngine engine;
        private readonly HostTarget target;
        private readonly HostItemFlags flags;

        private readonly IExpando thisExpando;
        private readonly IDynamic thisDynamic;

        private Type accessContext;

        private IDynamic targetDynamic;
        private IPropertyBag targetPropertyBag;
        private IList targetList;
        private DynamicMetaObject targetDynamicMetaObject;

        private string[] cachedLocalEventNames;
        private string[] cachedLocalFieldNames;
        private string[] cachedLocalMethodNames;
        private string[] cachedLocalPropertyNames;

        private string[] cachedFieldNames;
        private string[] cachedMethodNames;
        private string[] cachedPropertyNames;

        private FieldInfo[] cachedFields;
        private MethodInfo[] cachedMethods;
        private PropertyInfo[] cachedProperties;

        private string[] cachedMemberNames;
        private int[] cachedPropertyIndices;

        private ExtensionMethodSummary cachedExtensionMethodSummary;
        private int cachedListCount;
        private HashSet<string> expandoMemberNames;

        private Dictionary<string, HostMethod> hostMethodMap;
        private Dictionary<string, HostIndexedProperty> hostIndexedPropertyMap;

        private static readonly MemberMap<Field> fieldMap = new MemberMap<Field>();
        private static readonly MemberMap<Method> methodMap = new MemberMap<Method>();
        private static readonly MemberMap<Property> propertyMap = new MemberMap<Property>();

        #endregion

        #region constructors

        private HostItem(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            this.engine = engine;
            this.target = target;
            this.flags = flags;

            thisExpando = this;
            thisDynamic = this;

            Initialize();
        }

        #endregion

        #region wrappers

        public static object Wrap(ScriptEngine engine, object obj)
        {
            return Wrap(engine, obj, null, HostItemFlags.None);
        }

        public static object Wrap(ScriptEngine engine, object obj, Type type)
        {
            return Wrap(engine, obj, type, HostItemFlags.None);
        }

        public static object Wrap(ScriptEngine engine, object obj, HostItemFlags flags)
        {
            return Wrap(engine, obj, null, flags);
        }

        private static object Wrap(ScriptEngine engine, object obj, Type type, HostItemFlags flags)
        {
            Debug.Assert(!(obj is HostItem));

            if (obj == null)
            {
                return null;
            }

            var hostTarget = obj as HostTarget;
            if (hostTarget != null)
            {
                return BindOrCreate(engine, hostTarget, flags);
            }

            if (type == null)
            {
                type = obj.GetType();
            }
            else
            {
                Debug.Assert(type.IsInstanceOfType(obj));
            }

            if (obj is Enum)
            {
                return BindOrCreate(engine, obj, type, flags);
            }

            var typeCode = Type.GetTypeCode(type);
            if ((typeCode == TypeCode.Object) || (typeCode == TypeCode.DateTime))
            {
                return BindOrCreate(engine, obj, type, flags);
            }

            return obj;
        }

        #endregion

        #region public members

        public ScriptEngine Engine
        {
            get { return engine; }
        }

        public HostTarget Target
        {
            get { return target; }
        }

        public HostItemFlags Flags
        {
            get { return flags; }
        }

        public object InvokeMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture)
        {
            AdjustInvokeFlags(ref invokeFlags);

            object result;
            if (target.TryInvokeAuxMember(engine, name, invokeFlags, args, bindArgs, out result))
            {
                if (target is IHostVariable)
                {
                    // the variable may have been reassigned
                    BindSpecialTarget();
                }

                return result;
            }

            if (targetDynamic != null)
            {
                return InvokeDynamicMember(name, invokeFlags, args);
            }

            if (targetPropertyBag != null)
            {
                return InvokePropertyBagMember(name, invokeFlags, args, bindArgs);
            }

            if (targetList != null)
            {
                int index;
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    return InvokeListElement(index, invokeFlags, args, bindArgs);
                }
            }

            return InvokeHostMember(name, invokeFlags, args, bindArgs, culture);
        }

        #endregion

        #region internal members

        private static object BindOrCreate(ScriptEngine engine, object target, Type type, HostItemFlags flags)
        {
            return BindOrCreate(engine, HostObject.Wrap(target, type), flags);
        }

        private static object BindOrCreate(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            return engine.GetOrCreateHostItem(target, flags, Create);
        }

        private static HostItem Create(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            return new HostItem(engine, target, flags);
        }

        private void Initialize()
        {
            if ((target is HostObject) || (target is IHostVariable) || (target is IByRefArg))
            {
                BindSpecialTarget();
            }

            if (flags.HasFlag(HostItemFlags.PrivateAccess))
            {
                accessContext = target.Type;
            }

            var scriptableObject = target.Target as IScriptableObject;
            if (scriptableObject != null)
            {
                scriptableObject.OnExposedToScriptCode(engine);
            }
        }

        private void BindSpecialTarget()
        {
            if (BindSpecialTarget(out targetDynamic))
            {
                targetPropertyBag = null;
                targetList = null;
                targetDynamicMetaObject = null;
            }
            else if (BindSpecialTarget(out targetPropertyBag))
            {
                targetList = null;
                targetDynamicMetaObject = null;
            }
            else
            {
                IDynamicMetaObjectProvider dynamicMetaObjectProvider;
                if (!flags.HasFlag(HostItemFlags.HideDynamicMembers) && BindSpecialTarget(out dynamicMetaObjectProvider))
                {
                    targetDynamicMetaObject = dynamicMetaObjectProvider.GetMetaObject(Expression.Constant(target.InvokeTarget));
                    targetList = null;
                }
                else
                {
                    targetDynamicMetaObject = null;
                    BindSpecialTarget(out targetList);
                }
            }
        }

        private bool BindSpecialTarget<T>(out T specialTarget) where T : class
        {
            // The check here is required because the item may be bound to a specific target base
            // class or interface - one that must not trigger special treatment.

            if (typeof(T).IsAssignableFrom(target.Type))
            {
                specialTarget = target.InvokeTarget as T;
                return specialTarget != null;
            }

            specialTarget = null;
            return false;
        }

        private BindingFlags GetCommonBindFlags()
        {
            var bindFlags = BindingFlags.Public;

            if (flags.HasFlag(HostItemFlags.PrivateAccess) || (target.Type == engine.AccessContext))
            {
                bindFlags |= BindingFlags.NonPublic;
            }

            if (target.Flags.HasFlag(HostTargetFlags.AllowStaticMembers))
            {
                bindFlags |= BindingFlags.Static;
            }

            if (target.Flags.HasFlag(HostTargetFlags.AllowInstanceMembers))
            {
                bindFlags |= BindingFlags.Instance;
            }

            return bindFlags;
        }

        private BindingFlags GetMethodBindFlags()
        {
            return GetCommonBindFlags() | BindingFlags.OptionalParamBinding;
        }

        private void AdjustInvokeFlags(ref BindingFlags invokeFlags)
        {
            const BindingFlags onFlags =
                BindingFlags.Public |
                BindingFlags.OptionalParamBinding;

            const BindingFlags offFlags =
                BindingFlags.DeclaredOnly |
                BindingFlags.IgnoreCase |
                BindingFlags.ExactBinding;

            const BindingFlags setPropertyFlags =
                BindingFlags.SetProperty |
                BindingFlags.PutDispProperty |
                BindingFlags.PutRefDispProperty;

            invokeFlags |= onFlags;
            invokeFlags &= ~offFlags;

            if (flags.HasFlag(HostItemFlags.PrivateAccess) || (target.Type == engine.AccessContext))
            {
                invokeFlags |= BindingFlags.NonPublic;
            }

            if (target.Flags.HasFlag(HostTargetFlags.AllowStaticMembers))
            {
                invokeFlags |= BindingFlags.Static;
            }
            else
            {
                invokeFlags &= ~BindingFlags.Static;
            }

            if (target.Flags.HasFlag(HostTargetFlags.AllowInstanceMembers))
            {
                invokeFlags |= BindingFlags.Instance;
            }
            else
            {
                invokeFlags &= ~BindingFlags.Instance;
            }

            if (invokeFlags.HasFlag(BindingFlags.GetProperty))
            {
                invokeFlags |= BindingFlags.GetField;
            }

            if ((invokeFlags & setPropertyFlags) != 0)
            {
                invokeFlags |= BindingFlags.SetField;
            }
        }

        private string[] GetLocalEventNames()
        {
            if (cachedLocalEventNames == null)
            {
                var localEvents = target.Type.GetScriptableEvents(GetCommonBindFlags());
                cachedLocalEventNames = localEvents.Select(eventInfo => eventInfo.GetScriptName()).ToArray();
            }

            return cachedLocalEventNames;
        }

        private string[] GetLocalFieldNames()
        {
            if (cachedLocalFieldNames == null)
            {
                var localFields = target.Type.GetScriptableFields(GetCommonBindFlags());
                cachedLocalFieldNames = localFields.Select(field => field.GetScriptName()).ToArray();
            }

            return cachedLocalFieldNames;
        }

        private string[] GetLocalMethodNames()
        {
            if (cachedLocalMethodNames == null)
            {
                var localMethods = target.Type.GetScriptableMethods(GetMethodBindFlags());
                cachedLocalMethodNames = localMethods.Select(method => method.GetScriptName()).ToArray();
            }

            return cachedLocalMethodNames;
        }

        private string[] GetLocalPropertyNames()
        {
            if (cachedLocalPropertyNames == null)
            {
                var localProperties = target.Type.GetScriptableProperties(GetCommonBindFlags());
                cachedLocalPropertyNames = localProperties.Select(property => property.GetScriptName()).ToArray();
            }

            return cachedLocalPropertyNames;
        }

        private string[] GetAllFieldNames()
        {
            if ((targetDynamic == null) && (targetPropertyBag == null))
            {
                return GetLocalFieldNames().Concat(GetLocalEventNames()).Distinct().ToArray();
            }

            return MiscHelpers.GetEmptyArray<string>();
        }

        private string[] GetAllMethodNames()
        {
            var names = target.GetAuxMethodNames(GetMethodBindFlags()).AsEnumerable();
            if ((targetDynamic == null) && (targetPropertyBag == null))
            {
                names = names.Concat(GetLocalMethodNames());
                if (target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
                {
                    cachedExtensionMethodSummary = engine.ExtensionMethodSummary;
                    names = names.Concat(cachedExtensionMethodSummary.MethodNames);
                }
            }

            return names.Distinct().ToArray();
        }

        private string[] GetAllPropertyNames()
        {
            var names = target.GetAuxPropertyNames(GetCommonBindFlags()).AsEnumerable();
            if (targetDynamic != null)
            {
                names = names.Concat(targetDynamic.GetPropertyNames());
                names = names.Concat(targetDynamic.GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture)));
            }
            else if (targetPropertyBag != null)
            {
                names = names.Concat(targetPropertyBag.Keys);
            }
            else
            {
                names = names.Concat(GetLocalPropertyNames());

                if (targetList != null)
                {
                    cachedListCount = targetList.Count;
                    if (cachedListCount > 0)
                    {
                        names = names.Concat(Enumerable.Range(0, cachedListCount).Select(index => index.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                if (targetDynamicMetaObject != null)
                {
                    names = names.Concat(targetDynamicMetaObject.GetDynamicMemberNames());
                }
            }

            if (expandoMemberNames != null)
            {
                names = names.Except(expandoMemberNames);
            }

            return names.Distinct().ToArray();
        }

        private void UpdateFieldNames(out bool updated)
        {
            if (cachedFieldNames == null)
            {
                cachedFieldNames = GetAllFieldNames();
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void UpdateMethodNames(out bool updated)
        {
            if ((cachedMethodNames == null) ||
                (target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods) && (cachedExtensionMethodSummary != engine.ExtensionMethodSummary)))
            {
                cachedMethodNames = GetAllMethodNames();
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void UpdatePropertyNames(out bool updated)
        {
            if ((cachedPropertyNames == null) ||
                (targetDynamic != null) ||
                (targetPropertyBag != null) ||
                (targetDynamicMetaObject != null) ||
                ((targetList != null) && (cachedListCount != targetList.Count)))
            {
                cachedPropertyNames = GetAllPropertyNames();
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void AddExpandoMemberName(string name)
        {
            if (expandoMemberNames == null)
            {
                expandoMemberNames = new HashSet<string>();
            }

            expandoMemberNames.Add(name);
        }

        private void RemoveExpandoMemberName(string name)
        {
            if (expandoMemberNames != null)
            {
                expandoMemberNames.Remove(name);
            }
        }

        private object InvokeDynamicMember(string name, BindingFlags invokeFlags, object[] args)
        {
            if (invokeFlags.HasFlag(BindingFlags.CreateInstance))
            {
                if (name == SpecialMemberNames.Default)
                {
                    return targetDynamic.Invoke(args, true);
                }

                throw new InvalidOperationException("Invalid constructor invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    try
                    {
                        return targetDynamic.Invoke(args, false);
                    }
                    catch (Exception)
                    {
                        if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                        {
                            return targetDynamic;
                        }

                        throw;
                    }
                }

                try
                {
                    return targetDynamic.InvokeMethod(name, args);
                }
                catch (Exception)
                {
                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        return targetDynamic.GetProperty(name);
                    }

                    throw;
                }
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return targetDynamic.GetProperty(name);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length == 1)
                {
                    var value = args[0];
                    targetDynamic.SetProperty(name, value);
                    return value;
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokePropertyBagMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        return targetPropertyBag;
                    }

                    throw new NotSupportedException("Object does not support invocation");
                }

                object value;
                if (!targetPropertyBag.TryGetValue(name, out value))
                {
                    throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no property named '{0}'", name));
                }

                object result;
                if (InvokeHelpers.TryInvokeObject(engine, value, invokeFlags, args, bindArgs, true, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                {
                    return value;
                }

                throw new NotSupportedException("Object does not support the requested invocation operation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                if (args.Length < 1)
                {
                    object value;
                    return targetPropertyBag.TryGetValue(name, out value) ? value : Nonexistent.Value;
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length == 1)
                {
                    return targetPropertyBag[name] = args[0];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokeListElement(int index, BindingFlags invokeFlags, object[] args, object[] bindArgs)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                object result;
                if (InvokeHelpers.TryInvokeObject(engine, targetList[index], invokeFlags, args, bindArgs, true, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                {
                    return targetList[index];
                }

                throw new NotSupportedException("Object does not support the requested invocation operation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                if (args.Length < 1)
                {
                    return targetList[index];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length == 1)
                {
                    return targetList[index] = args[0];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokeHostMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture)
        {
            if (invokeFlags.HasFlag(BindingFlags.CreateInstance))
            {
                if (name == SpecialMemberNames.Default)
                {
                    var hostType = target as HostType;
                    if (hostType != null)
                    {
                        var typeArgs = GetTypeArgs(args).Select(HostType.Wrap).ToArray();
                        if (typeArgs.Length > 0)
                        {
                            // ReSharper disable CoVariantArrayConversion

                            object result;
                            if (hostType.TryInvoke(engine, BindingFlags.InvokeMethod, typeArgs, typeArgs, out result))
                            {
                                hostType = result as HostType;
                                if (hostType != null)
                                {
                                    args = args.Skip(typeArgs.Length).ToArray();

                                    var specificType = hostType.GetSpecificType();
                                    if (typeof(Delegate).IsAssignableFrom(specificType))
                                    {
                                        if (args.Length != 1)
                                        {
                                            throw new InvalidOperationException("Invalid constructor invocation");
                                        }

                                        return DelegateFactory.CreateDelegate(engine, args[0], specificType);
                                    }

                                    return specificType.CreateInstance(invokeFlags, args);
                                }
                            }

                            throw new InvalidOperationException("Invalid constructor invocation");

                            // ReSharper restore CoVariantArrayConversion
                        }

                        var type = hostType.GetSpecificType();
                        if (typeof(Delegate).IsAssignableFrom(type))
                        {
                            if (args.Length != 1)
                            {
                                throw new InvalidOperationException("Invalid constructor invocation");
                            }

                            return DelegateFactory.CreateDelegate(engine, args[0], type);
                        }

                        return type.CreateInstance(invokeFlags, args);
                    }

                    if (targetDynamicMetaObject != null)
                    {
                        object result;
                        if (targetDynamicMetaObject.TryCreateInstance(engine, args, out result))
                        {
                            return result;
                        }
                    }
                }

                throw new InvalidOperationException("Invalid constructor invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    object result;
                    if (InvokeHelpers.TryInvokeObject(engine, target, invokeFlags, args, bindArgs, targetDynamicMetaObject != null, out result))
                    {
                        return result;
                    }

                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        return target;
                    }

                    throw new NotSupportedException("Object does not support the requested invocation operation");
                }

                if ((targetDynamicMetaObject != null) && (targetDynamicMetaObject.GetDynamicMemberNames().Contains(name)))
                {
                    object result;
                    if (targetDynamicMetaObject.TryInvokeMember(engine, name, invokeFlags, args, out result))
                    {
                        return result;
                    }
                }

                if (thisExpando.GetMethods(GetMethodBindFlags()).Any(method => method.Name == name))
                {
                    return InvokeMethod(name, args, bindArgs);
                }

                var property = target.Type.GetScriptableProperty(name, GetCommonBindFlags(), MiscHelpers.GetEmptyArray<object>());
                if ((property != null) && (typeof(Delegate).IsAssignableFrom(property.PropertyType)))
                {
                    var del = (Delegate)property.GetValue(target.InvokeTarget, invokeFlags | BindingFlags.GetProperty, Type.DefaultBinder, MiscHelpers.GetEmptyArray<object>(), culture);
                    return InvokeHelpers.InvokeDelegate(engine, del, args);
                }

                var field = target.Type.GetScriptableField(name, GetCommonBindFlags());
                if ((field != null) && (typeof(Delegate).IsAssignableFrom(field.FieldType)))
                {
                    var del = (Delegate)field.GetValue(target.InvokeTarget);
                    return InvokeHelpers.InvokeDelegate(engine, del, args);
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    return GetHostProperty(name, invokeFlags, args, bindArgs, culture);
                }

                throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no suitable method named '{0}'", name));
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return GetHostProperty(name, invokeFlags, args, bindArgs, culture);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                return SetHostProperty(name, invokeFlags, args, bindArgs, culture);
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object GetHostProperty(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture)
        {
            if ((targetDynamicMetaObject != null) && (targetDynamicMetaObject.GetDynamicMemberNames().Contains(name)))
            {
                object result;
                if (targetDynamicMetaObject.TryGetMember(name, out result))
                {
                    return result;
                }

                if (hostMethodMap == null)
                {
                    hostMethodMap = new Dictionary<string, HostMethod>();
                }

                HostMethod hostMethod;
                if (!hostMethodMap.TryGetValue(name, out hostMethod))
                {
                    hostMethod = new HostMethod(this, name);
                    hostMethodMap.Add(name, hostMethod);
                }

                return hostMethod;
            }

            var property = target.Type.GetScriptableProperty(name, invokeFlags, bindArgs);
            if (property != null)
            {
                var result = property.GetValue(target.InvokeTarget, invokeFlags, Type.DefaultBinder, args, culture);
                return engine.PrepareResult(result, property.PropertyType, property.IsRestrictedForScript());
            }

            if (target.Type.GetScriptableProperties(name, invokeFlags).Any())
            {
                if (hostIndexedPropertyMap == null)
                {
                    hostIndexedPropertyMap = new Dictionary<string, HostIndexedProperty>();
                }

                HostIndexedProperty hostIndexedProperty;
                if (!hostIndexedPropertyMap.TryGetValue(name, out hostIndexedProperty))
                {
                    hostIndexedProperty = new HostIndexedProperty(this, name);
                    hostIndexedPropertyMap.Add(name, hostIndexedProperty);
                }

                return hostIndexedProperty;
            }

            var field = target.Type.GetScriptableField(name, invokeFlags);
            if (field != null)
            {
                var result = field.GetValue(target.InvokeTarget);
                return engine.PrepareResult(result, field.FieldType, field.IsRestrictedForScript());
            }

            var eventInfo = target.Type.GetScriptableEvent(name, invokeFlags);
            if (eventInfo != null)
            {
                var type = typeof(EventSource<>).MakeSpecificType(eventInfo.EventHandlerType);
                return type.CreateInstance(BindingFlags.NonPublic, engine, target.InvokeTarget, eventInfo);
            }

            var method = thisExpando.GetMethods(GetMethodBindFlags()).FirstOrDefault(testMethod => testMethod.Name == name);
            if (method != null)
            {
                if (hostMethodMap == null)
                {
                    hostMethodMap = new Dictionary<string, HostMethod>();
                }

                HostMethod hostMethod;
                if (!hostMethodMap.TryGetValue(name, out hostMethod))
                {
                    hostMethod = new HostMethod(this, name);
                    hostMethodMap.Add(name, hostMethod);
                }

                return hostMethod;
            }

            return Nonexistent.Value;
        }

        private object SetHostProperty(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture)
        {
            if (name == SpecialMemberNames.Default)
            {
                // special case to enable JScript/VBScript "x(a) = b" syntax when x is a host indexed property 

                object result;
                if (InvokeHelpers.TryInvokeObject(engine, target, invokeFlags, args, bindArgs, false, out result))
                {
                    return result;
                }

                throw new InvalidOperationException("Invalid property assignment");
            }

            if ((targetDynamicMetaObject != null) && (args.Length == 1))
            {
                object result;
                if (targetDynamicMetaObject.TrySetMember(name, args[0], out result))
                {
                    return result;
                }
            }

            if (args.Length < 1)
            {
                throw new InvalidOperationException("Invalid argument count");
            }

            var property = target.Type.GetScriptableProperty(name, invokeFlags, bindArgs.Take(bindArgs.Length - 1).ToArray());
            if (property != null)
            {
                if (property.IsReadOnlyForScript())
                {
                    throw new UnauthorizedAccessException("Property is read-only");
                }

                var value = args[args.Length - 1];
                if (property.PropertyType.IsAssignableFrom(ref value))
                {
                    property.SetValue(target.InvokeTarget, value, invokeFlags, Type.DefaultBinder, args.Take(args.Length - 1).ToArray(), culture);
                    return value;
                }

                throw new ArgumentException("Invalid property assignment");
            }

            var field = target.Type.GetScriptableField(name, invokeFlags);
            if (field != null)
            {
                if (field.IsReadOnlyForScript())
                {
                    throw new UnauthorizedAccessException("Field is read-only");
                }

                if (args.Length == 1)
                {
                    var value = args[0];
                    if (field.FieldType.IsAssignableFrom(ref value))
                    {
                        field.SetValue(target.InvokeTarget, value);
                        return value;
                    }

                    throw new ArgumentException("Invalid field assignment");
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no suitable property or field named '{0}'", name));
        }

        #endregion

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("[{0}]", target);
        }

        #endregion

        #region DynamicObject overrides

        public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            result = thisDynamic.Invoke(args, true).ToDynamicResult(engine);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = thisDynamic.GetProperty(binder.Name).ToDynamicResult(engine);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            thisDynamic.SetProperty(binder.Name, value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            if (indices.Length == 1)
            {
                int index;
                if (MiscHelpers.TryGetIndex(indices[0], out index))
                {
                    result = thisDynamic.GetProperty(index).ToDynamicResult(engine);
                }
                else
                {
                    result = thisDynamic.GetProperty(indices[0].ToString()).ToDynamicResult(engine);
                }

                return true;
            }

            throw new InvalidOperationException("Invalid argument or index count");
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indices, object value)
        {
            if (indices.Length == 1)
            {
                int index;
                if (MiscHelpers.TryGetIndex(indices[0], out index))
                {
                    thisDynamic.SetProperty(index, value);
                }
                else
                {
                    thisDynamic.SetProperty(indices[0].ToString(), value);
                }

                return true;
            }

            throw new InvalidOperationException("Invalid argument or index count");
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = thisDynamic.Invoke(args, false).ToDynamicResult(engine);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = thisDynamic.InvokeMethod(binder.Name, args).ToDynamicResult(engine);
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if ((target is HostObject) || (target is IHostVariable) || (target is IByRefArg))
            {
                if (binder.Type.IsAssignableFrom(target.Type))
                {
                    result = Convert.ChangeType(target.InvokeTarget, binder.Type);
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region IReflect implementation

        FieldInfo IReflect.GetField(string name, BindingFlags bindFlags)
        {
            var fields = thisExpando.GetFields(bindFlags).Where(field => field.Name == name).ToArray();
            if (fields.Length < 1)
            {
                return null;
            }

            if (fields.Length > 1)
            {
                throw new AmbiguousMatchException(MiscHelpers.FormatInvariant("Object has multiple fields named '{0}'", name));
            }

            return fields[0];
        }

        FieldInfo[] IReflect.GetFields(BindingFlags bindFlags)
        {
            return engine.HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdateFieldNames(out updated);
                if (updated || (cachedFields == null))
                {
                    cachedFields = fieldMap.GetMembers(cachedFieldNames);
                }

                return cachedFields;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindFlags)
        {
            return thisExpando.GetMembers(bindFlags).Where(member => member.Name == name).ToArray();
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindFlags)
        {
            return thisExpando.GetFields(bindFlags).Cast<MemberInfo>().Concat(thisExpando.GetMethods(bindFlags)).Concat(thisExpando.GetProperties(bindFlags)).ToArray();
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindFlags)
        {
            var methods = thisExpando.GetMethods(bindFlags).Where(method => method.Name == name).ToArray();
            if (methods.Length < 1)
            {
                return null;
            }

            if (methods.Length > 1)
            {
                throw new AmbiguousMatchException(MiscHelpers.FormatInvariant("Object has multiple methods named '{0}'", name));
            }

            return methods[0];
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindFlags, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        MethodInfo[] IReflect.GetMethods(BindingFlags bindFlags)
        {
            return engine.HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdateMethodNames(out updated);
                if (updated || (cachedMethods == null))
                {
                    cachedMethods = methodMap.GetMembers(cachedMethodNames);
                }

                return cachedMethods;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindFlags)
        {
            return engine.HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdatePropertyNames(out updated);
                if (updated || (cachedProperties == null))
                {
                    cachedProperties = propertyMap.GetMembers(cachedPropertyNames);
                }

                return cachedProperties;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindFlags, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindFlags)
        {
            var properties = thisExpando.GetProperties(bindFlags).Where(property => property.Name == name).ToArray();
            if (properties.Length < 1)
            {
                return null;
            }

            if (properties.Length > 1)
            {
                throw new AmbiguousMatchException(MiscHelpers.FormatInvariant("Object has multiple properties named '{0}'", name));
            }

            return properties[0];
        }

        object IReflect.InvokeMember(string name, BindingFlags invokeFlags, Binder binder, object invokeTarget, object[] wrappedArgs, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParams)
        {
            return engine.MarshalToScript(engine.HostInvoke(() =>
            {
                var args = engine.MarshalToHost(wrappedArgs, false);

                var skipFirst = false;
                if ((namedParams != null) && (namedParams.Length > 0) && (namedParams[0] == SpecialParamNames.This))
                {
                    args = args.Skip(1).ToArray();
                    skipFirst = true;
                }

                var bindArgs = args;
                if (invokeFlags.HasFlag(BindingFlags.InvokeMethod) || invokeFlags.HasFlag(BindingFlags.CreateInstance))
                {
                    bindArgs = engine.MarshalToHost(wrappedArgs, true);
                    if (skipFirst)
                    {
                        bindArgs = bindArgs.Skip(1).ToArray();
                    }
                }

                return InvokeMember(name, invokeFlags, args, bindArgs, culture);
            }));
        }

        Type IReflect.UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IExpando implementation

        FieldInfo IExpando.AddField(string name)
        {
            return engine.HostInvoke(() =>
            {
                if ((targetDynamic != null) || ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly) || (targetDynamicMetaObject != null))
                {
                    AddExpandoMemberName(name);
                    return fieldMap.GetMember(name);
                }

                throw new NotSupportedException("Object does not support dynamic fields");
            });
        }

        PropertyInfo IExpando.AddProperty(string name)
        {
            return engine.HostInvoke(() =>
            {
                if ((targetDynamic != null) || ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly) || (targetDynamicMetaObject != null))
                {
                    AddExpandoMemberName(name);
                    return propertyMap.GetMember(name);
                }

                throw new NotSupportedException("Object does not support dynamic properties");
            });
        }

        MethodInfo IExpando.AddMethod(string name, Delegate method)
        {
            throw new NotImplementedException();
        }

        void IExpando.RemoveMember(MemberInfo member)
        {
            engine.HostInvoke(() =>
            {
                if (targetDynamic != null)
                {
                    int index;
                    if (int.TryParse(member.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                    {
                        if (targetDynamic.DeleteProperty(index))
                        {
                            RemoveExpandoMemberName(index.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else if (targetDynamic.DeleteProperty(member.Name))
                    {
                        RemoveExpandoMemberName(member.Name);
                    }
                }
                else if (targetPropertyBag != null)
                {
                    if (targetPropertyBag.Remove(member.Name))
                    {
                        RemoveExpandoMemberName(member.Name);
                    }
                }
                else if (targetDynamicMetaObject != null)
                {
                    bool result;
                    if (targetDynamicMetaObject.TryDeleteMember(member.Name, out result) && result)
                    {
                        RemoveExpandoMemberName(member.Name);
                    }
                }
                else
                {
                    throw new NotSupportedException("Object does not support dynamic members");
                }
            });
        }

        #endregion

        #region IDynamic implementation

        object IDynamic.GetProperty(string name)
        {
            return thisExpando.InvokeMember(name, BindingFlags.GetProperty, null, thisExpando, MiscHelpers.GetEmptyArray<object>(), null, CultureInfo.InvariantCulture, null);
        }

        void IDynamic.SetProperty(string name, object value)
        {
            thisExpando.InvokeMember(name, BindingFlags.SetProperty, null, thisExpando, new[] { value }, null, CultureInfo.InvariantCulture, null);
        }

        bool IDynamic.DeleteProperty(string name)
        {
            return engine.HostInvoke(() =>
            {
                if (targetDynamic != null)
                {
                    return targetDynamic.DeleteProperty(name);
                }

                if (targetPropertyBag != null)
                {
                    return targetPropertyBag.Remove(name);
                }

                if (targetDynamicMetaObject != null)
                {
                    bool result;
                    if (targetDynamicMetaObject.TryDeleteMember(name, out result))
                    {
                        return result;
                    }

                    throw new InvalidOperationException("Invalid dynamic member deletion");
                }

                throw new NotSupportedException("Object does not support dynamic members");
            });
        }

        string[] IDynamic.GetPropertyNames()
        {
            return engine.HostInvoke(() =>
            {
                bool updatedFieldNames;
                UpdateFieldNames(out updatedFieldNames);

                bool updatedMethodNames;
                UpdateMethodNames(out updatedMethodNames);

                bool updatedPropertyNames;
                UpdatePropertyNames(out updatedPropertyNames);

                if (updatedFieldNames || updatedMethodNames || updatedPropertyNames || (cachedMemberNames == null))
                {
                    cachedMemberNames = cachedFieldNames.Concat(cachedMethodNames).Concat(cachedPropertyNames).ExcludeIndices().Distinct().ToArray();
                }

                return cachedMemberNames;
            });
        }

        object IDynamic.GetProperty(int index)
        {
            return thisDynamic.GetProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        void IDynamic.SetProperty(int index, object value)
        {
            thisDynamic.SetProperty(index.ToString(CultureInfo.InvariantCulture), value);
        }

        bool IDynamic.DeleteProperty(int index)
        {
            return engine.HostInvoke(() =>
            {
                if (targetDynamic != null)
                {
                    return targetDynamic.DeleteProperty(index);
                }

                if (targetPropertyBag != null)
                {
                    return targetPropertyBag.Remove(index.ToString(CultureInfo.InvariantCulture));
                }

                if (targetDynamicMetaObject != null)
                {
                    bool result;
                    if (targetDynamicMetaObject.TryDeleteMember(index.ToString(CultureInfo.InvariantCulture), out result))
                    {
                        return result;
                    }

                    throw new InvalidOperationException("Invalid dynamic member deletion");
                }

                return false;
            });
        }

        int[] IDynamic.GetPropertyIndices()
        {
            return engine.HostInvoke(() =>
            {
                bool updated;
                UpdatePropertyNames(out updated);
                if (updated || (cachedPropertyIndices == null))
                {
                    cachedPropertyIndices = cachedPropertyNames.GetIndices().Distinct().ToArray();
                }

                return cachedPropertyIndices;
            });
        }

        object IDynamic.Invoke(object[] args, bool asConstructor)
        {
            return thisExpando.InvokeMember(SpecialMemberNames.Default, asConstructor ? BindingFlags.CreateInstance : BindingFlags.InvokeMethod, null, thisExpando, args, null, CultureInfo.InvariantCulture, null);
        }

        object IDynamic.InvokeMethod(string name, object[] args)
        {
            return thisExpando.InvokeMember(name, BindingFlags.InvokeMethod, null, thisExpando, args, null, CultureInfo.InvariantCulture, null);
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public object Unwrap()
        {
            return target.Target;
        }

        #endregion
    }
}
