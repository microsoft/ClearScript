// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// forward declarations
//-----------------------------------------------------------------------------

struct V8DocumentInfo;

//-----------------------------------------------------------------------------
// HostObjectUtil
//-----------------------------------------------------------------------------

struct HostObjectUtil final: StaticBase
{
    static void* AddRef(void* pvObject);
    static void Release(void* pvObject);

    enum class Invocability : int32_t
    {
        // IMPORTANT: maintain bitwise equivalence with managed enum Invocability
        None,
        Delegate,
        Dynamic,
        DefaultProperty
    };

    static Invocability GetInvocability(void* pvObject);

    static V8Value GetProperty(void* pvObject, const StdString& name, bool& isCacheable);
    static void SetProperty(void* pvObject, const StdString& name, const V8Value& value);
    static bool DeleteProperty(void* pvObject, const StdString& name);
    static void GetPropertyNames(void* pvObject, std::vector<StdString>& names);

    static V8Value GetProperty(void* pvObject, int32_t index);
    static void SetProperty(void* pvObject, int32_t index, const V8Value& value);
    static bool DeleteProperty(void* pvObject, int32_t index);
    static void GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices);

    static V8Value Invoke(void* pvObject, bool asConstructor, size_t argCount, const V8Value* pArgs);
    static V8Value InvokeMethod(void* pvObject, const StdString& name, size_t argCount, const V8Value* pArgs);

    static V8Value GetEnumerator(void* pvObject);
    static V8Value GetAsyncEnumerator(void* pvObject);

    static void* CreateV8ObjectCache();
    static void CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object);
    static void* GetCachedV8Object(void* pvCache, void* pvObject);
    static void GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs);
    static bool RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject);

    enum class DebugDirective
    {
        ConnectClient,
        SendCommand,
        DisconnectClient
    };

    using DebugCallback = std::function<void(DebugDirective directive, const StdString* pCommand)>;
    static void* CreateDebugAgent(const StdString& name, const StdString& version, int32_t port, bool remote, DebugCallback&& callback);
    static void SendDebugMessage(void* pvAgent, const StdString& content);
    static void DestroyDebugAgent(void* pvAgent);

    using NativeCallback = std::function<void()>;
    static void QueueNativeCallback(NativeCallback&& callback);
    static void* CreateNativeCallbackTimer(int32_t dueTime, int32_t period, NativeCallback&& callback);
    static bool ChangeNativeCallbackTimer(void* pvTimer, int32_t dueTime, int32_t period);
    static void DestroyNativeCallbackTimer(void* pvTimer);

    static StdString LoadModule(const V8DocumentInfo& sourceDocumentInfo, const StdString& specifier, V8DocumentInfo& documentInfo, V8Value& exports);
    static std::vector<std::pair<StdString, V8Value>> CreateModuleContext(const V8DocumentInfo& documentInfo);

    static size_t GetMaxScriptCacheSize();
    static size_t GetMaxModuleCacheSize();
};

//-----------------------------------------------------------------------------
// FastHostObjectUtil
//-----------------------------------------------------------------------------

struct FastHostObjectUtil final: StaticBase
{
    enum class PropertyFlags : int32_t
    {
        // IMPORTANT: maintain bitwise equivalence with managed enum V8.FastProxy.V8FastHostPropertyFlags
        None = 0,
        Available = 0x00000001,
        Cacheable = 0x00000002,
        Enumerable = 0x00000004,
        Writable = 0x00000008,
        Deletable = 0x00000010
    };

    static V8Value GetProperty(void* pvObject, const StdString& name, bool& isCacheable);
    static void SetProperty(void* pvObject, const StdString& name, const V8Value& value);
    static PropertyFlags QueryProperty(void* pvObject, const StdString& name);
    static bool DeleteProperty(void* pvObject, const StdString& name);
    static void GetPropertyNames(void* pvObject, std::vector<StdString>& names);

    static V8Value GetProperty(void* pvObject, int32_t index);
    static void SetProperty(void* pvObject, int32_t index, const V8Value& value);
    static PropertyFlags QueryProperty(void* pvObject, int32_t index);
    static bool DeleteProperty(void* pvObject, int32_t index);
    static void GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices);

    static V8Value Invoke(void* pvObject, bool asConstructor, size_t argCount, const V8Value* pArgs);

    static V8Value GetEnumerator(void* pvObject);
    static V8Value GetAsyncEnumerator(void* pvObject);
};
