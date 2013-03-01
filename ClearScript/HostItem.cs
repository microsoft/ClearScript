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
using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private PropertyInfo[] cachedIndexProperties;

        private ExtensionMethodSummary cachedExtensionMethodSummary;

        private static readonly MemberMap<Field> fieldMap = new MemberMap<Field>();
        private static readonly MemberMap<Method> methodMap = new MemberMap<Method>();
        private static readonly MemberMap<Property> propertyMap = new MemberMap<Property>();

        #endregion

        #region constructors

        private HostItem(ScriptEngine engine, object target, Type type, HostItemFlags flags)
            : this(engine, (target as HostTarget) ?? HostObject.Wrap(target, type), flags)
        {
        }

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
                return new HostItem(engine, hostTarget, flags);
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
                return new HostItem(engine, obj, type, flags);
            }

            var typeCode = Type.GetTypeCode(type);
            if ((typeCode == TypeCode.Object) || (typeCode == TypeCode.DateTime))
            {
                return new HostItem(engine, obj, type, flags);
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
            if (target.TryInvokeAuxMember(name, invokeFlags, args, bindArgs, out result))
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

        private void Initialize()
        {
            if (!(target is HostType) && (target.InvokeTarget != null))
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
            // The checks here are required because the item may be bound to a specific target base
            // class or interface - one that must not trigger special treatment.

            if (typeof(IDynamic).IsAssignableFrom(target.Type))
            {
                targetDynamic = target.InvokeTarget as IDynamic;
                if (targetDynamic != null)
                {
                    return;
                }
            }

            if (typeof(IPropertyBag).IsAssignableFrom(target.Type))
            {
                targetPropertyBag = target.InvokeTarget as IPropertyBag;
                if (targetPropertyBag != null)
                {
                    return;
                }
            }

            if (typeof(IList).IsAssignableFrom(target.Type))
            {
                targetList = target.InvokeTarget as IList;
            }
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
            if ((targetDynamic != null) || (targetPropertyBag != null))
            {
                return MiscHelpers.GetEmptyArray<string>();
            }

            if (cachedFieldNames == null)
            {
                cachedFieldNames = GetLocalFieldNames().Concat(GetLocalEventNames()).ToArray();
            }

            return cachedFieldNames;
        }

        private string[] GetAllMethodNames()
        {
            if ((targetDynamic != null) || (targetPropertyBag != null))
            {
                return MiscHelpers.GetEmptyArray<string>();
            }

            if (cachedMethodNames == null)
            {
                var names = GetLocalMethodNames().Concat(target.GetAuxMethodNames(GetMethodBindFlags()));
                if (target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
                {
                    names = names.Concat(engine.ExtensionMethodSummary.MethodNames);
                }

                cachedMethodNames = names.Distinct().ToArray();
            }

            return cachedMethodNames;
        }

        private string[] GetAllPropertyNames()
        {
            if (targetDynamic != null)
            {
                var names = targetDynamic.GetPropertyNames();
                var indexStrings = targetDynamic.GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture));
                return names.Concat(indexStrings).ToArray();
            }

            if (targetPropertyBag != null)
            {
                return targetPropertyBag.Keys.ToArray();
            }

            if (cachedPropertyNames == null)
            {
                var names = GetLocalPropertyNames().Concat(target.GetAuxPropertyNames(GetCommonBindFlags()));
                cachedPropertyNames = names.Distinct().ToArray();
            }

            return cachedPropertyNames;
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
                    catch (Exception exception)
                    {
                        if ((exception is NotSupportedException) || (exception is InvalidOperationException) || (exception is ExternalException))
                        {
                            if (invokeFlags.HasFlag(BindingFlags.GetField))
                            {
                                return targetDynamic;
                            }
                        }

                        throw;
                    }
                }

                try
                {
                    return targetDynamic.InvokeMethod(name, args);
                }
                catch (Exception exception)
                {
                    if ((exception is MissingMemberException) || (exception is NotSupportedException) || (exception is InvalidOperationException) || (exception is ExternalException))
                    {
                        if (invokeFlags.HasFlag(BindingFlags.GetField))
                        {
                            return targetDynamic.GetProperty(name);
                        }
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
                var value = args.First();
                targetDynamic.SetProperty(name, value);
                return value;
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokePropertyBagMember(string name, BindingFlags invokeFlags, object[] args, object[] bindArgs)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    if (invokeFlags.HasFlag(BindingFlags.GetField))
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
                if (InvokeHelpers.TryInvokeObject(value, invokeFlags, args, bindArgs, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    return value;
                }

                throw new NotSupportedException("Object does not support invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                object value;
                return targetPropertyBag.TryGetValue(name, out value) ? value : Nonexistent.Value;
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (!targetPropertyBag.IsReadOnly)
                {
                    return targetPropertyBag[name] = args.First();
                }

                throw new UnauthorizedAccessException("Object is read-only");
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object InvokeListElement(int index, BindingFlags invokeFlags, object[] args, object[] bindArgs)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                object result;
                if (InvokeHelpers.TryInvokeObject(targetList[index], invokeFlags, args, bindArgs, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    return targetList[index];
                }

                throw new NotSupportedException("Object does not support invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return targetList[index];
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                return targetList[index] = args.First();
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
                            if (hostType.TryInvoke(BindingFlags.InvokeMethod, typeArgs, typeArgs, out result))
                            {
                                hostType = result as HostType;
                                if (hostType != null)
                                {
                                    args = args.Skip(typeArgs.Length).ToArray();
                                    return hostType.GetSpecificType().CreateInstance(invokeFlags, args);
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
                }

                throw new InvalidOperationException("Invalid constructor invocation");
            }

            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    object result;
                    if (InvokeHelpers.TryInvokeObject(target, invokeFlags, args, bindArgs, out result))
                    {
                        return result;
                    }

                    if (invokeFlags.HasFlag(BindingFlags.GetField))
                    {
                        return target;
                    }

                    throw new NotSupportedException("Object does not support invocation");
                }

                if (thisExpando.GetMethods(GetMethodBindFlags()).Any(method => method.Name == name))
                {
                    return InvokeMethod(name, args, bindArgs);
                }

                var property = target.Type.GetScriptableProperty(name, GetCommonBindFlags());
                if ((property != null) && (typeof(Delegate).IsAssignableFrom(property.PropertyType)))
                {
                    var del = (Delegate)property.GetValue(target.InvokeTarget, invokeFlags | BindingFlags.GetProperty, Type.DefaultBinder, MiscHelpers.GetEmptyArray<object>(), culture);
                    return InvokeHelpers.InvokeDelegate(del, args);
                }

                var field = target.Type.GetScriptableField(name, GetCommonBindFlags());
                if ((field != null) && (typeof(Delegate).IsAssignableFrom(field.FieldType)))
                {
                    var del = (Delegate)field.GetValue(target.InvokeTarget);
                    return InvokeHelpers.InvokeDelegate(del, args);
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField))
                {
                    return GetHostProperty(name, invokeFlags, args, culture);
                }

                throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}'", name));
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                return GetHostProperty(name, invokeFlags, args, culture);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (name == SpecialMemberNames.Default)
                {
                    object result;
                    if (InvokeHelpers.TryInvokeObject(target, invokeFlags, args, bindArgs, out result))
                    {
                        return result;
                    }

                    throw new InvalidOperationException("Invalid property assignment");
                }

                var property = target.Type.GetScriptableProperty(name, invokeFlags);
                if (property != null)
                {
                    if (args.Length > 0)
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

                    throw new InvalidOperationException("Invalid argument count");
                }

                var field = target.Type.GetScriptableField(name, invokeFlags);
                if (field != null)
                {
                    if (args.Length == 1)
                    {
                        if (field.IsReadOnlyForScript())
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

                throw new MissingMemberException(MiscHelpers.FormatInvariant("Object has no property or field named '{0}'", name));
            }

            throw new InvalidOperationException("Invalid member invocation mode");
        }

        private object GetHostProperty(string name, BindingFlags invokeFlags, object[] args, CultureInfo culture)
        {
            var property = target.Type.GetScriptableProperty(name, invokeFlags);
            if (property != null)
            {
                if ((property.GetIndexParameters().Length > 0) && (args.Length < 1))
                {
                    return new HostIndexer(this, name);
                }

                return property.GetValue(target.InvokeTarget, invokeFlags, Type.DefaultBinder, args, culture);
            }

            var field = target.Type.GetScriptableField(name, invokeFlags);
            if (field != null)
            {
                return field.GetValue(target.InvokeTarget);
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
                return new HostMethod(this, name);
            }

            return Nonexistent.Value;
        }

        #endregion

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("[{0}]", target);
        }

        #endregion

        #region DynamicObject overrides

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
                    result = Convert.ChangeType(target.InvokeTarget, binder.ReturnType);
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

                if (cachedFields == null)
                {
                    cachedFields = fieldMap.GetMembers(GetAllFieldNames());
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

                var extensionMethodSummary = engine.ExtensionMethodSummary;
                if (extensionMethodSummary != cachedExtensionMethodSummary)
                {
                    cachedMethodNames = null;
                    cachedMethods = null;
                    cachedExtensionMethodSummary = extensionMethodSummary;
                }

                if (cachedMethods == null)
                {
                    cachedMethods = methodMap.GetMembers(GetAllMethodNames());
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

                if (cachedProperties == null)
                {
                    cachedProperties = propertyMap.GetMembers(GetAllPropertyNames());
                }

                if ((targetList != null) && (targetList.Count > 0))
                {
                    if ((cachedIndexProperties == null) || (cachedIndexProperties.Length != targetList.Count))
                    {
                        cachedIndexProperties = propertyMap.GetMembers(Enumerable.Range(0, targetList.Count).Select(index => index.ToString(CultureInfo.InvariantCulture)).ToArray());
                    }

                    return cachedProperties.Concat(cachedIndexProperties).ToArray();
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
                if ((namedParams != null) && (namedParams.Length > 0) && (namedParams[0] == SpecialParameterNames.This))
                {
                    args = args.Skip(1).ToArray();
                    skipFirst = true;
                }

                object[] bindArgs = null;
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
                if ((targetDynamic != null) || ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly))
                {
                    return fieldMap.GetMember(name);
                }

                throw new NotSupportedException("Object does not support dynamic fields");
            });
        }

        PropertyInfo IExpando.AddProperty(string name)
        {
            return engine.HostInvoke(() =>
            {
                if ((targetDynamic != null) || ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly))
                {
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
                        targetDynamic.DeleteProperty(index);
                    }

                    targetDynamic.DeleteProperty(member.Name);
                }
                else if ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly)
                {
                    targetPropertyBag.Remove(member.Name);
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

                if ((targetPropertyBag != null) && !targetPropertyBag.IsReadOnly)
                {
                    return targetPropertyBag.Remove(name);
                }

                throw new NotSupportedException("Object does not support dynamic members");
            });
        }

        string[] IDynamic.GetPropertyNames()
        {
            return engine.HostInvoke(() =>
            {
                if (targetDynamic != null)
                {
                    return targetDynamic.GetPropertyNames();
                }

                if (targetPropertyBag != null)
                {
                    return targetPropertyBag.Keys.ExcludeIndices().ToArray();
                }

                var extensionMethodSummary = engine.ExtensionMethodSummary;
                if (extensionMethodSummary != cachedExtensionMethodSummary)
                {
                    cachedMethodNames = null;
                    cachedMethods = null;
                    cachedExtensionMethodSummary = extensionMethodSummary;
                }

                return GetAllFieldNames().Concat(GetAllPropertyNames()).Concat(GetAllMethodNames()).ToArray();
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

                return false;
            });
        }

        int[] IDynamic.GetPropertyIndices()
        {
            return engine.HostInvoke(() =>
            {
                if (targetDynamic != null)
                {
                    return targetDynamic.GetPropertyIndices();
                }

                if (targetPropertyBag != null)
                {
                    return targetPropertyBag.Keys.GetIndices().ToArray();
                }

                if ((targetList != null) && (targetList.Count > 0))
                {
                    return Enumerable.Range(0, targetList.Count).ToArray();
                }

                return MiscHelpers.GetEmptyArray<int>();
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
