// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal interface IDynamic
    {
        object GetProperty(string name, params object[] args);
        object GetProperty(string name, out bool isCacheable, params object[] args);
        void SetProperty(string name, params object[] args);
        bool DeleteProperty(string name);
        string[] GetPropertyNames();

        object GetProperty(int index);
        void SetProperty(int index, object value);
        bool DeleteProperty(int index);
        int[] GetPropertyIndices();

        object Invoke(bool asConstructor, params object[] args);
        object InvokeMethod(string name, params object[] args);
    }
}
