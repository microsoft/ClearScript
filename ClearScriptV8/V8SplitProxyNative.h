// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8EntityName
//-----------------------------------------------------------------------------

template <typename T>
struct V8EntityName final: StaticBase
{
};

//-----------------------------------------------------------------------------
// V8EntityStringFactory
//-----------------------------------------------------------------------------

template <typename T>
struct V8EntityStringFactory final: StaticBase
{
    static StdString CreateStdString(T& /*entity*/, v8::Local<v8::Value> /*hValue*/)
    {
        return StdString();
    }
};

//-----------------------------------------------------------------------------

template <>
struct V8EntityStringFactory<V8Isolate> final: StaticBase
{
    static StdString CreateStdString(V8Isolate& isolate, v8::Local<v8::Value> hValue)
    {
        return isolate.CreateStdString(hValue);
    }
};

//-----------------------------------------------------------------------------

template <>
struct V8EntityStringFactory<V8Context> final: StaticBase
{
    static StdString CreateStdString(V8Context& context, v8::Local<v8::Value> hValue)
    {
        return context.CreateStdString(hValue);
    }
};

//-----------------------------------------------------------------------------
// V8EntityHandleBase
//-----------------------------------------------------------------------------

class V8EntityHandleBase
{
public:

    virtual V8EntityHandleBase* Clone() const noexcept = 0;
    virtual StdString CreateStdString(v8::Local<v8::Value> hValue) const noexcept = 0;
    virtual void ReleaseEntity() noexcept = 0;
    virtual ~V8EntityHandleBase() noexcept {}

protected:

    static StdString GetEntityReleasedMessage(const StdChar* pName) noexcept;
    static void ScheduleInvalidOperationException(const StdString& message) noexcept;
};

//-----------------------------------------------------------------------------
// V8EntityHandle
//-----------------------------------------------------------------------------

template <typename T>
class V8EntityHandle final: public V8EntityHandleBase
{
    PROHIBIT_COPY(V8EntityHandle)

public:

    explicit V8EntityHandle(T* pEntity) noexcept:
        m_spEntity(pEntity)
    {
    }

    SharedPtr<T> GetEntity() const noexcept
    {
        SharedPtr<T> spEntity;

        BEGIN_MUTEX_SCOPE(m_Mutex)
            spEntity = m_spEntity;
        END_MUTEX_SCOPE

        if (spEntity.IsEmpty())
        {
            ScheduleInvalidOperationException(GetEntityReleasedMessage(V8EntityName<T>::Get()));
        }
        
        return spEntity;
    }

    bool TryGetEntity(SharedPtr<T>& spEntity) const noexcept
    {
        BEGIN_MUTEX_SCOPE(m_Mutex)

            if (m_spEntity.IsEmpty())
            {
                return false;
            }

            spEntity = m_spEntity;
            return true;

        END_MUTEX_SCOPE
    }

    V8EntityHandleBase* Clone() const noexcept override
    {
        SharedPtr<T> spEntity;

        BEGIN_MUTEX_SCOPE(m_Mutex)
            spEntity = m_spEntity;
        END_MUTEX_SCOPE

        return new V8EntityHandle(std::move(spEntity));
    }

    StdString CreateStdString(v8::Local<v8::Value> hValue) const noexcept override
    {
        SharedPtr<T> spEntity;
        return TryGetEntity(spEntity) ? V8EntityStringFactory<T>::CreateStdString(*spEntity, hValue) : StdString();
    }

    void ReleaseEntity() noexcept override
    {
        SharedPtr<T> spEntity;

        BEGIN_MUTEX_SCOPE(m_Mutex)
            spEntity = std::move(m_spEntity);
        END_MUTEX_SCOPE
    }

private:

    explicit V8EntityHandle(SharedPtr<T>&& spEntity) noexcept :
        m_spEntity(std::move(spEntity))
    {
    }

    mutable SimpleMutex m_Mutex;
    SharedPtr<T> m_spEntity;
};

//-----------------------------------------------------------------------------

#define DEFINE_V8_ENTITY_HANDLE(TYPE, TARGET, NAME) \
    template <> struct V8EntityName<TARGET> final: StaticBase { static const StdChar* Get() { return NAME; } }; \
    using TYPE = V8EntityHandle<TARGET>;

