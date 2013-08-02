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
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal abstract class ScriptItem : DynamicObject, IDynamic, IScriptMarshalWrapper
    {
        public abstract ScriptEngine Engine { get; }

        protected abstract bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result);

        protected virtual object[] AdjustInvokeArgs(object[] args)
        {
            return args;
        }

        private bool TryWrappedBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            object tempResult = null;
            var succeeded = Engine.ScriptInvoke(() =>
            {
                if (!TryBindAndInvoke(binder, Engine.MarshalToScript(args), out tempResult))
                {
                    Engine.ThrowScriptError();
                    return false;
                }

                return true;
            });

            if (succeeded)
            {
                result = Engine.MarshalToHost(tempResult, false).ToDynamicResult(Engine);
                return true;
            }

            result = null;
            return false;
        }

        private bool TryWrappedInvokeOrInvokeMember(DynamicMetaObjectBinder binder, ParameterInfo[] parameters, object[] args, out object result)
        {
            Type[] paramTypes = null;
            if ((parameters != null) && (parameters.Length >= args.Length))
            {
                paramTypes = parameters.Skip(parameters.Length - args.Length).Select(parameter => parameter.ParameterType).ToArray();
            }

            if (paramTypes != null)
            {
                for (var index = 0; index < paramTypes.Length; index++)
                {
                    var paramType = paramTypes[index];
                    if (paramType.IsByRef)
                    {
                        args[index] = typeof(HostVariable<>).MakeSpecificType(paramType.GetElementType()).CreateInstance(args[index]);
                    }
                }
            }

            if (TryWrappedBindAndInvoke(binder, AdjustInvokeArgs(args), out result))
            {
                if (paramTypes != null)
                {
                    for (var index = 0; index < paramTypes.Length; index++)
                    {
                        if (paramTypes[index].IsByRef)
                        {
                            var hostVariable = args[index] as IHostVariable;
                            if (hostVariable != null)
                            {
                                args[index] = hostVariable.Value;
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }

        #region DynamicObject overrides

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetPropertyNames().Concat(GetPropertyIndices().Select(index => index.ToString(CultureInfo.InvariantCulture)));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryWrappedBindAndInvoke(binder, MiscHelpers.GetEmptyArray<object>(), out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object ignoredResult;
            return TryWrappedBindAndInvoke(binder, new[] { value }, out ignoredResult);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indices, out object result)
        {
            return TryWrappedBindAndInvoke(binder, indices, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indices, object value)
        {
            object ignoredResult;
            return TryWrappedBindAndInvoke(binder, indices.Concat(new[] { value }).ToArray(), out ignoredResult);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var parameters = new StackFrame(1, false).GetMethod().GetParameters();
            return TryWrappedInvokeOrInvokeMember(binder, parameters, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var parameters = new StackFrame(1, false).GetMethod().GetParameters();
            return TryWrappedInvokeOrInvokeMember(binder, parameters, args, out result);
        }

        #endregion

        #region IDynamic implementation (abstract)

        public abstract object GetProperty(string name);
        public abstract void SetProperty(string name, object value);
        public abstract bool DeleteProperty(string name);
        public abstract string[] GetPropertyNames();
        public abstract object GetProperty(int index);
        public abstract void SetProperty(int index, object value);
        public abstract bool DeleteProperty(int index);
        public abstract int[] GetPropertyIndices();
        public abstract object Invoke(object[] args, bool asConstructor);
        public abstract object InvokeMethod(string name, object[] args);

        #endregion

        #region IScriptMarshalWrapper implementation (abstract)

        public abstract object Unwrap();

        #endregion
    }
}
