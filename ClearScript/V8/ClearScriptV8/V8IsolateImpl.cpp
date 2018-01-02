// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// SharedPtrTraits<v8::Task>
//-----------------------------------------------------------------------------

template<>
class SharedPtrTraits<v8::Task>
{
    PROHIBIT_CONSTRUCT(SharedPtrTraits)

public:

    static void Destroy(v8::Task* pTarget)
    {
        pTarget->Delete();
    }
};

//-----------------------------------------------------------------------------
// V8Platform
//-----------------------------------------------------------------------------

class V8Platform: public v8::Platform
{
public:

    static V8Platform& GetInstance();
    static void EnsureInstalled();

    virtual size_t NumberOfAvailableBackgroundThreads() override;
    virtual void CallOnBackgroundThread(v8::Task* pTask, ExpectedRuntime runtime) override;
    virtual void CallOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask) override;
    virtual void CallDelayedOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask, double delayInSeconds) override;
    virtual void CallIdleOnForegroundThread(v8::Isolate* pIsolate, v8::IdleTask* pTask) override;
    virtual bool IdleTasksEnabled(v8::Isolate* pIsolate) override;
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

size_t V8Platform::NumberOfAvailableBackgroundThreads()
{
    return HighResolutionClock::GetHardwareConcurrency();
}

//-----------------------------------------------------------------------------

void V8Platform::CallOnBackgroundThread(v8::Task* pTask, ExpectedRuntime /*runtime*/)
{
    auto pIsolate = v8::Isolate::GetCurrent();
    if (pIsolate == nullptr)
    {
        pTask->Run();
        pTask->Delete();
    }
    else
    {
        static_cast<V8IsolateImpl*>(pIsolate->GetData(0))->RunTaskAsync(pTask);
    }
}

//-----------------------------------------------------------------------------

void V8Platform::CallOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask)
{
    static_cast<V8IsolateImpl*>(pIsolate->GetData(0))->RunTaskWithLockAsync(pTask);
}

//-----------------------------------------------------------------------------

void V8Platform::CallDelayedOnForegroundThread(v8::Isolate* pIsolate, v8::Task* pTask, double delayInSeconds)
{
    static_cast<V8IsolateImpl*>(pIsolate->GetData(0))->RunTaskWithLockDelayed(pTask, delayInSeconds);
}

//-----------------------------------------------------------------------------

void V8Platform::CallIdleOnForegroundThread(v8::Isolate* /*pIsolate*/, v8::IdleTask* /*pTask*/)
{
    // unexpected call to unsupported method
    std::terminate();
}

//-----------------------------------------------------------------------------

bool V8Platform::IdleTasksEnabled(v8::Isolate* /*pIsolate*/)
{
    return false;
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
// V8ArrayBufferAllocator
//-----------------------------------------------------------------------------

class V8ArrayBufferAllocator: public v8::ArrayBuffer::Allocator
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

static const int s_ContextGroupId = 1;
static const size_t s_StackBreathingRoom = static_cast<size_t>(16 * 1024);
static size_t* const s_pMinStackLimit = reinterpret_cast<size_t*>(sizeof(size_t));
static std::atomic<size_t> s_InstanceCount(0);

//-----------------------------------------------------------------------------

V8IsolateImpl::V8IsolateImpl(const StdString& name, const V8IsolateConstraints* pConstraints, bool enableDebugging, bool enableRemoteDebugging, int debugPort) :
    m_Name(name),
    m_DebuggingEnabled(false),
    m_AwaitingDebugger(false),
    m_InMessageLoop(false),
    m_QuitMessageLoop(false),
    m_AbortMessageLoop(false),
    m_MaxHeapSize(0),
    m_HeapWatchLevel(0),
    m_MaxStackUsage(0),
    m_StackWatchLevel(0),
    m_pStackLimit(nullptr),
    m_pExecutionScope(nullptr),
    m_IsOutOfMemory(false),
    m_IsExecutionTerminating(false),
    m_Released(false)
{
    V8Platform::EnsureInstalled();

    v8::Isolate::CreateParams params;
    params.array_buffer_allocator = &V8ArrayBufferAllocator::GetInstance();
    if (pConstraints != nullptr)
    {
        params.constraints.set_max_semi_space_size_in_kb(static_cast<size_t>(pConstraints->GetMaxNewSpaceSize()) * 1024);
        params.constraints.set_max_old_space_size(pConstraints->GetMaxOldSpaceSize());
    }

    m_pIsolate = v8::Isolate::New(params);
    m_pIsolate->AddBeforeCallEnteredCallback(OnBeforeCallEntered);

    BEGIN_ADDREF_SCOPE
    BEGIN_ISOLATE_SCOPE

        m_pIsolate->SetData(0, this);
        m_pIsolate->SetCaptureStackTraceForUncaughtExceptions(true, 64, v8::StackTrace::kDetailed);

        m_hHostObjectHolderKey = CreatePersistent(CreatePrivate());

        if (enableDebugging)
        {
            EnableDebugging(debugPort, enableRemoteDebugging);
        }

    END_ISOLATE_SCOPE
    END_ADDREF_SCOPE

    ++s_InstanceCount;
}

//-----------------------------------------------------------------------------

size_t V8IsolateImpl::GetInstanceCount()
{
    return s_InstanceCount;
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::AddContext(V8ContextImpl* pContextImpl, bool enableDebugging, bool enableRemoteDebugging, int debugPort)
{
    _ASSERTE(IsCurrent() && IsLocked());

    if (!enableDebugging)
    {
        m_ContextPtrs.push_back(pContextImpl);
    }
    else
    {
        m_ContextPtrs.push_front(pContextImpl);
        EnableDebugging(debugPort, enableRemoteDebugging);
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

    m_ContextPtrs.remove(pContextImpl);
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

        m_spInspector = v8_inspector::V8Inspector::create(m_pIsolate, this);

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

V8ScriptHolder* V8IsolateImpl::Compile(const StdString& documentName, const StdString& code)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextPtrs.size() > 0) ? m_ContextPtrs.front() : new V8ContextImpl(this, StdString(), false, true, false, 0));
        return spContextImpl->Compile(documentName, code);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextPtrs.size() > 0) ? m_ContextPtrs.front() : new V8ContextImpl(this, StdString(), false, true, false, 0));
        return spContextImpl->Compile(documentName, code, cacheType, cacheBytes);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8IsolateImpl::Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted)
{
    BEGIN_ISOLATE_SCOPE

        SharedPtr<V8ContextImpl> spContextImpl((m_ContextPtrs.size() > 0) ? m_ContextPtrs.front() : new V8ContextImpl(this, StdString(), false, true, false, 0));
        return spContextImpl->Compile(documentName, code, cacheType, cacheBytes, cacheAccepted);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::GetHeapInfo(V8IsolateHeapInfo& heapInfo)
{
    BEGIN_ISOLATE_SCOPE

    v8::HeapStatistics heapStatistics;
    m_pIsolate->GetHeapStatistics(&heapStatistics);

    heapInfo.Set(
        heapStatistics.total_heap_size(),
        heapStatistics.total_heap_size_executable(),
        heapStatistics.total_physical_size(),
        heapStatistics.used_heap_size(),
        heapStatistics.heap_size_limit()
    );

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::CollectGarbage(bool exhaustive)
{
    BEGIN_ISOLATE_SCOPE

    if (exhaustive)
    {
        LowMemoryNotification();
    }
    else
    {
        while (!IdleNotificationDeadline(V8Platform::GetInstance().MonotonicallyIncreasingTime() + 0.1));
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

    if (m_ContextPtrs.size() > 0)
    {
        return m_ContextPtrs.front()->GetContext();
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
        pTask->Delete();
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask, [] (v8::Task* pTask) { pTask->Delete(); });
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
                        m_AsyncTasks.erase(std::remove(m_AsyncTasks.begin(), m_AsyncTasks.end(), spTask), m_AsyncTasks.end());
                    END_MUTEX_SCOPE
                }
            }
        });
    }
}

