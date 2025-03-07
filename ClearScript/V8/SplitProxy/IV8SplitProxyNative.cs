// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Microsoft.ClearScript.V8.SplitProxy
{
    internal interface IV8SplitProxyNative
    {
        #region initialization

        IntPtr V8SplitProxyManaged_SetMethodTable(IntPtr pMethodTable);
        string V8SplitProxyNative_GetVersion();
        void V8Environment_InitializeICU(IntPtr pICUData, uint size);

        #endregion

        #region memory methods

        IntPtr Memory_Allocate(UIntPtr size);
        IntPtr Memory_AllocateZeroed(UIntPtr size);
        void Memory_Free(IntPtr pMemory);

        #endregion

        #region StdString methods

        StdString.Ptr StdString_New(string value);
        string StdString_GetValue(StdString.Ptr pString);
        TValue StdString_GetValue<TValue>(StdString.Ptr pString, Func<IntPtr, int, TValue> factory);
        TValue StdString_GetValue<TValue, TArg>(StdString.Ptr pString, Func<IntPtr, int, TArg, TValue> factory, in TArg arg);
        void StdString_SetValue(StdString.Ptr pString, string value);
        void StdString_Delete(StdString.Ptr pString);

        #endregion

        #region StdStringArray methods

        StdStringArray.Ptr StdStringArray_New(int elementCount);
        int StdStringArray_GetElementCount(StdStringArray.Ptr pArray);
        void StdStringArray_SetElementCount(StdStringArray.Ptr pArray, int elementCount);
        string StdStringArray_GetElement(StdStringArray.Ptr pArray, int index);
        void StdStringArray_SetElement(StdStringArray.Ptr pArray, int index, string value);
        void StdStringArray_Delete(StdStringArray.Ptr pArray);

        #endregion

        #region StdByteArray methods

        StdByteArray.Ptr StdByteArray_New(int elementCount);
        int StdByteArray_GetElementCount(StdByteArray.Ptr pArray);
        void StdByteArray_SetElementCount(StdByteArray.Ptr pArray, int elementCount);
        IntPtr StdByteArray_GetData(StdByteArray.Ptr pArray);
        void StdByteArray_Delete(StdByteArray.Ptr pArray);

        #endregion

        #region StdInt32Array methods

        StdInt32Array.Ptr StdInt32Array_New(int elementCount);
        int StdInt32Array_GetElementCount(StdInt32Array.Ptr pArray);
        void StdInt32Array_SetElementCount(StdInt32Array.Ptr pArray, int elementCount);
        IntPtr StdInt32Array_GetData(StdInt32Array.Ptr pArray);
        void StdInt32Array_Delete(StdInt32Array.Ptr pArray);

        #endregion

        #region StdUInt32Array methods

        StdUInt32Array.Ptr StdUInt32Array_New(int elementCount);
        int StdUInt32Array_GetElementCount(StdUInt32Array.Ptr pArray);
        void StdUInt32Array_SetElementCount(StdUInt32Array.Ptr pArray, int elementCount);
        IntPtr StdUInt32Array_GetData(StdUInt32Array.Ptr pArray);
        void StdUInt32Array_Delete(StdUInt32Array.Ptr pArray);

        #endregion

        #region StdUInt64Array methods

        StdUInt64Array.Ptr StdUInt64Array_New(int elementCount);
        int StdUInt64Array_GetElementCount(StdUInt64Array.Ptr pArray);
        void StdUInt64Array_SetElementCount(StdUInt64Array.Ptr pArray, int elementCount);
        IntPtr StdUInt64Array_GetData(StdUInt64Array.Ptr pArray);
        void StdUInt64Array_Delete(StdUInt64Array.Ptr pArray);

        #endregion

        #region StdPtrArray methods

        StdPtrArray.Ptr StdPtrArray_New(int elementCount);
        int StdPtrArray_GetElementCount(StdPtrArray.Ptr pArray);
        void StdPtrArray_SetElementCount(StdPtrArray.Ptr pArray, int elementCount);
        IntPtr StdPtrArray_GetData(StdPtrArray.Ptr pArray);
        void StdPtrArray_Delete(StdPtrArray.Ptr pArray);

        #endregion

        #region StdV8ValueArray methods

        StdV8ValueArray.Ptr StdV8ValueArray_New(int elementCount);
        int StdV8ValueArray_GetElementCount(StdV8ValueArray.Ptr pArray);
        void StdV8ValueArray_SetElementCount(StdV8ValueArray.Ptr pArray, int elementCount);
        V8Value.Ptr StdV8ValueArray_GetData(StdV8ValueArray.Ptr pArray);
        void StdV8ValueArray_Delete(StdV8ValueArray.Ptr pArray);

        #endregion

        #region V8Value methods

        V8Value.Ptr V8Value_New();
        void V8Value_SetNonexistent(V8Value.Ptr pV8Value);
        void V8Value_SetUndefined(V8Value.Ptr pV8Value);
        void V8Value_SetNull(V8Value.Ptr pV8Value);
        void V8Value_SetBoolean(V8Value.Ptr pV8Value, bool value);
        void V8Value_SetNumber(V8Value.Ptr pV8Value, double value);
        void V8Value_SetString(V8Value.Ptr pV8Value, string value);
        void V8Value_SetDateTime(V8Value.Ptr pV8Value, double value);
        void V8Value_SetBigInt(V8Value.Ptr pV8Value, int signBit, byte[] bytes);
        void V8Value_SetV8Object(V8Value.Ptr pV8Value, V8Object.Handle hObject, V8Value.Subtype subtype, V8Value.Flags flags);
        void V8Value_SetHostObject(V8Value.Ptr pV8Value, IntPtr pObject, V8Value.Subtype subtype, V8Value.Flags flags);
        void V8Value_Decode(V8Value.Ptr pV8Value, out V8Value.Decoded decoded);
        void V8Value_Delete(V8Value.Ptr pV8Value);

        #endregion

        #region V8CpuProfile methods

        void V8CpuProfile_GetInfo(V8CpuProfile.Ptr pProfile, V8Entity.Handle hEntity, out string name, out ulong startTimestamp, out ulong endTimestamp, out int sampleCount, out V8CpuProfile.Node.Ptr pRootNode);
        bool V8CpuProfile_GetSample(V8CpuProfile.Ptr pProfile, int index, out ulong nodeId, out ulong timestamp);
        void V8CpuProfileNode_GetInfo(V8CpuProfile.Node.Ptr pNode, V8Entity.Handle hEntity, out ulong nodeId, out long scriptId, out string scriptName, out string functionName, out string bailoutReason, out long lineNumber, out long columnNumber, out ulong hitCount, out uint hitLineCount, out int childCount);
        bool V8CpuProfileNode_GetHitLines(V8CpuProfile.Node.Ptr pNode, out int[] lineNumbers, out uint[] hitCounts);
        V8CpuProfile.Node.Ptr V8CpuProfileNode_GetChildNode(V8CpuProfile.Node.Ptr pNode, int index);

        #endregion

        #region V8 isolate methods

        V8Isolate.Handle V8Isolate_Create(string name, int maxNewSpaceSize, int maxOldSpaceSize, double heapExpansionMultiplier, ulong maxArrayBufferAllocation, V8RuntimeFlags flags, int debugPort);
        V8Context.Handle V8Isolate_CreateContext(V8Isolate.Handle hIsolate, string name, V8ScriptEngineFlags flags, int debugPort);
        UIntPtr V8Isolate_GetMaxHeapSize(V8Isolate.Handle hIsolate);
        void V8Isolate_SetMaxHeapSize(V8Isolate.Handle hIsolate, UIntPtr size);
        double V8Isolate_GetHeapSizeSampleInterval(V8Isolate.Handle hIsolate);
        void V8Isolate_SetHeapSizeSampleInterval(V8Isolate.Handle hIsolate, double milliseconds);
        UIntPtr V8Isolate_GetMaxStackUsage(V8Isolate.Handle hIsolate);
        void V8Isolate_SetMaxStackUsage(V8Isolate.Handle hIsolate, UIntPtr size);
        void V8Isolate_AwaitDebuggerAndPause(V8Isolate.Handle hIsolate);
        void V8Isolate_CancelAwaitDebugger(V8Isolate.Handle hIsolate);
        V8Script.Handle V8Isolate_Compile(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code);
        V8Script.Handle V8Isolate_CompileProducingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes);
        V8Script.Handle V8Isolate_CompileConsumingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);
        V8Script.Handle V8Isolate_CompileUpdatingCache(V8Isolate.Handle hIsolate, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult);
        bool V8Isolate_GetEnableInterruptPropagation(V8Isolate.Handle hIsolate);
        void V8Isolate_SetEnableInterruptPropagation(V8Isolate.Handle hIsolate, bool value);
        bool V8Isolate_GetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate);
        void V8Isolate_SetDisableHeapSizeViolationInterrupt(V8Isolate.Handle hIsolate, bool value);
        void V8Isolate_GetHeapStatistics(V8Isolate.Handle hIsolate, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize);
        void V8Isolate_GetStatistics(V8Isolate.Handle hIsolate, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts);
        void V8Isolate_CollectGarbage(V8Isolate.Handle hIsolate, bool exhaustive);
        bool V8Isolate_BeginCpuProfile(V8Isolate.Handle hIsolate, string name, bool recordSamples);
        void V8Isolate_EndCpuProfile(V8Isolate.Handle hIsolate, string name, IntPtr pAction);
        void V8Isolate_CollectCpuProfileSample(V8Isolate.Handle hIsolate);
        uint V8Isolate_GetCpuProfileSampleInterval(V8Isolate.Handle hIsolate);
        void V8Isolate_SetCpuProfileSampleInterval(V8Isolate.Handle hIsolate, uint value);
        void V8Isolate_WriteHeapSnapshot(V8Isolate.Handle hIsolate, IntPtr pStream);

        #endregion

        #region V8 context methods

        UIntPtr V8Context_GetMaxIsolateHeapSize(V8Context.Handle hContext);
        void V8Context_SetMaxIsolateHeapSize(V8Context.Handle hContext, UIntPtr size);
        double V8Context_GetIsolateHeapSizeSampleInterval(V8Context.Handle hContext);
        void V8Context_SetIsolateHeapSizeSampleInterval(V8Context.Handle hContext, double milliseconds);
        UIntPtr V8Context_GetMaxIsolateStackUsage(V8Context.Handle hContext);
        void V8Context_SetMaxIsolateStackUsage(V8Context.Handle hContext, UIntPtr size);
        void V8Context_InvokeWithLock(V8Context.Handle hContext, IntPtr pAction);
        void V8Context_InvokeWithLockWithArg(V8Context.Handle hContext, IntPtr pAction, IntPtr pArg);
        object V8Context_GetRootItem(V8Context.Handle hContext);
        void V8Context_AddGlobalItem(V8Context.Handle hContext, string name, object value, bool globalMembers);
        void V8Context_AwaitDebuggerAndPause(V8Context.Handle hContext);
        void V8Context_CancelAwaitDebugger(V8Context.Handle hContext);
        object V8Context_ExecuteCode(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, bool evaluate);
        V8Script.Handle V8Context_Compile(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code);
        V8Script.Handle V8Context_CompileProducingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, out byte[] cacheBytes);
        V8Script.Handle V8Context_CompileConsumingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, byte[] cacheBytes, out bool cacheAccepted);
        V8Script.Handle V8Context_CompileUpdatingCache(V8Context.Handle hContext, string resourceName, string sourceMapUrl, ulong uniqueId, DocumentKind documentKind, IntPtr pDocumentInfo, string code, V8CacheKind cacheKind, ref byte[] cacheBytes, out V8CacheResult cacheResult);
        object V8Context_ExecuteScript(V8Context.Handle hContext, V8Script.Handle hScript, bool evaluate);
        void V8Context_Interrupt(V8Context.Handle hContext);
        void V8Context_CancelInterrupt(V8Context.Handle hContext);
        bool V8Context_GetEnableIsolateInterruptPropagation(V8Context.Handle hContext);
        void V8Context_SetEnableIsolateInterruptPropagation(V8Context.Handle hContext, bool value);
        bool V8Context_GetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext);
        void V8Context_SetDisableIsolateHeapSizeViolationInterrupt(V8Context.Handle hContext, bool value);
        void V8Context_GetIsolateHeapStatistics(V8Context.Handle hContext, out ulong totalHeapSize, out ulong totalHeapSizeExecutable, out ulong totalPhysicalSize, out ulong totalAvailableSize, out ulong usedHeapSize, out ulong heapSizeLimit, out ulong totalExternalSize);
        void V8Context_GetIsolateStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong scriptCacheSize, out ulong moduleCount, out ulong[] postedTaskCounts, out ulong[] invokedTaskCounts);
        void V8Context_GetStatistics(V8Context.Handle hContext, out ulong scriptCount, out ulong moduleCount, out ulong moduleCacheSize);
        void V8Context_CollectGarbage(V8Context.Handle hContext, bool exhaustive);
        void V8Context_OnAccessSettingsChanged(V8Context.Handle hContext);
        bool V8Context_BeginCpuProfile(V8Context.Handle hContext, string name, bool recordSamples);
        void V8Context_EndCpuProfile(V8Context.Handle hContext, string name, IntPtr pAction);
        void V8Context_CollectCpuProfileSample(V8Context.Handle hContext);
        uint V8Context_GetCpuProfileSampleInterval(V8Context.Handle hContext);
        void V8Context_SetCpuProfileSampleInterval(V8Context.Handle hContext, uint value);
        void V8Context_WriteIsolateHeapSnapshot(V8Context.Handle hContext, IntPtr pStream);

        #endregion

        #region V8 object methods

        object V8Object_GetNamedProperty(V8Object.Handle hObject, string name);
        bool V8Object_TryGetNamedProperty(V8Object.Handle hObject, string name, out object value);
        void V8Object_SetNamedProperty(V8Object.Handle hObject, string name, object value);
        bool V8Object_DeleteNamedProperty(V8Object.Handle hObject, string name);
        string[] V8Object_GetPropertyNames(V8Object.Handle hObject, bool includeIndices);
        object V8Object_GetIndexedProperty(V8Object.Handle hObject, int index);
        void V8Object_SetIndexedProperty(V8Object.Handle hObject, int index, object value);
        bool V8Object_DeleteIndexedProperty(V8Object.Handle hObject, int index);
        int[] V8Object_GetPropertyIndices(V8Object.Handle hObject);
        object V8Object_Invoke(V8Object.Handle hObject, bool asConstructor, object[] args);
        object V8Object_InvokeMethod(V8Object.Handle hObject, string name, object[] args);
        void V8Object_GetArrayBufferOrViewInfo(V8Object.Handle hObject, out IV8Object arrayBuffer, out ulong offset, out ulong size, out ulong length);
        void V8Object_InvokeWithArrayBufferOrViewData(V8Object.Handle hObject, IntPtr pAction);
        void V8Object_InvokeWithArrayBufferOrViewDataWithArg(V8Object.Handle hObject, IntPtr pAction, IntPtr pArg);

        #endregion

        #region V8 debug callback methods

        void V8DebugCallback_ConnectClient(V8DebugCallback.Handle hCallback);
        void V8DebugCallback_SendCommand(V8DebugCallback.Handle hCallback, string command);
        void V8DebugCallback_DisconnectClient(V8DebugCallback.Handle hCallback);

        #endregion

        #region native callback methods

        void NativeCallback_Invoke(NativeCallback.Handle hCallback);

        #endregion

        #region V8 entity methods

        void V8Entity_Release(V8Entity.Handle hEntity);
        V8Entity.Handle V8Entity_CloneHandle(V8Entity.Handle hEntity);
        void V8Entity_DestroyHandle(V8Entity.Handle hEntity);

        #endregion

        #region error handling

        void HostException_Schedule(string message, object exception);

        #endregion

        #region unit test support

        UIntPtr V8UnitTestSupport_GetTextDigest(string value);
        void V8UnitTestSupport_GetStatistics(out ulong isolateCount, out ulong contextCount);

        #endregion
    }
}
