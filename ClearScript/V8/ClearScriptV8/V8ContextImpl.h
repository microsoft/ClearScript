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

class V8WeakContextBinding;

//-----------------------------------------------------------------------------
// V8ContextImpl
//-----------------------------------------------------------------------------

class V8ContextImpl: public V8Context
{
    PROHIBIT_COPY(V8ContextImpl)

public:

    V8ContextImpl(V8IsolateImpl* pIsolateImpl, const StdString& name, bool enableDebugging, bool disableGlobalMembers, int debugPort);
    const StdString& GetName() const { return m_Name; }

    size_t GetMaxIsolateStackUsage();
    void SetMaxIsolateStackUsage(size_t value);

    void CallWithLock(LockCallbackT* pCallback, void* pvArg);
    V8Value GetRootObject();
    void SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers);
    V8Value Execute(const StdString& documentName, const StdString& code, bool evaluate, bool discard);
    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code);
    bool CanExecute(V8ScriptHolder* pHolder);
    V8Value Execute(V8ScriptHolder* pHolder, bool evaluate);
    void Interrupt();
    void GetIsolateHeapInfo(V8IsolateHeapInfo& heapInfo);
    void CollectGarbage(bool exhaustive);

    V8Value GetV8ObjectProperty(void* pvObject, const StdString& name);
    void SetV8ObjectProperty(void* pvObject, const StdString& name, const V8Value& value);
    bool DeleteV8ObjectProperty(void* pvObject, const StdString& name);
    void GetV8ObjectPropertyNames(void* pvObject, std::vector<StdString>& names);

    V8Value GetV8ObjectProperty(void* pvObject, int index);
    void SetV8ObjectProperty(void* pvObject, int index, const V8Value& value);
    bool DeleteV8ObjectProperty(void* pvObject, int index);
    void GetV8ObjectPropertyIndices(void* pvObject, std::vector<int>& indices);

    V8Value InvokeV8Object(void* pvObject, const std::vector<V8Value>& args, bool asConstructor);
    V8Value InvokeV8ObjectMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args);

    void ProcessDebugMessages();
    ~V8ContextImpl();

