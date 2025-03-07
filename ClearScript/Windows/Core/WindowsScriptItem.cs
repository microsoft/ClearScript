// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;
using Microsoft.ClearScript.Util.COM;

namespace Microsoft.ClearScript.Windows.Core
{
    internal class WindowsScriptItem : ScriptItem, IWindowsScriptObject, IDictionary<string, object>, IWindowsScriptItemTag
    {
        private readonly WindowsScriptEngine engine;
        private readonly IDispatchEx target;
        private WindowsScriptItem holder;
        private readonly InterlockedOneWayFlag disposedFlag = new();

        private WindowsScriptItem(WindowsScriptEngine engine, IDispatchEx target)
        {
            this.engine = engine;
            this.target = target;
        }

        public static object Wrap(WindowsScriptEngine engine, object obj)
        {
            Debug.Assert(obj is not IScriptMarshalWrapper);

            if (obj is null)
            {
                return null;
            }

            if ((obj is IDispatchEx target) && (obj.GetType().IsCOMObject))
            {
                return (engine is IJavaScriptEngine) ? new WindowsJavaScriptObject(engine, target) : new WindowsScriptItem(engine, target);
            }

            return obj;
        }

        private IScriptEngineException GetScriptError(Exception exception)
        {
            if (TryGetScriptError(exception, out var scriptError))
            {
                return scriptError;
            }

            return new ScriptEngineException(engine.Name, exception.Message, null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception);
        }

        private bool TryGetScriptError(Exception exception, out IScriptEngineException scriptError)
        {
            // WORKAROUND: Windows Script items often throw ugly exceptions. The code here
            // attempts to clean up specific cases.

            while (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }

            scriptError = exception as IScriptEngineException;
            if (scriptError is not null)
            {
                return true;
            }

            if (exception is COMException comException)
            {
                var result = comException.ErrorCode;
                if (((result == HResult.SCRIPT_E_REPORTED) || (result == HResult.CLEARSCRIPT_E_HOSTEXCEPTION)) && (engine.CurrentScriptFrame is not null))
                {
                    scriptError = engine.CurrentScriptFrame.ScriptError ?? engine.CurrentScriptFrame.PendingScriptError;
                    if (scriptError is not null)
                    {
                        return true;
                    }

                    var hostException = engine.CurrentScriptFrame.HostException;
                    if (hostException is not null)
                    {
                        scriptError = new ScriptEngineException(engine.Name, hostException.Message, null, HResult.CLEARSCRIPT_E_HOSTEXCEPTION, false, true, null, hostException);
                        return true;
                    }
                }
                else if (HResult.GetFacility(result) == HResult.FACILITY_CONTROL)
                {
                    // These exceptions often have awful messages that include COM error codes.
                    // The script engine itself may be able to provide a better message.

                    if (engine.RuntimeErrorMap.TryGetValue(HResult.GetCode(result), out var runtimeErrorMessage) && (runtimeErrorMessage != exception.Message))
                    {
                        scriptError = new ScriptEngineException(engine.Name, runtimeErrorMessage, null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                        return true;
                    }

                    if (engine.SyntaxErrorMap.TryGetValue(HResult.GetCode(result), out var syntaxErrorMessage) && (syntaxErrorMessage != exception.Message))
                    {
                        scriptError = new ScriptEngineException(engine.Name, syntaxErrorMessage, null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                        return true;
                    }
                }
                else if ((result == HResult.DISP_E_MEMBERNOTFOUND) || (result == HResult.DISP_E_UNKNOWNNAME))
                {
                    // this usually indicates invalid object or property access in JScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                    return true;
                }
            }
            else
            {
                if ((exception is ArgumentException argumentException) && (argumentException.ParamName is null))
                {
                    // this usually indicates invalid object or property access in VBScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                    return true;
                }
            }

            return false;
        }

        private void VerifyNotDisposed()
        {
            if (disposedFlag.IsSet)
            {
                throw new ObjectDisposedException(ToString());
            }
        }

        private bool TryGetProperty(string name, out object value)
        {
            VerifyNotDisposed();

            var ctx = (self: this, name, value: (object)null);

            var found = engine.ScriptInvoke(
                static pCtx =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    try
                    {
                        ctx.value = ctx.self.target.GetProperty(ctx.name, false);
                        return ctx.value is not Nonexistent;
                    }
                    catch (Exception exception)
                    {
                        if (!ctx.name.IsDispIDName(out _) && (exception.HResult != HResult.DISP_E_UNKNOWNNAME))
                        {
                            // Property retrieval failed, but a method with the given name exists;
                            // create a tear-off method. This currently applies only to VBScript.

                            ctx.value = new ScriptMethod(ctx.self, ctx.name);
                            return true;
                        }

                        return false;
                    }
                },
                StructPtr.FromRef(ref ctx)
            );

            if (found)
            {
                var result = engine.MarshalToHost(ctx.value, false);
                if ((result is WindowsScriptItem resultScriptItem) && (resultScriptItem.engine == engine))
                {
                    resultScriptItem.holder = this;
                }

                value = result;
                return true;
            }

            value = null;
            return false;
        }

        #region ScriptItem overrides

        protected override bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            VerifyNotDisposed();

            var succeeded = DynamicHelpers.TryBindAndInvoke(binder, target, args, out result);
            if (!succeeded)
            {
                if ((result is Exception exception) && (engine.CurrentScriptFrame is not null))
                {
                    var scriptError = exception as IScriptEngineException ?? GetScriptError(exception);

                    if (scriptError.ExecutionStarted && (binder.GetType().FullName != "Microsoft.VisualBasic.CompilerServices.VBGetBinder"))
                    {
                        throw (Exception)scriptError;
                    }

                    engine.CurrentScriptFrame.ScriptError = scriptError;
                }

                result = null;
                return false;
            }

            return true;
        }

