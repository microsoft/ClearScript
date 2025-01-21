// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8 split proxy managed method list
//-----------------------------------------------------------------------------

    //-------------------------------------------------------------------------------
    // IMPORTANT: maintain synchronization with V8SplitProxyManaged.CreateMethodTable
    //-------------------------------------------------------------------------------

#define V8_SPLIT_PROXY_MANAGED_METHOD_LIST \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ScheduleForwardingException, const V8Value& exception) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ScheduleInvalidOperationException, const StdString& message) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ScheduleScriptEngineException, const StdString& engineName, const StdString& message, const StdString& stackTrace, StdBool isFatal, StdBool executionStarted, const V8Value& scriptException, const V8Value& innerException) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ScheduleScriptInterruptedException, const StdString& engineName, const StdString& message, const StdString& stackTrace, StdBool isFatal, StdBool executionStarted, const V8Value& scriptException, const V8Value& innerException) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, InvokeHostAction, void* pvAction) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ProcessArrayBufferOrViewData, void* pvData, void* pvAction) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ProcessCpuProfile, const v8::CpuProfile& profile, void* pvAction) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void*, CreateV8ObjectCache) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, CacheV8Object, void* pvCache, void* pvObject, void* pvV8Object) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void*, GetCachedV8Object, void* pvCache, void* pvObject) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetAllCachedV8Objects, void* pvCache, std::vector<void*>& v8ObjectPtrs) \
    V8_SPLIT_PROXY_MANAGED_METHOD(StdBool, RemoveV8ObjectCacheEntry, void* pvCache, void* pvObject) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void*, CreateDebugAgent, const StdString& name, const StdString& version, int32_t port, StdBool remote, V8DebugCallbackHandle* pCallbackHandle) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, SendDebugMessage, void* pvAgent, const StdString& content) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, DestroyDebugAgent, void* pvAgent) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(uint32_t, GetMaxScriptCacheSize) \
    V8_SPLIT_PROXY_MANAGED_METHOD(uint32_t, GetMaxModuleCacheSize) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void*, AddRefHostObject, void* pvObject) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, ReleaseHostObject, void* pvObject) \
    V8_SPLIT_PROXY_MANAGED_METHOD(IHostObjectUtil::Invocability, GetHostObjectInvocability, void* pvObject) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectNamedProperty, void* pvObject, const StdString& name, V8Value& value) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectNamedPropertyWithCacheability, void* pvObject, const StdString& name, V8Value& value, StdBool& isCacheable) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, SetHostObjectNamedProperty, void* pvObject, const StdString& name, const V8Value::Decoded& value) \
    V8_SPLIT_PROXY_MANAGED_METHOD(StdBool, DeleteHostObjectNamedProperty, void* pvObject, const StdString& name) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectPropertyNames, void* pvObject, std::vector<StdString>& names) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectIndexedProperty, void* pvObject, int32_t index, V8Value& value) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, SetHostObjectIndexedProperty, void* pvObject, int32_t index, const V8Value::Decoded& value) \
    V8_SPLIT_PROXY_MANAGED_METHOD(StdBool, DeleteHostObjectIndexedProperty, void* pvObject, int32_t index) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectPropertyIndices, void* pvObject, std::vector<int32_t>& indices) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, InvokeHostObject, void* pvObject, StdBool asConstructor, int32_t argCount, const V8Value::Decoded* pArgs, V8Value& result) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, InvokeHostObjectMethod, void* pvObject, const StdString& name, int32_t argCount, const V8Value::Decoded* pArgs, V8Value& result) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectEnumerator, void* pvObject, V8Value& result) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, GetHostObjectAsyncEnumerator, void* pvObject, V8Value& result) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, QueueNativeCallback, NativeCallbackHandle* pCallbackHandle) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void*, CreateNativeCallbackTimer, int32_t dueTime, int32_t period, NativeCallbackHandle* pCallbackHandle) \
    V8_SPLIT_PROXY_MANAGED_METHOD(StdBool, ChangeNativeCallbackTimer, void* pvTimer, int32_t dueTime, int32_t period) \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, DestroyNativeCallbackTimer, void* pvTimer) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, LoadModule, void* pvSourceDocumentInfo, const StdString& specifier, StdString& resourceName, StdString& sourceMapUrl, uint64_t& uniqueId, DocumentKind& documentKind, StdString& code, void*& pvDocumentInfo, V8Value& exports) \
    V8_SPLIT_PROXY_MANAGED_METHOD(int32_t, CreateModuleContext, void* pvDocumentInfo, std::vector<StdString>& names, std::vector<V8Value>& values) \
    \
    V8_SPLIT_PROXY_MANAGED_METHOD(void, WriteBytesToStream, void* pvStream, const uint8_t* pBytes, int32_t count) \
    V8_SPLIT_PROXY_MANAGED_METHOD(V8GlobalFlags, GetGlobalFlags)

