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
using System.Reflection;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript
{
    internal interface IByRefArg
    {
        Type Type { get; }
        object Value { get; set; }
    }

    internal interface IOutArg : IByRefArg
    {
    }

    internal interface IRefArg : IByRefArg
    {
    }

    internal abstract class ByRefArg<T> : HostTarget, IByRefArg
    {
        private readonly HostVariable<T> target;

        protected ByRefArg(HostVariable<T> target)
        {
            this.target = target;
        }

        public T Value
        {
            get { return target.Value; }

            set { target.Value = value; }
        }

        #region HostTarget overrides

        public override Type Type
        {
            get { return target.Type; }
        }

        public override object Target
        {
            get { return this; }
        }

        public override object InvokeTarget
        {
            get { return target.InvokeTarget; }
        }

        public override object DynamicInvokeTarget
        {
            get { return target.DynamicInvokeTarget; }
        }

        public override HostTargetFlags Flags
        {
            get { return target.Flags; }
        }

        public override string[] GetAuxMethodNames(BindingFlags bindFlags)
        {
            return target.GetAuxMethodNames(bindFlags);
        }

        public override string[] GetAuxPropertyNames(BindingFlags bindFlags)
        {
            return target.GetAuxPropertyNames(bindFlags);
        }

        public override bool TryInvokeAuxMember(ScriptEngine engine, string name, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvokeAuxMember(engine, name, invokeFlags, args, bindArgs, out result);
        }

        public override bool TryInvoke(ScriptEngine engine, BindingFlags invokeFlags, object[] args, object[] bindArgs, out object result)
        {
            return target.TryInvoke(engine, invokeFlags, args, bindArgs, out result);
        }

        #endregion

        #region IByRefArg implementation

        object IByRefArg.Value
        {
            get { return target.Value; }

            set { ((IHostVariable)target).Value = value; }
        }

        #endregion
    }

    internal class OutArg<T> : ByRefArg<T>, IOutArg
    {
        public OutArg(HostVariable<T> target)
            : base(target)
        {
        }

        public OutArg(T initValue)
            : this(new HostVariable<T>(initValue))
        {
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("out {0}", Type.GetFriendlyName());
        }

        #endregion
    }

    internal class RefArg<T> : ByRefArg<T>, IRefArg
    {
        public RefArg(HostVariable<T> target)
            : base(target)
        {
        }

        public RefArg(T initValue)
            : this(new HostVariable<T>(initValue))
        {
        }

        #region Object overrides

        public override string ToString()
        {
            return MiscHelpers.FormatInvariant("ref {0}", Type.GetFriendlyName());
        }

        #endregion
    }
}
