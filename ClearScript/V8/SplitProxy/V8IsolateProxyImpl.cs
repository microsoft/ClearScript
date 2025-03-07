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
            holder = new V8EntityHolder("V8 runtime", () => V8SplitProxyNative.Invoke(
                static (instance, ctx) => instance.V8Isolate_Create(
                    ctx.name,
                    ctx.constraints?.MaxNewSpaceSize ?? -1,
                    ctx.constraints?.MaxOldSpaceSize ?? -1,
                    ctx.constraints?.HeapExpansionMultiplier ?? 0,
                    ctx.constraints?.MaxArrayBufferAllocation ?? ulong.MaxValue,
                    ctx.flags,
                    ctx.debugPort
                ),
                (name, constraints, flags, debugPort)
            ));
        }

        public V8Context.Handle CreateContext(string name, V8ScriptEngineFlags flags, int debugPort)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_CreateContext(ctx.Handle, ctx.name, ctx.flags, ctx.debugPort), (Handle, name, flags, debugPort));
        }

        #region V8IsolateProxy overrides

        public override UIntPtr MaxHeapSize
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_GetMaxHeapSize(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetMaxHeapSize(ctx.Handle, ctx.value), (Handle, value));
        }

        public override TimeSpan HeapSizeSampleInterval
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => TimeSpan.FromMilliseconds(instance.V8Isolate_GetHeapSizeSampleInterval(handle)), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetHeapSizeSampleInterval(ctx.Handle, ctx.value.TotalMilliseconds), (Handle, value));
        }

        public override UIntPtr MaxStackUsage
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_GetMaxStackUsage(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetMaxStackUsage(ctx.Handle, ctx.value), (Handle, value));
        }

        public override void AwaitDebuggerAndPause()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_AwaitDebuggerAndPause(handle), Handle);
        }

        public override void CancelAwaitDebugger()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_CancelAwaitDebugger(handle), Handle);
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code)
        {
            return new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(
                static (instance, ctx) => instance.V8Isolate_Compile(
                    ctx.Handle,
                    MiscHelpers.GetUrlOrPath(ctx.documentInfo.Uri, ctx.documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(ctx.documentInfo.SourceMapUri, string.Empty),
                    ctx.documentInfo.UniqueId,
                    ctx.documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(ctx.documentInfo),
                    ctx.code
                ),
                (Handle, documentInfo, code)
            ));
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            if (cacheKind == V8CacheKind.None)
            {
                cacheBytes = null;
                return Compile(documentInfo, code);
            }

            var ctx = (Handle, documentInfo, code, cacheKind, cacheBytes: (byte[])null);

            var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(
                static (instance, pCtx) =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    return instance.V8Isolate_CompileProducingCache(
                        ctx.Handle,
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.Uri, ctx.documentInfo.UniqueName),
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.SourceMapUri, string.Empty),
                        ctx.documentInfo.UniqueId,
                        ctx.documentInfo.Category.Kind,
                        V8ProxyHelpers.AddRefHostObject(ctx.documentInfo),
                        ctx.code,
                        ctx.cacheKind,
                        out ctx.cacheBytes
                    );
                },
                StructPtr.FromRef(ref ctx)
            ));

            cacheBytes = ctx.cacheBytes;
            return script;
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            if ((cacheKind == V8CacheKind.None) || (cacheBytes is null) || (cacheBytes.Length < 1))
            {
                cacheAccepted = false;
                return Compile(documentInfo, code);
            }

            var ctx = (Handle, documentInfo, code, cacheKind, cacheBytes, cacheAccepted: false);

            var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(
                static (instance, pCtx) =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    return instance.V8Isolate_CompileConsumingCache(
                        ctx.Handle,
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.Uri, ctx.documentInfo.UniqueName),
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.SourceMapUri, string.Empty),
                        ctx.documentInfo.UniqueId,
                        ctx.documentInfo.Category.Kind,
                        V8ProxyHelpers.AddRefHostObject(ctx.documentInfo),
                        ctx.code,
                        ctx.cacheKind,
                        ctx.cacheBytes,
                        out ctx.cacheAccepted
                    );
                },
                StructPtr.FromRef(ref ctx)
            ));

            cacheAccepted = ctx.cacheAccepted;
            return script;
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult)
        {
            if (cacheKind == V8CacheKind.None)
            {
                cacheResult = V8CacheResult.Disabled;
                return Compile(documentInfo, code);
            }

            V8.V8Script script;

            if ((cacheBytes is null) || (cacheBytes.Length < 1))
            {
                script = Compile(documentInfo, code, cacheKind, out var tempCacheBytes);
                cacheResult = (tempCacheBytes is not null) && (tempCacheBytes.Length > 0) ? V8CacheResult.Updated : V8CacheResult.UpdateFailed;
                if (cacheResult == V8CacheResult.Updated)
                {
                    cacheBytes = tempCacheBytes;
                }

                return script;
            }

            var ctx = (Handle, documentInfo, code, cacheKind, cacheBytes, cacheResult: V8CacheResult.Disabled);

            script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(
                static (instance, pCtx) =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    return instance.V8Isolate_CompileUpdatingCache(
                        ctx.Handle,
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.Uri, ctx.documentInfo.UniqueName),
                        MiscHelpers.GetUrlOrPath(ctx.documentInfo.SourceMapUri, string.Empty),
                        ctx.documentInfo.UniqueId,
                        ctx.documentInfo.Category.Kind,
                        V8ProxyHelpers.AddRefHostObject(ctx.documentInfo),
                        ctx.code,
                        ctx.cacheKind,
                        ref ctx.cacheBytes,
                        out ctx.cacheResult
                    );
                },
                StructPtr.FromRef(ref ctx)
            ));

            cacheResult = ctx.cacheResult;
            if (cacheResult == V8CacheResult.Updated)
            {
                cacheBytes = ctx.cacheBytes;
            }

            return script;
        }

        public override bool EnableInterruptPropagation
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_GetEnableInterruptPropagation(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetEnableInterruptPropagation(ctx.Handle, ctx.value), (Handle, value));
        }

        public override bool DisableHeapSizeViolationInterrupt
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_GetDisableHeapSizeViolationInterrupt(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetDisableHeapSizeViolationInterrupt(ctx.Handle, ctx.value), (Handle, value));
        }

        public override V8RuntimeHeapInfo GetHeapInfo()
        {
            var ctx = (
                Handle,
                totalHeapSize: 0UL,
                totalHeapSizeExecutable: 0UL,
                totalPhysicalSize: 0UL,
                totalAvailableSize: 0UL,
                usedHeapSize: 0UL,
                heapSizeLimit: 0UL,
                totalExternalSize: 0UL
            );

            V8SplitProxyNative.Invoke(
                static (instance, pCtx) =>
                {
                    ref var ctx = ref pCtx.AsRef();
                    instance.V8Isolate_GetHeapStatistics(
                        ctx.Handle,
                        out ctx.totalHeapSize,
                        out ctx.totalHeapSizeExecutable,
                        out ctx.totalPhysicalSize,
                        out ctx.totalAvailableSize,
                        out ctx.usedHeapSize,
                        out ctx.heapSizeLimit,
                        out ctx.totalExternalSize
                    );
                },
                StructPtr.FromRef(ref ctx)
            );

            return new V8RuntimeHeapInfo
            {
                TotalHeapSize = ctx.totalHeapSize,
                TotalHeapSizeExecutable = ctx.totalHeapSizeExecutable,
                TotalPhysicalSize = ctx.totalPhysicalSize,
                TotalAvailableSize = ctx.totalAvailableSize,
                UsedHeapSize = ctx.usedHeapSize,
                HeapSizeLimit = ctx.heapSizeLimit,
                TotalExternalSize = ctx.totalExternalSize
            };
        }

        public override V8Runtime.Statistics GetStatistics()
        {
            var statistics = new V8Runtime.Statistics();
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_GetStatistics(ctx.Handle, out ctx.statistics.ScriptCount, out ctx.statistics.ScriptCacheSize, out ctx.statistics.ModuleCount, out ctx.statistics.PostedTaskCounts, out ctx.statistics.InvokedTaskCounts), (Handle, statistics));
            return statistics;
        }

        public override void CollectGarbage(bool exhaustive)
        {
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_CollectGarbage(ctx.Handle, ctx.exhaustive), (Handle, exhaustive));
        }

        public override bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_BeginCpuProfile(ctx.Handle, ctx.name, ctx.flags.HasAllFlags(V8CpuProfileFlags.EnableSampleCollection)), (Handle, name, flags));
        }

        public override V8.V8CpuProfile EndCpuProfile(string name)
        {
            var profile = new V8.V8CpuProfile();

            Action<V8CpuProfile.Ptr> action = pProfile => V8CpuProfile.ProcessProfile(Handle, pProfile, profile);
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_EndCpuProfile(ctx.Handle, ctx.name, ctx.pAction), (Handle, name, pAction));
            }

            return profile;
        }

        public override void CollectCpuProfileSample()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_CollectCpuProfileSample(handle), Handle);
        }

        public override uint CpuProfileSampleInterval
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Isolate_GetCpuProfileSampleInterval(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_SetCpuProfileSampleInterval(ctx.Handle, ctx.value), (Handle, value));
        }

        public override void WriteHeapSnapshot(Stream stream)
        {
            using (var streamScope = V8ProxyHelpers.CreateAddRefHostObjectScope(stream))
            {
                var pStream = streamScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Isolate_WriteHeapSnapshot(ctx.Handle, ctx.pStream), (Handle, pStream));
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
