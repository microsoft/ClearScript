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
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region data

        private static readonly ConcurrentDictionary<BindSignature, object> coreBindCache = new ConcurrentDictionary<BindSignature, object>();
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
            if ((bindResult is MethodBindFailure) && target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
            {
                var targetArg = target.Target.ToEnumerable();
                var extensionArgs = targetArg.Concat(args).ToArray();

                var targetBindArg = new object[] { target };
                var extensionBindArgs = targetBindArg.Concat(bindArgs).ToArray();

                foreach (var type in ExtensionMethodSummary.Types)
                {
                    var extensionHostItem = (HostItem)Wrap(engine, HostType.Wrap(type));
                    var extensionBindResult = extensionHostItem.BindMethod(name, typeArgs, extensionArgs, extensionBindArgs);
                    if (extensionBindResult is MethodBindSuccess)
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
                if (hostType == null)
                {
                    yield break;
                }

                var typeArg = hostType.GetTypeArgNoThrow();
                if (typeArg == null)
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

            var signature = new BindSignature(accessContext, bindFlags, target, name, typeArgs, bindArgs);
            MethodBindResult result;

            object rawResult;
            if (engine.TryGetCachedBindResult(signature, out rawResult))
            {
                result = MethodBindResult.Create(name, rawResult, target, args);
            }
            else
            {
                result = BindMethodInternal(accessContext, bindFlags, target, name, typeArgs, args, bindArgs);
                if (!result.IsPreferredMethod(this, name))
                {
                    if (result is MethodBindSuccess)
                    {
                        result = new MethodBindFailure(() => new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}' that matches the specified arguments", name)));
                    }

                    foreach (var altName in GetAltMethodNames(name, bindFlags))
                    {
                        var altResult = BindMethodInternal(accessContext, bindFlags, target, altName, typeArgs, args, bindArgs);
                        if (altResult.IsUnblockedMethod(this))
                        {
                            result = altResult;
                            break;
                        }
                    }
                }

                if ((result is MethodBindFailure) && engine.UseReflectionBindFallback)
                {
                    var reflectionResult = BindMethodUsingReflection(bindFlags, target, name, typeArgs, args);
                    if (reflectionResult is MethodBindSuccess)
                    {
                        result = reflectionResult;
                    }
                }

                engine.CacheBindResult(signature, result.RawResult);
            }

            return result;
        }

        private static MethodBindResult BindMethodInternal(Type bindContext, BindingFlags bindFlags, HostTarget target, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            // WARNING: BindSignature holds on to the specified typeArgs; subsequent modification
            // will result in bugs that are difficult to diagnose. Create a copy if necessary.

            var signature = new BindSignature(bindContext, bindFlags, target, name, typeArgs, bindArgs);
            MethodBindResult result;

            object rawResult;
            if (coreBindCache.TryGetValue(signature, out rawResult))
            {
                result = MethodBindResult.Create(name, rawResult, target, args);
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

            var result = MethodBindResult.Create(name, rawResult, target, args);
            if ((result is MethodBindFailure) && !(target is HostType) && target.Type.IsInterface)
            {
                // binding through interface failed; try base interfaces
                foreach (var interfaceType in target.Type.GetInterfaces())
                {
                    var baseInterfaceTarget = HostObject.Wrap(target.InvokeTarget, interfaceType);
                    rawResult = BindMethodRaw(bindFlags, binder, baseInterfaceTarget, bindArgs);

                    var baseInterfaceResult = MethodBindResult.Create(name, rawResult, target, args);
                    if (baseInterfaceResult is MethodBindSuccess)
                    {
                        return baseInterfaceResult;
                    }
                }

                // binding through base interfaces failed; try System.Object
                var objectTarget = HostObject.Wrap(target.InvokeTarget, typeof(object));
                rawResult = BindMethodRaw(bindFlags, binder, objectTarget, bindArgs);

                var objectResult = MethodBindResult.Create(name, rawResult, target, args);
                if (objectResult is MethodBindSuccess)
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

            if (expr == null)
            {
                return new Func<Exception>(() => new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}'", binder.Name)));
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
                    return (object)method ?? new Func<Exception>(() => new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}'", binder.Name)));
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
            foreach (var method in target.Type.GetScriptableMethods(name, bindFlags, accessContext, defaultAccess))
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
            if (arg != null)
            {
                flags |= CSharpArgumentInfoFlags.UseCompileTimeType;
                if (arg is int)
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

        private MethodBindResult BindMethodUsingReflection(BindingFlags bindFlags, HostTarget hostTarget, string name, Type[] typeArgs, object[] args)
        {
            // ReSharper disable CoVariantArrayConversion
            // ReSharper disable PossibleNullReferenceException

            var candidates = GetReflectionCandidates(bindFlags, hostTarget, name, typeArgs).Distinct().ToArray();
            if (candidates.Length > 0)
            {
                try
                {
                    object state;
                    var rawResult = Type.DefaultBinder.BindToMethod(bindFlags, candidates, ref args, null, null, null, out state);
                    return MethodBindResult.Create(name, rawResult, hostTarget, args);
                }
                catch (MissingMethodException)
                {
                }
                catch (AmbiguousMatchException)
                {
                }
            }

            return new MethodBindFailure(() => new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}' that matches the specified arguments", name)));

            // ReSharper restore PossibleNullReferenceException
            // ReSharper restore CoVariantArrayConversion
        }

        private IEnumerable<MethodInfo> GetReflectionCandidates(BindingFlags bindFlags, HostTarget hostTarget, string name, Type[] typeArgs)
        {
            foreach (var method in GetReflectionCandidates(bindFlags, hostTarget.Type, name, typeArgs))
            {
                yield return method;
            }

            if (!(hostTarget is HostType) && hostTarget.Type.IsInterface)
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
            foreach (var method in type.GetScriptableMethods(name, bindFlags, accessContext, defaultAccess))
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

                if ((tempMethod != null) && !tempMethod.ContainsGenericParameters)
                {
                    yield return tempMethod;
                }
            }
        }

        #endregion

        #region unit test support

        internal static void ResetCoreBindCache()
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

        private abstract class MethodBindResult
        {
            public static MethodBindResult Create(string name, object rawResult, HostTarget hostTarget, object[] args)
            {
                var method = rawResult as MethodInfo;
                if (method != null)
                {
                    if ((method.IsStatic) && !hostTarget.Flags.HasFlag(HostTargetFlags.AllowStaticMembers))
                    {
                        return new MethodBindFailure(() => new InvalidOperationException(MiscHelpers.FormatInvariant("Cannot access static method '{0}' in non-static context", method.Name)));
                    }

                    return new MethodBindSuccess(hostTarget, method, args);
                }

                return new MethodBindFailure((rawResult as Func<Exception>) ?? (() => new NotSupportedException(MiscHelpers.FormatInvariant("Invocation of method '{0}' failed (unrecognized binding)", name))));
            }

            public abstract object RawResult { get; }

            public abstract bool IsPreferredMethod(HostItem hostItem, string name);

            public abstract bool IsUnblockedMethod(HostItem hostItem);

            public abstract object Invoke(HostItem hostItem);
        }

        #endregion

        #region Nested type: MethodBindSuccess

        private sealed class MethodBindSuccess : MethodBindResult
        {
            private static readonly MethodInfo[] reflectionMethods =
            {
                typeof(object).GetMethod("GetType"),
                typeof(_Exception).GetMethod("GetType"),
                typeof(Exception).GetMethod("GetType")
            };

            private readonly HostTarget hostTarget;
            private readonly MethodInfo method;
            private readonly object[] args;

            public MethodBindSuccess(HostTarget hostTarget, MethodInfo method, object[] args)
            {
                this.hostTarget = hostTarget;
                this.method = method;
                this.args = args;
            }

            #region MethodBindResult overrides

            public override object RawResult
            {
                get { return method; }
            }

            public override bool IsPreferredMethod(HostItem hostItem, string name)
            {
                return !method.IsBlockedFromScript(hostItem.DefaultAccess) && (method.GetScriptName() == name);
            }

            public override bool IsUnblockedMethod(HostItem hostItem)
            {
                return !method.IsBlockedFromScript(hostItem.DefaultAccess);
            }

            public override object Invoke(HostItem hostItem)
            {
                if (reflectionMethods.Contains(method, MemberComparer<MethodInfo>.Instance))
                {
                    hostItem.Engine.CheckReflection();
                }

                return InvokeHelpers.InvokeMethod(hostItem, hostTarget.InvokeTarget, method, args);
            }

            #endregion
        }

        #endregion

        #region Nested type: MethodBindFailure

        private sealed class MethodBindFailure : MethodBindResult
        {
            private readonly Func<Exception> exceptionFactory;

            public MethodBindFailure(Func<Exception> exceptionFactory)
            {
                this.exceptionFactory = exceptionFactory;
            }

            #region MethodBindResult overrides

            public override object RawResult
            {
                get { return exceptionFactory; }
            }

            public override bool IsPreferredMethod(HostItem hostItem, string name)
            {
                return false;
            }

            public override bool IsUnblockedMethod(HostItem hostItem)
            {
                return false;
            }

            public override object Invoke(HostItem hostItem)
            {
                throw exceptionFactory();
            }

            #endregion
        }

        #endregion

        #region Nested type: MethodBindingVisitor

        private sealed class MethodBindingVisitor : ExpressionVisitor
        {
            private readonly object target;
            private readonly string name;
            private readonly List<object> results = new List<object>();

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
                    if (method != null)
                    {
                        Debug.Assert(method.Name == name);
                    }
                }
            }

            public object Result
            {
                get { return results[0]; }
            }

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
                var targetDelegate = target as Delegate;
                if (targetDelegate != null)
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
                    var exception = DynamicHelpers.Invoke(node.Operand) as Exception;
                    if (exception != null)
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
