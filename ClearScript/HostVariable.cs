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
using System.Dynamic;
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal interface IHostVariable
    {
        Type Type { get; }
        object Value { get; set; }
    }

    internal abstract class HostVariableBase : HostTarget
    {
        private static readonly string[] auxPropertyNames = { "out", "ref", "value" };

        public override string[] GetAuxPropertyNames(BindingFlags bindFlags)
        {
            return auxPropertyNames;
        }
    }

    internal class HostVariable<T> : HostVariableBase, IHostVariable
    {
        private T value;

        public HostVariable(T initValue)
        {
            if ((typeof(T) == typeof(Undefined)) || (typeof(T) == typeof(VoidResult)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if (typeof(HostItem).IsAssignableFrom(typeof(T)) || typeof(HostTarget).IsAssignableFrom(typeof(T)))
            {
                throw new NotSupportedException("Unsupported variable type");
            }

            if ((initValue is HostItem) || (initValue is HostTarget))
            {
                throw new NotSupportedException("Unsupported value type");
            }

            value = initValue;
        }

        public T Value
        {
            // Be careful when renaming or deleting this property; it is accessed by name in the
            // expression tree construction code in DelegateFactory.CreateComplexDelegate().

            get { return value; }

            set { this.value = value; }
        }

        #region Object overrides

        public override string ToString()
        {
            var objectName = value.GetFriendlyName(typeof(T));
            return MiscHelpers.FormatInvariant("HostVariable:{0}", objectName);
        }

        #endregion

        #region HostTarget overrides

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override object Target
        {
            get { return value; }
        }

        public override object InvokeTarget
        {
            get { return value; }
        }

        public override object DynamicInvokeTarget
        {
            get { return value; }
        }

        public override HostTargetFlags Flags
        {
            get { return HostTargetFlags.AllowInstanceMembers | HostTargetFlags.AllowExtensionMethods; }
        }

        public override bool TryInvokeAuxMember(ScriptEngine engine, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            const BindingFlags getPropertyFlags =
                BindingFlags.GetField |
                BindingFlags.GetProperty;

            const BindingFlags setPropertyFlags =
                BindingFlags.SetProperty |
                BindingFlags.PutDispProperty |
                BindingFlags.PutRefDispProperty;

            if (name == "out")
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new OutArg<T>(this);
                    return true;
                }
            }
            else if (name == "ref")
            {
                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = new RefArg<T>(this);
                    return true;
                }
            }
            else if (name == "value")
            {
                if (invokeFlags.HasFlag(BindingFlags.InvokeMethod))
                {
                    if (InvokeHelpers.TryInvokeObject(engine, value, invokeFlags, args, bindArgs, typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)), out result))
                    {
                        return true;
                    }

                    if (invokeFlags.HasFlag(BindingFlags.GetField) && (args.Length < 1))
                    {
                        result = engine.PrepareResult(value, true);
                        return true;
                    }

                    result = null;
                    return false;
                }

                if ((invokeFlags & getPropertyFlags) != 0)
                {
                    result = engine.PrepareResult(value, true);
                    return true;
                }

                if ((invokeFlags & setPropertyFlags) != 0)
                {
                    if (args.Length == 1)
                    {
                        result = engine.PrepareResult(((IHostVariable)this).Value = args[0], typeof(T), true);
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region IHostVariable implementation

        object IHostVariable.Value
        {
            get { return value; }

            set
            {
                var tempValue = default(T);
                var succeeded = false;

                try
                {
                    tempValue = (T)value;
                    succeeded = true;
                }
                catch (InvalidCastException)
                {
                    try
                    {
                        tempValue = (T)Convert.ChangeType(value, typeof(T));
                        succeeded = true;
                    }
                    catch (InvalidCastException)
                    {
                    }
                    catch (FormatException)
                    {
                    }
                    catch (OverflowException)
                    {
                    }
                }

                if (!succeeded)
                {
                    throw new InvalidOperationException("Assignment invalid due to type mismatch");
                }

                if ((tempValue is HostItem) || (tempValue is HostTarget))
                {
                    throw new NotSupportedException("Unsupported value type");
                }

                this.value = tempValue;
            }
        }

        #endregion
    }
}
