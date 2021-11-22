// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// HostObjectUtilNativeImpl
//-----------------------------------------------------------------------------

class HostObjectUtilNativeImpl: public IHostObjectUtil
{
    PROHIBIT_COPY(HostObjectUtilNativeImpl)
    PROHIBIT_HEAP(HostObjectUtilNativeImpl)

public:

    static IHostObjectUtil& GetInstance() noexcept
    {
        return s_Instance;
    }

    virtual void* AddRef(void* pvObject) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, AddRefHostObject, pvObject);
    }

    virtual void Release(void* pvObject) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(ReleaseHostObject, pvObject);
    }

    virtual V8Value GetProperty(void* pvObject, const StdString& name) override
    {
        V8Value value(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectNamedProperty, pvObject, name, value);
        return value;
    }

    virtual V8Value GetProperty(void* pvObject, const StdString& name, bool& isCacheable) override
    {
        V8Value value(V8Value::Nonexistent);
        StdBool tempIsCacheable;
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectNamedPropertyWithCacheability, pvObject, name, value, tempIsCacheable);
        isCacheable = (tempIsCacheable != 0);
        return value;
    }

    virtual void SetProperty(void* pvObject, const StdString& name, const V8Value& value) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetHostObjectNamedProperty, pvObject, name, value);
    }

    virtual bool DeleteProperty(void* pvObject, const StdString& name) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteHostObjectNamedProperty, pvObject, name);
    }

    virtual void GetPropertyNames(void* pvObject, std::vector<StdString>& names) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectPropertyNames, pvObject, names);
    }

    virtual V8Value GetProperty(void* pvObject, int32_t index) override
    {
        V8Value value(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectIndexedProperty, pvObject, index, value);
        return value;
    }

    virtual void SetProperty(void* pvObject, int32_t index, const V8Value& value) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetHostObjectIndexedProperty, pvObject, index, value);
    }

    virtual bool DeleteProperty(void* pvObject, int32_t index) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteHostObjectIndexedProperty, pvObject, index);
    }

    virtual void GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectPropertyIndices, pvObject, indices);
    }

    virtual V8Value Invoke(void* pvObject, bool asConstructor, const std::vector<V8Value>& args) override
    {
        V8Value result(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObject, pvObject, asConstructor, args, result);
        return result;
    }

    virtual V8Value InvokeMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args) override
    {
        V8Value result(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObjectMethod, pvObject, name, args, result);
        return result;
    }

    virtual Invocability GetInvocability(void* pvObject) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE(Invocability, GetHostObjectInvocability, pvObject);
    }

    virtual V8Value GetEnumerator(void* pvObject) override
    {
        V8Value result(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectEnumerator, pvObject, result);
        return result;
    }

    virtual V8Value GetAsyncEnumerator(void* pvObject) override
    {
        V8Value result(V8Value::Nonexistent);
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectAsyncEnumerator, pvObject, result);
        return result;
    }

    virtual void* CreateV8ObjectCache() override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateV8ObjectCache);
    }

    virtual void CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(CacheV8Object, pvCache, pvObject, pvV8Object);
    }

    virtual void* GetCachedV8Object(void* pvCache, void* pvObject) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, GetCachedV8Object, pvCache, pvObject);
    }

    virtual void GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(GetAllCachedV8Objects, pvCache, v8ObjectPtrs);
    }

    virtual bool RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(StdBool, RemoveV8ObjectCacheEntry, pvCache, pvObject);
    }

    virtual void* CreateDebugAgent(const StdString& name, const StdString& version, int32_t port, bool remote, DebugCallback&& callback) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateDebugAgent, name, version, port, remote, new V8DebugCallbackHandle(new DebugCallback(std::move(callback))));
    }

    virtual void SendDebugMessage(void* pvAgent, const StdString& content) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(SendDebugMessage, pvAgent, content);
    }

    virtual void DestroyDebugAgent(void* pvAgent) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(DestroyDebugAgent, pvAgent);
    }

    virtual void QueueNativeCallback(NativeCallback&& callback) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(QueueNativeCallback, new NativeCallbackHandle(new NativeCallback(std::move(callback))));
    }

    virtual void* CreateNativeCallbackTimer(int32_t dueTime, int32_t period, NativeCallback&& callback) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateNativeCallbackTimer, dueTime, period, new NativeCallbackHandle(new NativeCallback(std::move(callback))));
    }

    virtual bool ChangeNativeCallbackTimer(void* pvTimer, int32_t dueTime, int32_t period) override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(StdBool, ChangeNativeCallbackTimer, pvTimer, dueTime, period);
    }

    virtual void DestroyNativeCallbackTimer(void* pvTimer) override
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(DestroyNativeCallbackTimer, pvTimer);
    }

    virtual StdString LoadModule(const V8DocumentInfo& sourceDocumentInfo, const StdString& specifier, V8DocumentInfo& documentInfo) override
    {
        StdString resourceName;
        StdString sourceMapUrl;
        uint64_t uniqueId;
        StdBool isModule;
        StdString code;
        void* pvDocumentInfo;
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(LoadModule, sourceDocumentInfo.GetDocumentInfo(), specifier, resourceName, sourceMapUrl, uniqueId, isModule, code, pvDocumentInfo);

        documentInfo = V8DocumentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, isModule, pvDocumentInfo);
        return code;
    }

    virtual std::vector<std::pair<StdString, V8Value>> CreateModuleContext(const V8DocumentInfo& documentInfo) override
    {
        std::vector<StdString> names;
        std::vector<V8Value> values;
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(CreateModuleContext, documentInfo.GetDocumentInfo(), names, values);
        auto count = std::min(names.size(), values.size());

        std::vector<std::pair<StdString, V8Value>> context;
        context.reserve(count);
        for (size_t index = 0; index < count; index++)
        {
            context.emplace_back(names[index], values[index]);
        }

        return context;
    }

    virtual size_t GetMaxScriptCacheSize() override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(int32_t, GetMaxScriptCacheSize);
    }

    virtual size_t GetMaxModuleCacheSize() override
    {
        return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(int32_t, GetMaxModuleCacheSize);
    }

private:

    HostObjectUtilNativeImpl() noexcept = default;

    static HostObjectUtilNativeImpl s_Instance;
};

//-----------------------------------------------------------------------------

HostObjectUtilNativeImpl HostObjectUtilNativeImpl::s_Instance;

//-----------------------------------------------------------------------------
// HostObjectUtil implementation
//-----------------------------------------------------------------------------

IHostObjectUtil& HostObjectUtil::GetInstance() noexcept
{
    return HostObjectUtilNativeImpl::GetInstance();
}
