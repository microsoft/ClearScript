// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// forward declarations
//-----------------------------------------------------------------------------

class V8ContextImpl;

//-----------------------------------------------------------------------------
// V8IsolateImpl
//-----------------------------------------------------------------------------

class V8IsolateImpl final: public V8Isolate, public v8_inspector::V8InspectorClient, public v8_inspector::V8Inspector::Channel
{
    PROHIBIT_COPY(V8IsolateImpl)

    class NativeScope final
    {
        PROHIBIT_COPY(NativeScope)
        PROHIBIT_HEAP(NativeScope)

    public:

        explicit NativeScope(V8IsolateImpl* pIsolateImpl):
            m_pIsolateImpl(pIsolateImpl),
            m_LockScope(m_pIsolateImpl->m_spIsolate.get()),
            m_IsolateScope(m_pIsolateImpl->m_spIsolate.get()),
            m_HandleScope(m_pIsolateImpl->m_spIsolate.get())
        {
            m_pIsolateImpl->ProcessCallWithLockQueue();
        }

        ~NativeScope()
        {
            m_pIsolateImpl->ProcessCallWithLockQueue();
        }

    private:

        V8IsolateImpl* m_pIsolateImpl;
        v8::Locker m_LockScope;
        v8::Isolate::Scope m_IsolateScope;
        v8::HandleScope m_HandleScope;
    };

public:

    class Scope final
    {
        PROHIBIT_COPY(Scope)
        PROHIBIT_HEAP(Scope)

    public:

        explicit Scope(V8IsolateImpl* pIsolateImpl):
            m_MutexLock(pIsolateImpl->m_Mutex),
            m_NativeScope(pIsolateImpl)
        {
        }

    private:

        MutexLock<RecursiveMutex> m_MutexLock;
        NativeScope m_NativeScope;
    };

    class ExecutionScope final
    {
        PROHIBIT_COPY(ExecutionScope)
        PROHIBIT_HEAP(ExecutionScope)

    public:

        explicit ExecutionScope(V8IsolateImpl* pIsolateImpl):
            m_pIsolateImpl(pIsolateImpl),
            m_ExecutionStarted(false)
        {
            m_pPreviousExecutionScope = m_pIsolateImpl->EnterExecutionScope(this, reinterpret_cast<size_t*>(&pIsolateImpl));
        }

        void OnExecutionStarted()
        {
            m_ExecutionStarted = true;
        }

        bool ExecutionStarted() const
        {
            return m_ExecutionStarted;
        }

        ~ExecutionScope()
        {
            m_pIsolateImpl->ExitExecutionScope(m_pPreviousExecutionScope);
        }

    private:

        V8IsolateImpl* m_pIsolateImpl;
        ExecutionScope* m_pPreviousExecutionScope;
        bool m_ExecutionStarted;
    };

    class DocumentScope final
    {
        PROHIBIT_COPY(DocumentScope)
        PROHIBIT_HEAP(DocumentScope)

    public:

        DocumentScope(V8IsolateImpl* pIsolateImpl, const V8DocumentInfo& documentInfo):
            m_pIsolateImpl(pIsolateImpl)
        {
            m_pPreviousDocumentInfo = m_pIsolateImpl->m_pDocumentInfo;
            m_pIsolateImpl->m_pDocumentInfo = &documentInfo;
        }

        ~DocumentScope()
        {
            m_pIsolateImpl->m_pDocumentInfo = m_pPreviousDocumentInfo;
        }

    private:

        V8IsolateImpl* m_pIsolateImpl;
        const V8DocumentInfo* m_pPreviousDocumentInfo;
    };

    class TryCatch final: public v8::TryCatch
    {
        PROHIBIT_COPY(TryCatch)
        PROHIBIT_HEAP(TryCatch)

    public:

        explicit TryCatch(V8IsolateImpl* pIsolateImpl):
            v8::TryCatch(pIsolateImpl->m_spIsolate.get())
        {
        }
    };

    V8IsolateImpl(const StdString& name, const v8::ResourceConstraints* pConstraints, const Options& options);

    static V8IsolateImpl* GetInstanceFromIsolate(v8::Isolate* pIsolate);
    static size_t GetInstanceCount();

    const StdString& GetName() const { return m_Name; }
    const Persistent<v8::Private>& GetHostObjectHolderKey() const { return m_hHostObjectHolderKey; }
    const V8DocumentInfo* GetDocumentInfo() const { return m_pDocumentInfo; }

