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

        private static readonly ConcurrentDictionary<BindSignature, object> coreBindCache = new ConcurrentDictionary<BindSignature, object>();
        private static long coreBindCount;

        #endregion

        #region internal members

        private object InvokeMethod(string name, object[] args, object[] bindArgs)
        {
            var typeArgs = GetTypeArgs(args).ToArray();
            if (typeArgs.Length > 0)
            {
                args = args.Skip(typeArgs.Length).ToArray();
                bindArgs = bindArgs.Skip(typeArgs.Length).ToArray();
            }

            return InvokeMethod(name, typeArgs, args, bindArgs);
        }

        private object InvokeMethod(string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            var bindResult = BindMethod(name, typeArgs, args, bindArgs);
            if ((bindResult is MethodBindFailure) && target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
            {
                var targetArg = new[] { target.Target };
                var extensionArgs = targetArg.Concat(args).ToArray();

                var targetBindArg = new object[] { target };
                var extensionBindArgs = targetBindArg.Concat(bindArgs).ToArray();

                foreach (var type in cachedExtensionMethodSummary.Types)
                {
                    var extensionHostItem = (HostItem)Wrap(engine, HostType.Wrap(type));
                    var extensionBindResult = extensionHostItem.BindMethod(name, typeArgs, extensionArgs, extensionBindArgs);
                    if (extensionBindResult is MethodBindSuccess)
                    {
                        return extensionBindResult.Invoke(engine);
                    }
                }
            }

            return bindResult.Invoke(engine);
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
            // WARNING: BindSignature holds on to the specified typeArgs; subsequent modification
            // will result in bugs that are difficult to diagnose. Create a copy if necessary.

            var context = accessContext ?? engine.AccessContext;
            var signature = new BindSignature(context, target, name, typeArgs, bindArgs);
            MethodBindResult result;

            object rawResult;
            if (engine.TryGetCachedBindResult(signature, out rawResult))
            {
                result = MethodBindResult.Create(name, rawResult, target, args);
            }
            else
            {
                result = BindMethodInternal(context, name, typeArgs, args, bindArgs);
                if (!result.IsPreferredMethod(name))
                {
                    if (result is MethodBindSuccess)
                    {
                        result = new MethodBindFailure(() => new MissingMemberException(MiscHelpers.FormatInvariant("Object has no method named '{0}' that matches the specified arguments", name)));
                    }

                    foreach (var altName in GetAltMethodNames(name))
                    {
                        var altResult = BindMethodInternal(context, altName, typeArgs, args, bindArgs);
                        if (altResult.IsUnblockedMethod())
                        {
                            result = altResult;
                            break;
                        }
                    }
                }

                engine.CacheBindResult(signature, result.RawResult);
            }

            return result;
        }

        private MethodBindResult BindMethodInternal(Type context, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            var signature = new BindSignature(context, target, name, typeArgs, bindArgs);
            MethodBindResult result;

            object rawResult;
            if (coreBindCache.TryGetValue(signature, out rawResult))
            {
                result = MethodBindResult.Create(name, rawResult, target, args);
            }
            else
            {
                result = BindMethodCore(context, name, typeArgs, args, bindArgs);
                coreBindCache.TryAdd(signature, result.RawResult);
            }

            return result;
        }

        private MethodBindResult BindMethodCore(Type context, string name, Type[] typeArgs, object[] args, object[] bindArgs)
        {
            Interlocked.Increment(ref coreBindCount);

            // create C# member invocation binder
            const CSharpBinderFlags binderFlags = CSharpBinderFlags.InvokeSimpleName | CSharpBinderFlags.ResultDiscarded;
            var binder = Binder.InvokeMember(binderFlags, name, typeArgs, context, CreateArgInfoEnum(bindArgs));

            // perform default binding
            var binding = DynamicHelpers.Bind((DynamicMetaObjectBinder)binder, target, bindArgs);
            var rawResult = (new MethodBindingVisitor(target.InvokeTarget, name, binding.Expression)).Result;

            var result = MethodBindResult.Create(name, rawResult, target, args);
            if (!(result is MethodBindSuccess) && !(target is HostType) && target.Type.IsInterface)
            {
                // binding through interface failed; try base interfaces
                foreach (var interfaceType in target.Type.GetInterfaces())
                {
                    var tempTarget = HostObject.Wrap(target.InvokeTarget, interfaceType);
                    binding = DynamicHelpers.Bind((DynamicMetaObjectBinder)binder, tempTarget, bindArgs);
                    rawResult = (new MethodBindingVisitor(target.InvokeTarget, name, binding.Expression)).Result;

                    var tempResult = MethodBindResult.Create(name, rawResult, target, args);
                    if (tempResult is MethodBindSuccess)
                    {
                        return tempResult;
                    }
                }

                // binding through base interfaces failed; try System.Object
                var objectTarget = HostObject.Wrap(target.InvokeTarget, typeof(object));
                binding = DynamicHelpers.Bind((DynamicMetaObjectBinder)binder, objectTarget, bindArgs);
                rawResult = (new MethodBindingVisitor(target.InvokeTarget, name, binding.Expression)).Result;

                var objectResult = MethodBindResult.Create(name, rawResult, target, args);
                if (objectResult is MethodBindSuccess)
                {
                    return objectResult;
                }
            }

            return result;
        }

        private IEnumerable<string> GetAltMethodNames(string name)
        {
            return GetAltMethodNamesInternal(name).Distinct();
        }

        private IEnumerable<string> GetAltMethodNamesInternal(string name)
        {
            foreach (var method in target.Type.GetScriptableMethods(name, GetMethodBindFlags()))
            {
                var methodName = method.GetShortName();
                if (methodName != name)
                {
                    yield return methodName;
                }
            }
        }

        private IEnumerable<CSharpArgumentInfo> CreateArgInfoEnum(object[] args)
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
                if (arg is IOutArg)
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

            public abstract bool IsPreferredMethod(string name);

            public abstract bool IsUnblockedMethod();

            public abstract object Invoke(ScriptEngine engine);
        }

        #endregion

        #region Nested type: MethodBindSuccess

        private class MethodBindSuccess : MethodBindResult
        {
            private static readonly MethodInfo getTypeMethod = typeof(object).GetMethod("GetType");

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

            public override bool IsPreferredMethod(string name)
            {
                return !method.IsBlockedFromScript() && (method.GetScriptName() == name);
            }

            public override bool IsUnblockedMethod()
            {
                return !method.IsBlockedFromScript();
            }

            public override object Invoke(ScriptEngine engine)
            {
                if (method == getTypeMethod)
                {
                    engine.CheckReflection();
                }

                return InvokeHelpers.InvokeMethod(engine, hostTarget.InvokeTarget, method, args);
            }

            #endregion
        }

        #endregion

        #region Nested type: MethodBindFailure

        private class MethodBindFailure : MethodBindResult
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

            public override bool IsPreferredMethod(string name)
            {
                return false;
            }

            public override bool IsUnblockedMethod()
            {
                return false;
            }

            public override object Invoke(ScriptEngine engine)
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
