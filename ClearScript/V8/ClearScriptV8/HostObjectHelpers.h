// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HostObjectHelpers
//-----------------------------------------------------------------------------

class HostObjectHelpers
{
    PROHIBIT_CONSTRUCT(HostObjectHelpers)

public:

    static void* AddRef(void* pvObject);
    static void Release(void* pvObject);

    static V8Value GetProperty(void* pvObject, const StdString& name);
    static V8Value GetProperty(void* pvObject, const StdString& name, bool& isCacheable);
    static void SetProperty(void* pvObject, const StdString& name, const V8Value& value);
    static bool DeleteProperty(void* pvObject, const StdString& name);
    static void GetPropertyNames(void* pvObject, std::vector<StdString>& names);

    static V8Value GetProperty(void* pvObject, int index);
    static void SetProperty(void* pvObject, int index, const V8Value& value);
    static bool DeleteProperty(void* pvObject, int index);
    static void GetPropertyIndices(void* pvObject, std::vector<int>& indices);

    static V8Value Invoke(void* pvObject, const std::vector<V8Value>& args, bool asConstructor);
    static V8Value InvokeMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args);
    static bool IsDelegate(void* pvObject);

    static V8Value GetEnumerator(void* pvObject);
    static bool AdvanceEnumerator(void* pvEnumerator, V8Value& value);

    static void* CreateV8ObjectCache();
    static void CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object);
    static void* GetCachedV8Object(void* pvCache, void* pvObject);
    static void GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs);
    static bool RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject);

    enum class DebugDirective { SendDebugCommand, DispatchDebugMessages };
    typedef std::function<void(DebugDirective directive, const StdString* pCommand)> DebugCallback;
    static void* CreateDebugAgent(const StdString& name, const StdString& version, int port, DebugCallback&& callback);
    static void SendDebugMessage(void* pvAgent, const StdString& content);
    static void DestroyDebugAgent(void* pvAgent);

    typedef std::function<void()> NativeCallback;
    static void QueueNativeCallback(NativeCallback&& callback);
    static void* CreateNativeCallbackTimer(int dueTime, int period, NativeCallback&& callback);
    static bool ChangeNativeCallbackTimer(void* pvTimer, int dueTime, int period);
    static void DestroyNativeCallbackTimer(void* pvTimer);

    static bool TryParseInt32(const StdString& text, int& result);
};
