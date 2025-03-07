// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// HostObjectUtil implementation
//-----------------------------------------------------------------------------

void* HostObjectUtil::AddRef(void* pvObject)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, AddRefHostObject, pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::Release(void* pvObject)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(ReleaseHostObject, pvObject);
}

//-----------------------------------------------------------------------------

HostObjectUtil::Invocability HostObjectUtil::GetInvocability(void* pvObject)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(Invocability, GetHostObjectInvocability, pvObject);
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::GetProperty(void* pvObject, const StdString& name, bool& isCacheable)
{
    V8Value value(V8Value::Nonexistent);
    StdBool tempIsCacheable;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectNamedProperty, pvObject, name, value, tempIsCacheable);
    isCacheable = (tempIsCacheable != 0);
    return value;
}

//-----------------------------------------------------------------------------

void HostObjectUtil::SetProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    V8Value::Decoded decodedValue;
    value.Decode(decodedValue);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetHostObjectNamedProperty, pvObject, name, decodedValue);
}

//-----------------------------------------------------------------------------

bool HostObjectUtil::DeleteProperty(void* pvObject, const StdString& name)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteHostObjectNamedProperty, pvObject, name);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::GetPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectPropertyNames, pvObject, names);
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::GetProperty(void* pvObject, int32_t index)
{
    V8Value value(V8Value::Nonexistent);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectIndexedProperty, pvObject, index, value);
    return value;
}

//-----------------------------------------------------------------------------

void HostObjectUtil::SetProperty(void* pvObject, int32_t index, const V8Value& value)
{
    V8Value::Decoded decodedValue;
    value.Decode(decodedValue);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetHostObjectIndexedProperty, pvObject, index, decodedValue);
}

//-----------------------------------------------------------------------------

bool HostObjectUtil::DeleteProperty(void* pvObject, int32_t index)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteHostObjectIndexedProperty, pvObject, index);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectPropertyIndices, pvObject, indices);
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::Invoke(void* pvObject, bool asConstructor, size_t argCount, const V8Value* pArgs)
{
    V8Value result(V8Value::Nonexistent);

    if (argCount < 1)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObject, pvObject, asConstructor, 0, nullptr, result);
    }
    else if (argCount <= Constants::MaxInlineArgCount)
    {
        V8Value::Decoded decodedArgs[argCount];

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(decodedArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObject, pvObject, asConstructor, static_cast<int32_t>(argCount), decodedArgs, result);
    }
    else
    {
        auto upDecodedArgs = std::make_unique<V8Value::Decoded[]>(argCount);
        auto pDecodedArgs = upDecodedArgs.get();

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(pDecodedArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObject, pvObject, asConstructor, static_cast<int32_t>(argCount), pDecodedArgs, result);
    }

    return result;
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::InvokeMethod(void* pvObject, const StdString& name, size_t argCount, const V8Value* pArgs)
{
    V8Value result(V8Value::Nonexistent);

    if (argCount < 1)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObjectMethod, pvObject, name, 0, nullptr, result);
    }
    else if (argCount <= Constants::MaxInlineArgCount)
    {
        V8Value::Decoded decodedArgs[argCount];

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(decodedArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObjectMethod, pvObject, name, static_cast<int32_t>(argCount), decodedArgs, result);
    }
    else
    {
        auto upDecodedArgs = std::make_unique<V8Value::Decoded[]>(argCount);
        auto pDecodedArgs = upDecodedArgs.get();

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(pDecodedArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeHostObjectMethod, pvObject, name, static_cast<int32_t>(argCount), pDecodedArgs, result);
    }

    return result;
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::GetEnumerator(void* pvObject)
{
    V8Value result(V8Value::Nonexistent);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectEnumerator, pvObject, result);
    return result;
}

//-----------------------------------------------------------------------------

V8Value HostObjectUtil::GetAsyncEnumerator(void* pvObject)
{
    V8Value result(V8Value::Nonexistent);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetHostObjectAsyncEnumerator, pvObject, result);
    return result;
}

//-----------------------------------------------------------------------------

void* HostObjectUtil::CreateV8ObjectCache()
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateV8ObjectCache);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(CacheV8Object, pvCache, pvObject, pvV8Object);
}

//-----------------------------------------------------------------------------

void* HostObjectUtil::GetCachedV8Object(void* pvCache, void* pvObject)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, GetCachedV8Object, pvCache, pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(GetAllCachedV8Objects, pvCache, v8ObjectPtrs);
}

//-----------------------------------------------------------------------------

bool HostObjectUtil::RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(StdBool, RemoveV8ObjectCacheEntry, pvCache, pvObject);
}

//-----------------------------------------------------------------------------

void* HostObjectUtil::CreateDebugAgent(const StdString& name, const StdString& version, int32_t port, bool remote, DebugCallback&& callback)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateDebugAgent, name, version, port, remote, new V8DebugCallbackHandle(new DebugCallback(std::move(callback))));
}

//-----------------------------------------------------------------------------

void HostObjectUtil::SendDebugMessage(void* pvAgent, const StdString& content)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(SendDebugMessage, pvAgent, content);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::DestroyDebugAgent(void* pvAgent)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(DestroyDebugAgent, pvAgent);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::QueueNativeCallback(NativeCallback&& callback)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(QueueNativeCallback, new NativeCallbackHandle(new NativeCallback(std::move(callback))));
}

//-----------------------------------------------------------------------------

void* HostObjectUtil::CreateNativeCallbackTimer(int32_t dueTime, int32_t period, NativeCallback&& callback)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(void*, CreateNativeCallbackTimer, dueTime, period, new NativeCallbackHandle(new NativeCallback(std::move(callback))));
}

//-----------------------------------------------------------------------------