DEFINE_V8_ENTITY_HANDLE(V8IsolateHandle, V8Isolate, SL("V8 runtime"))
DEFINE_V8_ENTITY_HANDLE(V8ContextHandle, V8Context, SL("V8 script engine"))
DEFINE_V8_ENTITY_HANDLE(V8ObjectHandle, V8ObjectHolder, SL("V8 object"))
DEFINE_V8_ENTITY_HANDLE(V8ScriptHandle, V8ScriptHolder, SL("V8 script"))
DEFINE_V8_ENTITY_HANDLE(V8DebugCallbackHandle, HostObjectUtil::DebugCallback, SL("V8 debug callback"))
DEFINE_V8_ENTITY_HANDLE(NativeCallbackHandle, HostObjectUtil::NativeCallback, SL("native callback"))

//-----------------------------------------------------------------------------
// V8 split proxy native entry points
//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void**) V8SplitProxyManaged_SetMethodTable(void** pMethodTable) noexcept;
NATIVE_ENTRY_POINT(const StdChar*) V8SplitProxyNative_GetVersion() noexcept;
NATIVE_ENTRY_POINT(void) V8Environment_InitializeICU(const char* pICUData, uint32_t size) noexcept;

NATIVE_ENTRY_POINT(void*) Memory_Allocate(size_t size) noexcept;
NATIVE_ENTRY_POINT(void*) Memory_AllocateZeroed(size_t size) noexcept;
NATIVE_ENTRY_POINT(void) Memory_Free(const void* pMemory) noexcept;

NATIVE_ENTRY_POINT(StdString*) StdString_New(const StdChar* pValue, int32_t length) noexcept;
NATIVE_ENTRY_POINT(const StdChar*) StdString_GetValue(const StdString& string, int32_t& length) noexcept;
NATIVE_ENTRY_POINT(void) StdString_SetValue(StdString& string, const StdChar* pValue, int32_t length) noexcept;
NATIVE_ENTRY_POINT(void) StdString_Delete(StdString* pString) noexcept;

