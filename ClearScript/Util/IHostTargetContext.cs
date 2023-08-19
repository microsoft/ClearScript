// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Microsoft.ClearScript.Util
{
    internal interface IHostTargetContext : IHostContext
    {
        HostTargetFlags TargetFlags { get; }
    }
}
