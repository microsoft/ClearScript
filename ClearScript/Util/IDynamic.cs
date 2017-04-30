// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal interface IDynamic
    {
        object GetProperty(string name, object[] args);
        object GetProperty(string name, object[] args, out bool isCacheable);
        void SetProperty(string name, object[] args);
        bool DeleteProperty(string name);
        string[] GetPropertyNames();

        object GetProperty(int index);
        void SetProperty(int index, object value);
        bool DeleteProperty(int index);
        int[] GetPropertyIndices();

        object Invoke(object[] args, bool asConstructor);
        object InvokeMethod(string name, object[] args);
    }
}
