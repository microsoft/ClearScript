// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;

namespace Microsoft.ClearScript.JavaScript
{
    // ReSharper disable once PartialTypeWithSinglePart
    internal partial interface IJavaScriptEngine : IScriptEngine
    {
        uint BaseLanguageVersion { get; }

        CommonJSManager CommonJSManager { get; }
        JsonModuleManager JsonModuleManager { get; }

        object CreatePromiseForTask<T>(Task<T> task);
        object CreatePromiseForTask(Task task);

        object CreatePromiseForValueTask<T>(ValueTask<T> valueTask);
        object CreatePromiseForValueTask(ValueTask valueTask);

        Task<object> CreateTaskForPromise(ScriptObject promise);
    }
}
