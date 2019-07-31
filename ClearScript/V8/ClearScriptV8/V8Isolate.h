// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Isolate
//-----------------------------------------------------------------------------

class V8Isolate: public WeakRefTarget<V8Isolate>, public IV8Entity
{
public:

    struct Options final
    {
        bool EnableDebugging = false;
        bool EnableRemoteDebugging = false;
        bool EnableDynamicModuleImports = false;
        int DebugPort = 0;
    };

    struct Statistics final
    {
        size_t ScriptCount = 0;
        size_t ScriptCacheSize = 0;
        size_t ModuleCount = 0;
    };

    static V8Isolate* Create(const StdString& name, const v8::ResourceConstraints* pConstraints, const Options& options);
    static size_t GetInstanceCount();

    virtual size_t GetMaxHeapSize() = 0;
    virtual void SetMaxHeapSize(size_t value) = 0;
    virtual double GetHeapSizeSampleInterval() = 0;
    virtual void SetHeapSizeSampleInterval(double value) = 0;

    virtual size_t GetMaxStackUsage() = 0;
    virtual void SetMaxStackUsage(size_t value) = 0;

    virtual void AwaitDebuggerAndPause() = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual void GetHeapStatistics(v8::HeapStatistics& heapStatistics) = 0;
    virtual Statistics GetStatistics() = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;

    typedef void CpuProfileCallbackT(const v8::CpuProfile& profile, void* pvArg);
    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) = 0;
    virtual bool EndCpuProfile(const StdString& name, CpuProfileCallbackT* pCallback, void* pvArg) = 0;
    virtual void CollectCpuProfileSample() = 0;
    virtual uint32_t GetCpuProfileSampleInterval() = 0;
    virtual void SetCpuProfileSampleInterval(uint32_t value) = 0;

    virtual ~V8Isolate() {}
};
