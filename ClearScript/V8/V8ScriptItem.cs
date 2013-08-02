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
using System.Diagnostics;
using System.Dynamic;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8
{
    internal class V8ScriptItem : ScriptItem, IDisposable
    {
        private readonly V8ScriptEngine engine;
        private readonly IV8Object target;
        private V8ScriptItem holder;
        private bool disposed;

        private V8ScriptItem(V8ScriptEngine engine, IV8Object target)
        {
            this.engine = engine;
            this.target = target;
        }

        public static object Wrap(V8ScriptEngine engine, object obj)
        {
            Debug.Assert(!(obj is IScriptMarshalWrapper));

            if (obj == null)
            {
                return null;
            }

            var target = obj as IV8Object;
            if (target != null)
            {
                return new V8ScriptItem(engine, target);
            }

            return obj;
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        #region ScriptItem overrides

        public override ScriptEngine Engine
        {
            get { return engine; }
        }

        protected override bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            VerifyNotDisposed();

            try
            {
                var getMemberBinder = binder as GetMemberBinder;
                if (getMemberBinder != null)
                {
                    result = target.GetProperty(getMemberBinder.Name);
                    return true;
                }

                var setMemberBinder = binder as SetMemberBinder;
                if ((setMemberBinder != null) && (args != null) && (args.Length > 0))
                {
                    target.SetProperty(setMemberBinder.Name, args[0]);
                    result = args[0];
                    return true;
                }

                var getIndexBinder = binder as GetIndexBinder;
                if (getIndexBinder != null)
                {
                    if ((args != null) && (args.Length == 1))
                    {
                        int index;
                        if (MiscHelpers.TryGetIndex(args[0], out index))
                        {
                            result = target.GetProperty(index);
                        }
                        else
                        {
                            result = target.GetProperty(args[0].ToString());
                        }

                        return true;
                    }

                    throw new InvalidOperationException("Invalid argument or index count");
                }

                var setIndexBinder = binder as SetIndexBinder;
                if (setIndexBinder != null)
                {
                    if ((args != null) && (args.Length == 2))
                    {
                        int index;
                        if (MiscHelpers.TryGetIndex(args[0], out index))
                        {
                            target.SetProperty(index, args[1]);
                        }
                        else
                        {
                            target.SetProperty(args[0].ToString(), args[1]);
                        }

                        result = args[1];
                        return true;
                    }

                    throw new InvalidOperationException("Invalid argument or index count");
                }

                var invokeBinder = binder as InvokeBinder;
                if (invokeBinder != null)
                {
                    result = target.Invoke(args, false);
                    return true;
                }

                var invokeMemberBinder = binder as InvokeMemberBinder;
                if (invokeMemberBinder != null)
                {
                    result = target.InvokeMethod(invokeMemberBinder.Name, args);
                    return true;
                }
            }
            catch (Exception exception)
            {
                if (engine.CurrentScriptFrame != null)
                {
                    var scriptError = exception as IScriptEngineException;
                    if (scriptError != null)
                    {
                        engine.CurrentScriptFrame.ScriptError = scriptError;
                    }
                    else
                    {
                        engine.CurrentScriptFrame.ScriptError = new ScriptEngineException(engine.Name, exception.Message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, exception);
                    }
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region IDynamic implementation

        public override object GetProperty(string name)
        {
            VerifyNotDisposed();

            var result = engine.MarshalToHost(engine.ScriptInvoke(() => target.GetProperty(name)), false);

            var resultScriptItem = result as V8ScriptItem;
            if ((resultScriptItem != null) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, object value)
        {
            VerifyNotDisposed();
            engine.ScriptInvoke(() => target.SetProperty(name, engine.MarshalToScript(value)));
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.DeleteProperty(name));
        }

        public override string[] GetPropertyNames()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetPropertyNames());
        }

        public override object GetProperty(int index)
        {
            VerifyNotDisposed();
            return engine.MarshalToHost(engine.ScriptInvoke(() => target.GetProperty(index)), false);
        }

        public override void SetProperty(int index, object value)
        {
            VerifyNotDisposed();
            engine.ScriptInvoke(() => target.SetProperty(index, engine.MarshalToScript(value)));
        }

        public override bool DeleteProperty(int index)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.DeleteProperty(index));
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetPropertyIndices());
        }

        public override object Invoke(object[] args, bool asConstructor)
        {
            VerifyNotDisposed();

            if (asConstructor)
            {
                return engine.Script.EngineInternal.invokeConstructor(this, args);
            }

            return engine.Script.EngineInternal.invokeMethod(holder, this, args);
        }

        public override object InvokeMethod(string name, object[] args)
        {
            VerifyNotDisposed();
            return engine.MarshalToHost(engine.ScriptInvoke(() => target.InvokeMethod(name, engine.MarshalToScript(args))), false);
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public override object Unwrap()
        {
            return target;
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if (!disposed)
            {
                target.Dispose();
                disposed = true;
            }
        }

        #endregion
    }
}