private:

    class Scope
    {
        PROHIBIT_COPY(Scope)
        PROHIBIT_HEAP(Scope)

    public:

        explicit Scope(V8ContextImpl* pContextImpl):
            m_pContextImpl(pContextImpl),
            m_ContextScope(m_pContextImpl->m_hContext)
        {
        }

        ~Scope()
        {
            if (!std::uncaught_exception() && m_pContextImpl->m_hContext->HasOutOfMemoryException())
            {
                m_pContextImpl->m_spIsolateImpl->ThrowOutOfMemoryException();
            }
        }

    private:

        V8ContextImpl* m_pContextImpl;
        Context::Scope m_ContextScope;
    };

    Local<Context> CreateContext(ExtensionConfiguration* pExtensionConfiguation = nullptr, Handle<ObjectTemplate> hGlobalTemplate = Handle<ObjectTemplate>(), Handle<Value> hGlobalObject = Handle<Value>())
    {
        return m_spIsolateImpl->CreateContext(pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    Handle<Primitive> GetUndefined()
    {
        return m_spIsolateImpl->GetUndefined();
    }

    Handle<Primitive> GetNull()
    {
        return m_spIsolateImpl->GetNull();
    }

    Handle<Boolean> GetTrue()
    {
        return m_spIsolateImpl->GetTrue();
    }

    Handle<Boolean> GetFalse()
    {
        return m_spIsolateImpl->GetFalse();
    }

    Local<Number> CreateNumber(double value)
    {
        return m_spIsolateImpl->CreateNumber(value);
    }

    Local<Integer> CreateInteger(__int32 value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    Local<Integer> CreateInteger(unsigned __int32 value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    Local<String> CreateString(const StdString& value)
    {
        return m_spIsolateImpl->CreateString(value);
    }

    Local<Array> CreateArray(int length = 0)
    {
        return m_spIsolateImpl->CreateArray(length);
    }

    Local<External> CreateExternal(void* pvValue)
    {
        return m_spIsolateImpl->CreateExternal(pvValue);
    }

    Local<ObjectTemplate> CreateObjectTemplate()
    {
        return m_spIsolateImpl->CreateObjectTemplate();
    }

    Local<FunctionTemplate> CreateFunctionTemplate()
    {
        return m_spIsolateImpl->CreateFunctionTemplate();
    }

    template <typename T>
    Local<T> CreateLocal(Handle<T> hTarget)
    {
        return m_spIsolateImpl->CreateLocal(hTarget);
    }

    template <typename T>
    Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->CreateLocal(hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Handle<T> hTarget)
    {
        return m_spIsolateImpl->CreatePersistent(hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->CreatePersistent(hTarget);
    }

    template <typename T, typename TArg>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg* pArg, void (*pCallback)(Isolate*, Persistent<T>*, TArg*))
    {
        return m_spIsolateImpl->MakeWeak(hTarget, pArg, pCallback);
    }

    template <typename T>
    void Dispose(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->Dispose(hTarget);
    }

    Local<Value> ThrowException(Local<Value> hException)
    {
        return m_spIsolateImpl->ThrowException(hException);
    }

    void TerminateExecution()
    {
        return m_spIsolateImpl->TerminateExecution();
    }

    template <typename T>
    T Verify(const TryCatch& tryCatch, T result)
    {
        Verify(tryCatch);
        return result;
    }

    Handle<Value> Wrap();
    SharedPtr<V8WeakContextBinding> GetWeakBinding();

    void GetV8ObjectPropertyNames(Handle<Object> hObject, std::vector<StdString>& names);
    void GetV8ObjectPropertyIndices(Handle<Object> hObject, std::vector<int>& indices);

    static void GetGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Value>& info);
    static void SetGlobalProperty(Local<String> hName, Local<Value> value, const PropertyCallbackInfo<Value>& info);
    static void QueryGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Integer>& info);
    static void DeleteGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Boolean>& info);
    static void GetGlobalPropertyNames(const PropertyCallbackInfo<Array>& info);

    static void GetGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Value>& info);
    static void SetGlobalProperty(unsigned __int32 index, Local<Value> hValue, const PropertyCallbackInfo<Value>& info);
    static void QueryGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Integer>& info);
    static void DeleteGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Boolean>& info);
    static void GetGlobalPropertyIndices(const PropertyCallbackInfo<Array>& info);

    static void GetHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Value>& info);
    static void SetHostObjectProperty(Local<String> hName, Local<Value> hValue, const PropertyCallbackInfo<Value>& info);
    static void QueryHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Integer>& info);
    static void DeleteHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Boolean>& info);
    static void GetHostObjectPropertyNames(const PropertyCallbackInfo<Array>& info);

    static void GetHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Value>& info);
    static void SetHostObjectProperty(unsigned __int32 index, Local<Value> hValue, const PropertyCallbackInfo<Value>& info);
    static void QueryHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Integer>& info);
    static void DeleteHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Boolean>& info);
    static void GetHostObjectPropertyIndices(const PropertyCallbackInfo<Array>& info);

    static void InvokeHostObject(const FunctionCallbackInfo<Value>& info);
    static void DisposeWeakHandle(Isolate* pIsolate, Persistent<Object>* phObject, void* pvV8ObjectCache);

    Handle<Value> ImportValue(const V8Value& value);
    V8Value ExportValue(Handle<Value> hValue);
    void ImportValues(const std::vector<V8Value>& values, std::vector<Handle<Value>>& importedValues);

    void Verify(const TryCatch& tryCatch);
    void VerifyNotOutOfMemory();
    void ThrowScriptException(const HostException& exception);

    StdString m_Name;
    SharedPtr<V8IsolateImpl> m_spIsolateImpl;
    Persistent<Context> m_hContext;
    Persistent<Object> m_hGlobal;
    std::vector<Persistent<Object>> m_GlobalMembersStack;
    Persistent<String> m_hHostObjectCookieName;
    Persistent<String> m_hInnerExceptionName;
    Persistent<FunctionTemplate> m_hHostObjectTemplate;
    SharedPtr<V8WeakContextBinding> m_spWeakBinding;
    void* m_pvV8ObjectCache;
};
