// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    internal interface IV8Object : IDisposable
    {
        object GetProperty(string name);
        void SetProperty(string name, object value);
        bool DeleteProperty(string name);
        string[] GetPropertyNames();

        object GetProperty(int index);
        void SetProperty(int index, object value);
        bool DeleteProperty(int index);
        int[] GetPropertyIndices();

        object Invoke(bool asConstructor, object[] args);
        object InvokeMethod(string name, object[] args);

        bool IsArrayBufferOrView();
        V8ArrayBufferOrViewKind GetArrayBufferOrViewKind();
        V8ArrayBufferOrViewInfo GetArrayBufferOrViewInfo();
        void InvokeWithArrayBufferOrViewData(Action<IntPtr> action);
    }
}
