// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.ClearScript.JavaScript
{
    internal interface IJavaScriptEngine
    {
        uint BaseLanguageVersion { get; }

        object CreatePromiseForTask<T>(Task<T> task);
        object CreatePromiseForTask(Task task);

        Task<object> CreateTaskForPromise(ScriptObject promise);
    }
}
