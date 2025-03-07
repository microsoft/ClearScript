// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8ContextProxy : V8Proxy
    {
        public static V8ContextProxy Create(V8IsolateProxy isolateProxy, string name, V8ScriptEngineFlags flags, int debugPort)
        {
            return new V8ContextProxyImpl(isolateProxy, name, flags, debugPort);
        }

        public abstract UIntPtr MaxIsolateHeapSize { get; set; }

        public abstract TimeSpan IsolateHeapSizeSampleInterval { get; set; }

        public abstract UIntPtr MaxIsolateStackUsage { get; set; }

        public abstract void InvokeWithLock(Action action);

        public abstract void InvokeWithLock<TArg>(Action<TArg> action, in TArg arg);

        public abstract object GetRootItem();

        public abstract void AddGlobalItem(string name, object item, bool globalMembers);

        public abstract void AwaitDebuggerAndPause();

        public abstract void CancelAwaitDebugger();

        public abstract object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult);

        public abstract object Execute(V8Script script, bool evaluate);

        public abstract void Interrupt();

        public abstract void CancelInterrupt();

        public abstract bool EnableIsolateInterruptPropagation { get; set; }

        public abstract bool DisableIsolateHeapSizeViolationInterrupt { get; set; }

        public abstract V8RuntimeHeapInfo GetIsolateHeapInfo();

        public abstract V8Runtime.Statistics GetIsolateStatistics();

        public abstract V8ScriptEngine.Statistics GetStatistics();

        public abstract void CollectGarbage(bool exhaustive);

        public abstract void OnAccessSettingsChanged();

        public abstract bool BeginCpuProfile(string name, V8CpuProfileFlags flags);

        public abstract V8CpuProfile EndCpuProfile(string name);

        public abstract void CollectCpuProfileSample();

        public abstract uint CpuProfileSampleInterval { get; set; }

        public abstract void WriteIsolateHeapSnapshot(Stream stream);
    }
}
