// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Expando;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.Windows
{
    internal sealed class WindowsScriptItem : ScriptItem, IWindowsScriptObject, IDisposable
    {
        private readonly WindowsScriptEngine engine;
        private readonly IExpando target;
        private WindowsScriptItem holder;
        private readonly InterlockedOneWayFlag disposedFlag = new InterlockedOneWayFlag();

        private WindowsScriptItem(WindowsScriptEngine engine, IExpando target)
        {
            this.engine = engine;
            this.target = target;
        }

        public static object Wrap(WindowsScriptEngine engine, object obj)
        {
            Debug.Assert(!(obj is IScriptMarshalWrapper));

            if (obj == null)
            {
                return null;
            }

            var expando = obj as IExpando;
            if ((expando != null) && (obj.GetType().IsCOMObject))
            {
                return new WindowsScriptItem(engine, expando);
            }

            return obj;
        }

        private IScriptEngineException GetScriptError(Exception exception)
        {
            IScriptEngineException scriptError;
            if (TryGetScriptError(exception, out scriptError))
            {
                return scriptError;
            }

            return new ScriptEngineException(engine.Name, exception.Message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception);
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
            if (scriptError != null)
            {
                return true;
            }

            var comException = exception as COMException;
            if (comException != null)
            {
                var result = comException.ErrorCode;
                if (((result == RawCOMHelpers.HResult.SCRIPT_E_REPORTED) || (result == RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION)) && (engine.CurrentScriptFrame != null))
                {
                    scriptError = engine.CurrentScriptFrame.ScriptError ?? engine.CurrentScriptFrame.PendingScriptError;
                    if (scriptError != null)
                    {
                        return true;
                    }

                    var hostException = engine.CurrentScriptFrame.HostException;
                    if (hostException != null)
                    {
                        scriptError = new ScriptEngineException(engine.Name, hostException.Message, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_HOSTEXCEPTION, false, true, null, hostException);
                        return true;
                    }
                }
                else if (RawCOMHelpers.HResult.GetFacility(result) == RawCOMHelpers.HResult.FACILITY_CONTROL)
                {
                    // These exceptions often have awful messages that include COM error codes.
                    // The engine itself may be able to provide a better message.

                    string runtimeErrorMessage;
                    if (engine.RuntimeErrorMap.TryGetValue(RawCOMHelpers.HResult.GetCode(result), out runtimeErrorMessage) && (runtimeErrorMessage != exception.Message))
                    {
                        scriptError = new ScriptEngineException(engine.Name, runtimeErrorMessage, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                        return true;
                    }

                    string syntaxErrorMessage;
                    if (engine.SyntaxErrorMap.TryGetValue(RawCOMHelpers.HResult.GetCode(result), out syntaxErrorMessage) && (syntaxErrorMessage != exception.Message))
                    {
                        scriptError = new ScriptEngineException(engine.Name, syntaxErrorMessage, null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                        return true;
                    }
                }
                else if ((result == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND) || (result == RawCOMHelpers.HResult.DISP_E_UNKNOWNNAME))
                {
                    // this usually indicates invalid object or property access in JScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
                    return true;
                }
            }
            else
            {
                var argumentException = exception as ArgumentException;
                if ((argumentException != null) && (argumentException.ParamName == null))
                {
                    // this usually indicates invalid object or property access in VBScript
                    scriptError = new ScriptEngineException(engine.Name, "Invalid object or property access", null, RawCOMHelpers.HResult.CLEARSCRIPT_E_SCRIPTITEMEXCEPTION, false, false, null, exception.InnerException);
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

        #region ScriptItem overrides

        protected override bool TryBindAndInvoke(DynamicMetaObjectBinder binder, object[] args, out object result)
        {
            VerifyNotDisposed();

            var succeeded = DynamicHelpers.TryBindAndInvoke(binder, target, args, out result);
            if (!succeeded)
            {
                var exception = result as Exception;
                if ((exception != null) && (engine.CurrentScriptFrame != null))
                {
                    var scriptError = exception as IScriptEngineException;

                    if (scriptError == null)
                    {
                        scriptError = GetScriptError(exception);
                    }

                    if (scriptError.ExecutionStarted)
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
            return ((engine is JScriptEngine) && (args.Length < 1)) ? new object[] { Undefined.Value } : args;
        }

        public override string[] GetPropertyNames()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetProperties(BindingFlags.Default).Select(property => property.Name).ExcludeIndices().ToArray());
        }

        public override int[] GetPropertyIndices()
        {
            VerifyNotDisposed();
            return engine.ScriptInvoke(() => target.GetProperties(BindingFlags.Default).Select(property => property.Name).GetIndices().ToArray());
        }

        #endregion

        #region ScriptObject overrides

        public override object GetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();

            var result = engine.MarshalToHost(engine.ScriptInvoke(() =>
            {
                try
                {
                    return target.InvokeMember(name, BindingFlags.GetProperty, null, target, engine.MarshalToScript(args), null, CultureInfo.InvariantCulture, null);
                }
                catch (Exception)
                {
                    if (target.GetMethod(name, BindingFlags.GetProperty) != null)
                    {
                        // Property retrieval failed, but a method with the given name exists;
                        // create a tear-off method. This currently applies only to VBScript.

                        return new ScriptMethod(this, name);
                    }

                    return Undefined.Value;
                }
            }), false);

            var resultScriptItem = result as WindowsScriptItem;
            if ((resultScriptItem != null) && (resultScriptItem.engine == engine))
            {
                resultScriptItem.holder = this;
            }

            return result;
        }

        public override void SetProperty(string name, params object[] args)
        {
            VerifyNotDisposed();

            engine.ScriptInvoke(() =>
            {
                var marshaledArgs = engine.MarshalToScript(args);
                try
                {
                    try
                    {
                        target.InvokeMember(name, BindingFlags.SetProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                    }
                    catch (COMException primaryException)
                    {
                        // VBScript objects can be finicky about property-put dispatch flags

                        if (primaryException.ErrorCode == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND)
                        {
                            try
                            {
                                target.InvokeMember(name, BindingFlags.SetProperty | BindingFlags.PutDispProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                            }
                            catch (COMException secondaryException)
                            {
                                if (secondaryException.ErrorCode == RawCOMHelpers.HResult.DISP_E_MEMBERNOTFOUND)
                                {
                                    target.InvokeMember(name, BindingFlags.SetProperty | BindingFlags.PutRefDispProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (MissingMemberException)
                {
                    target.AddProperty(name);
                    target.InvokeMember(name, BindingFlags.SetProperty, null, target, marshaledArgs, null, CultureInfo.InvariantCulture, null);
                }
            });
        }

        public override bool DeleteProperty(string name)
        {
            VerifyNotDisposed();

            return engine.ScriptInvoke(() =>
            {
                var field = target.GetField(name, BindingFlags.Default);
                if (field != null)
                {
                    target.RemoveMember(field);
                    return true;
                }

                var property = target.GetProperty(name, BindingFlags.Default);
                if (property != null)
                {
                    target.RemoveMember(property);
                    return true;
                }

                return false;
            });
        }

        public override object GetProperty(int index)
        {
            VerifyNotDisposed();
            return GetProperty(index.ToString(CultureInfo.InvariantCulture), ArrayHelpers.GetEmptyArray<object>());
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

            if (asConstructor)
            {
                return engine.Script.EngineInternal.invokeConstructor(this, args);
            }

            return engine.Script.EngineInternal.invokeMethod(holder, this, args);
        }

        public override object InvokeMethod(string name, params object[] args)
        {
            VerifyNotDisposed();

            try
            {
                return engine.MarshalToHost(engine.ScriptInvoke(() =>
                {
                    // ReSharper disable SuspiciousTypeConversion.Global

                    var dispatchEx = target as IDispatchEx;
                    if (dispatchEx != null)
                    {
                        // Standard IExpando-over-IDispatchEx support appears to repeat failing
                        // invocations. This issue has been reported. In the meantime we'll bypass
                        // this facility and interface with IDispatchEx directly.

                        return dispatchEx.InvokeMethod(name, false, engine.MarshalToScript(args));
                    }

                    // ReSharper restore SuspiciousTypeConversion.Global

                    return target.InvokeMember(name, BindingFlags.InvokeMethod, null, target, engine.MarshalToScript(args), null, CultureInfo.InvariantCulture, null);

                }), false);
            }
            catch (Exception exception)
            {
                IScriptEngineException scriptError;
                if (TryGetScriptError(exception, out scriptError))
                {
                    throw (Exception)scriptError;
                }

                throw;
            }
        }

        #endregion

        #region IScriptMarshalWrapper implementation

        public override ScriptEngine Engine
        {
            get { return engine; }
        }

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

        #region IDisposable implementation

        public void Dispose()
        {
            if (disposedFlag.Set())
            {
                Marshal.ReleaseComObject(target);
            }
        }

        #endregion
    }
}
