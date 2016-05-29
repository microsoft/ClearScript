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
    internal abstract class ScriptItem : DynamicObject, IExpando, IDynamic, IScriptMarshalWrapper
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
                    throw new ScriptInterruptedException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.InnerException);
                }

                throw new ScriptEngineException(scriptError.EngineName, scriptError.Message, scriptError.ErrorDetails, scriptError.HResult, scriptError.IsFatal, scriptError.ExecutionStarted, scriptError.InnerException);
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

        private DynamicMetaObject PostProcessBindResult(DynamicMetaObject result)
        {
            var catchBody = Expression.Block(Expression.Call(throwLastScriptErrorMethod), Expression.Rethrow(), Expression.Default(result.Expression.Type));
            return new DynamicMetaObject(Expression.TryCatchFinally(result.Expression, Expression.Call(clearLastScriptErrorMethod), Expression.Catch(typeof(Exception), catchBody)), result.Restrictions);
        }

        #region DynamicObject overrides

        public override DynamicMetaObject GetMetaObject(Expression param)
        {
            return new MetaScriptItem(this, base.GetMetaObject(param));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetPropertyNames().Concat(GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture)));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryWrappedBindAndInvoke(binder, MiscHelpers.GetEmptyArray<object>(), out result);
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
            return TryWrappedBindAndInvoke(binder, indices.Concat(new[] { value }).ToArray(), out ignoredResult);
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
            return MiscHelpers.GetEmptyArray<MethodInfo>();
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
            return MiscHelpers.GetEmptyArray<PropertyInfo>();
        }

        public MemberInfo[] GetMember(string name, BindingFlags bindFlags)
        {
            // ReSharper disable CoVariantArrayConversion
            return GetFields(bindFlags).Where(propertyInfo => propertyInfo.Name == name).ToArray();
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
                    return Invoke(args, false);
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

        public object GetProperty(string name, object[] args, out bool isCacheable)
        {
            isCacheable = false;
            return GetProperty(name, args);
        }

        #endregion

        #region IDynamic implementation (abstract)

        public abstract object GetProperty(string name, object[] args);
        public abstract void SetProperty(string name, object[] args);
        public abstract bool DeleteProperty(string name);
        public abstract string[] GetPropertyNames();
        public abstract object GetProperty(int index);
        public abstract void SetProperty(int index, object value);
        public abstract bool DeleteProperty(int index);
        public abstract int[] GetPropertyIndices();
        public abstract object Invoke(object[] args, bool asConstructor);
        public abstract object InvokeMethod(string name, object[] args);

        #endregion

        #region IScriptMarshalWrapper implementation (abstract)

        public abstract ScriptEngine Engine { get; }

        public abstract object Unwrap();

        #endregion

        #region Nested type: MetaScriptItem

        private class MetaScriptItem : DynamicMetaObject
        {
            private readonly ScriptItem scriptItem;
            private readonly DynamicMetaObject metaDynamic;

            public MetaScriptItem(ScriptItem scriptItem, DynamicMetaObject metaDynamic)
                : base(metaDynamic.Expression, metaDynamic.Restrictions, metaDynamic.Value)
            {
                this.scriptItem = scriptItem;
                this.metaDynamic = metaDynamic;
            }

            #region DynamicMetaObject overrides

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return metaDynamic.GetDynamicMemberNames();
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindBinaryOperation(binder, arg));
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindConvert(binder));
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindCreateInstance(binder, args));
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindDeleteIndex(binder, indexes));
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindDeleteMember(binder));
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindGetIndex(binder, indexes));
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindGetMember(binder));
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindInvoke(binder, args));
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindInvokeMember(binder, args));
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindSetIndex(binder, indexes, value));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindSetMember(binder, value));
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
            {
                return scriptItem.PostProcessBindResult(metaDynamic.BindUnaryOperation(binder));
            }

            #endregion
        }

        #endregion
    }
}
