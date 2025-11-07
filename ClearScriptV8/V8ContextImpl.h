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

class V8ContextImpl final: public V8Context
{
    PROHIBIT_COPY(V8ContextImpl)

public:

    V8ContextImpl(V8IsolateImpl* pIsolateImpl, const StdString& name);
    V8ContextImpl(SharedPtr<V8IsolateImpl>&& spIsolateImpl, const StdString& name, const Options& options);
    static size_t GetInstanceCount();

    const StdString& GetName() const { return m_Name; }
    const Persistent<v8::Context>& GetContext() const { return m_hContext; }

    virtual size_t GetMaxIsolateHeapSize() override;
    virtual void SetMaxIsolateHeapSize(size_t value) override;
    virtual double GetIsolateHeapSizeSampleInterval() override;
    virtual void SetIsolateHeapSizeSampleInterval(double value) override;

    virtual size_t GetMaxIsolateStackUsage() override;
    virtual void SetMaxIsolateStackUsage(size_t value) override;

    virtual void CallWithLock(CallWithLockCallback* pCallback, void* pvAction) override;
    virtual void CallWithLockWithArg(CallWithLockWithArgCallback* pCallback, void* pvAction, void* pvArg) override;

    virtual V8Value GetRootObject() override;
    virtual void SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers) override;

    virtual void AwaitDebuggerAndPause() override;
    virtual void CancelAwaitDebugger() override;

    virtual V8Value Execute(const V8DocumentInfo& documentInfo, const StdString& code, bool evaluate) override;
    virtual V8Value ExecuteScriptFromUtf8(const V8DocumentInfo& documentInfo, const char* code, int codeLength, size_t codeDigest, bool evaluate) override;

    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code) override;
    virtual V8ScriptHolder* CompileScriptFromUtf8(const V8DocumentInfo& documentInfo, const char* code, int codeLength, size_t codeDigest) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted) override;
    virtual V8ScriptHolder* Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult) override;

    virtual bool CanExecute(const SharedPtr<V8ScriptHolder>& spHolder) override;
    virtual V8Value Execute(const SharedPtr<V8ScriptHolder>& spHolder, bool evaluate) override;

    virtual void Interrupt() override;
    virtual void CancelInterrupt() override;
    virtual bool GetEnableIsolateInterruptPropagation() override;
    virtual void SetEnableIsolateInterruptPropagation(bool value) override;
    virtual bool GetDisableIsolateHeapSizeViolationInterrupt() override;
    virtual void SetDisableIsolateHeapSizeViolationInterrupt(bool value) override;

    virtual void GetIsolateHeapStatistics(v8::HeapStatistics& heapStatistics) override;
    virtual V8Isolate::Statistics GetIsolateStatistics() override;
    virtual Statistics GetStatistics() override;
    virtual void CollectGarbage(bool exhaustive) override;
    virtual void OnAccessSettingsChanged() override;

    virtual bool BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples) override;
    virtual bool EndCpuProfile(const StdString& name, V8Isolate::CpuProfileCallback* pCallback, void* pvArg) override;
    virtual void CollectCpuProfileSample() override;
    virtual uint32_t GetCpuProfileSampleInterval() override;
    virtual void SetCpuProfileSampleInterval(uint32_t value) override;

    virtual void WriteIsolateHeapSnapshot(void* pvStream) override;

    virtual void Flush() override;
    virtual void Destroy() override;

    V8Value GetV8ObjectProperty(void* pvObject, const StdString& name);
    bool TryGetV8ObjectProperty(void* pvObject, const StdString& name, V8Value& value);
    void SetV8ObjectProperty(void* pvObject, const StdString& name, const V8Value& value);
    bool DeleteV8ObjectProperty(void* pvObject, const StdString& name);
    void GetV8ObjectPropertyNames(void* pvObject, bool includeIndices, std::vector<StdString>& names);

    V8Value GetV8ObjectProperty(void* pvObject, int index);
    void SetV8ObjectProperty(void* pvObject, int index, const V8Value& value);
    bool DeleteV8ObjectProperty(void* pvObject, int index);
    void GetV8ObjectPropertyIndices(void* pvObject, std::vector<int>& indices);

    V8Value InvokeV8Object(void* pvObject, bool asConstructor, const std::vector<V8Value>& args);
    V8Value InvokeV8ObjectMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args);

    void GetV8ObjectArrayBufferOrViewInfo(void* pvObject, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length);
    void InvokeWithV8ObjectArrayBufferOrViewData(void* pvObject, V8ObjectHelpers::ArrayBufferOrViewDataCallback* pCallback, void* pvArg);

    void InitializeImportMeta(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta);
    v8::MaybeLocal<v8::Promise> ImportModule(v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> hResourceName, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> hImportAssertions);
    v8::MaybeLocal<v8::Module> ResolveModule(v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer);

