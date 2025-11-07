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

        explicit NativeScope(V8IsolateImpl& isolateImpl):
            m_IsolateImpl(isolateImpl),
            m_LockScope(m_IsolateImpl.m_upIsolate.get()),
            m_IsolateScope(m_IsolateImpl.m_upIsolate.get()),
            m_HandleScope(m_IsolateImpl.m_upIsolate.get())
        {
            m_IsolateImpl.ProcessCallWithLockQueue();
        }

        ~NativeScope()
        {
            m_IsolateImpl.ProcessCallWithLockQueue();
        }

    private:

        V8IsolateImpl& m_IsolateImpl;
        v8::Locker m_LockScope;
        v8::Isolate::Scope m_IsolateScope;
        v8::HandleScope m_HandleScope;
    };

public:

    using CallWithLockCallback = std::function<void(V8IsolateImpl*)>;

    class Scope final
    {
        PROHIBIT_COPY(Scope)
        PROHIBIT_HEAP(Scope)

    public:

        explicit Scope(V8IsolateImpl& isolateImpl):
            m_MutexLock(isolateImpl.m_Mutex),
            m_NativeScope(isolateImpl)
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

        explicit ExecutionScope(V8IsolateImpl& isolateImpl):
            m_IsolateImpl(isolateImpl),
            m_ExecutionStarted(false)
        {
            m_pPreviousExecutionScope = m_IsolateImpl.EnterExecutionScope(this, reinterpret_cast<size_t*>(this));
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
            m_IsolateImpl.ExitExecutionScope(m_pPreviousExecutionScope);
        }

    private:

        V8IsolateImpl& m_IsolateImpl;
        ExecutionScope* m_pPreviousExecutionScope;
        bool m_ExecutionStarted;
    };

    class DocumentScope final
    {
        PROHIBIT_COPY(DocumentScope)
        PROHIBIT_HEAP(DocumentScope)

    public:

        DocumentScope(V8IsolateImpl& isolateImpl, const V8DocumentInfo& documentInfo):
            m_IsolateImpl(isolateImpl)
        {
            m_pPreviousDocumentInfo = m_IsolateImpl.m_pDocumentInfo;
            m_IsolateImpl.m_pDocumentInfo = &documentInfo;
        }

        ~DocumentScope()
        {
            m_IsolateImpl.m_pDocumentInfo = m_pPreviousDocumentInfo;
        }

    private:

        V8IsolateImpl& m_IsolateImpl;
        const V8DocumentInfo* m_pPreviousDocumentInfo;
    };

    class TryCatch final: public v8::TryCatch
    {
        PROHIBIT_COPY(TryCatch)
        PROHIBIT_HEAP(TryCatch)

    public:

        explicit TryCatch(V8IsolateImpl& isolateImpl):
            v8::TryCatch(isolateImpl.m_upIsolate.get())
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
        return v8::Context::New(m_upIsolate.get(), pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    v8::Local<v8::Primitive> GetUndefined()
    {
        return v8::Undefined(m_upIsolate.get());
    }

    v8::Local<v8::Primitive> GetNull()
    {
        return v8::Null(m_upIsolate.get());
    }

    v8::Local<v8::Boolean> GetTrue()
    {
        return v8::True(m_upIsolate.get());
    }

    v8::Local<v8::Boolean> GetFalse()
    {
        return v8::False(m_upIsolate.get());
    }

    bool BooleanValue(v8::Local<v8::Value> hValue)
    {
        return hValue->BooleanValue(m_upIsolate.get());
    }

    v8::Local<v8::Symbol> GetIteratorSymbol()
    {
        return v8::Symbol::GetIterator(m_upIsolate.get());
    }

    v8::Local<v8::Symbol> GetAsyncIteratorSymbol()
    {
        return v8::Symbol::GetAsyncIterator(m_upIsolate.get());
    }

    v8::Local<v8::Symbol> GetToStringTagSymbol()
    {
        return v8::Symbol::GetToStringTag(m_upIsolate.get());
    }

    v8::Local<v8::Object> CreateObject()
    {
        return v8::Object::New(m_upIsolate.get());
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return v8::Number::New(m_upIsolate.get(), value);
    }

    v8::Local<v8::Integer> CreateInteger(int32_t value)
    {
        return v8::Int32::New(m_upIsolate.get(), value);
    }

    v8::Local<v8::Integer> CreateInteger(uint32_t value)
    {
        return v8::Uint32::NewFromUnsigned(m_upIsolate.get(), value);
    }

    v8::Local<v8::BigInt> CreateBigInt(int64_t value)
    {
        return v8::BigInt::New(m_upIsolate.get(), value);
    }

    v8::Local<v8::BigInt> CreateBigInt(uint64_t value)
    {
        return v8::BigInt::NewFromUnsigned(m_upIsolate.get(), value);
    }

    v8::MaybeLocal<v8::String> CreateString(const StdString& value, v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return value.ToV8String(m_upIsolate.get(), type);
    }

    v8::MaybeLocal<v8::String> CreateStringFromUtf8(const char* data, int length, v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return v8::String::NewFromUtf8(m_upIsolate.get(), data, type, length);
    }

    template <int N>
    v8::Local<v8::String> CreateString(const char (&value)[N], v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return v8::String::NewFromUtf8Literal(m_upIsolate.get(), value, type);
    }

    virtual StdString CreateStdString(v8::Local<v8::Value> hValue) override
    {
        return StdString(m_upIsolate.get(), hValue);
    }

    v8::Local<v8::Symbol> CreateSymbol(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Symbol::New(m_upIsolate.get(), hName);
    }

    v8::Local<v8::Private> CreatePrivate(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Private::New(m_upIsolate.get(), hName);
    }

    v8::Local<v8::Array> CreateArray(int length = 0)
    {
        return v8::Array::New(m_upIsolate.get(), length);
    }

    v8::Local<v8::PrimitiveArray> CreatePrimitiveArray(int length)
    {
        return v8::PrimitiveArray::New(m_upIsolate.get(), length);
    }

    void SetPrimitiveArrayItem(v8::Local<v8::PrimitiveArray> hArray, int index, v8::Local<v8::Primitive> hItem)
    {
        hArray->Set(m_upIsolate.get(), index, hItem);
    }

    v8::Local<v8::Primitive> GetPrimitiveArrayItem(v8::Local<v8::PrimitiveArray> hArray, int index)
    {
        return hArray->Get(m_upIsolate.get(), index);
    }

    v8::Local<v8::External> CreateExternal(void* pvValue)
    {
        return v8::External::New(m_upIsolate.get(), pvValue);
    }

    v8::Local<v8::ObjectTemplate> CreateObjectTemplate()
    {
        return v8::ObjectTemplate::New(m_upIsolate.get());
    }

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate(v8::FunctionCallback callback = 0, v8::Local<v8::Value> data = v8::Local<v8::Value>(), v8::Local<v8::Signature> signature = v8::Local<v8::Signature>(), int length = 0)
    {
        return v8::FunctionTemplate::New(m_upIsolate.get(), callback, data, signature, length);
    }

    v8::Local<v8::SharedArrayBuffer> CreateSharedArrayBuffer(const std::shared_ptr<v8::BackingStore>& spBackingStore)
    {
        return v8::SharedArrayBuffer::New(m_upIsolate.get(), spBackingStore);
    }

    v8::MaybeLocal<v8::UnboundScript> CompileUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = v8::ScriptCompiler::CompileUnboundScript(m_upIsolate.get(), pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ScriptCount;
        }

        return result;
    }

    v8::MaybeLocal<v8::Module> CompileModule(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = v8::ScriptCompiler::CompileModule(m_upIsolate.get(), pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ModuleCount;
        }

        return result;
    }

    v8::Local<v8::Module> CreateSyntheticModule(v8::Local<v8::String> moduleName, const std::vector<v8::Local<v8::String>>& exportNames, v8::Module::SyntheticModuleEvaluationSteps evaluationSteps)
    {
        return v8::Module::CreateSyntheticModule(m_upIsolate.get(), moduleName, v8::MemorySpan<const v8::Local<v8::String>>(exportNames.cbegin(), exportNames.cend()), evaluationSteps);
    }

    v8::Maybe<bool> SetSyntheticModuleExport(v8::Local<v8::Module> hModule, v8::Local<v8::String> hName, v8::Local<v8::Value> hValue)
    {
        return hModule->SetSyntheticModuleExport(m_upIsolate.get(), hName, hValue);
    }

    v8::ScriptOrigin CreateScriptOrigin(v8::Local<v8::Value> hResourceName, int lineOffset = 0, int columnOffset = 0, bool isSharedCrossOrigin = false, int scriptId = -1, v8::Local<v8::Value> hSourceMapUrl = v8::Local<v8::Value>(), bool isOpaque = false, bool isWasm = false, bool isModule = false, v8::Local<v8::PrimitiveArray> hHostDefinedOptions = v8::Local<v8::PrimitiveArray>())
    {
        return v8::ScriptOrigin(hResourceName, lineOffset, columnOffset, isSharedCrossOrigin, scriptId, hSourceMapUrl, isOpaque, isWasm, isModule, hHostDefinedOptions);
    }

    template <typename T>
    v8::Local<T> CreateLocal(v8::Local<T> hTarget)
    {
        return v8::Local<T>::New(m_upIsolate.get(), hTarget);
    }

    template <typename T>
    v8::Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return hTarget.CreateLocal(m_upIsolate.get());
    }

    template <typename T>
    Persistent<T> CreatePersistent(v8::Local<T> hTarget)
    {
        return Persistent<T>::New(m_upIsolate.get(), hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return Persistent<T>::New(m_upIsolate.get(), hTarget);
    }

    template <typename T, typename TArg1, typename TArg2>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg1*, TArg2*))
    {
        return hTarget.MakeWeak(m_upIsolate.get(), pArg1, pArg2, pCallback);
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
        return m_upIsolate->ThrowException(hException);
    }

    bool IsDebuggingEnabled()
    {
        return m_DebuggingEnabled;
    }

    void TerminateExecution(bool force)
    {
        BEGIN_MUTEX_SCOPE(m_DataMutex)

            if (m_optRunMessageLoopReason == RunMessageLoopReason::AwaitingDebugger)
            {
                m_optExitMessageLoopReason = ExitMessageLoopReason::TerminatedExecution;
                m_CallWithLockQueueChanged.notify_one();
                return;
            }

        END_MUTEX_SCOPE

        BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)

            if (force || (m_pExecutionScope != nullptr))
            {
                TerminateExecutionInternal();
            }

        END_MUTEX_SCOPE
    }

    bool IsExecutionTerminating()
    {
        BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)
            return m_IsExecutionTerminating;
        END_MUTEX_SCOPE
    }

    void CancelTerminateExecution()
    {
        BEGIN_MUTEX_SCOPE(m_TerminateExecutionMutex)

            if (m_pExecutionScope != nullptr)
            {
                CancelTerminateExecutionInternal();
            }

        END_MUTEX_SCOPE
    }

    int ContextDisposedNotification()
    {
        return m_upIsolate->ContextDisposedNotification();
    }

    void RequestGarbageCollectionForTesting(v8::Isolate::GarbageCollectionType type)
    {
        m_upIsolate->RequestGarbageCollectionForTesting(type);
    }

    void ClearCachesForTesting()
    {
        m_upIsolate->ClearCachesForTesting();
    }

    v8::Local<v8::StackFrame> GetStackFrame(v8::Local<v8::StackTrace> hStackTrace, uint32_t index)
    {
        return hStackTrace->GetFrame(m_upIsolate.get(), index);
    }

    void RequestInterrupt(v8::InterruptCallback callback, void* pvData)
    {
        m_upIsolate->RequestInterrupt(callback, pvData);
    }

    bool IsCurrent() const
    {
        return m_upIsolate.get() == v8::Isolate::GetCurrent();
    }

    bool IsLocked() const
    {
        return v8::Locker::IsLocked(m_upIsolate.get());
    }

    v8::Local<v8::String> GetTypeOf(v8::Local<v8::Value> hValue)
    {
        return !hValue.IsEmpty() ? hValue->TypeOf(m_upIsolate.get()) : GetUndefined()->TypeOf(m_upIsolate.get());
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
    virtual void CancelAwaitDebugger() override;

    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult) override;

    virtual bool GetEnableInterruptPropagation() override;
    virtual void SetEnableInterruptPropagation(bool value) override;
    virtual bool GetDisableHeapSizeViolationInterrupt() override;
    virtual void SetDisableHeapSizeViolationInterrupt(bool value) override;

    virtual void GetHeapStatistics(v8::HeapStatistics& heapStatistics) override;
    virtual Statistics GetStatistics() override;
    virtual void CollectGarbage(bool exhaustive) override;

    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) override;
    virtual bool EndCpuProfile(const StdString& name, CpuProfileCallback* pCallback, void* pvArg) override;
    virtual void CollectCpuProfileSample() override;
    virtual uint32_t GetCpuProfileSampleInterval() override;
    virtual void SetCpuProfileSampleInterval(uint32_t value) override;

    virtual void WriteHeapSnapshot(void* pvStream) override;

    virtual void runMessageLoopOnPause(int contextGroupId) override;
    virtual void quitMessageLoopOnPause() override;
    virtual void runIfWaitingForDebugger(int contextGroupId) override;
    virtual v8::Local<v8::Context> ensureDefaultContextInGroup(int contextGroupId) override;
    virtual double currentTimeMS() override;

    virtual void sendResponse(int callId, std::unique_ptr<v8_inspector::StringBuffer> upMessage) override;
    virtual void sendNotification(std::unique_ptr<v8_inspector::StringBuffer> upMessage) override;
    virtual void flushProtocolNotifications() override;

    void* AddRefV8Object(void* pvObject);
    void ReleaseV8Object(void* pvObject);

    void* AddRefV8Script(void* pvScript);
    void ReleaseV8Script(void* pvScript);

    void RunTaskAsync(std::unique_ptr<v8::Task> upTask);
    void RunTaskDelayed(std::unique_ptr<v8::Task> upTask, double delayInSeconds);
    void RunTaskWithLockAsync(bool allowNesting, std::unique_ptr<v8::Task> upTask);
    void RunTaskWithLockDelayed(bool allowNesting, std::unique_ptr<v8::Task> upTask, double delayInSeconds);
    std::shared_ptr<v8::TaskRunner> GetForegroundTaskRunner();

    void* AllocateArrayBuffer(size_t size);
    void* AllocateUninitializedArrayBuffer(size_t size);
    void FreeArrayBuffer(void* pvData, size_t size);

    void CallWithLockNoWait(bool allowNesting, CallWithLockCallback&& callback);
    void NORETURN ThrowOutOfMemoryException();

    static void ImportMetaInitializeCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta);
    static v8::MaybeLocal<v8::Promise> ModuleImportCallback(v8::Local<v8::Context> hContext, v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> hResourceName, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> hImportAssertions);
    static v8::MaybeLocal<v8::Module> ModuleResolveCallback(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> importAssertions, v8::Local<v8::Module> hReferrer);

    void InitializeImportMeta(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta);
    v8::MaybeLocal<v8::Promise> ImportModule(v8::Local<v8::Context> hContext, v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> hResourceName, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> hImportAssertions);
    v8::MaybeLocal<v8::Module> ResolveModule(v8::Local<v8::Context> hContext, v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer);

    bool TryGetCachedScriptInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo);
    v8::Local<v8::UnboundScript> GetCachedScript(uint64_t uniqueId, size_t codeDigest);
    v8::Local<v8::UnboundScript> GetCachedScript(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes);
    void CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript);
    void CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript, const std::vector<uint8_t>& cacheBytes);
    void SetCachedScriptCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes);
    void ClearScriptCache();

    void TerminateExecutionInternal();
    void CancelTerminateExecutionInternal();

    ~V8IsolateImpl();