//-----------------------------------------------------------------------------
// V8SplitProxyManaged
//-----------------------------------------------------------------------------

class V8SplitProxyManaged final: StaticBase
{
public:

    enum class MethodSlot
    {
        #define V8_SPLIT_PROXY_MANAGED_METHOD(TYPE, NAME, ...) NAME,
            V8_SPLIT_PROXY_MANAGED_METHOD_LIST
        #undef V8_SPLIT_PROXY_MANAGED_METHOD
    };

    struct Method: StaticBase
    {
        #define V8_SPLIT_PROXY_MANAGED_METHOD(TYPE, NAME, ...) typedef MANAGED_METHOD(TYPE) NAME(__VA_ARGS__);
            V8_SPLIT_PROXY_MANAGED_METHOD_LIST
        #undef V8_SPLIT_PROXY_MANAGED_METHOD
    };

    static void** SetMethodTable(void** pMethodTable) noexcept;
    static void SetHostException(HostException&& exception) noexcept;

    template <typename T>
    static T Invoke(const std::function<T(void** pMethodTable)>& action)
    {
        auto pMethodTable = ms_pMethodTable;
        _ASSERTE(pMethodTable != nullptr);

        BEGIN_PULSE_VALUE_SCOPE(&ms_pHostException, nullptr)
            auto result = action(pMethodTable);
            ThrowHostException();
            return result;
        END_PULSE_VALUE_SCOPE
    }

    static void InvokeVoid(const std::function<void(void** pMethodTable)>& action)
    {
        auto pMethodTable = ms_pMethodTable;
        _ASSERTE(pMethodTable != nullptr);

        BEGIN_PULSE_VALUE_SCOPE(&ms_pHostException, nullptr)
            action(pMethodTable);
            ThrowHostException();
        END_PULSE_VALUE_SCOPE
    }

    template <typename T>
    static T InvokeNoThrow(const std::function<T(void** pMethodTable)>& action) noexcept
    {
        auto pMethodTable = ms_pMethodTable;
        _ASSERTE(pMethodTable != nullptr);
        return action(pMethodTable);
    }

    static void InvokeVoidNoThrow(const std::function<void(void** pMethodTable)>& action) noexcept
    {
        auto pMethodTable = ms_pMethodTable;
        _ASSERTE(pMethodTable != nullptr);
        action(pMethodTable);
    }

private:

    static void ThrowHostException();

    static thread_local void** ms_pMethodTable;
    static thread_local HostException* ms_pHostException;
};

//-----------------------------------------------------------------------------

#define V8_SPLIT_PROXY_MANAGED_CALL(TABLE, NAME, ...) \
    reinterpret_cast<V8SplitProxyManaged::Method::NAME*>((TABLE)[static_cast<std::underlying_type_t<V8SplitProxyManaged::MethodSlot>>(V8SplitProxyManaged::MethodSlot::NAME)])(__VA_ARGS__)

#define V8_SPLIT_PROXY_MANAGED_INVOKE(TYPE, NAME, ...) \
    V8SplitProxyManaged::Invoke<TYPE>([&] (void** pMethodTable) noexcept { return V8_SPLIT_PROXY_MANAGED_CALL(pMethodTable, NAME, __VA_ARGS__); })

#define V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(NAME, ...) \
    V8SplitProxyManaged::InvokeVoid([&] (void** pMethodTable) noexcept { V8_SPLIT_PROXY_MANAGED_CALL(pMethodTable, NAME, __VA_ARGS__); })

#define V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(TYPE, NAME, ...) \
    V8SplitProxyManaged::InvokeNoThrow<TYPE>([&] (void** pMethodTable) noexcept { return V8_SPLIT_PROXY_MANAGED_CALL(pMethodTable, NAME, __VA_ARGS__); })

#define V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(NAME, ...) \
    V8SplitProxyManaged::InvokeVoidNoThrow([&] (void** pMethodTable) noexcept { V8_SPLIT_PROXY_MANAGED_CALL(pMethodTable, NAME, __VA_ARGS__); })
