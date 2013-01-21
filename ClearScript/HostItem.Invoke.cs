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
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.ClearScript.Util;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Microsoft.ClearScript
{
    internal partial class HostItem
    {
        #region internal members

        private object InvokeMethod(string name, object[] args)
        {
            var typeArgs = GetTypeArgs(args).ToArray();
            if (typeArgs.Length > 0)
            {
                args = args.Skip(typeArgs.Length).ToArray();
            }

            return InvokeMethod(name, typeArgs, args);
        }

        private object InvokeMethod(string name, Type[] typeArgs, object[] args)
        {
            var bindResult = BindMethod(name, typeArgs, args);
            if ((bindResult is MethodBindFailure) && target.Flags.HasFlag(HostTargetFlags.AllowExtensionMethods))
            {
                var targetArg = new[] { target.DynamicInvokeTarget };
                var extensionArgs = targetArg.Concat(args).ToArray();
                foreach (var type in cachedExtensionMethodSummary.Types)
                {
                    var extensionHostItem = (HostItem)Wrap(engine, HostType.Wrap(type));
                    var extensionBindResult = extensionHostItem.BindMethod(name, typeArgs, extensionArgs);
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

        private MethodBindResult BindMethod(string name, Type[] typeArgs, object[] args)
        {
            MethodBindResult result = null;

            // WARNING: BindSignature holds on to the specified typeArgs; subsequent modification
            // will result in bugs that are difficult to diagnose. Create a copy if necessary.

            object rawResult;
            var signature = new BindSignature(target, name, typeArgs, args);
            if (engine.TryGetCachedBindResult(signature, out rawResult))
            {
                result = MethodBindResult.Create(name, rawResult, target, args);
            }
            else
            {
                var entryList = new List<MethodBindEntry>();
                if ((target is HostType) || (target.InvokeTarget == null) || !target.Type.IsInterface)
                {
                    entryList.Add(new MethodBindEntry(name, accessContext ?? engine.AccessContext));
                }
                else
                {
                    foreach (var mapping in target.InvokeTarget.GetType().ExtGetInterfaceMaps(target.Type))
                    {
                        for (var index = 0; index < mapping.InterfaceMethods.Length; index++)
                        {
                            if (mapping.InterfaceMethods[index].Name == name)
                            {
                                var targetMethod = mapping.TargetMethods[index];
                                entryList.Add(new MethodBindEntry(targetMethod.Name, targetMethod.DeclaringType));
                            }
                        }
                    }

                    if (entryList.Count < 1)
                    {
                        entryList.Add(new MethodBindEntry(name, accessContext ?? engine.AccessContext));
                    }
                }

                foreach (var entry in entryList)
                {
                    const CSharpBinderFlags binderFlags = CSharpBinderFlags.InvokeSimpleName | CSharpBinderFlags.ResultDiscarded;
                    var binder = Binder.InvokeMember(binderFlags, entry.Name, typeArgs, entry.AccessContext, CreateArgInfoEnum(args));

                    var binding = DynamicHelpers.Bind((DynamicMetaObjectBinder)binder, target.DynamicInvokeTarget, args);
                    rawResult = (new MethodBindingVisitor(target.InvokeTarget, entry.Name, binding.Expression)).Result;
                    result = MethodBindResult.Create(entry.Name, rawResult, target, args);
                    if (result is MethodBindSuccess)
                    {
                        break;
                    }
                }

                Debug.Assert(rawResult != null);
                engine.CacheBindResult(signature, rawResult);
            }

            return result;
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
            if (arg is IOutArg)
            {
                flags |= CSharpArgumentInfoFlags.IsOut | CSharpArgumentInfoFlags.UseCompileTimeType;
            }
            else if (arg is IRefArg)
            {
                flags |= CSharpArgumentInfoFlags.IsRef | CSharpArgumentInfoFlags.UseCompileTimeType;
            }

            return CSharpArgumentInfo.Create(flags, null);
        }

        private static CSharpArgumentInfo CreateStaticTypeArgInfo()
        {
            return CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType, null);
        }

        #endregion

        #region Nested type: MethodBindEntry

        private struct MethodBindEntry
        {
            public readonly string Name;

            public readonly Type AccessContext;

            public MethodBindEntry(string name, Type accessContext)
            {
                Name = name;
                AccessContext = accessContext;
            }
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
                        return new MethodBindFailure(new InvalidOperationException(MiscHelpers.FormatInvariant("Cannot access static method '{0}' in non-static context", method.Name)));
                    }

                    return new MethodBindSuccess(hostTarget, method, args);
                }

                return new MethodBindFailure((rawResult as Exception) ?? new NotSupportedException(MiscHelpers.FormatInvariant("Invocation of method '{0}' failed (unrecognized binding)", name)));
            }

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

            public override object Invoke(ScriptEngine engine)
            {
                if (method == getTypeMethod)
                {
                    engine.CheckReflection();
                }

                return InvokeHelpers.InvokeMethod(hostTarget.InvokeTarget, method, args);
            }

            #endregion
        }

        #endregion

        #region Nested type: MethodBindFailure

        private class MethodBindFailure : MethodBindResult
        {
            private readonly Exception exception;

            public MethodBindFailure(Exception exception)
            {
                this.exception = exception;
            }

            #region MethodBindResult overrides

            public override object Invoke(ScriptEngine engine)
            {
                throw exception;
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
                    results.Add(new NotSupportedException(MiscHelpers.FormatInvariant("Invocation of method '{0}' failed (unrecognized binding)", name)));
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
                    results.Add(node.Method);
                }

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitInvocation(InvocationExpression node)
            {
                var targetDelegate = target as Delegate;
                if (targetDelegate != null)
                {
                    var del = DynamicHelpers.InvokeExpression(node.Expression) as Delegate;
                    if (del == targetDelegate)
                    {
                        results.Add(del.GetType().GetMethod("Invoke"));
                    }
                }

                return base.VisitInvocation(node);
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Throw)
                {
                    var exception = DynamicHelpers.InvokeExpression(node.Operand) as Exception;
                    if (exception != null)
                    {
                        results.Add(exception);
                    }
                }

                return base.VisitUnary(node);
            }
        }

        #endregion
    }
}
