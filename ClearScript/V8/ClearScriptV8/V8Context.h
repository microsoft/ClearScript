// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Context
//-----------------------------------------------------------------------------

class V8Context: public WeakRefTarget<V8Context>, public IV8Entity
{
public:

    struct Options final
    {
        bool EnableDebugging = false;
        bool EnableRemoteDebugging = false;
        bool DisableGlobalMembers = true;
        bool EnableDateTimeConversion = false;
        bool EnableDynamicModuleImports = false;
        int DebugPort = 0;
    };

    struct Statistics final
    {
        size_t ScriptCount = 0;
        size_t ModuleCount = 0;
        size_t ModuleCacheSize = 0;
    };

    static V8Context* Create(const SharedPtr<V8Isolate>& spIsolate, const StdString& name, const Options& options);
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

    virtual void AwaitDebuggerAndPause() = 0;
    virtual V8Value Execute(const V8DocumentInfo& documentInfo, const StdString& code, bool evaluate) = 0;

    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual bool CanExecute(V8ScriptHolder* pHolder) = 0;
    virtual V8Value Execute(V8ScriptHolder* pHolder, bool evaluate) = 0;

    virtual void Interrupt() = 0;
    virtual void GetIsolateHeapStatistics(v8::HeapStatistics& heapStatistics) = 0;
    virtual V8Isolate::Statistics GetIsolateStatistics() = 0;
    virtual Statistics GetStatistics() = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;
    virtual void OnAccessSettingsChanged() = 0;

    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) = 0;
    virtual bool EndCpuProfile(const StdString& name, V8Isolate::CpuProfileCallbackT* pCallback, void* pvArg) = 0;
    virtual void CollectCpuProfileSample() = 0;
    virtual uint32_t GetCpuProfileSampleInterval() = 0;
    virtual void SetCpuProfileSampleInterval(uint32_t value) = 0;

    virtual void Flush() = 0;
    virtual void Destroy() = 0;

protected:

    virtual ~V8Context() {};
};

//-----------------------------------------------------------------------------
// SharedPtrTraits<V8Context>
//-----------------------------------------------------------------------------

template <>
class SharedPtrTraits<V8Context> final
{
    PROHIBIT_CONSTRUCT(SharedPtrTraits)

public:

    static void Destroy(V8Context* pTarget)
    {
        pTarget->Destroy();
    }
};
