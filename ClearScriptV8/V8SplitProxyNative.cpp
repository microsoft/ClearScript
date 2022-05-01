// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

static size_t AdjustConstraint(int value) noexcept
{
    value = std::max(value, 0);
    size_t result = value;

    const int maxValueInMiB = 1024 * 1024;
    if (value <= maxValueInMiB)
    {
        const size_t bytesPerMiB = 1024 * 1024;
        result *= bytesPerMiB;
    }

    return result;
}

//-------------------------------------------------------------------------

static void InvokeHostAction(void* pvAction) noexcept
{
    try
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostAction, pvAction);
    }
    catch (const HostException& exception)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleForwardingException, exception.GetException());
    }
}

//-------------------------------------------------------------------------

static void ProcessArrayBufferOrViewData(void* pvData, void* pvAction)
{
    try
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ProcessArrayBufferOrViewData, pvData, pvAction);
    }
    catch (const HostException& exception)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleForwardingException, exception.GetException());
    }
}

//-----------------------------------------------------------------------------

static void ProcessCpuProfile(const v8::CpuProfile& profile, void* pvAction)
{
    try
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ProcessCpuProfile, profile, pvAction);
    }
    catch (const HostException& exception)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleForwardingException, exception.GetException());
    }
}

//-----------------------------------------------------------------------------
// V8Exception implementation
//-----------------------------------------------------------------------------

void V8Exception::ScheduleScriptEngineException() const noexcept
{
    switch (m_Type)
    {
        case Type::General: default:
            V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleScriptEngineException, m_EngineName, m_Message, m_StackTrace, false, m_ExecutionStarted, m_ScriptException, m_InnerException);
            break;

        case Type::Fatal:
            V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleScriptEngineException, m_EngineName, m_Message, m_StackTrace, true, m_ExecutionStarted, m_ScriptException, m_InnerException);
            break;

        case Type::Interrupt:
            V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleScriptInterruptedException, m_EngineName, m_Message, m_StackTrace, false, m_ExecutionStarted, m_ScriptException, m_InnerException);
            break;
    }
}

//-----------------------------------------------------------------------------
// V8EntityHandleBase (implementation)
//-----------------------------------------------------------------------------

StdString V8EntityHandleBase::GetEntityReleasedMessage(const StdChar* pName) noexcept
{
    StdString message(SL("The "));
    message += pName;
    message += SL(" has been released");
    return message;
}

//-----------------------------------------------------------------------------

void V8EntityHandleBase::ScheduleInvalidOperationException(const StdString& message) noexcept
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleInvalidOperationException, message);
}

