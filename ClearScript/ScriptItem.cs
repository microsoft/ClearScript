// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
    internal abstract class ScriptItem : ScriptObject, IExpando, IDynamic, IScriptMarshalWrapper
    {
        private static readonly MethodInfo throwLastScriptErrorMethod = typeof(ScriptItem).GetMethod("ThrowLastScriptError");
        private static readonly MethodInfo clearLastScriptErrorMethod = typeof(ScriptItem).GetMethod("ClearLastScriptError");
        [ThreadStatic] private static IScriptEngineException lastScriptError;

        public static void ThrowLastScriptError()
        {
            var scriptError = lastScriptError;
            if (scriptError != null)
            {
                if (scriptError is ScriptInterruptedException)
                {
                    throw new ScriptInterruptedException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptException, scriptError.InnerException);
                }

                throw new ScriptEngineException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.ScriptException, scriptError.InnerException);
            }
        }

        public static void ClearLastScriptError()
        {
            lastScriptError = null;
        }

        protected abstract bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result);

        protected virtual object[] AdjustInvokeArgs(object[] args)
        {
            return args;
        }

        private bool TryWrappedBindAndInvoke(DynamicMetaObjectBinder binder, object[] wrappedArgs, out object result)
        {
            object[] args = null;
            object[] savedArgs = null;

            object tempResult = null;
            var succeeded = Engine.ScriptInvoke(() =>
            {
                args = Engine.MarshalToScript(wrappedArgs);
                savedArgs = (object[])args.Clone();

                if (!TryBindAndInvoke(binder, args, out tempResult))
                {
                    if ((Engine.CurrentScriptFrame != null) && (lastScriptError == null))
                    {
                        lastScriptError = Engine.CurrentScriptFrame.ScriptError;
                    }

                    return false;
                }

                return true;
            });

            if (succeeded)
            {
                for (var index = 0; index < args.Length; index++)
                {
                    var arg = args[index];
                    if (!ReferenceEquals(arg, savedArgs[index]))
                    {
                        wrappedArgs[index] = Engine.MarshalToHost(args[index], false);
                    }
                }

                result = Engine.MarshalToHost(tempResult, false).ToDynamicResult(Engine);
                return true;
            }

            result = null;
            return false;
        }

        private bool TryWrappedInvokeOrInvokeMember(DynamicMetaObjectBinder binder, ParameterInfo[] parameters, object[] args, out object result)
        {
            Type[] paramTypes = null;
            if ((parameters != null) && (parameters.Length >= args.Length))
            {
                paramTypes = parameters.Skip(parameters.Length - args.Length).Select(param => param.ParameterType).ToArray();
            }

            if (paramTypes != null)
            {
                for (var index = 0; index < paramTypes.Length; index++)
                {
                    var paramType = paramTypes[index];
                    if (paramType.IsByRef)
                    {
                        args[index] = typeof(HostVariable<>).MakeSpecificType(paramType.GetElementType()).CreateInstance(args[index]);
                    }
                }
            }

            if (TryWrappedBindAndInvoke(binder, AdjustInvokeArgs(args), out result))
            {
                if (paramTypes != null)
                {
                    for (var index = 0; index < paramTypes.Length; index++)
                    {
                        if (paramTypes[index].IsByRef)
                        {
                            var hostVariable = args[index] as IHostVariable;
                            if (hostVariable != null)
                            {
                                args[index] = hostVariable.Value;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private string[] GetAllPropertyNames()
        {
            return GetPropertyNames().Concat(GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture))).ToArray();
        }

        private static DynamicMetaObject PostProcessBindResult(DynamicMetaObject result)
        {
            var catchBody = Expression.Block(Expression.Call(throwLastScriptErrorMethod), Expression.Rethrow(), Expression.Default(result.Expression.Type));
            return new DynamicMetaObject(Expression.TryCatchFinally(result.Expression, Expression.Call(clearLastScriptErrorMethod), Expression.Catch(typeof(Exception), catchBody)), result.Restrictions);
        }

        #region ScriptObject overrides

        public override IEnumerable<string> PropertyNames
        {
            get { return GetPropertyNames(); }
        }

        public override IEnumerable<int> PropertyIndices
        {
            get { return GetPropertyIndices(); }
        }

        public new object this[string name, params object[] args]
        {
            get { return GetProperty(name, args).ToDynamicResult(Engine); }
            set { SetProperty(name, args.Concat(value.ToEnumerable()).ToArray()); }
        }

        public new object this[int index]
        {
            get { return GetProperty(index).ToDynamicResult(Engine); }
            set { SetProperty(index, value); }
        }

        #endregion

        #region DynamicObject overrides

        public override DynamicMetaObject GetMetaObject(Expression param)
        {
            return new MetaScriptItem(base.GetMetaObject(param));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetPropertyNames().Concat(GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture)));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryWrappedBindAndInvoke(binder, ArrayHelpers.GetEmptyArray<object>(), out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object ignoredResult;
            return TryWrappedBindAndInvoke(binder, new[] { value }, out ignoredResult);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            return TryWrappedBindAndInvoke(binder, indices, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indices, object value)
        {
            object ignoredResult;
            return TryWrappedBindAndInvoke(binder, indices.Concat(value.ToEnumerable()).ToArray(), out ignoredResult);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            ParameterInfo[] parameters = null;
            if (Engine.EnableAutoHostVariables)
            {
                parameters = new StackFrame(1, false).GetMethod().GetParameters();
            }

            return TryWrappedInvokeOrInvokeMember(binder, parameters, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            ParameterInfo[] parameters = null;
            if (Engine.EnableAutoHostVariables)
            {
                parameters = new StackFrame(1, false).GetMethod().GetParameters();
            }

            return TryWrappedInvokeOrInvokeMember(binder, parameters, args, out result);
        }

        #endregion

        #region IReflect implementation

        public MethodInfo GetMethod(string name, BindingFlags bindFlags, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public MethodInfo GetMethod(string name, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        public MethodInfo[] GetMethods(BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<MethodInfo>();
        }

        public FieldInfo GetField(string name, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        public FieldInfo[] GetFields(BindingFlags bindFlags)
        {
            return MemberMap.GetFields(GetAllPropertyNames());
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindFlags)
        {
            throw new NotImplementedException();
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindFlags, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public PropertyInfo[] GetProperties(BindingFlags bindFlags)
        {
            return ArrayHelpers.GetEmptyArray<PropertyInfo>();
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindFlags)
        {
            // ReSharper disable CoVariantArrayConversion
            return new [] { MemberMap.GetField(name) };
            // ReSharper restore CoVariantArrayConversion
        }

        public MemberInfo[] GetMembers(BindingFlags bindFlags)
        {
            // ReSharper disable CoVariantArrayConversion
            return GetFields(bindFlags);
            // ReSharper restore CoVariantArrayConversion
        }

        public object InvokeMember(string name, BindingFlags invokeFlags, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
            {
                if (name == SpecialMemberNames.Default)
                {
                    return Invoke(false, args);
                }

                return InvokeMethod(name, args);
            }

            if (invokeFlags.HasFlag(BindingFlags.GetField))
            {
                int index;
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    return GetProperty(index);
                }

                return GetProperty(name, args);
            }

            if (invokeFlags.HasFlag(BindingFlags.SetField))
            {
                if (args.Length != 1)
                {
                    throw new InvalidOperationException("Invalid argument count");
                }

                var value = args[0];

                int index;
                if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    SetProperty(index, value);
                    return value;
                }

                SetProperty(name, args);
                return value;
            }

            throw new InvalidOperationException("Invalid member access mode");
        }

        public Type UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IExpando implementation

        public FieldInfo AddField(string name)
        {
            throw new NotImplementedException();
        }

        public PropertyInfo AddProperty(string name)
        {
            throw new NotImplementedException();
        }

        public MethodInfo AddMethod(string name, Delegate method)
        {
            throw new NotImplementedException();
        }

        public void RemoveMember(MemberInfo member)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDynamic implementation

        public object GetProperty(string name, out bool isCacheable, params object[] args)
        {
            isCacheable = false;
            return GetProperty(name, args);
        }

        #endregion

        #region IDynamic implementation (abstract)

        public abstract string[] GetPropertyNames();
        public abstract int[] GetPropertyIndices();

        #endregion

        #region IScriptMarshalWrapper implementation (abstract)

        public abstract object Unwrap();

        #endregion

        #region Nested type: MetaScriptItem

        private sealed class MetaScriptItem : DynamicMetaObject
        {
            private readonly DynamicMetaObject metaDynamic;

            public MetaScriptItem(DynamicMetaObject metaDynamic)
                : base(metaDynamic.Expression, metaDynamic.Restrictions, metaDynamic.Value)
            {
                this.metaDynamic = metaDynamic;
            }

            #region DynamicMetaObject overrides

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return metaDynamic.GetDynamicMemberNames();
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
            {
                return PostProcessBindResult(metaDynamic.BindBinaryOperation(binder, arg));
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                return PostProcessBindResult(metaDynamic.BindConvert(binder));
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                return PostProcessBindResult(metaDynamic.BindCreateInstance(binder, args));
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
            {
                return PostProcessBindResult(metaDynamic.BindDeleteIndex(binder, indexes));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                return PostProcessBindResult(metaDynamic.BindDeleteMember(binder));
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                return PostProcessBindResult(metaDynamic.BindGetIndex(binder, indexes));
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                return PostProcessBindResult(metaDynamic.BindGetMember(binder));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                return PostProcessBindResult(metaDynamic.BindInvoke(binder, args));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                return PostProcessBindResult(metaDynamic.BindInvokeMember(binder, args));
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                return PostProcessBindResult(metaDynamic.BindSetIndex(binder, indexes, value));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                return PostProcessBindResult(metaDynamic.BindSetMember(binder, value));
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
            {
                return PostProcessBindResult(metaDynamic.BindUnaryOperation(binder));
            }

            #endregion
        }

        #endregion
    }
}