private:

    class Scope final
    {
        PROHIBIT_COPY(Scope)
        PROHIBIT_HEAP(Scope)

    public:

        explicit Scope(V8ContextImpl* pContextImpl):
            m_ContextScope(pContextImpl->m_hContext)
        {
        }

    private:

        v8::Context::Scope m_ContextScope;
    };

    struct ModuleCacheEntry final
    {
        V8DocumentInfo DocumentInfo;
        size_t CodeDigest;
        Persistent<v8::Module> hModule;
        std::vector<uint8_t> CacheBytes;
        Persistent<v8::Object> hMetaHolder;
    };

    struct SyntheticModuleExport final
    {
        Persistent<v8::String> hName;
        Persistent<v8::Value> hValue;
    };

    struct SyntheticModuleEntry final
    {
        Persistent<v8::Module> hModule;
        std::vector<SyntheticModuleExport> Exports;
    };

    const Persistent<v8::Private>& GetHostObjectHolderKey() const
    {
        return m_spIsolateImpl->GetHostObjectHolderKey();
    }

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

    v8::Local<v8::Symbol> GetAsyncIteratorSymbol()
    {
        return m_spIsolateImpl->GetAsyncIteratorSymbol();
    }

    v8::Local<v8::Symbol> GetToStringTagSymbol()
    {
        return m_spIsolateImpl->GetToStringTagSymbol();
    }

    v8::Local<v8::Object> CreateObject()
    {
        return m_spIsolateImpl->CreateObject();
    }

    v8::Local<v8::Number> CreateNumber(double value)
    {
        return m_spIsolateImpl->CreateNumber(value);
    }

    v8::Local<v8::Integer> CreateInteger(int32_t value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::Local<v8::Integer> CreateInteger(uint32_t value)
    {
        return m_spIsolateImpl->CreateInteger(value);
    }

    v8::Local<v8::BigInt> CreateBigInt(int64_t value)
    {
        return m_spIsolateImpl->CreateBigInt(value);
    }

    v8::Local<v8::BigInt> CreateBigInt(uint64_t value)
    {
        return m_spIsolateImpl->CreateBigInt(value);
    }

    v8::MaybeLocal<v8::String> CreateString(const StdString& value, v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return m_spIsolateImpl->CreateString(value, type);
    }

    v8::MaybeLocal<v8::String> CreateStringFromUtf8(const char* data, int length, v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return m_spIsolateImpl->CreateStringFromUtf8(data, length, type);
    }

    template <int N>
    v8::Local<v8::String> CreateString(const char (&value)[N], v8::NewStringType type = v8::NewStringType::kNormal)
    {
        return m_spIsolateImpl->CreateString(value, type);
    }

    virtual StdString CreateStdString(v8::Local<v8::Value> hValue) override
    {
        return m_spIsolateImpl->CreateStdString(hValue);
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

    v8::Local<v8::PrimitiveArray> CreatePrimitiveArray(int length)
    {
        return m_spIsolateImpl->CreatePrimitiveArray(length);
    }

    void SetPrimitiveArrayItem(v8::Local<v8::PrimitiveArray> hArray, int index, v8::Local<v8::Primitive> hItem)
    {
        m_spIsolateImpl->SetPrimitiveArrayItem(hArray, index, hItem);
    }

    v8::Local<v8::Primitive> GetPrimitiveArrayItem(v8::Local<v8::PrimitiveArray> hArray, int index)
    {
        return m_spIsolateImpl->GetPrimitiveArrayItem(hArray, index);
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

    v8::Local<v8::SharedArrayBuffer> CreateSharedArrayBuffer(const std::shared_ptr<v8::BackingStore>& spBackingStore)
    {
        return m_spIsolateImpl->CreateSharedArrayBuffer(spBackingStore);
    }

    v8::MaybeLocal<v8::UnboundScript> CompileUnboundScript(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = m_spIsolateImpl->CompileUnboundScript(pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ScriptCount;
        }

        return result;
    }

    v8::MaybeLocal<v8::Module> CompileModule(v8::ScriptCompiler::Source* pSource, v8::ScriptCompiler::CompileOptions options = v8::ScriptCompiler::kNoCompileOptions, v8::ScriptCompiler::NoCacheReason noCacheReason = v8::ScriptCompiler::kNoCacheNoReason)
    {
        auto result = m_spIsolateImpl->CompileModule(pSource, options, noCacheReason);

        if (!result.IsEmpty())
        {
            ++m_Statistics.ModuleCount;
        }

        return result;
    }

    v8::Local<v8::Module> CreateSyntheticModule(v8::Local<v8::String> moduleName, const std::vector<v8::Local<v8::String>>& exportNames, v8::Module::SyntheticModuleEvaluationSteps evaluationSteps)
    {
        return m_spIsolateImpl->CreateSyntheticModule(moduleName, exportNames, evaluationSteps);
    }

    v8::Maybe<bool> SetSyntheticModuleExport(v8::Local<v8::Module> hModule, v8::Local<v8::String> hName, v8::Local<v8::Value> hValue)
    {
        return m_spIsolateImpl->SetSyntheticModuleExport(hModule, hName, hValue);
    }

    v8::ScriptOrigin CreateScriptOrigin(v8::Local<v8::Value> hResourceName, int lineOffset = 0, int columnOffset = 0, bool isSharedCrossOrigin = false, int scriptId = -1, v8::Local<v8::Value> hSourceMapUrl = v8::Local<v8::Value>(), bool isOpaque = false, bool isWasm = false, bool isModule = false, v8::Local<v8::PrimitiveArray> hHostDefinedOptions = v8::Local<v8::PrimitiveArray>())
    {
        return m_spIsolateImpl->CreateScriptOrigin(hResourceName, lineOffset, columnOffset, isSharedCrossOrigin, scriptId, hSourceMapUrl, isOpaque, isWasm, isModule, hHostDefinedOptions);
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

    void TerminateExecution(bool force)
    {
        return m_spIsolateImpl->TerminateExecution(force);
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

    void RequestGarbageCollectionForTesting(v8::Isolate::GarbageCollectionType type)
    {
        m_spIsolateImpl->RequestGarbageCollectionForTesting(type);
    }

    void ClearCachesForTesting()
    {
        m_spIsolateImpl->ClearCachesForTesting();
    }

    v8::Local<v8::StackFrame> GetStackFrame(v8::Local<v8::StackTrace> hStackTrace, uint32_t index)
    {
        return m_spIsolateImpl->GetStackFrame(hStackTrace, index);
    }

    v8::Local<v8::String> GetTypeOf(v8::Local<v8::Value> hValue)
    {
        return m_spIsolateImpl->GetTypeOf(hValue);
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

    void GetV8ObjectPropertyNames(v8::Local<v8::Object> hObject, std::vector<StdString>& names, v8::PropertyFilter filter, v8::IndexFilter indexFilter);
    void GetV8ObjectPropertyIndices(v8::Local<v8::Object> hObject, std::vector<int>& indices, v8::PropertyFilter filter);

    static v8::Intercepted GetGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetGlobalProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static v8::Intercepted GetGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetGlobalProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetGlobalPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void HostObjectConstructorCallHandler(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetHostObjectAsyncIterator(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetFastHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetFastHostObjectAsyncIterator(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetHostObjectJson(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void CreateFunctionForHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void InvokeHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info);

    static v8::Intercepted GetHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static v8::Intercepted GetHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetHostObjectProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static v8::Intercepted GetFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetFastHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetFastHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info);

    static v8::Intercepted GetFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info);
    static v8::Intercepted SetFastHostObjectProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info);
    static v8::Intercepted QueryFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info);
    static v8::Intercepted DeleteFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info);
    static void GetFastHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info);

    static void InvokeHostObject(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void InvokeFastHostObject(const v8::FunctionCallbackInfo<v8::Value>& info);

    static void FlushCallback(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void PerformanceNowCallback(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void PerformanceSleepCallback(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void SetModuleResultCallback(const v8::FunctionCallbackInfo<v8::Value>& info);

    static void GetPromiseStateCallback(const v8::FunctionCallbackInfo<v8::Value>& info);
    static void GetPromiseResultCallback(const v8::FunctionCallbackInfo<v8::Value>& info);

    v8::MaybeLocal<v8::Promise> ImportModule(const V8DocumentInfo* pSourceDocumentInfo, v8::Local<v8::String> hSpecifier);
    v8::MaybeLocal<v8::Module> ResolveModule(v8::Local<v8::String> hSpecifier, const V8DocumentInfo* pSourceDocumentInfo);
    static v8::MaybeLocal<v8::Value> PopulateSyntheticModule(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule);
    v8::MaybeLocal<v8::Value> PopulateSyntheticModule(v8::Local<v8::Module> hModule);

    static void DisposeWeakHandle(v8::Isolate* pIsolate, Persistent<v8::Object>* phObject, HostObjectHolder* pHolder, void* pvV8ObjectCache);

    bool TryGetCachedModuleInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo);
    bool TryGetCachedModuleInfo(v8::Local<v8::Module> hModule, V8DocumentInfo& documentInfo);
    bool TryGetCachedModuleMetaHolder(v8::Local<v8::Module> hModule, v8::Local<v8::Object>& hMetaHolder);
    bool TryGetCachedModuleMetaHolder(uint64_t uniqueId, v8::Local<v8::Object>& hMetaHolder);
    v8::Local<v8::Module> GetCachedModule(uint64_t uniqueId, size_t codeDigest);
    v8::Local<v8::Module> GetCachedModule(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes);
    void CacheModule(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::Module> hModule);
    void CacheModule(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::Module> hModule, const std::vector<uint8_t>& cacheBytes);
    void SetCachedModuleCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes);
    void ClearModuleCache();

    bool TryGetCachedScriptInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo);
    v8::Local<v8::UnboundScript> GetCachedScript(uint64_t uniqueId, size_t codeDigest);
    v8::Local<v8::UnboundScript> GetCachedScript(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes);
    void CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript);
    void CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript, const std::vector<uint8_t>& cacheBytes);
    void SetCachedScriptCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes);

    v8::Local<v8::Value> ImportValue(const V8Value& value);
    V8Value ExportValue(v8::Local<v8::Value> hValue);
    void ImportValues(const std::vector<V8Value>& values, std::vector<v8::Local<v8::Value>>& importedValues);

    v8::ScriptOrigin CreateScriptOrigin(const V8DocumentInfo& documentInfo);
    void Verify(const V8IsolateImpl::ExecutionScope& isolateExecutionScope, const v8::TryCatch& tryCatch);
    void VerifyNotOutOfMemory();
    void ThrowScriptException(const HostException& exception);

    StdString m_Name;
    SharedPtr<V8IsolateImpl> m_spIsolateImpl;
    Persistent<v8::Context> m_hContext;
    std::vector<std::pair<StdString, Persistent<v8::Object>>> m_GlobalMembersStack;
    Persistent<v8::Symbol> m_hIsHostObjectKey;
    Persistent<v8::Symbol> m_hModuleResultKey;
    Persistent<v8::Symbol> m_hMissingPropertyValue;
    Persistent<v8::String> m_hHostExceptionKey;
    Persistent<v8::Private> m_hCacheKey;
    Persistent<v8::Private> m_hAccessTokenKey;
    Persistent<v8::Object> m_hAccessToken;
    Persistent<v8::String> m_hInternalUseOnly;
    Persistent<v8::String> m_hStackKey;
    Persistent<v8::String> m_hObjectNotInvocable;
    Persistent<v8::String> m_hMethodOrPropertyNotFound;
    Persistent<v8::String> m_hPropertyValueNotInvocable;
    Persistent<v8::String> m_hInvalidModuleRequest;
    Persistent<v8::String> m_hConstructorKey;
    Persistent<v8::String> m_hSetModuleResultKey;
    Persistent<v8::FunctionTemplate> m_hHostObjectTemplate;
    Persistent<v8::FunctionTemplate> m_hHostInvocableTemplate;
    Persistent<v8::FunctionTemplate> m_hHostDelegateTemplate;
    Persistent<v8::FunctionTemplate> m_hFastHostObjectTemplate;
    Persistent<v8::FunctionTemplate> m_hFastHostFunctionTemplate;
    Persistent<v8::Function> m_hToIteratorFunction;
    Persistent<v8::Function> m_hToAsyncIteratorFunction;
    Persistent<v8::Function> m_hToJsonFunction;
    Persistent<v8::Function> m_hFlushFunction;
    Persistent<v8::Function> m_hGetModuleResultFunction;
    Persistent<v8::Value> m_hAsyncGeneratorConstructor;
    Persistent<v8::Value> m_hTerminationException;
    SharedPtr<V8WeakContextBinding> m_spWeakBinding;
    std::list<ModuleCacheEntry> m_ModuleCache;
    std::list<SyntheticModuleEntry> m_SyntheticModuleData;
    Statistics m_Statistics;
    bool m_DateTimeConversionEnabled;
    bool m_HideHostExceptions;
    bool m_AllowHostObjectConstructorCall;
    bool m_ChangedTimerResolution;
    void* m_pvV8ObjectCache;
    double m_RelativeTimeOrigin;
};

//-----------------------------------------------------------------------------
// SharedPtrTraits<V8ContextImpl>
//-----------------------------------------------------------------------------

template <>
struct SharedPtrTraits<V8ContextImpl> final: StaticBase
{
    static void Destroy(V8ContextImpl* pTarget)
    {
        pTarget->Destroy();
    }
};
