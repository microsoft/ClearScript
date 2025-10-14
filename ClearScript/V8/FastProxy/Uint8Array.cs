using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    /// <summary>
    /// Wraps a Uint8Array, an array of <see cref="byte"/>.
    /// </summary>
    public readonly ref struct Uint8Array
    {
        private readonly V8Object.Handle handle;

        internal Uint8Array(V8Object.Handle pArray)
        {
            handle = pArray;

            Length = (int)V8SplitProxyNative.InvokeRaw(static (instance, hObject) =>
            {
                V8Value.Ptr pArrayBuffer = instance.V8Value_New();
                instance.V8Object_GetArrayBufferOrViewInfo(hObject, pArrayBuffer, out _, out _, out ulong length);
                instance.V8Value_Delete(pArrayBuffer);
                return length;
            }, pArray);
        }

        /// <summary>
        /// Copy the contents of the wrapped Uint8Array to a managed <see cref="byte"/> array.
        /// </summary>
        /// <param name="array">The destination array. It must be large enough to contain the entire
        /// contents of the wrapped Uint8Array.</param>
        public unsafe void CopyTo(byte[] array)
        {
            if (Length > array.Length)
                throw new IndexOutOfRangeException(
                    $"Tried to copy {Length} items to a {array.Length} item array");

            Action<IntPtr, (byte[] array, int length)> action = static (data, arg) =>
                Marshal.Copy(data, arg.array, 0, arg.length);

            var ctx = (action, (array, Length));
            var pCtx = (IntPtr)Unsafe.AsPointer(ref ctx);

            Action<IntPtr, IntPtr> actionWithArg = static (pData, pCtx) =>
            {
                ref var ctx = ref Unsafe.AsRef<(Action<IntPtr, (byte[], int)> action, (byte[], int) arg)>(pCtx.ToPointer());
                ctx.action(pData, ctx.arg);
            };

            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(actionWithArg))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.InvokeRaw(static (instance, ctx) => instance.V8Object_InvokeWithArrayBufferOrViewDataWithArg(ctx.handle, ctx.pAction, ctx.pCtx), (handle, pAction, pCtx));
            }
        }

        /// <summary>
        /// The length of the wrapped Uint8Array.
        /// </summary>
        public int Length { get; }
    }
}
