// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal static partial class DelegateFactory
    {
        public static Delegate CreateProc(ScriptEngine engine, object target, int argCount)
        {
            if ((argCount < 0) || (argCount > maxArgCount))
            {
                throw new ArgumentException("Invalid argument count", "argCount");
            }

            var typeArgs = Enumerable.Repeat(typeof(object), argCount).ToArray();
            return CreateDelegate(engine, target, procTemplates[argCount].MakeSpecificType(typeArgs));
        }

        public static Delegate CreateFunc<TResult>(ScriptEngine engine, object target, int argCount)
        {
            if ((argCount < 0) || (argCount > maxArgCount))
            {
                throw new ArgumentException("Invalid argument count", "argCount");
            }

            var typeArgs = Enumerable.Repeat(typeof(object), argCount).Concat(typeof(TResult).ToEnumerable()).ToArray();
            return CreateDelegate(engine, target, funcTemplates[argCount].MakeSpecificType(typeArgs));
        }

        public static TDelegate CreateDelegate<TDelegate>(ScriptEngine engine, object target)
        {
            return (TDelegate)(object)CreateDelegate(engine, target, typeof(TDelegate));
        }

        public static Delegate CreateDelegate(ScriptEngine engine, object target, Type delegateType)
        {
            MiscHelpers.VerifyNonNullArgument(target, "target");
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
            {
                throw new ArgumentException("Invalid delegate type");
            }

            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                throw new ArgumentException("Invalid delegate type (invocation method not found)");
            }

            var parameters = method.GetParameters();
            if (parameters.Length > maxArgCount)
            {
                throw new ArgumentException("Invalid delegate type (parameter count too large)");
            }

            var paramTypes = parameters.Select(param => param.ParameterType).ToArray();
            if (paramTypes.Any(paramType => paramType.IsByRef))
            {
                return CreateComplexDelegate(engine, target, delegateType);
            }

            return CreateSimpleDelegate(engine, target, delegateType);
        }

        private static Delegate CreateSimpleDelegate(ScriptEngine engine, object target, Type delegateType)
        {
            // ReSharper disable PossibleNullReferenceException

            var method = delegateType.GetMethod("Invoke");
            var paramTypes = method.GetParameters().Select(param => param.ParameterType).ToArray();

            Type shimType;
            if (method.ReturnType == typeof(void))
            {
                var typeArgs = paramTypes.Concat(delegateType.ToEnumerable()).ToArray();
                shimType = procShimTemplates[paramTypes.Length].MakeSpecificType(typeArgs);
            }
            else
            {
                var typeArgs = paramTypes.Concat(new[] { method.ReturnType, delegateType }).ToArray();
                shimType = funcShimTemplates[paramTypes.Length].MakeSpecificType(typeArgs);
            }

            var shim = (DelegateShim)shimType.CreateInstance(engine, target);
            return shim.Delegate;

            // ReSharper restore PossibleNullReferenceException
        }

        private static Delegate CreateComplexDelegate(ScriptEngine engine, object target, Type delegateType)
        {
            // ReSharper disable CoVariantArrayConversion
            // ReSharper disable PossibleNullReferenceException

            var method = delegateType.GetMethod("Invoke");

            var parameters = method.GetParameters();
            var paramTypes = parameters.Select(param => param.ParameterType).ToArray();

            var innerParamTypes = new Type[parameters.Length];
            for (var index = 0; index < parameters.Length; index++)
            {
                var paramType = paramTypes[index];
                if (parameters[index].IsOut)
                {
                    innerParamTypes[index] = typeof(OutArg<>).MakeSpecificType(paramType.GetElementType());
                }
                else if (paramType.IsByRef)
                {
                    innerParamTypes[index] = typeof(RefArg<>).MakeSpecificType(paramType.GetElementType());
                }
                else
                {
                    innerParamTypes[index] = paramType;
                }
            }

            Type innerDelegateType;
            if (method.ReturnType == typeof(void))
            {
                innerDelegateType = procTemplates[innerParamTypes.Length].MakeSpecificType(innerParamTypes);
            }
            else
            {
                var typeArgs = innerParamTypes.Concat(method.ReturnType.ToEnumerable()).ToArray();
                innerDelegateType = funcTemplates[innerParamTypes.Length].MakeSpecificType(typeArgs);
            }

            var paramExprs = paramTypes.Select((paramType, index) => Expression.Parameter(paramType, "a" + index)).ToArray();
            var varExprs = innerParamTypes.Select((paramType, index) => Expression.Variable(paramType, "v" + index)).ToArray();

            var topExprs = new List<Expression>();
            for (var index = 0; index < varExprs.Length; index++)
            {
                if (paramTypes[index].IsByRef)
                {
                    // ReSharper disable AssignNullToNotNullAttribute

                    var constructor = innerParamTypes[index].GetConstructor(new[] { paramTypes[index].GetElementType() });
                    topExprs.Add(Expression.Assign(varExprs[index], Expression.New(constructor, paramExprs[index])));

                    // ReSharper restore AssignNullToNotNullAttribute
                }
                else
                {
                    topExprs.Add(Expression.Assign(varExprs[index], paramExprs[index]));
                }
            }

            var innerDelegate = CreateSimpleDelegate(engine, target, innerDelegateType);
            var invokeExpr = Expression.Invoke(Expression.Constant(innerDelegate), varExprs);

            var finallyExprs = new List<Expression>();
            for (var index = 0; index < varExprs.Length; index++)
            {
                if (paramTypes[index].IsByRef)
                {
                    // ReSharper disable AssignNullToNotNullAttribute

                    var member = innerParamTypes[index].GetProperty("Value");
                    var resultExpr = Expression.MakeMemberAccess(varExprs[index], member);
                    finallyExprs.Add(Expression.Assign(paramExprs[index], resultExpr));

                    // ReSharper restore AssignNullToNotNullAttribute
                }
            }

            var finallyBlockExpr = Expression.Block(finallyExprs);
            topExprs.Add(Expression.TryFinally(invokeExpr, finallyBlockExpr));

            var topBlockExpr = Expression.Block(method.ReturnType, varExprs, topExprs);
            return Expression.Lambda(delegateType, topBlockExpr, paramExprs).Compile();

            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore CoVariantArrayConversion
        }

        private abstract class DelegateShim
        {
            public abstract Delegate Delegate { get; }

            protected ScriptEngine Engine { get; private set; }

            protected DelegateShim(ScriptEngine engine)
            {
                Engine = engine;
            }

            protected static bool GetAllByValue(params Type[] types)
            {
                return !types.Any(type => typeof(IByRefArg).IsAssignableFrom(type));
            }

            protected static object GetArgValue(object arg)
            {
                var byRefArg = arg as IByRefArg;
                return (byRefArg != null) ? byRefArg.Value : arg;
            }

            protected static void SetArgValue(object arg, object value)
            {
                var byRefArg = arg as IByRefArg;
                if (byRefArg != null)
                {
                    byRefArg.Value = value;
                }
            }

            protected static object GetCompatibleTarget(Type delegateType, object target)
            {
                var del = target as Delegate;
                if ((del != null) && (del.GetType() != delegateType))
                {
                    // The target is a delegate of a different type from the one we are creating.
                    // Normally we expect the target to be a script function (COM object), but
                    // since we'll be invoking it dynamically, there's no need to restrict it.
                    // However, if the target is a delegate of a type that's inaccessible to this
                    // assembly, dynamic invocation fails. Strangely, delegate type accessibility
                    // doesn't seem to affect delegate creation, so we can work around this by
                    // creating a compatible delegate based on the properties of the target.

                    return Delegate.CreateDelegate(delegateType, del.Target, del.Method);
                }

                return target;
            }
        }

        private abstract class ProcShim : DelegateShim
        {
            private readonly Action<Action> invoker;

            protected ProcShim(ScriptEngine engine)
                : base(engine)
            {
                if (engine == null)
                {
                    invoker = action => action();
                }
                else
                {
                    invoker = engine.SyncInvoke;
                }
            }

            protected void Invoke(Action action)
            {
                invoker(action);
            }
        }

        private abstract class FuncShim<TResult> : DelegateShim
        {
            private readonly Func<Func<TResult>, TResult> invoker;

            protected FuncShim(ScriptEngine engine)
                : base(engine)
            {
                if (engine == null)
                {
                    invoker = func => func();
                }
                else
                {
                    invoker = engine.SyncInvoke;
                }
            }

            protected TResult Invoke(Func<TResult> func)
            {
                return invoker(func);
            }
        }
    }
}