    v8::Local<v8::Context> CreateContext(v8::ExtensionConfiguration* pExtensionConfiguation = nullptr, v8::Local<v8::ObjectTemplate> hGlobalTemplate = v8::Local<v8::ObjectTemplate>(), v8::Local<v8::Value> hGlobalObject = v8::Local<v8::Value>())
    {
        return v8::Context::New(m_spIsolate.get(), pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    v8::Local<v8::Primitive> GetUndefined()
    {
        return v8::Undefined(m_spIsolate.get());
    }

    v8::Local<v8::Primitive> GetNull()
    {
        return v8::Null(m_spIsolate.get());
    }

    v8::Local<v8::Boolean> GetTrue()
    {
        return v8::True(m_spIsolate.get());
    }

    v8::Local<v8::Boolean> GetFalse()
    {
        return v8::False(m_spIsolate.get());
    }

    bool BooleanValue(v8::Local<v8::Value> hValue)
    {
        return hValue->BooleanValue(m_spIsolate.get());
    }

    v8::Local<v8::Symbol> GetIteratorSymbol()
    {
        return v8::Symbol::GetIterator(m_spIsolate.get());
    }

    v8::Local<v8::Object> CreateObject()
    {
        return v8::Object::New(m_spIsolate.get());
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return v8::Number::New(m_spIsolate.get(), value);
    }

    v8::Local<v8::Integer> CreateInteger(int32_t value)
    {
        return v8::Int32::New(m_spIsolate.get(), value);
    }

    v8::Local<v8::Integer> CreateInteger(uint32_t value)
    {
        return v8::Uint32::NewFromUnsigned(m_spIsolate.get(), value);
    }

    v8::MaybeLocal<v8::String> CreateString(const StdString& value, v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return value.ToV8String(m_spIsolate.get(), type);
    }

    StdString CreateStdString(v8::Local<v8::Value> hValue)
    {
        return StdString(m_spIsolate.get(), hValue);
    }

    v8::Local<v8::Symbol> CreateSymbol(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Symbol::New(m_spIsolate.get(), hName);
    }

    v8::Local<v8::Private> CreatePrivate(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Private::New(m_spIsolate.get(), hName);
    }

    v8::Local<v8::Array> CreateArray(int length = 0)
    {
        return v8::Array::New(m_spIsolate.get(), length);
    }

    v8::Local<v8::External> CreateExternal(void* pvValue)
    {
        return v8::External::New(m_spIsolate.get(), pvValue);
    }

    v8::Local<v8::ObjectTemplate> CreateObjectTemplate()
    {
        return v8::ObjectTemplate::New(m_spIsolate.get());
    }

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate(v8::FunctionCallback callback = 0, v8::Local<v8::Value> data = v8::Local<v8::Value>(), v8::Local<v8::Signature> signature = v8::Local<v8::Signature>(), int length = 0)
    {
        return v8::FunctionTemplate::New(m_spIsolate.get(), callback, data, signature, length);
    }

    v8::MaybeLocal<v8::UnboundScript> CompileUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = v8::ScriptCompiler::CompileUnboundScript(m_spIsolate.get(), pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ScriptCount;
        }

        return result;
    }

    v8::MaybeLocal<v8::Module> CompileModule(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = v8::ScriptCompiler::CompileModule(m_spIsolate.get(), pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ModuleCount;
        }

        return result;
    }

    template <typename T>
    v8::Local<T> CreateLocal(v8::Local<T> hTarget)
    {
        return v8::Local<T>::New(m_spIsolate.get(), hTarget);
    }

    template <typename T>
    v8::Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return hTarget.CreateLocal(m_spIsolate.get());
    }

