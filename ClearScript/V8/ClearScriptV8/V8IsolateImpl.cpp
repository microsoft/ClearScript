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
    static void EnsureInstalled();

    virtual int NumberOfWorkerThreads() override;
    virtual std::shared_ptr<v8::TaskRunner> GetForegroundTaskRunner(v8::Isolate* pIsolate) override;
    virtual void CallOnWorkerThread(std::unique_ptr<v8::Task> spTask) override;
    virtual void CallDelayedOnWorkerThread(std::unique_ptr<v8::Task> spTask, double delayInSeconds) override;
    virtual void CallOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask) override;
    virtual void CallDelayedOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask, double delayInSeconds) override;
    virtual double MonotonicallyIncreasingTime() override;
    virtual double CurrentClockTimeMillis() override;
    virtual v8::TracingController* GetTracingController() override;

private:

    V8Platform();

    static V8Platform ms_Instance;
    static OnceFlag ms_InstallationFlag;
    v8::TracingController m_TracingController;
};

//-----------------------------------------------------------------------------

V8Platform& V8Platform::GetInstance()
{
    return ms_Instance;
}

//-----------------------------------------------------------------------------

void V8Platform::EnsureInstalled()
{
    ms_InstallationFlag.CallOnce([]
    {
        v8::V8::InitializePlatform(&ms_Instance);
        ASSERT_EVAL(v8::V8::Initialize());
    });
}

//-----------------------------------------------------------------------------

int V8Platform::NumberOfWorkerThreads()
{
    return static_cast<int>(HighResolutionClock::GetHardwareConcurrency());
}

//-----------------------------------------------------------------------------

std::shared_ptr<v8::TaskRunner> V8Platform::GetForegroundTaskRunner(v8::Isolate* pIsolate)
{
    return V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->GetForegroundTaskRunner();
}

//-----------------------------------------------------------------------------

void V8Platform::CallOnWorkerThread(std::unique_ptr<v8::Task> spTask)
{
    auto pIsolate = v8::Isolate::GetCurrent();
    if (pIsolate == nullptr)
    {
        spTask->Run();
    }
    else
    {
        V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskAsync(spTask.release());
    }
}

//-----------------------------------------------------------------------------

void V8Platform::CallDelayedOnWorkerThread(std::unique_ptr<v8::Task> spTask, double delayInSeconds)
{
    auto pIsolate = v8::Isolate::GetCurrent();
    if (pIsolate != nullptr)
    {
        V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskDelayed(spTask.release(), delayInSeconds);
    }
}

//-----------------------------------------------------------------------------

void V8Platform::CallOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask)
{
    V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskWithLockAsync(pTask);
}

//-----------------------------------------------------------------------------

