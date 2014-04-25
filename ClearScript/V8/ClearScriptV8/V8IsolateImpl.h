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

public:

    class Scope
    {
        PROHIBIT_COPY(Scope)
        PROHIBIT_HEAP(Scope)

    public:

        explicit Scope(V8IsolateImpl* pIsolateImpl):
            m_LockScope(pIsolateImpl->m_pIsolate),
            m_IsolateScope(pIsolateImpl->m_pIsolate),
            m_HandleScope(pIsolateImpl->m_pIsolate)
        {
        }

    private:

        Locker m_LockScope;
        Isolate::Scope m_IsolateScope;
        HandleScope m_HandleScope;
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
    const StdString& GetName() const { return m_Name; }

    Local<Context> CreateContext(ExtensionConfiguration* pExtensionConfiguation = nullptr, Handle<ObjectTemplate> hGlobalTemplate = Handle<ObjectTemplate>(), Handle<Value> hGlobalObject = Handle<Value>())
    {
        return Context::New(m_pIsolate, pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    Handle<Primitive> GetUndefined()
    {
        return Undefined(m_pIsolate);
    }

    Handle<Primitive> GetNull()
    {
        return Null(m_pIsolate);
    }

    Handle<Boolean> GetTrue()
    {
        return True(m_pIsolate);
    }

    Handle<Boolean> GetFalse()
    {
        return False(m_pIsolate);
    }

    Local<Number> CreateNumber(double value)
    {
        return Number::New(m_pIsolate, value);
    }

    Local<Integer> CreateInteger(__int32 value)
    {
        return Int32::New(m_pIsolate, value);
    }

    Local<Integer> CreateInteger(unsigned __int32 value)
    {
        return Uint32::NewFromUnsigned(m_pIsolate, value);
    }

    Local<String> CreateString(const StdString& value)
    {
        return value.ToV8String(m_pIsolate);
    }

    Local<Array> CreateArray(int length = 0)
    {
        return Array::New(m_pIsolate, length);
    }

    Local<External> CreateExternal(void* pvValue)
    {
        return External::New(m_pIsolate, pvValue);
    }

    Local<ObjectTemplate> CreateObjectTemplate()
    {
        return ObjectTemplate::New(m_pIsolate);
    }

    Local<FunctionTemplate> CreateFunctionTemplate()
    {
        return FunctionTemplate::New(m_pIsolate);
    }

    template <typename T>
    Local<T> CreateLocal(Handle<T> hTarget)
    {
        return Local<T>::New(m_pIsolate, hTarget);
    }

    template <typename T>
    Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return hTarget.CreateLocal(m_pIsolate);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Handle<T> hTarget)
    {
        return Persistent<T>::New(m_pIsolate, hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return Persistent<T>::New(m_pIsolate, hTarget);
    }

    template <typename T, typename TArg>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg* pArg, void (*pCallback)(Isolate*, Persistent<T>*, TArg*))
    {
        return hTarget.MakeWeak(m_pIsolate, pArg, pCallback);
    }

    template <typename T>
    void Dispose(Persistent<T> hTarget)
    {
        hTarget.Dispose();
    }

    Local<Value> ThrowException(Local<Value> hException)
    {
        return m_pIsolate->ThrowException(hException);
    }

    void TerminateExecution()
    {
        V8::TerminateExecution(m_pIsolate);
    }

    bool IsOutOfMemory() const
    {
        return m_IsOutOfMemory;
    }

    void AddContext(V8ContextImpl* pContextImpl, bool enableDebugging, int debugPort);
    void RemoveContext(V8ContextImpl* pContextImpl);

    void EnableDebugging(int debugPort);
    void DisableDebugging();

    size_t GetMaxStackUsage();
    void SetMaxStackUsage(size_t value);

    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code);
    void GetHeapInfo(V8IsolateHeapInfo& heapInfo);
    void CollectGarbage(bool exhaustive);

    void* AddRefV8Object(void* pvObject);
    void ReleaseV8Object(void* pvObject);

    void* AddRefV8Script(void* pvScript);
    void ReleaseV8Script(void* pvScript);

    void DECLSPEC_NORETURN ThrowOutOfMemoryException();

    ~V8IsolateImpl();

private:

    void DispatchDebugMessages();
    void ProcessDebugMessages();

    void EnterExecutionScope(size_t* pStackMarker);
    void ExitExecutionScope();

    StdString m_Name;
    Isolate* m_pIsolate;
    std::list<V8ContextImpl*> m_ContextPtrs;

    bool m_DebuggingEnabled;
    int m_DebugPort;
    Debug::DebugMessageDispatchHandler m_pDebugMessageDispatcher;
    std::atomic<size_t> m_DebugMessageDispatchCount;

    std::atomic<size_t> m_MaxStackUsage;
    size_t m_ExecutionLevel;
    size_t* m_pStackLimit;

    std::atomic<bool> m_IsOutOfMemory;
};