//-----------------------------------------------------------------------------
// V8 split proxy native entry points (implementation)
//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void**) V8SplitProxyManaged_SetMethodTable(void** pMethodTable) noexcept
{
    return V8SplitProxyManaged::SetMethodTable(pMethodTable);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Environment_InitializeICU(const StdChar* pDataPath) noexcept
{
    StdString dataPath(pDataPath);
    ASSERT_EVAL(v8::V8::InitializeICU(dataPath.ToUTF8().c_str()));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdString*) StdString_New(const StdChar* pValue, int32_t length) noexcept
{
    return new StdString(pValue, length);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(const StdChar*) StdString_GetValue(const StdString& string, int32_t& length) noexcept
{
    length = string.GetLength();
    return string.ToCString();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdString_SetValue(StdString& string, const StdChar* pValue, int32_t length) noexcept
{
    string = StdString(pValue, length);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdString_Delete(StdString* pString) noexcept
{
    delete pString;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<StdString>*) StdStringArray_New(int32_t elementCount) noexcept
{
    return new std::vector<StdString>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdStringArray_GetElementCount(const std::vector<StdString>& stringArray) noexcept
{
    return static_cast<int32_t>(stringArray.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdStringArray_SetElementCount(std::vector<StdString>& stringArray, int32_t elementCount) noexcept
{
    stringArray.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(const StdChar*) StdStringArray_GetElement(const std::vector<StdString>& stringArray, int32_t index, int32_t& length) noexcept
{
    return StdString_GetValue(stringArray[index], length);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdStringArray_SetElement(std::vector<StdString>& stringArray, int32_t index, const StdChar* pValue, int32_t length) noexcept
{
    stringArray[index] = StdString(pValue, length);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdStringArray_Delete(std::vector<StdString>* pStringArray) noexcept
{
    delete pStringArray;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<uint8_t>*) StdByteArray_New(int32_t elementCount) noexcept
{
    return new std::vector<uint8_t>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdByteArray_GetElementCount(const std::vector<uint8_t>& byteArray) noexcept
{
    return static_cast<int32_t>(byteArray.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdByteArray_SetElementCount(std::vector<uint8_t>& byteArray, int32_t elementCount) noexcept
{
    byteArray.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(uint8_t*) StdByteArray_GetData(std::vector<uint8_t>& byteArray) noexcept
{
    return byteArray.data();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdByteArray_Delete(std::vector<uint8_t>* pByteArray) noexcept
{
    delete pByteArray;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<int32_t>*) StdInt32Array_New(int32_t elementCount) noexcept
{
    return new std::vector<int32_t>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdInt32Array_GetElementCount(const std::vector<int32_t>& int32Array) noexcept
{
    return static_cast<int32_t>(int32Array.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdInt32Array_SetElementCount(std::vector<int32_t>& int32Array, int32_t elementCount) noexcept
{
    int32Array.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t*) StdInt32Array_GetData(std::vector<int32_t>& int32Array) noexcept
{
    return int32Array.data();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdInt32Array_Delete(std::vector<int32_t>* pInt32Array) noexcept
{
    delete pInt32Array;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<uint32_t>*) StdUInt32Array_New(int32_t elementCount) noexcept
{
    return new std::vector<uint32_t>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdUInt32Array_GetElementCount(const std::vector<uint32_t>& uint32Array) noexcept
{
    return static_cast<int32_t>(uint32Array.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdUInt32Array_SetElementCount(std::vector<uint32_t>& uint32Array, int32_t elementCount) noexcept
{
    uint32Array.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(uint32_t*) StdUInt32Array_GetData(std::vector<uint32_t>& uint32Array) noexcept
{
    return uint32Array.data();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdUInt32Array_Delete(std::vector<uint32_t>* pUInt32Array) noexcept
{
    delete pUInt32Array;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<uint64_t>*) StdUInt64Array_New(int32_t elementCount) noexcept
{
    return new std::vector<uint64_t>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdUInt64Array_GetElementCount(const std::vector<uint64_t>& uint64Array) noexcept
{
    return static_cast<int32_t>(uint64Array.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdUInt64Array_SetElementCount(std::vector<uint64_t>& uint64Array, int32_t elementCount) noexcept
{
    uint64Array.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(uint64_t*) StdUInt64Array_GetData(std::vector<uint64_t>& uint64Array) noexcept
{
    return uint64Array.data();
}

//-----------------------------------------------------------------------------


NATIVE_ENTRY_POINT(void) StdUInt64Array_Delete(std::vector<uint64_t>* pUInt64Array) noexcept
{
    delete pUInt64Array;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<void*>*) StdPtrArray_New(int32_t elementCount) noexcept
{
    return new std::vector<void*>(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdPtrArray_GetElementCount(const std::vector<void*>& ptrArray) noexcept
{
    return static_cast<int32_t>(ptrArray.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdPtrArray_SetElementCount(std::vector<void*>& ptrArray, int32_t elementCount) noexcept
{
    ptrArray.resize(elementCount);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void**) StdPtrArray_GetData(std::vector<void*>& ptrArray) noexcept
{
    return ptrArray.data();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdPtrArray_Delete(std::vector<void*>* pPtrArray) noexcept
{
    delete pPtrArray;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(std::vector<V8Value>*) StdV8ValueArray_New(int32_t elementCount) noexcept
{
    return new std::vector<V8Value>(elementCount, V8Value(V8Value::Nonexistent));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(int32_t) StdV8ValueArray_GetElementCount(const std::vector<V8Value>& v8ValueArray) noexcept
{
    return static_cast<int32_t>(v8ValueArray.size());
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdV8ValueArray_SetElementCount(std::vector<V8Value>& v8ValueArray, int32_t elementCount) noexcept
{
    v8ValueArray.resize(elementCount, V8Value(V8Value::Nonexistent));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8Value*) StdV8ValueArray_GetData(std::vector<V8Value>& v8ValueArray) noexcept
{
    return v8ValueArray.data();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) StdV8ValueArray_Delete(std::vector<V8Value>* pV8ValueArray) noexcept
{
    delete pV8ValueArray;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8Value*) V8Value_New() noexcept
{
    return new V8Value(V8Value::Nonexistent);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetNonexistent(V8Value* pV8Value) noexcept
{
    *pV8Value = V8Value(V8Value::Nonexistent);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetUndefined(V8Value* pV8Value) noexcept
{
    *pV8Value = V8Value(V8Value::Undefined);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetNull(V8Value* pV8Value) noexcept
{
    *pV8Value = V8Value(V8Value::Null);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetBoolean(V8Value* pV8Value, StdBool value) noexcept
{
    *pV8Value = V8Value(value != 0);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetNumber(V8Value* pV8Value, double value) noexcept
{
    *pV8Value = V8Value(value);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetInt32(V8Value* pV8Value, int32_t value) noexcept
{
    *pV8Value = V8Value(value);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetUInt32(V8Value* pV8Value, uint32_t value) noexcept
{
    *pV8Value = V8Value(value);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetString(V8Value* pV8Value, const StdChar* pValue, int32_t length) noexcept
{
    *pV8Value = V8Value(new StdString(pValue, length));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetDateTime(V8Value* pV8Value, double value) noexcept
{
    *pV8Value = V8Value(V8Value::DateTime, value);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetBigInt(V8Value* pV8Value, int32_t signBit, const uint8_t* pBytes, int32_t length) noexcept
{
    std::vector<uint64_t> words;
    if (length > 0)
    {
        words.resize((length + sizeof(uint64_t) - 1) / sizeof(uint64_t), 0);
        memcpy(words.data(), pBytes, length);
    }

    *pV8Value = V8Value(new V8BigInt(signBit, std::move(words)));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetV8Object(V8Value* pV8Value, const V8ObjectHandle& handle, V8Value::Subtype subtype, V8Value::Flags flags) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        *pV8Value = V8Value(spV8ObjectHolder->Clone(), subtype, flags);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_SetHostObject(V8Value* pV8Value, void* pvObject) noexcept
{
    *pV8Value = V8Value(new HostObjectHolderImpl(pvObject));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8Value::Type) V8Value_Decode(const V8Value& value, int32_t& intValue, uint32_t& uintValue, double& doubleValue, const void*& pvData) noexcept
{
    {
        bool result;
        if (value.AsBoolean(result))
        {
            intValue = result;
            return V8Value::Type::Boolean;
        }
    }

    {
        double result;
        if (value.AsNumber(result))
        {
            doubleValue = result;
            return V8Value::Type::Number;
        }
    }

    {
        int32_t result;
        if (value.AsInt32(result))
        {
            intValue = result;
            return V8Value::Type::Int32;
        }
    }

    {
        uint32_t result;
        if (value.AsUInt32(result))
        {
            uintValue = result;
            return V8Value::Type::UInt32;
        }
    }

    {
        const StdString* pString;
        if (value.AsString(pString))
        {
            pvData = pString->ToCString();
            intValue = pString->GetLength();
            return V8Value::Type::String;
        }
    }

    {
        double result;
        if (value.AsDateTime(result))
        {
            doubleValue = result;
            return V8Value::Type::DateTime;
        }
    }

    {
        const V8BigInt* pBigInt;
        if (value.AsBigInt(pBigInt))
        {
            intValue = pBigInt->GetSignBit();
            pvData = pBigInt->GetWords().data();
            uintValue = static_cast<uint32_t>(pBigInt->GetWords().size());
            return V8Value::Type::BigInt;
        }
    }

    {
        V8ObjectHolder* pHolder;
        V8Value::Subtype subtype;
        V8Value::Flags flags;
        if (value.AsV8Object(pHolder, subtype, flags))
        {
            pvData = new V8ObjectHandle(pHolder->Clone());
            uintValue = static_cast<std::underlying_type_t<V8Value::Subtype>>(subtype);
            intValue = static_cast<std::underlying_type_t<V8Value::Flags>>(flags);
            return V8Value::Type::V8Object;
        }
    }

    {
        HostObjectHolder* pHolder;
        if (value.AsHostObject(pHolder))
        {
            pvData = pHolder->GetObject();
            return V8Value::Type::HostObject;
        }
    }

    return value.GetType();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Value_Delete(V8Value* pV8Value) noexcept
{
    delete pV8Value;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8CpuProfile_GetInfo(const v8::CpuProfile& profile, const V8EntityHandleBase& entityHandle, StdString& name, uint64_t& startTimestamp, uint64_t& endTimestamp, int32_t& sampleCount, const v8::CpuProfileNode*& pRootNode) noexcept
{
    name = entityHandle.CreateStdString(profile.GetTitle());
    startTimestamp = profile.GetStartTime();
    endTimestamp = profile.GetEndTime();
    sampleCount = profile.GetSamplesCount();
    pRootNode = profile.GetTopDownRoot();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8CpuProfile_GetSample(const v8::CpuProfile& profile, int32_t index, uint64_t& nodeId, uint64_t& timestamp) noexcept
{
    auto pNode = profile.GetSample(index);
    if (pNode != nullptr)
    {
        nodeId = pNode->GetNodeId();
        timestamp = profile.GetSampleTimestamp(index);
        return true;
    }

    return false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8CpuProfileNode_GetInfo(const v8::CpuProfileNode& node, const V8EntityHandleBase& entityHandle, uint64_t& nodeId, int64_t& scriptId, StdString& scriptName, StdString& functionName, StdString& bailoutReason, int64_t& lineNumber, int64_t& columnNumber, uint64_t& hitCount, uint32_t& hitLineCount, int32_t& childCount)
{
    nodeId = node.GetNodeId();
    scriptId = node.GetScriptId();
    scriptName = entityHandle.CreateStdString(node.GetScriptResourceName());
    functionName = entityHandle.CreateStdString(node.GetFunctionName());
    bailoutReason = StdString(node.GetBailoutReason());
    lineNumber = node.GetLineNumber();
    columnNumber = node.GetColumnNumber();
    hitCount = node.GetHitCount();
    hitLineCount = node.GetHitLineCount();
    childCount = node.GetChildrenCount();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8CpuProfileNode_GetHitLines(const v8::CpuProfileNode& node, std::vector<int32_t>& lineNumbers, std::vector<uint32_t>& hitCounts) noexcept
{
    auto hitLineCount = node.GetHitLineCount();
    if (hitLineCount > 0)
    {
        std::vector<v8::CpuProfileNode::LineTick> hitLines(hitLineCount);
        if (node.GetLineTicks(hitLines.data(), static_cast<unsigned>(hitLines.size())))
        {
            lineNumbers.resize(hitLineCount);
            hitCounts.resize(hitLineCount);

            for (auto index = 0U; index < hitLineCount; index++)
            {
                lineNumbers[index] = hitLines[index].line;
                hitCounts[index] = hitLines[index].hit_count;
            }

            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(const v8::CpuProfileNode*) V8CpuProfileNode_GetChildNode(const v8::CpuProfileNode& node, int32_t index) noexcept
{
    return node.GetChild(index);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8IsolateHandle*) V8Isolate_Create(const StdString& name, int32_t maxNewSpaceSize, int32_t maxOldSpaceSize, double heapExpansionMultiplier, uint64_t maxArrayBufferAllocation, StdBool enableDebugging, StdBool enableRemoteDebugging, StdBool enableDynamicModuleImports, int32_t debugPort) noexcept
{
    v8::ResourceConstraints* pConstraints = nullptr;

    v8::ResourceConstraints constraints;
    if ((maxNewSpaceSize >= 0) && (maxOldSpaceSize >= 0))
    {
        constraints.set_max_young_generation_size_in_bytes(AdjustConstraint(maxNewSpaceSize));
        constraints.set_max_old_generation_size_in_bytes(AdjustConstraint(maxOldSpaceSize));
        pConstraints = &constraints;
    }

    V8Isolate::Options options;
    options.HeapExpansionMultiplier = heapExpansionMultiplier;
    options.EnableDebugging = enableDebugging;
    options.EnableRemoteDebugging = enableRemoteDebugging;
    options.EnableDynamicModuleImports = enableDynamicModuleImports;
    options.DebugPort = debugPort;

    if (maxArrayBufferAllocation < SIZE_MAX)
    {
        options.MaxArrayBufferAllocation = static_cast<size_t>(maxArrayBufferAllocation);
    }

    try
    {
        return new V8IsolateHandle(V8Isolate::Create(name, pConstraints, options));
    }
    catch (const V8Exception& exception)
    {
        exception.ScheduleScriptEngineException();
        return nullptr;
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ContextHandle*) V8Isolate_CreateContext(const V8IsolateHandle& handle, const StdString& name, StdBool enableDebugging, StdBool enableRemoteDebugging, StdBool disableGlobalMembers, StdBool enableDateTimeConversion, StdBool enableDynamicModuleImports, int32_t debugPort) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        V8Context::Options options;
        options.EnableDebugging = enableDebugging;
        options.EnableRemoteDebugging = enableRemoteDebugging;
        options.DisableGlobalMembers = disableGlobalMembers;
        options.EnableDateTimeConversion = enableDateTimeConversion;
        options.EnableDynamicModuleImports = enableDynamicModuleImports;
        options.DebugPort = debugPort;

        try
        {
            return new V8ContextHandle(V8Context::Create(spIsolate, name, options));
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(size_t) V8Isolate_GetMaxHeapSize(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetMaxHeapSize() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetMaxHeapSize(const V8IsolateHandle& handle, size_t size) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetMaxHeapSize(size);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(double) V8Isolate_GetHeapSizeSampleInterval(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetHeapSizeSampleInterval() : 0.0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetHeapSizeSampleInterval(const V8IsolateHandle& handle, double milliseconds) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetHeapSizeSampleInterval(milliseconds);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(size_t) V8Isolate_GetMaxStackUsage(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetMaxStackUsage() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetMaxStackUsage(const V8IsolateHandle& handle, size_t size) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetMaxStackUsage(size);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_AwaitDebuggerAndPause(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        try
        {
            spIsolate->AwaitDebuggerAndPause();
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_CancelAwaitDebugger(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->CancelAwaitDebugger();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_Compile(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code) noexcept
{
    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        try
        {
            return new V8ScriptHandle(spIsolate->Compile(documentInfo, std::move(code)));
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_CompileProducingCache(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) noexcept
{
    cacheBytes.clear();

    if (cacheType == V8CacheType::None)
    {
        return V8Isolate_Compile(handle, std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo, std::move(code));
    }

    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        try
        {
            return new V8ScriptHandle(spIsolate->Compile(documentInfo, std::move(code), cacheType, cacheBytes));
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Isolate_CompileConsumingCache(const V8IsolateHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, StdBool& cacheAccepted) noexcept
{
    cacheAccepted = false;

    if ((cacheType == V8CacheType::None) || cacheBytes.empty())
    {
        return V8Isolate_Compile(handle, std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo, std::move(code));
    }

    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        try
        {
            auto tempCacheAccepted = false;
            auto pScriptHandle = new V8ScriptHandle(spIsolate->Compile(documentInfo, std::move(code), cacheType, cacheBytes, tempCacheAccepted));

            cacheAccepted = tempCacheAccepted;
            return pScriptHandle;
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Isolate_GetEnableInterruptPropagation(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetEnableInterruptPropagation() : false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetEnableInterruptPropagation(const V8IsolateHandle& handle, StdBool value) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetEnableInterruptPropagation(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Isolate_GetDisableHeapSizeViolationInterrupt(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetDisableHeapSizeViolationInterrupt() : false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetDisableHeapSizeViolationInterrupt(const V8IsolateHandle& handle, StdBool value) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetDisableHeapSizeViolationInterrupt(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_GetHeapStatistics(const V8IsolateHandle& handle, uint64_t& totalHeapSize, uint64_t& totalHeapSizeExecutable, uint64_t& totalPhysicalSize, uint64_t& usedHeapSize, uint64_t& heapSizeLimit) noexcept
{
    totalHeapSize = 0UL;
    totalHeapSizeExecutable = 0UL;
    totalPhysicalSize = 0UL;
    usedHeapSize = 0UL;
    heapSizeLimit = 0UL;

    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        v8::HeapStatistics statistics;
        spIsolate->GetHeapStatistics(statistics);

        totalHeapSize = statistics.total_heap_size();
        totalHeapSizeExecutable = statistics.total_heap_size_executable();
        totalPhysicalSize = statistics.total_physical_size();
        usedHeapSize = statistics.used_heap_size();
        heapSizeLimit = statistics.heap_size_limit();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_GetStatistics(const V8IsolateHandle& handle, uint64_t& scriptCount, uint64_t& scriptCacheSize, uint64_t& moduleCount, std::vector<uint64_t>& postedTaskCounts, std::vector<uint64_t>& invokedTaskCounts) noexcept
{
    scriptCount = 0UL;
    scriptCacheSize = 0UL;
    moduleCount = 0UL;
    postedTaskCounts.clear();
    invokedTaskCounts.clear();

    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        auto statistics = spIsolate->GetStatistics();

        scriptCount = statistics.ScriptCount;
        scriptCacheSize = statistics.ScriptCacheSize;
        moduleCount = statistics.ModuleCount;

        auto count = statistics.PostedTaskCounts.size();
        postedTaskCounts.reserve(count);
        std::copy(statistics.PostedTaskCounts.cbegin(), statistics.PostedTaskCounts.cend(), std::back_inserter(postedTaskCounts));

        count = statistics.InvokedTaskCounts.size();
        invokedTaskCounts.reserve(count);
        std::copy(statistics.InvokedTaskCounts.cbegin(), statistics.InvokedTaskCounts.cend(), std::back_inserter(invokedTaskCounts));
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_CollectGarbage(const V8IsolateHandle& handle, StdBool exhaustive) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->CollectGarbage(exhaustive);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Isolate_BeginCpuProfile(const V8IsolateHandle& handle, const StdString& name, StdBool recordSamples) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() && spIsolate->BeginCpuProfile(name, v8::kLeafNodeLineNumbers, recordSamples);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_EndCpuProfile(const V8IsolateHandle& handle, const StdString& name, void* pvAction) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->EndCpuProfile(name, ProcessCpuProfile, pvAction);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_CollectCpuProfileSample(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->CollectCpuProfileSample();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(uint32_t) V8Isolate_GetCpuProfileSampleInterval(const V8IsolateHandle& handle) noexcept
{
    auto spIsolate = handle.GetEntity();
    return !spIsolate.IsEmpty() ? spIsolate->GetCpuProfileSampleInterval() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_SetCpuProfileSampleInterval(const V8IsolateHandle& handle, uint32_t value) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->SetCpuProfileSampleInterval(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Isolate_WriteHeapSnapshot(const V8IsolateHandle& handle, void* pvStream) noexcept
{
    auto spIsolate = handle.GetEntity();
    if (!spIsolate.IsEmpty())
    {
        spIsolate->WriteHeapSnapshot(pvStream);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(size_t) V8Context_GetMaxIsolateHeapSize(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetMaxIsolateHeapSize() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetMaxIsolateHeapSize(const V8ContextHandle& handle, size_t size) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetMaxIsolateHeapSize(size);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(double) V8Context_GetIsolateHeapSizeSampleInterval(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetIsolateHeapSizeSampleInterval() : 0.0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetIsolateHeapSizeSampleInterval(const V8ContextHandle& handle, double milliseconds) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetIsolateHeapSizeSampleInterval(milliseconds);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(size_t) V8Context_GetMaxIsolateStackUsage(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetMaxIsolateStackUsage() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetMaxIsolateStackUsage(const V8ContextHandle& handle, size_t size) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetMaxIsolateStackUsage(size);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_InvokeWithLock(const V8ContextHandle& handle, void* pvAction) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            spContext->CallWithLock(InvokeHostAction, pvAction);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_GetRootItem(const V8ContextHandle& handle, V8Value& item) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            item = spContext->GetRootObject();
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_AddGlobalItem(const V8ContextHandle& handle, const StdString& name, const V8Value& value, StdBool globalMembers) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            spContext->SetGlobalProperty(name, value, globalMembers);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_AwaitDebuggerAndPause(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            spContext->AwaitDebuggerAndPause();
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_CancelAwaitDebugger(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->CancelAwaitDebugger();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_ExecuteCode(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, const StdString& code, StdBool evaluate, V8Value& result) noexcept
{
    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            result = spContext->Execute(documentInfo, code, evaluate);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_Compile(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code) noexcept
{
    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            return new V8ScriptHandle(spContext->Compile(documentInfo, std::move(code)));
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_CompileProducingCache(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) noexcept
{
    cacheBytes.clear();

    if (cacheType == V8CacheType::None)
    {
        return V8Context_Compile(handle, std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo, std::move(code));
    }

    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            return new V8ScriptHandle(spContext->Compile(documentInfo, std::move(code), cacheType, cacheBytes));
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(V8ScriptHandle*) V8Context_CompileConsumingCache(const V8ContextHandle& handle, StdString&& resourceName, StdString&& sourceMapUrl, uint64_t uniqueId, StdBool isModule, void* pvDocumentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, StdBool& cacheAccepted) noexcept
{
    cacheAccepted = false;

    if ((cacheType == V8CacheType::None) || cacheBytes.empty())
    {
        return V8Context_Compile(handle, std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo, std::move(code));
    }

    V8DocumentInfo documentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        try
        {
            auto tempCacheAccepted = false;
            auto pScriptHandle = new V8ScriptHandle(spContext->Compile(documentInfo, std::move(code), cacheType, cacheBytes, tempCacheAccepted));

            cacheAccepted = tempCacheAccepted;
            return pScriptHandle;
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_ExecuteScript(const V8ContextHandle& handle, const V8ScriptHandle& scriptHandle, StdBool evaluate, V8Value& result) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        auto spScriptHolder = scriptHandle.GetEntity();
        if (!spScriptHolder.IsEmpty())
        {
            try
            {
                result = spContext->Execute(spScriptHolder, evaluate);
            }
            catch (const V8Exception& exception)
            {
                exception.ScheduleScriptEngineException();
            }
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_Interrupt(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->Interrupt();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_CancelInterrupt(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->CancelInterrupt();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Context_GetEnableIsolateInterruptPropagation(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetEnableIsolateInterruptPropagation() : false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetEnableIsolateInterruptPropagation(const V8ContextHandle& handle, StdBool value) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetEnableIsolateInterruptPropagation(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Context_GetDisableIsolateHeapSizeViolationInterrupt(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetDisableIsolateHeapSizeViolationInterrupt() : false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetDisableIsolateHeapSizeViolationInterrupt(const V8ContextHandle& handle, StdBool value) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetDisableIsolateHeapSizeViolationInterrupt(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_GetIsolateHeapStatistics(const V8ContextHandle& handle, uint64_t& totalHeapSize, uint64_t& totalHeapSizeExecutable, uint64_t& totalPhysicalSize, uint64_t& usedHeapSize, uint64_t& heapSizeLimit) noexcept
{
    totalHeapSize = 0UL;
    totalHeapSizeExecutable = 0UL;
    totalPhysicalSize = 0UL;
    usedHeapSize = 0UL;
    heapSizeLimit = 0UL;

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        v8::HeapStatistics statistics;
        spContext->GetIsolateHeapStatistics(statistics);

        totalHeapSize = statistics.total_heap_size();
        totalHeapSizeExecutable = statistics.total_heap_size_executable();
        totalPhysicalSize = statistics.total_physical_size();
        usedHeapSize = statistics.used_heap_size();
        heapSizeLimit = statistics.heap_size_limit();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_GetIsolateStatistics(const V8ContextHandle& handle, uint64_t& scriptCount, uint64_t& scriptCacheSize, uint64_t& moduleCount, std::vector<uint64_t>& postedTaskCounts, std::vector<uint64_t>& invokedTaskCounts) noexcept
{
    scriptCount = 0UL;
    scriptCacheSize = 0UL;
    moduleCount = 0UL;
    postedTaskCounts.clear();
    invokedTaskCounts.clear();

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        auto statistics = spContext->GetIsolateStatistics();

        scriptCount = statistics.ScriptCount;
        scriptCacheSize = statistics.ScriptCacheSize;
        moduleCount = statistics.ModuleCount;

        auto count = statistics.PostedTaskCounts.size();
        postedTaskCounts.reserve(count);
        std::copy(statistics.PostedTaskCounts.cbegin(), statistics.PostedTaskCounts.cend(), std::back_inserter(postedTaskCounts));

        count = statistics.InvokedTaskCounts.size();
        invokedTaskCounts.reserve(count);
        std::copy(statistics.InvokedTaskCounts.cbegin(), statistics.InvokedTaskCounts.cend(), std::back_inserter(invokedTaskCounts));
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_GetStatistics(const V8ContextHandle& handle, uint64_t& scriptCount, uint64_t& moduleCount, uint64_t& moduleCacheSize) noexcept
{
    scriptCount = 0UL;
    moduleCount = 0UL;
    moduleCacheSize = 0UL;

    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        auto statistics = spContext->GetStatistics();
        scriptCount = statistics.ScriptCount;
        moduleCount = statistics.ModuleCount;
        moduleCacheSize = statistics.ModuleCacheSize;
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_CollectGarbage(const V8ContextHandle& handle, StdBool exhaustive) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->CollectGarbage(exhaustive);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_OnAccessSettingsChanged(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->OnAccessSettingsChanged();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Context_BeginCpuProfile(const V8ContextHandle& handle, const StdString& name, StdBool recordSamples) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() && spContext->BeginCpuProfile(name, v8::kLeafNodeLineNumbers, recordSamples);
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_EndCpuProfile(const V8ContextHandle& handle, const StdString& name, void* pvAction) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->EndCpuProfile(name, ProcessCpuProfile, pvAction);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_CollectCpuProfileSample(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->CollectCpuProfileSample();
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(uint32_t) V8Context_GetCpuProfileSampleInterval(const V8ContextHandle& handle) noexcept
{
    auto spContext = handle.GetEntity();
    return !spContext.IsEmpty() ? spContext->GetCpuProfileSampleInterval() : 0;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_SetCpuProfileSampleInterval(const V8ContextHandle& handle, uint32_t value) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->SetCpuProfileSampleInterval(value);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Context_WriteIsolateHeapSnapshot(const V8ContextHandle& handle, void* pvStream) noexcept
{
    auto spContext = handle.GetEntity();
    if (!spContext.IsEmpty())
    {
        spContext->WriteIsolateHeapSnapshot(pvStream);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_GetNamedProperty(const V8ObjectHandle& handle, const StdString& name, V8Value& value) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            value = V8ObjectHelpers::GetProperty(spV8ObjectHolder, name);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_SetNamedProperty(const V8ObjectHandle& handle, const StdString& name, const V8Value& value) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            V8ObjectHelpers::SetProperty(spV8ObjectHolder, name, value);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Object_DeleteNamedProperty(const V8ObjectHandle& handle, const StdString& name) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            return V8ObjectHelpers::DeleteProperty(spV8ObjectHolder, name);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_GetPropertyNames(const V8ObjectHandle& handle, std::vector<StdString>& names) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            V8ObjectHelpers::GetPropertyNames(spV8ObjectHolder, names);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_GetIndexedProperty(const V8ObjectHandle& handle, int32_t index, V8Value& value) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            value = V8ObjectHelpers::GetProperty(spV8ObjectHolder, index);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_SetIndexedProperty(const V8ObjectHandle& handle, int32_t index, const V8Value& value) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            V8ObjectHelpers::SetProperty(spV8ObjectHolder, index, value);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(StdBool) V8Object_DeleteIndexedProperty(const V8ObjectHandle& handle, int32_t index) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            return V8ObjectHelpers::DeleteProperty(spV8ObjectHolder, index);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_GetPropertyIndices(const V8ObjectHandle& handle, std::vector<int32_t>& indices) noexcept
{
    indices.clear();

    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            V8ObjectHelpers::GetPropertyIndices(spV8ObjectHolder, indices);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_Invoke(const V8ObjectHandle& handle, StdBool asConstructor, const std::vector<V8Value>& args, V8Value& result) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            result = V8ObjectHelpers::Invoke(spV8ObjectHolder, asConstructor, args);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_InvokeMethod(const V8ObjectHandle& handle, const StdString& name, const std::vector<V8Value>& args, V8Value& result) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            result = V8ObjectHelpers::InvokeMethod(spV8ObjectHolder, name, args);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_GetArrayBufferOrViewInfo(const V8ObjectHandle& handle, V8Value& arrayBuffer, uint64_t& offset, uint64_t& size, uint64_t& length) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            size_t tempOffset = 0;
            size_t tempSize = 0;
            size_t tempLength = 0;
            V8ObjectHelpers::GetArrayBufferOrViewInfo(spV8ObjectHolder, arrayBuffer, tempOffset, tempSize, tempLength);

            offset = tempOffset;
            size = tempSize;
            length = tempLength;
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Object_InvokeWithArrayBufferOrViewData(const V8ObjectHandle& handle, void* pvAction) noexcept
{
    auto spV8ObjectHolder = handle.GetEntity();
    if (!spV8ObjectHolder.IsEmpty())
    {
        try
        {
            V8ObjectHelpers::InvokeWithArrayBufferOrViewData(spV8ObjectHolder, ProcessArrayBufferOrViewData, pvAction);
        }
        catch (const V8Exception& exception)
        {
            exception.ScheduleScriptEngineException();
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8DebugCallback_ConnectClient(const V8DebugCallbackHandle& handle) noexcept
{
    SharedPtr<IHostObjectUtil::DebugCallback> spCallback;
    if (handle.TryGetEntity(spCallback))
    {
        (*spCallback)(IHostObjectUtil::DebugDirective::ConnectClient, nullptr /*pCommand*/);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8DebugCallback_SendCommand(const V8DebugCallbackHandle& handle, const StdString& command) noexcept
{
    SharedPtr<IHostObjectUtil::DebugCallback> spCallback;
    if (handle.TryGetEntity(spCallback))
    {
        (*spCallback)(IHostObjectUtil::DebugDirective::SendCommand, &command);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8DebugCallback_DisconnectClient(const V8DebugCallbackHandle& handle) noexcept
{
    SharedPtr<IHostObjectUtil::DebugCallback> spCallback;
    if (handle.TryGetEntity(spCallback))
    {
        (*spCallback)(IHostObjectUtil::DebugDirective::DisconnectClient, nullptr /*pCommand*/);
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) NativeCallback_Invoke(const NativeCallbackHandle& handle) noexcept
{
    SharedPtr<IHostObjectUtil::NativeCallback> spCallback;
    if (handle.TryGetEntity(spCallback))
    {
        try
        {
            (*spCallback)();
        }
        catch (const std::exception&)
        {
        }
        catch (...)
        {
        }
    }
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Entity_Release(V8EntityHandleBase& handle) noexcept
{
    handle.ReleaseEntity();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8Entity_DestroyHandle(V8EntityHandleBase* pHandle) noexcept
{
    delete pHandle;
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) HostException_Schedule(StdString&& message, V8Value&& exception) noexcept
{
    V8SplitProxyManaged::SetHostException(HostException(std::move(message), std::move(exception)));
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(size_t) V8UnitTestSupport_GetTextDigest(const StdString& value) noexcept
{
    return value.GetDigest();
}

//-----------------------------------------------------------------------------

NATIVE_ENTRY_POINT(void) V8UnitTestSupport_GetStatistics(uint64_t& isolateCount, uint64_t& contextCount) noexcept
{
    isolateCount = V8IsolateImpl::GetInstanceCount();
    contextCount = V8ContextImpl::GetInstanceCount();
}