    template <typename T>
    Persistent<T> CreatePersistent(v8::Local<T> hTarget)
    {
        return Persistent<T>::New(m_spIsolate.get(), hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return Persistent<T>::New(m_spIsolate.get(), hTarget);
    }

    template <typename T, typename TArg1, typename TArg2>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg1*, TArg2*))
    {
        return hTarget.MakeWeak(m_spIsolate.get(), pArg1, pArg2, pCallback);
    }

    template <typename T>
    void ClearWeak(Persistent<T> hTarget)
    {
        return hTarget.ClearWeak();
    }

    template <typename T>
    void Dispose(Persistent<T> hTarget)
    {
        hTarget.Dispose();
    }

    v8::Local<v8::Value> ThrowException(v8::Local<v8::Value> hException)
    {
        return m_spIsolate->ThrowException(hException);
    }

    bool IsDebuggingEnabled()
    {
        return m_DebuggingEnabled;
    }

    void TerminateExecution()
    {
        BEGIN_MUTEX_SCOPE(m_DataMutex)

            if (m_AwaitingDebugger)
            {
                m_AbortMessageLoop = true;
                m_CallWithLockQueueChanged.notify_one();
                return;
            }

        END_MUTEX_SCOPE

        m_spIsolate->TerminateExecution();
        m_IsExecutionTerminating = true;
    }

    bool IsExecutionTerminating()
    {
        return m_spIsolate->IsExecutionTerminating() || m_IsExecutionTerminating;
    }

    void CancelTerminateExecution()
    {
        m_spIsolate->CancelTerminateExecution();
        m_IsExecutionTerminating = false;
    }

    int ContextDisposedNotification()
    {
        return m_spIsolate->ContextDisposedNotification();
    }

    bool IdleNotificationDeadline(double deadlineInSeconds)
    {
        return m_spIsolate->IdleNotificationDeadline(deadlineInSeconds);
    }

    void LowMemoryNotification()
    {
        m_spIsolate->LowMemoryNotification();
    }

    v8::Local<v8::StackFrame> GetStackFrame(v8::Local<v8::StackTrace> hStackTrace, uint32_t index)
    {
        return hStackTrace->GetFrame(m_spIsolate.get(), index);
    }

    void RequestInterrupt(v8::InterruptCallback callback, void* pvData)
    {
        m_spIsolate->RequestInterrupt(callback, pvData);
    }

    bool IsCurrent() const
    {
        return m_spIsolate.get() == v8::Isolate::GetCurrent();
    }

    bool IsLocked() const
    {
        return v8::Locker::IsLocked(m_spIsolate.get());
    }

    v8::Local<v8::String> GetTypeOf(v8::Local<v8::Value> hValue)
    {
        return !hValue.IsEmpty() ? hValue->TypeOf(m_spIsolate.get()) : GetUndefined()->TypeOf(m_spIsolate.get());
    }

    bool IsOutOfMemory() const
    {
        return m_IsOutOfMemory;
    }

    void AddContext(V8ContextImpl* pContextImpl, const V8Context::Options& options);
    void RemoveContext(V8ContextImpl* pContextImpl);
    V8ContextImpl* FindContext(v8::Local<v8::Context> hContext);

    void EnableDebugging(int port, bool remote);
    void DisableDebugging();

    virtual size_t GetMaxHeapSize() override;
    virtual void SetMaxHeapSize(size_t value) override;
    virtual double GetHeapSizeSampleInterval() override;
    virtual void SetHeapSizeSampleInterval(double value) override;

    virtual size_t GetMaxStackUsage() override;
    virtual void SetMaxStackUsage(size_t value) override;

    virtual void AwaitDebuggerAndPause() override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, std::vector<uint8_t>& cacheBytes) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheType cacheType, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) override;
    virtual void GetHeapStatistics(v8::HeapStatistics& heapStatistics) override;
    virtual Statistics GetStatistics() override;
    virtual void CollectGarbage(bool exhaustive) override;

    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) override;
    virtual bool EndCpuProfile(const StdString& name, CpuProfileCallbackT* pCallback, void* pvArg) override;
    virtual void CollectCpuProfileSample() override;
    virtual uint32_t GetCpuProfileSampleInterval() override;
    virtual void SetCpuProfileSampleInterval(uint32_t value) override;

    virtual void runMessageLoopOnPause(int contextGroupId) override;
    virtual void quitMessageLoopOnPause() override;
    virtual void runIfWaitingForDebugger(int contextGroupId) override;
    virtual v8::Local<v8::Context> ensureDefaultContextInGroup(int contextGroupId) override;
    virtual double currentTimeMS() override;

    virtual void sendResponse(int callId, std::unique_ptr<v8_inspector::StringBuffer> spMessage) override;
    virtual void sendNotification(std::unique_ptr<v8_inspector::StringBuffer> spMessage) override;
    virtual void flushProtocolNotifications() override;

    void* AddRefV8Object(void* pvObject);
    void ReleaseV8Object(void* pvObject);

    void* AddRefV8Script(void* pvScript);
    void ReleaseV8Script(void* pvScript);

    void RunTaskAsync(v8::Task* pTask);
    void RunTaskDelayed(v8::Task* pTask, double delayInSeconds);
    void RunTaskWithLockAsync(v8::Task* pTask);
    void RunTaskWithLockDelayed(v8::Task* pTask, double delayInSeconds);
    std::shared_ptr<v8::TaskRunner> GetForegroundTaskRunner();

    void CallWithLockNoWait(std::function<void(V8IsolateImpl*)>&& callback);
    void DECLSPEC_NORETURN ThrowOutOfMemoryException();

    static void ImportMetaInitializeCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta);
    static v8::MaybeLocal<v8::Promise> ModuleImportCallback(v8::Local<v8::Context> hContext, v8::Local<v8::ScriptOrModule> hReferrer, v8::Local<v8::String> hSpecifier);
    static v8::MaybeLocal<v8::Module> ModuleResolveCallback(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer);

    void InitializeImportMeta(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta);
    v8::MaybeLocal<v8::Promise> ImportModule(v8::Local<v8::Context> hContext, v8::Local<v8::ScriptOrModule> hReferrer, v8::Local<v8::String> hSpecifier);
    v8::MaybeLocal<v8::Module> ResolveModule(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer);

    v8::Local<v8::UnboundScript> GetCachedScript(const V8DocumentInfo& documentInfo, size_t codeDigest);
    void CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript);
    void ClearScriptCache();

    ~V8IsolateImpl();

