// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8Context
//-----------------------------------------------------------------------------

class V8Context: public WeakRefTarget<V8Context>, public IV8Entity
{
public:

    enum class Flags : int32_t
    {
        // IMPORTANT: maintain bitwise equivalence with managed enum V8.V8ScriptEngineFlags
        None = 0,
        EnableDebugging = 0x00000001,
        DisableGlobalMembers = 0x00000002,
        EnableRemoteDebugging = 0x00000004,
        AwaitDebuggerAndPauseOnStart = 0x00000008,
        EnableDateTimeConversion = 0x00000010,
        EnableDynamicModuleImports = 0x00000020,
        MarshalUnsafeInt64AsBigInt = 0x00000040,
        MarshalAllInt64AsBigInt = 0x00000080,
        EnableTaskPromiseConversion = 0x00000100,
        EnableValueTaskPromiseConversion = 0x00000200,
        UseCaseInsensitiveMemberBinding = 0x00000400,
        EnableStringifyEnhancements = 0x00000800,
        HideHostExceptions = 0x00001000,
        UseSynchronizationContexts = 0x00002000,
        AddPerformanceObject = 0x00004000,
        SetTimerResolution = 0x00008000,
        EnableArrayConversion = 0x00010000
    };

    struct Options final
    {
        Flags Flags = Flags::None;
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

    typedef void CallWithLockCallback(void* pvAction);
    virtual void CallWithLock(CallWithLockCallback* pCallback, void* pvAction) = 0;

    typedef void CallWithLockWithArgCallback(void* pvAction, void* pvArg);
    virtual void CallWithLockWithArg(CallWithLockWithArgCallback* pCallback, void* pvAction, void* pvArg) = 0;

    virtual V8Value GetRootObject() = 0;
    virtual void SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers) = 0;

    virtual void AwaitDebuggerAndPause() = 0;
    virtual void CancelAwaitDebugger() = 0;

    virtual V8Value Execute(const V8DocumentInfo& documentInfo, const StdString& code, bool evaluate) = 0;

    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) = 0;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult) = 0;

    virtual bool CanExecute(const SharedPtr<V8ScriptHolder>& spHolder) = 0;
    virtual V8Value Execute(const SharedPtr<V8ScriptHolder>& spHolder, bool evaluate) = 0;

    virtual void Interrupt() = 0;
    virtual void CancelInterrupt() = 0;
    virtual bool GetEnableIsolateInterruptPropagation() = 0;
    virtual void SetEnableIsolateInterruptPropagation(bool value) = 0;
    virtual bool GetDisableIsolateHeapSizeViolationInterrupt() = 0;
    virtual void SetDisableIsolateHeapSizeViolationInterrupt(bool value) = 0;

    virtual void GetIsolateHeapStatistics(v8::HeapStatistics& heapStatistics) = 0;
    virtual V8Isolate::Statistics GetIsolateStatistics() = 0;
    virtual Statistics GetStatistics() = 0;
    virtual void CollectGarbage(bool exhaustive) = 0;
    virtual void OnAccessSettingsChanged() = 0;

    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) = 0;
    virtual bool EndCpuProfile(const StdString& name, V8Isolate::CpuProfileCallback* pCallback, void* pvArg) = 0;
    virtual void CollectCpuProfileSample() = 0;
    virtual uint32_t GetCpuProfileSampleInterval() = 0;
    virtual void SetCpuProfileSampleInterval(uint32_t value) = 0;

    virtual void WriteIsolateHeapSnapshot(void* pvStream) = 0;

    virtual void Flush() = 0;
    virtual void Destroy() = 0;

protected:

    virtual ~V8Context() {};
};

//-----------------------------------------------------------------------------
// SharedPtrTraits<V8Context>
//-----------------------------------------------------------------------------

template <>
struct SharedPtrTraits<V8Context> final: StaticBase
{
    static void Destroy(V8Context* pTarget)
    {
        pTarget->Destroy();
    }
};
