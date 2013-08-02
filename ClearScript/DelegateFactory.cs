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

            var typeArgs = Enumerable.Repeat(typeof(object), argCount).Concat(new[] { typeof(TResult) }).ToArray();
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

            var paramTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();
            if (paramTypes.Any(paramType => paramType.IsByRef))
            {
                return CreateComplexDelegate(engine, target, delegateType);
            }

            return CreateSimpleDelegate(engine, target, delegateType);
        }

        private static Delegate CreateSimpleDelegate(ScriptEngine engine, object target, Type delegateType)
        {
            var method = delegateType.GetMethod("Invoke");
            var paramTypes = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

            Type shimType;
            if (method.ReturnType == typeof(void))
            {
                var typeArgs = paramTypes.Concat(new[] { delegateType }).ToArray();
                shimType = procShimTemplates[paramTypes.Length].MakeSpecificType(typeArgs);
            }
            else
            {
                var typeArgs = paramTypes.Concat(new[] { method.ReturnType, delegateType }).ToArray();
                shimType = funcShimTemplates[paramTypes.Length].MakeSpecificType(typeArgs);
            }

            var shim = (DelegateShim)shimType.CreateInstance(engine, target);
            return shim.Delegate;
        }

        private static Delegate CreateComplexDelegate(ScriptEngine engine, object target, Type delegateType)
        {
            // ReSharper disable CoVariantArrayConversion

            var method = delegateType.GetMethod("Invoke");

            var parameters = method.GetParameters();
            var paramTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

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
                var typeArgs = innerParamTypes.Concat(new[] { method.ReturnType }).ToArray();
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
                    topExprs.Add(Expression.Assign(varExprs[index], Expression.New(constructor, new[] { paramExprs[index] })));

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
                    var member = innerParamTypes[index].GetProperty("Value");
                    var resultExpr = Expression.MakeMemberAccess(varExprs[index], member);
                    finallyExprs.Add(Expression.Assign(paramExprs[index], resultExpr));
                }
            }

            var finallyBlockExpr = Expression.Block(finallyExprs);
            topExprs.Add(Expression.TryFinally(invokeExpr, finallyBlockExpr));

            var topBlockExpr = Expression.Block(method.ReturnType, varExprs, topExprs);
            return Expression.Lambda(delegateType, topBlockExpr, paramExprs).Compile();

            // ReSharper restore CoVariantArrayConversion
        }

        private abstract class DelegateShim
        {
            public abstract Delegate Delegate { get; }

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
