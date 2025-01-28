// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    internal interface IV8DebugListener : IDisposable
    {
        void ConnectClient();
        void SendCommand(string command);
        void DisconnectClient();
    }
}
