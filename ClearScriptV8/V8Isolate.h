// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Isolate
//-----------------------------------------------------------------------------

class V8Isolate: public WeakRefTarget<V8Isolate>, public IV8Entity
{
public:

    enum class TaskKind: uint16_t
    {
        Worker,
        DelayedWorker,
        Foreground,
        DelayedForeground,
        NonNestableForeground,
        NonNestableDelayedForeground,
        Count
    };

    struct Options final
    {
        double HeapExpansionMultiplier = 0;
        size_t MaxArrayBufferAllocation = SIZE_MAX;
        bool EnableDebugging = false;
        bool EnableRemoteDebugging = false;
        bool EnableDynamicModuleImports = false;
        int DebugPort = 0;
    };

    struct Statistics final
    {
        using TaskCounts = std::array<size_t, static_cast<size_t>(TaskKind::Count)>;

        void BumpPostedTaskCount(TaskKind kind)
        {
            ++PostedTaskCounts[static_cast<size_t>(kind)];
        }

        void BumpInvokedTaskCount(TaskKind kind)
        {
            ++InvokedTaskCounts[static_cast<size_t>(kind)];
        }

        size_t ScriptCount = 0;
        size_t ScriptCacheSize = 0;
        size_t ModuleCount = 0;
        TaskCounts PostedTaskCounts = {};
        TaskCounts InvokedTaskCounts = {};
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
    virtual void CancelAwaitDebugger() = 0;

    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual void GetHeapStatistics(v8::HeapStatistics& heapStatistics) = 0;
    virtual Statistics GetStatistics() = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;

    typedef void CpuProfileCallback(const v8::CpuProfile& profile, void* pvArg);
    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) = 0;
    virtual bool EndCpuProfile(const StdString& name, CpuProfileCallback* pCallback, void* pvArg) = 0;
    virtual void CollectCpuProfileSample() = 0;
    virtual uint32_t GetCpuProfileSampleInterval() = 0;
    virtual void SetCpuProfileSampleInterval(uint32_t value) = 0;

    virtual void WriteHeapSnapshot(void* pvStream) = 0;

    virtual ~V8Isolate() {}
};
