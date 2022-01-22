// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8IsolateProxyImpl : V8IsolateProxy
    {
        private V8EntityHolder holder;

        private V8Isolate.Handle Handle => (V8Isolate.Handle)holder.Handle;

        public V8IsolateProxyImpl(string name, V8RuntimeConstraints constraints, V8RuntimeFlags flags, int debugPort)
        {
            holder = new V8EntityHolder("V8 runtime", () => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_Create(
                name,
                constraints?.MaxNewSpaceSize ?? -1,
                constraints?.MaxOldSpaceSize ?? -1,
                constraints?.HeapExpansionMultiplier ?? 0,
                constraints?.MaxArrayBufferAllocation ?? ulong.MaxValue,
                flags.HasFlag(V8RuntimeFlags.EnableDebugging),
                flags.HasFlag(V8RuntimeFlags.EnableRemoteDebugging),
                flags.HasFlag(V8RuntimeFlags.EnableDynamicModuleImports),
                debugPort
            )));
        }

        public V8Context.Handle CreateContext(string name, V8ScriptEngineFlags flags, int debugPort)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CreateContext(
                Handle,
                name,
                flags.HasFlag(V8ScriptEngineFlags.EnableDebugging),
                flags.HasFlag(V8ScriptEngineFlags.EnableRemoteDebugging),
                flags.HasFlag(V8ScriptEngineFlags.DisableGlobalMembers),
                flags.HasFlag(V8ScriptEngineFlags.EnableDateTimeConversion),
                flags.HasFlag(V8ScriptEngineFlags.EnableDynamicModuleImports),
                debugPort
            ));
        }

        #region V8IsolateProxy overrides

        public override UIntPtr MaxHeapSize
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetMaxHeapSize(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_SetMaxHeapSize(Handle, value));
        }

        public override TimeSpan HeapSizeSampleInterval
        {
            get => V8SplitProxyNative.Invoke(instance => TimeSpan.FromMilliseconds(instance.V8Isolate_GetHeapSizeSampleInterval(Handle)));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_SetHeapSizeSampleInterval(Handle, value.TotalMilliseconds));
        }

        public override UIntPtr MaxStackUsage
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetMaxStackUsage(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_SetMaxStackUsage(Handle, value));
        }

        public override void AwaitDebuggerAndPause()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_AwaitDebuggerAndPause(Handle));
        }

        public override void CancelAwaitDebugger()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CancelAwaitDebugger(Handle));
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code)
        {
            return new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Isolate_Compile(
                Handle,
                MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                documentInfo.UniqueId,
                documentInfo.Category == ModuleCategory.Standard,
                V8ProxyHelpers.AddRefHostObject(documentInfo),
                code
            )));
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes)
        {
            if (cacheKind == V8CacheKind.None)
            {
                cacheBytes = null;
                return Compile(documentInfo, code);
            }

            byte[] tempCacheBytes = null;
            var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CompileProducingCache(
                Handle,
                MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                documentInfo.UniqueId,
                documentInfo.Category == ModuleCategory.Standard,
                V8ProxyHelpers.AddRefHostObject(documentInfo),
                code,
                cacheKind,
                out tempCacheBytes
            )));

            cacheBytes = tempCacheBytes;
            return script;
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted)
        {
            if ((cacheKind == V8CacheKind.None) || (cacheBytes == null) || (cacheBytes.Length < 1))
            {
                cacheAccepted = false;
                return Compile(documentInfo, code);
            }

            var cacheSize = cacheBytes.Length;
            using (var cacheBlock = new CoTaskMemBlock(cacheSize))
            {
                Marshal.Copy(cacheBytes, 0, cacheBlock.Addr, cacheSize);

                var tempCacheAccepted = false;
                var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CompileConsumingCache(
                    Handle,
                    MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                    documentInfo.UniqueId,
                    documentInfo.Category == ModuleCategory.Standard,
                    V8ProxyHelpers.AddRefHostObject(documentInfo),
                    code,
                    cacheKind,
                    cacheBytes,
                    out tempCacheAccepted
                )));

                cacheAccepted = tempCacheAccepted;
                return script;
            }
        }

        public override bool EnableInterruptPropagation
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetEnableInterruptPropagation(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_SetEnableInterruptPropagation(Handle, value));
        }

        public override V8RuntimeHeapInfo GetHeapInfo()
        {
            var totalHeapSize = 0UL;
            var totalHeapSizeExecutable = 0UL;
            var totalPhysicalSize = 0UL;
            var usedHeapSize = 0UL;
            var heapSizeLimit = 0UL;
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetHeapStatistics(Handle, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out usedHeapSize, out heapSizeLimit));

            return new V8RuntimeHeapInfo
            {
                TotalHeapSize = totalHeapSize,
                TotalHeapSizeExecutable = totalHeapSizeExecutable,
                TotalPhysicalSize = totalPhysicalSize,
                UsedHeapSize = usedHeapSize,
                HeapSizeLimit = heapSizeLimit
            };
        }

        public override V8Runtime.Statistics GetStatistics()
        {
            var statistics = new V8Runtime.Statistics();
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetStatistics(Handle, out statistics.ScriptCount, out statistics.ScriptCacheSize, out statistics.ModuleCount, out statistics.PostedTaskCounts, out statistics.InvokedTaskCounts));
            return statistics;
        }

        public override void CollectGarbage(bool exhaustive)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CollectGarbage(Handle, exhaustive));
        }

        public override bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Isolate_BeginCpuProfile(Handle, name, flags.HasFlag(V8CpuProfileFlags.EnableSampleCollection)));
        }

        public override V8.V8CpuProfile EndCpuProfile(string name)
        {
            var profile = new V8.V8CpuProfile();

            Action<V8CpuProfile.Ptr> action = pProfile => V8CpuProfile.ProcessProfile(Handle, pProfile, profile);
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Isolate_EndCpuProfile(Handle, name, pAction));
            }

            return profile;
        }

        public override void CollectCpuProfileSample()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Isolate_CollectCpuProfileSample(Handle));
        }

        public override uint CpuProfileSampleInterval
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_GetCpuProfileSampleInterval(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Isolate_SetCpuProfileSampleInterval(Handle, value));
        }

        public override void WriteHeapSnapshot(Stream stream)
        {
            using (var streamScope = V8ProxyHelpers.CreateAddRefHostObjectScope(stream))
            {
                var pStream = streamScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Isolate_WriteHeapSnapshot(Handle, pStream));
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
