// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8ContextProxy : V8Proxy
    {
        public static V8ContextProxy Create(V8IsolateProxy isolateProxy, string name, bool enableDebugging, bool disableGlobalMembers, int debugPort)
        {
            return CreateImpl<V8ContextProxy>(isolateProxy, name, enableDebugging, disableGlobalMembers, debugPort);
        }

        public abstract UIntPtr MaxRuntimeHeapSize { get; set; }

        public abstract TimeSpan RuntimeHeapSizeSampleInterval { get; set; }

        public abstract UIntPtr MaxRuntimeStackUsage { get; set; }

        public abstract void InvokeWithLock(Action action);

        public abstract object GetRootItem();

        public abstract void AddGlobalItem(string name, object item, bool globalMembers);

        public abstract object Execute(string documentName, string code, bool evaluate, bool discard);

        public abstract V8Script Compile(string documentName, string code);

        public abstract V8Script Compile(string documentName, string code, V8CacheKind cacheKind, out byte[] cacheBytes);

        public abstract V8Script Compile(string documentName, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);

        public abstract object Execute(V8Script script, bool evaluate);

        public abstract void Interrupt();

        public abstract V8RuntimeHeapInfo GetRuntimeHeapInfo();

        public abstract void CollectGarbage(bool exhaustive);

        public abstract void OnAccessSettingsChanged();
    }
}
