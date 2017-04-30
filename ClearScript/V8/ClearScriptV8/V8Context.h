// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Context
//-----------------------------------------------------------------------------

class V8Context: public WeakRefTarget<V8Context>
{
public:

    static V8Context* Create(const SharedPtr<V8Isolate>& spIsolate, const StdString& name, bool enableDebugging, bool disableGlobalMembers, int debugPort);
    static size_t GetInstanceCount();

    virtual size_t GetMaxIsolateHeapSize() = 0;
    virtual void SetMaxIsolateHeapSize(size_t value) = 0;
    virtual double GetIsolateHeapSizeSampleInterval() = 0;
    virtual void SetIsolateHeapSizeSampleInterval(double value) = 0;

    virtual size_t GetMaxIsolateStackUsage() = 0;
    virtual void SetMaxIsolateStackUsage(size_t value) = 0;

    typedef void LockCallbackT(void* pvArg);
    virtual void CallWithLock(LockCallbackT* pCallback, void* pvArg) = 0;

    virtual V8Value GetRootObject() = 0;
    virtual void SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers) = 0;
    virtual V8Value Execute(const StdString& documentName, const StdString& code, bool evaluate, bool discard) = 0;

    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code) = 0;
    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual bool CanExecute(V8ScriptHolder* pHolder) = 0;
    virtual V8Value Execute(V8ScriptHolder* pHolder, bool evaluate) = 0;

    virtual void Interrupt() = 0;
    virtual void GetIsolateHeapInfo(V8IsolateHeapInfo& heapInfo) = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;
    virtual void OnAccessSettingsChanged() = 0;

    virtual void Destroy() = 0;

protected:

    virtual ~V8Context() {};
};

//-----------------------------------------------------------------------------
// SharedPtrTraits<V8Context>
//-----------------------------------------------------------------------------

template<>
class SharedPtrTraits<V8Context>
{
    PROHIBIT_CONSTRUCT(SharedPtrTraits)

public:

    static void Destroy(V8Context* pTarget)
    {
        pTarget->Destroy();
    }
};
