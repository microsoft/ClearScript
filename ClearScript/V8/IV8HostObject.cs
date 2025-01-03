using System;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8.Unsafe
{
    /// <summary>
    /// 
    /// </summary>
    public interface IV8HostObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="isConst"></param>
        void GetNamedProperty(StdString name, V8Value value, out bool isConst);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetNamedProperty(StdString name, V8Value.Decoded value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool DeleteNamedProperty(StdString name) => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pValue"></param>
        void GetIndexedProperty(int index, V8Value pValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        void SetIndexedProperty(int index, V8Value.Decoded value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        bool DeleteIndexedProperty(int index) => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void GetEnumerator(V8Value result) => result.SetNonexistent();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void GetAsyncEnumerator(V8Value result) => result.SetNonexistent();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="names"></param>
        void GetNamedPropertyNames(StdStringArray names) => names.SetElementCount(0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices"></param>
        void GetIndexedPropertyIndices(StdInt32Array indices) => indices.SetElementCount(0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        void InvokeMethod(StdString name, ReadOnlySpan<V8Value.Decoded> args, V8Value result)
        {
            GetNamedProperty(name, result, out _);
            object method = result.GetHostObject();
            ((InvokeHostObject)method)(args, result);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="result"></param>
    public delegate void InvokeHostObject(ReadOnlySpan<V8Value.Decoded> args, V8Value result);
}

namespace Demo
{
    using Microsoft.ClearScript.V8.Unsafe;

    public sealed class Bomb : IV8HostObject
    {
        private Action<string> log;
        private InvokeHostObject explode;

        public Bomb(Action<string> log)
        {
            this.log = log;
            explode = Explode;
        }

        private void Explode(ReadOnlySpan<V8Value.Decoded> args, V8Value result)
        {
            log("Boom!");
            result.SetNonexistent();
        }

        public void GetNamedProperty(StdString name, V8Value value, out bool isConst)
        {
            if (name.Equals(nameof(explode)))
            {
                value.SetHostObject(explode);
                isConst = true;
            }
            else
            {
                value.SetNonexistent();
                isConst = true;
            }
        }

        public void GetIndexedProperty(int index, V8Value value) => value.SetNonexistent();

        public void SetNamedProperty(StdString name, V8Value.Decoded value) { }

        public void SetIndexedProperty(int index, V8Value.Decoded value) { }
    }
}