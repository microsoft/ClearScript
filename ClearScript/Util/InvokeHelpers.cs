// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.ClearScript.Util
{
    internal static class InvokeHelpers
    {
        public static object InvokeMethod(IHostContext context, MethodInfo method, object target, object[] args, ScriptMemberFlags flags)
        {
            return InvokeMethodInternal(context, method, target, args, static (method, target, args) => method.Invoke(target, args), method.ReturnType, flags);
        }

        public static object InvokeConstructor(IHostContext context, ConstructorInfo constructor, object[] args)
        {
            return InvokeMethodInternal(context, constructor, null, args, static (constructor, _, args) => constructor.Invoke(args), constructor.DeclaringType, ScriptMemberFlags.None);
        }

        public static object InvokeDelegate(IHostContext context, Delegate del, object[] args)
        {
            return InvokeMethod(context, del.GetType().GetMethod("Invoke"), del, args, ScriptMemberFlags.None);
        }

        public static bool TryInvokeObject(IHostContext context, object target, BindingFlags invokeFlags, object[] args, object[] bindArgs, bool tryDynamic, out object result)
        {
            if (target is HostTarget hostTarget)
            {
                if (hostTarget.TryInvoke(context, invokeFlags, args, bindArgs, out result))
                {
                    return true;
                }

                if (hostTarget is HostType)
                {
                    return false;
                }

                target = hostTarget.InvokeTarget;
                tryDynamic = tryDynamic && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(hostTarget.Type);
            }

            if ((target is not null) && invokeFlags.HasAllFlags(BindingFlags.InvokeMethod))
            {
                if (target is ScriptItem scriptItem)
                {
                    target = DelegateFactory.CreateFunc<object>(scriptItem.Engine, target, args.Length);
                }

                if (target is Delegate del)
                {
                    result = InvokeDelegate(context, del, args);
                    return true;
                }

                if (tryDynamic)
                {
                    if (target is IDynamicMetaObjectProvider dynamicMetaObjectProvider)
                    {
                        if (dynamicMetaObjectProvider.GetMetaObject(Expression.Constant(target)).TryInvoke(context, args, out result))
                        {
                            return true;
                        }
                    }
                }
            }

            result = null;
            return false;
        }

        private static object InvokeMethodInternal<T>(IHostContext context, T method, object target, object[] args, Func<T, object, object[], object> invoker, Type returnType, ScriptMemberFlags flags) where T : MethodBase
        {
            var argList = new List<object>();
            var byRefArgInfo = new List<ByRefArgItem>();
            object tailArgsArg = null;

            var parameters = method.GetParameters();
            for (var index = 0; index < parameters.Length; index++)
            {
                var param = parameters[index];
                if (CustomAttributes.Has<ParamArrayAttribute>(context, param, false))
                {
                    if ((index != (args.Length - 1)) || !param.ParameterType.IsInstanceOfType(args[index]))
                    {
                        var tailArgType = param.ParameterType.GetElementType();
                        var tailArgs = Array.CreateInstance(tailArgType, Math.Max(args.Length - index, 0));
                        for (var innerIndex = index; innerIndex < args.Length; innerIndex++)
                        {
                            var byRefArg = args[innerIndex] as IByRefArg;
                            if (byRefArg is null)
                            {
                                tailArgs.SetValue(CompatibleArg.Get(param.Name, tailArgType, args[innerIndex]), innerIndex - index);
                            }
                            else
                            {
                                tailArgs.SetValue(CompatibleArg.Get(param.Name, tailArgType, byRefArg.Value), innerIndex - index);
                                byRefArgInfo.Add(new ByRefArgItem(byRefArg, tailArgs, innerIndex - index));
                            }
                        }

                        argList.Add(tailArgs);
                        tailArgsArg = tailArgs;
                        break;
                    }
                }

                if ((index < args.Length) && (args[index] is not Missing))
                {
                    var byRefArg = args[index] as IByRefArg;
                    if (byRefArg is null)
                    {
                        argList.Add(CompatibleArg.Get(param, args[index]));
                    }
                    else
                    {
                        argList.Add(CompatibleArg.Get(param, byRefArg.Value));
                        byRefArgInfo.Add(new ByRefArgItem(byRefArg, null, index));
                    }
                }
                else if (param.IsOptional)
                {
                    if (param.Attributes.HasAllFlags(ParameterAttributes.HasDefault))
                    {
                        try
                        {
                            argList.Add(param.DefaultValue);
                        }
                        catch (FormatException)
                        {
                            // undocumented but observed when calling HostFunctions.newVar()
                            argList.Add(null);
                        }
                    }
                    else
                    {
                        argList.Add(Missing.Value);
                    }
                }
                else
                {
                    break;
                }
            }

            var finalArgs = argList.ToArray();
            var result = invoker(method, target, finalArgs);

            foreach (var item in byRefArgInfo)
            {
                var array = item.Array ?? finalArgs;
                item.ByRefArg.Value = array.GetValue(item.Index);
            }

            for (var index = 0; index < finalArgs.Length; index++)
            {
                if (index >= args.Length)
                {
                    break;
                }

                var finalArg = finalArgs[index];
                if (ReferenceEquals(finalArg, tailArgsArg))
                {
                    break;
                }

                args[index] = finalArg;
            }

            if (returnType == typeof(void))
            {
                return context.Engine.VoidResultValue;
            }

            return context.Engine.PrepareResult(result, returnType, flags, false);
        }

        #region Nested type: ByRefArgItem

        private sealed class ByRefArgItem
        {
            public IByRefArg ByRefArg { get; }

            public Array Array { get; }

            public int Index { get; }

            public ByRefArgItem(IByRefArg arg, Array array, int index)
            {
                ByRefArg = arg;
                Array = array;
                Index = index;
            }
        }

        #endregion

        #region Nested type: CompatibleArg

        private abstract class CompatibleArg
        {
            private static readonly ConcurrentDictionary<Type, CompatibleArg> map = new();

            public static object Get(ParameterInfo param, object value)
            {
                return Get(param.Name, param.ParameterType, value);
            }

            public static object Get(string paramName, Type type, object value)
            {
                return type.IsAssignableFromValue(ref value) ? value : GetImpl(type).Get(paramName, value);
            }
            public abstract object Get(string paramName, object value);

            private static CompatibleArg GetImpl(Type type)
            {
                return map.GetOrAdd(type, static type => (CompatibleArg)Activator.CreateInstance(typeof(Impl<>).MakeGenericType(type)));
            }

            #region Nested type: Impl<T>

            private sealed class Impl<T> : CompatibleArg
            {
                public override object Get(string paramName, object value)
                {
                    if (value is T result)
                    {
                        return result;
                    }

                    throw new ArgumentException(MiscHelpers.FormatInvariant("Invalid argument specified for parameter '{0}'", paramName));
                }
            }

            #endregion
        }

        #endregion
    }
}
