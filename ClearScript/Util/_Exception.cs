// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using System.Runtime.Serialization;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

namespace System.Runtime.InteropServices
{
    internal interface _Exception
    {
        string ToString();
        bool Equals(object obj);
        int GetHashCode();
        Type GetType();
        string Message { get; }
        Exception GetBaseException();
        string StackTrace { get; }
        string HelpLink { get; set; }
        string Source { get; set; }
        void GetObjectData(SerializationInfo info, StreamingContext context);
        Exception InnerException { get; }
        MethodBase TargetSite { get; }
    }
}

// ReSharper restore InconsistentNaming
// ReSharper restore CheckNamespace
