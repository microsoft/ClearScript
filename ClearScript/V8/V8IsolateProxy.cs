// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.ClearScript.V8.SplitProxy;

namespace Microsoft.ClearScript.V8
{
    internal abstract class V8IsolateProxy : V8Proxy
    {
        public static V8IsolateProxy Create(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
        {
            return new V8IsolateProxyImpl(name, constraints, flags, debugPort);
        }

        public abstract UIntPtr MaxHeapSize { get; set; }

        public abstract TimeSpan HeapSizeSampleInterval { get; set; }

        public abstract UIntPtr MaxStackUsage { get; set; }

        public abstract void AwaitDebuggerAndPause();

        public abstract void CancelAwaitDebugger();

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);

        public abstract V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult);

        public abstract bool EnableInterruptPropagation { get; set; }

        public abstract bool DisableHeapSizeViolationInterrupt { get; set; }

        public abstract V8RuntimeHeapInfo GetHeapInfo();

        public abstract V8Runtime.Statistics GetStatistics();

        public abstract void CollectGarbage(bool exhaustive);

        public abstract bool BeginCpuProfile(string name, V8CpuProfileFlags flags);

        public abstract V8CpuProfile EndCpuProfile(string name);

        public abstract void CollectCpuProfileSample();

        public abstract uint CpuProfileSampleInterval { get; set; }

        public abstract void WriteHeapSnapshot(Stream stream);
    }
}
