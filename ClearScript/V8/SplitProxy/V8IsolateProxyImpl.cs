// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8IsolateProxyImpl : V8IsolateProxy
    {
        private V8EntityHolder holder;

        private V8Isolate.Handle Handle => (V8Isolate.Handle)holder.Handle;

        public V8IsolateProxyImpl(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
        {
            holder = new V8EntityHolder("V8 runtime", () =>
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_Create(
                        name,
                        constraints?.MaxNewSpaceSize ?? -1,
                        constraints?.MaxOldSpaceSize ?? -1,
                        constraints?.HeapExpansionMultiplier ?? 0,
                        constraints?.MaxArrayBufferAllocation ?? ulong.MaxValue,
                        flags,
                        debugPort
                    );
                }
            });
        }

        public V8Context.Handle CreateContext(string name, V8ScriptEngineFlags flags, int debugPort)
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                return instance.V8Isolate_CreateContext(
                    Handle,
                    name,
                    flags,
                    debugPort
                );
            }
        }

        #region V8IsolateProxy overrides

        public override UIntPtr MaxHeapSize
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_GetMaxHeapSize(Handle);
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetMaxHeapSize(Handle, value);
                }
            }
        }

        public override TimeSpan HeapSizeSampleInterval
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return TimeSpan.FromMilliseconds(instance.V8Isolate_GetHeapSizeSampleInterval(Handle));
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetHeapSizeSampleInterval(Handle, value.TotalMilliseconds);
                }
            }
        }

        public override UIntPtr MaxStackUsage
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_GetMaxStackUsage(Handle);
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetMaxStackUsage(Handle, value);
                }
            }
        }

        public override void AwaitDebuggerAndPause()
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_AwaitDebuggerAndPause(Handle);
            }
        }

        public override void CancelAwaitDebugger()
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_CancelAwaitDebugger(Handle);
            }
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code)
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                return new V8ScriptImpl(documentInfo, code.GetDigest(), instance.V8Isolate_Compile(
                    Handle,
                    MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                    documentInfo.UniqueId,
                    documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(documentInfo),
                    code
                ));
            }
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            if (cacheKind == V8CacheKind.None)
            {
                cacheBytes = null;
                return Compile(documentInfo, code);
            }

            using (V8SplitProxyNative.Invoke(out var instance))
            {
                return new V8ScriptImpl(documentInfo, code.GetDigest(), instance.V8Isolate_CompileProducingCache(
                    Handle,
                    MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                    documentInfo.UniqueId,
                    documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(documentInfo),
                    code,
                    cacheKind,
                    out cacheBytes
                ));
            }
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            if ((cacheKind == V8CacheKind.None) || (cacheBytes == null) || (cacheBytes.Length < 1))
            {
                cacheAccepted = false;
                return Compile(documentInfo, code);
            }

            using (V8SplitProxyNative.Invoke(out var instance))
            {
                return new V8ScriptImpl(documentInfo, code.GetDigest(), instance.V8Isolate_CompileConsumingCache(
                    Handle,
                    MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                    documentInfo.UniqueId,
                    documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(documentInfo),
                    code,
                    cacheKind,
                    cacheBytes,
                    out cacheAccepted
                ));
            }
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            if (cacheKind == V8CacheKind.None)
            {
                cacheResult = V8CacheResult.Disabled;
                return Compile(documentInfo, code);
            }

            V8.V8Script script;
            var tempCacheBytes = cacheBytes;

            if ((cacheBytes == null) || (cacheBytes.Length < 1))
            {
                script = Compile(documentInfo, code, cacheKind, out tempCacheBytes);
                cacheResult = (tempCacheBytes != null) && (tempCacheBytes.Length > 0) ? V8CacheResult.Updated : V8CacheResult.UpdateFailed;
                if (cacheResult == V8CacheResult.Updated)
                {
                    cacheBytes = tempCacheBytes;
                }

                return script;
            }

            using (V8SplitProxyNative.Invoke(out var instance))
            {
                script = new V8ScriptImpl(documentInfo, code.GetDigest(), instance.V8Isolate_CompileUpdatingCache(
                    Handle,
                    MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                    documentInfo.UniqueId,
                    documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(documentInfo),
                    code,
                    cacheKind,
                    ref tempCacheBytes,
                    out cacheResult
                ));
            }

            if (cacheResult == V8CacheResult.Updated)
            {
                cacheBytes = tempCacheBytes;
            }

            return script;
        }

        public override bool EnableInterruptPropagation
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_GetEnableInterruptPropagation(Handle);
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetEnableInterruptPropagation(Handle, value);
                }
            }
        }

        public override bool DisableHeapSizeViolationInterrupt
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_GetDisableHeapSizeViolationInterrupt(Handle);
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetDisableHeapSizeViolationInterrupt(Handle, value);
                }
            }
        }

        public override V8RuntimeHeapInfo GetHeapInfo()
        {
            var totalHeapSize = 0UL;
            var totalHeapSizeExecutable = 0UL;
            var totalPhysicalSize = 0UL;
            var totalAvailableSize = 0UL;
            var usedHeapSize = 0UL;
            var heapSizeLimit = 0UL;
            var totalExternalSize = 0UL;

            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_GetHeapStatistics(Handle, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out totalAvailableSize, out usedHeapSize, out heapSizeLimit, out totalExternalSize);
            }

            return new V8RuntimeHeapInfo
            {
                TotalHeapSize = totalHeapSize,
                TotalHeapSizeExecutable = totalHeapSizeExecutable,
                TotalPhysicalSize = totalPhysicalSize,
                TotalAvailableSize = totalAvailableSize,
                UsedHeapSize = usedHeapSize,
                HeapSizeLimit = heapSizeLimit,
                TotalExternalSize = totalExternalSize
            };
        }

        public override V8Runtime.Statistics GetStatistics()
        {
            var statistics = new V8Runtime.Statistics();

            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_GetStatistics(Handle, out statistics.ScriptCount, out statistics.ScriptCacheSize, out statistics.ModuleCount, out statistics.PostedTaskCounts, out statistics.InvokedTaskCounts);
            }

            return statistics;
        }

        public override void CollectGarbage(bool exhaustive)
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_CollectGarbage(Handle, exhaustive);
            }
        }

        public override bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                return instance.V8Isolate_BeginCpuProfile(Handle, name, flags.HasFlag(V8CpuProfileFlags.EnableSampleCollection));
            }
        }

        public override V8.V8CpuProfile EndCpuProfile(string name)
        {
            var profile = new V8.V8CpuProfile();

            Action<V8CpuProfile.Ptr> action = pProfile => V8CpuProfile.ProcessProfile(Handle, pProfile, profile);
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_EndCpuProfile(Handle, name, actionScope.Value);
                }
            }

            return profile;
        }

        public override void CollectCpuProfileSample()
        {
            using (V8SplitProxyNative.Invoke(out var instance))
            {
                instance.V8Isolate_CollectCpuProfileSample(Handle);
            }
        }

        public override uint CpuProfileSampleInterval
        {
            get
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    return instance.V8Isolate_GetCpuProfileSampleInterval(Handle);
                }
            }

            set
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_SetCpuProfileSampleInterval(Handle, value);
                }
            }
        }

        public override void WriteHeapSnapshot(Stream stream)
        {
            using (var streamScope = V8ProxyHelpers.CreateAddRefHostObjectScope(stream))
            {
                using (V8SplitProxyNative.Invoke(out var instance))
                {
                    instance.V8Isolate_WriteHeapSnapshot(Handle, streamScope.Value);
                }
            }
        }

        #endregion

        #region disposal / finalization

        public override void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~V8IsolateProxyImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
