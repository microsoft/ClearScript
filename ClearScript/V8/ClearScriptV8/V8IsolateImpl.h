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

class V8IsolateImpl: public V8Isolate
{
    PROHIBIT_COPY(V8IsolateImpl)

    class NativeScope
    {
        PROHIBIT_COPY(NativeScope)
        PROHIBIT_HEAP(NativeScope)

    public:

        explicit NativeScope(V8IsolateImpl* pIsolateImpl):
            m_pIsolateImpl(pIsolateImpl),
            m_LockScope(m_pIsolateImpl->m_pIsolate),
            m_IsolateScope(m_pIsolateImpl->m_pIsolate),
            m_HandleScope(m_pIsolateImpl->m_pIsolate)
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

    class Scope
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

    class ExecutionScope
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

    class TryCatch: public v8::TryCatch
    {
        PROHIBIT_COPY(TryCatch)
        PROHIBIT_HEAP(TryCatch)

    public:

        explicit TryCatch(V8IsolateImpl* pIsolateImpl):
            v8::TryCatch(pIsolateImpl->m_pIsolate)
        {
        }
    };

    V8IsolateImpl(const StdString& name, const V8IsolateConstraints* pConstraints, bool enableDebugging, int debugPort);
    static size_t GetInstanceCount();

    const StdString& GetName() const { return m_Name; }

    v8::Local<v8::Context> CreateContext(v8::ExtensionConfiguration* pExtensionConfiguation = nullptr, v8::Local<v8::ObjectTemplate> hGlobalTemplate = v8::Local<v8::ObjectTemplate>(), v8::Local<v8::Value> hGlobalObject = v8::Local<v8::Value>())
    {
        return v8::Context::New(m_pIsolate, pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    v8::Local<v8::Primitive> GetUndefined()
    {
        return v8::Undefined(m_pIsolate);
    }

    v8::Local<v8::Primitive> GetNull()
    {
        return v8::Null(m_pIsolate);
    }

    v8::Local<v8::Boolean> GetTrue()
    {
        return v8::True(m_pIsolate);
    }

    v8::Local<v8::Boolean> GetFalse()
    {
        return v8::False(m_pIsolate);
    }

    v8::Local<v8::Symbol> GetIteratorSymbol()
    {
        return v8::Symbol::GetIterator(m_pIsolate);
    }

    v8::Local<v8::Object> CreateObject()
    {
        return v8::Object::New(m_pIsolate);
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return v8::Number::New(m_pIsolate, value);
    }

    v8::Local<v8::Integer> CreateInteger(std::int32_t value)
    {
        return v8::Int32::New(m_pIsolate, value);
    }

    v8::Local<v8::Integer> CreateInteger(std::uint32_t value)
    {
        return v8::Uint32::NewFromUnsigned(m_pIsolate, value);
    }

    v8::MaybeLocal<v8::String> CreateString(const StdString& value)
    {
        return value.ToV8String(m_pIsolate);
    }

    v8::Local<v8::Symbol> CreateSymbol(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Symbol::New(m_pIsolate, hName);
    }

    v8::Local<v8::Private> CreatePrivate(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return v8::Private::New(m_pIsolate, hName);
    }

    v8::Local<v8::Array> CreateArray(int length = 0)
    {
        return v8::Array::New(m_pIsolate, length);
    }

    v8::Local<v8::External> CreateExternal(void* pvValue)
    {
        return v8::External::New(m_pIsolate, pvValue);
    }

    v8::Local<v8::ObjectTemplate> CreateObjectTemplate()
    {
        return v8::ObjectTemplate::New(m_pIsolate);
    }

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate(v8::FunctionCallback callback = 0, v8::Local<v8::Value> data = v8::Local<v8::Value>(), v8::Local<v8::Signature> signature = v8::Local<v8::Signature>(), int length = 0)
    {
        return v8::FunctionTemplate::New(m_pIsolate, callback, data, signature, length);
    }

    v8::MaybeLocal<v8::UnboundScript> CreateUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
    {
        return v8::ScriptCompiler::CompileUnboundScript(m_pIsolate, pSource, options);
    }

    template <typename T>
    v8::Local<T> CreateLocal(v8::Local<T> hTarget)
    {
        return v8::Local<T>::New(m_pIsolate, hTarget);
    }

    template <typename T>
    v8::Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return hTarget.CreateLocal(m_pIsolate);
    }

    template <typename T>
    Persistent<T> CreatePersistent(v8::Local<T> hTarget)
    {
        return Persistent<T>::New(m_pIsolate, hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return Persistent<T>::New(m_pIsolate, hTarget);
    }

    template <typename T, typename TArg1, typename TArg2>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg1*, TArg2*))
    {
        return hTarget.MakeWeak(m_pIsolate, pArg1, pArg2, pCallback);
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
        return m_pIsolate->ThrowException(hException);
    }

