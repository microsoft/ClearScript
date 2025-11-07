// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// support for v8::Maybe and v8::MaybeLocal
//-----------------------------------------------------------------------------

class FromMaybeScope final
{
};

//-----------------------------------------------------------------------------

class FromMaybeFailure final
{
};

//-----------------------------------------------------------------------------

template <typename T>
inline T FromMaybe(FromMaybeScope& /*scope*/, const v8::Maybe<T>& maybe)
{
    T result;
    if (maybe.To(&result))
    {
        return result;
    }

    throw FromMaybeFailure();
}

//-----------------------------------------------------------------------------

template <typename T>
inline v8::Local<T> FromMaybe(FromMaybeScope& /*scope*/, const v8::MaybeLocal<T>& maybe)
{
    v8::Local<T> result;
    if (maybe.ToLocal(&result))
    {
        return result;
    }

    throw FromMaybeFailure();
}

//-----------------------------------------------------------------------------

template <typename T>
inline T FromMaybeDefault(const v8::Maybe<T>& maybe, const T& defaultValue = T())
{
    return maybe.FromMaybe(defaultValue);
}

//-----------------------------------------------------------------------------

template <typename T>
inline v8::Local<T> FromMaybeDefault(const v8::MaybeLocal<T>& maybe, const v8::Local<T>& defaultValue = v8::Local<T>())
{
    return maybe.FromMaybe(defaultValue);
}

//-----------------------------------------------------------------------------

#define FROM_MAYBE_TRY \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        FromMaybeScope t_FromMaybeScope; \
        DEFAULT_WARNING(4456) \
        try \
        {

#define FROM_MAYBE_CATCH \
            IGNORE_UNUSED(t_FromMaybeScope); \
        } \
        catch (const FromMaybeFailure&) \
        { \

#define FROM_MAYBE_END \
            IGNORE_UNUSED(t_FromMaybeScope); \
        } \
    }

#define FROM_MAYBE_CATCH_CONSUME \
    FROM_MAYBE_CATCH \
    FROM_MAYBE_END

#define FROM_MAYBE(...) \
    (::FromMaybe(t_FromMaybeScope, __VA_ARGS__))

#define FROM_MAYBE_DEFAULT(...) \
    (::FromMaybeDefault(__VA_ARGS__))

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

inline v8::Local<v8::String> ValueAsString(const v8::Local<v8::Value>& hValue)
{
    if (hValue.IsEmpty())
    {
        return v8::Local<v8::String>();
    }

    if (hValue->IsString())
    {
        return hValue.As<v8::String>();
    }

    if (hValue->IsStringObject())
    {
        return hValue.As<v8::StringObject>()->ValueOf();
    }

    return v8::Local<v8::String>();
}

//-----------------------------------------------------------------------------

inline v8::Local<v8::Object> ValueAsObject(const v8::Local<v8::Value>& hValue)
{
    return (!hValue.IsEmpty() && hValue->IsObject()) ? hValue.As<v8::Object>() : v8::Local<v8::Object>();
}

//-----------------------------------------------------------------------------

inline v8::Local<v8::External> ValueAsExternal(const v8::Local<v8::Value>& hValue)
{
    return (!hValue.IsEmpty() && hValue->IsExternal()) ? hValue.As<v8::External>() : v8::Local<v8::External>();
}

//-----------------------------------------------------------------------------

inline v8::Local<v8::BigInt> ValueAsBigInt(const v8::Local<v8::Value>& hValue)
{
    if (hValue.IsEmpty())
    {
        return v8::Local<v8::BigInt>();
    }

    if (hValue->IsBigInt())
    {
        return hValue.As<v8::BigInt>();
    }

    if (hValue->IsBigIntObject())
    {
        return hValue.As<v8::BigIntObject>()->ValueOf();
    }

    return v8::Local<v8::BigInt>();
}

//-----------------------------------------------------------------------------

inline bool TryGetValueAsBoolean(const SharedPtr<V8IsolateImpl>& spIsolateImpl, const v8::Local<v8::Value>& hValue, bool& value)
{
    if (hValue.IsEmpty())
    {
        return false;
    }

    if (hValue->IsBoolean())
    {
        value = spIsolateImpl->BooleanValue(hValue);
        return true;
    }

    if (hValue->IsBooleanObject())
    {
        value = hValue.As<v8::BooleanObject>()->ValueOf();
        return true;
    }

    return false;
}

//-----------------------------------------------------------------------------

inline bool TryGetValueAsNumber(v8::Local<v8::Context> hContext, const v8::Local<v8::Value>& hValue, double& value)
{
    if (hValue.IsEmpty())
    {
        return false;
    }

    FROM_MAYBE_TRY

        if (hValue->IsNumber())
        {
            value = FROM_MAYBE(hValue->NumberValue(hContext));
            return true;
        }

        if (hValue->IsNumberObject())
        {
            value = hValue.As<v8::NumberObject>()->ValueOf();
            return true;
        }

    FROM_MAYBE_CATCH_CONSUME

    return false;
}

//-----------------------------------------------------------------------------

template <typename TInfo>
inline V8ContextImpl* GetContextImplFromHolder(const TInfo& info)
{
    auto hHolder = info.HolderV2();
    if (!hHolder.IsEmpty() && hHolder->InternalFieldCount() > 0)
    {
        auto hField = hHolder->GetInternalField(0);
        if (!hField.IsEmpty() && hField->IsValue() && !hField.template As<v8::Value>()->IsUndefined())
        {
            return static_cast<V8ContextImpl*>(hHolder->GetAlignedPointerFromInternalField(0));
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

template <typename TInfo>
inline V8ContextImpl* GetContextImplFromData(const TInfo& info)
{
    auto hContextImpl = ::ValueAsExternal(info.Data());
    return !hContextImpl.IsEmpty() ? static_cast<V8ContextImpl*>(hContextImpl->Value()) : nullptr;
}

//-----------------------------------------------------------------------------

template <typename TVariable, typename TValue>
inline void SetIfSameType(TVariable& /*variable*/, TValue /*value*/)
{
}

//-----------------------------------------------------------------------------

template <typename TVariable>
inline void SetIfSameType(TVariable& variable, TVariable value)
{
    variable = value;
}

//-----------------------------------------------------------------------------

template <typename T>
inline v8::Intercepted GetCallbackStatus(const T& /*value*/)
{
    return v8::Intercepted::kYes;
}

//-----------------------------------------------------------------------------

template <typename T>
inline v8::Intercepted GetCallbackStatus(const v8::Local<T>& hValue)
{
    return hValue.IsEmpty() ? v8::Intercepted::kNo : v8::Intercepted::kYes;
}

//-----------------------------------------------------------------------------

template <typename TInfo, typename TResult, typename TStatus>
inline void ProcessCallbackResult(const TResult& result, const TInfo& info, TStatus& status)
{
    ::SetIfSameType(status, ::GetCallbackStatus(result));
    info.GetReturnValue().Set(result);
}

//-----------------------------------------------------------------------------

template <typename TInfo, typename TStatus>
inline void ProcessVoidCallbackResult(const TInfo& info, TStatus& status)
{
    IGNORE_UNUSED(info);
    ::SetIfSameType(status, v8::Intercepted::kYes);
}

//-----------------------------------------------------------------------------
// V8ContextImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        V8IsolateImpl::Scope t_IsolateScope(*m_spIsolateImpl); \
        DEFAULT_WARNING(4456)

#define END_ISOLATE_SCOPE \
        IGNORE_UNUSED(t_IsolateScope); \
    }

#define BEGIN_CONTEXT_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        Scope t_ContextScope(this); \
        DEFAULT_WARNING(4456)

#define END_CONTEXT_SCOPE \
        IGNORE_UNUSED(t_ContextScope); \
    }

#define BEGIN_EXECUTION_SCOPE \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        V8IsolateImpl::ExecutionScope t_ExecutionScope(*m_spIsolateImpl); \
        V8IsolateImpl::TryCatch t_TryCatch(*m_spIsolateImpl); \
        DEFAULT_WARNING(4456)

#define END_EXECUTION_SCOPE \
        IGNORE_UNUSED(t_TryCatch); \
        IGNORE_UNUSED(t_ExecutionScope); \
    }

#define EXECUTION_STARTED \
    (t_ExecutionScope.ExecutionStarted())

#define BEGIN_DOCUMENT_SCOPE(DOCUMENT_INFO) \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        V8IsolateImpl::DocumentScope t_DocumentScope(*m_spIsolateImpl, DOCUMENT_INFO); \
        DEFAULT_WARNING(4456)

#define END_DOCUMENT_SCOPE \
        IGNORE_UNUSED(t_DocumentScope); \
    }

#define VERIFY(RESULT) \
    (Verify(t_ExecutionScope, t_TryCatch, RESULT))

#define VERIFY_MAYBE(RESULT) \
    VERIFY(FROM_MAYBE_DEFAULT(RESULT))

#define VERIFY_CHECKPOINT() \
    (Verify(t_ExecutionScope, t_TryCatch))

#define CALLBACK_RETURN(RESULT) \
    BEGIN_COMPOUND_MACRO \
        ::ProcessCallbackResult(RESULT, info, t_CallbackStatus); \
        return; \
    END_COMPOUND_MACRO

#define CALLBACK_RETURN_VOID() \
    BEGIN_COMPOUND_MACRO \
        ::ProcessVoidCallbackResult(info, t_CallbackStatus); \
        return; \
    END_COMPOUND_MACRO

#define CALLBACK_ENTER \
    auto t_CallbackStatus = v8::Intercepted::kNo; \
    ([&] () noexcept -> void \
        { \

#define CALLBACK_EXIT \
        } \
    )(); \
    return t_CallbackStatus;

constexpr enum class DummyCallbackStatus {} t_CallbackStatus {};

//-----------------------------------------------------------------------------

static std::atomic<size_t> s_InstanceCount(0);

//-----------------------------------------------------------------------------

V8ContextImpl::V8ContextImpl(V8IsolateImpl* pIsolateImpl, const StdString& name):
    V8ContextImpl(SharedPtr<V8IsolateImpl>(pIsolateImpl), name, Options())
{
}

//-----------------------------------------------------------------------------

