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
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.Expando;

namespace Microsoft.ClearScript.Util
{
    internal static class DynamicHelpers
    {
        public static DynamicMetaObject Bind(DynamicMetaObjectBinder binder, object target, object[] args)
        {
            return binder.Bind(CreateDynamicTarget(target), CreateDynamicArgs(args));
        }

        public static object InvokeExpression(Expression expression)
        {
            Debug.Assert(expression != null);
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        public static bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object target, object[] args, out object result)
        {
            try
            {
                // For COM property access, use IReflect/IExpando if possible. This works around
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

                var binding = Bind(binder, target, args);
                result = InvokeExpression(binding.Expression);
                return true;
            }
            catch (ApplicationException)
            {
                result = null;
                return false;
            }
        }

        private static bool TryGetProperty(IReflect target, string name, bool ignoreCase, object[] args, out object result)
        {
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

        private static DynamicMetaObject CreateDynamicTarget(object target)
        {
            var byRefArg = target as IByRefArg;
            if (byRefArg != null)
            {
                return DynamicMetaObject.Create(byRefArg.Value, Expression.Parameter(byRefArg.Type.MakeByRefType()));
            }

            var hostTarget = target as HostTarget;
            if (hostTarget == null)
            {
                return DynamicMetaObject.Create(target, Expression.Constant(target));
            }

            target = hostTarget.DynamicInvokeTarget;
            if (hostTarget is HostType)
            {
                return DynamicMetaObject.Create(target, Expression.Constant(target));
            }

            return DynamicMetaObject.Create(target, Expression.Constant(target, hostTarget.Type));
        }

        private static DynamicMetaObject CreateDynamicArg(object arg)
        {
            var byRefArg = arg as IByRefArg;
            if (byRefArg != null)
            {
                return DynamicMetaObject.Create(byRefArg.Value, Expression.Parameter(byRefArg.Type.MakeByRefType()));
            }

            if (arg is HostType)
            {
                return DynamicMetaObject.Create(arg, Expression.Constant(arg));
            }

            var hostTarget = arg as HostTarget;
            if (hostTarget == null)
            {
                return DynamicMetaObject.Create(arg, Expression.Constant(arg));
            }

            arg = hostTarget.Target;
            return DynamicMetaObject.Create(arg, Expression.Constant(arg, hostTarget.Type));
        }

        private static DynamicMetaObject[] CreateDynamicArgs(object[] args)
        {
            return args.Select(CreateDynamicArg).ToArray();
        }
    }
}
