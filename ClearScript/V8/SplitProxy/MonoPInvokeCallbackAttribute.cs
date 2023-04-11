using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// MonoPInvokeCallbackAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class MonoPInvokeCallbackAttribute : Attribute
    {
        /// <summary>
        /// MonoPInvokeCallbackAttribute
        /// </summary>
        public MonoPInvokeCallbackAttribute()
        {
        }
    }
}