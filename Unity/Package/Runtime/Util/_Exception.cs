// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
    // ReSharper disable once InconsistentNaming
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
