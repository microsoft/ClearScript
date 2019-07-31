// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Expando;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal partial class HostItem : DynamicObject, IReflect, IDynamic, IEnumVARIANT, ICustomQueryInterface, IScriptMarshalWrapper, IHostInvokeContext
    {
        #region data

        private readonly ScriptEngine engine;
        private readonly HostTarget target;
        private readonly HostItemFlags flags;

        private Type accessContext;
        private ScriptAccess defaultAccess;
        private HostTargetMemberData targetMemberData;

        internal static bool EnableVTablePatching;
        [ThreadStatic] private static bool bypassVTablePatching;

        private static readonly PropertyInfo[] reflectionProperties =
        {
            typeof(Delegate).GetProperty("Method")
        };

        #endregion

        #region constructors

        private HostItem(ScriptEngine engine, HostTarget target, HostItemFlags flags)
        {
            this.engine = engine;
            this.target = target;
            this.flags = flags;

            BindSpecialTarget();
            BindTargetMemberData();

            var scriptableObject = target.Target as IScriptableObject;
            if (scriptableObject != null)
            {
                scriptableObject.OnExposedToScriptCode(engine);
            }
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
            if (obj == null)
            {
                return null;
            }

            var hostItem = obj as HostItem;
            if (hostItem != null)
            {
                obj = hostItem.Target;
            }

            var hostTarget = obj as HostTarget;
            if (hostTarget != null)
            {
                return BindOrCreate(engine, hostTarget, flags);
            }

            if (type == null)
            {
                type = obj.GetTypeOrTypeInfo();
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

        public delegate HostItem CreateFunc(ScriptEngine engine, HostTarget target, HostItemFlags flags);

        public HostTarget Target
        {
            get { return target; }
        }

        public HostItemFlags Flags
        {
            get { return flags; }
        }

        public Invocability Invocability
        {
            get
            {
                if (TargetInvocability == null)
                {
                    TargetInvocability = target.GetInvocability(GetCommonBindFlags(), accessContext, defaultAccess, flags.HasFlag(HostItemFlags.HideDynamicMembers));
                }

                return TargetInvocability.GetValueOrDefault();
            }
        }

        public object InvokeMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture, bool bypassTunneling)
        {
            bool isCacheable;
            return InvokeMember(name, invokeFlags, args, bindArgs, culture, bypassTunneling, out isCacheable);
        }

        public object InvokeMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture, bool bypassTunneling, out bool isCacheable)
        {
            AdjustInvokeFlags(ref invokeFlags);
            isCacheable = false;

            object result;
            if (target.TryInvokeAuxMember(this, name, invokeFlags, args, bindArgs, out result))
            {
                if (target is IHostVariable)
                {
                    // the variable may have been reassigned
                    BindSpecialTarget();
                }

                return result;
            }

            if (TargetDynamic != null)
            {
                return InvokeDynamicMember(name, invokeFlags, args);
            }

            if (TargetPropertyBag != null)
            {
                return InvokePropertyBagMember(name, invokeFlags, args, bindArgs);
            }

            if (TargetList != null)
            {
                int index;
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    return InvokeListElement(index, invokeFlags, args, bindArgs);
                }
            }

            if (!bypassTunneling)
            {
                int testLength;
                if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
                {
                    testLength = invokeFlags.HasFlag(BindingFlags.GetField) ? 0 : -1;
                }
                else
                {
                    testLength = invokeFlags.HasFlag(BindingFlags.SetField) ? 1 : 0;
                }

                if ((args.Length > testLength) && (name != SpecialMemberNames.Default))
                {
                    bool valueIsCacheable;
                    var value = GetHostProperty(name, GetCommonBindFlags(), ArrayHelpers.GetEmptyArray<object>(), ArrayHelpers.GetEmptyArray<object>(), culture, false, out valueIsCacheable);
                    if (!(value is Nonexistent))
                    {
                        var hostItem = engine.MarshalToScript(value) as HostItem;
                        if (hostItem != null)
                        {
                            return hostItem.InvokeMember(SpecialMemberNames.Default, invokeFlags, args, bindArgs, culture, true, out isCacheable);
                        }
                    }
                }
            }

            return InvokeHostMember(name, invokeFlags, args, bindArgs, culture, out isCacheable);
        }

        #endregion

        #region internal members

        #region interface accessors

        private IReflect ThisReflect
        {
            get { return this; }
        }

        private IDynamic ThisDynamic
        {
            get { return this; }
        }

        #endregion

        #region collateral accessors

        private IDynamic TargetDynamic
        {
            get { return Collateral.TargetDynamic.Get(this); }
        }

        private IPropertyBag TargetPropertyBag
        {
            get { return Collateral.TargetPropertyBag.Get(this); }
            set { Collateral.TargetPropertyBag.Set(this, value); }
        }

        private IHostList TargetList
        {
            get { return Collateral.TargetList.Get(this); }
            set { Collateral.TargetList.Set(this, value); }
        }

        private DynamicMetaObject TargetDynamicMetaObject
        {
            get { return Collateral.TargetDynamicMetaObject.Get(this); }
            set { Collateral.TargetDynamicMetaObject.Set(this, value); }
        }

        private IEnumerator TargetEnumerator
        {
            get { return Collateral.TargetEnumerator.Get(this); }
        }

        private HashSet<string> ExpandoMemberNames
        {
            get { return Collateral.ExpandoMemberNames.Get(this); }
            set { Collateral.ExpandoMemberNames.Set(this, value); }
        }

        private Dictionary<string, HostMethod> HostMethodMap
        {
            get { return Collateral.HostMethodMap.Get(this); }
            set { Collateral.HostMethodMap.Set(this, value); }
        }

        private Dictionary<string, HostIndexedProperty> HostIndexedPropertyMap
        {
            get { return Collateral.HostIndexedPropertyMap.Get(this); }
            set { Collateral.HostIndexedPropertyMap.Set(this, value); }
        }

        private int[] PropertyIndices
        {
            get { return Collateral.ListData.GetOrCreate(this).PropertyIndices; }
            set { Collateral.ListData.GetOrCreate(this).PropertyIndices = value; }
        }

        private int CachedListCount
        {
            get { return Collateral.ListData.GetOrCreate(this).CachedCount; }
            set { Collateral.ListData.GetOrCreate(this).CachedCount = value; }
        }

        private HostItemCollateral Collateral
        {
            get { return Engine.HostItemCollateral; }
        }

        #endregion

        #region target member data accessors

        private string[] TypeEventNames
        {
            get { return targetMemberData.TypeEventNames; }
            set { targetMemberData.TypeEventNames = value; }
        }

        private string[] TypeFieldNames
        {
            get { return targetMemberData.TypeFieldNames; }
            set { targetMemberData.TypeFieldNames = value; }
        }

        private string[] TypeMethodNames
        {
            get { return targetMemberData.TypeMethodNames; }
            set { targetMemberData.TypeMethodNames = value; }
        }

        private string[] TypePropertyNames
        {
            get { return targetMemberData.TypePropertyNames; }
            set { targetMemberData.TypePropertyNames = value; }
        }

        private string[] AllFieldNames
        {
            get { return targetMemberData.AllFieldNames; }
            set { targetMemberData.AllFieldNames = value; }
        }

        private string[] AllMethodNames
        {
            get { return targetMemberData.AllMethodNames; }
            set { targetMemberData.AllMethodNames = value; }
        }

        private string[] OwnMethodNames
        {
            get { return targetMemberData.OwnMethodNames; }
            set { targetMemberData.OwnMethodNames = value; }
        }

        private string[] EnumeratedMethodNames
        {
            get { return engine.EnumerateInstanceMethods ? (engine.EnumerateExtensionMethods ? AllMethodNames : OwnMethodNames) : ArrayHelpers.GetEmptyArray<string>(); }
        }

        private string[] AllPropertyNames
        {
            get { return targetMemberData.AllPropertyNames; }
            set { targetMemberData.AllPropertyNames = value; }
        }

        private string[] AllMemberNames
        {
            get { return targetMemberData.AllMemberNames; }
            set { targetMemberData.AllMemberNames = value; }
        }

        private FieldInfo[] AllFields
        {
            get { return targetMemberData.AllFields; }
            set { targetMemberData.AllFields = value; }
        }

        private MethodInfo[] AllMethods
        {
            get { return targetMemberData.AllMethods; }
            set { targetMemberData.AllMethods = value; }
        }

        private PropertyInfo[] AllProperties
        {
            get { return targetMemberData.AllProperties; }
            set { targetMemberData.AllProperties = value; }
        }

        private object EnumerationSettingsToken
        {
            get { return targetMemberData.EnumerationSettingsToken; }
            set { targetMemberData.EnumerationSettingsToken = value; }
        }

        private ExtensionMethodSummary ExtensionMethodSummary
        {
            get { return targetMemberData.ExtensionMethodSummary; }
            set { targetMemberData.ExtensionMethodSummary = value; }
        }

        private Invocability? TargetInvocability
        {
            get { return targetMemberData.TargetInvocability; }
            set { targetMemberData.TargetInvocability = value; }
        }

        #endregion

        #region initialization

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
            return TargetSupportsExpandoMembers(target, flags) ? new ExpandoHostItem(engine, target, flags) : new HostItem(engine, target, flags);
        }

        private void BindSpecialTarget()
        {
            if (TargetSupportsSpecialTargets(target))
            {
                if (BindSpecialTarget(Collateral.TargetDynamic))
                {
                    TargetPropertyBag = null;
                    TargetList = null;
                    TargetDynamicMetaObject = null;
                }
                else if (BindSpecialTarget(Collateral.TargetPropertyBag))
                {
                    TargetList = null;
                    TargetDynamicMetaObject = null;
                }
                else
                {
                    IDynamicMetaObjectProvider dynamicMetaObjectProvider;
                    if (!flags.HasFlag(HostItemFlags.HideDynamicMembers) && BindSpecialTarget(out dynamicMetaObjectProvider))
                    {
                        TargetDynamicMetaObject = dynamicMetaObjectProvider.GetMetaObject(Expression.Constant(target.InvokeTarget));
                        TargetList = null;
                    }
                    else
                    {
                        TargetDynamicMetaObject = null;
                        BindSpecialTarget(Collateral.TargetList);
                    }
                }
            }
        }

        private bool BindSpecialTarget<T>(CollateralObject<HostItem, T> property) where T : class
        {
            T value;
            if (BindSpecialTarget(out value))
            {
                property.Set(this, value);
                return true;
            }

            property.Clear(this);
            return false;
        }

        private bool BindSpecialTarget<T>(out T specialTarget) where T : class
        {
            if (target.InvokeTarget == null)
            {
                specialTarget = null;
                return false;
            }

            if (typeof(T) == typeof(IDynamic))
            {
                // provide fully dynamic behavior for exposed IDispatchEx implementations

                var dispatchEx = target.InvokeTarget as IDispatchEx;
                if ((dispatchEx != null) && dispatchEx.GetType().IsCOMObject)
                {
                    specialTarget = (T)(object)(new DynamicDispatchExWrapper(dispatchEx));
                    return true;
                }
            }
            else if (typeof(T) == typeof(IHostList))
            {
                // generic list support

                Type[] typeArgs;
                if (target.Type.IsAssignableToGenericType(typeof(IList<>), out typeArgs))
                {
                    if (typeof(IList).IsAssignableFrom(target.Type))
                    {
                        specialTarget = new HostList(engine, (IList)target.InvokeTarget, typeArgs[0]) as T;
                        return specialTarget != null;
                    }

                    specialTarget = typeof(HostList<>).MakeGenericType(typeArgs).CreateInstance(engine, target.InvokeTarget) as T;
                    return specialTarget != null;
                }

                if (typeof(IList).IsAssignableFrom(target.Type))
                {
                    specialTarget = new HostList(engine, (IList)target.InvokeTarget, typeof(object)) as T;
                    return specialTarget != null;
                }

                specialTarget = null;
                return false;
            }

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

        private void BindTargetMemberData()
        {
            var newAccessContext = flags.HasFlag(HostItemFlags.PrivateAccess) || (target.Type.IsAnonymous() && !engine.EnforceAnonymousTypeAccess) ? target.Type : engine.AccessContext;
            var newDefaultAccess = engine.DefaultAccess;

            if ((targetMemberData == null) || (accessContext != newAccessContext) || (defaultAccess != newDefaultAccess))
            {
                accessContext = newAccessContext;
                defaultAccess = newDefaultAccess;

                if (target is HostMethod)
                {
                    // host methods can share their (dummy) member data
                    targetMemberData = engine.SharedHostMethodMemberData;
                    return;
                }

                if (target is HostIndexedProperty)
                {
                    // host indexed properties can share their (dummy) member data
                    targetMemberData = engine.SharedHostIndexedPropertyMemberData;
                    return;
                }

                if (target is ScriptMethod)
                {
                    // script methods can share their (dummy) member data
                    targetMemberData = engine.SharedScriptMethodMemberData;
                    return;
                }

                var hostObject = target as HostObject;
                if (hostObject != null)
                {
                    if ((TargetDynamic == null) && (TargetPropertyBag == null) && (TargetList == null) && (TargetDynamicMetaObject == null))
                    {
                        // host objects without dynamic members can share their member data
                        targetMemberData = engine.GetSharedHostObjectMemberData(hostObject, accessContext, defaultAccess);
                        return;
                    }
                }

                // all other targets use unique member data
                targetMemberData = new HostTargetMemberData();
            }
        }

        private static bool TargetSupportsSpecialTargets(HostTarget target)
        {
            return (target is HostObject) || (target is IHostVariable) || (target is IByRefArg);
        }

        private static bool TargetSupportsExpandoMembers(HostTarget target, HostItemFlags flags)
        {
            if (!TargetSupportsSpecialTargets(target))
            {
                return false;
            }

            if (typeof(IDynamic).IsAssignableFrom(target.Type))
            {
                return true;
            }

            if (target is IHostVariable)
            {
                if (target.Type.IsImport)
                {
                    return true;
                }
            }
            else
            {
                var dispatchEx = target.InvokeTarget as IDispatchEx;
                if ((dispatchEx != null) && dispatchEx.GetType().IsCOMObject)
                {
                    return true;
                }
            }

            if (typeof(IPropertyBag).IsAssignableFrom(target.Type))
            {
                return true;
            }

            if (!flags.HasFlag(HostItemFlags.HideDynamicMembers) && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(target.Type))
            {
                return true;
            }

            return false;
        }

        private bool CanAddExpandoMembers()
        {
            return (TargetDynamic != null) || ((TargetPropertyBag != null) && !TargetPropertyBag.IsReadOnly) || (TargetDynamicMetaObject != null);
        }

        #endregion

        #region member data maintenance

        private string[] GetLocalEventNames()
        {
            if (TypeEventNames == null)
            {
                var localEvents = target.Type.GetScriptableEvents(GetCommonBindFlags(), accessContext, defaultAccess);
                TypeEventNames = localEvents.Select(eventInfo => eventInfo.GetScriptName()).ToArray();
            }

            return TypeEventNames;
        }

        private string[] GetLocalFieldNames()
        {
            if (TypeFieldNames == null)
            {
                var localFields = target.Type.GetScriptableFields(GetCommonBindFlags(), accessContext, defaultAccess);
                TypeFieldNames = localFields.Select(field => field.GetScriptName()).ToArray();
            }

            return TypeFieldNames;
        }

        private string[] GetLocalMethodNames()
        {
            if (TypeMethodNames == null)
            {
                var localMethods = target.Type.GetScriptableMethods(GetMethodBindFlags(), accessContext, defaultAccess);
                TypeMethodNames = localMethods.Select(method => method.GetScriptName()).ToArray();
            }

            return TypeMethodNames;
        }

        private string[] GetLocalPropertyNames()
        {
            if (TypePropertyNames == null)
            {
                var localProperties = target.Type.GetScriptableProperties(GetCommonBindFlags(), accessContext, defaultAccess);
                TypePropertyNames = localProperties.Select(property => property.GetScriptName()).ToArray();
            }

            return TypePropertyNames;
        }

        private string[] GetAllFieldNames()
        {
            if ((TargetDynamic == null) && (TargetPropertyBag == null))
            {
                return GetLocalFieldNames().Concat(GetLocalEventNames()).Distinct().ToArray();
            }

            return ArrayHelpers.GetEmptyArray<string>();
        }

        private string[] GetAllMethodNames(out string[] ownMethodNames)
        {
            ownMethodNames = null;

            var names = target.GetAuxMethodNames(this, GetMethodBindFlags()).AsEnumerable();
            if ((TargetDynamic == null) && (TargetPropertyBag == null))
            {
                names = names.Concat(GetLocalMethodNames());
                if (target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
                {
                    var extensionMethodSummary = engine.ExtensionMethodSummary;
                    ExtensionMethodSummary = extensionMethodSummary;

                    var extensionMethodNames = extensionMethodSummary.MethodNames;
                    if (extensionMethodNames.Length > 0)
                    {
                        ownMethodNames = names.Distinct().ToArray();
                        names = ownMethodNames.Concat(extensionMethodNames);
                    }
                }
            }

            var result = names.Distinct().ToArray();
            if (ownMethodNames == null)
            {
                ownMethodNames = result;
            }

            return result;
        }

        private string[] GetAllPropertyNames()
        {
            var names = target.GetAuxPropertyNames(this, GetCommonBindFlags()).AsEnumerable();
            if (TargetDynamic != null)
            {
                names = names.Concat(TargetDynamic.GetPropertyNames());
                names = names.Concat(TargetDynamic.GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture)));
            }
            else if (TargetPropertyBag != null)
            {
                names = names.Concat(TargetPropertyBag.Keys);
            }
            else
            {
                names = names.Concat(GetLocalPropertyNames());

                if (TargetList != null)
                {
                    CachedListCount = TargetList.Count;
                    if (CachedListCount > 0)
                    {
                        names = names.Concat(Enumerable.Range(0, CachedListCount).Select(index => index.ToString(CultureInfo.InvariantCulture)));
                    }
                }

                if (TargetDynamicMetaObject != null)
                {
                    names = names.Concat(TargetDynamicMetaObject.GetDynamicMemberNames());
                }
            }

            if (ExpandoMemberNames != null)
            {
                names = names.Except(ExpandoMemberNames);
            }

            return names.Distinct().ToArray();
        }

        private void UpdateFieldNames(out bool updated)
        {
            if (AllFieldNames == null)
            {
                AllFieldNames = GetAllFieldNames();
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void UpdateMethodNames(out bool updated)
        {
            if ((AllMethodNames == null) ||
                (target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods) && (ExtensionMethodSummary != engine.ExtensionMethodSummary)))
            {
                string[] ownMethodNames;
                AllMethodNames = GetAllMethodNames(out ownMethodNames);
                OwnMethodNames = ownMethodNames;
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void UpdatePropertyNames(out bool updated)
        {
            if ((AllPropertyNames == null) ||
                (TargetDynamic != null) ||
                (TargetPropertyBag != null) ||
                (TargetDynamicMetaObject != null) ||
                ((TargetList != null) && (CachedListCount != TargetList.Count)))
            {
                AllPropertyNames = GetAllPropertyNames();
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void UpdateEnumerationSettingsToken(out bool updated)
        {
            var enumerationSettingsToken = engine.EnumerationSettingsToken;
            if (EnumerationSettingsToken != enumerationSettingsToken)
            {
                EnumerationSettingsToken = enumerationSettingsToken;
                updated = true;
            }
            else
            {
                updated = false;
            }
        }

        private void AddExpandoMemberName(string name)
        {
            if (ExpandoMemberNames == null)
            {
                ExpandoMemberNames = new HashSet<string>();
            }

            ExpandoMemberNames.Add(name);
        }

        private void RemoveExpandoMemberName(string name)
        {
            if (ExpandoMemberNames != null)
            {
                ExpandoMemberNames.Remove(name);
            }
        }

        #endregion

        #region member invocation

        private void HostInvoke(Action action)
        {
            BindTargetMemberData();
            engine.HostInvoke(action);
        }

        private T HostInvoke<T>(Func<T> func)
        {
            BindTargetMemberData();
            return engine.HostInvoke(func);
        }

        private BindingFlags GetCommonBindFlags()
        {
            var bindFlags = BindingFlags.Public | BindingFlags.NonPublic;

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
                BindingFlags.NonPublic |
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

        private object InvokeReflectMember(string name, BindingFlags invokeFlags, object[] wrappedArgs, CultureInfo culture, string[] namedParams)
        {
            bool isCacheable;
            return InvokeReflectMember(name, invokeFlags, wrappedArgs, culture, namedParams, out isCacheable);
        }

        private object InvokeReflectMember(string name, BindingFlags invokeFlags, object[] wrappedArgs, CultureInfo culture, string[] namedParams, out bool isCacheable)
        {
            var resultIsCacheable = false;
            var result = engine.MarshalToScript(HostInvoke(() =>
            {
                var args = engine.MarshalToHost(wrappedArgs, false);

                var argOffset = 0;
                if ((namedParams != null) && (namedParams.Length > 0) && (namedParams[0] == SpecialParamNames.This))
                {
                    args = args.Skip(1).ToArray();
                    argOffset = 1;
                }

                var bindArgs = args;
                if ((args.Length > 0) && (invokeFlags.HasFlag(BindingFlags.InvokeMethod) || invokeFlags.HasFlag(BindingFlags.CreateInstance)))
                {
                    bindArgs = engine.MarshalToHost(wrappedArgs, true);
                    if (argOffset > 0)
                    {
                        bindArgs = bindArgs.Skip(argOffset).ToArray();
                    }

                    var savedArgs = (object[])args.Clone();
                    var tempResult = InvokeMember(name, invokeFlags, args, bindArgs, culture, false, out resultIsCacheable);

                    for (var index = 0; index < args.Length; index++)
                    {
                        var arg = args[index];
                        if (!ReferenceEquals(arg, savedArgs[index]))
                        {
                            wrappedArgs[argOffset + index] = engine.MarshalToScript(arg);
                        }
                    }

                    return tempResult;
                }

                return InvokeMember(name, invokeFlags, args, bindArgs, culture, false, out resultIsCacheable);
            }));

            isCacheable = resultIsCacheable;
            return result;
        }

        private object InvokeDynamicMember(string name, BindingFlags invokeFlags, object[] args)
        {
            if (invokeFlags.HasFlag(BindingFlags.CreateInstance))
            {
                if (name == SpecialMemberNames.Default)
                {
                    return TargetDynamic.Invoke(true, args);
                }

                throw new InvalidOperationException("Invalid constructor invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    try
                    {
                        return TargetDynamic.Invoke(false, args);
                    }
                    catch (Exception)
                    {
                        if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                        {
                            return TargetDynamic;
                        }

                        throw;
                    }
                }

                try
                {
                    return TargetDynamic.InvokeMethod(name, args);
                }
                catch (Exception)
                {
                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        return TargetDynamic.GetProperty(name, args);
                    }

                    throw;
                }
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return TargetDynamic.GetProperty(name, args);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length > 0)
                {
                    TargetDynamic.SetProperty(name, args);
                    return args[args.Length - 1];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokePropertyBagMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                object value;

                if (name == SpecialMemberNames.Default)
                {
                    if (invokeFlags.HasFlag(BindingFlags.GetField))
                    {
                        if (args.Length < 1)
                        {
                            return TargetPropertyBag;
                        }

                        if (args.Length == 1)
                        {
                            return TargetPropertyBag.TryGetValue(Convert.ToString(args[0]), out value) ? value : Nonexistent.Value;
                        }
                    }

                    throw new NotSupportedException("Object does not support the requested invocation operation");
                }

                if (name == SpecialMemberNames.NewEnum)
                {
                    return HostObject.Wrap(TargetPropertyBag.GetEnumerator(), typeof(IEnumerator));
                }

                if (!TargetPropertyBag.TryGetValue(name, out value))
                {
                    throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no property named '{0}'", name));
                }

                object result;
                if (InvokeHelpers.TryInvokeObject(this, value, invokeFlags, args, bindArgs, true, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    if (args.Length < 1)
                    {
                        return value;
                    }

                    if (args.Length == 1)
                    {
                        if (value == null)
                        {
                            throw new InvalidOperationException("Cannot invoke a null property value");
                        }

                        return ((HostItem)Wrap(engine, value)).InvokeMember(SpecialMemberNames.Default, invokeFlags, args, bindArgs, null, true);
                    }
                }

                throw new NotSupportedException("Object does not support the requested invocation operation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                if (name == SpecialMemberNames.Default)
                {
                    if (args.Length == 1)
                    {
                        return TargetPropertyBag[Convert.ToString(args[0])];
                    }

                    throw new InvalidOperationException("Invalid argument count");
                }

                if (args.Length < 1)
                {
                    object value;
                    return TargetPropertyBag.TryGetValue(name, out value) ? value : Nonexistent.Value;
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (name == SpecialMemberNames.Default)
                {
                    if (args.Length == 2)
                    {
                        return TargetPropertyBag[Convert.ToString(args[0])] = args[1];
                    }

                    throw new InvalidOperationException("Invalid argument count");
                }

                if (args.Length == 1)
                {
                    return TargetPropertyBag[name] = args[0];
                }

                if (args.Length == 2)
                {
                    object value;
                    if (TargetPropertyBag.TryGetValue(name, out value))
                    {
                        if (value == null)
                        {
                            throw new InvalidOperationException("Cannot invoke a null property value");
                        }

                        return ((HostItem)Wrap(engine, value)).InvokeMember(SpecialMemberNames.Default, invokeFlags, args, bindArgs, null, true);
                    }

                    throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no property named '{0}'", name));
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
                if (InvokeHelpers.TryInvokeObject(this, TargetList[index], invokeFlags, args, bindArgs, true, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                {
                    return TargetList[index];
                }

                throw new NotSupportedException("Object does not support the requested invocation operation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                if (args.Length < 1)
                {
                    return TargetList[index];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length == 1)
                {
                    return TargetList[index] = args[0];
                }

                throw new InvalidOperationException("Invalid argument count");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokeHostMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture, out bool isCacheable)
        {
            isCacheable = false;
            object result;

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

                            if (hostType.TryInvoke(this, BindingFlags.InvokeMethod, typeArgs, typeArgs, out result))
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

                                    return specificType.CreateInstance(accessContext, defaultAccess, args);
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

                        return type.CreateInstance(accessContext, defaultAccess, args);
                    }

                    if (TargetDynamicMetaObject != null)
                    {
                        if (TargetDynamicMetaObject.TryCreateInstance(args, out result))
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
                    if (InvokeHelpers.TryInvokeObject(this, target, invokeFlags, args, bindArgs, TargetDynamicMetaObject != null, out result))
                    {
                        return result;
                    }

                    if (invokeFlags.HasFlag(BindingFlags.GetField))
                    {
                        result = GetHostProperty(name, invokeFlags, args, bindArgs, culture, true, out isCacheable);
                        if (!(result is Nonexistent))
                        {
                            return result;
                        }

                        if (args.Length < 1)
                        {
                            return target;
                        }

                        if (TargetDynamicMetaObject != null)
                        {
                            // dynamic target; don't throw for default indexed property retrieval failure

                            return result;
                        }
                    }

                    throw new NotSupportedException("Object does not support the requested invocation operation");
                }

                if (name == SpecialMemberNames.NewEnum)
                {
                    if ((target is HostObject) || (target is IHostVariable) || (target is IByRefArg))
                    {
                        IEnumerable enumerable;
                        if (BindSpecialTarget(out enumerable))
                        {
                            var enumerableHelpersHostItem = Wrap(engine, EnumerableHelpers.HostType, HostItemFlags.PrivateAccess);
                            try
                            {
                                return ((IDynamic)enumerableHelpersHostItem).InvokeMethod("GetEnumerator", this);
                            }
                            catch (MissingMemberException)
                            {
                            }
                        }
                    }

                    throw new NotSupportedException("Object is not enumerable");
                }

                if ((TargetDynamicMetaObject != null) && (TargetDynamicMetaObject.GetDynamicMemberNames().Contains(name)))
                {
                    if (TargetDynamicMetaObject.TryInvokeMember(this, name, invokeFlags, args, out result))
                    {
                        return result;
                    }
                }

                if (ThisReflect.GetMethods(GetMethodBindFlags()).Any(method => method.Name == name))
                {
                    // The target appears to have a method with the right name, but it could be an
                    // extension method that fails to bind. If that happens, we should attempt the
                    // fallback but throw the original exception if the fallback fails as well.

                    try
                    {
                        return InvokeMethod(name, args, bindArgs);
                    }
                    catch (TargetInvocationException)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        // ReSharper disable EmptyGeneralCatchClause

                        try
                        {
                            if (invokeFlags.HasFlag(BindingFlags.GetField))
                            {
                                return GetHostProperty(name, invokeFlags, args, bindArgs, culture, true, out isCacheable);
                            }

                        }
                        catch (TargetInvocationException)
                        {
                            throw;
                        }
                        catch (Exception)
                        {
                        }
                        
                        throw;

                        // ReSharper restore EmptyGeneralCatchClause
                    }
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    return GetHostProperty(name, invokeFlags, args, bindArgs, culture, true, out isCacheable);
                }

                throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no suitable method named '{0}'", name));
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return GetHostProperty(name, invokeFlags, args, bindArgs, culture, true, out isCacheable);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                return SetHostProperty(name, invokeFlags, args, bindArgs, culture);
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object GetHostProperty(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture, bool includeBoundMembers, out bool isCacheable)
        {
            isCacheable = false;

            if (name == SpecialMemberNames.Default)
            {
                var defaultProperty = target.Type.GetScriptableDefaultProperty(invokeFlags, bindArgs, accessContext, defaultAccess);
                if (defaultProperty != null)
                {
                    return GetHostProperty(defaultProperty, invokeFlags, args, culture);
                }

                if (TargetDynamicMetaObject != null)
                {
                    object result;
                    if (TargetDynamicMetaObject.TryGetIndex(args, out result))
                    {
                        return result;
                    }
                }

                return Nonexistent.Value;
            }

            if ((TargetDynamicMetaObject != null) && (args.Length < 1))
            {
                int index;
                object result;

                if (TargetDynamicMetaObject.GetDynamicMemberNames().Contains(name))
                {
                    if (TargetDynamicMetaObject.TryGetMember(name, out result))
                    {
                        return result;
                    }

                    if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                    {
                        if (TargetDynamicMetaObject.TryGetIndex(new object[] { index }, out result))
                        {
                            return result;
                        }
                    }

                    if (TargetDynamicMetaObject.TryGetIndex(new object[] { name }, out result))
                    {
                        return result;
                    }

                    if (includeBoundMembers)
                    {
                        if (HostMethodMap == null)
                        {
                            HostMethodMap = new Dictionary<string, HostMethod>();
                        }

                        HostMethod hostMethod;
                        if (!HostMethodMap.TryGetValue(name, out hostMethod))
                        {
                            hostMethod = new HostMethod(this, name);
                            HostMethodMap.Add(name, hostMethod);
                        }

                        return hostMethod;
                    }

                    return Nonexistent.Value;
                }

                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    if (TargetDynamicMetaObject.TryGetIndex(new object[] { index }, out result))
                    {
                        return result;
                    }
                }

                if (TargetDynamicMetaObject.TryGetIndex(new object[] { name }, out result))
                {
                    return result;
                }
            }

            var property = target.Type.GetScriptableProperty(name, invokeFlags, bindArgs, accessContext, defaultAccess);
            if (property != null)
            {
                return GetHostProperty(property, invokeFlags, args, culture);
            }

            if (args.Length > 0)
            {
                throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no suitable property named '{0}'", name));
            }

            var eventInfo = target.Type.GetScriptableEvent(name, invokeFlags, accessContext, defaultAccess);
            if (eventInfo != null)
            {
                var type = typeof(EventSource<>).MakeSpecificType(eventInfo.EventHandlerType);
                isCacheable = (TargetDynamicMetaObject == null);
                return type.CreateInstance(BindingFlags.NonPublic, engine, target.InvokeTarget, eventInfo);
            }

            var field = target.Type.GetScriptableField(name, invokeFlags, accessContext, defaultAccess);
            if (field != null)
            {
                var result = field.GetValue(target.InvokeTarget);
                isCacheable = (TargetDynamicMetaObject == null) && (field.IsLiteral || field.IsInitOnly);
                return engine.PrepareResult(result, field.FieldType, field.GetScriptMemberFlags(), false);
            }

            if (includeBoundMembers)
            {
                if (target.Type.GetScriptableProperties(name, invokeFlags, accessContext, defaultAccess).Any())
                {
                    if (HostIndexedPropertyMap == null)
                    {
                        HostIndexedPropertyMap = new Dictionary<string, HostIndexedProperty>();
                    }

                    HostIndexedProperty hostIndexedProperty;
                    if (!HostIndexedPropertyMap.TryGetValue(name, out hostIndexedProperty))
                    {
                        hostIndexedProperty = new HostIndexedProperty(this, name);
                        HostIndexedPropertyMap.Add(name, hostIndexedProperty);
                    }

                    return hostIndexedProperty;
                }

                var method = ThisReflect.GetMethods(GetMethodBindFlags()).FirstOrDefault(testMethod => testMethod.Name == name);
                if (method != null)
                {
                    if (HostMethodMap == null)
                    {
                        HostMethodMap = new Dictionary<string, HostMethod>();
                    }

                    HostMethod hostMethod;
                    if (!HostMethodMap.TryGetValue(name, out hostMethod))
                    {
                        hostMethod = new HostMethod(this, name);
                        HostMethodMap.Add(name, hostMethod);
                    }

                    isCacheable = (TargetDynamicMetaObject == null);
                    return hostMethod;
                }
            }

            return Nonexistent.Value;
        }

        private object GetHostProperty(PropertyInfo property, BindingFlags invokeFlags, object[] args, CultureInfo culture)
        {
            if (reflectionProperties.Contains(property, MemberComparer<PropertyInfo>.Instance))
            {
                engine.CheckReflection();
            }

            var getMethod = property.GetMethod;
            if ((getMethod == null) || !getMethod.IsAccessible(accessContext) || getMethod.IsBlockedFromScript(defaultAccess, false))
            {
                throw new UnauthorizedAccessException("Property get method is unavailable or inaccessible");
            }

            var result = property.GetValue(target.InvokeTarget, invokeFlags, Type.DefaultBinder, args, culture);
            return engine.PrepareResult(result, property.PropertyType, property.GetScriptMemberFlags(), false);
        }

        private object SetHostProperty(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, CultureInfo culture)
        {
            if (name == SpecialMemberNames.Default)
            {
                if (args.Length < 2)
                {
                    throw new InvalidOperationException("Invalid argument count");
                }

                object result;

                var defaultProperty = target.Type.GetScriptableDefaultProperty(invokeFlags, bindArgs.Take(bindArgs.Length - 1).ToArray(), accessContext, defaultAccess);
                if (defaultProperty != null)
                {
                    return SetHostProperty(defaultProperty, invokeFlags, args, culture);
                }

                if (TargetDynamicMetaObject != null)
                {
                    if (TargetDynamicMetaObject.TrySetIndex(args.Take(args.Length - 1).ToArray(), args[args.Length - 1], out result))
                    {
                        return result;
                    }
                }

                // special case to enable JScript/VBScript "x(a) = b" syntax when x is a host indexed property 

                if (InvokeHelpers.TryInvokeObject(this, target, invokeFlags, args, bindArgs, false, out result))
                {
                    return result;
                }

                throw new InvalidOperationException("Invalid property assignment");
            }

            if ((TargetDynamicMetaObject != null) && (args.Length == 1))
            {
                object result;

                if (TargetDynamicMetaObject.TrySetMember(name, args[0], out result))
                {
                    return result;
                }

                int index;
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    if (TargetDynamicMetaObject.TrySetIndex(new object[] { index }, args[0], out result))
                    {
                        return result;
                    }
                }

                if (TargetDynamicMetaObject.TrySetIndex(new object[] { name }, args[0], out result))
                {
                    return result;
                }
            }

            if (args.Length < 1)
            {
                throw new InvalidOperationException("Invalid argument count");
            }

            var property = target.Type.GetScriptableProperty(name, invokeFlags, bindArgs.Take(bindArgs.Length - 1).ToArray(), accessContext, defaultAccess);
            if (property != null)
            {
                return SetHostProperty(property, invokeFlags, args, culture);
            }

            var field = target.Type.GetScriptableField(name, invokeFlags, accessContext, defaultAccess);
            if (field != null)
            {
                if (args.Length == 1)
                {
                    if (field.IsLiteral || field.IsInitOnly || field.IsReadOnlyForScript(defaultAccess))
                    {
                        throw new UnauthorizedAccessException("Field is read-only");
                    }

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

        private object SetHostProperty(PropertyInfo property, BindingFlags invokeFlags, object[] args, CultureInfo culture)
        {
            if (property.IsReadOnlyForScript(defaultAccess))
            {
                throw new UnauthorizedAccessException("Property is read-only");
            }

            var setMethod = property.SetMethod;
            if ((setMethod == null) || !setMethod.IsAccessible(accessContext) || setMethod.IsBlockedFromScript(defaultAccess, false))
            {
                throw new UnauthorizedAccessException("Property set method is unavailable or inaccessible");
            }

            var value = args[args.Length - 1];
            if (property.PropertyType.IsAssignableFrom(ref value))
            {
                property.SetValue(target.InvokeTarget, value, invokeFlags, Type.DefaultBinder, args.Take(args.Length - 1).ToArray(), culture);
                return value;
            }

            // Some COM properties have setters where the final parameter type doesn't match
            // the property type. The latter has failed, so let's try the former.

            var parameters = setMethod.GetParameters();
            if ((parameters.Length == args.Length) && (parameters[args.Length - 1].ParameterType.IsAssignableFrom(ref value)))
            {
                property.SetValue(target.InvokeTarget, value, invokeFlags, Type.DefaultBinder, args.Take(args.Length - 1).ToArray(), culture);
                return value;
            }

            throw new ArgumentException("Invalid property assignment");
        }

        #endregion

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
            result = ThisDynamic.Invoke(true, args).ToDynamicResult(engine);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = ThisDynamic.GetProperty(binder.Name, ArrayHelpers.GetEmptyArray<object>()).ToDynamicResult(engine);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ThisDynamic.SetProperty(binder.Name, value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            if (indices.Length == 1)
            {
                int index;
                if (MiscHelpers.TryGetNumericIndex(indices[0], out index))
                {
                    result = ThisDynamic.GetProperty(index).ToDynamicResult(engine);
                    return true;
                }

                result = ThisDynamic.GetProperty(indices[0].ToString(), ArrayHelpers.GetEmptyArray<object>()).ToDynamicResult(engine);
                return true;
            }

            if (indices.Length > 1)
            {
                result = ThisDynamic.GetProperty(SpecialMemberNames.Default, indices).ToDynamicResult(engine);
                return true;
            }

            throw new InvalidOperationException("Invalid argument or index count");
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indices, object value)
        {
            if (indices.Length == 1)
            {
                int index;
                if (MiscHelpers.TryGetNumericIndex(indices[0], out index))
                {
                    ThisDynamic.SetProperty(index, value);
                    return true;
                }

                ThisDynamic.SetProperty(indices[0].ToString(), value);
                return true;
            }

            if (indices.Length > 1)
            {
                ThisDynamic.SetProperty(SpecialMemberNames.Default, indices.Concat(value.ToEnumerable()).ToArray());
            }

            throw new InvalidOperationException("Invalid argument or index count");
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = ThisDynamic.Invoke(false, args).ToDynamicResult(engine);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = ThisDynamic.InvokeMethod(binder.Name, args).ToDynamicResult(engine);
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
            var fields = ThisReflect.GetFields(bindFlags).Where(field => field.Name == name).ToArray();
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
            return HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdateFieldNames(out updated);
                if (updated || (AllFields == null))
                {
                    AllFields = MemberMap.GetFields(AllFieldNames);
                }

                return AllFields;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        MemberInfo[] IReflect.GetMember(string name, BindingFlags bindFlags)
        {
            return ThisReflect.GetMembers(bindFlags).Where(member => member.Name == name).ToArray();
        }

        MemberInfo[] IReflect.GetMembers(BindingFlags bindFlags)
        {
            return ThisReflect.GetFields(bindFlags).Cast<MemberInfo>().Concat(ThisReflect.GetMethods(bindFlags)).Concat(ThisReflect.GetProperties(bindFlags)).ToArray();
        }

        MethodInfo IReflect.GetMethod(string name, BindingFlags bindFlags)
        {
            var methods = ThisReflect.GetMethods(bindFlags).Where(method => method.Name == name).ToArray();
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
            return HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdateMethodNames(out updated);
                if (updated || (AllMethods == null))
                {
                    AllMethods = MemberMap.GetMethods(AllMethodNames);
                }

                return AllMethods;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        PropertyInfo[] IReflect.GetProperties(BindingFlags bindFlags)
        {
            return HostInvoke(() =>
            {
                // ReSharper disable CoVariantArrayConversion

                bool updated;
                UpdatePropertyNames(out updated);
                if (updated || (AllProperties == null))
                {
                    AllProperties = MemberMap.GetProperties(AllPropertyNames);
                }

                return AllProperties;

                // ReSharper restore CoVariantArrayConversion
            });
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindFlags, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        PropertyInfo IReflect.GetProperty(string name, BindingFlags bindFlags)
        {
            var properties = ThisReflect.GetProperties(bindFlags).Where(property => property.Name == name).ToArray();
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
            return InvokeReflectMember(name, invokeFlags, wrappedArgs, culture, namedParams);
        }

        Type IReflect.UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDynamic implementation

        object IDynamic.GetProperty(string name, params object[] args)
        {
            return InvokeReflectMember(name, BindingFlags.GetProperty, args, CultureInfo.InvariantCulture, null);
        }

        object IDynamic.GetProperty(string name, out bool isCacheable, params object[] args)
        {
            return InvokeReflectMember(name, BindingFlags.GetProperty, args, CultureInfo.InvariantCulture, null, out isCacheable);
        }

        void IDynamic.SetProperty(string name, object[] args)
        {
            ThisReflect.InvokeMember(name, BindingFlags.SetProperty, null, ThisReflect, args, null, CultureInfo.InvariantCulture, null);
        }

        bool IDynamic.DeleteProperty(string name)
        {
            return HostInvoke(() =>
            {
                if (TargetDynamic != null)
                {
                    return TargetDynamic.DeleteProperty(name);
                }

                if (TargetPropertyBag != null)
                {
                    return TargetPropertyBag.Remove(name);
                }

                if (TargetDynamicMetaObject != null)
                {
                    bool result;
                    if (TargetDynamicMetaObject.TryDeleteMember(name, out result) && result)
                    {
                        return true;
                    }

                    if (TargetDynamicMetaObject.TryDeleteIndex(new object[] { name }, out result))
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
            return HostInvoke(() =>
            {
                bool updatedFieldNames;
                UpdateFieldNames(out updatedFieldNames);

                bool updatedMethodNames;
                UpdateMethodNames(out updatedMethodNames);

                bool updatedPropertyNames;
                UpdatePropertyNames(out updatedPropertyNames);

                bool updatedEnumerationSettingsToken;
                UpdateEnumerationSettingsToken(out updatedEnumerationSettingsToken);

                if (updatedFieldNames || updatedMethodNames || updatedPropertyNames || updatedEnumerationSettingsToken || (AllMemberNames == null))
                {
                    AllMemberNames = AllFieldNames.Concat(EnumeratedMethodNames).Concat(AllPropertyNames).ExcludeIndices().Distinct().ToArray();
                }

                return AllMemberNames;
            });
        }

        object IDynamic.GetProperty(int index)
        {
            return ThisDynamic.GetProperty(index.ToString(CultureInfo.InvariantCulture), ArrayHelpers.GetEmptyArray<object>());
        }

        void IDynamic.SetProperty(int index, object value)
        {
            ThisDynamic.SetProperty(index.ToString(CultureInfo.InvariantCulture), value);
        }

        bool IDynamic.DeleteProperty(int index)
        {
            return HostInvoke(() =>
            {
                if (TargetDynamic != null)
                {
                    return TargetDynamic.DeleteProperty(index);
                }

                if (TargetPropertyBag != null)
                {
                    return TargetPropertyBag.Remove(index.ToString(CultureInfo.InvariantCulture));
                }

                if (TargetDynamicMetaObject != null)
                {
                    bool result;

                    if (TargetDynamicMetaObject.TryDeleteMember(index.ToString(CultureInfo.InvariantCulture), out result) && result)
                    {
                        return true;
                    }

                    if (TargetDynamicMetaObject.TryDeleteIndex(new object[] { index }, out result))
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
            return HostInvoke(() =>
            {
                bool updated;
                UpdatePropertyNames(out updated);
                if (updated || (PropertyIndices == null))
                {
                    PropertyIndices = AllPropertyNames.GetIndices().Distinct().ToArray();
                }

                return PropertyIndices;
            });
        }

        object IDynamic.Invoke(bool asConstructor, params object[] args)
        {
            return ThisReflect.InvokeMember(SpecialMemberNames.Default, asConstructor ? BindingFlags.CreateInstance : ((args.Length < 1) ? BindingFlags.InvokeMethod : BindingFlags.InvokeMethod | BindingFlags.GetProperty), null, ThisReflect, args, null, CultureInfo.InvariantCulture, null);
        }

        object IDynamic.InvokeMethod(string name, params object[] args)
        {
            return ThisReflect.InvokeMember(name, BindingFlags.InvokeMethod, null, ThisReflect, args, null, CultureInfo.InvariantCulture, null);
        }

        #endregion

        #region IEnumVARIANT implementation

        int IEnumVARIANT.Next(int count, object[] elements, IntPtr pCountFetched)
        {
            return HostInvoke(() =>
            {
                var index = 0;
                if (elements != null)
                {
                    var maxCount = Math.Min(count, elements.Length);
                    while ((index < maxCount) && TargetEnumerator.MoveNext())
                    {
                        elements[index++] = ThisDynamic.GetProperty("Current", ArrayHelpers.GetEmptyArray<object>());
                    }
                }

                if (pCountFetched != IntPtr.Zero)
                {
                    Marshal.WriteInt32(pCountFetched, index);
                }

                return (index == count) ? RawCOMHelpers.HResult.S_OK : RawCOMHelpers.HResult.S_FALSE;
            });
        }

        int IEnumVARIANT.Skip(int count)
        {
            return HostInvoke(() =>
            {
                var index = 0;
                while ((index < count) && TargetEnumerator.MoveNext())
                {
                    index++;
                }

                return (index == count) ? RawCOMHelpers.HResult.S_OK : RawCOMHelpers.HResult.S_FALSE;
            });
        }

        int IEnumVARIANT.Reset()
        {
            return HostInvoke(() =>
            {
                TargetEnumerator.Reset();
                return RawCOMHelpers.HResult.S_OK;
            });
        }

        IEnumVARIANT IEnumVARIANT.Clone()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICustomQueryInterface implementation

        public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr pInterface)
        {
            if (iid == typeof(IEnumVARIANT).GUID)
            {
                if ((target is HostObject) || (target is IHostVariable) || (target is IByRefArg))
                {
                    pInterface = IntPtr.Zero;
                    return BindSpecialTarget(Collateral.TargetEnumerator) ? CustomQueryInterfaceResult.NotHandled : CustomQueryInterfaceResult.Failed;
                }
            }
            else if (iid == typeof(IDispatchEx).GUID)
            {
                if (EnableVTablePatching && !bypassVTablePatching)
                {
                    var pUnknown = Marshal.GetIUnknownForObject(this);

                    bypassVTablePatching = true;
                    pInterface = RawCOMHelpers.QueryInterfaceNoThrow<IDispatchEx>(pUnknown);
                    bypassVTablePatching = false;

                    Marshal.Release(pUnknown);

                    if (pInterface != IntPtr.Zero)
                    {
                        VTablePatcher.GetInstance().PatchDispatchEx(pInterface);
                        return CustomQueryInterfaceResult.Handled;
                    }
                }
            }

            pInterface = IntPtr.Zero;
            return CustomQueryInterfaceResult.NotHandled;
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public ScriptEngine Engine
        {
            get { return engine; }
        }

        public Type AccessContext
        {
            get { return accessContext; }
        }

        public object Unwrap()
        {
            return target.Target;
        }

        #endregion

        #region IHostInvokeContext implementation

        public ScriptAccess DefaultAccess
        {
            get { return defaultAccess; }
        }

        #endregion

        #region Nested type: ExpandoHostItem

        private sealed class ExpandoHostItem : HostItem, IExpando
        {
            #region constructors

            public ExpandoHostItem(ScriptEngine engine, HostTarget target, HostItemFlags flags)
                : base(engine, target, flags)
            {
            }

            #endregion

            #region IExpando implementation

            FieldInfo IExpando.AddField(string name)
            {
                return HostInvoke(() =>
                {
                    if (CanAddExpandoMembers())
                    {
                        AddExpandoMemberName(name);
                        return MemberMap.GetField(name);
                    }

                    throw new NotSupportedException("Object does not support dynamic fields");
                });
            }

            PropertyInfo IExpando.AddProperty(string name)
            {
                return HostInvoke(() =>
                {
                    if (CanAddExpandoMembers())
                    {
                        AddExpandoMemberName(name);
                        return MemberMap.GetProperty(name);
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
                HostInvoke(() =>
                {
                    if (TargetDynamic != null)
                    {
                        int index;
                        if (int.TryParse(member.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                        {
                            if (TargetDynamic.DeleteProperty(index))
                            {
                                RemoveExpandoMemberName(index.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                        else if (TargetDynamic.DeleteProperty(member.Name))
                        {
                            RemoveExpandoMemberName(member.Name);
                        }
                    }
                    else if (TargetPropertyBag != null)
                    {
                        if (TargetPropertyBag.Remove(member.Name))
                        {
                            RemoveExpandoMemberName(member.Name);
                        }
                    }
                    else if (TargetDynamicMetaObject != null)
                    {
                        bool result;
                        if (TargetDynamicMetaObject.TryDeleteMember(member.Name, out result) && result)
                        {
                            RemoveExpandoMemberName(member.Name);
                        }
                        else
                        {
                            int index;
                            if (int.TryParse(member.Name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index) && TargetDynamicMetaObject.TryDeleteIndex(new object[] { index }, out result))
                            {
                                RemoveExpandoMemberName(index.ToString(CultureInfo.InvariantCulture));
                            }
                            else if (TargetDynamicMetaObject.TryDeleteIndex(new object[] { member.Name }, out result))
                            {
                                RemoveExpandoMemberName(member.Name);
                            }
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Object does not support dynamic members");
                    }
                });
            }

            #endregion
        }

        #endregion
    }
}
