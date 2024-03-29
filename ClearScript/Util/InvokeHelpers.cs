// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
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
            return InvokeMethodInternal(context, method, target, args, (invokeMethod, invokeTarget, invokeArgs) => invokeMethod.Invoke(invokeTarget, invokeArgs), method.ReturnType, flags);
        }

        public static object InvokeConstructor(IHostContext context, ConstructorInfo constructor, object[] args)
        {
            return InvokeMethodInternal(context, constructor, null, args, (invokeConstructor, invokeTarget, invokeArgs) => invokeConstructor.Invoke(invokeArgs), constructor.DeclaringType, ScriptMemberFlags.None);
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

            if ((target != null) && invokeFlags.HasFlag(BindingFlags.InvokeMethod))
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
                            if (byRefArg == null)
                            {
                                tailArgs.SetValue(GetCompatibleArg(param.Name, tailArgType, args[innerIndex]), innerIndex - index);
                            }
                            else
                            {
                                tailArgs.SetValue(GetCompatibleArg(param.Name, tailArgType, byRefArg.Value), innerIndex - index);
                                byRefArgInfo.Add(new ByRefArgItem(byRefArg, tailArgs, innerIndex - index));
                            }
                        }

                        argList.Add(tailArgs);
                        tailArgsArg = tailArgs;
                        break;
                    }
                }

                if ((index < args.Length) && !(args[index] is Missing))
                {
                    var byRefArg = args[index] as IByRefArg;
                    if (byRefArg == null)
                    {
                        argList.Add(GetCompatibleArg(param, args[index]));
                    }
                    else
                    {
                        argList.Add(GetCompatibleArg(param, byRefArg.Value));
                        byRefArgInfo.Add(new ByRefArgItem(byRefArg, null, index));
                    }
                }
                else if (param.IsOptional)
                {
                    if (param.Attributes.HasFlag(ParameterAttributes.HasDefault))
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

        private static object GetCompatibleArg(ParameterInfo param, object value)
        {
            return GetCompatibleArg(param.Name, param.ParameterType, value);
        }

        private static object GetCompatibleArg(string paramName, Type type, object value)
        {
            if (!type.IsAssignableFromValue(ref value))
            {
                throw new ArgumentException(MiscHelpers.FormatInvariant("Invalid argument specified for parameter '{0}'", paramName));
            }

            return value;
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
    }
}
