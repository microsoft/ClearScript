// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    /// <summary>
    /// Provides data for debugger events associated with V8 runtimes.
    /// </summary>
    public sealed class V8RuntimeDebuggerEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name associated with the V8 runtime instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the TCP port of the debugger connection.
        /// </summary>
        public int Port { get; }

        internal V8RuntimeDebuggerEventArgs(string name, int port)
        {
            Name = name;
            Port = port;
        }
    }
}
