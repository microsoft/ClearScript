// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8Platform
//-----------------------------------------------------------------------------

class V8Platform final: public v8::Platform
{
public:

    static V8Platform& GetInstance();
    void EnsureInitialized();
    V8GlobalFlags GetGlobalFlags() const;

    virtual v8::PageAllocator* GetPageAllocator() override;
    virtual int NumberOfWorkerThreads() override;
    virtual std::shared_ptr<v8::TaskRunner> GetForegroundTaskRunner(v8::Isolate* pIsolate, v8::TaskPriority priority) override;
    virtual double MonotonicallyIncreasingTime() override;
    virtual double CurrentClockTimeMillis() override;
    virtual v8::TracingController* GetTracingController() override;

protected:

    virtual std::unique_ptr<v8::JobHandle> CreateJobImpl(v8::TaskPriority priority, std::unique_ptr<v8::JobTask> upJobTask, const v8::SourceLocation& location) override;
    virtual void PostTaskOnWorkerThreadImpl(v8::TaskPriority priority, std::unique_ptr<v8::Task> upTask, const v8::SourceLocation& location) override;
    virtual void PostDelayedTaskOnWorkerThreadImpl(v8::TaskPriority priority, std::unique_ptr<v8::Task> upTask, double delayInSeconds, const v8::SourceLocation& location) override;

private:

    V8Platform();

    static V8Platform ms_Instance;
    std::unique_ptr<v8::PageAllocator> m_upPageAllocator;
    OnceFlag m_InitializationFlag;
    V8GlobalFlags m_GlobalFlags;
    v8::TracingController m_TracingController;
};

//-----------------------------------------------------------------------------

V8Platform& V8Platform::GetInstance()
{
    return ms_Instance;
}

//-----------------------------------------------------------------------------

V8GlobalFlags V8Platform::GetGlobalFlags() const
{
    return m_GlobalFlags;
}

//-----------------------------------------------------------------------------

void V8Platform::EnsureInitialized()
{
    m_InitializationFlag.CallOnce([this]
    {
        v8::V8::InitializePlatform(&ms_Instance);

        m_GlobalFlags = V8_SPLIT_PROXY_MANAGED_INVOKE_NOTHROW(V8GlobalFlags, GetGlobalFlags);
        std::vector<std::string> flagStrings;
        flagStrings.push_back("--expose_gc");

    #ifdef CLEARSCRIPT_TOP_LEVEL_AWAIT_CONTROL

        if (!HasFlag(globalFlags, V8GlobalFlags::EnableTopLevelAwait))
        {
            flagStrings.push_back("--no_harmony_top_level_await");
        }

    #endif // CLEARSCRIPT_TOP_LEVEL_AWAIT_CONTROL

        if (HasFlag(m_GlobalFlags, V8GlobalFlags::DisableJITCompilation))
        {
            flagStrings.push_back("--jitless");
        }

        if (HasFlag(m_GlobalFlags, V8GlobalFlags::DisableBackgroundWork))
        {
            flagStrings.push_back("--single_threaded");
        }

        if (!flagStrings.empty())
        {
            std::string flagsString(flagStrings[0]);
            for (size_t index = 1; index < flagStrings.size(); index++)
            {
                flagsString += " ";
                flagsString += flagStrings[index];
            }

            v8::V8::SetFlagsFromString(flagsString.c_str(), flagsString.length());
        }

        ASSERT_EVAL(v8::V8::Initialize());
    });
}

//-----------------------------------------------------------------------------

v8::PageAllocator* V8Platform::GetPageAllocator()
{
    return m_upPageAllocator.get();
}

//-----------------------------------------------------------------------------

int V8Platform::NumberOfWorkerThreads()
{
    return static_cast<int>(HighResolutionClock::GetHardwareConcurrency());
}

//-----------------------------------------------------------------------------

std::shared_ptr<v8::TaskRunner> V8Platform::GetForegroundTaskRunner(v8::Isolate* pIsolate, v8::TaskPriority /*priority*/)
{
    return V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->GetForegroundTaskRunner();
}

//-----------------------------------------------------------------------------

std::unique_ptr<v8::JobHandle> V8Platform::CreateJobImpl(v8::TaskPriority priority, std::unique_ptr<v8::JobTask> upJobTask, const v8::SourceLocation& /*location*/)
{
    return v8::platform::NewDefaultJobHandle(this, priority, std::move(upJobTask), NumberOfWorkerThreads());
}

//-----------------------------------------------------------------------------

void V8Platform::PostTaskOnWorkerThreadImpl(v8::TaskPriority /*priority*/, std::unique_ptr<v8::Task> upTask, const v8::SourceLocation& /*location*/)
{
    auto pIsolate = v8::Isolate::GetCurrent();
    if (pIsolate == nullptr)
    {
        upTask->Run();
    }
    else
    {
        V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskAsync(std::move(upTask));
    }
}

//-----------------------------------------------------------------------------

void V8Platform::PostDelayedTaskOnWorkerThreadImpl(v8::TaskPriority /*priority*/, std::unique_ptr<v8::Task> upTask, double delayInSeconds, const v8::SourceLocation& /*location*/)
{
    auto pIsolate = v8::Isolate::GetCurrent();
    if (pIsolate != nullptr)
    {
        V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskDelayed(std::move(upTask), delayInSeconds);
    }
}

//-----------------------------------------------------------------------------

double V8Platform::MonotonicallyIncreasingTime()
{
    return HighResolutionClock::GetRelativeSeconds();
}

//-----------------------------------------------------------------------------

double V8Platform::CurrentClockTimeMillis()
{
    return std::chrono::duration_cast<std::chrono::duration<double, std::milli>>(std::chrono::system_clock::now().time_since_epoch()).count();
}

//-----------------------------------------------------------------------------

v8::TracingController* V8Platform::GetTracingController()
{
    return &m_TracingController;
}

//-----------------------------------------------------------------------------

V8Platform::V8Platform():
    m_upPageAllocator(v8::platform::NewDefaultPageAllocator()),
    m_GlobalFlags(V8GlobalFlags::None)
{
}

//-----------------------------------------------------------------------------

V8Platform V8Platform::ms_Instance;

//-----------------------------------------------------------------------------
// V8ForegroundTaskRunner
//-----------------------------------------------------------------------------

class V8ForegroundTaskRunner final: public v8::TaskRunner
{
    PROHIBIT_COPY(V8ForegroundTaskRunner)

public:

    V8ForegroundTaskRunner(V8IsolateImpl& isolateImpl);

    virtual void PostTask(std::unique_ptr<v8::Task> upTask) override;
    virtual void PostNonNestableTask(std::unique_ptr<v8::Task> upTask) override;
    virtual void PostDelayedTask(std::unique_ptr<v8::Task> upTask, double delayInSeconds) override;
    virtual void PostNonNestableDelayedTask(std::unique_ptr<v8::Task> upTask, double delayInSeconds) override;
    virtual void PostIdleTask(std::unique_ptr<v8::IdleTask> upTask) override;
    virtual bool IdleTasksEnabled() override;
    virtual bool NonNestableTasksEnabled() const override;
    virtual bool NonNestableDelayedTasksEnabled() const override;

private:

    V8IsolateImpl& m_IsolateImpl;
    WeakRef<V8Isolate> m_wrIsolate;
};

//-----------------------------------------------------------------------------

