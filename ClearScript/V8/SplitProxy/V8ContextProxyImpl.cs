// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.ClearScript.JavaScript;
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
            get => V8SplitProxyNative.Invoke(instance => instance.V8Context_GetMaxIsolateHeapSize(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Context_SetMaxIsolateHeapSize(Handle, value));
        }

        public override TimeSpan IsolateHeapSizeSampleInterval
        {
            get => V8SplitProxyNative.Invoke(instance => TimeSpan.FromMilliseconds(instance.V8Context_GetIsolateHeapSizeSampleInterval(Handle)));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Context_SetIsolateHeapSizeSampleInterval(Handle, value.TotalMilliseconds));
        }

        public override UIntPtr MaxIsolateStackUsage
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Context_GetMaxIsolateStackUsage(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Context_SetMaxIsolateStackUsage(Handle, value));
        }

        public override void InvokeWithLock(Action action)
        {
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Context_InvokeWithLock(Handle, pAction));
            }
        }

        public override object GetRootItem()
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Context_GetRootItem(Handle));
        }

        public override void AddGlobalItem(string name, object item, bool globalMembers)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_AddGlobalItem(Handle, name, item, globalMembers));
        }

        public override void AwaitDebuggerAndPause()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_AwaitDebuggerAndPause(Handle));
        }

        public override void CancelAwaitDebugger()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_CancelAwaitDebugger(Handle));
        }

        public override object Execute(UniqueDocumentInfo documentInfo, string code, bool evaluate)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Context_ExecuteCode(
                Handle,
                MiscHelpers.GetUrlOrPath(documentInfo.Uri, documentInfo.UniqueName),
                MiscHelpers.GetUrlOrPath(documentInfo.SourceMapUri, string.Empty),
                documentInfo.UniqueId,
                documentInfo.Category == ModuleCategory.Standard,
                V8ProxyHelpers.AddRefHostObject(documentInfo),
                code,
                evaluate
            ));
        }

        public override V8.V8Script Compile(UniqueDocumentInfo documentInfo, string code)
        {
            return new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Context_Compile(
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
            var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Context_CompileProducingCache(
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
                var script = new V8ScriptImpl(documentInfo, code.GetDigest(), V8SplitProxyNative.Invoke(instance => instance.V8Context_CompileConsumingCache(
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

        public override object Execute(V8.V8Script script, bool evaluate)
        {
            if (!(script is V8ScriptImpl scriptImpl))
            {
                throw new ArgumentException("Invalid compiled script", nameof(script));
            }

            return V8SplitProxyNative.Invoke(instance => instance.V8Context_ExecuteScript(
                Handle,
                scriptImpl.Handle,
                evaluate
            ));
        }

        public override void Interrupt()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_Interrupt(Handle));
        }

        public override void CancelInterrupt()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_CancelInterrupt(Handle));
        }

        public override bool EnableIsolateInterruptPropagation
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Context_GetEnableIsolateInterruptPropagation(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Context_SetEnableIsolateInterruptPropagation(Handle, value));
        }

        public override V8RuntimeHeapInfo GetIsolateHeapInfo()
        {
            var totalHeapSize = 0UL;
            var totalHeapSizeExecutable = 0UL;
            var totalPhysicalSize = 0UL;
            var usedHeapSize = 0UL;
            var heapSizeLimit = 0UL;
            V8SplitProxyNative.Invoke(instance => instance.V8Context_GetIsolateHeapStatistics(Handle, out totalHeapSize, out totalHeapSizeExecutable, out totalPhysicalSize, out usedHeapSize, out heapSizeLimit));

            return new V8RuntimeHeapInfo
            {
                TotalHeapSize = totalHeapSize,
                TotalHeapSizeExecutable = totalHeapSizeExecutable,
                TotalPhysicalSize = totalPhysicalSize,
                UsedHeapSize = usedHeapSize,
                HeapSizeLimit = heapSizeLimit
            };
        }

        public override V8Runtime.Statistics GetIsolateStatistics()
        {
            var statistics = new V8Runtime.Statistics();
            V8SplitProxyNative.Invoke(instance => instance.V8Context_GetIsolateStatistics(Handle, out statistics.ScriptCount, out statistics.ScriptCacheSize, out statistics.ModuleCount, out statistics.PostedTaskCounts, out statistics.InvokedTaskCounts));
            return statistics;
        }

        public override V8ScriptEngine.Statistics GetStatistics()
        {
            var statistics = new V8ScriptEngine.Statistics();
            V8SplitProxyNative.Invoke(instance => instance.V8Context_GetStatistics(Handle, out statistics.ScriptCount, out statistics.ModuleCount, out statistics.ModuleCacheSize));
            return statistics;
        }

        public override void CollectGarbage(bool exhaustive)
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_CollectGarbage(Handle, exhaustive));
        }

        public override void OnAccessSettingsChanged()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_OnAccessSettingsChanged(Handle));
        }

        public override bool BeginCpuProfile(string name, V8CpuProfileFlags flags)
        {
            return V8SplitProxyNative.Invoke(instance => instance.V8Context_BeginCpuProfile(Handle, name, flags.HasFlag(V8CpuProfileFlags.EnableSampleCollection)));
        }

        public override V8.V8CpuProfile EndCpuProfile(string name)
        {
            var profile = new V8.V8CpuProfile();

            Action<V8CpuProfile.Ptr> action = pProfile => V8CpuProfile.ProcessProfile(Handle, pProfile, profile);
            using (var actionScope = V8ProxyHelpers.CreateAddRefHostObjectScope(action))
            {
                var pAction = actionScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Context_EndCpuProfile(Handle, name, pAction));
            }

            return profile;
        }

        public override void CollectCpuProfileSample()
        {
            V8SplitProxyNative.Invoke(instance => instance.V8Context_CollectCpuProfileSample(Handle));
        }

        public override uint CpuProfileSampleInterval
        {
            get => V8SplitProxyNative.Invoke(instance => instance.V8Context_GetCpuProfileSampleInterval(Handle));
            set => V8SplitProxyNative.Invoke(instance => instance.V8Context_SetCpuProfileSampleInterval(Handle, value));
        }

        public override void WriteIsolateHeapSnapshot(Stream stream)
        {
            using (var streamScope = V8ProxyHelpers.CreateAddRefHostObjectScope(stream))
            {
                var pStream = streamScope.Value;
                V8SplitProxyNative.Invoke(instance => instance.V8Context_WriteIsolateHeapSnapshot(Handle, pStream));
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
