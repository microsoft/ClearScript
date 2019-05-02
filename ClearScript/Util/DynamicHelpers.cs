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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Expando;

namespace Microsoft.ClearScript.Util
{
    internal static class DynamicHelpers
    {
        #region public members

        public static DynamicMetaObject Bind(DynamicMetaObjectBinder binder, object target, object[] args)
        {
            return binder.Bind(CreateDynamicTarget(target), CreateDynamicArgs(args));
        }

        public static object Invoke(Expression expr)
        {
            Debug.Assert(expr != null);
            return Expression.Lambda(expr).Compile().DynamicInvoke();
        }

        public static object Invoke(Expression expr, IEnumerable<ParameterExpression> parameters, object[] args)
        {
            Debug.Assert(expr != null);
            return Expression.Lambda(expr, parameters).Compile().DynamicInvoke(args);
        }

        public static bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object target, object[] args, out object result)
        {
            try
            {
                // For COM member access, use IReflect/IExpando if possible. This works around
                // some dynamic binder bugs and limitations observed during batch test runs.

                var reflect = target as IReflect;
                if ((reflect != null) && reflect.GetType().IsCOMObject)
                {
                    var getMemberBinder = binder as GetMemberBinder;
                    if (getMemberBinder != null)
                    {
                        if (TryGetProperty(reflect, getMemberBinder.Name, getMemberBinder.IgnoreCase, args, out result))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        var setMemberBinder = binder as SetMemberBinder;
                        if (setMemberBinder != null)
                        {
                            if (TrySetProperty(reflect, setMemberBinder.Name, setMemberBinder.IgnoreCase, args, out result))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            var createInstanceBinder = binder as CreateInstanceBinder;
                            if (createInstanceBinder != null)
                            {
                                if (TryCreateInstance(reflect, args, out result))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                var invokeBinder = binder as InvokeBinder;
                                if (invokeBinder != null)
                                {
                                    if (TryInvoke(reflect, args, out result))
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    var invokeMemberBinder = binder as InvokeMemberBinder;
                                    if (invokeMemberBinder != null)
                                    {
                                        if (TryInvokeMethod(reflect, invokeMemberBinder.Name, invokeMemberBinder.IgnoreCase, args, out result))
                                        {
                                            return true;
                                        }
                                    }
                                    else if ((args != null) && (args.Length > 0))
                                    {
                                        var getIndexBinder = binder as GetIndexBinder;
                                        if (getIndexBinder != null)
                                        {
                                            if (TryGetProperty(reflect, args[0].ToString(), false, args.Skip(1).ToArray(), out result))
                                            {
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            var setIndexBinder = binder as SetIndexBinder;
                                            if (setIndexBinder != null)
                                            {
                                                if (TrySetProperty(reflect, args[0].ToString(), false, args.Skip(1).ToArray(), out result))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var binding = Bind(binder, target, args);
                result = Invoke(binding.Expression);
                return true;
            }
            catch (Exception exception)
            {
                result = exception;
                return false;
            }
        }

        #endregion

        #region DynamicMetaObject extensions

        public static bool TryCreateInstance(this DynamicMetaObject target, object[] args, out object result)
        {
            return TryDynamicOperation(() => target.CreateInstance(args), out result);
        }

        public static bool TryInvoke(this DynamicMetaObject target, IHostInvokeContext context, object[] args, out object result)
        {
            return TryDynamicOperation(() => target.Invoke(args), out result);
        }

        public static bool TryInvokeMember(this DynamicMetaObject target, IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args, out object result)
        {
            return TryDynamicOperation(() => target.InvokeMember(context, name, invokeFlags, args), out result);
        }

        public static bool TryGetMember(this DynamicMetaObject target, string name, out object result)
        {
            return TryDynamicOperation(() => target.GetMember(name), out result);
        }

        public static bool TrySetMember(this DynamicMetaObject target, string name, object value, out object result)
        {
            return TryDynamicOperation(() => target.SetMember(name, value), out result);
        }

        public static bool TryDeleteMember(this DynamicMetaObject target, string name, out bool result)
        {
            return TryDynamicOperation(() => target.DeleteMember(name), out result);
        }

        public static bool TryGetIndex(this DynamicMetaObject target, object[] indices, out object result)
        {
            return TryDynamicOperation(() => target.GetIndex(indices), out result);
        }

        public static bool TrySetIndex(this DynamicMetaObject target, object[] indices, object value, out object result)
        {
            return TryDynamicOperation(() => target.SetIndex(indices, value), out result);
        }

        public static bool TryDeleteIndex(this DynamicMetaObject target, object[] indices, out bool result)
        {
            return TryDynamicOperation(() => target.DeleteIndex(indices), out result);
        }

        #endregion

        #region internal members

        private static bool TryGetProperty(IReflect target, string name, bool ignoreCase, object[] args, out object result)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            var dispatchEx = target as IDispatchEx;
            if (dispatchEx != null)
            {
                // Standard IExpando-over-IDispatchEx support appears to leak the variants it
                // creates for the invocation arguments. This issue has been reported. In the
                // meantime we'll bypass this facility and interface with IDispatchEx directly.

                result = dispatchEx.GetProperty(name, ignoreCase, args);
                return true;
            }

            // ReSharper restore SuspiciousTypeConversion.Global

            var flags = BindingFlags.Public;
            if (ignoreCase)
            {
                flags |= BindingFlags.IgnoreCase;
            }

            var property = target.GetProperty(name, flags);
            if (property != null)
            {
                result = property.GetValue(target, args);
                return true;
            }

            result = null;
            return false;
        }

        private static bool TrySetProperty(IReflect target, string name, bool ignoreCase, object[] args, out object result)
        {
            if ((args != null) && (args.Length > 0))
            {
                // ReSharper disable SuspiciousTypeConversion.Global

                var dispatchEx = target as IDispatchEx;
                if (dispatchEx != null)
                {
                    // Standard IExpando-over-IDispatchEx support appears to leak the variants it
                    // creates for the invocation arguments. This issue has been reported. In the
                    // meantime we'll bypass this facility and interface with IDispatchEx directly.

                    dispatchEx.SetProperty(name, ignoreCase, args);
                    result = args[args.Length - 1];
                    return true;
                }

                // ReSharper restore SuspiciousTypeConversion.Global

                var flags = BindingFlags.Public;
                if (ignoreCase)
                {
                    flags |= BindingFlags.IgnoreCase;
                }

                var property = target.GetProperty(name, flags);
                if (property == null)
                {
                    var expando = target as IExpando;
                    if (expando != null)
                    {
                        property = expando.AddProperty(name);
                    }
                }

                if (property != null)
                {
                    property.SetValue(target, args[args.Length - 1], args.Take(args.Length - 1).ToArray());
                    result = args[args.Length - 1];
                    return true;
                }
            }

            result = null;
            return false;
        }

        private static bool TryCreateInstance(IReflect target, object[] args, out object result)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            var dispatchEx = target as IDispatchEx;
            if (dispatchEx != null)
            {
                // Standard IExpando-over-IDispatchEx support appears to leak the variants it
                // creates for the invocation arguments. This issue has been reported. In the
                // meantime we'll bypass this facility and interface with IDispatchEx directly.

                result = dispatchEx.Invoke(true, args);
                return true;
            }

            // ReSharper restore SuspiciousTypeConversion.Global

            try
            {
                result = target.InvokeMember(SpecialMemberNames.Default, BindingFlags.CreateInstance, null, target, args, null, CultureInfo.InvariantCulture, null);
                return true;
            }
            catch (TargetInvocationException)
            {
                throw;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private static bool TryInvoke(IReflect target, object[] args, out object result)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            var dispatchEx = target as IDispatchEx;
            if (dispatchEx != null)
            {
                // Standard IExpando-over-IDispatchEx support appears to leak the variants it
                // creates for the invocation arguments. This issue has been reported. In the
                // meantime we'll bypass this facility and interface with IDispatchEx directly.

                result = dispatchEx.Invoke(false, args);
                return true;
            }

            // ReSharper restore SuspiciousTypeConversion.Global

            try
            {
                result = target.InvokeMember(SpecialMemberNames.Default, BindingFlags.InvokeMethod, null, target, args, null, CultureInfo.InvariantCulture, null);
                return true;
            }
            catch (TargetInvocationException)
            {
                throw;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        private static bool TryInvokeMethod(IReflect target, string name, bool ignoreCase, object[] args, out object result)
        {
            // ReSharper disable SuspiciousTypeConversion.Global

            var dispatchEx = target as IDispatchEx;
            if (dispatchEx != null)
            {
                // Standard IExpando-over-IDispatchEx support appears to leak the variants it
                // creates for the invocation arguments. This issue has been reported. In the
                // meantime we'll bypass this facility and interface with IDispatchEx directly.

                result = dispatchEx.InvokeMethod(name, ignoreCase, args);
                return true;
            }

            // ReSharper restore SuspiciousTypeConversion.Global

            var flags = BindingFlags.Public;
            if (ignoreCase)
            {
                flags |= BindingFlags.IgnoreCase;
            }

            var method = target.GetMethod(name, flags);
            if (method != null)
            {
                result = method.Invoke(target, BindingFlags.InvokeMethod | flags, null, args, CultureInfo.InvariantCulture);
                return true;
            }

            result = null;
            return false;
        }

        private static bool TryDynamicOperation<T>(Func<T> operation, out T result)
        {
            try
            {
                result = operation();
                return true;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is InvalidDynamicOperationException)
                {
                    result = default(T);
                    return false;
                }

                throw;
            }
        }

        private static object CreateInstance(this DynamicMetaObject target, object[] args)
        {
            var paramNames = Enumerable.Range(0, args.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(args[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindCreateInstance(new DynamicCreateInstanceBinder(paramNames), parameters);
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block, paramExprs, args);
        }

        private static object Invoke(this DynamicMetaObject target, object[] args)
        {
            var paramNames = Enumerable.Range(0, args.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(args[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindInvoke(new DynamicInvokeBinder(paramNames), parameters);
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block, paramExprs, args);
        }

        private static object InvokeMember(this DynamicMetaObject target, IHostInvokeContext context, string name, BindingFlags invokeFlags, object[] args)
        {
            var paramNames = Enumerable.Range(0, args.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(args[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindInvokeMember(new DynamicInvokeMemberBinder(context, name, invokeFlags, paramNames), parameters);
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block, paramExprs, args);
        }

        private static object GetMember(this DynamicMetaObject target, string name)
        {
            var bindResult = target.BindGetMember(new DynamicGetMemberBinder(name));
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block);
        }

        private static object SetMember(this DynamicMetaObject target, string name, object value)
        {
            var bindResult = target.BindSetMember(new DynamicSetMemberBinder(name), CreateDynamicArg(value));
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block);
        }

        private static bool DeleteMember(this DynamicMetaObject target, string name)
        {
            var bindResult = target.BindDeleteMember(new DynamicDeleteMemberBinder(name));
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);

            try
            {
                Invoke(block);
                return true;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is InvalidDynamicOperationException)
                {
                    return false;
                }

                throw;
            }
        }

        private static object GetIndex(this DynamicMetaObject target, object[] indices)
        {
            var paramNames = Enumerable.Range(0, indices.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(indices[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindGetIndex(new DynamicGetIndexBinder(paramNames), parameters);
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block, paramExprs, indices);
        }

        private static object SetIndex(this DynamicMetaObject target, object[] indices, object value)
        {
            var paramNames = Enumerable.Range(0, indices.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(indices[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindSetIndex(new DynamicSetIndexBinder(paramNames), parameters, CreateDynamicArg(value));
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);
            return Invoke(block, paramExprs, indices);
        }

        private static bool DeleteIndex(this DynamicMetaObject target, object[] indices)
        {
            var paramNames = Enumerable.Range(0, indices.Length).Select(index => "a" + index).ToArray();
            var paramExprs = paramNames.Select((paramName, index) => Expression.Parameter(GetParamTypeForArg(indices[index]), paramName)).ToArray();
            var parameters = paramExprs.Select(paramExpr => new DynamicMetaObject(paramExpr, BindingRestrictions.Empty)).ToArray();
            var bindResult = target.BindDeleteIndex(new DynamicDeleteIndexBinder(paramNames), parameters);
            var block = Expression.Block(Expression.Label(CallSiteBinder.UpdateLabel), bindResult.Expression);

            try
            {
                Invoke(block, paramExprs, indices);
                return true;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is InvalidDynamicOperationException)
                {
                    return false;
                }

                throw;
            }
        }

        private static DynamicMetaObject CreateDynamicTarget(object target)
        {
            var byRefArg = target as IByRefArg;
            if (byRefArg != null)
            {
                return CreateDynamicMetaObject(byRefArg.Value, Expression.Parameter(byRefArg.Type.MakeByRefType()));
            }

            var hostTarget = target as HostTarget;
            if (hostTarget == null)
            {
                return CreateDynamicMetaObject(target, Expression.Constant(target));
            }

            target = hostTarget.DynamicInvokeTarget;
            if (hostTarget is HostType)
            {
                return CreateDynamicMetaObject(target, Expression.Constant(target));
            }

            var type = hostTarget.Type;
            try
            {
                return CreateDynamicMetaObject(target, Expression.Constant(target, type));
            }
            catch (ArgumentException)
            {
                return CreateDynamicMetaObject(target, Expression.Constant(target));
            }
        }

        private static DynamicMetaObject CreateDynamicArg(object arg)
        {
            var byRefArg = arg as IByRefArg;
            if (byRefArg != null)
            {
                return CreateDynamicMetaObject(byRefArg.Value, Expression.Parameter(byRefArg.Type.MakeByRefType()));
            }

            if (arg is HostType)
            {
                return CreateDynamicMetaObject(arg, Expression.Constant(arg));
            }

            var hostTarget = arg as HostTarget;
            if (hostTarget == null)
            {
                return CreateDynamicMetaObject(arg, Expression.Constant(arg));
            }

            arg = hostTarget.Target;

            var type = hostTarget.Type;
            try
            {
                return CreateDynamicMetaObject(arg, Expression.Constant(arg, type));
            }
            catch (ArgumentException)
            {
                return CreateDynamicMetaObject(arg, Expression.Constant(arg));
            }
        }

        private static DynamicMetaObject[] CreateDynamicArgs(object[] args)
        {
            return args.Select(CreateDynamicArg).ToArray();
        }

        private static DynamicMetaObject CreateDynamicMetaObject(object value, Expression expr)
        {
            return new DynamicMetaObject(expr, BindingRestrictions.Empty, value);
        }

        private static Expression CreateThrowExpr<T>(string message) where T : Exception
        {
            var constructor = typeof(T).GetConstructor(new[] { typeof(string) });

            Expression exceptionExpr;
            if (constructor != null)
            {
                exceptionExpr = Expression.New(constructor, Expression.Constant(message));
            }
            else
            {
                exceptionExpr = Expression.Constant(typeof(T).CreateInstance(message));
            }

            return Expression.Throw(exceptionExpr);
        }

        private static Type GetParamTypeForArg(object arg)
        {
            return (arg != null) ? arg.GetType() : typeof(object);
        }

        #endregion

        #region Nested type: DynamicCreateInstanceBinder

        private sealed class DynamicCreateInstanceBinder : CreateInstanceBinder
        {
            public DynamicCreateInstanceBinder(string[] paramNames)
                : base(new CallInfo(paramNames.Length, paramNames))
            {
            }

            public override DynamicMetaObject FallbackCreateInstance(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic instantiation algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic instantiation.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic instantiation"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicInvokeBinder

        private sealed class DynamicInvokeBinder : InvokeBinder
        {
            public DynamicInvokeBinder(string[] paramNames)
                : base(new CallInfo(paramNames.Length, paramNames))
            {
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic invocation algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic invocation.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic object invocation"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicGetMemberBinder

        private sealed class DynamicGetMemberBinder : GetMemberBinder
        {
            public DynamicGetMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic member retrieval algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic member retrieval.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic member retrieval"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicSetMemberBinder

        private sealed class DynamicSetMemberBinder : SetMemberBinder
        {
            public DynamicSetMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic member assignment algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic member assignment.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic member assignment"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicInvokeMemberBinder

        private sealed class DynamicInvokeMemberBinder : InvokeMemberBinder
        {
            private static readonly MethodInfo invokeMemberValueMethod = typeof(DynamicInvokeMemberBinder).GetMethod("InvokeMemberValue", BindingFlags.NonPublic | BindingFlags.Static);
            private readonly IHostInvokeContext context;
            private readonly BindingFlags invokeFlags;

            public DynamicInvokeMemberBinder(IHostInvokeContext context, string name, BindingFlags invokeFlags, string[] paramNames)
                : base(name, false, new CallInfo(paramNames.Length, paramNames))
            {
                this.context = context;
                this.invokeFlags = invokeFlags;
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic member invocation algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic member invocation.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic member invocation"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // behave as in other scenarios, but the observed value is always null
                    return errorSuggestion;
                }

                // construct an algorithm for invoking a member value
                var argExprs = new[] { Expression.Constant(context), target.Expression, Expression.Constant(invokeFlags), Expression.NewArrayInit(typeof(object), args.Select(GetArgRefExpr)) };
                return new DynamicMetaObject(Expression.Call(invokeMemberValueMethod, argExprs), BindingRestrictions.Empty);
            }

            private static Expression GetArgRefExpr(DynamicMetaObject arg)
            {
                var argExpr = arg.Expression;
                return argExpr.Type.IsValueType ? Expression.Convert(argExpr, typeof(object)) : argExpr;
            }

            // ReSharper disable UnusedMember.Local

            private static object InvokeMemberValue(IHostInvokeContext context, object target, BindingFlags invokeFlags, object[] args)
            {
                object result;
                if (InvokeHelpers.TryInvokeObject(context, target, BindingFlags.InvokeMethod, args, args, true, out result))
                {
                    return result;
                }

                if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                {
                    return target;
                }

                throw new InvalidDynamicOperationException("Invalid dynamic member value invocation");
            }

            // ReSharper restore UnusedMember.Local
        }

        #endregion

        #region Nested type: DynamicDeleteMemberBinder
        
        private sealed class DynamicDeleteMemberBinder : DeleteMemberBinder
        {
            public DynamicDeleteMemberBinder(string name)
                : base(name, false)
            {
            }

            public override DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic member deletion algorithm
                    return errorSuggestion;
                }

                // construct an algorithm for dealing with unsuccessful dynamic member deletion
                return new DynamicMetaObject(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic member deletion"), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicGetIndexBinder

        private sealed class DynamicGetIndexBinder : GetIndexBinder
        {
            public DynamicGetIndexBinder(string[] paramNames)
                : base(new CallInfo(paramNames.Length, paramNames))
            {
            }

            public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic index retrieval algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic index retrieval.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic index retrieval"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicSetIndexBinder

        private sealed class DynamicSetIndexBinder : SetIndexBinder
        {
            public DynamicSetIndexBinder(string[] paramNames)
                : base(new CallInfo(paramNames.Length, paramNames))
            {
            }

            public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic index assignment algorithm
                    return errorSuggestion;
                }

                // Construct an algorithm for dealing with unsuccessful dynamic index assignment.
                // A block returning a reference object appears to be required for some reason.
                return new DynamicMetaObject(Expression.Block(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic index assignment"), Expression.Constant(Nonexistent.Value)), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: DynamicDeleteIndexBinder

        private sealed class DynamicDeleteIndexBinder : DeleteIndexBinder
        {
            public DynamicDeleteIndexBinder(string[] paramNames)
                : base(new CallInfo(paramNames.Length, paramNames))
            {
            }

            public override DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indices, DynamicMetaObject errorSuggestion)
            {
                if (errorSuggestion != null)
                {
                    // errorSuggestion is the dynamic index deletion algorithm
                    return errorSuggestion;
                }

                // construct an algorithm for dealing with unsuccessful dynamic index deletion
                return new DynamicMetaObject(CreateThrowExpr<InvalidDynamicOperationException>("Invalid dynamic index deletion"), BindingRestrictions.Empty);
            }
        }

        #endregion

        #region Nested type: InvalidDynamicOperationException

        [Serializable]
        private sealed class InvalidDynamicOperationException : InvalidOperationException
        {
            public InvalidDynamicOperationException(string message)
                : base(message)
            {
            }
        }

        #endregion
    }
}