V8ContextImpl::V8ContextImpl(SharedPtr<V8IsolateImpl>&& spIsolateImpl, const StdString& name, const Options& options):
    m_Name(name),
    m_spIsolateImpl(std::move(spIsolateImpl)),
    m_DateTimeConversionEnabled(::HasFlag(options.Flags, Flags::EnableDateTimeConversion)),
    m_HideHostExceptions(::HasFlag(options.Flags, Flags::HideHostExceptions)),
    m_AllowHostObjectConstructorCall(false),
    m_ChangedTimerResolution(false),
    m_pvV8ObjectCache(nullptr)
{
    VerifyNotOutOfMemory();

    if (::HasAllFlags(options.Flags, Flags::AddPerformanceObject, Flags::SetTimerResolution))
    {
        m_ChangedTimerResolution = HighResolutionClock::SetTimerResolution();
    }

    auto timeOrigin = HighResolutionClock::GetMillisecondsSinceUnixEpoch();
    m_RelativeTimeOrigin = HighResolutionClock::GetRelativeMilliseconds();

    BEGIN_ISOLATE_SCOPE
    FROM_MAYBE_TRY

        if (::HasFlag(options.Flags, Flags::DisableGlobalMembers))
        {
            m_hContext = CreatePersistent(CreateContext());
        }
        else
        {
            auto hGlobalTemplate = CreateObjectTemplate();
            hGlobalTemplate->SetInternalFieldCount(1);
            hGlobalTemplate->SetHandler(v8::NamedPropertyHandlerConfiguration(GetGlobalProperty, SetGlobalProperty, QueryGlobalProperty, DeleteGlobalProperty, GetGlobalPropertyNames, v8::Local<v8::Value>(), v8::PropertyHandlerFlags::kNonMasking));
            hGlobalTemplate->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetGlobalProperty, SetGlobalProperty, QueryGlobalProperty, DeleteGlobalProperty, GetGlobalPropertyIndices));

            m_hContext = CreatePersistent(CreateContext(nullptr, hGlobalTemplate));
            if (!m_hContext.IsEmpty())
            {
                auto hGlobal = m_hContext->Global();
                if (!hGlobal.IsEmpty() && (hGlobal->InternalFieldCount() > 0))
                {
                    hGlobal->SetAlignedPointerInInternalField(0, this);
                }
            }
        }

        if (m_hContext.IsEmpty())
        {
            throw FromMaybeFailure();
        }

        auto hContextImpl = CreateExternal(this);

        v8::Local<v8::FunctionTemplate> hGetHostObjectIteratorFunction;
        v8::Local<v8::FunctionTemplate> hGetHostObjectAsyncIteratorFunction;
        v8::Local<v8::FunctionTemplate> hGetFastHostObjectIteratorFunction;
        v8::Local<v8::FunctionTemplate> hGetFastHostObjectAsyncIteratorFunction;
        v8::Local<v8::FunctionTemplate> hGetHostObjectJsonFunction;
        v8::Local<v8::FunctionTemplate> hHostDelegateToFunctionFunction;

        BEGIN_CONTEXT_SCOPE

            m_hContext->SetAlignedPointerInEmbedderData(1, this);

            m_hIsHostObjectKey = CreatePersistent(CreateSymbol());
            ASSERT_EVAL(FROM_MAYBE(m_hContext->Global()->Set(m_hContext, CreateString("isHostObjectKey"), m_hIsHostObjectKey)));

            m_hModuleResultKey = CreatePersistent(CreateSymbol());
            ASSERT_EVAL(FROM_MAYBE(m_hContext->Global()->Set(m_hContext, CreateString("moduleResultKey"), m_hModuleResultKey)));

            ASSERT_EVAL(FROM_MAYBE(m_hContext->Global()->Set(m_hContext, CreateString("getPromiseState"), FROM_MAYBE(v8::Function::New(m_hContext, GetPromiseStateCallback, hContextImpl)))));
            ASSERT_EVAL(FROM_MAYBE(m_hContext->Global()->Set(m_hContext, CreateString("getPromiseResult"), FROM_MAYBE(v8::Function::New(m_hContext, GetPromiseResultCallback, hContextImpl)))));

            m_hMissingPropertyValue = CreatePersistent(CreateSymbol());
            m_hHostExceptionKey = CreatePersistent(CreateString("hostException"));
            m_hCacheKey = CreatePersistent(CreatePrivate());
            m_hAccessTokenKey = CreatePersistent(CreatePrivate());
            m_hInternalUseOnly = CreatePersistent(CreateString("The invoked function is for ClearScript internal use only"));
            m_hStackKey = CreatePersistent(CreateString("stack"));
            m_hObjectNotInvocable = CreatePersistent(CreateString("The object does not support invocation"));
            m_hMethodOrPropertyNotFound = CreatePersistent(CreateString("Method or property not found"));
            m_hPropertyValueNotInvocable = CreatePersistent(CreateString("The property value does not support invocation"));
            m_hInvalidModuleRequest = CreatePersistent(CreateString("Invalid module load request"));
            m_hConstructorKey = CreatePersistent(CreateString("constructor"));
            m_hSetModuleResultKey = CreatePersistent(CreateString("setResult"));

            hGetHostObjectIteratorFunction = CreateFunctionTemplate(GetHostObjectIterator, hContextImpl);
            hGetHostObjectAsyncIteratorFunction = CreateFunctionTemplate(GetHostObjectAsyncIterator, hContextImpl);
            hGetFastHostObjectIteratorFunction = CreateFunctionTemplate(GetFastHostObjectIterator, hContextImpl);
            hGetFastHostObjectAsyncIteratorFunction = CreateFunctionTemplate(GetFastHostObjectAsyncIterator, hContextImpl);
            hGetHostObjectJsonFunction = CreateFunctionTemplate(GetHostObjectJson, hContextImpl);
            hHostDelegateToFunctionFunction = CreateFunctionTemplate(CreateFunctionForHostDelegate, hContextImpl);

            m_hAccessToken = CreatePersistent(CreateObject());
            m_hFlushFunction = CreatePersistent(FROM_MAYBE(v8::Function::New(m_hContext, FlushCallback)));
            m_hTerminationException = CreatePersistent(v8::Exception::Error(CreateString("Script execution was interrupted")));

            if (::HasFlag(options.Flags, Flags::AddPerformanceObject))
            {
                auto hPerformance = CreateObject();
                ASSERT_EVAL(FROM_MAYBE(hPerformance->DefineOwnProperty(m_hContext, CreateString("timeOrigin"), CreateNumber(timeOrigin), ::CombineFlags(v8::ReadOnly, v8::DontDelete))));
                ASSERT_EVAL(FROM_MAYBE(hPerformance->DefineOwnProperty(m_hContext, CreateString("now"), FROM_MAYBE(v8::Function::New(m_hContext, PerformanceNowCallback, hContextImpl)), ::CombineFlags(v8::ReadOnly, v8::DontDelete))));
                ASSERT_EVAL(FROM_MAYBE(hPerformance->DefineOwnProperty(m_hContext, CreateString("sleep"), FROM_MAYBE(v8::Function::New(m_hContext, PerformanceSleepCallback, hContextImpl)), ::CombineFlags(v8::ReadOnly, v8::DontDelete))));
                ASSERT_EVAL(FROM_MAYBE(m_hContext->Global()->DefineOwnProperty(m_hContext, CreateString("Performance"), hPerformance, v8::DontEnum)));
            }

        END_CONTEXT_SCOPE

        auto hToJSON = CreateString("toJSON");

        m_hHostObjectTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostObjectTemplate->SetClassName(CreateString("HostObject"));
        m_hHostObjectTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostObjectTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hHostObjectTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, hContextImpl));
        m_hHostObjectTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetHostObjectIteratorFunction);
        m_hHostObjectTemplate->PrototypeTemplate()->Set(GetAsyncIteratorSymbol(), hGetHostObjectAsyncIteratorFunction);
        m_hHostObjectTemplate->PrototypeTemplate()->Set(hToJSON, hGetHostObjectJsonFunction, ::CombineFlags(v8::ReadOnly, v8::DontDelete, v8::DontEnum));

        m_hHostInvocableTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostInvocableTemplate->SetClassName(CreateString("HostInvocable"));
        m_hHostInvocableTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostInvocableTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hHostInvocableTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, hContextImpl));
        m_hHostInvocableTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetHostObjectIteratorFunction);
        m_hHostInvocableTemplate->PrototypeTemplate()->Set(GetAsyncIteratorSymbol(), hGetHostObjectAsyncIteratorFunction);
        m_hHostInvocableTemplate->PrototypeTemplate()->Set(hToJSON, hGetHostObjectJsonFunction, ::CombineFlags(v8::ReadOnly, v8::DontDelete, v8::DontEnum));
        m_hHostInvocableTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, hContextImpl);

        m_hHostDelegateTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostDelegateTemplate->SetClassName(CreateString("HostDelegate"));
        m_hHostDelegateTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostDelegateTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hHostDelegateTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, hContextImpl));
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetHostObjectIteratorFunction);
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(GetAsyncIteratorSymbol(), hGetHostObjectAsyncIteratorFunction);
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(hToJSON, hGetHostObjectJsonFunction, ::CombineFlags(v8::ReadOnly, v8::DontDelete, v8::DontEnum));
        m_hHostDelegateTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, hContextImpl);
        m_hHostDelegateTemplate->InstanceTemplate()->SetHostDelegate(); // instructs our patched V8 typeof implementation to return "function" 
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(CreateString("toFunction"), hHostDelegateToFunctionFunction);

        m_hFastHostObjectTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hFastHostObjectTemplate->SetClassName(CreateString("FastHostObject"));
        m_hFastHostObjectTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hFastHostObjectTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetFastHostObjectProperty, SetFastHostObjectProperty, QueryFastHostObjectProperty, DeleteFastHostObjectProperty, GetFastHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hFastHostObjectTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetFastHostObjectProperty, SetFastHostObjectProperty, QueryFastHostObjectProperty, DeleteFastHostObjectProperty, GetFastHostObjectPropertyIndices, hContextImpl));
        m_hFastHostObjectTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetFastHostObjectIteratorFunction);
        m_hFastHostObjectTemplate->PrototypeTemplate()->Set(GetAsyncIteratorSymbol(), hGetFastHostObjectAsyncIteratorFunction);

        m_hFastHostFunctionTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hFastHostFunctionTemplate->SetClassName(CreateString("FastHostFunction"));
        m_hFastHostFunctionTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hFastHostFunctionTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetFastHostObjectProperty, SetFastHostObjectProperty, QueryFastHostObjectProperty, DeleteFastHostObjectProperty, GetFastHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hFastHostFunctionTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetFastHostObjectProperty, SetFastHostObjectProperty, QueryFastHostObjectProperty, DeleteFastHostObjectProperty, GetFastHostObjectPropertyIndices, hContextImpl));
        m_hFastHostFunctionTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetFastHostObjectIteratorFunction);
        m_hFastHostFunctionTemplate->PrototypeTemplate()->Set(GetAsyncIteratorSymbol(), hGetFastHostObjectAsyncIteratorFunction);
        m_hFastHostFunctionTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeFastHostObject, hContextImpl);
        m_hFastHostFunctionTemplate->InstanceTemplate()->SetHostDelegate(); // instructs our patched V8 typeof implementation to return "function" 
        m_hFastHostFunctionTemplate->PrototypeTemplate()->Set(CreateString("toFunction"), hHostDelegateToFunctionFunction);

        m_pvV8ObjectCache = HostObjectUtil::CreateV8ObjectCache();
        m_spIsolateImpl->AddContext(this, options);

    FROM_MAYBE_CATCH

        Teardown();
        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot initialize the script engine because a script exception is pending")), false);

    FROM_MAYBE_END
    END_ISOLATE_SCOPE

    ++s_InstanceCount;
}

//-----------------------------------------------------------------------------

size_t V8ContextImpl::GetInstanceCount()
{
    return s_InstanceCount;
}

//-----------------------------------------------------------------------------

