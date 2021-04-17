// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.ClearScript.JavaScript
{
    internal partial interface IJavaScriptEngine
    {
        object CreatePromiseForValueTask<T>(ValueTask<T> valueTask);
        object CreatePromiseForValueTask(ValueTask valueTask);
    }
}