    bool IsDebuggingEnabled()
    {
        return m_DebuggingEnabled;
    }

    void TerminateExecution()
    {
        m_pIsolate->TerminateExecution();
        m_IsExecutionTerminating = true;
    }

    bool IsExecutionTerminating()
    {
        return m_pIsolate->IsExecutionTerminating() || m_IsExecutionTerminating;
    }

    void CancelTerminateExecution()
    {
        m_pIsolate->CancelTerminateExecution();
        m_IsExecutionTerminating = false;
    }

    int ContextDisposedNotification()
    {
        return m_pIsolate->ContextDisposedNotification();
    }

    bool IdleNotificationDeadline(double deadlineInSeconds)
    {
        return m_pIsolate->IdleNotificationDeadline(deadlineInSeconds);
    }

    void LowMemoryNotification()
    {
        m_pIsolate->LowMemoryNotification();
    }

    void RequestInterrupt(v8::InterruptCallback callback, void* pvData)
    {
        m_pIsolate->RequestInterrupt(callback, pvData);
    }

    void ProcessDebugMessages()
    {
        v8::Debug::ProcessDebugMessages(m_pIsolate);
    }

    bool IsCurrent() const
    {
        return m_pIsolate == v8::Isolate::GetCurrent();
    }

    bool IsLocked() const
    {
        return v8::Locker::IsLocked(m_pIsolate);
    }

    bool IsOutOfMemory() const
    {
        return m_IsOutOfMemory;
    }

    void AddContext(V8ContextImpl* pContextImpl, bool enableDebugging, int debugPort);
    void RemoveContext(V8ContextImpl* pContextImpl);

    void EnableDebugging(int debugPort);
    void DisableDebugging();

    size_t GetMaxHeapSize();
    void SetMaxHeapSize(size_t value);
    double GetHeapSizeSampleInterval();
    void SetHeapSizeSampleInterval(double value);

    size_t GetMaxStackUsage();
    void SetMaxStackUsage(size_t value);

    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code);
    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes);
    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted);
    void GetHeapInfo(V8IsolateHeapInfo& heapInfo);
    void CollectGarbage(bool exhaustive);

    void* AddRefV8Object(void* pvObject);
    void ReleaseV8Object(void* pvObject);

    void* AddRefV8Script(void* pvScript);
    void ReleaseV8Script(void* pvScript);

    void RunTaskAsync(v8::Task* pTask);
    void RunTaskWithLockAsync(v8::Task* pTask);
    void RunTaskWithLockDelayed(v8::Task* pTask, double delayInSeconds);

    void CallWithLockNoWait(std::function<void(V8IsolateImpl*)>&& callback);
    void DECLSPEC_NORETURN ThrowOutOfMemoryException();

    ~V8IsolateImpl();

private:

    void CallWithLockAsync(std::function<void(V8IsolateImpl*)>&& callback);
    static void ProcessCallWithLockQueue(v8::Isolate* pIsolate, void* pvIsolateImpl);
    void ProcessCallWithLockQueue();

    void SendDebugCommand(const StdString& command);
    static void OnDebugMessageShared(const v8::Debug::Message& message);
    void OnDebugMessage(const v8::Debug::Message& message);
    void DispatchDebugMessages();

    ExecutionScope* EnterExecutionScope(ExecutionScope* pExecutionScope, size_t* pStackMarker);
    void ExitExecutionScope(ExecutionScope* pPreviousExecutionScope);

    void SetUpHeapWatchTimer(size_t maxHeapSize);
    void CheckHeapSize(size_t maxHeapSize);

    static void OnBeforeCallEntered(v8::Isolate* pIsolate);
    void OnBeforeCallEntered();

    StdString m_Name;
    v8::Isolate* m_pIsolate;
    RecursiveMutex m_Mutex;
    std::list<V8ContextImpl*> m_ContextPtrs;
    SimpleMutex m_DataMutex;
    std::vector<std::shared_ptr<v8::Task>> m_AsyncTasks;
    std::queue<std::function<void(V8IsolateImpl*)>> m_CallWithLockQueue;
    std::vector<SharedPtr<Timer>> m_TaskTimers;
    bool m_DebuggingEnabled;
    int m_DebugPort;
    void* m_pvDebugAgent;
    std::atomic<size_t> m_DebugMessageDispatchCount;
    std::atomic<size_t> m_MaxHeapSize;
    std::atomic<double> m_HeapSizeSampleInterval;
    size_t m_HeapWatchLevel;
    SharedPtr<Timer> m_spHeapWatchTimer;
    std::atomic<size_t> m_MaxStackUsage;
    size_t m_StackWatchLevel;
    size_t* m_pStackLimit;
    ExecutionScope* m_pExecutionScope;
    std::atomic<bool> m_IsOutOfMemory;
    std::atomic<bool> m_IsExecutionTerminating;
    std::atomic<bool> m_Released;
};