V8ForegroundTaskRunner::V8ForegroundTaskRunner(V8IsolateImpl& isolateImpl):
    m_IsolateImpl(isolateImpl),
    m_wrIsolate(isolateImpl.CreateWeakRef())
{
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostTask(std::unique_ptr<v8::Task> upTask)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (spIsolate.IsEmpty())
    {
        upTask->Run();
    }
    else
    {
        m_IsolateImpl.RunTaskWithLockAsync(true /*allowNesting*/, std::move(upTask));
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostNonNestableTask(std::unique_ptr<v8::Task> upTask)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        m_IsolateImpl.RunTaskWithLockAsync(false /*allowNesting*/, std::move(upTask));
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostDelayedTask(std::unique_ptr<v8::Task> upTask, double delayInSeconds)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        m_IsolateImpl.RunTaskWithLockDelayed(true /*allowNesting*/, std::move(upTask), delayInSeconds);
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostNonNestableDelayedTask(std::unique_ptr<v8::Task> upTask, double delayInSeconds)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        m_IsolateImpl.RunTaskWithLockDelayed(false /*allowNesting*/, std::move(upTask), delayInSeconds);
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostIdleTask(std::unique_ptr<v8::IdleTask> /*upTask*/)
{
    // unexpected call to unsupported method
    std::terminate();
}

//-----------------------------------------------------------------------------

bool V8ForegroundTaskRunner::IdleTasksEnabled()
{
    return false;
}

//-----------------------------------------------------------------------------

bool V8ForegroundTaskRunner::NonNestableTasksEnabled() const
{
    return true;
}

//-----------------------------------------------------------------------------

bool V8ForegroundTaskRunner::NonNestableDelayedTasksEnabled() const
{
    return true;
}

//-----------------------------------------------------------------------------
// V8ArrayBufferAllocator
//-----------------------------------------------------------------------------

class V8ArrayBufferAllocator final: public v8::ArrayBuffer::Allocator
{
public:

    V8ArrayBufferAllocator(V8IsolateImpl& isolateImpl);

    virtual void* Allocate(size_t size) override;
    virtual void* AllocateUninitialized(size_t size) override;
    virtual void Free(void* pvData, size_t size) override;

private:

    V8IsolateImpl& m_IsolateImpl;
    WeakRef<V8Isolate> m_wrIsolate;
};

//-----------------------------------------------------------------------------

V8ArrayBufferAllocator::V8ArrayBufferAllocator(V8IsolateImpl& isolateImpl):
    m_IsolateImpl(isolateImpl),
    m_wrIsolate(isolateImpl.CreateWeakRef())
{
}

//-----------------------------------------------------------------------------

void* V8ArrayBufferAllocator::Allocate(size_t size)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        return m_IsolateImpl.AllocateArrayBuffer(size);
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

void* V8ArrayBufferAllocator::AllocateUninitialized(size_t size)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        return m_IsolateImpl.AllocateUninitializedArrayBuffer(size);
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

void V8ArrayBufferAllocator::Free(void* pvData, size_t size)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        m_IsolateImpl.FreeArrayBuffer(pvData, size);
    }
    else if (pvData)
    {
        ::free(pvData);
    }
}

//-----------------------------------------------------------------------------
// V8OutputStream
//-----------------------------------------------------------------------------

class V8OutputStream final: public v8::OutputStream
{
    PROHIBIT_COPY(V8OutputStream)

public:

    explicit V8OutputStream(void* pvStream):
        m_pvStream(pvStream)
    {
    }

    virtual int GetChunkSize() override;
    virtual WriteResult WriteAsciiChunk(char* pData, int size) override;
    virtual void EndOfStream() override;

private:

    void* m_pvStream;
};

//-----------------------------------------------------------------------------

int V8OutputStream::GetChunkSize()
{
    return 64 * 1024;
}

//-----------------------------------------------------------------------------

V8OutputStream::WriteResult V8OutputStream::WriteAsciiChunk(char* pData, int size)
{
    try
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(WriteBytesToStream, m_pvStream, reinterpret_cast<uint8_t*>(pData), size);
        return kContinue;
    }
    catch (const HostException& exception)
    {
        V8_SPLIT_PROXY_MANAGED_INVOKE_VOID(ScheduleForwardingException, exception.GetException());
        return kAbort;
    }
}

//-----------------------------------------------------------------------------

void V8OutputStream::EndOfStream()
{
}

//-----------------------------------------------------------------------------
// V8IsolateImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_NATIVE_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        NativeScope t_IsolateNativeScope(*this); \
        DEFAULT_WARNING(4456)

#define END_ISOLATE_NATIVE_SCOPE \
        IGNORE_UNUSED(t_IsolateNativeScope); \
    }

#define BEGIN_ISOLATE_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        Scope t_IsolateScope(*this); \
        DEFAULT_WARNING(4456)

#define END_ISOLATE_SCOPE \
        IGNORE_UNUSED(t_IsolateScope); \
    }

#define BEGIN_PROMISE_HOOK_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        PromiseHookScope t_PromiseHookScope(*this); \
        DEFAULT_WARNING(4456)

#define END_PROMISE_HOOK_SCOPE \
        IGNORE_UNUSED(t_PromiseHookScope); \
    }

//-----------------------------------------------------------------------------

static std::atomic<size_t> s_InstanceCount(0);
static const int s_ContextGroupId = 1;
static const size_t s_StackBreathingRoom = static_cast<size_t>(16 * 1024);
static size_t* const s_pMinStackLimit = reinterpret_cast<size_t*>(sizeof(size_t));

//-----------------------------------------------------------------------------

V8IsolateImpl::V8IsolateImpl(const StdString& name, const v8::ResourceConstraints* pConstraints, const Options& options):
    m_Name(name),
    m_CallWithLockLevel(0),
    m_DebuggingEnabled(false),
    m_MaxArrayBufferAllocation(options.MaxArrayBufferAllocation),
    m_ArrayBufferAllocation(0),
    m_MaxHeapSize(0),
    m_HeapSizeSampleInterval(0.0),
    m_HeapWatchLevel(0),
    m_HeapExpansionMultiplier(options.HeapExpansionMultiplier),
    m_MaxStackUsage(0),
    m_EnableInterruptPropagation(false),
    m_DisableHeapSizeViolationInterrupt(false),
    m_CpuProfileSampleInterval(1000U),
    m_StackWatchLevel(0),
    m_pStackLimit(nullptr),
    m_IsExecutionTerminating(false),
    m_pExecutionScope(nullptr),
    m_pDocumentInfo(nullptr),
    m_IsOutOfMemory(false),
    m_Released(false)
{
    V8Platform::GetInstance().EnsureInitialized();

    m_upIsolate.reset(v8::Isolate::Allocate());
    m_upIsolate->SetData(0, this);

    BEGIN_ADDREF_SCOPE

        v8::Isolate::CreateParams params;
        params.array_buffer_allocator_shared = std::make_shared<V8ArrayBufferAllocator>(*this);
        if (pConstraints != nullptr)
        {
            params.constraints.set_max_young_generation_size_in_bytes(pConstraints->max_young_generation_size_in_bytes());
            params.constraints.set_max_old_generation_size_in_bytes(pConstraints->max_old_generation_size_in_bytes());
        }

        v8::Isolate::Initialize(m_upIsolate.get(), params);

        m_upIsolate->AddNearHeapLimitCallback(HeapExpansionCallback, this);
        m_upIsolate->AddBeforeCallEnteredCallback(OnBeforeCallEntered);

        BEGIN_ISOLATE_SCOPE

            m_upIsolate->SetCaptureStackTraceForUncaughtExceptions(true, 64, v8::StackTrace::kDetailed);

            m_hHostObjectHolderKey = CreatePersistent(CreatePrivate());

            if (HasFlag(options.Flags, Flags::EnableDebugging))
            {
                EnableDebugging(options.DebugPort, HasFlag(options.Flags, Flags::EnableRemoteDebugging));
            }

            m_upIsolate->SetHostInitializeImportMetaObjectCallback(ImportMetaInitializeCallback);
            if (HasFlag(options.Flags, Flags::EnableDynamicModuleImports))
            {
                m_upIsolate->SetHostImportModuleDynamicallyCallback(ModuleImportCallback);
            }

        END_ISOLATE_SCOPE

    END_ADDREF_SCOPE

    ++s_InstanceCount;
}

