// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

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
    static size_t GetInstanceCount();

    const StdString& GetName() const { return m_Name; }

    size_t GetMaxIsolateHeapSize();
    void SetMaxIsolateHeapSize(size_t value);
    double GetIsolateHeapSizeSampleInterval();
    void SetIsolateHeapSizeSampleInterval(double value);

    size_t GetMaxIsolateStackUsage();
    void SetMaxIsolateStackUsage(size_t value);

    void CallWithLock(LockCallbackT* pCallback, void* pvArg);

    V8Value GetRootObject();
    void SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers);
    V8Value Execute(const StdString& documentName, const StdString& code, bool evaluate, bool discard);

    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code);
    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes);
    V8ScriptHolder* Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted);
    bool CanExecute(V8ScriptHolder* pHolder);
    V8Value Execute(V8ScriptHolder* pHolder, bool evaluate);

    void Interrupt();
    void GetIsolateHeapInfo(V8IsolateHeapInfo& heapInfo);
    void CollectGarbage(bool exhaustive);
    void OnAccessSettingsChanged();

    void Destroy();

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

    void GetV8ObjectArrayBufferOrViewInfo(void* pvObject, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length);
    void InvokeWithV8ObjectArrayBufferOrViewData(void* pvObject, V8ObjectHelpers::ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg);

    void ProcessDebugMessages();

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

    private:

        V8ContextImpl* m_pContextImpl;
        v8::Context::Scope m_ContextScope;
    };

    v8::Local<v8::Context> CreateContext(v8::ExtensionConfiguration* pExtensionConfiguation = nullptr, v8::Local<v8::ObjectTemplate> hGlobalTemplate = v8::Local<v8::ObjectTemplate>(), v8::Local<v8::Value> hGlobalObject = v8::Local<v8::Value>())
    {
        return m_spIsolateImpl->CreateContext(pExtensionConfiguation, hGlobalTemplate, hGlobalObject);
    }

    v8::Local<v8::Primitive> GetUndefined()
    {
        return m_spIsolateImpl->GetUndefined();
    }

    v8::Local<v8::Primitive> GetNull()
    {
        return m_spIsolateImpl->GetNull();
    }

    v8::Local<v8::Boolean> GetTrue()
    {
        return m_spIsolateImpl->GetTrue();
    }

    v8::Local<v8::Boolean> GetFalse()
    {
        return m_spIsolateImpl->GetFalse();
    }

    v8::Local<v8::Symbol> GetIteratorSymbol()
    {
        return m_spIsolateImpl->GetIteratorSymbol();
    }

    v8::Local<v8::Object> CreateObject()
    {
        return m_spIsolateImpl->CreateObject();
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return m_spIsolateImpl->CreateNumber(value);
    }

    v8::Local<v8::Integer> CreateInteger(std::int32_t value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::Local<v8::Integer> CreateInteger(std::uint32_t value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::MaybeLocal<v8::String> CreateString(const StdString& value)
    {
        return m_spIsolateImpl->CreateString(value);
    }

    v8::Local<v8::Symbol> CreateSymbol(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return m_spIsolateImpl->CreateSymbol(hName);
    }

    v8::Local<v8::Private> CreatePrivate(v8::Local<v8::String> hName = v8::Local<v8::String>())
    {
        return m_spIsolateImpl->CreatePrivate(hName);
    }

    v8::Local<v8::Array> CreateArray(int length = 0)
    {
        return m_spIsolateImpl->CreateArray(length);
    }

    v8::Local<v8::External> CreateExternal(void* pvValue)
    {
        return m_spIsolateImpl->CreateExternal(pvValue);
    }

    v8::Local<v8::ObjectTemplate> CreateObjectTemplate()
    {
        return m_spIsolateImpl->CreateObjectTemplate();
    }

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate(v8::FunctionCallback callback = 0, v8::Local<v8::Value> data = v8::Local<v8::Value>(), v8::Local<v8::Signature> signature = v8::Local<v8::Signature>(), int length = 0)
    {
        return m_spIsolateImpl->CreateFunctionTemplate(callback, data, signature, length);
    }

    v8::MaybeLocal<v8::UnboundScript> CreateUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
    {
        return m_spIsolateImpl->CreateUnboundScript(pSource, options);
    }

    template <typename T>
    v8::Local<T> CreateLocal(v8::Local<T> hTarget)
    {
        return m_spIsolateImpl->CreateLocal(hTarget);
    }

    template <typename T>
    v8::Local<T> CreateLocal(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->CreateLocal(hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(v8::Local<T> hTarget)
    {
        return m_spIsolateImpl->CreatePersistent(hTarget);
    }

    template <typename T>
    Persistent<T> CreatePersistent(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->CreatePersistent(hTarget);
    }

    template <typename T, typename TArg1, typename TArg2>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg1*, TArg2*))
    {
        return m_spIsolateImpl->MakeWeak(hTarget, pArg1, pArg2, pCallback);
    }

    template <typename T>
    void ClearWeak(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->ClearWeak(hTarget);
    }

    template <typename T>
    void Dispose(Persistent<T> hTarget)
    {
        return m_spIsolateImpl->Dispose(hTarget);
    }

    v8::Local<v8::Value> ThrowException(v8::Local<v8::Value> hException)
    {
        return m_spIsolateImpl->ThrowException(hException);
    }

    void TerminateExecution()
    {
        return m_spIsolateImpl->TerminateExecution();
    }

    bool IsExecutionTerminating()
    {
        return m_spIsolateImpl->IsExecutionTerminating();
    }

    void CancelTerminateExecution()
    {
        m_spIsolateImpl->CancelTerminateExecution();
    }

    int ContextDisposedNotification()
    {
        return m_spIsolateImpl->ContextDisposedNotification();
    }

    bool IdleNotificationDeadline(double deadlineInSeconds)
    {
        return m_spIsolateImpl->IdleNotificationDeadline(deadlineInSeconds);
    }

    void LowMemoryNotification()
    {
        m_spIsolateImpl->LowMemoryNotification();
    }

    template <typename T>
    T Verify(const V8IsolateImpl::ExecutionScope& isolateExecutionScope, const v8::TryCatch& tryCatch, T result)
    {
        Verify(isolateExecutionScope, tryCatch);
        return result;
    }

    void Teardown();
    ~V8ContextImpl();

    SharedPtr<V8WeakContextBinding> GetWeakBinding();

    HostObjectHolder* GetHostObjectHolder(v8::Local<v8::Object> hObject);
    bool SetHostObjectHolder(v8::Local<v8::Object> hObject, HostObjectHolder* pHolder);
    void* GetHostObject(v8::Local<v8::Object> hObject);

    static bool CheckContextImplForGlobalObjectCallback(V8ContextImpl* pContextImpl);
    static bool CheckContextImplForHostObjectCallback(V8ContextImpl* pContextImpl);

    void GetV8ObjectPropertyNames(v8::Local<v8::Object> hObject, std::vector<StdString>& names);
    void GetV8ObjectPropertyIndices(v8::Local<v8::Object> hObject, std::vector<int>& indices);

    static void GetGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetGlobalProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void GetGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetGlobalProperty(std::uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void HostObjectConstructorCallHandler(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetIteratorForHostObject(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void AdvanceHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void CreateFunctionForHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void InvokeHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);

    static void GetHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void GetHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetHostObjectProperty(std::uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void InvokeHostObject(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void DisposeWeakHandle(v8::Isolate* pIsolate, Persistent<v8::Object>* phObject, HostObjectHolder* pHolder, void* pvV8ObjectCache);

    v8::Local<v8::Value> ImportValue(const V8Value& value);
    V8Value ExportValue(v8::Local<v8::Value> hValue);
    void ImportValues(const std::vector<V8Value>& values, std::vector<v8::Local<v8::Value>>& importedValues);

    void Verify(const V8IsolateImpl::ExecutionScope& isolateExecutionScope, const v8::TryCatch& tryCatch);
    void VerifyNotOutOfMemory();
    void ThrowScriptException(const HostException& exception);

    StdString m_Name;
    SharedPtr<V8IsolateImpl> m_spIsolateImpl;
    Persistent<v8::Context> m_hContext;
    Persistent<v8::Object> m_hGlobal;
    std::vector<std::pair<StdString, Persistent<v8::Object>>> m_GlobalMembersStack;
    Persistent<v8::Symbol> m_hIsHostObjectKey;
    Persistent<v8::Private> m_hHostObjectHolderKey;
    Persistent<v8::String> m_hHostExceptionKey;
    Persistent<v8::Private> m_hEnumeratorKey;
    Persistent<v8::String> m_hDoneKey;
    Persistent<v8::String> m_hValueKey;
    Persistent<v8::Private> m_hCacheKey;
    Persistent<v8::Private> m_hAccessTokenKey;
    Persistent<v8::Object> m_hAccessToken;
    Persistent<v8::String> m_hInternalUseOnly;
    Persistent<v8::FunctionTemplate> m_hHostObjectTemplate;
    Persistent<v8::FunctionTemplate> m_hHostDelegateTemplate;
    Persistent<v8::FunctionTemplate> m_hHostIteratorTemplate;
    Persistent<v8::Value> m_hTerminationException;
    SharedPtr<V8WeakContextBinding> m_spWeakBinding;
    void* m_pvV8ObjectCache;
    bool m_AllowHostObjectConstructorCall;
    bool m_DisableHostObjectInterception;
};

//-----------------------------------------------------------------------------
// SharedPtrTraits<V8ContextImpl>
//-----------------------------------------------------------------------------

template<>
class SharedPtrTraits<V8ContextImpl>
{
    PROHIBIT_CONSTRUCT(SharedPtrTraits)

public:

    static void Destroy(V8ContextImpl* pTarget)
    {
        pTarget->Destroy();
    }
};
