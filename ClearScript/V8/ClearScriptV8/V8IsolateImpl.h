// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

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
            m_pIsolateImpl(pIsolateImpl)
        {
            m_pIsolateImpl->EnterExecutionScope(reinterpret_cast<size_t*>(&pIsolateImpl));
        }

        ~ExecutionScope()
        {
            m_pIsolateImpl->ExitExecutionScope();
        }

    private:

        V8IsolateImpl* m_pIsolateImpl;
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

    v8::Local<v8::Object> CreateObject()
    {
        return v8::Object::New(m_pIsolate);
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return v8::Number::New(m_pIsolate, value);
    }

    v8::Local<v8::Integer> CreateInteger(__int32 value)
    {
        return v8::Int32::New(m_pIsolate, value);
    }

    v8::Local<v8::Integer> CreateInteger(unsigned __int32 value)
    {
        return v8::Uint32::NewFromUnsigned(m_pIsolate, value);
    }

    v8::Local<v8::String> CreateString(const StdString& value)
    {
        return value.ToV8String(m_pIsolate);
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

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate()
    {
        return v8::FunctionTemplate::New(m_pIsolate);
    }

    v8::Local<v8::Function> CreateFunction(v8::FunctionCallback callback, v8::Local<v8::Value> data = v8::Local<v8::Value>(), int length = 0)
    {
        return v8::Function::New(m_pIsolate, callback, data, length);
    }

    v8::Local<v8::Script> CreateScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
    {
        return v8::ScriptCompiler::Compile(m_pIsolate, pSource, options);
    }

    v8::Local<v8::UnboundScript> CreateUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
    {
        return v8::ScriptCompiler::CompileUnbound(m_pIsolate, pSource, options);
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

    template <typename T, typename TArg>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg* pArg, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg*))
    {
        return hTarget.MakeWeak(m_pIsolate, pArg, pCallback);
    }

    template<typename T>
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

    void TerminateExecution()
    {
        m_pIsolate->TerminateExecution();
        m_IsExecutionTerminating = true;
    }

    bool IsExecutionTerminating()
    {
        return m_pIsolate->IsExecutionTerminating() || m_IsExecutionTerminating;
    }

    int ContextDisposedNotification()
    {
        return m_pIsolate->ContextDisposedNotification();
    }

    bool IdleNotification(int idleTimeInMilliseconds)
    {
        return m_pIsolate->IdleNotification(idleTimeInMilliseconds);
    }

    void LowMemoryNotification()
    {
        m_pIsolate->LowMemoryNotification();
    }

    void RequestInterrupt(v8::InterruptCallback callback, void* pvData)
    {
        m_pIsolate->RequestInterrupt(callback, pvData);
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
    void GetHeapInfo(V8IsolateHeapInfo& heapInfo);
    void CollectGarbage(bool exhaustive);

    void* AddRefV8Object(void* pvObject);
    void ReleaseV8Object(void* pvObject);

    void* AddRefV8Script(void* pvScript);
    void ReleaseV8Script(void* pvScript);

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
    void ProcessDebugMessages();

    void EnterExecutionScope(size_t* pStackMarker);
    void ExitExecutionScope();

    void SetUpHeapWatchTimer(size_t maxHeapSize);
    void CheckHeapSize(size_t maxHeapSize);

    StdString m_Name;
    v8::Isolate* m_pIsolate;
    RecursiveMutex m_Mutex;
    std::list<V8ContextImpl*> m_ContextPtrs;
    SimpleMutex m_CallWithLockQueueMutex;
    std::queue<std::function<void(V8IsolateImpl*)>> m_CallWithLockQueue;
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
    std::atomic<bool> m_IsOutOfMemory;
    std::atomic<bool> m_IsExecutionTerminating;
};