//-----------------------------------------------------------------------------

V8IsolateImpl* V8IsolateImpl::GetInstanceFromIsolate(v8::Isolate* pIsolate)
{
    _ASSERTE(pIsolate);
    return static_cast<V8IsolateImpl*>(pIsolate->GetData(0));
}

//-----------------------------------------------------------------------------

size_t V8IsolateImpl::GetInstanceCount()
{
    return s_InstanceCount;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::AddContext(V8ContextImpl* pContextImpl, const V8Context::Options& options)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (!HasFlag(options.Flags, V8Context::Flags::EnableDebugging))
    {
        m_ContextEntries.emplace_back(pContextImpl);
    }
    else
    {
        m_ContextEntries.emplace_front(pContextImpl);
        EnableDebugging(options.DebugPort, HasFlag(options.Flags, V8Context::Flags::EnableRemoteDebugging));
    }

    if (HasFlag(options.Flags, V8Context::Flags::EnableDynamicModuleImports))
    {
        m_upIsolate->SetHostImportModuleDynamicallyCallback(ModuleImportCallback);
    }

    if (m_upInspector)
    {
        m_upInspector->contextCreated(v8_inspector::V8ContextInfo(pContextImpl->GetContext(), s_ContextGroupId, pContextImpl->GetName().GetStringView()));
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RemoveContext(V8ContextImpl* pContextImpl)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (m_upInspector)
    {
        m_upInspector->contextDestroyed(pContextImpl->GetContext());
    }

    m_ContextEntries.remove_if([pContextImpl] (const ContextEntry& contextEntry)
    {
        return contextEntry.pContextImpl == pContextImpl;
    });
}

//-----------------------------------------------------------------------------

V8ContextImpl* V8IsolateImpl::FindContext(v8::Local<v8::Context> hContext)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (const auto& entry : m_ContextEntries)
    {
        if (entry.pContextImpl->GetContext() == hContext)
        {
            return entry.pContextImpl;
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::EnableDebugging(int port, bool remote)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (!m_DebuggingEnabled)
    {
        const char* pVersion = v8::V8::GetVersion();
        StdString version(v8_inspector::StringView(reinterpret_cast<const uint8_t*>(pVersion), strlen(pVersion)));

        if (port < 1)
        {
            port = 9222;
        }

        auto wrIsolate = CreateWeakRef();
        m_pvDebugAgent = HostObjectUtil::GetInstance().CreateDebugAgent(m_Name, version, port, remote, [this, wrIsolate] (IHostObjectUtil::DebugDirective directive, const StdString* pCommand)
        {
            auto spIsolate = wrIsolate.GetTarget();
            if (!spIsolate.IsEmpty())
            {
                if (directive == IHostObjectUtil::DebugDirective::ConnectClient)
                {
                    ConnectDebugClient();
                }
                else if ((directive == IHostObjectUtil::DebugDirective::SendCommand) && pCommand)
                {
                    SendDebugCommand(*pCommand);
                }
                else if (directive == IHostObjectUtil::DebugDirective::DisconnectClient)
                {
                    DisconnectDebugClient();
                }
            }
        });

        m_upInspector = v8_inspector::V8Inspector::create(m_upIsolate.get(), this);

        m_DebuggingEnabled = true;
        m_DebugPort = port;
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::DisableDebugging()
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (m_DebuggingEnabled)
    {
        m_upInspectorSession.reset();
        m_upInspector.reset();

        HostObjectUtil::GetInstance().DestroyDebugAgent(m_pvDebugAgent);
        m_DebuggingEnabled = false;
    }
}

//-----------------------------------------------------------------------------

size_t V8IsolateImpl::GetMaxHeapSize()
{
    return m_MaxHeapSize;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetMaxHeapSize(size_t value)
{
    m_MaxHeapSize = value;
    m_IsOutOfMemory = false;
}

//-----------------------------------------------------------------------------

double V8IsolateImpl::GetHeapSizeSampleInterval()
{
    return m_HeapSizeSampleInterval;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetHeapSizeSampleInterval(double value)
{
    m_HeapSizeSampleInterval = value;
}

//-----------------------------------------------------------------------------

size_t V8IsolateImpl::GetMaxStackUsage()
{
    return m_MaxStackUsage;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetMaxStackUsage(size_t value)
{
    m_MaxStackUsage = value;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::AwaitDebuggerAndPause()
{
    BEGIN_ISOLATE_SCOPE

        if (m_DebuggingEnabled && !m_upInspectorSession)
        {
            auto exitReason = RunMessageLoop(RunMessageLoopReason::AwaitingDebugger);
            switch (exitReason)
            {
                case ExitMessageLoopReason::TerminatedExecution:
                    throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(SL("Script execution interrupted by host while awaiting debugger connection")), false);

                case ExitMessageLoopReason::CanceledAwaitDebugger:
                    return;

                default:
                    _ASSERTE(exitReason == ExitMessageLoopReason::ResumedExecution);
            }

            _ASSERTE(m_upInspectorSession);
            if (m_upInspectorSession)
            {
                StdString breakReason(SL("Break on debugger connection"));
                m_upInspectorSession->schedulePauseOnNextStatement(breakReason.GetStringView(), breakReason.GetStringView());
            }
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CancelAwaitDebugger()
{
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        if (m_optRunMessageLoopReason == RunMessageLoopReason::AwaitingDebugger)
        {
            m_optExitMessageLoopReason = ExitMessageLoopReason::CanceledAwaitDebugger;
            m_CallWithLockQueueChanged.notify_one();
        }

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl(!m_ContextEntries.empty() ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl(!m_ContextEntries.empty() ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code), cacheKind, cacheBytes);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl(!m_ContextEntries.empty() ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code), cacheKind, cacheBytes, cacheAccepted);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl(!m_ContextEntries.empty() ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code), cacheKind, cacheBytes, cacheResult);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::GetEnableInterruptPropagation()
{
    return m_EnableInterruptPropagation;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetEnableInterruptPropagation(bool value)
{
    m_EnableInterruptPropagation = value;
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::GetDisableHeapSizeViolationInterrupt()
{
    return m_DisableHeapSizeViolationInterrupt;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetDisableHeapSizeViolationInterrupt(bool value)
{
    m_DisableHeapSizeViolationInterrupt = value;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::GetHeapStatistics(v8::HeapStatistics& heapStatistics)
{
    BEGIN_ISOLATE_SCOPE

        m_upIsolate->GetHeapStatistics(&heapStatistics);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8Isolate::Statistics V8IsolateImpl::GetStatistics()
{
    BEGIN_ISOLATE_SCOPE
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        return m_Statistics;

    END_MUTEX_SCOPE
    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CollectGarbage(bool exhaustive)
{
    BEGIN_ISOLATE_SCOPE

        if (exhaustive)
        {
            ClearScriptCache();
            ClearCachesForTesting();
            RequestGarbageCollectionForTesting(v8::Isolate::kFullGarbageCollection);
        }
        else
        {
            RequestGarbageCollectionForTesting(v8::Isolate::kMinorGarbageCollection);
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples)
{
    BEGIN_ISOLATE_SCOPE

        if (!m_upCpuProfiler)
        {
            m_upCpuProfiler.reset(v8::CpuProfiler::New(m_upIsolate.get()));
        }

        v8::Local<v8::String> hName;
        if (!CreateString(name).ToLocal(&hName))
        {
            return false;
        }

        m_upCpuProfiler->StartProfiling(hName, mode, recordSamples);
        return true;

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::EndCpuProfile(const StdString& name, CpuProfileCallback* pCallback, void* pvArg)
{
    BEGIN_ISOLATE_SCOPE

        if (!m_upCpuProfiler)
        {
            return false;
        }

        v8::Local<v8::String> hName;
        if (!CreateString(name).ToLocal(&hName))
        {
            return false;
        }

        UniqueDeletePtr<v8::CpuProfile> upProfile(m_upCpuProfiler->StopProfiling(hName));
        if (!upProfile)
        {
            return false;
        }

        if (pCallback)
        {
            pCallback(*upProfile, pvArg);
        }

        return true;

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CollectCpuProfileSample()
{
    BEGIN_ISOLATE_SCOPE

        v8::CpuProfiler::CollectSample(m_upIsolate.get());

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

uint32_t V8IsolateImpl::GetCpuProfileSampleInterval()
{
    return m_CpuProfileSampleInterval;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetCpuProfileSampleInterval(uint32_t value)
{
    BEGIN_ISOLATE_SCOPE

        if (value != m_CpuProfileSampleInterval)
        {
            m_CpuProfileSampleInterval = std::min(std::max(value, 125U), static_cast<uint32_t>(INT_MAX));

            if (!m_upCpuProfiler)
            {
                m_upCpuProfiler.reset(v8::CpuProfiler::New(m_upIsolate.get()));
            }

            m_upCpuProfiler->SetSamplingInterval(static_cast<int>(m_CpuProfileSampleInterval));
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::WriteHeapSnapshot(void* pvStream)
{
    BEGIN_ISOLATE_SCOPE

        auto pSnapshot = m_upIsolate->GetHeapProfiler()->TakeHeapSnapshot();

        V8OutputStream stream(pvStream);
        pSnapshot->Serialize(&stream);

        const_cast<v8::HeapSnapshot*>(pSnapshot)->Delete();

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::runMessageLoopOnPause(int /*contextGroupId*/)
{
    RunMessageLoop(RunMessageLoopReason::PausedInDebugger);
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::quitMessageLoopOnPause()
{
    _ASSERTE(IsCurrent() && IsLocked());

    BEGIN_MUTEX_SCOPE(m_DataMutex)
        m_optExitMessageLoopReason = ExitMessageLoopReason::ResumedExecution;
    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::runIfWaitingForDebugger(int /*contextGroupId*/)
{
    quitMessageLoopOnPause();
}

//-----------------------------------------------------------------------------

v8::Local<v8::Context> V8IsolateImpl::ensureDefaultContextInGroup(int contextGroupId)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (!m_ContextEntries.empty())
    {
        return m_ContextEntries.front().pContextImpl->GetContext();
    }

    return v8_inspector::V8InspectorClient::ensureDefaultContextInGroup(contextGroupId);
}

//-----------------------------------------------------------------------------

double V8IsolateImpl::currentTimeMS()
{
    return HighResolutionClock::GetRelativeSeconds() * 1000;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::sendResponse(int /*callId*/, std::unique_ptr<v8_inspector::StringBuffer> upMessage)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (m_pvDebugAgent)
    {
        HostObjectUtil::GetInstance().SendDebugMessage(m_pvDebugAgent, StdString(upMessage->string()));
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::sendNotification(std::unique_ptr<v8_inspector::StringBuffer> upMessage)
{
    sendResponse(0, std::move(upMessage));
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::flushProtocolNotifications()
{
}

//-----------------------------------------------------------------------------

void* V8IsolateImpl::AddRefV8Object(void* pvObject)
{
    BEGIN_ISOLATE_SCOPE

        return ::PtrFromHandle(CreatePersistent(::HandleFromPtr<v8::Object>(pvObject)));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ReleaseV8Object(void* pvObject)
{
    CallWithLockNoWait(true /*allowNesting*/, [pvObject] (V8IsolateImpl* pIsolateImpl)
    {
        pIsolateImpl->Dispose(::HandleFromPtr<v8::Object>(pvObject));
    });
}

//-----------------------------------------------------------------------------

void* V8IsolateImpl::AddRefV8Script(void* pvScript)
{
    BEGIN_ISOLATE_SCOPE

        return ::PtrFromHandle(CreatePersistent(::HandleFromPtr<v8::UnboundScript>(pvScript)));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ReleaseV8Script(void* pvScript)
{
    CallWithLockNoWait(true /*allowNesting*/, [pvScript] (V8IsolateImpl* pIsolateImpl)
    {
        pIsolateImpl->Dispose(::HandleFromPtr<v8::Script>(pvScript));
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskAsync(std::unique_ptr<v8::Task> upTask)
{
    if (upTask)
    {
        if (m_Released)
        {
            upTask->Run();
        }
        else
        {
            std::shared_ptr<v8::Task> spTask(std::move(upTask));
            std::weak_ptr<v8::Task> wpTask(spTask);

            BEGIN_MUTEX_SCOPE(m_DataMutex)
                m_AsyncTasks.push_back(std::move(spTask));
                m_Statistics.BumpPostedTaskCount(TaskKind::Worker);
            END_MUTEX_SCOPE

            auto wrIsolate = CreateWeakRef();
            HostObjectUtil::GetInstance().QueueNativeCallback([this, wrIsolate, wpTask] ()
            {
                auto spIsolate = wrIsolate.GetTarget();
                if (!spIsolate.IsEmpty())
                {
                    auto spTask = wpTask.lock();
                    if (spTask)
                    {
                        spTask->Run();

                        BEGIN_MUTEX_SCOPE(m_DataMutex)
                            auto it = std::remove(m_AsyncTasks.begin(), m_AsyncTasks.end(), spTask);
                            m_AsyncTasks.erase(it, m_AsyncTasks.end());
                            m_Statistics.BumpInvokedTaskCount(TaskKind::Worker);
                        END_MUTEX_SCOPE
                    }
                }
            });
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskDelayed(std::unique_ptr<v8::Task> upTask, double delayInSeconds)
{
    if (upTask && !m_Released)
    {
        std::shared_ptr<v8::Task> spTask(std::move(upTask));

        auto wrIsolate = CreateWeakRef();
        SharedPtr<Timer> spTimer(new Timer(static_cast<int>(delayInSeconds * 1000), -1, [this, wrIsolate, spTask] (Timer* pTimer) mutable
        {
            auto spIsolate = wrIsolate.GetTarget();
            if (!spIsolate.IsEmpty())
            {
                spTask->Run();

                // Release the timer's strong task reference. Doing so avoids a deadlock when
                // spIsolate's implicit destruction below triggers immediate isolate teardown.

                spTask.reset();

                // the timer has fired; discard it

                BEGIN_MUTEX_SCOPE(m_DataMutex)
                    auto it = std::remove(m_TaskTimers.begin(), m_TaskTimers.end(), SharedPtr<Timer>(pTimer));
                    m_TaskTimers.erase(it, m_TaskTimers.end());
                    m_Statistics.BumpInvokedTaskCount(TaskKind::DelayedWorker);
                END_MUTEX_SCOPE
            }
            else
            {
                // Release the timer's strong task reference. Doing so avoids a deadlock if the
                // isolate is awaiting task completion on the managed finalization thread.

                spTask.reset();
            }
        }));

        // hold on to the timer to ensure callback execution

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            m_TaskTimers.push_back(spTimer);
            m_Statistics.BumpPostedTaskCount(TaskKind::DelayedWorker);
        END_MUTEX_SCOPE

        // Release the local task reference explicitly. Doing so avoids a deadlock if the callback is
        // executed synchronously. That shouldn't happen given the current timer implementation.

        spTask.reset();

        // now it's safe to start the timer

        spTimer->Start();
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskWithLockAsync(bool allowNesting, std::unique_ptr<v8::Task> upTask)
{
    if (upTask)
    {
        if (m_Released)
        {
            if (allowNesting)
            {
                upTask->Run();
            }
        }
        else
        {
            std::shared_ptr<v8::Task> spTask(std::move(upTask));
            CallWithLockAsync(allowNesting, [allowNesting, spTask] (V8IsolateImpl* pIsolateImpl)
            {
                spTask->Run();

                BEGIN_MUTEX_SCOPE(pIsolateImpl->m_DataMutex)
                    pIsolateImpl->m_Statistics.BumpInvokedTaskCount(allowNesting ? TaskKind::Foreground : TaskKind::NonNestableForeground);
                END_MUTEX_SCOPE
            });

            BEGIN_MUTEX_SCOPE(m_DataMutex)
                m_Statistics.BumpPostedTaskCount(allowNesting ? TaskKind::Foreground : TaskKind::NonNestableForeground);
            END_MUTEX_SCOPE
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskWithLockDelayed(bool allowNesting, std::unique_ptr<v8::Task> upTask, double delayInSeconds)
{
    if (upTask && !m_Released)
    {
        std::shared_ptr<v8::Task> spTask(std::move(upTask));

        auto wrIsolate = CreateWeakRef();
        SharedPtr<Timer> spTimer(new Timer(static_cast<int>(delayInSeconds * 1000), -1, [this, wrIsolate, allowNesting, spTask] (Timer* pTimer) mutable
        {
            auto spIsolate = wrIsolate.GetTarget();
            if (!spIsolate.IsEmpty())
            {
                CallWithLockNoWait(allowNesting, [allowNesting, spTask] (V8IsolateImpl* pIsolateImpl)
                {
                    spTask->Run();

                    BEGIN_MUTEX_SCOPE(pIsolateImpl->m_DataMutex)
                        pIsolateImpl->m_Statistics.BumpInvokedTaskCount(allowNesting ? TaskKind::DelayedForeground : TaskKind::NonNestableDelayedForeground);
                    END_MUTEX_SCOPE
                });

                // Release the timer's strong task reference. Doing so avoids a deadlock when
                // spIsolate's implicit destruction below triggers immediate isolate teardown.

                spTask.reset();

                // the timer has fired; discard it

                BEGIN_MUTEX_SCOPE(m_DataMutex)
                    auto it = std::remove(m_TaskTimers.begin(), m_TaskTimers.end(), pTimer);
                    m_TaskTimers.erase(it, m_TaskTimers.end());
                END_MUTEX_SCOPE
            }
            else
            {
                // Release the timer's strong task reference. Doing so avoids a deadlock if the
                // isolate is awaiting task completion on the managed finalization thread.

                spTask.reset();
            }
        }));

        // hold on to the timer to ensure callback execution

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            m_TaskTimers.push_back(spTimer);
            m_Statistics.BumpPostedTaskCount(allowNesting ? TaskKind::DelayedForeground : TaskKind::NonNestableDelayedForeground);
        END_MUTEX_SCOPE

        // Release the local task reference explicitly. Doing so avoids a deadlock if the callback is
        // executed synchronously. That shouldn't happen given the current timer implementation.

        spTask.reset();

        // now it's safe to start the timer

        spTimer->Start();
    }
}

//-----------------------------------------------------------------------------

std::shared_ptr<v8::TaskRunner> V8IsolateImpl::GetForegroundTaskRunner()
{
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        if (!m_spForegroundTaskRunner)
        {
            m_spForegroundTaskRunner = std::make_shared<V8ForegroundTaskRunner>(*this);
        }

        return m_spForegroundTaskRunner;

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void* V8IsolateImpl::AllocateArrayBuffer(size_t size)
{
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        auto newArrayBufferAllocation = m_ArrayBufferAllocation + size;
        if ((newArrayBufferAllocation >= m_ArrayBufferAllocation) && (newArrayBufferAllocation <= m_MaxArrayBufferAllocation))
        {
            auto pvData = ::calloc(1, size);
            if (pvData)
            {
                m_ArrayBufferAllocation = newArrayBufferAllocation;
                return pvData;
            }
        }

        return nullptr;

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void* V8IsolateImpl::AllocateUninitializedArrayBuffer(size_t size)
{
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        auto newArrayBufferAllocation = m_ArrayBufferAllocation + size;
        if ((newArrayBufferAllocation >= m_ArrayBufferAllocation) && (newArrayBufferAllocation <= m_MaxArrayBufferAllocation))
        {
            auto pvData = ::malloc(size);
            if (pvData)
            {
                m_ArrayBufferAllocation = newArrayBufferAllocation;
                return pvData;
            }
        }

        return nullptr;

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::FreeArrayBuffer(void* pvData, size_t size)
{
    BEGIN_MUTEX_SCOPE(m_DataMutex)

        if (pvData)
        {
            ::free(pvData);
            if (m_ArrayBufferAllocation >= size)
            {
                m_ArrayBufferAllocation -= size;
            }
        }

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CallWithLockNoWait(bool allowNesting, CallWithLockCallback&& callback)
{
    if (callback)
    {
        if (m_Mutex.TryLock())
        {
            // the callback may release this instance; hold it for destruction outside isolate scope
            SharedPtr<V8IsolateImpl> spThis(this);

            MutexLock<RecursiveMutex> lock(m_Mutex, false);
            if (allowNesting || (m_CallWithLockLevel < 1))
            {
                BEGIN_ISOLATE_NATIVE_SCOPE
                BEGIN_PULSE_VALUE_SCOPE(&m_CallWithLockLevel, m_CallWithLockLevel + 1)

                    callback(this);
                    return;

                END_PULSE_VALUE_SCOPE
                END_ISOLATE_NATIVE_SCOPE
            }
        }

        CallWithLockAsync(allowNesting, std::move(callback));
    }
}

//-----------------------------------------------------------------------------

void NORETURN V8IsolateImpl::ThrowOutOfMemoryException()
{
    m_IsOutOfMemory = true;
    throw V8Exception(V8Exception::Type::Fatal, m_Name, StdString(SL("The V8 runtime has exceeded its memory limit")), ExecutionStarted());
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ImportMetaInitializeCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta)
{
    GetInstanceFromIsolate(hContext->GetIsolate())->InitializeImportMeta(hContext, hModule, hMeta);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Promise> V8IsolateImpl::ModuleImportCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> hResourceName, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> hImportAssertions)
{
    return GetInstanceFromIsolate(hContext->GetIsolate())->ImportModule(hContext, hHostDefinedOptions, hResourceName, hSpecifier, hImportAssertions);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Module> V8IsolateImpl::ModuleResolveCallback(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> /*importAssertions*/, v8::Local<v8::Module> hReferrer)
{
    return GetInstanceFromIsolate(hContext->GetIsolate())->ResolveModule(hContext, hSpecifier, hReferrer);
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::InitializeImportMeta(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta)
{
    _ASSERTE(IsCurrent() && IsLocked());

    auto pContextImpl = FindContext(hContext);
    if (pContextImpl)
    {
        return pContextImpl->InitializeImportMeta(hContext, hModule, hMeta);
    }
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Promise> V8IsolateImpl::ImportModule(v8::Local<v8::Context> hContext, v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> hResourceName, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> hImportAssertions)
{
    _ASSERTE(IsCurrent() && IsLocked());

    auto pContextImpl = FindContext(hContext);
    if (pContextImpl)
    {
        return pContextImpl->ImportModule(hHostDefinedOptions, hResourceName, hSpecifier, hImportAssertions);
    }

    return v8::MaybeLocal<v8::Promise>();
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Module> V8IsolateImpl::ResolveModule(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer)
{
    _ASSERTE(IsCurrent() && IsLocked());

    auto pContextImpl = FindContext(hContext);
    if (pContextImpl)
    {
        return pContextImpl->ResolveModule(hSpecifier, hReferrer);
    }

    return v8::MaybeLocal<v8::Module>();
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::TryGetCachedScriptInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto it = m_ScriptCache.cbegin(); it != m_ScriptCache.cend(); it++)
    {
        if (it->DocumentInfo.GetUniqueId() == uniqueId)
        {
            m_ScriptCache.splice(m_ScriptCache.begin(), m_ScriptCache, it);
            documentInfo = it->DocumentInfo;
            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

v8::Local<v8::UnboundScript> V8IsolateImpl::GetCachedScript(uint64_t uniqueId, size_t codeDigest)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto it = m_ScriptCache.cbegin(); it != m_ScriptCache.cend(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ScriptCache.splice(m_ScriptCache.begin(), m_ScriptCache, it);
            return it->hScript;
        }
    }

    return v8::Local<v8::UnboundScript>();
}

//-----------------------------------------------------------------------------

v8::Local<v8::UnboundScript> V8IsolateImpl::GetCachedScript(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto it = m_ScriptCache.cbegin(); it != m_ScriptCache.cend(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ScriptCache.splice(m_ScriptCache.begin(), m_ScriptCache, it);
            cacheBytes = it->CacheBytes;
            return it->hScript;
        }
    }

    cacheBytes.clear();
    return v8::Local<v8::UnboundScript>();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript)
{
    CacheScript(documentInfo, codeDigest, hScript, std::vector<uint8_t>());
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript, const std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(IsCurrent() && IsLocked());

    auto maxScriptCacheSize = HostObjectUtil::GetInstance().GetMaxScriptCacheSize();
    while (m_ScriptCache.size() >= maxScriptCacheSize)
    {
        Dispose(m_ScriptCache.back().hScript);
        m_ScriptCache.pop_back();
    }

    _ASSERTE(std::none_of(m_ScriptCache.begin(), m_ScriptCache.end(),
        [&documentInfo, codeDigest] (const ScriptCacheEntry& entry)
    {
        return (entry.DocumentInfo.GetUniqueId() == documentInfo.GetUniqueId()) && (entry.CodeDigest == codeDigest);
    }));

    ScriptCacheEntry entry { documentInfo, codeDigest, CreatePersistent(hScript), cacheBytes };
    m_ScriptCache.push_front(std::move(entry));

    m_Statistics.ScriptCacheSize = m_ScriptCache.size();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetCachedScriptCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto it = m_ScriptCache.begin(); it != m_ScriptCache.end(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ScriptCache.splice(m_ScriptCache.begin(), m_ScriptCache, it);
            it->CacheBytes = cacheBytes;
            return;
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ClearScriptCache()
{
    _ASSERTE(IsCurrent() && IsLocked());

    while (!m_ScriptCache.empty())
    {
        Dispose(m_ScriptCache.front().hScript);
        m_ScriptCache.pop_front();
    }

    m_Statistics.ScriptCacheSize = m_ScriptCache.size();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::TerminateExecutionInternal()
{
    if (!m_IsExecutionTerminating)
    {
        m_upIsolate->TerminateExecution();
        m_IsExecutionTerminating = true;
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CancelTerminateExecutionInternal()
{
    if (m_IsExecutionTerminating)
    {
        m_upIsolate->CancelTerminateExecution();
        m_IsExecutionTerminating = false;
    }
}

//-----------------------------------------------------------------------------

V8IsolateImpl::~V8IsolateImpl()
{
    --s_InstanceCount;
    m_Released = true;

    // Entering the isolate scope triggers call-with-lock queue processing. It should always be
    // done here, if for no other reason than that it may prevent deadlocks in V8 isolate disposal.

    BEGIN_ISOLATE_SCOPE
        DisableDebugging();
        ClearScriptCache();
    END_ISOLATE_SCOPE

    {
        std::vector<std::shared_ptr<v8::Task>> asyncTasks;
        std::vector<SharedPtr<Timer>> taskTimers;

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            std::swap(asyncTasks, m_AsyncTasks);
            std::swap(taskTimers, m_TaskTimers);
        END_MUTEX_SCOPE

        for (const auto& spTask : asyncTasks)
        {
            spTask->Run();
        }
    }

    Dispose(m_hHostObjectHolderKey);

    m_upIsolate->SetHostImportModuleDynamicallyCallback(static_cast<v8::HostImportModuleDynamicallyCallback>(nullptr));
    m_upIsolate->SetHostInitializeImportMetaObjectCallback(nullptr);

    m_upIsolate->RemoveBeforeCallEnteredCallback(OnBeforeCallEntered);
    m_upIsolate->RemoveNearHeapLimitCallback(HeapExpansionCallback, 0);
}

//-----------------------------------------------------------------------------

V8IsolateImpl::ExitMessageLoopReason V8IsolateImpl::RunMessageLoop(RunMessageLoopReason reason)
{
    _ASSERTE(IsCurrent() && IsLocked());

    std::unique_lock<std::mutex> lock(m_DataMutex.GetImpl());

    if (!m_optRunMessageLoopReason)
    {
        m_optExitMessageLoopReason.reset();

        BEGIN_PULSE_VALUE_SCOPE(&m_optRunMessageLoopReason, reason)

            ProcessCallWithLockQueue(lock);

            while (true)
            {
                m_CallWithLockQueueChanged.wait(lock);
                ProcessCallWithLockQueue(lock);

                if (m_optExitMessageLoopReason)
                {
                    break;
                }
            }

        END_PULSE_VALUE_SCOPE

        ProcessCallWithLockQueue(lock);
        return *m_optExitMessageLoopReason;
    }

    return ExitMessageLoopReason::NestedInvocation;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CallWithLockAsync(bool allowNesting, CallWithLockCallback&& callback)
{
    if (callback)
    {
        BEGIN_MUTEX_SCOPE(m_DataMutex)

            m_CallWithLockQueue.push(std::make_pair(allowNesting, std::move(callback)));

            if (m_optRunMessageLoopReason)
            {
                m_CallWithLockQueueChanged.notify_one();
                return;
            }

            if (m_CallWithLockQueue.size() > 1)
            {
                return;
            }

        END_MUTEX_SCOPE

        // trigger asynchronous queue processing

        auto wrIsolate = CreateWeakRef();
        HostObjectUtil::GetInstance().QueueNativeCallback([this, wrIsolate] ()
        {
            auto spIsolate = wrIsolate.GetTarget();
            if (!spIsolate.IsEmpty())
            {
                if (m_Mutex.TryLock())
                {
                    MutexLock<RecursiveMutex> lock(m_Mutex, false);
                    BEGIN_ISOLATE_NATIVE_SCOPE
                        // do nothing; scope entry triggers automatic queue processing
                    END_ISOLATE_NATIVE_SCOPE
                }
                else
                {
                    // The isolate is active on another thread, and the queue will be processed automatically
                    // at scope exit, but an interrupt ensures relatively timely processing.

                    RequestInterrupt(ProcessCallWithLockQueue, this);
                }
            }
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue(v8::Isolate* /*pIsolate*/, void* pvIsolateImpl)
{
    static_cast<V8IsolateImpl*>(pvIsolateImpl)->ProcessCallWithLockQueue();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue()
{
    BEGIN_PROMISE_HOOK_SCOPE

        std::unique_lock<std::mutex> lock(m_DataMutex.GetImpl());
        ProcessCallWithLockQueue(lock);

    END_PROMISE_HOOK_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue(std::unique_lock<std::mutex>& lock)
{
    _ASSERTE(lock.mutex() == &m_DataMutex.GetImpl());
    _ASSERTE(lock.owns_lock());

    CallWithLockQueue callWithLockQueue(PopCallWithLockQueue(lock));
    while (!callWithLockQueue.empty())
    {
        lock.unlock();
        ProcessCallWithLockQueue(callWithLockQueue);
        lock.lock();
        callWithLockQueue = PopCallWithLockQueue(lock);
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue(CallWithLockQueue& callWithLockQueue)
{
    _ASSERTE(IsCurrent() && IsLocked());

    BEGIN_PULSE_VALUE_SCOPE(&m_CallWithLockLevel, m_CallWithLockLevel + 1)

        while (!callWithLockQueue.empty())
        {
            try
            {
                callWithLockQueue.front().second(this);
            }
            catch (...)
            {
            }

            callWithLockQueue.pop();
        }

    END_PULSE_VALUE_SCOPE
}

//-----------------------------------------------------------------------------

V8IsolateImpl::CallWithLockQueue V8IsolateImpl::PopCallWithLockQueue(const std::unique_lock<std::mutex>& lock)
{
    _ASSERTE(IsCurrent() && IsLocked());
    _ASSERTE(lock.mutex() == &m_DataMutex.GetImpl());
    _ASSERTE(lock.owns_lock());
    IGNORE_UNUSED(lock);

    if (m_CallWithLockLevel < 1)
    {
        return std::move(m_CallWithLockQueue);
    }

    CallWithLockQueue nestableCallWithLockQueue;
    CallWithLockQueue nonNestableCallWithLockQueue;

    while (!m_CallWithLockQueue.empty())
    {
        auto& callWithLockEntry = m_CallWithLockQueue.front();
        auto& callWithLockQueue = callWithLockEntry.first ? nestableCallWithLockQueue : nonNestableCallWithLockQueue;
        callWithLockQueue.push(std::move(callWithLockEntry));
        m_CallWithLockQueue.pop();
    }

    m_CallWithLockQueue = std::move(nonNestableCallWithLockQueue);
    return nestableCallWithLockQueue;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ConnectDebugClient()
{
    CallWithLockNoWait(true /*allowNesting*/, [] (V8IsolateImpl* pIsolateImpl)
    {
        if (pIsolateImpl->m_upInspector && !pIsolateImpl->m_upInspectorSession)
        {
            pIsolateImpl->m_upInspectorSession = pIsolateImpl->m_upInspector->connect(s_ContextGroupId, pIsolateImpl, v8_inspector::StringView(), v8_inspector::V8Inspector::ClientTrustLevel::kFullyTrusted);
        }
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SendDebugCommand(const StdString& command)
{
    CallWithLockNoWait(true /*allowNesting*/, [command] (V8IsolateImpl* pIsolateImpl)
    {
        if (pIsolateImpl->m_upInspectorSession)
        {
            pIsolateImpl->m_upInspectorSession->dispatchProtocolMessage(command.GetStringView());
        }
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::DisconnectDebugClient()
{
    CallWithLockNoWait(true /*allowNesting*/, [] (V8IsolateImpl* pIsolateImpl)
    {
        pIsolateImpl->m_upInspectorSession.reset();
    });
}

//-----------------------------------------------------------------------------

V8IsolateImpl::ExecutionScope* V8IsolateImpl::EnterExecutionScope(ExecutionScope* pExecutionScope, size_t* pStackMarker)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // is heap size monitoring in progress?
    if (m_HeapWatchLevel == 0)
    {
        // no; there should be no heap watch timer
        _ASSERTE(m_spHeapWatchTimer.IsEmpty());

        // is a heap size limit specified?
        size_t maxHeapSize = m_MaxHeapSize;
        if (maxHeapSize > 0)
        {
            // yes; perform initial check and set up heap watch timer
            CheckHeapSize(maxHeapSize, false /*timerTriggered*/);

            // enter outermost heap size monitoring scope
            m_HeapWatchLevel = 1;
        }
    }
    else
    {
        // heap size monitoring in progress; enter nested scope
        m_HeapWatchLevel++;
    }

    // is stack usage monitoring in progress?
    if (m_StackWatchLevel == 0)
    {
        // no; there should be no stack address limit
        _ASSERTE(m_pStackLimit == nullptr);

        // is a stack usage limit specified?
        size_t maxStackUsage = m_MaxStackUsage;
        if (maxStackUsage > 0)
        {
            // yes; ensure minimum breathing room
            maxStackUsage = std::max(maxStackUsage, s_StackBreathingRoom);

            // calculate stack address limit
            size_t* pStackLimit = pStackMarker - (maxStackUsage / sizeof(size_t));
            if ((pStackLimit < s_pMinStackLimit) || (pStackLimit > pStackMarker))
            {
                // underflow; use minimum non-null stack address
                pStackLimit = s_pMinStackLimit;
            }
            else
            {
                // check stack address limit sanity
                _ASSERTE(static_cast<size_t>(pStackMarker - pStackLimit) >= (s_StackBreathingRoom / sizeof(size_t)));
            }

            // set and record stack address limit
            m_upIsolate->SetStackLimit(reinterpret_cast<uintptr_t>(pStackLimit));
            m_pStackLimit = pStackLimit;

            // enter outermost stack usage monitoring scope
            m_StackWatchLevel = 1;
        }
    }
    else
    {
        // stack usage monitoring in progress
        if ((m_pStackLimit != nullptr) && (pStackMarker < m_pStackLimit))
        {
            // stack usage limit exceeded (host-side detection)
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime has exceeded its stack usage limit")), false /*executionStarted*/);
        }

        // enter nested stack usage monitoring scope
        m_StackWatchLevel++;
    }

    // mark execution scope
    return SetExecutionScope(pExecutionScope);
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ExitExecutionScope(ExecutionScope* pPreviousExecutionScope)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // reset execution scope
    SetExecutionScope(pPreviousExecutionScope);

    // is interrupt propagation enabled?
    if (!m_EnableInterruptPropagation)
    {
        // no; cancel termination to allow remaining script frames to execute
        CancelTerminateExecution();
    }

    // is stack usage monitoring in progress?
    if (m_StackWatchLevel > 0)
    {
        // yes; exit stack usage monitoring scope
        if (--m_StackWatchLevel == 0)
        {
            // exited outermost scope; remove stack address limit
            if (m_pStackLimit != nullptr)
            {
                // V8 has no API for removing a stack address limit
                m_upIsolate->SetStackLimit(reinterpret_cast<uintptr_t>(s_pMinStackLimit));
                m_pStackLimit = nullptr;
            }
        }
    }

    // is heap size monitoring in progress?
    if (m_HeapWatchLevel > 0)
    {
        // yes; exit heap size monitoring scope
        if (--m_HeapWatchLevel == 0)
        {
            // exited outermost scope; destroy heap watch timer
            m_spHeapWatchTimer.Empty();
        }
    }
}


//-----------------------------------------------------------------------------

V8IsolateImpl::ExecutionScope* V8IsolateImpl::SetExecutionScope(ExecutionScope* pExecutionScope)
{
    BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)

        auto pPrevExecutionScope = std::exchange(m_pExecutionScope, pExecutionScope);

        if (pExecutionScope == nullptr)
        {
            CancelTerminateExecutionInternal();
        }

        return pPrevExecutionScope;

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::InExecutionScope()
{
    BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)
        return m_pExecutionScope != nullptr;
    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::OnExecutionStarted()
{
    BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)

        if (m_pExecutionScope != nullptr)
        {
            m_pExecutionScope->OnExecutionStarted();
        }

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::ExecutionStarted()
{
    BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)
        return (m_pExecutionScope != nullptr) ? m_pExecutionScope->ExecutionStarted() : false;
    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SetUpHeapWatchTimer(bool forceMinInterval)
{
    _ASSERTE(IsCurrent() && IsLocked());

    const auto minInterval = 50.0;
    auto interval = forceMinInterval ? minInterval : std::max(GetHeapSizeSampleInterval(), minInterval);

    // create heap watch timer
    auto wrIsolate = CreateWeakRef();
    m_spHeapWatchTimer = new Timer(static_cast<int>(interval), -1, [this, wrIsolate] (Timer* pTimer)
    {
        // heap watch callback; is the isolate still alive?
        auto spIsolate = wrIsolate.GetTarget();
        if (!spIsolate.IsEmpty())
        {
            // yes; request callback on execution thread
            auto wrTimer = pTimer->CreateWeakRef();
            CallWithLockAsync(true /*allowNesting*/, [wrTimer] (V8IsolateImpl* pIsolateImpl)
            {
                // execution thread callback; is the timer still alive?
                auto spTimer = wrTimer.GetTarget();
                if (!spTimer.IsEmpty())
                {
                    // yes; check heap size
                    pIsolateImpl->CheckHeapSize(std::nullopt, true /*timerTriggered*/);
                }
            });
        }
    });

    // start heap watch timer
    m_spHeapWatchTimer->Start();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CheckHeapSize(const std::optional<size_t>& optMaxHeapSize, bool timerTriggered)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // do we have a heap size limit?
    auto maxHeapSize = optMaxHeapSize.has_value() ? optMaxHeapSize.value() : m_MaxHeapSize.load();
    if (maxHeapSize > 0)
    {
        // yes; use normal heap watch timer interval by default
        auto forceMinInterval = false;

        // is the total heap size over the limit?
        v8::HeapStatistics heapStatistics;
        GetHeapStatistics(heapStatistics);
        if (heapStatistics.total_heap_size() > maxHeapSize)
        {
            // yes; collect garbage
            ClearCachesForTesting();
            RequestGarbageCollectionForTesting(v8::Isolate::kFullGarbageCollection);

            // is the total heap size still over the limit?
            GetHeapStatistics(heapStatistics);
            if (heapStatistics.total_heap_size() > maxHeapSize)
            {
                // yes; the isolate is out of memory; act based on policy
                if (m_DisableHeapSizeViolationInterrupt)
                {
                    if (InExecutionScope())
                    {
                        m_MaxHeapSize = 0;
                        m_upIsolate->ThrowError("The V8 runtime has exceeded its memory limit");
                        return;
                    }

                    // defer exception until code execution is in progress
                    forceMinInterval = true;
                }
                else
                {
                    m_IsOutOfMemory = true;
                    TerminateExecution();
                    return;
                }
            }
        }

        // the isolate is not out of memory; is heap size monitoring in progress?
        if (!timerTriggered || (m_HeapWatchLevel > 0))
        {
            // yes; restart heap watch timer
            SetUpHeapWatchTimer(forceMinInterval);
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::OnBeforeCallEntered(v8::Isolate* pIsolate)
{
    GetInstanceFromIsolate(pIsolate)->OnBeforeCallEntered();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::OnBeforeCallEntered()
{
    _ASSERTE(IsCurrent() && IsLocked());
    OnExecutionStarted();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::PromiseHook(v8::PromiseHookType type, v8::Local<v8::Promise> hPromise, v8::Local<v8::Value> /*hParent*/)
{
    if ((type == v8::PromiseHookType::kResolve) && !hPromise.IsEmpty())
    {
        auto hContext = hPromise->GetCreationContext().FromMaybe(v8::Local<v8::Context>());
        if (!hContext.IsEmpty())
        {
            GetInstanceFromIsolate(hContext->GetIsolate())->FlushContextAsync(hContext);
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::FlushContextAsync(v8::Local<v8::Context> hContext)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto& contextEntry : m_ContextEntries)
    {
        if (contextEntry.pContextImpl->GetContext() == hContext)
        {
            FlushContextAsync(contextEntry);
            break;
        }
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::FlushContextAsync(ContextEntry& contextEntry)
{
    auto expected = false;
    if (contextEntry.FlushPending.compare_exchange_strong(expected, true))
    {
        auto wrContext = contextEntry.pContextImpl->CreateWeakRef();
        CallWithLockAsync(true /*allowNesting*/, [wrContext] (V8IsolateImpl* pIsolateImpl)
        {
            auto spContext = wrContext.GetTarget();
            if (!spContext.IsEmpty())
            {
                pIsolateImpl->FlushContext(spContext.DerefAs<V8ContextImpl>());
            }
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::FlushContext(V8ContextImpl& contextImpl)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto& contextEntry : m_ContextEntries)
    {
        if (contextEntry.pContextImpl == &contextImpl)
        {
            contextEntry.FlushPending = false;
            break;
        }
    }

    contextImpl.Flush();
}

//-----------------------------------------------------------------------------

size_t V8IsolateImpl::HeapExpansionCallback(void* pvData, size_t currentLimit, size_t /*initialLimit*/)
{
    const size_t minBump = 1024 * 1024;

    if (pvData)
    {
        auto multiplier = static_cast<const V8IsolateImpl*>(pvData)->m_HeapExpansionMultiplier;
        if (multiplier > 1.0)
        {
            auto newLimit = static_cast<size_t>(static_cast<double>(currentLimit) * multiplier);
            return std::max(newLimit, currentLimit + minBump);
        }
    }

    return currentLimit;
}