private:

    struct ContextEntry final
    {
        V8ContextImpl* pContextImpl;
        std::atomic<bool> FlushPending;

        ContextEntry(V8ContextImpl* pContextImplArg):
            pContextImpl(pContextImplArg),
            FlushPending(false)
        {
        }
    };

    struct ScriptCacheEntry final
    {
        V8DocumentInfo DocumentInfo;
        size_t CodeDigest;
        Persistent<v8::UnboundScript> hScript;
    };

    bool RunMessageLoop(bool awaitingDebugger);

    void CallWithLockAsync(std::function<void(V8IsolateImpl*)>&& callback);
    static void ProcessCallWithLockQueue(v8::Isolate* pIsolate, void* pvIsolateImpl);
    void ProcessCallWithLockQueue();
    void ProcessCallWithLockQueue(std::unique_lock<std::mutex>& lock);
    void ProcessCallWithLockQueue(std::queue<std::function<void(V8IsolateImpl*)>>& callWithLockQueue);

    void ConnectDebugClient();
    void SendDebugCommand(const StdString& command);
    void DisconnectDebugClient();

    ExecutionScope* EnterExecutionScope(ExecutionScope* pExecutionScope, size_t* pStackMarker);
    void ExitExecutionScope(ExecutionScope* pPreviousExecutionScope);

    void SetUpHeapWatchTimer(size_t maxHeapSize);
    void CheckHeapSize(size_t maxHeapSize);

    static void OnBeforeCallEntered(v8::Isolate* pIsolate);
    void OnBeforeCallEntered();

    static void PromiseHook(v8::PromiseHookType type, v8::Local<v8::Promise> hPromise, v8::Local<v8::Value> hParent);

    void FlushContextAsync(v8::Local<v8::Context> hContext);
    void FlushContextAsync(ContextEntry& contextEntry);
    void FlushContext(V8ContextImpl* pContextImpl);

    StdString m_Name;
    UniqueDisposePtr<v8::Isolate> m_spIsolate;
    UniqueDisposePtr<v8::CpuProfiler> m_spCpuProfiler;
    Persistent<v8::Private> m_hHostObjectHolderKey;
    RecursiveMutex m_Mutex;
    std::list<ContextEntry> m_ContextEntries;
    SimpleMutex m_DataMutex;
    std::shared_ptr<v8::TaskRunner> m_spForegroundTaskRunner;
    std::vector<std::shared_ptr<v8::Task>> m_AsyncTasks;
    std::queue<std::function<void(V8IsolateImpl*)>> m_CallWithLockQueue;
    std::condition_variable m_CallWithLockQueueChanged;
    std::vector<SharedPtr<Timer>> m_TaskTimers;
    std::list<ScriptCacheEntry> m_ScriptCache;
    bool m_DebuggingEnabled;
    int m_DebugPort;
    void* m_pvDebugAgent;
    std::unique_ptr<v8_inspector::V8Inspector> m_spInspector;
    std::unique_ptr<v8_inspector::V8InspectorSession> m_spInspectorSession;
    bool m_AwaitingDebugger;
    bool m_InMessageLoop;
    bool m_QuitMessageLoop;
    bool m_AbortMessageLoop;
    std::atomic<size_t> m_MaxHeapSize;
    std::atomic<double> m_HeapSizeSampleInterval;
    size_t m_HeapWatchLevel;
    SharedPtr<Timer> m_spHeapWatchTimer;
    std::atomic<size_t> m_MaxStackUsage;
    std::atomic<uint32_t> m_CpuProfileSampleInterval;
    size_t m_StackWatchLevel;
    size_t* m_pStackLimit;
    ExecutionScope* m_pExecutionScope;
    const V8DocumentInfo* m_pDocumentInfo;
    std::atomic<bool> m_IsOutOfMemory;
    std::atomic<bool> m_IsExecutionTerminating;
    std::atomic<bool> m_Released;
    Statistics m_Statistics;
};
