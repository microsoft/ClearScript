using System;
using System.Runtime.InteropServices;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    /// <summary>
    /// Wraps a Uint8Array, an array of <see cref="byte"/>.
    /// </summary>
    public readonly ref struct Uint8Array
    {
        private readonly V8Object.Handle ptr;

        internal Uint8Array(V8Object.Handle pArray)
        {
            ptr = pArray;
            using var arrayBuffer = V8Value.New();

            V8SplitProxyNative.Instance.V8Object_GetArrayBufferOrViewInfo(pArray, arrayBuffer.ptr,
                out _, out _, out ulong length);

            Length = (int)length;
        }

        /// <summary>
        /// Copy the contents of the wrapped Uint8Array to a managed <see cref="byte"/> array.
        /// </summary>
        /// <param name="array">The destination array. It must be large enough to contain the entire
        /// contents of the wrapped Uint8Array.</param>
        public void CopyTo(byte[] array)
        {
            int length = Length;

            if (length > array.Length)
                throw new IndexOutOfRangeException(
                    $"Tried to copy {length} items to a {array.Length} item array");

            // TODO: Don't allocate a lambda every time.
            IntPtr pAction = V8ProxyHelpers.AddRefHostObject(new Action<IntPtr>(data =>
                Marshal.Copy(data, array, 0, length)));

            try
            {
                V8SplitProxyNative.Instance.V8Object_InvokeWithArrayBufferOrViewData(ptr, pAction);
            }
            finally
            {
                V8ProxyHelpers.ReleaseHostObject(pAction);
            }
        }

        /// <summary>
        /// The length of the wrapped Uint8Array.
        /// </summary>
        public int Length { get; }
    }
}