//-----------------------------------------------------------------------------

void V8IsolateImpl::RunTaskWithLockAsync(v8::Task* pTask)
{
    if (m_Released)
    {
        pTask->Run();
        pTask->Delete();
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask, [] (v8::Task* pTask) { pTask->Delete(); });
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
        pTask->Delete();
    }
    else
    {
        std::shared_ptr<v8::Task> spTask(pTask, [] (v8::Task* pTask) { pTask->Delete(); });

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
                        m_TaskTimers.erase(std::remove(m_TaskTimers.begin(), m_TaskTimers.end(), pTimer), m_TaskTimers.end());
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

V8IsolateImpl::~V8IsolateImpl()
{
    --s_InstanceCount;
    m_Released = true;

    // Entering the isolate scope triggers call-with-lock queue processing. It should always be
    // done here, if for no other reason than that it may prevent deadlocks in V8 isolate disposal.

    BEGIN_ISOLATE_SCOPE
        DisableDebugging();
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

    m_pIsolate->RemoveBeforeCallEnteredCallback(OnBeforeCallEntered);
    m_pIsolate->Dispose();
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
        auto requestInterrupt = false;

        BEGIN_MUTEX_SCOPE(m_DataMutex)

            m_CallWithLockQueue.push(std::move(callback));

            if (m_InMessageLoop)
            {
                m_CallWithLockQueueChanged.notify_one();
            }
            else
            {
                requestInterrupt = m_CallWithLockQueue.size() == 1;
            }

        END_MUTEX_SCOPE

        if (requestInterrupt)
        {
            RequestInterrupt(ProcessCallWithLockQueue, this);
        }
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

    BEGIN_MUTEX_SCOPE(m_DataMutex)
        std::swap(callWithLockQueue, m_CallWithLockQueue);
    END_MUTEX_SCOPE

    ProcessCallWithLockQueue(callWithLockQueue);
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
            m_pIsolate->SetStackLimit(reinterpret_cast<std::uintptr_t>(pStackLimit));
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
                m_pIsolate->SetStackLimit(reinterpret_cast<std::uintptr_t>(s_pMinStackLimit));
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
    V8IsolateHeapInfo heapInfo;
    GetHeapInfo(heapInfo);
    if (heapInfo.GetTotalHeapSize() > maxHeapSize)
    {
        // yes; collect garbage
        LowMemoryNotification();

        // is the total heap size still over the limit?
        GetHeapInfo(heapInfo);
        if (heapInfo.GetTotalHeapSize() > maxHeapSize)
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
    static_cast<V8IsolateImpl*>(pIsolate->GetData(0))->OnBeforeCallEntered();
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