NATIVE_ENTRY_POINT(std::vector<StdString>*) StdStringArray_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdStringArray_GetElementCount(const std::vector<StdString>& stringArray) noexcept;
NATIVE_ENTRY_POINT(void) StdStringArray_SetElementCount(std::vector<StdString>& stringArray, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(const StdChar*) StdStringArray_GetElement(const std::vector<StdString>& stringArray, int32_t index, int32_t& length) noexcept;
NATIVE_ENTRY_POINT(void) StdStringArray_SetElement(std::vector<StdString>& stringArray, int32_t index, const StdChar* pValue, int32_t length) noexcept;
NATIVE_ENTRY_POINT(void) StdStringArray_Delete(std::vector<StdString>* pStringArray) noexcept;

NATIVE_ENTRY_POINT(std::vector<uint8_t>*) StdByteArray_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdByteArray_GetElementCount(const std::vector<uint8_t>& byteArray) noexcept;
NATIVE_ENTRY_POINT(void) StdByteArray_SetElementCount(std::vector<uint8_t>& byteArray, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(uint8_t*) StdByteArray_GetData(std::vector<uint8_t>& byteArray) noexcept;
NATIVE_ENTRY_POINT(void) StdByteArray_Delete(std::vector<uint8_t>* pByteArray) noexcept;

NATIVE_ENTRY_POINT(std::vector<int32_t>*) StdInt32Array_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdInt32Array_GetElementCount(const std::vector<int32_t>& int32Array) noexcept;
NATIVE_ENTRY_POINT(void) StdInt32Array_SetElementCount(std::vector<int32_t>& int32Array, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t*) StdInt32Array_GetData(std::vector<int32_t>& int32Array) noexcept;
NATIVE_ENTRY_POINT(void) StdInt32Array_Delete(std::vector<int32_t>* pInt32Array) noexcept;

NATIVE_ENTRY_POINT(std::vector<uint32_t>*) StdUInt32Array_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdUInt32Array_GetElementCount(const std::vector<uint32_t>& uint32Array) noexcept;
NATIVE_ENTRY_POINT(void) StdUInt32Array_SetElementCount(std::vector<uint32_t>& uint32Array, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(uint32_t*) StdUInt32Array_GetData(std::vector<uint32_t>& uint32Array) noexcept;
NATIVE_ENTRY_POINT(void) StdUInt32Array_Delete(std::vector<uint32_t>* pUInt32Array) noexcept;

NATIVE_ENTRY_POINT(std::vector<uint64_t>*) StdUInt64Array_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdUInt64Array_GetElementCount(const std::vector<uint64_t>& uint64Array) noexcept;
NATIVE_ENTRY_POINT(void) StdUInt64Array_SetElementCount(std::vector<uint64_t>& uint64Array, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(uint64_t*) StdUInt64Array_GetData(std::vector<uint64_t>& uint64Array) noexcept;
NATIVE_ENTRY_POINT(void) StdUInt64Array_Delete(std::vector<uint64_t>* pUInt64Array) noexcept;

NATIVE_ENTRY_POINT(std::vector<void*>*) StdPtrArray_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdPtrArray_GetElementCount(const std::vector<void*>& ptrArray) noexcept;
NATIVE_ENTRY_POINT(void) StdPtrArray_SetElementCount(std::vector<void*>& ptrArray, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(void**) StdPtrArray_GetData(std::vector<void*>& ptrArray) noexcept;
NATIVE_ENTRY_POINT(void) StdPtrArray_Delete(std::vector<void*>* pPtrArray) noexcept;

NATIVE_ENTRY_POINT(std::vector<V8Value>*) StdV8ValueArray_New(int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(int32_t) StdV8ValueArray_GetElementCount(const std::vector<V8Value>& v8ValueArray) noexcept;
NATIVE_ENTRY_POINT(void) StdV8ValueArray_SetElementCount(std::vector<V8Value>& v8ValueArray, int32_t elementCount) noexcept;
NATIVE_ENTRY_POINT(V8Value*) StdV8ValueArray_GetData(std::vector<V8Value>& v8ValueArray) noexcept;
NATIVE_ENTRY_POINT(void) StdV8ValueArray_Delete(std::vector<V8Value>* pV8ValueArray) noexcept;

NATIVE_ENTRY_POINT(V8Value*) V8Value_New() noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetNonexistent(V8Value* pV8Value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetUndefined(V8Value* pV8Value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetNull(V8Value* pV8Value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetBoolean(V8Value* pV8Value, StdBool value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetNumber(V8Value* pV8Value, double value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetString(V8Value* pV8Value, const StdChar* pValue, int32_t length) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetDateTime(V8Value* pV8Value, double value) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetBigInt(V8Value* pV8Value, int32_t signBit, const uint8_t* pBytes, int32_t length) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetV8Object(V8Value* pV8Value, const V8ObjectHandle& handle, V8Value::Subtype subtype, V8Value::Flags flags) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_SetHostObject(V8Value* pV8Value, void* pvObject, V8Value::Subtype subtype, V8Value::Flags flags) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_Decode(const V8Value& value, V8Value::Decoded& decoded) noexcept;
NATIVE_ENTRY_POINT(void) V8Value_Delete(V8Value* pV8Value) noexcept;

NATIVE_ENTRY_POINT(void) V8CpuProfile_GetInfo(const v8::CpuProfile& profile, const V8EntityHandleBase& entityHandle, StdString& name, uint64_t& startTimestamp, uint64_t& endTimestamp, int32_t& sampleCount, const v8::CpuProfileNode*& pRootNode) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8CpuProfile_GetSample(const v8::CpuProfile& profile, int32_t index, uint64_t& nodeId, uint64_t& timestamp) noexcept;
NATIVE_ENTRY_POINT(void) V8CpuProfileNode_GetInfo(const v8::CpuProfileNode& node, const V8EntityHandleBase& entityHandle, uint64_t& nodeId, int64_t& scriptId, StdString& scriptName, StdString& functionName, StdString& bailoutReason, int64_t& lineNumber, int64_t& columnNumber, uint64_t& hitCount, uint32_t& hitLineCount, int32_t& childCount);
NATIVE_ENTRY_POINT(StdBool) V8CpuProfileNode_GetHitLines(const v8::CpuProfileNode& node, std::vector<int32_t>& lineNumbers, std::vector<uint32_t>& hitCounts) noexcept;
NATIVE_ENTRY_POINT(const v8::CpuProfileNode*) V8CpuProfileNode_GetChildNode(const v8::CpuProfileNode& node, int32_t index) noexcept;

NATIVE_ENTRY_POINT(V8IsolateHandle*) V8Isolate_Create(const StdString& name, int32_t maxNewSpaceSize, int32_t maxOldSpaceSize, double heapExpansionMultiplier, uint64_t maxArrayBufferAllocation, V8Isolate::Flags flags, int32_t debugPort) noexcept;
NATIVE_ENTRY_POINT(V8ContextHandle*) V8Isolate_CreateContext(const V8IsolateHandle& handle, const StdString& name, V8Context::Flags flags, int32_t debugPort) noexcept;
NATIVE_ENTRY_POINT(size_t) V8Isolate_GetMaxHeapSize(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetMaxHeapSize(const V8IsolateHandle& handle, size_t size) noexcept;
NATIVE_ENTRY_POINT(double) V8Isolate_GetHeapSizeSampleInterval(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetHeapSizeSampleInterval(const V8IsolateHandle& handle, double milliseconds) noexcept;
NATIVE_ENTRY_POINT(size_t) V8Isolate_GetMaxStackUsage(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetMaxStackUsage(const V8IsolateHandle& handle, size_t size) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_AwaitDebuggerAndPause(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_CancelAwaitDebugger(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_Compile(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_CompileProducingCache(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_CompileConsumingCache(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, StdBool& cacheAccepted) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_CompileUpdatingCache(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Isolate_GetEnableInterruptPropagation(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetEnableInterruptPropagation(const V8IsolateHandle& handle, StdBool value) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Isolate_GetDisableHeapSizeViolationInterrupt(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetDisableHeapSizeViolationInterrupt(const V8IsolateHandle& handle, StdBool value) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_GetHeapStatistics(const V8IsolateHandle& handle, uint64_t& totalHeapSize, uint64_t& totalHeapSizeExecutable, uint64_t& totalPhysicalSize, uint64_t& totalAvailableSize, uint64_t& usedHeapSize, uint64_t& heapSizeLimit, uint64_t& totalExternalSize) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_GetStatistics(const V8IsolateHandle& handle, uint64_t& scriptCount, uint64_t& scriptCacheSize, uint64_t& moduleCount, std::vector<uint64_t>& postedTaskCounts, std::vector<uint64_t>& invokedTaskCounts) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_CollectGarbage(const V8IsolateHandle& handle, StdBool exhaustive) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Isolate_BeginCpuProfile(const V8IsolateHandle& handle, const StdString& name, StdBool recordSamples) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_EndCpuProfile(const V8IsolateHandle& handle, const StdString& name, void* pvAction) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_CollectCpuProfileSample(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(uint32_t) V8Isolate_GetCpuProfileSampleInterval(const V8IsolateHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_SetCpuProfileSampleInterval(const V8IsolateHandle& handle, uint32_t value) noexcept;
NATIVE_ENTRY_POINT(void) V8Isolate_WriteHeapSnapshot(const V8IsolateHandle& handle, void* pvStream) noexcept;

NATIVE_ENTRY_POINT(size_t) V8Context_GetMaxIsolateHeapSize(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetMaxIsolateHeapSize(const V8ContextHandle& handle, size_t size) noexcept;
NATIVE_ENTRY_POINT(double) V8Context_GetIsolateHeapSizeSampleInterval(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetIsolateHeapSizeSampleInterval(const V8ContextHandle& handle, double milliseconds) noexcept;
NATIVE_ENTRY_POINT(size_t) V8Context_GetMaxIsolateStackUsage(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetMaxIsolateStackUsage(const V8ContextHandle& handle, size_t size) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_InvokeWithLock(const V8ContextHandle& handle, void* pvAction) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_InvokeWithLockWithArg(const V8ContextHandle& handle, void* pvAction, void* pvArg) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_GetRootItem(const V8ContextHandle& handle, V8Value& item) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_AddGlobalItem(const V8ContextHandle& handle, const StdString& name, const V8Value& value, StdBool globalMembers) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_AwaitDebuggerAndPause(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_CancelAwaitDebugger(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_ExecuteCode(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, const StdString& code, StdBool evaluate, V8Value& result) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_Compile(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_CompileProducingCache(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_CompileConsumingCache(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, StdBool& cacheAccepted) noexcept;
NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_CompileUpdatingCache(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, DocumentKind documentKind, void* pvDocumentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_ExecuteScript(const V8ContextHandle& handle, const V8ScriptHandle& scriptHandle, StdBool evaluate, V8Value& result) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_Interrupt(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_CancelInterrupt(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Context_GetEnableIsolateInterruptPropagation(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetEnableIsolateInterruptPropagation(const V8ContextHandle& handle, StdBool value) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Context_GetDisableIsolateHeapSizeViolationInterrupt(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetDisableIsolateHeapSizeViolationInterrupt(const V8ContextHandle& handle, StdBool value) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_GetIsolateHeapStatistics(const V8ContextHandle& handle, uint64_t& totalHeapSize, uint64_t& totalHeapSizeExecutable, uint64_t& totalPhysicalSize, uint64_t& totalAvailableSize, uint64_t& usedHeapSize, uint64_t& heapSizeLimit, uint64_t& totalExternalSize) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_GetIsolateStatistics(const V8ContextHandle& handle, uint64_t& scriptCount, uint64_t& scriptCacheSize, uint64_t& moduleCount, std::vector<uint64_t>& postedTaskCounts, std::vector<uint64_t>& invokedTaskCounts) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_GetStatistics(const V8ContextHandle& handle, uint64_t& scriptCount, uint64_t& moduleCount, uint64_t& moduleCacheSize) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_CollectGarbage(const V8ContextHandle& handle, StdBool exhaustive) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_OnAccessSettingsChanged(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Context_BeginCpuProfile(const V8ContextHandle& handle, const StdString& name, StdBool recordSamples) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_EndCpuProfile(const V8ContextHandle& handle, const StdString& name, void* pvAction) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_CollectCpuProfileSample(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(uint32_t) V8Context_GetCpuProfileSampleInterval(const V8ContextHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_SetCpuProfileSampleInterval(const V8ContextHandle& handle, uint32_t value) noexcept;
NATIVE_ENTRY_POINT(void) V8Context_WriteIsolateHeapSnapshot(const V8ContextHandle& handle, void* pvStream) noexcept;

NATIVE_ENTRY_POINT(void) V8Object_GetNamedProperty(const V8ObjectHandle& handle, const StdString& name, V8Value& value) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Object_TryGetNamedProperty(const V8ObjectHandle& handle, const StdString& name, V8Value& value) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_SetNamedProperty(const V8ObjectHandle& handle, const StdString& name, const V8Value& value) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Object_DeleteNamedProperty(const V8ObjectHandle& handle, const StdString& name) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_GetPropertyNames(const V8ObjectHandle& handle, StdBool includeIndices, std::vector<StdString>& names) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_GetIndexedProperty(const V8ObjectHandle& handle, int32_t index, V8Value& value) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_SetIndexedProperty(const V8ObjectHandle& handle, int32_t index, const V8Value& value) noexcept;
NATIVE_ENTRY_POINT(StdBool) V8Object_DeleteIndexedProperty(const V8ObjectHandle& handle, int32_t index) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_GetPropertyIndices(const V8ObjectHandle& handle, std::vector<int32_t>& indices) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_Invoke(const V8ObjectHandle& handle, StdBool asConstructor, const std::vector<V8Value>& args, V8Value& result) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_InvokeMethod(const V8ObjectHandle& handle, const StdString& name, const std::vector<V8Value>& args, V8Value& result) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_GetArrayBufferOrViewInfo(const V8ObjectHandle& handle, V8Value& arrayBuffer, uint64_t& offset, uint64_t& size, uint64_t& length) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_InvokeWithArrayBufferOrViewData(const V8ObjectHandle& handle, void* pvAction) noexcept;
NATIVE_ENTRY_POINT(void) V8Object_InvokeWithArrayBufferOrViewDataWithArg(const V8ObjectHandle& handle, void* pvAction, void* pvArg) noexcept;

NATIVE_ENTRY_POINT(void) V8DebugCallback_ConnectClient(const V8DebugCallbackHandle& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8DebugCallback_SendCommand(const V8DebugCallbackHandle& handle, const StdString& command) noexcept;
NATIVE_ENTRY_POINT(void) V8DebugCallback_DisconnectClient(const V8DebugCallbackHandle& handle) noexcept;

NATIVE_ENTRY_POINT(void) NativeCallback_Invoke(const NativeCallbackHandle& handle) noexcept;

NATIVE_ENTRY_POINT(void) V8Entity_Release(V8EntityHandleBase& handle) noexcept;
NATIVE_ENTRY_POINT(V8EntityHandleBase*) V8Entity_CloneHandle(V8EntityHandleBase& handle) noexcept;
NATIVE_ENTRY_POINT(void) V8Entity_DestroyHandle(V8EntityHandleBase* pHandle) noexcept;

NATIVE_ENTRY_POINT(void) HostException_Schedule(StdString&& message, V8Value&& exception) noexcept;

NATIVE_ENTRY_POINT(size_t) V8UnitTestSupport_GetTextDigest(const StdString& value) noexcept;
NATIVE_ENTRY_POINT(void) V8UnitTestSupport_GetStatistics(uint64_t& isolateCount, uint64_t& contextCount) noexcept;
