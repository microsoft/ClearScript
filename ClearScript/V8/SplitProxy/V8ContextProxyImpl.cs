// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.ClearScript.Util;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal sealed class V8ContextProxyImpl : V8ContextProxy
    {
        private V8EntityHolder holder;

        private V8Context.Handle Handle => (V8Context.Handle)holder.Handle;

        public V8ContextProxyImpl(V8IsolateProxy isolateProxy, string name, V8ScriptEngineFlags flags, int debugPort)
        {
            holder = new V8EntityHolder("V8 script engine", () => ((V8IsolateProxyImpl)isolateProxy).CreateContext(name, flags, debugPort));
        }

        #region V8ContextProxy overrides

        public override UIntPtr MaxIsolateHeapSize
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetMaxIsolateHeapSize(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetMaxIsolateHeapSize(ctx.Handle, ctx.value), (Handle, value));
        }

        public override TimeSpan IsolateHeapSizeSampleInterval
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => TimeSpan.FromMilliseconds(instance.V8Context_GetIsolateHeapSizeSampleInterval(handle)), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetIsolateHeapSizeSampleInterval(ctx.Handle, ctx.value.TotalMilliseconds), (Handle, value));
        }

        public override UIntPtr MaxIsolateStackUsage
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetMaxIsolateStackUsage(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetMaxIsolateStackUsage(ctx.Handle, ctx.value), (Handle, value));
        }

        public override void InvokeWithLock(Action action)
        {
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_InvokeWithLock(ctx.Handle, ctx.pAction), (Handle, pAction));
            }
        }

        public override unsafe void InvokeWithLock<TArg>(Action<TArg> action, in TArg arg)
        {
            var ctx = (action, arg);
            var pCtx = (IntPtr)Unsafe.AsPointer(ref ctx);

            Action<IntPtr> actionWithArg = static pCtx =>
            {
                ref var ctx = ref Unsafe.AsRef<(Action<TArg> action, TArg arg)>(pCtx.ToPointer());
                ctx.action(ctx.arg);
            };

            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(actionWithArg))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_InvokeWithLockWithArg(ctx.Handle, ctx.pAction, ctx.pCtx), (Handle, pAction, pCtx));
            }
        }

        public override object GetRootItem()
        {
            return V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetRootItem(handle), Handle);
        }

        public override void AddGlobalItem(string name, object item, bool globalMembers)
        {
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_AddGlobalItem(ctx.Handle, ctx.name, ctx.item, ctx.globalMembers), (Handle, name, item, globalMembers));
        }

        public override void AwaitDebuggerAndPause()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_AwaitDebuggerAndPause(handle), Handle);
        }

        public override void CancelAwaitDebugger()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_CancelAwaitDebugger(handle), Handle);
        }

        public override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            return V8SplitProxyNative.Invoke(
                static (instance, ctx) => instance.V8Context_ExecuteCode(
                    ctx.Handle,
                    MiscHelpers.GetUrlOrPath(ctx.documentInfo.Uri, ctx.documentInfo.UniqueName),
                    MiscHelpers.GetUrlOrPath(ctx.documentInfo.SourceMapUri, string.Empty),
                    ctx.documentInfo.UniqueId,
                    ctx.documentInfo.Category.Kind,
                    V8ProxyHelpers.AddRefHostObject(ctx.documentInfo),
                    ctx.code,
                    ctx.evaluate
                ),
                (Handle, documentInfo, code, evaluate)
            );
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code)
        {
            return new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(
                static (instance, ctx) => instance.V8Context_Compile(
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
                    return instance.V8Context_CompileProducingCache(
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
                    return instance.V8Context_CompileConsumingCache(
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
                    return instance.V8Context_CompileUpdatingCache(
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

        public override object Execute(V8.V8Script script, bool evaluate)
        {
            if (script is V8ScriptImpl scriptImpl)
            {
                return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_ExecuteScript(ctx.Handle, ctx.scriptImpl.Handle, ctx.evaluate), (Handle, scriptImpl, evaluate));
            }

            throw new ArgumentException("Invalid compiled script", nameof(script));
        }

        public override void Interrupt()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_Interrupt(handle), Handle);
        }

        public override void CancelInterrupt()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_CancelInterrupt(handle), Handle);
        }

        public override bool EnableIsolateInterruptPropagation
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetEnableIsolateInterruptPropagation(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetEnableIsolateInterruptPropagation(ctx.Handle, ctx.value), (Handle, value));
        }

        public override bool DisableIsolateHeapSizeViolationInterrupt
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetDisableIsolateHeapSizeViolationInterrupt(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetDisableIsolateHeapSizeViolationInterrupt(ctx.Handle, ctx.value), (Handle, value));
        }

        public override V8RuntimeHeapInfo GetIsolateHeapInfo()
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
                    instance.V8Context_GetIsolateHeapStatistics(
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

        public override V8Runtime.Statistics GetIsolateStatistics()
        {
            var statistics = new V8Runtime.Statistics();
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_GetIsolateStatistics(ctx.Handle, out ctx.statistics.ScriptCount, out ctx.statistics.ScriptCacheSize, out ctx.statistics.ModuleCount, out ctx.statistics.PostedTaskCounts, out ctx.statistics.InvokedTaskCounts), (Handle, statistics));
            return statistics;
        }

        public override V8ScriptEngine.Statistics GetStatistics()
        {
            var statistics = new V8ScriptEngine.Statistics();
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_GetStatistics(ctx.Handle, out ctx.statistics.ScriptCount, out ctx.statistics.ModuleCount, out ctx.statistics.ModuleCacheSize), (Handle, statistics));
            return statistics;
        }

        public override void CollectGarbage(bool exhaustive)
        {
            V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_CollectGarbage(ctx.Handle, ctx.exhaustive), (Handle, exhaustive));
        }

        public override void OnAccessSettingsChanged()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_OnAccessSettingsChanged(handle), Handle);
        }

        public override bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            return V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_BeginCpuProfile(ctx.Handle, ctx.name, ctx.flags.HasAllFlags(V8CpuProfileFlags.EnableSampleCollection)), (Handle, name, flags));
        }

        public override V8.V8CpuProfile EndCpuProfile(string name)
        {
            var profile = new V8.V8CpuProfile();

            Action<V8CpuProfile.Ptr> action = pProfile => V8CpuProfile.ProcessProfile(Handle, pProfile, profile);
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_EndCpuProfile(ctx.Handle, ctx.name, ctx.pAction), (Handle, name, pAction));
            }

            return profile;
        }

        public override void CollectCpuProfileSample()
        {
            V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_CollectCpuProfileSample(handle), Handle);
        }

        public override uint CpuProfileSampleInterval
        {
            get => V8SplitProxyNative.Invoke(static (instance, handle) => instance.V8Context_GetCpuProfileSampleInterval(handle), Handle);
            set => V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_SetCpuProfileSampleInterval(ctx.Handle, ctx.value), (Handle, value));
        }

        public override void WriteIsolateHeapSnapshot(Stream stream)
        {
            using (var streamScope = V8ProxyHelpers.CreateAddRefHostObjectScope(stream))
            {
                var pStream = streamScope.Value;
                V8SplitProxyNative.Invoke(static (instance, ctx) => instance.V8Context_WriteIsolateHeapSnapshot(ctx.Handle, ctx.pStream), (Handle, pStream));
            }
        }

        #endregion

        #region disposal / finalization

        public override void Dispose()
        {
            holder.ReleaseEntity();
            GC.KeepAlive(this);
        }

        ~V8ContextProxyImpl()
        {
            V8EntityHolder.Destroy(ref holder);
        }

        #endregion
    }
}
