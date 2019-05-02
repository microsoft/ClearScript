// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8ContextProxy : V8Proxy
    {
        public static V8ContextProxy Create(V8IsolateProxy isolateProxy, string name, V8ScriptEngineFlags flags, int debugPort)
        {
            return CreateImpl<V8ContextProxy>(isolateProxy, name, flags, debugPort);
        }

        public abstract UIntPtr MaxRuntimeHeapSize { get; set; }

        public abstract TimeSpan RuntimeHeapSizeSampleInterval { get; set; }

        public abstract UIntPtr MaxRuntimeStackUsage { get; set; }

        public abstract void InvokeWithLock(Action action);

        public abstract object GetRootItem();

        public abstract void AddGlobalItem(string name, object item, bool globalMembers);

        public abstract void AwaitDebuggerAndPause();

        public abstract object Execute(DocumentInfo documentInfo, string code, bool evaluate);

        public abstract V8Script Compile(DocumentInfo documentInfo, string code);

        public abstract V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes);

        public abstract V8Script Compile(DocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);

        public abstract object Execute(V8Script script, bool evaluate);

        public abstract void Interrupt();

        public abstract V8RuntimeHeapInfo GetRuntimeHeapInfo();

        public abstract void CollectGarbage(bool exhaustive);

        public abstract void OnAccessSettingsChanged();

        public abstract bool BeginCpuProfile(string name, V8CpuProfileFlags flags);

        public abstract V8CpuProfile EndCpuProfile(string name);

        public abstract void CollectCpuProfileSample();

        public abstract uint CpuProfileSampleInterval { get; set; }
    }
}
