// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// forward declarations
//-----------------------------------------------------------------------------

struct V8DocumentInfo;

//-----------------------------------------------------------------------------
// IHostObjectUtil
//-----------------------------------------------------------------------------

struct IHostObjectUtil
{
    virtual void* AddRef(void* pvObject) = 0;
    virtual void Release(void* pvObject) = 0;

    virtual V8Value GetProperty(void* pvObject, const StdString& name) = 0;
    virtual V8Value GetProperty(void* pvObject, const StdString& name, bool& isCacheable) = 0;
    virtual void SetProperty(void* pvObject, const StdString& name, const V8Value& value) = 0;
    virtual bool DeleteProperty(void* pvObject, const StdString& name) = 0;
    virtual void GetPropertyNames(void* pvObject, std::vector<StdString>& names) = 0;

    virtual V8Value GetProperty(void* pvObject, int32_t index) = 0;
    virtual void SetProperty(void* pvObject, int32_t index, const V8Value& value) = 0;
    virtual bool DeleteProperty(void* pvObject, int32_t index) = 0;
    virtual void GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices) = 0;

    virtual V8Value Invoke(void* pvObject, bool asConstructor, const std::vector<V8Value>& args) = 0;
    virtual V8Value InvokeMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args) = 0;

    enum class Invocability : int32_t
    {
        // IMPORTANT: maintain bitwise equivalence with managed enum Invocability
        None,
        Delegate,
        Dynamic,
        DefaultProperty
    };

    virtual Invocability GetInvocability(void* pvObject) = 0;

    virtual V8Value GetEnumerator(void* pvObject) = 0;
    virtual V8Value GetAsyncEnumerator(void* pvObject) = 0;

    virtual void* CreateV8ObjectCache() = 0;
    virtual void CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object) = 0;
    virtual void* GetCachedV8Object(void* pvCache, void* pvObject) = 0;
    virtual void GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs) = 0;
    virtual bool RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject) = 0;

    enum class DebugDirective
    {
        ConnectClient,
        SendCommand,
        DisconnectClient
    };

    using DebugCallback = std::function<void(DebugDirective directive, const StdString* pCommand)>;
    virtual void* CreateDebugAgent(const StdString& name, const StdString& version, int32_t port, bool remote, DebugCallback&& callback) = 0;
    virtual void SendDebugMessage(void* pvAgent, const StdString& content) = 0;
    virtual void DestroyDebugAgent(void* pvAgent) = 0;

    using NativeCallback = std::function<void()>;
    virtual void QueueNativeCallback(NativeCallback&& callback) = 0;
    virtual void* CreateNativeCallbackTimer(int32_t dueTime, int32_t period, NativeCallback&& callback) = 0;
    virtual bool ChangeNativeCallbackTimer(void* pvTimer, int32_t dueTime, int32_t period) = 0;
    virtual void DestroyNativeCallbackTimer(void* pvTimer) = 0;

    virtual StdString LoadModule(const V8DocumentInfo& sourceDocumentInfo, const StdString& specifier, V8DocumentInfo& documentInfo, V8Value& exports) = 0;
    virtual std::vector<std::pair<StdString, V8Value>> CreateModuleContext(const V8DocumentInfo& documentInfo) = 0;

    virtual size_t GetMaxScriptCacheSize() = 0;
    virtual size_t GetMaxModuleCacheSize() = 0;
};

//-----------------------------------------------------------------------------
// HostObjectUtil
//-----------------------------------------------------------------------------

struct HostObjectUtil final: StaticBase
{
    static IHostObjectUtil& GetInstance() noexcept;
};