size_t V8ContextImpl::GetMaxIsolateHeapSize()
{
    return m_spIsolateImpl->GetMaxHeapSize();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetMaxIsolateHeapSize(size_t value)
{
    m_spIsolateImpl->SetMaxHeapSize(value);
}

//-----------------------------------------------------------------------------

double V8ContextImpl::GetIsolateHeapSizeSampleInterval()
{
    return m_spIsolateImpl->GetHeapSizeSampleInterval();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetIsolateHeapSizeSampleInterval(double value)
{
    m_spIsolateImpl->SetHeapSizeSampleInterval(value);
}

//-----------------------------------------------------------------------------

size_t V8ContextImpl::GetMaxIsolateStackUsage()
{
    return m_spIsolateImpl->GetMaxStackUsage();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetMaxIsolateStackUsage(size_t value)
{
    m_spIsolateImpl->SetMaxStackUsage(value);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CallWithLock(CallWithLockCallback* pCallback, void* pvAction)
{
    VerifyNotOutOfMemory();

    BEGIN_ISOLATE_SCOPE

        (*pCallback)(pvAction);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CallWithLockWithArg(CallWithLockWithArgCallback* pCallback, void* pvAction, void* pvArg)
{
    VerifyNotOutOfMemory();

    BEGIN_ISOLATE_SCOPE

        (*pCallback)(pvAction, pvArg);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetRootObject()
{
    BEGIN_CONTEXT_SCOPE

        return ExportValue(m_hContext->Global());

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetGlobalProperty(const StdString& name, const V8Value& value, bool globalMembers)
{
    BEGIN_CONTEXT_SCOPE
    FROM_MAYBE_TRY

        auto hName = FROM_MAYBE(CreateString(name));
        auto hValue = ::ValueAsObject(ImportValue(value));

        v8::Local<v8::Object> hOldValue;
        if (FROM_MAYBE(m_hContext->Global()->HasOwnProperty(m_hContext, hName)))
        {
            hOldValue = ::ValueAsObject(FROM_MAYBE(m_hContext->Global()->GetRealNamedProperty(m_hContext, hName)));
        }

        auto result = FROM_MAYBE(m_hContext->Global()->DefineOwnProperty(m_hContext, hName, hValue, v8::ReadOnly));
        if (result && globalMembers && !hValue.IsEmpty())
        {
            if (!hOldValue.IsEmpty())
            {
                for (auto it = m_GlobalMembersStack.begin(); it != m_GlobalMembersStack.end(); it++)
                {
                    if (it->first == name)
                    {
                        Dispose(it->second);
                        m_GlobalMembersStack.erase(it);
                        break;
                    }
                }
            }

            m_GlobalMembersStack.emplace_back(name, CreatePersistent(hValue));
        }

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), false);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::AwaitDebuggerAndPause()
{
    m_spIsolateImpl->AwaitDebuggerAndPause();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CancelAwaitDebugger()
{
    m_spIsolateImpl->CancelAwaitDebugger();
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(const V8DocumentInfo& documentInfo, const StdString& code, bool evaluate)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto codeDigest = code.GetDigest();
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo));
        v8::Local<v8::Value> hResult;

        if (documentInfo.IsModule())
        {
            auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest);
            if (hModule.IsEmpty())
            {
                hModule = VERIFY_MAYBE(CompileModule(&source));
                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                CacheModule(documentInfo, codeDigest, hModule);
            }

            if (hModule->GetStatus() == v8::Module::kUninstantiated)
            {
                ASSERT_EVAL(VERIFY_MAYBE(hModule->InstantiateModule(m_hContext, V8IsolateImpl::ModuleResolveCallback)));
            }

            if (hModule->GetStatus() == v8::Module::kInstantiated)
            {
                hResult = VERIFY_MAYBE(hModule->Evaluate(m_hContext));
                if (hResult->IsPromise())
                {
                    auto hPromise = hResult.As<v8::Promise>();
                    if (hModule->IsGraphAsync() || (hPromise->State() == v8::Promise::PromiseState::kPending))
                    {
                        v8::Local<v8::Object> hMetaHolder;
                        if (TryGetCachedModuleMetaHolder(hModule, hMetaHolder))
                        {
                            if (m_hGetModuleResultFunction.IsEmpty())
                            {
                                auto hEngineInternal = FROM_MAYBE(m_hContext->Global()->Get(m_hContext, CreateString("EngineInternal"))).As<v8::Object>();
                                m_hGetModuleResultFunction = CreatePersistent(FROM_MAYBE(hEngineInternal->Get(m_hContext, CreateString("getModuleResult"))).As<v8::Function>());
                            }

                            v8::Local<v8::Value> args[] = { hResult, hMetaHolder };
                            hResult = FROM_MAYBE(m_hGetModuleResultFunction->Call(m_hContext, GetUndefined(), 2, args));
                        }
                    }
                    else if (hPromise->State() == v8::Promise::PromiseState::kFulfilled)
                    {
                        hResult = hPromise->Result();
                        if (evaluate && (hResult.IsEmpty() || hResult->IsUndefined()))
                        {
                            v8::Local<v8::Object> hMetaHolder;
                            if (TryGetCachedModuleMetaHolder(hModule, hMetaHolder))
                            {
                                auto hMeta = ::ValueAsObject(FROM_MAYBE(hMetaHolder->Get(m_hContext, 0)));
                                if (!hMeta.IsEmpty())
                                {
                                    hResult = FROM_MAYBE(hMeta->Get(m_hContext, m_hModuleResultKey));
                                }
                            }
                        }
                    }
                    else if (hPromise->State() == v8::Promise::PromiseState::kRejected)
                    {
                        auto hException = hPromise->Result();

                        if (hException->IsObject())
                        {
                            auto hExceptionObject = hException.As<v8::Object>();
                            auto hHostException = FROM_MAYBE(hExceptionObject->Get(m_hContext, m_hHostExceptionKey));
                            throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hExceptionObject), CreateStdString(FROM_MAYBE(hExceptionObject->Get(m_hContext, m_hStackKey))), EXECUTION_STARTED, ExportValue(hException), ExportValue(hHostException));
                        }

                        throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hException), StdString(), EXECUTION_STARTED, ExportValue(hException), V8Value(V8Value::Undefined));
                    }
                }
            }
            else
            {
                evaluate = false;
            }
        }
        else
        {
            auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest);
            if (hScript.IsEmpty())
            {
                hScript = VERIFY_MAYBE(CompileUnboundScript(&source));
                if (hScript.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                CacheScript(documentInfo, codeDigest, hScript);
            }

            hResult = VERIFY_MAYBE(hScript->BindToCurrentContext()->Run(m_hContext));
        }

        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::ExecuteScriptFromUtf8(const V8DocumentInfo& documentInfo, const char* code, int codeLength, size_t codeDigest, bool evaluate)
{
    if (documentInfo.IsModule())
    {
        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("ExecuteScriptFromUtf8 cannot execute modules")), false);
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest);
        if (hScript.IsEmpty())
        {
            v8::ScriptCompiler::Source source(FROM_MAYBE(CreateStringFromUtf8(code, codeLength)), CreateScriptOrigin(documentInfo));

            hScript = VERIFY_MAYBE(CompileUnboundScript(&source));
            if (hScript.IsEmpty())
            {
                throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
            }

            CacheScript(documentInfo, codeDigest, hScript);
        }

        v8::Local<v8::Value> hResult = VERIFY_MAYBE(hScript->BindToCurrentContext()->Run(m_hContext));

        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto codeDigest = code.GetDigest();
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo));
        std::unique_ptr<V8ScriptHolder> upScriptHolder;

        if (documentInfo.IsModule())
        {
            auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest);
            if (hModule.IsEmpty())
            {
                hModule = VERIFY_MAYBE(CompileModule(&source));
                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                CacheModule(documentInfo, codeDigest, hModule);
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hModule)), documentInfo, codeDigest, std::move(code)));
        }
        else
        {
            auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest);
            if (hScript.IsEmpty())
            {
                hScript = VERIFY_MAYBE(CompileUnboundScript(&source));
                if (hScript.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                CacheScript(documentInfo, codeDigest, hScript);
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)), documentInfo, codeDigest));
        }

        return upScriptHolder.release();

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::CompileScriptFromUtf8(const V8DocumentInfo& documentInfo, const char* code, int codeLength, size_t codeDigest)
{
    if (documentInfo.IsModule())
    {
        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("CompileScriptFromUtf8 cannot compile modules")), false);
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest);
        if (hScript.IsEmpty())
        {
            v8::ScriptCompiler::Source source(FROM_MAYBE(CreateStringFromUtf8(code, codeLength)), CreateScriptOrigin(documentInfo));

            hScript = VERIFY_MAYBE(CompileUnboundScript(&source));
            if (hScript.IsEmpty())
            {
                throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
            }

            CacheScript(documentInfo, codeDigest, hScript);
        }

        std::unique_ptr<V8ScriptHolder> upScriptHolder;
        upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)), documentInfo, codeDigest));
        return upScriptHolder.release();

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes)
{
    if (cacheKind == V8CacheKind::None)
    {
        cacheBytes.clear();
        return Compile(documentInfo, std::move(code));
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto codeDigest = code.GetDigest();
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo));
        std::unique_ptr<V8ScriptHolder> upScriptHolder;

        if (documentInfo.IsModule())
        {
            auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
            if (hModule.IsEmpty())
            {
                hModule = VERIFY_MAYBE(CompileModule(&source));
                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hModule->GetUnboundModuleScript()));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                }

                CacheModule(documentInfo, codeDigest, hModule, cacheBytes);
            }
            else if (cacheBytes.empty())
            {
                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hModule->GetUnboundModuleScript()));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                    SetCachedModuleCacheBytes(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
                }
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hModule)), documentInfo, codeDigest, std::move(code)));
        }
        else
        {
            auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
            if (hScript.IsEmpty())
            {
                hScript = VERIFY_MAYBE(CompileUnboundScript(&source));
                if (hScript.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hScript));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                }

                CacheScript(documentInfo, codeDigest, hScript, cacheBytes);
            }
            else if (cacheBytes.empty())
            {
                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hScript));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                    SetCachedScriptCacheBytes(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
                }
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)), documentInfo, codeDigest));
        }

        if (!cacheBytes.empty() && documentInfo.IsModule())
        {
            upScriptHolder->SetCacheBytes(cacheBytes);
        }

        return upScriptHolder.release();

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, const std::vector<uint8_t>& cacheBytes, bool& cacheAccepted)
{
    cacheAccepted = false;

    if ((cacheKind == V8CacheKind::None) || cacheBytes.empty())
    {
        return Compile(documentInfo, std::move(code));
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto codeDigest = code.GetDigest();
        auto pCachedData = new v8::ScriptCompiler::CachedData(cacheBytes.data(), static_cast<int>(cacheBytes.size()), v8::ScriptCompiler::CachedData::BufferNotOwned);
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo), pCachedData);
        std::unique_ptr<V8ScriptHolder> upScriptHolder;

        if (documentInfo.IsModule())
        {
            auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest);
            if (hModule.IsEmpty())
            {
                hModule = VERIFY_MAYBE(CompileModule(&source, v8::ScriptCompiler::kConsumeCodeCache));
                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                cacheAccepted = !pCachedData->rejected;
                CacheModule(documentInfo, codeDigest, hModule, cacheAccepted ? cacheBytes : std::vector<uint8_t>());
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hModule)), documentInfo, codeDigest, std::move(code)));
        }
        else
        {
            auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest);
            if (hScript.IsEmpty())
            {
                hScript = VERIFY_MAYBE(CompileUnboundScript(&source, v8::ScriptCompiler::kConsumeCodeCache));
                if (hScript.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                cacheAccepted = !pCachedData->rejected;
                CacheScript(documentInfo, codeDigest, hScript, cacheAccepted ? cacheBytes : std::vector<uint8_t>());
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)), documentInfo, codeDigest));
        }

        if (cacheAccepted && documentInfo.IsModule())
        {
            upScriptHolder->SetCacheBytes(cacheBytes);
        }

        return upScriptHolder.release();

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const V8DocumentInfo& documentInfo, StdString&& code, V8CacheKind cacheKind, std::vector<uint8_t>& cacheBytes, V8CacheResult& cacheResult)
{
    if (cacheKind == V8CacheKind::None)
    {
        cacheResult = V8CacheResult::Disabled;
        return Compile(documentInfo, std::move(code));
    }

    if (cacheBytes.empty())
    {
        auto pScriptHolder = Compile(documentInfo, std::move(code), cacheKind, cacheBytes);
        cacheResult = !cacheBytes.empty() ? V8CacheResult::Updated : V8CacheResult::UpdateFailed;
        return pScriptHolder;
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(documentInfo)
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto codeDigest = code.GetDigest();
        auto pCachedData = new v8::ScriptCompiler::CachedData(cacheBytes.data(), static_cast<int>(cacheBytes.size()), v8::ScriptCompiler::CachedData::BufferNotOwned);
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo), pCachedData);
        std::unique_ptr<V8ScriptHolder> upScriptHolder;
        std::vector<uint8_t> cachedCacheBytes;

        if (documentInfo.IsModule())
        {
            auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest, cachedCacheBytes);
            if (hModule.IsEmpty())
            {
                hModule = VERIFY_MAYBE(CompileModule(&source, v8::ScriptCompiler::kConsumeCodeCache));
                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                if (!pCachedData->rejected)
                {
                    cacheResult = V8CacheResult::Accepted;
                }
                else
                {
                    std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hModule->GetUnboundModuleScript()));
                    if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                    {
                        cacheBytes.resize(upCachedData->length);
                        memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                        cacheResult = V8CacheResult::Updated;
                    }
                    else
                    {
                        cacheResult = V8CacheResult::UpdateFailed;
                    }
                }

                CacheModule(documentInfo, codeDigest, hModule, (cacheResult != V8CacheResult::UpdateFailed) ? cacheBytes : std::vector<uint8_t>());
            }
            else if (cachedCacheBytes.empty())
            {
                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hModule->GetUnboundModuleScript()));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                    SetCachedModuleCacheBytes(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
                    cacheResult = V8CacheResult::Updated;
                }
                else
                {
                    cacheResult = V8CacheResult::UpdateFailed;
                }
            }
            else if (cachedCacheBytes == cacheBytes)
            {
                cacheResult = V8CacheResult::Verified;
            }
            else
            {
                cacheBytes = cachedCacheBytes;
                cacheResult = V8CacheResult::Updated;
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hModule)), documentInfo, codeDigest, std::move(code)));
        }
        else
        {
            auto hScript = GetCachedScript(documentInfo.GetUniqueId(), codeDigest, cachedCacheBytes);
            if (hScript.IsEmpty())
            {
                hScript = VERIFY_MAYBE(CompileUnboundScript(&source, v8::ScriptCompiler::kConsumeCodeCache));
                if (hScript.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Script compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                if (!pCachedData->rejected)
                {
                    cacheResult = V8CacheResult::Accepted;
                }
                else
                {
                    std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hScript));
                    if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                    {
                        cacheBytes.resize(upCachedData->length);
                        memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                        cacheResult = V8CacheResult::Updated;
                    }
                    else
                    {
                        cacheResult = V8CacheResult::UpdateFailed;
                    }
                }

                CacheScript(documentInfo, codeDigest, hScript, (cacheResult != V8CacheResult::UpdateFailed) ? cacheBytes : std::vector<uint8_t>());
            }
            else if (cachedCacheBytes.empty())
            {
                std::unique_ptr<v8::ScriptCompiler::CachedData> upCachedData(v8::ScriptCompiler::CreateCodeCache(hScript));
                if (upCachedData && (upCachedData->length > 0) && (upCachedData->data != nullptr))
                {
                    cacheBytes.resize(upCachedData->length);
                    memcpy(cacheBytes.data(), upCachedData->data, upCachedData->length);
                    SetCachedScriptCacheBytes(documentInfo.GetUniqueId(), codeDigest, cacheBytes);
                    cacheResult = V8CacheResult::Updated;
                }
                else
                {
                    cacheResult = V8CacheResult::UpdateFailed;
                }
            }
            else if (cachedCacheBytes == cacheBytes)
            {
                cacheResult = V8CacheResult::Verified;
            }
            else
            {
                cacheBytes = cachedCacheBytes;
                cacheResult = V8CacheResult::Updated;
            }

            upScriptHolder.reset(new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)), documentInfo, codeDigest));
        }

        if (cacheResult != V8CacheResult::UpdateFailed)
        {
            upScriptHolder->SetCacheBytes(cacheBytes);
        }

        return upScriptHolder.release();

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::CanExecute(const SharedPtr<V8ScriptHolder>& spHolder)
{
    return spHolder->IsSameIsolate(m_spIsolateImpl);
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(const SharedPtr<V8ScriptHolder>& spHolder, bool evaluate)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_DOCUMENT_SCOPE(spHolder->GetDocumentInfo())
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        if (!CanExecute(spHolder))
        {
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Invalid compiled script")), false);
        }

        v8::Local<v8::Value> hResult;

        if (spHolder->GetDocumentInfo().IsModule())
        {
            auto codeDigest = spHolder->GetCode().GetDigest();
            auto hModule = GetCachedModule(spHolder->GetDocumentInfo().GetUniqueId(), codeDigest);
            if (hModule.IsEmpty())
            {
                if (!spHolder->GetCacheBytes().empty())
                {
                    auto pCachedData = new v8::ScriptCompiler::CachedData(spHolder->GetCacheBytes().data(), static_cast<int>(spHolder->GetCacheBytes().size()), v8::ScriptCompiler::CachedData::BufferNotOwned);
                    v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(spHolder->GetCode())), CreateScriptOrigin(spHolder->GetDocumentInfo()), pCachedData);
                    hModule = VERIFY_MAYBE(CompileModule(&source));
                    _ASSERTE(!pCachedData->rejected);
                }
                else
                {
                    v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(spHolder->GetCode())), CreateScriptOrigin(spHolder->GetDocumentInfo()));
                    hModule = VERIFY_MAYBE(CompileModule(&source));
                }

                if (hModule.IsEmpty())
                {
                    throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Module compilation failed; no additional information was provided by the V8 runtime")), false);
                }

                CacheModule(spHolder->GetDocumentInfo(), codeDigest, hModule);
            }

            if (hModule->GetStatus() == v8::Module::kUninstantiated)
            {
                ASSERT_EVAL(VERIFY_MAYBE(hModule->InstantiateModule(m_hContext, V8IsolateImpl::ModuleResolveCallback)));
            }

            if (hModule->GetStatus() == v8::Module::kInstantiated)
            {
                hResult = VERIFY_MAYBE(hModule->Evaluate(m_hContext));
                if (hResult->IsPromise())
                {
                    auto hPromise = hResult.As<v8::Promise>();
                    if (hModule->IsGraphAsync() || (hPromise->State() == v8::Promise::PromiseState::kPending))
                    {
                        v8::Local<v8::Object> hMetaHolder;
                        if (TryGetCachedModuleMetaHolder(hModule, hMetaHolder))
                        {
                            if (m_hGetModuleResultFunction.IsEmpty())
                            {
                                auto hEngineInternal = FROM_MAYBE(m_hContext->Global()->Get(m_hContext, CreateString("EngineInternal"))).As<v8::Object>();
                                m_hGetModuleResultFunction = CreatePersistent(FROM_MAYBE(hEngineInternal->Get(m_hContext, CreateString("getModuleResult"))).As<v8::Function>());
                            }

                            v8::Local<v8::Value> args[] = { hResult, hMetaHolder };
                            hResult = FROM_MAYBE(m_hGetModuleResultFunction->Call(m_hContext, GetUndefined(), 2, args));
                        }
                    }
                    else if (hPromise->State() == v8::Promise::PromiseState::kFulfilled)
                    {
                        hResult = hPromise->Result();
                        if (evaluate && (hResult.IsEmpty() || hResult->IsUndefined()))
                        {
                            v8::Local<v8::Object> hMetaHolder;
                            if (TryGetCachedModuleMetaHolder(hModule, hMetaHolder))
                            {
                                auto hMeta = ::ValueAsObject(FROM_MAYBE(hMetaHolder->Get(m_hContext, 0)));
                                if (!hMeta.IsEmpty())
                                {
                                    hResult = FROM_MAYBE(hMeta->Get(m_hContext, m_hModuleResultKey));
                                }
                            }
                        }
                    }
                    else if (hPromise->State() == v8::Promise::PromiseState::kRejected)
                    {
                        auto hException = hPromise->Result();

                        if (hException->IsObject())
                        {
                            auto hExceptionObject = hException.As<v8::Object>();
                            auto hHostException = FROM_MAYBE(hExceptionObject->Get(m_hContext, m_hHostExceptionKey));
                            throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hExceptionObject), CreateStdString(FROM_MAYBE(hExceptionObject->Get(m_hContext, m_hStackKey))), EXECUTION_STARTED, ExportValue(hException), ExportValue(hHostException));
                        }

                        throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hException), StdString(), EXECUTION_STARTED, ExportValue(hException), V8Value(V8Value::Undefined));
                    }
                }
            }
            else
            {
                evaluate = false;
            }
        }
        else
        {
            auto hScript = ::HandleFromPtr<v8::UnboundScript>(spHolder->GetScript());
            hResult = VERIFY_MAYBE(hScript->BindToCurrentContext()->Run(m_hContext));
        }

        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_DOCUMENT_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Interrupt()
{
    TerminateExecution(false);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CancelInterrupt()
{
    CancelTerminateExecution();
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::GetEnableIsolateInterruptPropagation()
{
    return m_spIsolateImpl->GetEnableInterruptPropagation();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetEnableIsolateInterruptPropagation(bool value)
{
    m_spIsolateImpl->SetEnableInterruptPropagation(value);
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::GetDisableIsolateHeapSizeViolationInterrupt()
{
    return m_spIsolateImpl->GetDisableHeapSizeViolationInterrupt();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetDisableIsolateHeapSizeViolationInterrupt(bool value)
{
    m_spIsolateImpl->SetDisableHeapSizeViolationInterrupt(value);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetIsolateHeapStatistics(v8::HeapStatistics& heapStatistics)
{
    m_spIsolateImpl->GetHeapStatistics(heapStatistics);
}

//-----------------------------------------------------------------------------

V8Isolate::Statistics V8ContextImpl::GetIsolateStatistics()
{
    return m_spIsolateImpl->GetStatistics();
}

//-----------------------------------------------------------------------------

V8ContextImpl::Statistics V8ContextImpl::GetStatistics()
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    return m_Statistics;
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CollectGarbage(bool exhaustive)
{
    m_spIsolateImpl->CollectGarbage(exhaustive);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::OnAccessSettingsChanged()
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        Dispose(m_hAccessToken);
        m_hAccessToken = CreatePersistent(CreateObject());

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::BeginCpuProfile(const StdString& name, v8::CpuProfilingMode mode, bool recordSamples)
{
    return m_spIsolateImpl->BeginCpuProfile(name, mode, recordSamples);
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::EndCpuProfile(const StdString& name, V8Isolate::CpuProfileCallback* pCallback, void* pvArg)
{
    return m_spIsolateImpl->EndCpuProfile(name, pCallback, pvArg);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CollectCpuProfileSample()
{
    return m_spIsolateImpl->CollectCpuProfileSample();
}

//-----------------------------------------------------------------------------

uint32_t V8ContextImpl::GetCpuProfileSampleInterval()
{
    return m_spIsolateImpl->GetCpuProfileSampleInterval();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetCpuProfileSampleInterval(uint32_t value)
{
    m_spIsolateImpl->SetCpuProfileSampleInterval(value);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::WriteIsolateHeapSnapshot(void* pvStream)
{
    m_spIsolateImpl->WriteHeapSnapshot(pvStream);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Flush()
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        (void)m_hFlushFunction->Call(m_hContext, GetUndefined(), 0, nullptr);

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Destroy()
{
    m_spIsolateImpl->CallWithLockNoWait(true, [this] (V8IsolateImpl* /*pIsolateImpl*/)
    {
        delete this;
    });
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(void* pvObject, const StdString& name)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        return ExportValue(FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Get(m_hContext, FROM_MAYBE(CreateString(name)))));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetV8ObjectProperty(void* pvObject, const StdString& name, V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto hObject = ::HandleFromPtr<v8::Object>(pvObject);
        auto hName = FROM_MAYBE(CreateString(name));

        if (FROM_MAYBE(hObject->Has(m_hContext, hName)))
        {
            value = ExportValue(FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Get(m_hContext, hName)));
            return true;
        }

        return false;

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        ASSERT_EVAL(FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Set(m_hContext, FROM_MAYBE(CreateString(name)), ImportValue(value))));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, const StdString& name)
{
    BEGIN_CONTEXT_SCOPE
    FROM_MAYBE_TRY

        return FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Delete(m_hContext, FROM_MAYBE(CreateString(name))));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), false);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(void* pvObject, bool includeIndices, std::vector<StdString>& names)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyNames(::HandleFromPtr<v8::Object>(pvObject), names, v8::ONLY_ENUMERABLE, includeIndices ? v8::IndexFilter::kIncludeIndices : v8::IndexFilter::kSkipIndices);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(void* pvObject, int index)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        return ExportValue(FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Get(m_hContext, index)));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, int index, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        ASSERT_EVAL(FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Set(m_hContext, index, ImportValue(value))));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, int index)
{
    BEGIN_CONTEXT_SCOPE
    FROM_MAYBE_TRY

        return FROM_MAYBE(::HandleFromPtr<v8::Object>(pvObject)->Delete(m_hContext, index));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), false);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(void* pvObject, std::vector<int>& indices)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyIndices(::HandleFromPtr<v8::Object>(pvObject), indices, v8::ONLY_ENUMERABLE);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8Object(void* pvObject, bool asConstructor, const std::vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);
        if (!hObject->IsCallable())
        {
            FROM_MAYBE_TRY

                auto hError = v8::Exception::TypeError(m_hObjectNotInvocable).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, m_hStackKey))), EXECUTION_STARTED, ExportValue(hError), V8Value(V8Value::Undefined));

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The object does not support invocation")), EXECUTION_STARTED);

            FROM_MAYBE_END
        }

        std::vector<v8::Local<v8::Value>> importedArgs;
        ImportValues(args, importedArgs);

        if (asConstructor)
        {
            return ExportValue(VERIFY_MAYBE(hObject->CallAsConstructor(m_hContext, static_cast<int>(importedArgs.size()), importedArgs.data())));
        }

        return ExportValue(VERIFY_MAYBE(hObject->CallAsFunction(m_hContext, hObject, static_cast<int>(importedArgs.size()), importedArgs.data())));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8ObjectMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);

        auto hMethod = ::ValueAsObject(VERIFY_MAYBE(hObject->Get(m_hContext, FROM_MAYBE(CreateString(name)))));
        if (hMethod.IsEmpty())
        {
            FROM_MAYBE_TRY

                auto hError = v8::Exception::TypeError(m_hMethodOrPropertyNotFound).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, m_hStackKey))), EXECUTION_STARTED, ExportValue(hError), V8Value(V8Value::Undefined));

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("Method or property not found")), EXECUTION_STARTED);

            FROM_MAYBE_END
        }

        if (!hMethod->IsCallable())
        {
            FROM_MAYBE_TRY

                auto hError = v8::Exception::TypeError(m_hPropertyValueNotInvocable).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, m_hStackKey))), EXECUTION_STARTED, ExportValue(hError), V8Value(V8Value::Undefined));

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The property value does not support invocation")), EXECUTION_STARTED);

            FROM_MAYBE_END
        }

        std::vector<v8::Local<v8::Value>> importedArgs;
        ImportValues(args, importedArgs);

        return ExportValue(VERIFY_MAYBE(hMethod->CallAsFunction(m_hContext, hObject, static_cast<int>(importedArgs.size()), importedArgs.data())));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The V8 runtime cannot perform the requested operation because a script exception is pending")), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectArrayBufferOrViewInfo(void* pvObject, V8Value& arrayBuffer, size_t& offset, size_t& size, size_t& length)
{
    BEGIN_CONTEXT_SCOPE

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);

        if (hObject->IsArrayBuffer())
        {
            auto hArrayBuffer = hObject.As<v8::ArrayBuffer>();
            arrayBuffer = ExportValue(hObject);
            offset = 0;
            size = hArrayBuffer->ByteLength();
            length = size;
            return;
        }

        if (hObject->IsSharedArrayBuffer())
        {
            auto hSharedArrayBuffer = hObject.As<v8::SharedArrayBuffer>();
            arrayBuffer = ExportValue(hObject);
            offset = 0;
            size = hSharedArrayBuffer->ByteLength();
            length = size;
            return;
        }

        if (hObject->IsDataView())
        {
            auto hDataView = hObject.As<v8::DataView>();
            arrayBuffer = ExportValue(hDataView->Buffer());
            offset = hDataView->ByteOffset();
            size = hDataView->ByteLength();
            length = size;
            return;
        }

        if (hObject->IsTypedArray())
        {
            auto hTypedArray = hObject.As<v8::TypedArray>();
            arrayBuffer = ExportValue(hTypedArray->Buffer());
            offset = hTypedArray->ByteOffset();
            size = hTypedArray->ByteLength();
            length = hTypedArray->Length();
            return;
        }

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The object is not a V8 array buffer or view")), false);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeWithV8ObjectArrayBufferOrViewData(void* pvObject, V8ObjectHelpers::ArrayBufferOrViewDataCallback* pCallback, void* pvArg)
{
    BEGIN_CONTEXT_SCOPE

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);

        if (hObject->IsArrayBuffer())
        {
            auto hArrayBuffer = hObject.As<v8::ArrayBuffer>();
            auto spBackingStore = hArrayBuffer->GetBackingStore();
            (*pCallback)(spBackingStore->Data(), pvArg);
            return;
        }

        if (hObject->IsSharedArrayBuffer())
        {
            auto hSharedArrayBuffer = hObject.As<v8::SharedArrayBuffer>();
            auto spBackingStore = hSharedArrayBuffer->GetBackingStore();
            (*pCallback)(spBackingStore->Data(), pvArg);
            return;
        }

        if (hObject->IsDataView())
        {
            auto hDataView = hObject.As<v8::DataView>();
            auto spBackingStore = hDataView->Buffer()->GetBackingStore();
            (*pCallback)(static_cast<uint8_t*>(spBackingStore->Data()) + hDataView->ByteOffset(), pvArg);
            return;
        }

        if (hObject->IsTypedArray())
        {
            auto hTypedArray = hObject.As<v8::TypedArray>();
            auto spBackingStore = hTypedArray->Buffer()->GetBackingStore();
            (*pCallback)(static_cast<uint8_t*>(spBackingStore->Data()) + hTypedArray->ByteOffset(), pvArg);
            return;
        }

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(SL("The object is not a V8 array buffer or view")), false);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InitializeImportMeta(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule, v8::Local<v8::Object> hMeta)
{
    BEGIN_CONTEXT_SCOPE

    FROM_MAYBE_TRY

        try
        {
            for (auto& entry : m_ModuleCache)
            {
                if (entry.hModule == hModule)
                {
                    ASSERT_EVAL(FROM_MAYBE(entry.hMetaHolder->Set(hContext, 0, hMeta)));

                    auto hSetModuleResultFunction = FROM_MAYBE(v8::Function::New(m_hContext, SetModuleResultCallback, hMeta));
                    ASSERT_EVAL(FROM_MAYBE(hMeta->DefineOwnProperty(m_hContext, m_hSetModuleResultKey, hSetModuleResultFunction, v8::DontEnum)));

                    for (const auto& pair : HostObjectUtil::CreateModuleContext(entry.DocumentInfo))
                    {
                        ASSERT_EVAL(FROM_MAYBE(hMeta->Set(m_hContext, FROM_MAYBE(CreateString(pair.first)), ImportValue(pair.second))));
                    }

                    return;
                }
            }
        }
        catch (const HostException& exception)
        {
            ThrowScriptException(exception);
        }

    FROM_MAYBE_CATCH_CONSUME

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Promise> V8ContextImpl::ImportModule(v8::Local<v8::Data> hHostDefinedOptions, v8::Local<v8::Value> /*hResourceName*/, v8::Local<v8::String> hSpecifier, v8::Local<v8::FixedArray> /*hImportAssertions*/)
{
    V8DocumentInfo sourceDocumentInfo;
    const V8DocumentInfo* pSourceDocumentInfo = nullptr;

    if (!hHostDefinedOptions.IsEmpty())
    {
        auto hOptions = hHostDefinedOptions.As<v8::PrimitiveArray>();
        if (hOptions->Length() > 0)
        {
            auto hUniqueId = ::ValueAsBigInt(GetPrimitiveArrayItem(hOptions, 0));
            if (!hUniqueId.IsEmpty())
            {
                auto uniqueId = hUniqueId->Uint64Value();
                if (TryGetCachedScriptInfo(uniqueId, sourceDocumentInfo) || TryGetCachedModuleInfo(uniqueId, sourceDocumentInfo))
                {
                    pSourceDocumentInfo = &sourceDocumentInfo;
                }
            }
        }
    }

    return ImportModule(pSourceDocumentInfo, hSpecifier);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Promise> V8ContextImpl::ImportModule(const V8DocumentInfo* pSourceDocumentInfo, v8::Local<v8::String> hSpecifier)
{
    BEGIN_CONTEXT_SCOPE

        V8IsolateImpl::TryCatch outerTryCatch(*m_spIsolateImpl);

        FROM_MAYBE_TRY

            auto hResolver = FROM_MAYBE(v8::Promise::Resolver::New(m_hContext));

            V8IsolateImpl::TryCatch innerTryCatch(*m_spIsolateImpl);

            FROM_MAYBE_TRY

                auto hModule = FROM_MAYBE(ResolveModule(hSpecifier, pSourceDocumentInfo));

                if (hModule->GetStatus() == v8::Module::kUninstantiated)
                {
                    ASSERT_EVAL(FROM_MAYBE(hModule->InstantiateModule(m_hContext, V8IsolateImpl::ModuleResolveCallback)));
                }

                if (hModule->GetStatus() == v8::Module::kInstantiated)
                {
                    FROM_MAYBE(hModule->Evaluate(m_hContext));
                }

                ASSERT_EVAL(FROM_MAYBE(hResolver->Resolve(m_hContext, hModule->GetModuleNamespace())));
                return hResolver->GetPromise();

            FROM_MAYBE_CATCH_CONSUME

            auto innerHasCaught = innerTryCatch.HasCaught();
            _ASSERTE(innerHasCaught);

            if (innerHasCaught)
            {
                ASSERT_EVAL(FROM_MAYBE(hResolver->Reject(m_hContext, innerTryCatch.Exception())));
            }

            return hResolver->GetPromise();

        FROM_MAYBE_CATCH_CONSUME

        auto outerHasCaught = outerTryCatch.HasCaught();
        _ASSERTE(outerHasCaught);

        if (outerHasCaught)
        {
            outerTryCatch.ReThrow();
        }

        return v8::MaybeLocal<v8::Promise>();

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Module> V8ContextImpl::ResolveModule(v8::Local<v8::String> hSpecifier, v8::Local<v8::Module> hReferrer)
{
    V8DocumentInfo sourceDocumentInfo;
    const V8DocumentInfo* pSourceDocumentInfo = nullptr;

    if (TryGetCachedModuleInfo(hReferrer, sourceDocumentInfo))
    {
        pSourceDocumentInfo = &sourceDocumentInfo;
    }

    return ResolveModule(hSpecifier, pSourceDocumentInfo);
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Module> V8ContextImpl::ResolveModule(v8::Local<v8::String> hSpecifier, const V8DocumentInfo* pSourceDocumentInfo)
{
    BEGIN_CONTEXT_SCOPE

        V8IsolateImpl::TryCatch tryCatch(*m_spIsolateImpl);

        FROM_MAYBE_TRY

            if (pSourceDocumentInfo == nullptr)
            {
                pSourceDocumentInfo = m_spIsolateImpl->GetDocumentInfo();
            }

            if (pSourceDocumentInfo == nullptr)
            {
                ThrowException(v8::Exception::Error(m_hInvalidModuleRequest));
            }
            else
            {
                try
                {
                    V8DocumentInfo documentInfo;
                    V8Value exportsValue(V8Value::Undefined);
                    auto code = HostObjectUtil::LoadModule(*pSourceDocumentInfo, CreateStdString(hSpecifier), documentInfo, exportsValue);

                    auto codeDigest = code.GetDigest();
                    auto hModule = GetCachedModule(documentInfo.GetUniqueId(), codeDigest);
                    if (!hModule.IsEmpty())
                    {
                        return hModule;
                    }

                    if (!documentInfo.IsModule())
                    {
                        std::vector<v8::Local<v8::String>> names;
                        std::vector<SyntheticModuleExport> exports;

                        auto hExportsValue = ImportValue(exportsValue);
                        if (hExportsValue.IsEmpty())
                        {
                            hExportsValue = GetUndefined();
                        }

                        if ((documentInfo.GetKind() == DocumentKind::Json) || !hExportsValue->IsObject())
                        {
                            auto hName = CreateString("default");
                            names.push_back(hName);
                            exports.push_back({ CreatePersistent(hName), CreatePersistent(hExportsValue) });
                        }
                        else
                        {
                            auto hExports = hExportsValue.As<v8::Object>();
                            auto hOwnPropertyNames = FROM_MAYBE(hExports->GetOwnPropertyNames(m_hContext, v8::SKIP_SYMBOLS, v8::KeyConversionMode::kConvertToString));
                            if (!hOwnPropertyNames.IsEmpty())
                            {
                                auto length = hOwnPropertyNames->Length();

                                names.reserve(length);
                                exports.reserve(length);

                                for (uint32_t index = 0; index < length; index++)
                                {
                                    auto hName = ::ValueAsString(FROM_MAYBE(hOwnPropertyNames->Get(m_hContext, index)));
                                    if (!hName.IsEmpty())
                                    {
                                        names.push_back(hName);
                                        exports.push_back({ CreatePersistent(hName), CreatePersistent(FROM_MAYBE(hExports->Get(m_hContext, hName))) });
                                    }
                                }
                            }
                        }

                        hModule = CreateSyntheticModule(FROM_MAYBE(CreateString(documentInfo.GetResourceName())), names, PopulateSyntheticModule);
                        m_SyntheticModuleData.push_back({ CreatePersistent(hModule), std::move(exports) });
                    }
                    else
                    {
                        BEGIN_DOCUMENT_SCOPE(documentInfo)

                            v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), CreateScriptOrigin(documentInfo));
                            hModule = FROM_MAYBE(CompileModule(&source));

                        END_DOCUMENT_SCOPE
                    }

                    if (!hModule.IsEmpty())
                    {
                        CacheModule(documentInfo, codeDigest, hModule);
                    }

                    return hModule;
                }
                catch (const HostException& exception)
                {
                    ThrowScriptException(exception);
                }
            }

        FROM_MAYBE_CATCH_CONSUME

        auto hasCaught = tryCatch.HasCaught();
        _ASSERTE(hasCaught);

        if (hasCaught)
        {
            tryCatch.ReThrow();
        }

        return v8::MaybeLocal<v8::Module>();

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Value> V8ContextImpl::PopulateSyntheticModule(v8::Local<v8::Context> hContext, v8::Local<v8::Module> hModule)
{
    if (hContext->GetNumberOfEmbedderDataFields() > 1)
    {
        auto pContextImpl = static_cast<V8ContextImpl*>(hContext->GetAlignedPointerFromEmbedderData(1));
        if (pContextImpl != nullptr)
        {
            return pContextImpl->PopulateSyntheticModule(hModule);
        }
    }

    return v8::MaybeLocal<v8::Value>();
}

//-----------------------------------------------------------------------------

v8::MaybeLocal<v8::Value> V8ContextImpl::PopulateSyntheticModule(v8::Local<v8::Module> hModule)
{
    FROM_MAYBE_TRY

        auto hResolver = FROM_MAYBE(v8::Promise::Resolver::New(m_hContext));
        ASSERT_EVAL(FROM_MAYBE(hResolver->Resolve(m_hContext, GetUndefined())));

        for (auto itModule = m_SyntheticModuleData.begin(); itModule != m_SyntheticModuleData.end(); itModule++)
        {
            if (itModule->hModule == hModule)
            {
                Dispose(itModule->hModule);
                for (auto itExport = itModule->Exports.begin(); itExport != itModule->Exports.end(); itExport++)
                {
                    FROM_MAYBE(SetSyntheticModuleExport(hModule, itExport->hName, itExport->hValue));
                    Dispose(itExport->hName);
                    Dispose(itExport->hValue);
                }

                m_SyntheticModuleData.erase(itModule);
                break;
            }
        }

        return hResolver->GetPromise();

    FROM_MAYBE_CATCH

        return v8::MaybeLocal<v8::Value>();

    FROM_MAYBE_END
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Teardown()
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    m_spIsolateImpl->RemoveContext(this);

    if (m_pvV8ObjectCache != nullptr)
    {
        std::vector<void*> v8ObjectPtrs;
        HostObjectUtil::GetAllCachedV8Objects(m_pvV8ObjectCache, v8ObjectPtrs);
        for (auto pvV8Object : v8ObjectPtrs)
        {
            auto hObject = ::HandleFromPtr<v8::Object>(pvV8Object);

            auto pHolder = GetHostObjectHolder(hObject);
            if (pHolder != nullptr)
            {
                delete pHolder;
            }

            ClearWeak(hObject);
            Dispose(hObject);
        }

        HostObjectUtil::Release(m_pvV8ObjectCache);
        m_pvV8ObjectCache = nullptr;
    }

    for (auto itModule = m_SyntheticModuleData.rbegin(); itModule != m_SyntheticModuleData.rend(); itModule++)
    {
        Dispose(itModule->hModule);
        for (auto itExport = itModule->Exports.begin(); itExport != itModule->Exports.end(); itExport++)
        {
            Dispose(itExport->hName);
            Dispose(itExport->hValue);
        }
    }

    ClearModuleCache();

    for (auto it = m_GlobalMembersStack.rbegin(); it != m_GlobalMembersStack.rend(); it++)
    {
        Dispose(it->second);
    }

    Dispose(m_hAsyncGeneratorConstructor);
    Dispose(m_hGetModuleResultFunction);
    Dispose(m_hToJsonFunction);
    Dispose(m_hToAsyncIteratorFunction);
    Dispose(m_hToIteratorFunction);
    Dispose(m_hFastHostFunctionTemplate);
    Dispose(m_hFastHostObjectTemplate);
    Dispose(m_hHostDelegateTemplate);
    Dispose(m_hHostInvocableTemplate);
    Dispose(m_hHostObjectTemplate);
    Dispose(m_hTerminationException);
    Dispose(m_hFlushFunction);
    Dispose(m_hSetModuleResultKey);
    Dispose(m_hConstructorKey);
    Dispose(m_hInvalidModuleRequest);
    Dispose(m_hPropertyValueNotInvocable);
    Dispose(m_hMethodOrPropertyNotFound);
    Dispose(m_hObjectNotInvocable);
    Dispose(m_hStackKey);
    Dispose(m_hInternalUseOnly);
    Dispose(m_hAccessToken);
    Dispose(m_hAccessTokenKey);
    Dispose(m_hCacheKey);
    Dispose(m_hHostExceptionKey);
    Dispose(m_hMissingPropertyValue);
    Dispose(m_hModuleResultKey);
    Dispose(m_hIsHostObjectKey);

    // As of V8 3.16.0, the global property getter for a disposed context
    // may be invoked during GC after the V8ContextImpl instance is gone.

    if (!m_hContext.IsEmpty())
    {
        auto hGlobal = m_hContext->Global();
        if (!hGlobal.IsEmpty() && (hGlobal->InternalFieldCount() > 0))
        {
            hGlobal->SetAlignedPointerInInternalField(0, nullptr);
        }

        if (m_hContext->GetNumberOfEmbedderDataFields() > 1)
        {
            m_hContext->SetAlignedPointerInEmbedderData(1, nullptr);
        }

        Dispose(m_hContext);
    }

    if (m_ChangedTimerResolution)
    {
        HighResolutionClock::RestoreTimerResolution();
        m_ChangedTimerResolution = false;
    }
}

//-----------------------------------------------------------------------------

V8ContextImpl::~V8ContextImpl()
{
    --s_InstanceCount;
    Teardown();
    ContextDisposedNotification();
}

//-----------------------------------------------------------------------------

SharedPtr<V8WeakContextBinding> V8ContextImpl::GetWeakBinding()
{
    if (m_spWeakBinding.IsEmpty())
    {
        m_spWeakBinding = new V8WeakContextBinding(m_spIsolateImpl, this);
    }

    return m_spWeakBinding;
}

//-----------------------------------------------------------------------------

HostObjectHolder* V8ContextImpl::GetHostObjectHolder(v8::Local<v8::Object> hObject)
{
    if (!hObject.IsEmpty())
    {
        auto hHolder = ::ValueAsExternal(FROM_MAYBE_DEFAULT(hObject->GetPrivate(m_hContext, GetHostObjectHolderKey())));
        if (!hHolder.IsEmpty())
        {
            return static_cast<HostObjectHolder*>(hHolder->Value());
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::SetHostObjectHolder(v8::Local<v8::Object> hObject, HostObjectHolder* pHolder)
{
    if (!hObject.IsEmpty())
    {
        return FROM_MAYBE_DEFAULT(hObject->SetPrivate(m_hContext, GetHostObjectHolderKey(), CreateExternal(pHolder)));
    }

    return false;
}

//-----------------------------------------------------------------------------

void* V8ContextImpl::GetHostObject(v8::Local<v8::Object> hObject)
{
    auto pHolder = GetHostObjectHolder(hObject);
    if (pHolder != nullptr)
    {
        return pHolder->GetObject();
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::CheckContextImplForGlobalObjectCallback(V8ContextImpl* pContextImpl)
{
    if (pContextImpl == nullptr)
    {
        return false;
    }

    if (pContextImpl->IsExecutionTerminating())
    {
        pContextImpl->ThrowException(pContextImpl->m_hTerminationException);
        return false;
    }

    return true;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::CheckContextImplForHostObjectCallback(V8ContextImpl* pContextImpl)
{
    if (pContextImpl == nullptr)
    {
        return false;
    }

    if (pContextImpl->IsExecutionTerminating())
    {
        pContextImpl->ThrowException(pContextImpl->m_hTerminationException);
        return false;
    }

    return true;
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(v8::Local<v8::Object> hObject, std::vector<StdString>& names, v8::PropertyFilter filter, v8::IndexFilter indexFilter)
{
    names.clear();

    FROM_MAYBE_TRY

        auto hNames = FROM_MAYBE(hObject->GetPropertyNames(m_hContext, v8::KeyCollectionMode::kIncludePrototypes, static_cast<v8::PropertyFilter>(filter | v8::SKIP_SYMBOLS), indexFilter, v8::KeyConversionMode::kConvertToString));
        auto nameCount = static_cast<int>(hNames->Length());

        names.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = ::ValueAsString(FROM_MAYBE(hNames->Get(m_hContext, index)));
            if (!hName.IsEmpty())
            {
                names.push_back(CreateStdString(hName));
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(v8::Local<v8::Object> hObject, std::vector<int>& indices, v8::PropertyFilter filter)
{
    indices.clear();

    FROM_MAYBE_TRY

        auto hNames = FROM_MAYBE(hObject->GetPropertyNames(m_hContext, v8::KeyCollectionMode::kIncludePrototypes, static_cast<v8::PropertyFilter>(filter | v8::SKIP_SYMBOLS), v8::IndexFilter::kIncludeIndices, v8::KeyConversionMode::kKeepNumbers));
        auto nameCount = static_cast<int>(hNames->Length());

        indices.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = FROM_MAYBE(hNames->Get(m_hContext, index));
            if (hName->IsInt32())
            {
                indices.push_back(FROM_MAYBE(hName->Int32Value(m_hContext)));
            }
            else if (hName->IsUint32())
            {
                auto value = FROM_MAYBE(hName->Uint32Value(m_hContext));
                if (value <= static_cast<uint32_t>(INT_MAX))
                {
                    indices.push_back(static_cast<int>(value));
                }
            }
            else
            {
                double value;
                if (::TryGetValueAsNumber(m_hContext, hName, value))
                {
                    if (value == std::round(value) && (value >= static_cast<double>(INT_MIN)) && (value <= static_cast<double>(INT_MAX)))
                    {
                        indices.push_back(static_cast<int>(value));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        CALLBACK_RETURN(FROM_MAYBE(it->second->Get(pContextImpl->m_hContext, hName)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetGlobalProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        ASSERT_EVAL(FROM_MAYBE(it->second->Set(pContextImpl->m_hContext, hName, hValue)));
                        CALLBACK_RETURN_VOID();
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        CALLBACK_RETURN(FROM_MAYBE(it->second->GetPropertyAttributes(pContextImpl->m_hContext, hName)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        // WORKAROUND: v8::Object::Delete() crashes if a custom property deleter calls
                        // ThrowException(). Interestingly, there is no crash if the same deleter is
                        // invoked directly from script via the delete operator.

                        auto pvObject = pContextImpl->GetHostObject(it->second);
                        if (pvObject != nullptr)
                        {
                            try
                            {
                                CALLBACK_RETURN(HostObjectUtil::DeleteProperty(pvObject, pContextImpl->CreateStdString(hName)));
                            }
                            catch (const HostException&)
                            {
                                CALLBACK_RETURN(false);
                            }
                        }

                        CALLBACK_RETURN(FROM_MAYBE(it->second->Delete(pContextImpl->m_hContext, hName)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            try
            {
                const auto& stack = pContextImpl->m_GlobalMembersStack;
                if (!stack.empty())
                {
                    std::vector<StdString> names;
                    for (auto it = stack.rbegin(); it != stack.rend(); it++)
                    {
                        std::vector<StdString> tempNames;

                        auto pvObject = pContextImpl->GetHostObject(it->second);
                        if (pvObject != nullptr)
                        {
                            HostObjectUtil::GetPropertyNames(pvObject, tempNames);
                        }
                        else
                        {
                            pContextImpl->GetV8ObjectPropertyNames(it->second, tempNames, v8::ONLY_ENUMERABLE, v8::IndexFilter::kSkipIndices);
                        }

                        names.insert(names.end(), tempNames.begin(), tempNames.end());
                    }

                    std::sort(names.begin(), names.end());
                    auto newEnd = std::unique(names.begin(), names.end());
                    auto nameCount = static_cast<int>(newEnd - names.begin());

                    auto hImportedNames = pContextImpl->CreateArray(nameCount);
                    for (auto index = 0; index < nameCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedNames->Set(pContextImpl->m_hContext, index, FROM_MAYBE(pContextImpl->CreateString(names[index])))));
                    }

                    CALLBACK_RETURN(hImportedNames);
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                auto hName = FROM_MAYBE(pContextImpl->CreateInteger(index)->ToString(pContextImpl->m_hContext));
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        CALLBACK_RETURN(FROM_MAYBE(it->second->Get(pContextImpl->m_hContext, index)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetGlobalProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                auto hName = FROM_MAYBE(pContextImpl->CreateInteger(index)->ToString(pContextImpl->m_hContext));
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        ASSERT_EVAL(FROM_MAYBE(it->second->Set(pContextImpl->m_hContext, index, hValue)));
                        CALLBACK_RETURN_VOID();
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                auto hIndex = pContextImpl->CreateInteger(index);
                auto hName = FROM_MAYBE(hIndex->ToString(pContextImpl->m_hContext));
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        CALLBACK_RETURN(FROM_MAYBE(it->second->GetPropertyAttributes(pContextImpl->m_hContext, hIndex)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteGlobalProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (!stack.empty())
            {
                auto hName = FROM_MAYBE(pContextImpl->CreateInteger(index)->ToString(pContextImpl->m_hContext));
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        CALLBACK_RETURN(FROM_MAYBE(it->second->Delete(pContextImpl->m_hContext, index)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            try
            {
                const auto& stack = pContextImpl->m_GlobalMembersStack;
                if (!stack.empty())
                {
                    std::vector<int> indices;
                    for (auto it = stack.rbegin(); it != stack.rend(); it++)
                    {
                        std::vector<int> tempIndices;

                        auto pvObject = pContextImpl->GetHostObject(it->second);
                        if (pvObject != nullptr)
                        {
                            HostObjectUtil::GetPropertyIndices(pvObject, tempIndices);
                        }
                        else
                        {
                            pContextImpl->GetV8ObjectPropertyIndices(it->second, tempIndices, v8::ONLY_ENUMERABLE);
                        }

                        indices.insert(indices.end(), tempIndices.begin(), tempIndices.end());
                    }

                    std::sort(indices.begin(), indices.end());
                    auto newEnd = std::unique(indices.begin(), indices.end());
                    auto indexCount = static_cast<int>(newEnd - indices.begin());

                    auto hImportedIndices = pContextImpl->CreateArray(indexCount);
                    for (auto index = 0; index < indexCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedIndices->Set(pContextImpl->m_hContext, index, pContextImpl->CreateInteger(indices[index]))));
                    }

                    CALLBACK_RETURN(hImportedIndices);
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::HostObjectConstructorCallHandler(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if ((pContextImpl != nullptr) && !pContextImpl->m_AllowHostObjectConstructorCall)
    {
        pContextImpl->ThrowException(v8::Exception::Error(pContextImpl->m_hInternalUseOnly));
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto pvObject = pContextImpl->GetHostObject(info.This());
            if (pvObject != nullptr)
            {
                try
                {
                    if (pContextImpl->m_hToIteratorFunction.IsEmpty())
                    {
                        auto hEngineInternal = FROM_MAYBE(pContextImpl->m_hContext->Global()->Get(pContextImpl->m_hContext, pContextImpl->CreateString("EngineInternal"))).As<v8::Object>();
                        pContextImpl->m_hToIteratorFunction = pContextImpl->CreatePersistent(FROM_MAYBE(hEngineInternal->Get(pContextImpl->m_hContext, pContextImpl->CreateString("toIterator"))).As<v8::Function>());
                    }

                    v8::Local<v8::Value> args[] = { pContextImpl->ImportValue(HostObjectUtil::GetEnumerator(pvObject)) };
                    CALLBACK_RETURN(FROM_MAYBE(pContextImpl->m_hToIteratorFunction->Call(pContextImpl->m_hContext, pContextImpl->GetUndefined(), 1, args)));
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectAsyncIterator(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto pvObject = pContextImpl->GetHostObject(info.This());
            if (pvObject != nullptr)
            {
                try
                {
                    if (pContextImpl->m_hToAsyncIteratorFunction.IsEmpty())
                    {
                        auto hEngineInternal = FROM_MAYBE(pContextImpl->m_hContext->Global()->Get(pContextImpl->m_hContext, pContextImpl->CreateString("EngineInternal"))).As<v8::Object>();
                        pContextImpl->m_hToAsyncIteratorFunction = pContextImpl->CreatePersistent(FROM_MAYBE(hEngineInternal->Get(pContextImpl->m_hContext, pContextImpl->CreateString("toAsyncIterator"))).As<v8::Function>());
                    }

                    v8::Local<v8::Value> args[] = { pContextImpl->ImportValue(HostObjectUtil::GetAsyncEnumerator(pvObject)) };
                    CALLBACK_RETURN(FROM_MAYBE(pContextImpl->m_hToAsyncIteratorFunction->Call(pContextImpl->m_hContext, pContextImpl->GetUndefined(), 1, args)));
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetFastHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto pvObject = pContextImpl->GetHostObject(info.This());
            if (pvObject != nullptr)
            {
                try
                {
                    if (pContextImpl->m_hToIteratorFunction.IsEmpty())
                    {
                        auto hEngineInternal = FROM_MAYBE(pContextImpl->m_hContext->Global()->Get(pContextImpl->m_hContext, pContextImpl->CreateString("EngineInternal"))).As<v8::Object>();
                        pContextImpl->m_hToIteratorFunction = pContextImpl->CreatePersistent(FROM_MAYBE(hEngineInternal->Get(pContextImpl->m_hContext, pContextImpl->CreateString("toIterator"))).As<v8::Function>());
                    }

                    v8::Local<v8::Value> args[] = { pContextImpl->ImportValue(FastHostObjectUtil::GetEnumerator(pvObject)) };
                    CALLBACK_RETURN(FROM_MAYBE(pContextImpl->m_hToIteratorFunction->Call(pContextImpl->m_hContext, pContextImpl->GetUndefined(), 1, args)));
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetFastHostObjectAsyncIterator(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto pvObject = pContextImpl->GetHostObject(info.This());
            if (pvObject != nullptr)
            {
                try
                {
                    if (pContextImpl->m_hToAsyncIteratorFunction.IsEmpty())
                    {
                        auto hEngineInternal = FROM_MAYBE(pContextImpl->m_hContext->Global()->Get(pContextImpl->m_hContext, pContextImpl->CreateString("EngineInternal"))).As<v8::Object>();
                        pContextImpl->m_hToAsyncIteratorFunction = pContextImpl->CreatePersistent(FROM_MAYBE(hEngineInternal->Get(pContextImpl->m_hContext, pContextImpl->CreateString("toAsyncIterator"))).As<v8::Function>());
                    }

                    v8::Local<v8::Value> args[] = { pContextImpl->ImportValue(FastHostObjectUtil::GetAsyncEnumerator(pvObject)) };
                    CALLBACK_RETURN(FROM_MAYBE(pContextImpl->m_hToAsyncIteratorFunction->Call(pContextImpl->m_hContext, pContextImpl->GetUndefined(), 1, args)));
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectJson(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto hObject = info.This();
            if (pContextImpl->GetHostObject(hObject) != nullptr)
            {
                try
                {
                    if (pContextImpl->m_hToJsonFunction.IsEmpty())
                    {
                        auto hEngineInternal = FROM_MAYBE(pContextImpl->m_hContext->Global()->Get(pContextImpl->m_hContext, pContextImpl->CreateString("EngineInternal"))).As<v8::Object>();
                        pContextImpl->m_hToJsonFunction = pContextImpl->CreatePersistent(FROM_MAYBE(hEngineInternal->Get(pContextImpl->m_hContext, pContextImpl->CreateString("toJson"))).As<v8::Function>());
                    }

                    v8::Local<v8::Value> args[] = { info[0], hObject };
                    CALLBACK_RETURN(FROM_MAYBE(pContextImpl->m_hToJsonFunction->Call(pContextImpl->m_hContext, pContextImpl->GetUndefined(), 2, args)));
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }

            CALLBACK_RETURN(hObject);
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CreateFunctionForHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            CALLBACK_RETURN(FROM_MAYBE(v8::Function::New(pContextImpl->m_hContext, InvokeHostDelegate, info.This())));
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto hTarget = ::ValueAsObject(info.Data());
    if (!hTarget.IsEmpty())
    {
        auto argCount = static_cast<size_t>(info.Length());

        if (argCount < 1)
        {
            if (info.IsConstructCall())
            {
                CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsConstructor(info.GetIsolate()->GetCurrentContext(), 0, nullptr)));
            }

            CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsFunction(info.GetIsolate()->GetCurrentContext(), hTarget, 0, nullptr)));
        }

        if (argCount <= Constants::MaxInlineArgCount)
        {
            v8::Local<v8::Value> args[argCount];

            for (size_t index = 0; index < argCount; index++)
            {
                args[index] = info[index];
            }

            if (info.IsConstructCall())
            {
                CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsConstructor(info.GetIsolate()->GetCurrentContext(), argCount, args)));
            }

            CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsFunction(info.GetIsolate()->GetCurrentContext(), hTarget, argCount, args)));
        }

        std::vector<v8::Local<v8::Value>> args;
        args.reserve(argCount);

        for (size_t index = 0; index < argCount; index++)
        {
            args.push_back(info[index]);
        }

        if (info.IsConstructCall())
        {
            CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsConstructor(info.GetIsolate()->GetCurrentContext(), argCount, args.data())));
        }

        CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsFunction(info.GetIsolate()->GetCurrentContext(), hTarget, argCount, args.data())));
    }
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto hHolder = info.HolderV2();
            auto pvObject = pContextImpl->GetHostObject(hHolder);

            auto hName = ::ValueAsString(hKey);
            if (hName.IsEmpty())
            {
                if (!hKey.IsEmpty() && hKey->StrictEquals(pContextImpl->m_hIsHostObjectKey))
                {
                    CALLBACK_RETURN((pvObject != nullptr) ? pContextImpl->GetTrue() : pContextImpl->GetFalse());
                }
            }
            else if (pvObject != nullptr)
            {
                try
                {
                    auto hAccessToken = FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hAccessTokenKey));
                    if (pContextImpl->m_hAccessToken != hAccessToken)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hHolder->DeletePrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                        ASSERT_EVAL(FROM_MAYBE(hHolder->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hAccessTokenKey, pContextImpl->m_hAccessToken)));
                    }
                    else
                    {
                        auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                        if (!hCache.IsEmpty() && FROM_MAYBE(hCache->HasOwnProperty(pContextImpl->m_hContext, hName)))
                        {
                            auto hValue = FROM_MAYBE(hCache->Get(pContextImpl->m_hContext, hName));
                            CALLBACK_RETURN(!hValue->StrictEquals(pContextImpl->m_hMissingPropertyValue) ? hValue : v8::Local<v8::Value>());
                        }
                    }

                    bool isCacheable;
                    auto hResult = pContextImpl->ImportValue(HostObjectUtil::GetProperty(pvObject, pContextImpl->CreateStdString(hName), isCacheable));
                    if (isCacheable)
                    {
                        auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                        if (hCache.IsEmpty())
                        {
                            hCache = pContextImpl->CreateObject();
                            ASSERT_EVAL(FROM_MAYBE(hHolder->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey, hCache)));
                        }

                        ASSERT_EVAL(FROM_MAYBE(hCache->Set(pContextImpl->m_hContext, hName, !hResult.IsEmpty() ? hResult : pContextImpl->m_hMissingPropertyValue)));
                    }

                    CALLBACK_RETURN(hResult);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                HostObjectUtil::SetProperty(pvObject, pContextImpl->CreateStdString(hName), pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN_VOID();
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                std::vector<StdString> names;
                HostObjectUtil::GetPropertyNames(pvObject, names);

                auto name = pContextImpl->CreateStdString(hName);
                for (auto it = names.begin(); it != names.end(); it++)
                {
                    if (it->Compare(name) == 0)
                    {
                        CALLBACK_RETURN(v8::None);
                    }
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(HostObjectUtil::DeleteProperty(pvObject, pContextImpl->CreateStdString(hName)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<StdString> names;
                    HostObjectUtil::GetPropertyNames(pvObject, names);
                    auto nameCount = static_cast<int>(names.size());

                    auto hImportedNames = pContextImpl->CreateArray(nameCount);
                    for (auto index = 0; index < nameCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedNames->Set(pContextImpl->m_hContext, index, FROM_MAYBE(pContextImpl->CreateString(names[index])))));
                    }

                    CALLBACK_RETURN(hImportedNames);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectUtil::GetProperty(pvObject, index)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetHostObjectProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                HostObjectUtil::SetProperty(pvObject, index, pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN_VOID();
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                std::vector<int> indices;
                HostObjectUtil::GetPropertyIndices(pvObject, indices);

                for (auto it = indices.begin(); it < indices.end(); it++)
                {
                    if (*it == static_cast<int>(index))
                    {
                        CALLBACK_RETURN(v8::None);
                    }
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(HostObjectUtil::DeleteProperty(pvObject, index));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<int> indices;
                    HostObjectUtil::GetPropertyIndices(pvObject, indices);
                    auto indexCount = static_cast<int>(indices.size());

                    auto hImportedIndices = pContextImpl->CreateArray(indexCount);
                    for (auto index = 0; index < indexCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedIndices->Set(pContextImpl->m_hContext, index, pContextImpl->CreateInteger(indices[index]))));
                    }

                    CALLBACK_RETURN(hImportedIndices);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto hHolder = info.HolderV2();
            auto pvObject = pContextImpl->GetHostObject(hHolder);

            auto hName = ::ValueAsString(hKey);
            if (hName.IsEmpty())
            {
                if (!hKey.IsEmpty() && hKey->StrictEquals(pContextImpl->m_hIsHostObjectKey))
                {
                    CALLBACK_RETURN((pvObject != nullptr) ? pContextImpl->GetTrue() : pContextImpl->GetFalse());
                }
            }
            else if (pvObject != nullptr)
            {
                try
                {
                    auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                    if (!hCache.IsEmpty() && FROM_MAYBE(hCache->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        auto hValue = FROM_MAYBE(hCache->Get(pContextImpl->m_hContext, hName));
                        CALLBACK_RETURN(!hValue->StrictEquals(pContextImpl->m_hMissingPropertyValue) ? hValue : v8::Local<v8::Value>());
                    }

                    bool isCacheable;
                    auto hResult = pContextImpl->ImportValue(FastHostObjectUtil::GetProperty(pvObject, pContextImpl->CreateStdString(hName), isCacheable));
                    if (isCacheable)
                    {
                        auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                        if (hCache.IsEmpty())
                        {
                            hCache = pContextImpl->CreateObject();
                            ASSERT_EVAL(FROM_MAYBE(hHolder->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey, hCache)));
                        }

                        ASSERT_EVAL(FROM_MAYBE(hCache->Set(pContextImpl->m_hContext, hName, !hResult.IsEmpty() ? hResult : pContextImpl->m_hMissingPropertyValue)));
                    }

                    CALLBACK_RETURN(hResult);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetFastHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                FastHostObjectUtil::SetProperty(pvObject, pContextImpl->CreateStdString(hName), pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN_VOID();
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                auto flags = FastHostObjectUtil::QueryProperty(pvObject, pContextImpl->CreateStdString(hName));
                if (::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Available))
                {
                    auto attrs = v8::None;

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Writable))
                    {
                        attrs = ::CombineFlags(attrs, v8::ReadOnly);
                    }

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Enumerable))
                    {
                        attrs = ::CombineFlags(attrs, v8::DontEnum);
                    }

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Deletable))
                    {
                        attrs = ::CombineFlags(attrs, v8::DontDelete);
                    }

                    CALLBACK_RETURN(attrs);
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteFastHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(FastHostObjectUtil::DeleteProperty(pvObject, pContextImpl->CreateStdString(hName)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetFastHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<StdString> names;
                    FastHostObjectUtil::GetPropertyNames(pvObject, names);
                    auto nameCount = static_cast<int>(names.size());

                    auto hImportedNames = pContextImpl->CreateArray(nameCount);
                    for (auto index = 0; index < nameCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedNames->Set(pContextImpl->m_hContext, index, FROM_MAYBE(pContextImpl->CreateString(names[index])))));
                    }

                    CALLBACK_RETURN(hImportedNames);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::GetFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(pContextImpl->ImportValue(FastHostObjectUtil::GetProperty(pvObject, index)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::SetFastHostObjectProperty(uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<void>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                FastHostObjectUtil::SetProperty(pvObject, index, pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN_VOID();
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::QueryFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                auto flags = FastHostObjectUtil::QueryProperty(pvObject, index);
                if (::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Available))
                {
                    auto attrs = v8::None;

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Writable))
                    {
                        attrs = ::CombineFlags(attrs, v8::ReadOnly);
                    }

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Enumerable))
                    {
                        attrs = ::CombineFlags(attrs, v8::DontEnum);
                    }

                    if (!::HasFlag(flags, FastHostObjectUtil::PropertyFlags::Deletable))
                    {
                        attrs = ::CombineFlags(attrs, v8::DontDelete);
                    }

                    CALLBACK_RETURN(attrs);
                }
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

v8::Intercepted V8ContextImpl::DeleteFastHostObjectProperty(uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    CALLBACK_ENTER

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(FastHostObjectUtil::DeleteProperty(pvObject, index));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }

    CALLBACK_EXIT
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetFastHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.HolderV2());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<int> indices;
                    FastHostObjectUtil::GetPropertyIndices(pvObject, indices);
                    auto indexCount = static_cast<int>(indices.size());

                    auto hImportedIndices = pContextImpl->CreateArray(indexCount);
                    for (auto index = 0; index < indexCount; index++)
                    {
                        ASSERT_EVAL(FROM_MAYBE(hImportedIndices->Set(pContextImpl->m_hContext, index, pContextImpl->CreateInteger(indices[index]))));
                    }

                    CALLBACK_RETURN(hImportedIndices);
                }
                catch (const HostException& exception)
                {
                    pContextImpl->ThrowScriptException(exception);
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeHostObject(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.This());
        if (pvObject != nullptr)
        {
            try
            {
                auto argCount = static_cast<size_t>(info.Length());

                if (argCount < 1)
                {
                    CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectUtil::Invoke(pvObject, info.IsConstructCall(), 0, nullptr)));
                }

                if (argCount <= Constants::MaxInlineArgCount)
                {
                    NonexistentV8Value exportedArgs[argCount];

                    for (size_t index = 0; index < argCount; index++)
                    {
                        exportedArgs[index] = pContextImpl->ExportValue(info[index]);
                    }

                    CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectUtil::Invoke(pvObject, info.IsConstructCall(), argCount, exportedArgs)));
                }

                std::vector<V8Value> exportedArgs;
                exportedArgs.reserve(argCount);

                for (size_t index = 0; index < argCount; index++)
                {
                    exportedArgs.push_back(pContextImpl->ExportValue(info[index]));
                }

                CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectUtil::Invoke(pvObject, info.IsConstructCall(), argCount, exportedArgs.data())));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeFastHostObject(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.This());
        if (pvObject != nullptr)
        {
            try
            {
                auto argCount = static_cast<size_t>(info.Length());

                if (argCount < 1)
                {
                    CALLBACK_RETURN(pContextImpl->ImportValue(FastHostObjectUtil::Invoke(pvObject, info.IsConstructCall(), 0, nullptr)));
                }

                if (argCount <= Constants::MaxInlineArgCount)
                {
                    NonexistentV8Value exportedArgs[argCount];

                    for (size_t index = 0; index < argCount; index++)
                    {
                        exportedArgs[index] = pContextImpl->ExportValue(info[index]);
                    }

                    CALLBACK_RETURN(pContextImpl->ImportValue(FastHostObjectUtil::Invoke(pvObject, info.IsConstructCall(), argCount, exportedArgs)));
                }

                std::vector<V8Value> exportedArgs;
                exportedArgs.reserve(argCount);

                for (size_t index = 0; index < argCount; index++)
                {
                    exportedArgs.push_back(pContextImpl->ExportValue(info[index]));
                }

                CALLBACK_RETURN(pContextImpl->ImportValue(FastHostObjectUtil::Invoke(pvObject, info.IsConstructCall(), argCount, exportedArgs.data())));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::FlushCallback(const v8::FunctionCallbackInfo<v8::Value>& /*info*/)
{
}

//-----------------------------------------------------------------------------

void V8ContextImpl::PerformanceNowCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto now = HighResolutionClock::GetRelativeMilliseconds();

    auto pContextImpl = ::GetContextImplFromData(info);
    if (pContextImpl == nullptr)
    {
        return;
    }

    CALLBACK_RETURN(now - pContextImpl->m_RelativeTimeOrigin);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::PerformanceSleepCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (pContextImpl != nullptr)
    {
        double delay;
        if ((info.Length() < 1) || !::TryGetValueAsNumber(pContextImpl->m_hContext, info[0], delay))
        {
            pContextImpl->ThrowException(v8::Exception::TypeError(pContextImpl->CreateString("Invalid argument: 'delay' must be a number")));
        }
        else
        {
            auto precise = false;
            if (info.Length() > 1)
            {
                auto hArg = info[1];
                if (!hArg.IsEmpty())
                {
                    precise = hArg->BooleanValue(info.GetIsolate());
                }
            }

            if (delay > 0)
            {
                CALLBACK_RETURN(HighResolutionClock::SleepMilliseconds(delay, precise));
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetModuleResultCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto hMeta = ::ValueAsObject(info.Data());
        if (!hMeta.IsEmpty())
        {
            auto pIsolate = info.GetIsolate();
            if (pIsolate != nullptr)
            {
                auto pIsolateImpl = V8IsolateImpl::GetInstanceFromIsolate(pIsolate);
                if (pIsolateImpl != nullptr)
                {
                    auto pContextImpl = pIsolateImpl->FindContext(FROM_MAYBE(hMeta->GetCreationContext(pIsolate)));
                    if (pContextImpl != nullptr)
                    {
                        auto hValue = (info.Length() > 0) ? info[0] : v8::Local<v8::Value>(pContextImpl->GetUndefined());
                        auto value = pContextImpl->ExportValue(hValue);
                        ASSERT_EVAL(FROM_MAYBE(hMeta->Set(pContextImpl->m_hContext, pContextImpl->m_hModuleResultKey, hValue)));
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetPromiseStateCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (pContextImpl != nullptr)
    {
        if (info.Length() > 0)
        {
            auto hArg = info[0];
            if (!hArg.IsEmpty() && hArg->IsPromise())
            {
                auto hPromise = hArg.As<v8::Promise>();
                CALLBACK_RETURN(hPromise->State());
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetPromiseResultCallback(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (pContextImpl != nullptr)
    {
        if (info.Length() > 0)
        {
            auto hArg = info[0];
            if (!hArg.IsEmpty() && hArg->IsPromise())
            {
                auto hPromise = hArg.As<v8::Promise>();
                if (hPromise->State() != v8::Promise::PromiseState::kPending)
                {
                    CALLBACK_RETURN(hPromise->Result());
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisposeWeakHandle(v8::Isolate* pIsolate, Persistent<v8::Object>* phObject, HostObjectHolder* pHolder, void* pvV8ObjectCache)
{
    IGNORE_UNUSED(pIsolate);

    ASSERT_EVAL(HostObjectUtil::RemoveV8ObjectCacheEntry(pvV8ObjectCache, pHolder->GetObject()));
    delete pHolder;

    phObject->Dispose();
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetCachedModuleInfo(v8::Local<v8::Module> hModule, V8DocumentInfo& documentInfo)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if (it->hModule == hModule)
        {
            m_ModuleCache.splice(m_ModuleCache.begin(), m_ModuleCache, it);
            documentInfo = it->DocumentInfo;
            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetCachedModuleInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if (it->DocumentInfo.GetUniqueId() == uniqueId)
        {
            m_ModuleCache.splice(m_ModuleCache.begin(), m_ModuleCache, it);
            documentInfo = it->DocumentInfo;
            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetCachedModuleMetaHolder(v8::Local<v8::Module> hModule, v8::Local<v8::Object>& hMetaHolder)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if (it->hModule == hModule)
        {
            hMetaHolder = it->hMetaHolder;
            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetCachedModuleMetaHolder(uint64_t uniqueId, v8::Local<v8::Object>& hMetaHolder)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if (it->DocumentInfo.GetUniqueId() == uniqueId)
        {
            hMetaHolder = it->hMetaHolder;
            return true;
        }
    }

    return false;
}

//-----------------------------------------------------------------------------

v8::Local<v8::Module> V8ContextImpl::GetCachedModule(uint64_t uniqueId, size_t codeDigest)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ModuleCache.splice(m_ModuleCache.begin(), m_ModuleCache, it);
            return it->hModule;
        }
    }

    return v8::Local<v8::Module>();
}

//-----------------------------------------------------------------------------

v8::Local<v8::Module> V8ContextImpl::GetCachedModule(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ModuleCache.splice(m_ModuleCache.begin(), m_ModuleCache, it);
            cacheBytes = it->CacheBytes;
            return it->hModule;
        }
    }

    cacheBytes.clear();
    return v8::Local<v8::Module>();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CacheModule(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::Module> hModule)
{
    CacheModule(documentInfo, codeDigest, hModule, std::vector<uint8_t>());
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CacheModule(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::Module> hModule, const std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    auto maxModuleCacheSize = HostObjectUtil::GetMaxModuleCacheSize();
    while (m_ModuleCache.size() >= maxModuleCacheSize)
    {
        const auto& entry = m_ModuleCache.back();
        Dispose(entry.hMetaHolder);
        Dispose(entry.hModule);
        m_ModuleCache.pop_back();
    }

    _ASSERTE(std::none_of(m_ModuleCache.begin(), m_ModuleCache.end(),
        [&documentInfo, codeDigest] (const ModuleCacheEntry& entry)
    {
        return (entry.DocumentInfo.GetUniqueId() == documentInfo.GetUniqueId()) && (entry.CodeDigest == codeDigest);
    }));

    ModuleCacheEntry entry { documentInfo, codeDigest, CreatePersistent(hModule), cacheBytes, CreatePersistent(CreateObject()) };
    m_ModuleCache.push_front(std::move(entry));

    m_Statistics.ModuleCacheSize = m_ModuleCache.size();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetCachedModuleCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes)
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    for (auto it = m_ModuleCache.begin(); it != m_ModuleCache.end(); it++)
    {
        if ((it->DocumentInfo.GetUniqueId() == uniqueId) && (it->CodeDigest == codeDigest))
        {
            m_ModuleCache.splice(m_ModuleCache.begin(), m_ModuleCache, it);
            it->CacheBytes = cacheBytes;
            return;
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ClearModuleCache()
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    while (!m_ModuleCache.empty())
    {
        const auto& entry = m_ModuleCache.front();
        Dispose(entry.hMetaHolder);
        Dispose(entry.hModule);
        m_ModuleCache.pop_front();
    }

    m_Statistics.ModuleCacheSize = m_ModuleCache.size();
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::TryGetCachedScriptInfo(uint64_t uniqueId, V8DocumentInfo& documentInfo)
{
    return m_spIsolateImpl->TryGetCachedScriptInfo(uniqueId, documentInfo);
}

//-----------------------------------------------------------------------------

v8::Local<v8::UnboundScript> V8ContextImpl::GetCachedScript(uint64_t uniqueId, size_t codeDigest)
{
    return m_spIsolateImpl->GetCachedScript(uniqueId, codeDigest);
}

//-----------------------------------------------------------------------------

v8::Local<v8::UnboundScript> V8ContextImpl::GetCachedScript(uint64_t uniqueId, size_t codeDigest, std::vector<uint8_t>& cacheBytes)
{
    return m_spIsolateImpl->GetCachedScript(uniqueId, codeDigest, cacheBytes);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript)
{
    m_spIsolateImpl->CacheScript(documentInfo, codeDigest, hScript);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CacheScript(const V8DocumentInfo& documentInfo, size_t codeDigest, v8::Local<v8::UnboundScript> hScript, const std::vector<uint8_t>& cacheBytes)
{
    m_spIsolateImpl->CacheScript(documentInfo, codeDigest, hScript, cacheBytes);
}

void V8ContextImpl::SetCachedScriptCacheBytes(uint64_t uniqueId, size_t codeDigest, const std::vector<uint8_t>& cacheBytes)
{
    m_spIsolateImpl->SetCachedScriptCacheBytes(uniqueId, codeDigest, cacheBytes);
}

//-----------------------------------------------------------------------------

v8::Local<v8::Value> V8ContextImpl::ImportValue(const V8Value& value)
{
    FROM_MAYBE_TRY

        if (value.IsNonexistent())
        {
            return v8::Local<v8::Value>();
        }

        if (value.IsUndefined())
        {
            return GetUndefined();
        }

        if (value.IsNull())
        {
            return GetNull();
        }

        {
            bool result;
            if (value.AsBoolean(result))
            {
                return result ? GetTrue() : GetFalse();
            }
        }

        {
            double result;
            if (value.AsNumber(result))
            {
                return CreateNumber(result);
            }
        }

        {
            const StdString* pString;
            if (value.AsString(pString))
            {
                return FROM_MAYBE(CreateString(*pString));
            }
        }

        {
            HostObjectHolder* pHolder;
            V8Value::Subtype subtype;
            V8Value::Flags flags;
            if (value.AsHostObject(pHolder, subtype, flags))
            {
                auto pvV8Object = HostObjectUtil::GetCachedV8Object(m_pvV8ObjectCache, pHolder->GetObject());
                if (pvV8Object != nullptr)
                {
                    return CreateLocal(::HandleFromPtr<v8::Object>(pvV8Object));
                }

                v8::Local<v8::Object> hObject;

                if (::HasFlag(flags, V8Value::Flags::Fast))
                {
                    if (subtype == V8Value::Subtype::Function)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                            hObject = FROM_MAYBE(m_hFastHostFunctionTemplate->InstanceTemplate()->NewInstance(m_hContext));
                        END_PULSE_VALUE_SCOPE
                    }
                    else
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                            hObject = FROM_MAYBE(m_hFastHostObjectTemplate->InstanceTemplate()->NewInstance(m_hContext));
                        END_PULSE_VALUE_SCOPE
                    }
                }
                else
                {
                    auto invocability = HostObjectUtil::GetInvocability(pHolder->GetObject());
                    if (invocability == HostObjectUtil::Invocability::None)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                            hObject = FROM_MAYBE(m_hHostObjectTemplate->InstanceTemplate()->NewInstance(m_hContext));
                        END_PULSE_VALUE_SCOPE
                    }
                    else if (invocability == HostObjectUtil::Invocability::Delegate)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                            hObject = FROM_MAYBE(m_hHostDelegateTemplate->InstanceTemplate()->NewInstance(m_hContext));
                        END_PULSE_VALUE_SCOPE
                    }
                    else
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                            hObject = FROM_MAYBE(m_hHostInvocableTemplate->InstanceTemplate()->NewInstance(m_hContext));
                        END_PULSE_VALUE_SCOPE
                    }
                }

                ASSERT_EVAL(SetHostObjectHolder(hObject, pHolder = pHolder->Clone()));
                ASSERT_EVAL(FROM_MAYBE(hObject->SetPrivate(m_hContext, m_hAccessTokenKey, m_hAccessToken)));
                pvV8Object = ::PtrFromHandle(MakeWeak(CreatePersistent(hObject), pHolder, m_pvV8ObjectCache, DisposeWeakHandle));
                HostObjectUtil::CacheV8Object(m_pvV8ObjectCache, pHolder->GetObject(), pvV8Object);

                return hObject;
            }
        }

        {
            V8ObjectHolder* pHolder;
            V8Value::Subtype subtype;
            V8Value::Flags flags;
            if (value.AsV8Object(pHolder, subtype, flags))
            {
                if (pHolder->IsSameIsolate(m_spIsolateImpl))
                {
                    return CreateLocal(::HandleFromPtr<v8::Object>(pHolder->GetObject()));
                }

                if (::HasFlag(flags, V8Value::Flags::Shared))
                {
                    const auto& spSharedObjectInfo = pHolder->GetSharedObjectInfo();
                    if (!spSharedObjectInfo.IsEmpty())
                    {
                        switch (subtype)
                        {
                            case V8Value::Subtype::ArrayBuffer:
                                return CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore());

                            case V8Value::Subtype::DataView:
                                return v8::DataView::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetSize());

                            case V8Value::Subtype::Uint8Array:
                                return v8::Uint8Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Uint8ClampedArray:
                                return v8::Uint8ClampedArray::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Int8Array:
                                return v8::Int8Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Uint16Array:
                                return v8::Uint16Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Int16Array:
                                return v8::Int16Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Uint32Array:
                                return v8::Uint32Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Int32Array:
                                return v8::Int32Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::BigUint64Array:
                                return v8::BigUint64Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::BigInt64Array:
                                return v8::BigInt64Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Float32Array:
                                return v8::Float32Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            case V8Value::Subtype::Float64Array:
                                return v8::Float64Array::New(CreateSharedArrayBuffer(spSharedObjectInfo->GetBackingStore()), spSharedObjectInfo->GetOffset(), spSharedObjectInfo->GetLength());

                            default:
                                break;
                        }
                    }
                }

                return GetUndefined();
            }
        }

        {
            double result;
            if (value.AsDateTime(result))
            {
                return FROM_MAYBE(v8::Date::New(m_hContext, result));
            }
        }

        {
            const V8BigInt* pBigInt;
            if (value.AsBigInt(pBigInt))
            {
                const auto& words = pBigInt->GetWords();
                const auto wordCount = static_cast<int>(std::min(words.size(), static_cast<size_t>(INT_MAX)));
                return FROM_MAYBE(v8::BigInt::NewFromWords(m_hContext, pBigInt->GetSignBit(), wordCount, words.data()));
            }
        }

    FROM_MAYBE_CATCH_CONSUME

    return GetUndefined();
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::ExportValue(v8::Local<v8::Value> hValue)
{
    FROM_MAYBE_TRY

        if (hValue.IsEmpty())
        {
            return V8Value(V8Value::Nonexistent);
        }

        if (hValue->IsUndefined())
        {
            return V8Value(V8Value::Undefined);
        }

        if (hValue->IsNull())
        {
            return V8Value(V8Value::Null);
        }

        {
            bool value;
            if (::TryGetValueAsBoolean(m_spIsolateImpl, hValue, value))
            {
                return V8Value(value);
            }
        }

        {
            double value;
            if (::TryGetValueAsNumber(m_hContext, hValue, value))
            {
                return V8Value(value);
            }
        }

        auto hString = ::ValueAsString(hValue);
        if (!hString.IsEmpty())
        {
            return V8Value(new StdString(CreateStdString(hString)));
        }

        if (m_DateTimeConversionEnabled && hValue->IsDate())
        {
            return V8Value(V8Value::DateTime, hValue.As<v8::Date>()->ValueOf());
        }

        auto hBigInt = ::ValueAsBigInt(hValue);
        if (!hBigInt.IsEmpty())
        {
            auto signBit = 0;
            std::vector<uint64_t> words;

            auto wordCount = hBigInt->WordCount();
            if (wordCount > 0)
            {
                words.resize(wordCount);
                hBigInt->ToWordsArray(&signBit, &wordCount, words.data());
            }

            return V8Value(new V8BigInt(signBit, std::move(words)));
        }

        auto hObject = ::ValueAsObject(hValue);
        if (!hObject.IsEmpty())
        {
            auto pHolder = GetHostObjectHolder(hObject);
            if (pHolder != nullptr)
            {
                return V8Value(pHolder->Clone());
            }

            auto subtype = V8Value::Subtype::None;
            auto flags = V8Value::Flags::None;
            SharedPtr<V8SharedObjectInfo> spSharedObjectInfo;

            if (hObject->IsFunction())
            {
                subtype = V8Value::Subtype::Function;

                if (hObject->IsAsyncFunction())
                {
                    flags = ::CombineFlags(flags, V8Value::Flags::Async);
                }

                if (hObject->IsGeneratorFunction())
                {
                    flags = ::CombineFlags(flags, V8Value::Flags::Generator);
                }
            }
            else if (hObject->IsGeneratorObject())
            {
                subtype = V8Value::Subtype::Iterator;

                if (m_hAsyncGeneratorConstructor.IsEmpty())
                {
                    auto hEngineInternal = FROM_MAYBE(m_hContext->Global()->Get(m_hContext, CreateString("EngineInternal"))).As<v8::Object>();
                    m_hAsyncGeneratorConstructor = CreatePersistent(FROM_MAYBE(hEngineInternal->Get(m_hContext, CreateString("asyncGenerator"))));
                }

                if (FROM_MAYBE(hObject->Get(m_hContext, m_hConstructorKey))->StrictEquals(m_hAsyncGeneratorConstructor))
                {
                    flags = ::CombineFlags(flags, V8Value::Flags::Async);
                }
            }
            else if (hObject->IsMapIterator() || hObject->IsSetIterator())
            {
                subtype = V8Value::Subtype::Iterator;
            }
            else if (hObject->IsPromise())
            {
                subtype = V8Value::Subtype::Promise;

                auto hPromise = hObject.As<v8::Promise>();
                switch (hPromise->State())
                {
                    case v8::Promise::PromiseState::kFulfilled:
                        break;

                    case v8::Promise::PromiseState::kRejected:
                        flags = ::CombineFlags(flags, V8Value::Flags::Rejected);
                        break;

                    default:
                        flags = ::CombineFlags(flags, V8Value::Flags::Pending);
                        break;
                }
            }
            else if (hObject->IsArray())
            {
                subtype = V8Value::Subtype::Array;
            }
            else if (hObject->IsArrayBuffer())
            {
                subtype = V8Value::Subtype::ArrayBuffer;
            }
            else if (hObject->IsSharedArrayBuffer())
            {
                subtype = V8Value::Subtype::ArrayBuffer;
                flags = ::CombineFlags(flags, V8Value::Flags::Shared);

                auto hSharedArrayBuffer = hObject.As<v8::SharedArrayBuffer>();
                auto size = hSharedArrayBuffer->ByteLength();
                spSharedObjectInfo = new V8SharedObjectInfo(hSharedArrayBuffer->GetBackingStore(), 0, size, size);
            }
            else if (hObject->IsArrayBufferView())
            {
                auto hArrayBufferView = hObject.As<v8::ArrayBufferView>();
                auto offset = hArrayBufferView->ByteOffset();
                auto size = hArrayBufferView->ByteLength();

                auto spBackingStore = hArrayBufferView->Buffer()->GetBackingStore();
                if (spBackingStore->IsShared())
                {
                    flags = ::CombineFlags(flags, V8Value::Flags::Shared);
                }

                if (hObject->IsDataView())
                {
                    subtype = V8Value::Subtype::DataView;
                    if (::HasFlag(flags, V8Value::Flags::Shared))
                    {
                        spSharedObjectInfo = new V8SharedObjectInfo(std::move(spBackingStore), offset, size, size);
                    }
                }
                else if (hObject->IsTypedArray())
                {
                    if (hObject->IsUint8Array())
                    {
                        subtype = V8Value::Subtype::Uint8Array;
                    }
                    else if (hObject->IsUint8ClampedArray())
                    {
                        subtype = V8Value::Subtype::Uint8ClampedArray;
                    }
                    else if (hObject->IsInt8Array())
                    {
                        subtype = V8Value::Subtype::Int8Array;
                    }
                    else if (hObject->IsUint16Array())
                    {
                        subtype = V8Value::Subtype::Uint16Array;
                    }
                    else if (hObject->IsInt16Array())
                    {
                        subtype = V8Value::Subtype::Int16Array;
                    }
                    else if (hObject->IsUint32Array())
                    {
                        subtype = V8Value::Subtype::Uint32Array;
                    }
                    else if (hObject->IsInt32Array())
                    {
                        subtype = V8Value::Subtype::Int32Array;
                    }
                    else if (hObject->IsBigUint64Array())
                    {
                        subtype = V8Value::Subtype::BigUint64Array;
                    }
                    else if (hObject->IsBigInt64Array())
                    {
                        subtype = V8Value::Subtype::BigInt64Array;
                    }
                    else if (hObject->IsFloat32Array())
                    {
                        subtype = V8Value::Subtype::Float32Array;
                    }
                    else if (hObject->IsFloat64Array())
                    {
                        subtype = V8Value::Subtype::Float64Array;
                    }

                    if (::HasFlag(flags, V8Value::Flags::Shared) && (subtype != V8Value::Subtype::None))
                    {
                        auto hTypedArray = hObject.As<v8::TypedArray>();
                        spSharedObjectInfo = new V8SharedObjectInfo(std::move(spBackingStore), offset, size, hTypedArray->Length());
                    }
                }
            }

            return V8Value(new V8ObjectHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hObject)), hObject->GetIdentityHash(), spSharedObjectInfo), subtype, flags);
        }

    FROM_MAYBE_CATCH_CONSUME

    return V8Value(V8Value::Undefined);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ImportValues(const std::vector<V8Value>& values, std::vector<v8::Local<v8::Value>>& importedValues)
{
    importedValues.clear();

    auto valueCount = static_cast<int>(values.size());
    importedValues.reserve(valueCount);

    for (auto index = 0; index < valueCount; index++)
    {
        importedValues.push_back(ImportValue(values[index]));
    }
}

//-----------------------------------------------------------------------------

v8::ScriptOrigin V8ContextImpl::CreateScriptOrigin(const V8DocumentInfo& documentInfo)
{
    FROM_MAYBE_TRY

        auto uniqueId = documentInfo.GetUniqueId();
        _ASSERTE(uniqueId > 0);

        auto hHostDefinedOptions = CreatePrimitiveArray(1);
        SetPrimitiveArrayItem(hHostDefinedOptions, 0, CreateBigInt(uniqueId));

        return CreateScriptOrigin(
            FROM_MAYBE(CreateString(documentInfo.GetResourceName())),
            0,
            0,
            false,
            -1,
            (documentInfo.GetSourceMapUrl().GetLength() > 0) ? FROM_MAYBE(CreateString(documentInfo.GetSourceMapUrl())) : v8::Local<v8::String>(),
            false,
            false,
            documentInfo.IsModule(),
            hHostDefinedOptions
        );

    FROM_MAYBE_CATCH

        throw;

    FROM_MAYBE_END
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Verify(const V8IsolateImpl::ExecutionScope& isolateExecutionScope, const v8::TryCatch& tryCatch)
{
    if (tryCatch.HasCaught())
    {
        if (!tryCatch.CanContinue())
        {
            VerifyNotOutOfMemory();
            throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(SL("Script execution interrupted by host")), CreateStdString(FROM_MAYBE_DEFAULT(tryCatch.StackTrace(m_hContext))), isolateExecutionScope.ExecutionStarted(), V8Value(V8Value::Null), V8Value(V8Value::Undefined));
        }

        auto hException = tryCatch.Exception();
        if (hException->SameValue(m_hTerminationException))
        {
            VerifyNotOutOfMemory();
            throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(SL("Script execution interrupted by host")), CreateStdString(FROM_MAYBE_DEFAULT(tryCatch.StackTrace(m_hContext))), isolateExecutionScope.ExecutionStarted(), V8Value(V8Value::Null), V8Value(V8Value::Undefined));
        }

        StdString message;
        bool stackOverflow;

        StdString constructorName;
        if (hException->IsObject())
        {
            constructorName = CreateStdString(hException.As<v8::Object>()->GetConstructorName());
        }

        auto value = CreateStdString(hException);
        if (value.GetLength() > 0)
        {
            message = std::move(value);
            stackOverflow = (strcmp(message.ToUTF8().c_str(), "RangeError: Maximum call stack size exceeded") == 0);
        }
        else if (!hException->IsObject())
        {
            message = SL("Unknown error; an unrecognized value was thrown and not caught");
            stackOverflow = false;
        }
        else
        {
            if ((constructorName == SL("Error")) || (constructorName == SL("RangeError")))
            {
                // It is unclear why V8 sometimes throws Error or RangeError objects that convert
                // to empty strings, but it probably has to do with memory pressure. It seems to
                // happen only during stack overflow recovery.

                message = SL("Unknown error (");
                message += constructorName;
                message += SL("); potential stack overflow detected");
                stackOverflow = true;
            }
            else if (constructorName.GetLength() > 0)
            {
                message = SL("Unknown error (");
                message += constructorName;
                message += SL(")");
                stackOverflow = false;
            }
            else
            {
                message = SL("Unknown error; an unrecognized object was thrown and not caught");
                stackOverflow = false;
            }
        }

        StdString stackTrace;
        V8Value hostException(V8Value::Undefined);

        if (stackOverflow)
        {
            stackTrace = message;
        }
        else
        {
            auto hStackTrace = FROM_MAYBE_DEFAULT(tryCatch.StackTrace(m_hContext));
            if (!hStackTrace.IsEmpty())
            {
                stackTrace = CreateStdString(hStackTrace);
            }

            auto hMessage = tryCatch.Message();
            if (!hMessage.IsEmpty())
            {
                if (message.GetLength() < 1)
                {
                    message = CreateStdString(hMessage->Get());
                }

                stackTrace = message;

                auto hMessageStackTrace = hMessage->GetStackTrace();
                auto frameCount = !hMessageStackTrace.IsEmpty() ? hMessageStackTrace->GetFrameCount() : 0;
                auto usedSourceLine = false;

                if ((frameCount < 1) || (constructorName == SL("SyntaxError")))
                {
                    auto hScriptResourceName = hMessage->GetScriptResourceName();
                    if (!hScriptResourceName.IsEmpty() && !hScriptResourceName->IsNull() && !hScriptResourceName->IsUndefined())
                    {
                        auto hScriptName = FROM_MAYBE_DEFAULT(hScriptResourceName->ToString(m_hContext));
                        if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                        {
                            stackTrace += SL("\n    at ");
                            stackTrace += CreateStdString(hScriptName);
                        }
                        else
                        {
                            stackTrace += SL("\n    at <anonymous>");
                        }
                    }
                    else
                    {
                        stackTrace += SL("\n    at <anonymous>");
                    }

                    stackTrace += SL(':');
                    stackTrace += StdString(std::to_string(FROM_MAYBE_DEFAULT(hMessage->GetLineNumber(m_hContext))));
                    stackTrace += SL(':');
                    stackTrace += StdString(std::to_string(FROM_MAYBE_DEFAULT(hMessage->GetStartColumn(m_hContext)) + 1));

                    auto hSourceLine = FROM_MAYBE_DEFAULT(hMessage->GetSourceLine(m_hContext));
                    if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                    {
                        stackTrace += SL(" -> ");
                        stackTrace += CreateStdString(hSourceLine);
                    }

                    usedSourceLine = true;
                }

                for (int index = 0; index < frameCount; index++)
                {
                    auto hFrame = GetStackFrame(hMessageStackTrace, index);
                    stackTrace += SL("\n    at ");

                    auto hFunctionName = hFrame->GetFunctionName();
                    if (!hFunctionName.IsEmpty() && (hFunctionName->Length() > 0))
                    {
                        if (hFrame->IsConstructor())
                        {
                            stackTrace += SL("new ");
                        }

                        stackTrace += CreateStdString(hFunctionName);
                        stackTrace += SL(" (");
                    }

                    auto hScriptName = hFrame->GetScriptName();
                    if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                    {
                        stackTrace += CreateStdString(hScriptName);
                    }
                    else
                    {
                        stackTrace += SL("<anonymous>");
                    }

                    stackTrace += SL(':');
                    auto lineNumber = hFrame->GetLineNumber();
                    if (lineNumber != v8::Message::kNoLineNumberInfo)
                    {
                        stackTrace += StdString(std::to_string(lineNumber));
                    }

                    stackTrace += SL(':');
                    auto column = hFrame->GetColumn();
                    if (column != v8::Message::kNoColumnInfo)
                    {
                        stackTrace += StdString(std::to_string(column));
                    }

                    if (!hFunctionName.IsEmpty() && (hFunctionName->Length() > 0))
                    {
                        stackTrace += L')';
                    }

                    if (!usedSourceLine)
                    {
                        auto hSourceLine = FROM_MAYBE_DEFAULT(hMessage->GetSourceLine(m_hContext));
                        if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                        {
                            stackTrace += SL(" -> ");
                            stackTrace += CreateStdString(hSourceLine);
                        }

                        usedSourceLine = true;
                    }
                }
            }

            if (hException->IsObject())
            {
                hostException = ExportValue(FROM_MAYBE_DEFAULT(hException.As<v8::Object>()->Get(m_hContext, m_hHostExceptionKey)));
            }
        }

        throw V8Exception(V8Exception::Type::General, m_Name, std::move(message), std::move(stackTrace), isolateExecutionScope.ExecutionStarted(), ExportValue(hException), std::move(hostException));
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::VerifyNotOutOfMemory()
{
    if (m_spIsolateImpl->IsOutOfMemory())
    {
        m_spIsolateImpl->ThrowOutOfMemoryException();
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ThrowScriptException(const HostException& exception)
{
    FROM_MAYBE_TRY

        // WARNING: Error instantiation may fail during script interruption. Check Exception::Error()
        // result to avoid access violations. Extra defense is warranted here.

        if (!IsExecutionTerminating())
        {
            auto hException = v8::Exception::Error(FROM_MAYBE(CreateString(exception.GetMessage()))).As<v8::Object>();
            if (!hException.IsEmpty() && hException->IsObject())
            {
                if (!m_HideHostExceptions)
                {
                    auto hHostException = ImportValue(exception.GetException());
                    if (!hHostException.IsEmpty() && hHostException->IsObject())
                    {
                        ASSERT_EVAL(FROM_MAYBE(hException->Set(m_hContext, m_hHostExceptionKey, hHostException)));
                    }
                }

                ThrowException(hException);
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}