        protected override object[] AdjustInvokeArgs(object[] args)
        {
            // WORKAROUND: JScript seems to require at least one argument to invoke a function
            return ((engine is IJScriptEngine) && (args.Length < 1)) ? new object[] { Undefined.Value } : args;
        }

        public override string[] GetPropertyNames()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static target => target.GetPropertyNames().ExcludeIndices().ToArray(), target);
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static target => target.GetPropertyNames().GetIndices().ToArray(), target);
        }

        #endregion

        #region ScriptObject overrides

        public override object GetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();

            var result = engine.MarshalToHost(
                engine.ScriptInvoke(
                    static ctx =>
                    {
                        try
                        {
                            var value = ctx.self.target.GetProperty(ctx.name, false, ctx.self.engine.MarshalToScript(ctx.args));
                            return (value is Nonexistent) ? Undefined.Value : value;
                        }
                        catch (Exception exception)
                        {
                            if (!ctx.name.IsDispIDName(out _) && (exception.HResult != HResult.DISP_E_UNKNOWNNAME))
                            {
                                // Property retrieval failed, but a method with the given name exists;
                                // create a tear-off method. This currently applies only to VBScript.

                                return new ScriptMethod(ctx.self, ctx.name);
                            }

                            return Undefined.Value;
                        }
                    },
                    (self: this, name, args)
                ),
                false
            );

            if ((result is WindowsScriptItem resultScriptItem) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();
            engine.ScriptInvoke(static ctx => ctx.self.target.SetProperty(ctx.name, false, ctx.self.engine.MarshalToScript(ctx.args)), (self: this, name, args));
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static ctx => ctx.target.DeleteProperty(ctx.name, false), (target, name));
        }

        public override object GetProperty(int index)
        {
            VerifyNotDisposed();
            return GetProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        public override void SetProperty(int index, object value)
        {
            VerifyNotDisposed();
            SetProperty(index.ToString(CultureInfo.InvariantCulture), value);
        }

        public override bool DeleteProperty(int index)
        {
            VerifyNotDisposed();
            return DeleteProperty(index.ToString(CultureInfo.InvariantCulture));
        }

        public override object Invoke(bool asConstructor, params object[] args)
        {
            VerifyNotDisposed();

            var engineInternal = (ScriptObject)engine.Global.GetProperty("EngineInternal");

            if (asConstructor)
            {
                return engineInternal.InvokeMethod("invokeConstructor", this, args);
            }

            return engineInternal.InvokeMethod("invokeMethod", holder, this, args);
        }

        public override object InvokeMethod(string name, params object[] args)
        {
            VerifyNotDisposed();

            try
            {
                return engine.MarshalToHost(engine.ScriptInvoke(static ctx => ctx.self.target.InvokeMethod(ctx.name, false, ctx.self.engine.MarshalToScript(ctx.args)), (self: this, name, args)), false);
            }
            catch (Exception exception)
            {
                if (TryGetScriptError(exception, out var scriptError))
                {
                    throw (Exception)scriptError;
                }

                throw;
            }
        }

        #endregion

        #region Object overrides

        public override bool Equals(object obj) => (obj is WindowsScriptItem that) && (engine == that.engine) && target.Equals(that.target);

        public override int GetHashCode() => target.GetHashCode();

        #endregion

        #region IScriptMarshalWrapper implementation

        public override ScriptEngine Engine => engine;

        public override object Unwrap()
        {
            return target;
        }

        #endregion

        #region IWindowsScriptObject implementation

        object IWindowsScriptObject.GetUnderlyingObject()
        {
            var pUnkTarget = Marshal.GetIUnknownForObject(target);
            var clone = Marshal.GetObjectForIUnknown(pUnkTarget);
            Marshal.Release(pUnkTarget);
            return clone;
        }

        #endregion

        #region IDictionary<string, object> implementation

        private IDictionary<string, object> ThisDictionary => this;

        private IEnumerable<string> PropertyKeys => GetPropertyKeys();

        private IEnumerable<KeyValuePair<string, object>> KeyValuePairs => PropertyKeys.Select(name => new KeyValuePair<string, object>(name, GetProperty(name)));

        private string[] GetPropertyKeys()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(static target => target.GetPropertyNames().ToArray(), target);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            // ReSharper disable once NotDisposedResourceIsReturned
            return KeyValuePairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // ReSharper disable once NotDisposedResourceIsReturned
            return ThisDictionary.GetEnumerator();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            SetProperty(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            PropertyKeys.ForEach(name => DeleteProperty(name));
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return TryGetProperty(item.Key, out var value) && Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            var source = KeyValuePairs.ToArray();
            Array.Copy(source, 0, array, arrayIndex, source.Length);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return ThisDictionary.Contains(item) && DeleteProperty(item.Key);
        }

        int ICollection<KeyValuePair<string, object>>.Count => PropertyKeys.Count();

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        void IDictionary<string, object>.Add(string key, object value)
        {
            SetProperty(key, value);
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return PropertyKeys.Contains(key);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            return DeleteProperty(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return TryGetProperty(key, out value);
        }

        object IDictionary<string, object>.this[string key]
        {
            get => TryGetProperty(key, out var value) ? value : throw new KeyNotFoundException();
            set => SetProperty(key, value);
        }

        ICollection<string> IDictionary<string, object>.Keys => PropertyKeys.ToList();

        ICollection<object> IDictionary<string, object>.Values => PropertyKeys.Select(name => GetProperty(name)).ToList();

        #endregion

        #region IDisposable implementation

        public override void Dispose()
        {
            if (disposedFlag.Set())
            {
                Marshal.ReleaseComObject(target);
            }
        }

        #endregion

        #region Nested type: WindowsJavaScriptObject

        private sealed class WindowsJavaScriptObject : WindowsScriptItem, IJavaScriptObject
        {
            public WindowsJavaScriptObject(WindowsScriptEngine engine, IDispatchEx target)
                : base(engine, target)
            {
            }

            #region IJavaScriptObject implementation

            public JavaScriptObjectKind Kind => JavaScriptObjectKind.Unknown;

            public JavaScriptObjectFlags Flags => JavaScriptObjectFlags.None;

            #endregion
        }

        #endregion
    }
}