void V8Platform::CallDelayedOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask, double delayInSeconds)
{
    V8IsolateImpl::GetInstanceFromIsolate(pIsolate)->RunTaskWithLockDelayed(pTask, delayInSeconds);
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

V8Platform::V8Platform()
{
}

//-----------------------------------------------------------------------------

V8Platform V8Platform::ms_Instance;
OnceFlag V8Platform::ms_InstallationFlag;

//-----------------------------------------------------------------------------
// V8ForegroundTaskRunner
//-----------------------------------------------------------------------------

class V8ForegroundTaskRunner final: public v8::TaskRunner
{
    PROHIBIT_COPY(V8ForegroundTaskRunner)

public:

    V8ForegroundTaskRunner(V8IsolateImpl* pIsolateImpl);

    virtual void PostTask(std::unique_ptr<v8::Task> spTask) override;
    virtual void PostDelayedTask(std::unique_ptr<v8::Task> spTask, double delayInSeconds) override;
    virtual void PostIdleTask(std::unique_ptr<v8::IdleTask> spTask) override;
    virtual bool IdleTasksEnabled() override;

private:

    V8IsolateImpl* m_pIsolateImpl;
    WeakRef<V8Isolate> m_wrIsolate;
};

//-----------------------------------------------------------------------------

V8ForegroundTaskRunner::V8ForegroundTaskRunner(V8IsolateImpl* pIsolateImpl):
    m_pIsolateImpl(pIsolateImpl),
    m_wrIsolate(pIsolateImpl->CreateWeakRef())
{
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostTask(std::unique_ptr<v8::Task> spTask)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (spIsolate.IsEmpty())
    {
        spTask->Run();
    }
    else
    {
        m_pIsolateImpl->RunTaskWithLockAsync(spTask.release());
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostDelayedTask(std::unique_ptr<v8::Task> spTask, double delayInSeconds)
{
    auto spIsolate = m_wrIsolate.GetTarget();
    if (!spIsolate.IsEmpty())
    {
        m_pIsolateImpl->RunTaskWithLockDelayed(spTask.release(), delayInSeconds);
    }
}

//-----------------------------------------------------------------------------

void V8ForegroundTaskRunner::PostIdleTask(std::unique_ptr<v8::IdleTask> spTask)
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
// V8ArrayBufferAllocator
//-----------------------------------------------------------------------------

class V8ArrayBufferAllocator final: public v8::ArrayBuffer::Allocator
{
public:

    static V8ArrayBufferAllocator& GetInstance();

    virtual void* Allocate(size_t size) override;
    virtual void* AllocateUninitialized(size_t size) override;
    virtual void Free(void* pvData, size_t size) override;

private:

    V8ArrayBufferAllocator();

    static V8ArrayBufferAllocator ms_Instance;
};

//-----------------------------------------------------------------------------

V8ArrayBufferAllocator& V8ArrayBufferAllocator::GetInstance()
{
    return ms_Instance;
}

//-----------------------------------------------------------------------------

void* V8ArrayBufferAllocator::Allocate(size_t size)
{
    return ::calloc(1, size);
}

//-----------------------------------------------------------------------------

void* V8ArrayBufferAllocator::AllocateUninitialized(size_t size)
{
    return ::malloc(size);
}

//-----------------------------------------------------------------------------

void V8ArrayBufferAllocator::Free(void* pvData, size_t /*size*/)
{
    ::free(pvData);
}

//-----------------------------------------------------------------------------

V8ArrayBufferAllocator::V8ArrayBufferAllocator()
{
}

//-----------------------------------------------------------------------------

V8ArrayBufferAllocator V8ArrayBufferAllocator::ms_Instance;

//-----------------------------------------------------------------------------
// V8IsolateImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_NATIVE_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        NativeScope t_IsolateNativeScope(this); \
    __pragma(warning(default:4456))

#define END_ISOLATE_NATIVE_SCOPE \
        IGNORE_UNUSED(t_IsolateNativeScope); \
    }

#define BEGIN_ISOLATE_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        Scope t_IsolateScope(this); \
    __pragma(warning(default:4456))

#define END_ISOLATE_SCOPE \
        IGNORE_UNUSED(t_IsolateScope); \
    }

//-----------------------------------------------------------------------------

static std::atomic<size_t> s_InstanceCount(0);
static const int s_ContextGroupId = 1;
static const size_t s_StackBreathingRoom = static_cast<size_t>(16 * 1024);
static size_t* const s_pMinStackLimit = reinterpret_cast<size_t*>(sizeof(size_t));
static const size_t s_MaxScriptCacheSize = 256;

//-----------------------------------------------------------------------------

V8IsolateImpl::V8IsolateImpl(const StdString& name, const v8::ResourceConstraints* pConstraints, const Options& options):
    m_Name(name),
    m_DebuggingEnabled(false),
    m_AwaitingDebugger(false),
    m_InMessageLoop(false),
    m_QuitMessageLoop(false),
    m_AbortMessageLoop(false),
    m_MaxHeapSize(0),
    m_HeapWatchLevel(0),
    m_MaxStackUsage(0),
    m_CpuProfileSampleInterval(1000U),
    m_StackWatchLevel(0),
    m_pStackLimit(nullptr),
    m_pExecutionScope(nullptr),
    m_pDocumentInfo(nullptr),
    m_IsOutOfMemory(false),
    m_IsExecutionTerminating(false),
    m_Released(false)
{
    V8Platform::EnsureInstalled();

    v8::Isolate::CreateParams params;
    params.array_buffer_allocator = &V8ArrayBufferAllocator::GetInstance();
    if (pConstraints != nullptr)
    {
        params.constraints.set_max_semi_space_size_in_kb(pConstraints->max_semi_space_size_in_kb());
        params.constraints.set_max_old_space_size(pConstraints->max_old_space_size());
    }

    m_spIsolate.reset(v8::Isolate::Allocate());
    m_spIsolate->SetData(0, this);
    v8::Isolate::Initialize(m_spIsolate.get(), params);

    m_spIsolate->AddBeforeCallEnteredCallback(OnBeforeCallEntered);
    m_spIsolate->SetPromiseHook(PromiseHook);

    BEGIN_ADDREF_SCOPE
    BEGIN_ISOLATE_SCOPE

        m_spIsolate->SetCaptureStackTraceForUncaughtExceptions(true, 64, v8::StackTrace::kDetailed);

        m_hHostObjectHolderKey = CreatePersistent(CreatePrivate());

        if (options.EnableDebugging)
        {
            EnableDebugging(options.DebugPort, options.EnableRemoteDebugging);
        }

        m_spIsolate->SetHostInitializeImportMetaObjectCallback(ImportMetaInitializeCallback);
        if (options.EnableDynamicModuleImports)
        {
            m_spIsolate->SetHostImportModuleDynamicallyCallback(ModuleImportCallback);
        }

    END_ISOLATE_SCOPE
    END_ADDREF_SCOPE

    ++s_InstanceCount;
}

//-----------------------------------------------------------------------------

V8IsolateImpl* V8IsolateImpl::GetInstanceFromIsolate(v8::Isolate* pIsolate)
{
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

    if (!options.EnableDebugging)
    {
        m_ContextEntries.emplace_back(pContextImpl);
    }
    else
    {
        m_ContextEntries.emplace_front(pContextImpl);
        EnableDebugging(options.DebugPort, options.EnableRemoteDebugging);
    }

    if (options.EnableDynamicModuleImports)
    {
        m_spIsolate->SetHostImportModuleDynamicallyCallback(ModuleImportCallback);
    }

    if (m_spInspector)
    {
        m_spInspector->contextCreated(v8_inspector::V8ContextInfo(pContextImpl->GetContext(), s_ContextGroupId, pContextImpl->GetName().GetStringView()));
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RemoveContext(V8ContextImpl* pContextImpl)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (m_spInspector)
    {
        m_spInspector->contextDestroyed(pContextImpl->GetContext());
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
        m_pvDebugAgent = HostObjectHelpers::CreateDebugAgent(m_Name, version, port, remote, [this, wrIsolate] (HostObjectHelpers::DebugDirective directive, const StdString* pCommand)
        {
            auto spIsolate = wrIsolate.GetTarget();
            if (!spIsolate.IsEmpty())
            {
                if (directive == HostObjectHelpers::DebugDirective::ConnectClient)
                {
                    ConnectDebugClient();
                }
                else if ((directive == HostObjectHelpers::DebugDirective::SendCommand) && pCommand)
                {
                    SendDebugCommand(*pCommand);
                }
                else if (directive == HostObjectHelpers::DebugDirective::DisconnectClient)
                {
                    DisconnectDebugClient();
                }
            }
        });

        m_spInspector.reset(v8_inspector::V8Inspector::createPtr(m_spIsolate.get(), this));

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
        m_spInspectorSession.reset();
        m_spInspector.reset();

        HostObjectHelpers::DestroyDebugAgent(m_pvDebugAgent);
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

        if (m_DebuggingEnabled)
        {
            if (!m_spInspectorSession && !RunMessageLoop(true))
            {
                throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(L"Script execution interrupted by host while awaiting debugger connection"), false);
            }

            _ASSERTE(m_spInspectorSession);
            if (m_spInspectorSession)
            {
                StdString breakReason(L"Break on debugger connection");
                m_spInspectorSession->schedulePauseOnNextStatement(breakReason.GetStringView(), breakReason.GetStringView());
            }
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextEntries.size() > 0) ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextEntries.size() > 0) ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code), cacheType, cacheBytes);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextEntries.size() > 0) ? m_ContextEntries.front().pContextImpl : new V8ContextImpl(this, m_Name));
        return spContextImpl->Compile(documentInfo, std::move(code), cacheType, cacheBytes, cacheAccepted);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::GetHeapStatistics(v8::HeapStatistics& heapStatistics)
{
    BEGIN_ISOLATE_SCOPE

        m_spIsolate->GetHeapStatistics(&heapStatistics);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8Isolate::Statistics V8IsolateImpl::GetStatistics()
{
    BEGIN_ISOLATE_SCOPE

        return m_Statistics;

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CollectGarbage(bool exhaustive)
{
    BEGIN_ISOLATE_SCOPE

        if (exhaustive)
        {
            ClearScriptCache();
            LowMemoryNotification();
        }
        else
        {
            while (!IdleNotificationDeadline(V8Platform::GetInstance().MonotonicallyIncreasingTime() + 0.1));
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples)
{
    BEGIN_ISOLATE_SCOPE

        if (!m_spCpuProfiler)
        {
            m_spCpuProfiler.reset(v8::CpuProfiler::New(m_spIsolate.get()));
        }

        v8::Local<v8::String> hName;
        if (!CreateString(name).ToLocal(&hName))
        {
            return false;
        }

        m_spCpuProfiler->StartProfiling(hName, mode, recordSamples);
        return true;

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::EndCpuProfile(const StdString& name, CpuProfileCallbackT* pCallback, void* pvArg)
{
    BEGIN_ISOLATE_SCOPE

        if (!m_spCpuProfiler)
        {
            return false;
        }

        v8::Local<v8::String> hName;
        if (!CreateString(name).ToLocal(&hName))
        {
            return false;
        }

        UniqueDeletePtr<v8::CpuProfile> spProfile(m_spCpuProfiler->StopProfiling(hName));
        if (!spProfile)
        {
            return false;
        }

        if (pCallback)
        {
            pCallback(*spProfile, pvArg);
        }

        return true;

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CollectCpuProfileSample()
{
    BEGIN_ISOLATE_SCOPE

        v8::CpuProfiler::CollectSample(m_spIsolate.get());

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
            m_CpuProfileSampleInterval = std::min(std::max(value, 250U), static_cast<uint32_t>(INT_MAX));

            if (!m_spCpuProfiler)
            {
                m_spCpuProfiler.reset(v8::CpuProfiler::New(m_spIsolate.get()));
            }

            m_spCpuProfiler->SetSamplingInterval(static_cast<int>(m_CpuProfileSampleInterval));
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::runMessageLoopOnPause(int /*contextGroupId*/)
{
    RunMessageLoop(false);
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::quitMessageLoopOnPause()
{
    _ASSERTE(IsCurrent() && IsLocked());

    BEGIN_MUTEX_SCOPE(m_DataMutex)
        m_QuitMessageLoop = true;
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

    if (m_ContextEntries.size() > 0)
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

void V8IsolateImpl::sendResponse(int /*callId*/, std::unique_ptr<v8_inspector::StringBuffer> spMessage)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (m_pvDebugAgent)
    {
        HostObjectHelpers::SendDebugMessage(m_pvDebugAgent, StdString(spMessage->string()));
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::sendNotification(std::unique_ptr<v8_inspector::StringBuffer> spMessage)
{
    sendResponse(0, std::move(spMessage));
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
    CallWithLockNoWait([pvObject] (V8IsolateImpl* pIsolateImpl)
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
    CallWithLockNoWait([pvScript] (V8IsolateImpl* pIsolateImpl)
    {
        pIsolateImpl->Dispose(::HandleFromPtr<v8::Script>(pvScript));
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskAsync(v8::Task* pTask)
{
    if (m_Released)
    {
        pTask->Run();
        delete pTask;
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask);
        std::weak_ptr<v8::Task> wpTask(spTask);

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            m_AsyncTasks.push_back(std::move(spTask));
        END_MUTEX_SCOPE

        auto wrIsolate = CreateWeakRef();
        HostObjectHelpers::QueueNativeCallback([this, wrIsolate, wpTask] ()
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
                    END_MUTEX_SCOPE
                }
            }
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskDelayed(v8::Task* pTask, double delayInSeconds)
{
    if (m_Released)
    {
        delete pTask;
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask);

        auto wrIsolate = CreateWeakRef();
        SharedPtr<Timer> spTimer(new Timer(static_cast<int>(delayInSeconds * 1000), -1, [this, wrIsolate, spTask] (Timer* pTimer) mutable
        {
            if (spTask)
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
            }
        }));

        // hold on to the timer to ensure callback execution

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            m_TaskTimers.push_back(spTimer);
        END_MUTEX_SCOPE

        // Release the local task reference explicitly. Doing so avoids a deadlock if the callback is
        // executed synchronously. That shouldn't happen given the current timer implementation.

        spTask.reset();

        // now it's safe to start the timer

        spTimer->Start();
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskWithLockAsync(v8::Task* pTask)
{
    if (m_Released)
    {
        pTask->Run();
        delete pTask;
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask);
        CallWithLockAsync([spTask] (V8IsolateImpl* /*pIsolateImpl*/)
        {
            spTask->Run();
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskWithLockDelayed(v8::Task* pTask, double delayInSeconds)
{
    if (m_Released)
    {
        delete pTask;
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask);

        auto wrIsolate = CreateWeakRef();
        SharedPtr<Timer> spTimer(new Timer(static_cast<int>(delayInSeconds * 1000), -1, [this, wrIsolate, spTask] (Timer* pTimer) mutable
        {
            if (spTask)
            {
                auto spIsolate = wrIsolate.GetTarget();
                if (!spIsolate.IsEmpty())
                {
                    CallWithLockNoWait([spTask] (V8IsolateImpl* /*pIsolateImpl*/)
                    {
                        spTask->Run();
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
            }
        }));

        // hold on to the timer to ensure callback execution

        BEGIN_MUTEX_SCOPE(m_DataMutex)
            m_TaskTimers.push_back(spTimer);
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
            m_spForegroundTaskRunner = std::make_shared<V8ForegroundTaskRunner>(this);
        }

        return m_spForegroundTaskRunner;

    END_MUTEX_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CallWithLockNoWait(std::function<void(V8IsolateImpl*)>&& callback)
{
    if (m_Mutex.TryLock())
    {
        // the callback may release this instance; hold it for destruction outside isolate scope
        SharedPtr<V8IsolateImpl> spThis(this);

        MutexLock<RecursiveMutex> lock(m_Mutex, false);
        BEGIN_ISOLATE_NATIVE_SCOPE
            callback(this);
        END_ISOLATE_NATIVE_SCOPE
    }
    else
    {
        CallWithLockAsync(std::move(callback));
    }
}

//-----------------------------------------------------------------------------

void DECLSPEC_NORETURN V8IsolateImpl::ThrowOutOfMemoryException()
{
    m_IsOutOfMemory = true;
    throw V8Exception(V8Exception::Type::Fatal, m_Name, StdString(L"The V8 runtime has exceeded its memory limit"), (m_pExecutionScope != nullptr) ? m_pExecutionScope->ExecutionStarted() : false);
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ImportMetaInitializeCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta)
{
    GetInstanceFromIsolate(hContext->GetIsolate())->InitializeImportMeta(hContext, hModule, hMeta);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Promise> V8IsolateImpl::ModuleImportCallback(v8::Local<v8::Context> hContext, v8::Local<v8::ScriptOrModule> hReferrer, v8::Local<v8::String> hSpecifier)
{
    return GetInstanceFromIsolate(hContext->GetIsolate())->ImportModule(hContext, hReferrer, hSpecifier);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Module> V8IsolateImpl::ModuleResolveCallback(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer)
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

v8::MaybeLocal<v8::Promise> V8IsolateImpl::ImportModule(v8::Local<v8::Context> hContext, v8::Local<v8::ScriptOrModule> hReferrer, v8::Local<v8::String> hSpecifier)
{
    _ASSERTE(IsCurrent() && IsLocked());

    auto pContextImpl = FindContext(hContext);
    if (pContextImpl)
    {
        return pContextImpl->ImportModule(hReferrer, hSpecifier);
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

v8::Local<v8::UnboundScript> V8IsolateImpl::GetCachedScript(const V8DocumentInfo& documentInfo, size_t codeDigest)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto it = m_ScriptCache.begin(); it != m_ScriptCache.end(); ++it)
    {
        if ((it->DocumentInfo.GetUniqueId() == documentInfo.GetUniqueId()) && (it->CodeDigest == codeDigest))
        {
            m_ScriptCache.splice(m_ScriptCache.begin(), m_ScriptCache, it);
            return it->hScript;
        }
    }

    return v8::Local<v8::UnboundScript>();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript)
{
    _ASSERTE(IsCurrent() && IsLocked());

    while (m_ScriptCache.size() >= s_MaxScriptCacheSize)
    {
        Dispose(m_ScriptCache.back().hScript);
        m_ScriptCache.pop_back();
    }

    _ASSERTE(std::none_of(m_ScriptCache.begin(), m_ScriptCache.end(),
        [&documentInfo, codeDigest] (const ScriptCacheEntry& entry)
    {
        return (entry.DocumentInfo.GetUniqueId() == documentInfo.GetUniqueId()) && (entry.CodeDigest == codeDigest);
    }));

    ScriptCacheEntry entry { documentInfo, codeDigest, CreatePersistent(hScript) };
    m_ScriptCache.push_front(std::move(entry));

    m_Statistics.ScriptCacheSize = m_ScriptCache.size();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ClearScriptCache()
{
    _ASSERTE(IsCurrent() && IsLocked());

    while (m_ScriptCache.size() > 0)
    {
        Dispose(m_ScriptCache.front().hScript);
        m_ScriptCache.pop_front();
    }

    m_Statistics.ScriptCacheSize = m_ScriptCache.size();
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
    m_spIsolate->SetHostImportModuleDynamicallyCallback(nullptr);
    m_spIsolate->SetHostInitializeImportMetaObjectCallback(nullptr);
    m_spIsolate->SetPromiseHook(nullptr);
    m_spIsolate->RemoveBeforeCallEnteredCallback(OnBeforeCallEntered);
}

//-----------------------------------------------------------------------------

bool V8IsolateImpl::RunMessageLoop(bool awaitingDebugger)
{
    _ASSERTE(IsCurrent() && IsLocked());

    std::unique_lock<std::mutex> lock(m_DataMutex.GetImpl());

    if (!m_InMessageLoop)
    {
        m_QuitMessageLoop = false;
        m_AbortMessageLoop = false;

        BEGIN_PULSE_VALUE_SCOPE(&m_AwaitingDebugger, awaitingDebugger)
        BEGIN_PULSE_VALUE_SCOPE(&m_InMessageLoop, true)

            ProcessCallWithLockQueue(lock);

            while (true)
            {
                m_CallWithLockQueueChanged.wait(lock);
                ProcessCallWithLockQueue(lock);

                if (m_QuitMessageLoop || m_AbortMessageLoop)
                {
                    break;
                }
            }

        END_PULSE_VALUE_SCOPE
        END_PULSE_VALUE_SCOPE

        ProcessCallWithLockQueue(lock);
        return m_QuitMessageLoop;
    }

    return false;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CallWithLockAsync(std::function<void(V8IsolateImpl*)>&& callback)
{
    if (callback)
    {
        BEGIN_MUTEX_SCOPE(m_DataMutex)

            m_CallWithLockQueue.push(std::move(callback));

            if (m_InMessageLoop)
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
        HostObjectHelpers::QueueNativeCallback([this, wrIsolate] ()
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
    std::queue<std::function<void(V8IsolateImpl*)>> callWithLockQueue;

    while (true)
    {
        BEGIN_MUTEX_SCOPE(m_DataMutex)
            std::swap(callWithLockQueue, m_CallWithLockQueue);
        END_MUTEX_SCOPE

        if (callWithLockQueue.size() > 0)
        {
            ProcessCallWithLockQueue(callWithLockQueue);
            continue;
        }

        break;
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue(std::unique_lock<std::mutex>& lock)
{
    _ASSERTE(lock.owns_lock());

    std::queue<std::function<void(V8IsolateImpl*)>> callWithLockQueue(std::move(m_CallWithLockQueue));
    while (callWithLockQueue.size() > 0)
    {
        lock.unlock();
        ProcessCallWithLockQueue(callWithLockQueue);
        lock.lock();
        callWithLockQueue = std::move(m_CallWithLockQueue);
    }
}


//-----------------------------------------------------------------------------

void V8IsolateImpl::ProcessCallWithLockQueue(std::queue<std::function<void(V8IsolateImpl*)>>& callWithLockQueue)
{
    _ASSERTE(IsCurrent() && IsLocked());

    while (callWithLockQueue.size() > 0)
    {
        try
        {
            callWithLockQueue.front()(this);
        }
        catch (...)
        {
        }

        callWithLockQueue.pop();
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ConnectDebugClient()
{
    CallWithLockNoWait([] (V8IsolateImpl* pIsolateImpl)
    {
        if (pIsolateImpl->m_spInspector && !pIsolateImpl->m_spInspectorSession)
        {
            pIsolateImpl->m_spInspectorSession = pIsolateImpl->m_spInspector->connect(s_ContextGroupId, pIsolateImpl, v8_inspector::StringView());
        }
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::SendDebugCommand(const StdString& command)
{
    CallWithLockNoWait([command] (V8IsolateImpl* pIsolateImpl)
    {
        if (pIsolateImpl->m_spInspectorSession)
        {
            pIsolateImpl->m_spInspectorSession->dispatchProtocolMessage(command.GetStringView());
        }
    });
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::DisconnectDebugClient()
{
    CallWithLockNoWait([] (V8IsolateImpl* pIsolateImpl)
    {
        pIsolateImpl->m_spInspectorSession.reset();
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
            CheckHeapSize(maxHeapSize);

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
                _ASSERTE((pStackMarker - pStackLimit) >= (s_StackBreathingRoom / sizeof(size_t)));
            }

            // set and record stack address limit
            m_spIsolate->SetStackLimit(reinterpret_cast<uintptr_t>(pStackLimit));
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
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime has exceeded its stack usage limit"), false /*executionStarted*/);
        }

        // enter nested stack usage monitoring scope
        m_StackWatchLevel++;
    }

    // clear termination flag
    m_IsExecutionTerminating = false;

    // mark execution scope
    auto pPreviousExecutionScope = m_pExecutionScope;
    m_pExecutionScope = pExecutionScope;
    return pPreviousExecutionScope;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::ExitExecutionScope(ExecutionScope* pPreviousExecutionScope)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // reset execution scope
    m_pExecutionScope = pPreviousExecutionScope;

    // cancel termination to allow remaining script frames to execute
    CancelTerminateExecution();

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
                m_spIsolate->SetStackLimit(reinterpret_cast<uintptr_t>(s_pMinStackLimit));
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

void V8IsolateImpl::SetUpHeapWatchTimer(size_t maxHeapSize)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // create heap watch timer
    auto wrIsolate = CreateWeakRef();
    m_spHeapWatchTimer = new Timer(static_cast<int>(std::max(GetHeapSizeSampleInterval(), 250.0)), -1, [this, wrIsolate, maxHeapSize] (Timer* pTimer)
    {
        // heap watch callback; is the isolate still alive?
        auto spIsolate = wrIsolate.GetTarget();
        if (!spIsolate.IsEmpty())
        {
            // yes; request callback on execution thread
            auto wrTimer = pTimer->CreateWeakRef();
            CallWithLockAsync([wrTimer, maxHeapSize] (V8IsolateImpl* pIsolateImpl)
            {
                // execution thread callback; is the timer still alive?
                auto spTimer = wrTimer.GetTarget();
                if (!spTimer.IsEmpty())
                {
                    // yes; check heap size
                    pIsolateImpl->CheckHeapSize(maxHeapSize);
                }
            });
        }
    });

    // start heap watch timer
    m_spHeapWatchTimer->Start();
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CheckHeapSize(size_t maxHeapSize)
{
    _ASSERTE(IsCurrent() && IsLocked());

    // is the total heap size over the limit?
    v8::HeapStatistics heapStatistics;
    GetHeapStatistics(heapStatistics);
    if (heapStatistics.total_heap_size() > maxHeapSize)
    {
        // yes; collect garbage
        LowMemoryNotification();

        // is the total heap size still over the limit?
        GetHeapStatistics(heapStatistics);
        if (heapStatistics.total_heap_size() > maxHeapSize)
        {
            // yes; the isolate is out of memory; request script termination
            m_IsOutOfMemory = true;
            TerminateExecution();
            return;
        }
    }

    // the isolate is not out of memory; restart heap watch timer
    SetUpHeapWatchTimer(maxHeapSize);
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

    if (m_pExecutionScope)
    {
        m_pExecutionScope->OnExecutionStarted();
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::PromiseHook(v8::PromiseHookType type, v8::Local<v8::Promise> hPromise, v8::Local<v8::Value> /*hParent*/)
{
    if ((type == v8::PromiseHookType::kResolve) && !hPromise.IsEmpty())
    {
        auto hContext = hPromise->CreationContext();
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
        CallWithLockAsync([wrContext] (V8IsolateImpl* pIsolateImpl)
        {
            auto spContext = wrContext.GetTarget();
            if (!spContext.IsEmpty())
            {
                pIsolateImpl->FlushContext(static_cast<V8ContextImpl*>(spContext.GetRawPtr()));
            }
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::FlushContext(V8ContextImpl* pContextImpl)
{
    _ASSERTE(IsCurrent() && IsLocked());

    for (auto& contextEntry : m_ContextEntries)
    {
        if (contextEntry.pContextImpl == pContextImpl)
        {
            contextEntry.FlushPending = false;
            break;
        }
    }

    pContextImpl->Flush();
}
