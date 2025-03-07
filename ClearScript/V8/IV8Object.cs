// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript.V8
{
    internal interface IV8Object : IDisposable
    {
        JavaScriptObjectKind ObjectKind { get; }
        JavaScriptObjectFlags ObjectFlags { get; }

        int IdentityHash { get; }

        object GetProperty(string name);
        bool TryGetProperty(string name, out object value);
        void SetProperty(string name, object value);
        bool DeleteProperty(string name);
        string[] GetPropertyNames(bool includeIndices);

        object GetProperty(int index);
        void SetProperty(int index, object value);
        bool DeleteProperty(int index);
        int[] GetPropertyIndices();

        object Invoke(bool asConstructor, object[] args);
        object InvokeMethod(string name, object[] args);

        bool IsPromise { get; }
        bool IsArray { get; }
        bool IsShared { get; }

        bool IsArrayBufferOrView { get; }
        V8ArrayBufferOrViewKind ArrayBufferOrViewKind { get; }
        V8ArrayBufferOrViewInfo GetArrayBufferOrViewInfo();
        void InvokeWithArrayBufferOrViewData(Action<IntPtr> action);
        void InvokeWithArrayBufferOrViewData<TArg>(Action<IntPtr, TArg> action, in TArg arg);
    }
}
