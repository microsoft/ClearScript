// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region data

        private static readonly ConcurrentDictionary<BindSignature, object> coreBindCache = new();
        private static long coreBindCount;

        #endregion

        #region internal members

        private object InvokeMethod(string name, object[] args, object[] bindArgs)
        {
            var typeArgs = GetTypeArgs(args).ToArray();
            if (typeArgs.Length > 0)
            {
                var mergedArgs = args;
                var argOffset = typeArgs.Length;

                args = args.Skip(argOffset).ToArray();
                bindArgs = bindArgs.Skip(argOffset).ToArray();

                var result = InvokeMethod(name, typeArgs, args, bindArgs);
                for (var index = 0; index < args.Length; index++)
                {
                    mergedArgs[argOffset + index] = args[index];
                }

                return result;
            }

            return InvokeMethod(name, typeArgs, args, bindArgs);
        }

        private object InvokeMethod(string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            var bindResult = BindMethod(name, typeArgs, args, bindArgs);
            if (!bindResult.IsSuccess && Target.GetFlags(this).HasAllFlags(HostTargetFlags.AllowExtensionMethods))
            {
                var targetArg = Target.Target.ToEnumerable();
                var extensionArgs = targetArg.Concat(args).ToArray();

                var targetBindArg = new object[] { Target };
                var extensionBindArgs = targetBindArg.Concat(bindArgs).ToArray();

                foreach (var type in ExtensionMethodSummary.Types)
                {
                    var extensionHostItem = (HostItem)Wrap(Engine, HostType.Wrap(type));
                    var extensionBindResult = extensionHostItem.BindMethod(name, typeArgs, extensionArgs, extensionBindArgs);
                    if (extensionBindResult.IsSuccess)
                    {
                        var result = extensionBindResult.Invoke(extensionHostItem);
                        for (var index = 1; index < extensionArgs.Length; index++)
                        {
                            args[index - 1] = extensionArgs[index];
                        }

                        return result;
                    }
                }
            }

            return bindResult.Invoke(this);
        }

        private static IEnumerable<Type> GetTypeArgs(object[] args)
        {
            foreach (var arg in args)
            {
                var hostType = arg as HostType;
                if (hostType is null)
                {
                    yield break;
                }

                var typeArg = hostType.GetTypeArgNoThrow();
                if (typeArg is null)
                {
                    yield break;
                }

                yield return typeArg;
            }
        }

        private MethodBindResult BindMethod(string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            var bindFlags = GetMethodBindFlags();

            // WARNING: BindSignature holds on to the specified typeArgs; subsequent modification
            // will result in bugs that are difficult to diagnose. Create a copy if necessary.

            var signature = new BindSignature(AccessContext, bindFlags, Target, name, typeArgs, bindArgs);
            MethodBindResult result;

            if (Engine.TryGetCachedMethodBindResult(signature, out var rawResult))
            {
                result = MethodBindResult.Create(name, bindFlags, rawResult, Target, args);
            }
            else
            {
                var forceReflection = Engine.DisableDynamicBinding;

                if (forceReflection)
                {
                    result = MethodBindResult.CreateFailure(() => new MissingMethodException(MiscHelpers.FormatInvariant("The object has no method named '{0}' that matches the specified arguments", name)));
                }
                else
                {
                    result = BindMethodInternal(signature, AccessContext, bindFlags, Target, name, typeArgs, args, bindArgs);
                    if (!result.IsPreferredMethod(this, name))
                    {
                        if (result.IsSuccess)
                        {
                            result = MethodBindResult.CreateFailure(() => new MissingMethodException(MiscHelpers.FormatInvariant("The object has no method named '{0}' that matches the specified arguments", name)));
                        }

                        foreach (var altName in GetAltMethodNames(name, bindFlags))
                        {
                            var altResult = BindMethodInternal(null, AccessContext, bindFlags, Target, altName, typeArgs, args, bindArgs);
                            if (altResult.IsUnblockedMethod(this))
                            {
                                result = altResult;
                                break;
                            }
                        }
                    }
                }

                if (!result.IsSuccess && (forceReflection || Engine.UseReflectionBindFallback))
                {
                    var reflectionResult = BindMethodUsingReflection(bindFlags, Target, name, typeArgs, args, bindArgs);
                    if (reflectionResult.IsSuccess || forceReflection)
                    {
                        result = reflectionResult;
                    }
                }

                Engine.CacheMethodBindResult(signature, result.RawResult);
            }

            return result;
        }

        private static MethodBindResult BindMethodInternal(BindSignature signature, Type bindContext, BindingFlags bindFlags, HostTarget target, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            if (signature is null)
            {
                // WARNING: BindSignature holds on to the specified typeArgs; subsequent modification
                // will result in bugs that are difficult to diagnose. Create a copy if necessary.

                signature = new BindSignature(bindContext, bindFlags, target, name, typeArgs, bindArgs);
            }

            MethodBindResult result;

            if (coreBindCache.TryGetValue(signature, out var rawResult))
            {
                result = MethodBindResult.Create(name, bindFlags, rawResult, target, args);
            }
            else
            {
                result = BindMethodCore(bindContext, bindFlags, target, name, typeArgs, args, bindArgs);
                coreBindCache.TryAdd(signature, result.RawResult);
            }

            return result;
        }

        private static MethodBindResult BindMethodCore(Type bindContext, BindingFlags bindFlags, HostTarget target, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            Interlocked.Increment(ref coreBindCount);

            // create C# member invocation binder
            const CSharpBinderFlags binderFlags = CSharpBinderFlags.InvokeSimpleName | CSharpBinderFlags.ResultDiscarded;
            var binder = (InvokeMemberBinder)Binder.InvokeMember(binderFlags, name, typeArgs, bindContext, CreateArgInfoEnum(target, bindArgs));

            // perform default binding
            var rawResult = BindMethodRaw(bindFlags, binder, target, bindArgs);

            var result = MethodBindResult.Create(name, bindFlags, rawResult, target, args);
            if (!result.IsSuccess && (target is not HostType) && target.Type.IsInterface)
            {
                // binding through interface failed; try base interfaces
                foreach (var interfaceType in target.Type.GetInterfaces())
                {
                    var baseInterfaceTarget = HostObject.Wrap(target.InvokeTarget, interfaceType);
                    rawResult = BindMethodRaw(bindFlags, binder, baseInterfaceTarget, bindArgs);

                    var baseInterfaceResult = MethodBindResult.Create(name, bindFlags, rawResult, target, args);
                    if (baseInterfaceResult.IsSuccess)
                    {
                        return baseInterfaceResult;
                    }
                }

                // binding through base interfaces failed; try System.Object
                var objectTarget = HostObject.Wrap(target.InvokeTarget, typeof(object));
                rawResult = BindMethodRaw(bindFlags, binder, objectTarget, bindArgs);

                var objectResult = MethodBindResult.Create(name, bindFlags, rawResult, target, args);
                if (objectResult.IsSuccess)
                {
                    return objectResult;
                }
            }

            return result;
        }

        private static object BindMethodRaw(BindingFlags bindFlags, InvokeMemberBinder binder, HostTarget target, object[] bindArgs)
        {
            var expr = DynamicHelpers.Bind(binder, target, bindArgs).Expression;

            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode

            if (expr is null)
            {
                return new Func<Exception>(() => new MissingMethodException(MiscHelpers.FormatInvariant("The object has no method named '{0}'", binder.Name)));
            }

            // ReSharper restore HeuristicUnreachableCode
            // ReSharper restore ConditionIsAlwaysTrueOrFalse

            if (expr.NodeType == ExpressionType.Dynamic)
            {
                // The binding result is a dynamic call, which is indicative of COM interop. This
                // sort of binding is not very useful here; it can't be resolved to a MethodInfo
                // instance, and caching it is problematic because it includes argument bindings.
                // Falling back to reflection should work in most cases because COM interfaces
                // support neither generic nor overloaded methods.

                try
                {
                    var method = target.Type.GetMethod(binder.Name, bindFlags);
                    return (object)method ?? new Func<Exception>(() => new MissingMethodException(MiscHelpers.FormatInvariant("The object has no method named '{0}'", binder.Name)));
                }
                catch (AmbiguousMatchException exception)
                {
                    return new Func<Exception>(() => new AmbiguousMatchException(exception.Message));
                }
            }

            return (new MethodBindingVisitor(target.InvokeTarget, binder.Name, expr)).Result;
        }

        private IEnumerable<string> GetAltMethodNames(string name, BindingFlags bindFlags)
        {
            return GetAltMethodNamesInternal(name, bindFlags).Distinct();
        }

        private IEnumerable<string> GetAltMethodNamesInternal(string name, BindingFlags bindFlags)
        {
            foreach (var method in Target.Type.GetScriptableMethods(this, name, bindFlags))
            {
                var methodName = method.GetShortName();
                if (methodName != name)
                {
                    yield return methodName;
                }
            }
        }

        private static IEnumerable<CSharpArgumentInfo> CreateArgInfoEnum(HostTarget target, object[] args)
        {
            if (target is HostType)
            {
                yield return CreateStaticTypeArgInfo();
            }
            else
            {
                yield return CreateArgInfo(target.DynamicInvokeTarget);
            }

            foreach (var arg in args)
            {
                yield return CreateArgInfo(arg);
            }
        }

        private static CSharpArgumentInfo CreateArgInfo(object arg)
        {
            var flags = CSharpArgumentInfoFlags.None;
            if (arg is not null)
            {
                flags |= CSharpArgumentInfoFlags.UseCompileTimeType;
                if (arg is HostObject hostObject)
                {
                    if ((hostObject.Type == typeof(int)) || (hostObject.Type.IsValueType && hostObject.Target.IsZero()))
                    {
                        flags |= CSharpArgumentInfoFlags.Constant;
                    }
                }
                else if (arg is HostVariable hostVariable)
                {
                    if ((hostVariable.Type == typeof(int)) || (hostVariable.Type.IsValueType && hostVariable.Target.IsZero()))
                    {
                        flags |= CSharpArgumentInfoFlags.Constant;
                    }
                }
                else if (arg is int || arg.IsZero())
                {
                    flags |= CSharpArgumentInfoFlags.Constant;
                }
                else if (arg is IOutArg)
                {
                    flags |= CSharpArgumentInfoFlags.IsOut;
                }
                else if (arg is IRefArg)
                {
                    flags |= CSharpArgumentInfoFlags.IsRef;
                }
            }

            return CSharpArgumentInfo.Create(flags, null);
        }

        private static CSharpArgumentInfo CreateStaticTypeArgInfo()
        {
            return CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType, null);
        }

        private MethodBindResult BindMethodUsingReflection(BindingFlags bindFlags, HostTarget hostTarget, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            var candidates = GetReflectionCandidates(bindFlags, hostTarget, name, typeArgs).Distinct().ToArray();
            if (candidates.Length > 0)
            {
                try
                {
                    var rawResult = TypeHelpers.BindToMember(this, candidates, bindFlags, args, bindArgs);
                    if (rawResult is not null)
                    {
                        return MethodBindResult.Create(name, bindFlags, rawResult, hostTarget, args);
                    }
                }
                catch (AmbiguousMatchException)
                {
                    return MethodBindResult.CreateFailure(() => new AmbiguousMatchException(MiscHelpers.FormatInvariant("The object has multiple methods named '{0}' that match the specified arguments", name)));
                }
            }

            return MethodBindResult.CreateFailure(() => new MissingMethodException(MiscHelpers.FormatInvariant("The object has no method named '{0}' that matches the specified arguments", name)));
        }

        private IEnumerable<MethodInfo> GetReflectionCandidates(BindingFlags bindFlags, HostTarget hostTarget, string name, Type[] typeArgs)
        {
            foreach (var method in GetReflectionCandidates(bindFlags, hostTarget.Type, name, typeArgs))
            {
                yield return method;
            }

            if ((hostTarget is not HostType) && hostTarget.Type.IsInterface)
            {
                foreach (var interfaceType in hostTarget.Type.GetInterfaces())
                {
                    foreach (var method in GetReflectionCandidates(bindFlags, interfaceType, name, typeArgs))
                    {
                        yield return method;
                    }
                }

                foreach (var method in GetReflectionCandidates(bindFlags, typeof(object), name, typeArgs))
                {
                    yield return method;
                }
            }
        }

        private IEnumerable<MethodInfo> GetReflectionCandidates(BindingFlags bindFlags, Type type, string name, Type[] typeArgs)
        {
            foreach (var method in type.GetScriptableMethods(this, name, bindFlags))
            {
                MethodInfo tempMethod = null;

                if (method.ContainsGenericParameters)
                {
                    try
                    {
                        tempMethod = method.MakeGenericMethod(typeArgs);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    catch (NotSupportedException)
                    {
                        continue;
                    }
                }
                else if (typeArgs.Length < 1)
                {
                    tempMethod = method;
                }

                if ((tempMethod is not null) && !tempMethod.ContainsGenericParameters)
                {
                    yield return tempMethod;
                }
            }
        }

        #endregion

        #region unit test support

        internal static void ClearCoreBindCache()
        {
            coreBindCache.Clear();
            Interlocked.Exchange(ref coreBindCount, 0);
        }

        internal static long GetCoreBindCount()
        {
            return Interlocked.Read(ref coreBindCount);
        }

        #endregion

        #region Nested type: MethodBindResult

        private readonly ref struct MethodBindResult
        {
            private readonly HostTarget hostTarget;
            private readonly MethodInfo method;
            private readonly object[] args;
            private readonly Func<Exception> exceptionFactory;

            private static readonly MethodInfo[] reflectionMethods =
            {
                typeof(object).GetMethod("GetType"),
                typeof(System.Runtime.InteropServices._Exception).GetMethod("GetType"),
                typeof(Exception).GetMethod("GetType")
            };

            private MethodBindResult(HostTarget hostTarget, MethodInfo method, object[] args)
            {
                this.hostTarget = hostTarget;
                this.method = method;
                this.args = args;
                exceptionFactory = null;
            }

            private MethodBindResult(Func<Exception> exceptionFactory)
            {
                hostTarget = null;
                method = null;
                args = null;
                this.exceptionFactory = exceptionFactory;
            }

            public static MethodBindResult Create(string name, BindingFlags bindFlags, object rawResult, HostTarget hostTarget, object[] args)
            {
                var method = rawResult as MethodInfo;
                if (method != null)
                {
                    if (method.IsStatic && !bindFlags.HasAllFlags(BindingFlags.Static))
                    {
                        return new MethodBindResult(() => new InvalidOperationException(MiscHelpers.FormatInvariant("Cannot access static method '{0}' in non-static context", method.Name)));
                    }

                    return new MethodBindResult(hostTarget, method, args);
                }

                return new MethodBindResult((rawResult as Func<Exception>) ?? (() => new NotSupportedException(MiscHelpers.FormatInvariant("Invocation of method '{0}' failed (unrecognized binding)", name))));
            }

            public static MethodBindResult CreateFailure(Func<Exception> exceptionFactory) => new(exceptionFactory);

            public bool IsSuccess => method != null;

            public object RawResult => IsSuccess ? method : exceptionFactory;

            public bool IsPreferredMethod(HostItem hostItem, string name)
            {
                return IsSuccess && IsUnblockedMethod(hostItem) && (method.GetScriptName(hostItem) == name);
            }

            public bool IsUnblockedMethod(HostItem hostItem)
            {
                return IsSuccess && !method.IsBlockedFromScript(hostItem, hostItem.DefaultAccess);
            }

            public object Invoke(HostItem hostItem)
            {
                if (!IsSuccess)
                {
                    throw exceptionFactory();
                }

                if (reflectionMethods.Contains(method, MemberComparer<MethodInfo>.Instance))
                {
                    hostItem.Engine.CheckReflection();
                }

                return InvokeHelpers.InvokeMethod(hostItem, method, hostTarget.InvokeTarget, args, method.GetScriptMemberFlags(hostItem));
            }
        }

        #endregion

        #region Nested type: MethodBindingVisitor

        private sealed class MethodBindingVisitor : ExpressionVisitor
        {
            private readonly object target;
            private readonly string name;
            private readonly List<object> results = new();

            public MethodBindingVisitor(object target, string name, Expression expression)
            {
                this.target = target;
                this.name = name;

                Visit(expression);
                if (results.Count != 1)
                {
                    results.Clear();
                    AddResult(() => new NotSupportedException(MiscHelpers.FormatInvariant("Invocation of method '{0}' failed (unrecognized binding)", name)));
                }
                else
                {
                    var method = results[0] as MethodInfo;
                    if (method is not null)
                    {
                        Debug.Assert(method.Name == name);
                    }
                }
            }

            public object Result => results[0];

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == name)
                {
                    AddResult(node.Method);
                }

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                if (target is Delegate targetDelegate)
                {
                    var del = DynamicHelpers.Invoke(node.Expression) as Delegate;
                    if (del == targetDelegate)
                    {
                        AddResult(del.GetType().GetMethod("Invoke"));
                    }
                }

                return base.VisitInvocation(node);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Throw)
                {
                    if (DynamicHelpers.Invoke(node.Operand) is Exception)
                    {
                        AddResult(() => (Exception)DynamicHelpers.Invoke(node.Operand));
                    }
                }

                return base.VisitUnary(node);
            }

            private void AddResult(MethodInfo method)
            {
                results.Add(method);
            }

            private void AddResult(Func<Exception> exceptionFactory)
            {
                results.Add(exceptionFactory);
            }
        }

        #endregion
    }
}
