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

    v8::Local<v8::Object> CreateObject()
    {
        return m_spIsolateImpl->CreateObject();
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return m_spIsolateImpl->CreateNumber(value);
    }

    v8::Local<v8::Integer> CreateInteger(__int32 value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::Local<v8::Integer> CreateInteger(unsigned __int32 value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::Local<v8::String> CreateString(const StdString& value)
    {
        return m_spIsolateImpl->CreateString(value);
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

    v8::Local<v8::FunctionTemplate> CreateFunctionTemplate()
    {
        return m_spIsolateImpl->CreateFunctionTemplate();
    }

    v8::Local<v8::Function> CreateFunction(v8::FunctionCallback callback, v8::Local<v8::Value> data = v8::Local<v8::Value>(), int length = 0)
    {
        return m_spIsolateImpl->CreateFunction(callback, data, length);
    }

    v8::Local<v8::Script> CreateScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
    {
        return m_spIsolateImpl->CreateScript(pSource, options);
    }

    v8::Local<v8::UnboundScript> CreateUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions)
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

    template <typename T, typename TArg>
    Persistent<T> MakeWeak(Persistent<T> hTarget, TArg* pArg, void (*pCallback)(v8::Isolate*, Persistent<T>*, TArg*))
    {
        return m_spIsolateImpl->MakeWeak(hTarget, pArg, pCallback);
    }

    template<typename T>
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

    int ContextDisposedNotification()
    {
        return m_spIsolateImpl->ContextDisposedNotification();
    }

    bool IdleNotification(int idleTimeInMilliseconds)
    {
        return m_spIsolateImpl->IdleNotification(idleTimeInMilliseconds);
    }

    void LowMemoryNotification()
    {
        m_spIsolateImpl->LowMemoryNotification();
    }

    template <typename T>
    T Verify(const v8::TryCatch& tryCatch, T result)
    {
        Verify(tryCatch);
        return result;
    }

    ~V8ContextImpl();

    v8::Local<v8::Value> Wrap();
    SharedPtr<V8WeakContextBinding> GetWeakBinding();

    static bool CheckContextImplForGlobalObjectCallback(V8ContextImpl* pContextImpl);
    static bool CheckContextImplForHostObjectCallback(V8ContextImpl* pContextImpl);

    void GetV8ObjectPropertyNames(v8::Local<v8::Object> hObject, std::vector<StdString>& names);
    void GetV8ObjectPropertyIndices(v8::Local<v8::Object> hObject, std::vector<int>& indices);

    static void GetGlobalProperty(v8::Local<v8::Name> hName, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetGlobalProperty(v8::Local<v8::Name> hName, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryGlobalProperty(v8::Local<v8::Name> hName, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteGlobalProperty(v8::Local<v8::Name> hName, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void GetGlobalProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetGlobalProperty(unsigned __int32 index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryGlobalProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteGlobalProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void HostObjectConstructorCallHandler(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void CreateFunctionForHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void InvokeHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);

    static void GetHostObjectProperty(v8::Local<v8::String> hName, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetHostObjectProperty(v8::Local<v8::String> hName, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryHostObjectProperty(v8::Local<v8::String> hName, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteHostObjectProperty(v8::Local<v8::String> hName, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void GetHostObjectProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void SetHostObjectProperty(unsigned __int32 index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info);
    static void QueryHostObjectProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static void DeleteHostObjectProperty(unsigned __int32 index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void InvokeHostObject(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void DisposeWeakHandle(v8::Isolate* pIsolate, Persistent<v8::Object>* phObject, void* pvV8ObjectCache);

    v8::Local<v8::Value> ImportValue(const V8Value& value);
    V8Value ExportValue(v8::Local<v8::Value> hValue);
    void ImportValues(const std::vector<V8Value>& values, std::vector<v8::Local<v8::Value>>& importedValues);

    void Verify(const v8::TryCatch& tryCatch);
    void VerifyNotOutOfMemory();
    void ThrowScriptException(const HostException& exception);

    StdString m_Name;
    SharedPtr<V8IsolateImpl> m_spIsolateImpl;
    Persistent<v8::Context> m_hContext;
    Persistent<v8::Object> m_hGlobal;
    std::vector<std::pair<StdString, Persistent<v8::Object>>> m_GlobalMembersStack;
    Persistent<v8::String> m_hHostObjectCookieName;
    Persistent<v8::String> m_hHostExceptionName;
    Persistent<v8::String> m_hAccessTokenName;
    Persistent<v8::Object> m_hAccessToken;
    Persistent<v8::FunctionTemplate> m_hHostObjectTemplate;
    Persistent<v8::FunctionTemplate> m_hHostDelegateTemplate;
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