bool HostObjectUtil::ChangeNativeCallbackTimer(void* pvTimer, int32_t dueTime, int32_t period)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(StdBool, ChangeNativeCallbackTimer, pvTimer, dueTime, period);
}

//-----------------------------------------------------------------------------

void HostObjectUtil::DestroyNativeCallbackTimer(void* pvTimer)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID_NOTHROW(DestroyNativeCallbackTimer, pvTimer);
}

//-----------------------------------------------------------------------------

StdString HostObjectUtil::LoadModule(const V8DocumentInfo& sourceDocumentInfo, const StdString& specifier, V8DocumentInfo& documentInfo, V8Value& exports)
{
    StdString resourceName;
    StdString sourceMapUrl;
    uint64_t uniqueId;
    DocumentKind documentKind;
    StdString code;
    void* pvDocumentInfo;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(LoadModule, sourceDocumentInfo.GetDocumentInfo(), specifier, resourceName, sourceMapUrl, uniqueId, documentKind, code, pvDocumentInfo, exports);

    documentInfo = V8DocumentInfo(std::move(resourceName), std::move(sourceMapUrl), uniqueId, documentKind, pvDocumentInfo);
    return code;
}

//-----------------------------------------------------------------------------

std::vector<std::pair<StdString, V8Value>> HostObjectUtil::CreateModuleContext(const V8DocumentInfo& documentInfo)
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

//-----------------------------------------------------------------------------

size_t HostObjectUtil::GetMaxScriptCacheSize()
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(int32_t, GetMaxScriptCacheSize);
}

//-----------------------------------------------------------------------------

size_t HostObjectUtil::GetMaxModuleCacheSize()
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(int32_t, GetMaxModuleCacheSize);
}

//-----------------------------------------------------------------------------
// FastHostObjectUtil implementation
//-----------------------------------------------------------------------------

V8Value FastHostObjectUtil::GetProperty(void* pvObject, const StdString& name, bool& isCacheable)
{
    V8Value::FastResult value;
    StdBool tempIsCacheable;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectNamedProperty, pvObject, name, value, tempIsCacheable);
    isCacheable = (tempIsCacheable != 0);
    return V8Value(value);
}

//-----------------------------------------------------------------------------

void FastHostObjectUtil::SetProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    V8Value::FastArg fastValue;
    value.Decode(fastValue);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetFastHostObjectNamedProperty, pvObject, name, fastValue);
}

//-----------------------------------------------------------------------------

FastHostObjectUtil::PropertyFlags FastHostObjectUtil::QueryProperty(void* pvObject, const StdString& name)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(PropertyFlags, QueryFastHostObjectNamedProperty, pvObject, name);
}

//-----------------------------------------------------------------------------

bool FastHostObjectUtil::DeleteProperty(void* pvObject, const StdString& name)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteFastHostObjectNamedProperty, pvObject, name);
}

//-----------------------------------------------------------------------------

void FastHostObjectUtil::GetPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectPropertyNames, pvObject, names);
}

//-----------------------------------------------------------------------------

V8Value FastHostObjectUtil::GetProperty(void* pvObject, int32_t index)
{
    V8Value::FastResult value;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectIndexedProperty, pvObject, index, value);
    return V8Value(value);
}

//-----------------------------------------------------------------------------

void FastHostObjectUtil::SetProperty(void* pvObject, int32_t index, const V8Value& value)
{
    V8Value::FastArg fastValue;
    value.Decode(fastValue);
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(SetFastHostObjectIndexedProperty, pvObject, index, fastValue);
}

//-----------------------------------------------------------------------------

FastHostObjectUtil::PropertyFlags FastHostObjectUtil::QueryProperty(void* pvObject, int32_t index)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(PropertyFlags, QueryFastHostObjectIndexedProperty, pvObject, index);
}

//-----------------------------------------------------------------------------

bool FastHostObjectUtil::DeleteProperty(void* pvObject, int32_t index)
{
    return V8_SPLIT_PROXY_MANAGED_INVOKE(StdBool, DeleteFastHostObjectIndexedProperty, pvObject, index);
}

//-----------------------------------------------------------------------------

void FastHostObjectUtil::GetPropertyIndices(void* pvObject, std::vector<int32_t>& indices)
{
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectPropertyIndices, pvObject, indices);
}

//-----------------------------------------------------------------------------

V8Value FastHostObjectUtil::Invoke(void* pvObject, bool asConstructor, size_t argCount, const V8Value* pArgs)
{
    V8Value::FastResult result;

    if (argCount < 1)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeFastHostObject, pvObject, asConstructor, 0, nullptr, result);
    }
    else if (argCount <= Constants::MaxInlineArgCount)
    {
        V8Value::FastArg fastArgs[argCount];

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(fastArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeFastHostObject, pvObject, asConstructor, static_cast<int32_t>(argCount), fastArgs, result);
    }
    else
    {
        auto upFastArgs = std::make_unique<V8Value::FastArg[]>(argCount);
        auto pFastArgs = upFastArgs.get();

        for (size_t index = 0; index < argCount; index++)
        {
            pArgs[index].Decode(pFastArgs[index]);
        }

        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(InvokeFastHostObject, pvObject, asConstructor, static_cast<int32_t>(argCount), pFastArgs, result);
    }

    return V8Value(result);
}

//-----------------------------------------------------------------------------

V8Value FastHostObjectUtil::GetEnumerator(void* pvObject)
{
    V8Value::FastResult result;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectEnumerator, pvObject, result);
    return V8Value(result);
}

//-----------------------------------------------------------------------------

V8Value FastHostObjectUtil::GetAsyncEnumerator(void* pvObject)
{
    V8Value::FastResult result;
    V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(GetFastHostObjectAsyncEnumerator, pvObject, result);
    return V8Value(result);
}
