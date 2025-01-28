using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    /// <summary>
    /// Wraps a JavaScript function.
    /// </summary>
    public sealed class V8Function : IDisposable
    {
        private V8EntityHolder holder;

        /// <summary>
        /// Evaluate a JavaScript expression and cast it to a function.
        /// </summary>
        /// <param name="code">A JavaScript expression that must evaluate to a function object.</param>
        /// <remarks>
        /// Currently, V8Function can only be created from within a callback from JavaScript.
        /// </remarks>
        /// <exception cref="InvalidCastException">If the JavaScript expression sis not evaluate to a
        /// function object.</exception>
        /// <exception cref="NotSupportedException">If the constructor is called not from within a
        /// callback from JavaScript.</exception>
        public V8Function(string code)
        {
            if (ScriptEngine.Current == null)
                throw new NotSupportedException(
                    "Currently, V8Function can only be created from within a callback from JavaScript.");

            holder = new V8EntityHolder(nameof(V8Function), () => V8SplitProxyNative.Invoke(instance =>
            {
                var engine = (V8ScriptEngine)ScriptEngine.Current;
                var contextProxy = (V8ContextProxyImpl)engine.ContextProxy;
                using var resourceName = new StdString(null);
                using var sourceMapUrl = new StdString(null);
                using var stdCode = new StdString(code);
                using var result = V8Value.New();

                instance.V8Context_ExecuteCode(contextProxy.Handle, resourceName.ptr, sourceMapUrl.ptr,
                    (ulong)GetHashCode(), DocumentKind.Script, IntPtr.Zero, stdCode.ptr, true,
                    result.ptr);

                var decoded = result.Decode();

                if (decoded.Subtype != V8Value.Subtype.Function)
                {
                    if (decoded.Type == V8Value.Type.V8Object)
                        instance.V8Entity_DestroyHandle((V8Entity.Handle)decoded.PtrOrHandle);

                    throw new InvalidCastException(
                        "The JavaScript expression did not evaluate to a function object");
                }

                return (V8Object.Handle)decoded.PtrOrHandle;
            }));
        }

        /// <summary>
        /// Release the wrapped JavaScript function, so it can be destroyed by the V8 garbace collector.
        /// </summary>
        public void Dispose()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        /// <summary>
        /// Invoke the wrapped JavaScript function.
        /// </summary>
        /// <param name="args">The list of arguments to pass to the function.</param>
        /// <param name="result">The function will write its return value, if any, here.</param>
        public void Invoke(StdV8ValueArray args, V8Value result)
        {
            StdV8ValueArray.Ptr argsPtr = args.ptr;
            V8Value.Ptr resultPtr = result.ptr;

            V8SplitProxyNative.Invoke(instance =>
                instance.V8Object_Invoke((V8Object.Handle)holder.Handle, false, argsPtr, resultPtr));
        }
    }
}