private:

    using CallWithLockEntry = std::pair<bool /*allowNesting*/, CallWithLockCallback>;
    using CallWithLockQueue = std::queue<CallWithLockEntry>;

    class PromiseHookScope final
    {
        PROHIBIT_COPY(PromiseHookScope)
        PROHIBIT_HEAP(PromiseHookScope)

    public:

        explicit PromiseHookScope(V8IsolateImpl& isolateImpl) :
            m_IsolateImpl(isolateImpl)
        {
            m_IsolateImpl.m_upIsolate->SetPromiseHook(PromiseHook);
        }

        ~PromiseHookScope()
        {
            m_IsolateImpl.m_upIsolate->SetPromiseHook(nullptr);
        }

    private:

        V8IsolateImpl& m_IsolateImpl;
    };

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
        std::vector<uint8_t> CacheBytes;
    };

    enum class RunMessageLoopReason
    {
        AwaitingDebugger,
        PausedInDebugger
    };

    enum class ExitMessageLoopReason
    {
        ResumedExecution,
        TerminatedExecution,
        CanceledAwaitDebugger,
        NestedInvocation
    };

    ExitMessageLoopReason RunMessageLoop(RunMessageLoopReason reason);

    void CallWithLockAsync(bool allowNesting, CallWithLockCallback&& callback);
    static void ProcessCallWithLockQueue(v8::Isolate* pIsolate, void* pvIsolateImpl);
    void ProcessCallWithLockQueue();
    void ProcessCallWithLockQueue(std::unique_lock<std::mutex>& lock);
    void ProcessCallWithLockQueue(CallWithLockQueue& callWithLockQueue);
    CallWithLockQueue PopCallWithLockQueue(const std::unique_lock<std::mutex>& lock);

    void ConnectDebugClient();
    void SendDebugCommand(const StdString& command);
    void DisconnectDebugClient();

    ExecutionScope* EnterExecutionScope(ExecutionScope* pExecutionScope, size_t* pStackMarker);
    void ExitExecutionScope(ExecutionScope* pPreviousExecutionScope);

    ExecutionScope* SetExecutionScope(ExecutionScope* pExecutionScope);
    bool InExecutionScope();
    void OnExecutionStarted();
    bool ExecutionStarted();

    void SetUpHeapWatchTimer(bool forceMinInterval);
    void CheckHeapSize(const std::optional<size_t>& optMaxHeapSize, bool timerTriggered);

    static void OnBeforeCallEntered(v8::Isolate* pIsolate);
    void OnBeforeCallEntered();

    static void PromiseHook(v8::PromiseHookType type, v8::Local<v8::Promise> hPromise, v8::Local<v8::Value> hParent);

    void FlushContextAsync(v8::Local<v8::Context> hContext);
    void FlushContextAsync(ContextEntry& contextEntry);
    void FlushContext(V8ContextImpl& contextImpl);

    static size_t HeapExpansionCallback(void* pvData, size_t currentLimit, size_t initialLimit);

    StdString m_Name;
    UniqueDisposePtr<v8::Isolate> m_upIsolate;
    UniqueDisposePtr<v8::CpuProfiler> m_upCpuProfiler;
    Persistent<v8::Private> m_hHostObjectHolderKey;
    RecursiveMutex m_Mutex;
    std::list<ContextEntry> m_ContextEntries;
    SimpleMutex m_DataMutex;
    std::shared_ptr<v8::TaskRunner> m_spForegroundTaskRunner;
    std::vector<std::shared_ptr<v8::Task>> m_AsyncTasks;
    CallWithLockQueue m_CallWithLockQueue;
    std::condition_variable m_CallWithLockQueueChanged;
    size_t m_CallWithLockLevel;
    std::vector<SharedPtr<Timer>> m_TaskTimers;
    std::list<ScriptCacheEntry> m_ScriptCache;
    bool m_DebuggingEnabled;
    int m_DebugPort;
    void* m_pvDebugAgent;
    std::unique_ptr<v8_inspector::V8Inspector> m_upInspector;
    std::unique_ptr<v8_inspector::V8InspectorSession> m_upInspectorSession;
    std::optional<RunMessageLoopReason> m_optRunMessageLoopReason;
    std::optional<ExitMessageLoopReason> m_optExitMessageLoopReason;
    size_t m_MaxArrayBufferAllocation;
    size_t m_ArrayBufferAllocation;
    std::atomic<size_t> m_MaxHeapSize;
    std::atomic<double> m_HeapSizeSampleInterval;
    size_t m_HeapWatchLevel;
    double m_HeapExpansionMultiplier;
    SharedPtr<Timer> m_spHeapWatchTimer;
    std::atomic<size_t> m_MaxStackUsage;
    std::atomic<bool> m_EnableInterruptPropagation;
    std::atomic<bool> m_DisableHeapSizeViolationInterrupt;
    std::atomic<uint32_t> m_CpuProfileSampleInterval;
    size_t m_StackWatchLevel;
    size_t* m_pStackLimit;
    SimpleMutex m_TerminateExecutionMutex;
    bool m_IsExecutionTerminating;
    ExecutionScope* m_pExecutionScope;
    const V8DocumentInfo* m_pDocumentInfo;
    std::atomic<bool> m_IsOutOfMemory;
    std::atomic<bool> m_Released;
    Statistics m_Statistics;
};
