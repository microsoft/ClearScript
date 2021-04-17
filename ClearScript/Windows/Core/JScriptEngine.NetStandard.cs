// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.ClearScript.JavaScript;

namespace Microsoft.ClearScript.Windows.Core
{
    public partial class JScriptEngine
    {
        #region IJavaScriptEngine implementation

        object IJavaScriptEngine.CreatePromiseForValueTask<T>(ValueTask<T> valueTask)
        {
            throw new NotImplementedException();
        }

        object IJavaScriptEngine.CreatePromiseForValueTask(ValueTask valueTask)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
