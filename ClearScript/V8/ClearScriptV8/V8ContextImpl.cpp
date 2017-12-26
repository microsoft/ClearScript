// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// FromMaybeScope
//-----------------------------------------------------------------------------

class FromMaybeScope
{
};

//-----------------------------------------------------------------------------
// FromMaybeFailure
//-----------------------------------------------------------------------------

class FromMaybeFailure
{
};

//-----------------------------------------------------------------------------
// local helper functions
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

inline v8::Local<v8::String> ValueAsString(const v8::Local<v8::Value>& hValue)
{
    return (!hValue.IsEmpty() && hValue->IsString()) ? hValue.As<v8::String>() : v8::Local<v8::String>();
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

template <typename TInfo>
static V8ContextImpl* GetContextImplFromHolder(const TInfo& info)
{
    auto hHolder = info.Holder();
    if (!hHolder.IsEmpty() && hHolder->InternalFieldCount() > 0)
    {
        auto hField = hHolder->GetInternalField(0);
        if (!hField.IsEmpty() && !hField->IsUndefined())
        {
            return static_cast<V8ContextImpl*>(hHolder->GetAlignedPointerFromInternalField(0));
        }
    }

    return nullptr;
}

//-----------------------------------------------------------------------------

template <typename TInfo>
static V8ContextImpl* GetContextImplFromData(const TInfo& info)
{
    auto hContextImpl = ::ValueAsExternal(info.Data());
    return !hContextImpl.IsEmpty() ? static_cast<V8ContextImpl*>(hContextImpl->Value()) : nullptr;
}

//-----------------------------------------------------------------------------
// V8ContextImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        V8IsolateImpl::Scope t_IsolateScope(m_spIsolateImpl); \
    __pragma(warning(default:4456))

#define END_ISOLATE_SCOPE \
        IGNORE_UNUSED(t_IsolateScope); \
    }

#define FROM_MAYBE_TRY \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        FromMaybeScope t_FromMaybeScope; \
    __pragma(warning(default:4456)) \
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

#define BEGIN_CONTEXT_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        Scope t_ContextScope(this); \
    __pragma(warning(default:4456))

#define END_CONTEXT_SCOPE \
        IGNORE_UNUSED(t_ContextScope); \
    }

#define BEGIN_EXECUTION_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        V8IsolateImpl::ExecutionScope t_ExecutionScope(m_spIsolateImpl); \
        V8IsolateImpl::TryCatch t_TryCatch(m_spIsolateImpl); \
    __pragma(warning(default:4456))

#define END_EXECUTION_SCOPE \
        IGNORE_UNUSED(t_TryCatch); \
        IGNORE_UNUSED(t_ExecutionScope); \
    }

#define EXECUTION_STARTED \
    (t_ExecutionScope.ExecutionStarted())

#define VERIFY(RESULT) \
    (Verify(t_ExecutionScope, t_TryCatch, RESULT))

#define VERIFY_MAYBE(RESULT) \
    (Verify(t_ExecutionScope, t_TryCatch, FROM_MAYBE_DEFAULT(RESULT)))

#define VERIFY_CHECKPOINT() \
    (Verify(t_ExecutionScope, t_TryCatch))

#define CALLBACK_RETURN(RESULT) \
    BEGIN_COMPOUND_MACRO \
        info.GetReturnValue().Set(RESULT); \
        return; \
    END_COMPOUND_MACRO

//-----------------------------------------------------------------------------

static std::atomic<size_t> s_InstanceCount(0);

//-----------------------------------------------------------------------------

V8ContextImpl::V8ContextImpl(V8IsolateImpl* pIsolateImpl, const StdString& name, bool enableDebugging, bool disableGlobalMembers, bool enableRemoteDebugging, int debugPort):
    m_Name(name),
    m_spIsolateImpl(pIsolateImpl),
    m_pvV8ObjectCache(nullptr),
    m_AllowHostObjectConstructorCall(false),
    m_DisableHostObjectInterception(false)
{
    VerifyNotOutOfMemory();

    BEGIN_ISOLATE_SCOPE
    FROM_MAYBE_TRY

        if (disableGlobalMembers)
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

            auto hGlobal = ::ValueAsObject(m_hContext->Global()->GetPrototype());
            if (!hGlobal.IsEmpty() && (hGlobal->InternalFieldCount() > 0))
            {
                m_hGlobal = CreatePersistent(hGlobal);
                m_hGlobal->SetAlignedPointerInInternalField(0, this);
            }
        }

        auto hContextImpl = CreateExternal(this);

        v8::Local<v8::FunctionTemplate> hGetIteratorFunction;
        v8::Local<v8::FunctionTemplate> hToFunctionFunction;
        v8::Local<v8::FunctionTemplate> hNextFunction;

        BEGIN_CONTEXT_SCOPE

            m_hIsHostObjectKey = CreatePersistent(CreateSymbol());
            FROM_MAYBE(m_hContext->Global()->Set(m_hContext, FROM_MAYBE(CreateString(StdString(L"isHostObjectKey"))), m_hIsHostObjectKey));

            m_hHostExceptionKey = CreatePersistent(FROM_MAYBE(CreateString(StdString(L"hostException"))));
            m_hEnumeratorKey = CreatePersistent(CreatePrivate());
            m_hDoneKey = CreatePersistent(FROM_MAYBE(CreateString(StdString(L"done"))));
            m_hValueKey = CreatePersistent(FROM_MAYBE(CreateString(StdString(L"value"))));
            m_hCacheKey = CreatePersistent(CreatePrivate());
            m_hAccessTokenKey = CreatePersistent(CreatePrivate());
            m_hInternalUseOnly = CreatePersistent(FROM_MAYBE(CreateString(StdString(L"This function is for ClearScript internal use only"))));

            hGetIteratorFunction = CreateFunctionTemplate(GetIteratorForHostObject, hContextImpl);
            hToFunctionFunction = CreateFunctionTemplate(CreateFunctionForHostDelegate, hContextImpl);
            hNextFunction = CreateFunctionTemplate(AdvanceHostObjectIterator, hContextImpl);
            m_hAccessToken = CreatePersistent(CreateObject());
            m_hTerminationException = CreatePersistent(v8::Exception::Error(FROM_MAYBE(CreateString(StdString(L"Script execution was interrupted")))));

        END_CONTEXT_SCOPE

        m_hHostObjectTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostObjectTemplate->SetClassName(FROM_MAYBE(CreateString(StdString(L"HostObject"))));
        m_hHostObjectTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostObjectTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hHostObjectTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, hContextImpl));
        m_hHostObjectTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, hContextImpl);
        m_hHostObjectTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetIteratorFunction);

        m_hHostDelegateTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostDelegateTemplate->SetClassName(FROM_MAYBE(CreateString(StdString(L"HostDelegate"))));
        m_hHostDelegateTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostDelegateTemplate->InstanceTemplate()->SetHandler(v8::NamedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, hContextImpl, v8::PropertyHandlerFlags::kNone));
        m_hHostDelegateTemplate->InstanceTemplate()->SetHandler(v8::IndexedPropertyHandlerConfiguration(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, hContextImpl));
        m_hHostDelegateTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, hContextImpl);
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(GetIteratorSymbol(), hGetIteratorFunction);
        m_hHostDelegateTemplate->PrototypeTemplate()->Set(FROM_MAYBE(CreateString(StdString(L"toFunction"))), hToFunctionFunction);

        m_hHostIteratorTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostIteratorTemplate->SetClassName(FROM_MAYBE(CreateString(StdString(L"HostIterator"))));
        m_hHostIteratorTemplate->SetCallHandler(HostObjectConstructorCallHandler, hContextImpl);
        m_hHostIteratorTemplate->PrototypeTemplate()->Set(FROM_MAYBE(CreateString(StdString(L"next"))), hNextFunction);

        m_spIsolateImpl->AddContext(this, enableDebugging, enableRemoteDebugging, debugPort);
        m_pvV8ObjectCache = HostObjectHelpers::CreateV8ObjectCache();

    FROM_MAYBE_CATCH

        Teardown();
        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot initialize the script engine because a script exception is pending"), false /*executionStarted*/);

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

void V8ContextImpl::CallWithLock(LockCallbackT* pCallback, void* pvArg)
{
    VerifyNotOutOfMemory();

    BEGIN_ISOLATE_SCOPE

        (*pCallback)(pvArg);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), false /*executionStarted*/);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::AwaitDebuggerAndPause()
{
    m_spIsolateImpl->AwaitDebuggerAndPause();
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(const StdString& documentName, const StdString& code, bool evaluate, bool /*discard*/)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), v8::ScriptOrigin(FROM_MAYBE(CreateString(documentName))));
        auto hScript = VERIFY_MAYBE(v8::ScriptCompiler::Compile(m_hContext, &source));
        if (hScript.IsEmpty())
        {
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Script compilation failed; no additional information was provided by the V8 runtime"), false /*executionStarted*/);
        }

        auto hResult = VERIFY_MAYBE(hScript->Run(m_hContext));
        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const StdString& documentName, const StdString& code)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), v8::ScriptOrigin(FROM_MAYBE(CreateString(documentName))));
        auto hScript = VERIFY_MAYBE(CreateUnboundScript(&source));
        if (hScript.IsEmpty())
        {
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Script compilation failed; no additional information was provided by the V8 runtime"), false /*executionStarted*/);
        }

        return new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, std::vector<std::uint8_t>& cacheBytes)
{
    if (cacheType == V8CacheType::None)
    {
        cacheBytes.clear();
        return Compile(documentName, code);
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), v8::ScriptOrigin(FROM_MAYBE(CreateString(documentName))));
        auto hScript = VERIFY_MAYBE(CreateUnboundScript(&source, (cacheType == V8CacheType::Parser) ? v8::ScriptCompiler::kProduceParserCache : v8::ScriptCompiler::kProduceCodeCache));
        if (hScript.IsEmpty())
        {
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Script compilation failed; no additional information was provided by the V8 runtime"), false /*executionStarted*/);
        }

        cacheBytes.clear();

        auto pCachedData = source.GetCachedData();
        if (pCachedData != nullptr)
        {
            if ((pCachedData->length > 0) && (pCachedData->data != nullptr))
            {
                cacheBytes.resize(pCachedData->length);
                memcpy_s(&cacheBytes[0], cacheBytes.size(), pCachedData->data, pCachedData->length);
            }

            // Delete cached data explicitly via a custom exported method. Doing so avoids Debug-
            // Release heap mismatches caused by V8's inlining of v8::ScriptCompiler::~Source.

            source.DeleteCachedData();
        }

        return new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const StdString& documentName, const StdString& code, V8CacheType cacheType, const std::vector<std::uint8_t>& cacheBytes, bool& cacheAccepted)
{
    if ((cacheType == V8CacheType::None) || (cacheBytes.size() < 1))
    {
        cacheAccepted = false;
        return Compile(documentName, code);
    }

    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE
    FROM_MAYBE_TRY

        auto pCachedData = new v8::ScriptCompiler::CachedData(&cacheBytes[0], static_cast<int>(cacheBytes.size()), v8::ScriptCompiler::CachedData::BufferNotOwned);
        v8::ScriptCompiler::Source source(FROM_MAYBE(CreateString(code)), v8::ScriptOrigin(FROM_MAYBE(CreateString(documentName))), pCachedData);
        auto hScript = VERIFY_MAYBE(CreateUnboundScript(&source, (cacheType == V8CacheType::Parser) ? v8::ScriptCompiler::kConsumeParserCache : v8::ScriptCompiler::kConsumeCodeCache));
        if (hScript.IsEmpty())
        {
            throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Script compilation failed; no additional information was provided by the V8 runtime"), false /*executionStarted*/);
        }

        cacheAccepted = !m_spIsolateImpl->IsDebuggingEnabled() && !pCachedData->rejected;
        return new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hScript)));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

    FROM_MAYBE_END
    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::CanExecute(V8ScriptHolder* pHolder)
{
    return pHolder->IsSameIsolate(m_spIsolateImpl.GetRawPtr());
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(V8ScriptHolder* pHolder, bool evaluate)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = ::HandleFromPtr<v8::UnboundScript>(pHolder->GetScript());
        auto hResult = VERIFY_MAYBE(hScript->BindToCurrentContext()->Run(m_hContext));
        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Interrupt()
{
    TerminateExecution();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetIsolateHeapInfo(V8IsolateHeapInfo& heapInfo)
{
    m_spIsolateImpl->GetHeapInfo(heapInfo);
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

void V8ContextImpl::Destroy()
{
    m_spIsolateImpl->CallWithLockNoWait([this] (V8IsolateImpl* /*pIsolateImpl*/)
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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), false /*executionStarted*/);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyNames(::HandleFromPtr<v8::Object>(pvObject), names);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

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

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), false /*executionStarted*/);

    FROM_MAYBE_END
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(void* pvObject, std::vector<int>& indices)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyIndices(::HandleFromPtr<v8::Object>(pvObject), indices);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8Object(void* pvObject, const std::vector<V8Value>& args, bool asConstructor)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);
        if (!hObject->IsCallable())
        {
            FROM_MAYBE_TRY

                auto hError = v8::Exception::TypeError(FROM_MAYBE(CreateString(StdString(L"Object does not support invocation")))).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, FROM_MAYBE(CreateString(StdString(L"stack")))))), V8Value(V8Value::Undefined), EXECUTION_STARTED);

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Object does not support invocation"), EXECUTION_STARTED);

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

                auto hError = v8::Exception::TypeError(FROM_MAYBE(CreateString(StdString(L"Method or property not found")))).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, FROM_MAYBE(CreateString(StdString(L"stack")))))), V8Value(V8Value::Undefined), EXECUTION_STARTED);

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Method or property not found"), EXECUTION_STARTED);

            FROM_MAYBE_END
        }

        if (!hMethod->IsCallable())
        {
            FROM_MAYBE_TRY

                auto hError = v8::Exception::TypeError(FROM_MAYBE(CreateString(StdString(L"Property value does not support invocation")))).As<v8::Object>();
                throw V8Exception(V8Exception::Type::General, m_Name, CreateStdString(hError), CreateStdString(FROM_MAYBE(hError->Get(m_hContext, FROM_MAYBE(CreateString(StdString(L"stack")))))), V8Value(V8Value::Undefined), EXECUTION_STARTED);

            FROM_MAYBE_CATCH

                throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Property value does not support invocation"), EXECUTION_STARTED);

            FROM_MAYBE_END
        }

        std::vector<v8::Local<v8::Value>> importedArgs;
        ImportValues(args, importedArgs);

        return ExportValue(VERIFY_MAYBE(hMethod->CallAsFunction(m_hContext, hObject, static_cast<int>(importedArgs.size()), importedArgs.data())));

    FROM_MAYBE_CATCH

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"The V8 runtime cannot perform the requested operation because a script exception is pending"), EXECUTION_STARTED);

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
            auto hArrayBuffer = v8::Local<v8::ArrayBuffer>::Cast(hObject);
            arrayBuffer = ExportValue(hObject);
            offset = 0;
            size = hArrayBuffer->ByteLength();
            length = size;
            return;
        }

        if (hObject->IsDataView())
        {
            auto hDataView = v8::Local<v8::DataView>::Cast(hObject);
            arrayBuffer = ExportValue(hDataView->Buffer());
            offset = hDataView->ByteOffset();
            size = hDataView->ByteLength();
            length = size;
            return;
        }

        if (hObject->IsTypedArray())
        {
            auto hTypedArray = v8::Local<v8::TypedArray>::Cast(hObject);
            arrayBuffer = ExportValue(hTypedArray->Buffer());
            offset = hTypedArray->ByteOffset();
            size = hTypedArray->ByteLength();
            length = hTypedArray->Length();
            return;
        }

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Object is not a V8 array buffer or view"), false /*executionStarted*/);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeWithV8ObjectArrayBufferOrViewData(void* pvObject, V8ObjectHelpers::ArrayBufferOrViewDataCallbackT* pCallback, void* pvArg)
{
    BEGIN_CONTEXT_SCOPE

        v8::Local<v8::Object> hObject = ::HandleFromPtr<v8::Object>(pvObject);

        if (hObject->IsArrayBuffer())
        {
            auto hArrayBuffer = v8::Local<v8::ArrayBuffer>::Cast(hObject);
            (*pCallback)(hArrayBuffer->GetContents().Data(), pvArg);
            return;
        }

        if (hObject->IsDataView())
        {
            auto hDataView = v8::Local<v8::DataView>::Cast(hObject);
            (*pCallback)(static_cast<std::uint8_t*>(hDataView->Buffer()->GetContents().Data()) + hDataView->ByteOffset(), pvArg);
            return;
        }

        if (hObject->IsTypedArray())
        {
            auto hTypedArray = v8::Local<v8::TypedArray>::Cast(hObject);
            (*pCallback)(static_cast<std::uint8_t*>(hTypedArray->Buffer()->GetContents().Data()) + hTypedArray->ByteOffset(), pvArg);
            return;
        }

        throw V8Exception(V8Exception::Type::General, m_Name, StdString(L"Object is not a V8 array buffer or view"), false /*executionStarted*/);

    END_CONTEXT_SCOPE

}

//-----------------------------------------------------------------------------

bool V8ContextImpl::IsHostObject(v8::Local<v8::Object> hObject)
{
    BEGIN_CONTEXT_SCOPE

        return !hObject.IsEmpty() && FROM_MAYBE_DEFAULT(hObject->HasPrivate(m_hContext, GetHostObjectHolderKey()));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Teardown()
{
    _ASSERTE(m_spIsolateImpl->IsCurrent() && m_spIsolateImpl->IsLocked());

    if (m_pvV8ObjectCache != nullptr)
    {
        std::vector<void*> v8ObjectPtrs;
        HostObjectHelpers::GetAllCachedV8Objects(m_pvV8ObjectCache, v8ObjectPtrs);
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

        HostObjectHelpers::Release(m_pvV8ObjectCache);
        m_pvV8ObjectCache = nullptr;
    }

    m_spIsolateImpl->RemoveContext(this);

    for (auto it = m_GlobalMembersStack.rbegin(); it != m_GlobalMembersStack.rend(); it++)
    {
        Dispose(it->second);
    }

    Dispose(m_hHostIteratorTemplate);
    Dispose(m_hHostDelegateTemplate);
    Dispose(m_hHostObjectTemplate);
    Dispose(m_hTerminationException);
    Dispose(m_hInternalUseOnly);
    Dispose(m_hAccessToken);
    Dispose(m_hAccessTokenKey);
    Dispose(m_hCacheKey);
    Dispose(m_hValueKey);
    Dispose(m_hDoneKey);
    Dispose(m_hEnumeratorKey);
    Dispose(m_hHostExceptionKey);
    Dispose(m_hIsHostObjectKey);

    // As of V8 3.16.0, the global property getter for a disposed context
    // may be invoked during GC after the V8ContextImpl instance is gone.

    if (!m_hGlobal.IsEmpty())
    {
        if (m_hGlobal->InternalFieldCount() > 0)
        {
            m_hGlobal->SetAlignedPointerInInternalField(0, nullptr);
        }

        Dispose(m_hGlobal);
    }

    Dispose(m_hContext);
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

    if (pContextImpl->m_DisableHostObjectInterception)
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

void V8ContextImpl::GetV8ObjectPropertyNames(v8::Local<v8::Object> hObject, std::vector<StdString>& names)
{
    names.clear();

    FROM_MAYBE_TRY

        auto hNames = FROM_MAYBE(hObject->GetPropertyNames(m_hContext));
        auto nameCount = static_cast<int>(hNames->Length());

        names.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = FROM_MAYBE(hNames->Get(m_hContext, index));
            auto name = CreateStdString(hName);

            int propertyIndex;
            if (!HostObjectHelpers::TryParseInt32(name, propertyIndex))
            {
                names.push_back(std::move(name));
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(v8::Local<v8::Object> hObject, std::vector<int>& indices)
{
    indices.clear();

    FROM_MAYBE_TRY

        auto hNames = FROM_MAYBE(hObject->GetPropertyNames(m_hContext));
        auto nameCount = static_cast<int>(hNames->Length());

        indices.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = FROM_MAYBE(hNames->Get(m_hContext, index));
            auto name = CreateStdString(hName);

            int propertyIndex;
            if (HostObjectHelpers::TryParseInt32(name, propertyIndex))
            {
                indices.push_back(propertyIndex);
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info)
{
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
            if (stack.size() > 0)
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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetGlobalProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info)
{
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
            if (stack.size() > 0)
            {
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        it->second->Set(pContextImpl->m_hContext, hName, hValue);
                        CALLBACK_RETURN(hValue);
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
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
            if (stack.size() > 0)
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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteGlobalProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
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
            if (stack.size() > 0)
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
                                CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(pvObject, pContextImpl->CreateStdString(hName)));
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
                if (stack.size() > 0)
                {
                    std::vector<StdString> names;
                    for (auto it = stack.rbegin(); it != stack.rend(); it++)
                    {
                        std::vector<StdString> tempNames;

                        auto pvObject = pContextImpl->GetHostObject(it->second);
                        if (pvObject != nullptr)
                        {
                            HostObjectHelpers::GetPropertyNames(pvObject, tempNames);
                        }
                        else
                        {
                            pContextImpl->GetV8ObjectPropertyNames(it->second, tempNames);
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

void V8ContextImpl::GetGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetGlobalProperty(std::uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
            {
                auto hName = FROM_MAYBE(pContextImpl->CreateInteger(index)->ToString(pContextImpl->m_hContext));
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    if (FROM_MAYBE(it->second->HasOwnProperty(pContextImpl->m_hContext, hName)))
                    {
                        it->second->Set(pContextImpl->m_hContext, index, hValue);
                        CALLBACK_RETURN(hValue);
                    }
                }
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteGlobalProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromHolder(info);
        if (CheckContextImplForGlobalObjectCallback(pContextImpl))
        {
            const auto& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
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
                if (stack.size() > 0)
                {
                    std::vector<int> indices;
                    for (auto it = stack.rbegin(); it != stack.rend(); it++)
                    {
                        std::vector<int> tempIndices;

                        auto pvObject = pContextImpl->GetHostObject(it->second);
                        if (pvObject != nullptr)
                        {
                            HostObjectHelpers::GetPropertyIndices(pvObject, tempIndices);
                        }
                        else
                        {
                            pContextImpl->GetV8ObjectPropertyIndices(it->second, tempIndices);
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

void V8ContextImpl::GetIteratorForHostObject(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            auto pvObject = pContextImpl->GetHostObject(info.Holder());
            if (pvObject != nullptr)
            {
                try
                {
                    auto hEnumerator = pContextImpl->ImportValue(HostObjectHelpers::GetEnumerator(pvObject));

                    v8::Local<v8::Object> hIterator;
                    BEGIN_PULSE_VALUE_SCOPE(&pContextImpl->m_AllowHostObjectConstructorCall, true)
                        hIterator = FROM_MAYBE(pContextImpl->m_hHostIteratorTemplate->InstanceTemplate()->NewInstance(pContextImpl->m_hContext));
                    END_PULSE_VALUE_SCOPE

                    ASSERT_EVAL(FROM_MAYBE(hIterator->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hEnumeratorKey, hEnumerator)));
                    CALLBACK_RETURN(hIterator);
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

void V8ContextImpl::AdvanceHostObjectIterator(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            try
            {
                auto hHolder = ::ValueAsObject(info.Holder());
                if (!hHolder.IsEmpty())
                {
                    auto hEnumerator = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hEnumeratorKey)));
                    if (!hEnumerator.IsEmpty())
                    {
                        auto pvObject = pContextImpl->GetHostObject(hEnumerator);
                        if (pvObject != nullptr)
                        {
                            auto hResult = pContextImpl->CreateObject();

                            V8Value value(V8Value::Undefined);
                            if (HostObjectHelpers::AdvanceEnumerator(pvObject, value))
                            {
                                ASSERT_EVAL(FROM_MAYBE(hResult->Set(pContextImpl->m_hContext, pContextImpl->m_hDoneKey, pContextImpl->GetFalse())));
                                ASSERT_EVAL(FROM_MAYBE(hResult->Set(pContextImpl->m_hContext, pContextImpl->m_hValueKey, pContextImpl->ImportValue(value))));
                            }
                            else
                            {
                                ASSERT_EVAL(FROM_MAYBE(hResult->Set(pContextImpl->m_hContext, pContextImpl->m_hDoneKey, pContextImpl->GetTrue())));
                            }

                            CALLBACK_RETURN(hResult);
                        }
                    }
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

void V8ContextImpl::CreateFunctionForHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (pContextImpl != nullptr)
        {
            CALLBACK_RETURN(FROM_MAYBE(v8::Function::New(pContextImpl->m_hContext, InvokeHostDelegate, info.Holder())));
        }

    FROM_MAYBE_CATCH_CONSUME
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeHostDelegate(const v8::FunctionCallbackInfo<v8::Value>& info)
{
    auto hTarget = ::ValueAsObject(info.Data());
    if (!hTarget.IsEmpty())
    {
        auto argCount = info.Length();
        if (argCount < 1)
        {
            if (info.IsConstructCall())
            {
                CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsConstructor(info.GetIsolate()->GetCurrentContext(), 0, nullptr)));
            }

            CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsFunction(info.GetIsolate()->GetCurrentContext(), hTarget, 0, nullptr)));
        }

        std::vector<v8::Local<v8::Value>> args;
        args.reserve(argCount);

        for (auto index = 0; index < argCount; index++)
        {
            args.push_back(info[index]);
        }

        if (info.IsConstructCall())
        {
            CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsConstructor(info.GetIsolate()->GetCurrentContext(), argCount, &args[0])));
        }

        CALLBACK_RETURN(FROM_MAYBE_DEFAULT(hTarget->CallAsFunction(info.GetIsolate()->GetCurrentContext(), hTarget, argCount, &args[0])));
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto hHolder = info.Holder();
            auto pvObject = pContextImpl->GetHostObject(hHolder);

            auto hName = ::ValueAsString(hKey);
            if (hName.IsEmpty())
            {
                if (!hKey.IsEmpty() && hKey->StrictEquals(pContextImpl->m_hIsHostObjectKey))
                {
                    CALLBACK_RETURN((pvObject != nullptr) ? pContextImpl->GetTrue() : pContextImpl->GetFalse());
                }

                return;
            }

            if (pvObject != nullptr)
            {
                try
                {
                    auto cacheCleared = false;

                    auto hAccessToken = FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hAccessTokenKey));
                    if (pContextImpl->m_hAccessToken != hAccessToken)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&pContextImpl->m_DisableHostObjectInterception, true)

                            auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                            if (!hCache.IsEmpty())
                            {
                                auto hNames = FROM_MAYBE(hCache->GetOwnPropertyNames(pContextImpl->m_hContext));
                                for (auto index = hNames->Length(); index > 0; index--)
                                {
                                    ASSERT_EVAL(FROM_MAYBE(hHolder->Delete(pContextImpl->m_hContext, FROM_MAYBE(hNames->Get(pContextImpl->m_hContext, index - 1)))));
                                }

                                ASSERT_EVAL(FROM_MAYBE(hHolder->DeletePrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                            }

                            ASSERT_EVAL(FROM_MAYBE(hHolder->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hAccessTokenKey, pContextImpl->m_hAccessToken)));
                            cacheCleared = true;

                        END_PULSE_VALUE_SCOPE
                    }

                    v8::Local<v8::Value> hResult;
                    if (!cacheCleared)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&pContextImpl->m_DisableHostObjectInterception, true)

                            if (FROM_MAYBE(hHolder->HasOwnProperty(pContextImpl->m_hContext, hName)))
                            {
                                CALLBACK_RETURN(hResult);
                            }

                        END_PULSE_VALUE_SCOPE
                    }

                    bool isCacheable;
                    hResult = pContextImpl->ImportValue(HostObjectHelpers::GetProperty(pvObject, pContextImpl->CreateStdString(hName), isCacheable));
                    if (isCacheable)
                    {
                        BEGIN_PULSE_VALUE_SCOPE(&pContextImpl->m_DisableHostObjectInterception, true)

                            auto hCache = ::ValueAsObject(FROM_MAYBE(hHolder->GetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey)));
                            if (hCache.IsEmpty())
                            {
                                hCache = pContextImpl->CreateObject();
                                ASSERT_EVAL(FROM_MAYBE(hHolder->SetPrivate(pContextImpl->m_hContext, pContextImpl->m_hCacheKey, hCache)));
                            }

                            ASSERT_EVAL(FROM_MAYBE(hCache->Set(pContextImpl->m_hContext, hName, hResult)));
                            ASSERT_EVAL(FROM_MAYBE(hHolder->DefineOwnProperty(pContextImpl->m_hContext, hName, hResult, v8::DontEnum)));

                        END_PULSE_VALUE_SCOPE
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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetHostObjectProperty(v8::Local<v8::Name> hKey, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                HostObjectHelpers::SetProperty(pvObject, pContextImpl->CreateStdString(hName), pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN(hValue);
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                std::vector<StdString> names;
                HostObjectHelpers::GetPropertyNames(pvObject, names);

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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteHostObjectProperty(v8::Local<v8::Name> hKey, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    auto hName = ::ValueAsString(hKey);
    if (hName.IsEmpty())
    {
        return;
    }

    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(pvObject, pContextImpl->CreateStdString(hName)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyNames(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.Holder());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<StdString> names;
                    HostObjectHelpers::GetPropertyNames(pvObject, names);
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

void V8ContextImpl::GetHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::GetProperty(pvObject, index)));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetHostObjectProperty(std::uint32_t index, v8::Local<v8::Value> hValue, const v8::PropertyCallbackInfo<v8::Value>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                HostObjectHelpers::SetProperty(pvObject, index, pContextImpl->ExportValue(hValue));
                CALLBACK_RETURN(hValue);
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Integer>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                std::vector<int> indices;
                HostObjectHelpers::GetPropertyIndices(pvObject, indices);

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
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteHostObjectProperty(std::uint32_t index, const v8::PropertyCallbackInfo<v8::Boolean>& info)
{
    auto pContextImpl = ::GetContextImplFromData(info);
    if (CheckContextImplForHostObjectCallback(pContextImpl))
    {
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(pvObject, index));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyIndices(const v8::PropertyCallbackInfo<v8::Array>& info)
{
    FROM_MAYBE_TRY

        auto pContextImpl = ::GetContextImplFromData(info);
        if (CheckContextImplForHostObjectCallback(pContextImpl))
        {
            auto pvObject = pContextImpl->GetHostObject(info.Holder());
            if (pvObject != nullptr)
            {
                try
                {
                    std::vector<int> indices;
                    HostObjectHelpers::GetPropertyIndices(pvObject, indices);
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
        auto pvObject = pContextImpl->GetHostObject(info.Holder());
        if (pvObject != nullptr)
        {
            try
            {
                auto argCount = info.Length();

                std::vector<V8Value> exportedArgs;
                exportedArgs.reserve(argCount);

                for (auto index = 0; index < argCount; index++)
                {
                    exportedArgs.push_back(pContextImpl->ExportValue(info[index]));
                }

                CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::Invoke(pvObject, exportedArgs, info.IsConstructCall())));
            }
            catch (const HostException& exception)
            {
                pContextImpl->ThrowScriptException(exception);
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisposeWeakHandle(v8::Isolate* pIsolate, Persistent<v8::Object>* phObject, HostObjectHolder* pHolder, void* pvV8ObjectCache)
{
    IGNORE_UNUSED(pIsolate);

    ASSERT_EVAL(HostObjectHelpers::RemoveV8ObjectCacheEntry(pvV8ObjectCache, pHolder->GetObject()));
    delete pHolder;

    phObject->Dispose();
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
            std::int32_t result;
            if (value.AsInt32(result))
            {
                return CreateInteger(result);
            }
        }

        {
            std::uint32_t result;
            if (value.AsUInt32(result))
            {
                return CreateInteger(result);
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
            if (value.AsHostObject(pHolder))
            {
                auto pvV8Object = HostObjectHelpers::GetCachedV8Object(m_pvV8ObjectCache, pHolder->GetObject());
                if (pvV8Object != nullptr)
                {
                    return CreateLocal(::HandleFromPtr<v8::Object>(pvV8Object));
                }

                v8::Local<v8::Object> hObject;
                if (HostObjectHelpers::IsDelegate(pHolder->GetObject()))
                {
                    BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                        hObject = FROM_MAYBE(m_hHostDelegateTemplate->InstanceTemplate()->NewInstance(m_hContext));
                    END_PULSE_VALUE_SCOPE
                }
                else
                {
                    BEGIN_PULSE_VALUE_SCOPE(&m_AllowHostObjectConstructorCall, true)
                        hObject = FROM_MAYBE(m_hHostObjectTemplate->InstanceTemplate()->NewInstance(m_hContext));
                    END_PULSE_VALUE_SCOPE
                }

                ASSERT_EVAL(SetHostObjectHolder(hObject, pHolder = pHolder->Clone()));
                ASSERT_EVAL(FROM_MAYBE(hObject->SetPrivate(m_hContext, m_hAccessTokenKey, m_hAccessToken)));
                pvV8Object = ::PtrFromHandle(MakeWeak(CreatePersistent(hObject), pHolder, m_pvV8ObjectCache, DisposeWeakHandle));
                HostObjectHelpers::CacheV8Object(m_pvV8ObjectCache, pHolder->GetObject(), pvV8Object);

                return hObject;
            }
        }

        {
            V8ObjectHolder* pHolder;
            V8Value::Subtype subtype;
            if (value.AsV8Object(pHolder, subtype))
            {
                return CreateLocal(::HandleFromPtr<v8::Object>(pHolder->GetObject()));
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

        if (hValue->IsBoolean())
        {
            return V8Value(FROM_MAYBE(hValue->BooleanValue(m_hContext)));
        }

        if (hValue->IsNumber())
        {
            return V8Value(FROM_MAYBE(hValue->NumberValue(m_hContext)));
        }

        if (hValue->IsInt32())
        {
            return V8Value(FROM_MAYBE(hValue->Int32Value(m_hContext)));
        }

        if (hValue->IsUint32())
        {
            return V8Value(FROM_MAYBE(hValue->Uint32Value(m_hContext)));
        }

        if (hValue->IsString())
        {
            return V8Value(new StdString(CreateStdString(hValue)));
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
            if (hObject->IsArrayBuffer())
                subtype = V8Value::Subtype::ArrayBuffer;
            else if (hObject->IsArrayBufferView())
                if (hObject->IsDataView())
                    subtype = V8Value::Subtype::DataView;
                else if (hObject->IsTypedArray())
                    if (hObject->IsUint8Array())
                        subtype = V8Value::Subtype::Uint8Array;
                    else if (hObject->IsUint8ClampedArray())
                        subtype = V8Value::Subtype::Uint8ClampedArray;
                    else if (hObject->IsInt8Array())
                        subtype = V8Value::Subtype::Int8Array;
                    else if (hObject->IsUint16Array())
                        subtype = V8Value::Subtype::Uint16Array;
                    else if (hObject->IsInt16Array())
                        subtype = V8Value::Subtype::Int16Array;
                    else if (hObject->IsUint32Array())
                        subtype = V8Value::Subtype::Uint32Array;
                    else if (hObject->IsInt32Array())
                        subtype = V8Value::Subtype::Int32Array;
                    else if (hObject->IsFloat32Array())
                        subtype = V8Value::Subtype::Float32Array;
                    else if (hObject->IsFloat64Array())
                        subtype = V8Value::Subtype::Float64Array;

            return V8Value(new V8ObjectHolderImpl(GetWeakBinding(), ::PtrFromHandle(CreatePersistent(hObject))), subtype);
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

void V8ContextImpl::Verify(const V8IsolateImpl::ExecutionScope& isolateExecutionScope, const v8::TryCatch& tryCatch)
{
    if (tryCatch.HasCaught())
    {
        if (!tryCatch.CanContinue())
        {
            VerifyNotOutOfMemory();
            throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(L"Script execution interrupted by host"), CreateStdString(FROM_MAYBE_DEFAULT(tryCatch.StackTrace(m_hContext))), V8Value(V8Value::Undefined), isolateExecutionScope.ExecutionStarted());
        }

        auto hException = tryCatch.Exception();
        if (hException->SameValue(m_hTerminationException))
        {
            VerifyNotOutOfMemory();
            throw V8Exception(V8Exception::Type::Interrupt, m_Name, StdString(L"Script execution interrupted by host"), CreateStdString(FROM_MAYBE_DEFAULT(tryCatch.StackTrace(m_hContext))), V8Value(V8Value::Undefined), isolateExecutionScope.ExecutionStarted());
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
            stackOverflow = (_wcsicmp(message.ToCString(), L"RangeError: Maximum call stack size exceeded") == 0);
        }
        else if (!hException->IsObject())
        {
            message = L"Unknown error; an unrecognized value was thrown and not caught";
            stackOverflow = false;
        }
        else
        {
            if ((constructorName == L"Error") || (constructorName == L"RangeError"))
            {
                // It is unclear why V8 sometimes throws Error or RangeError objects that convert
                // to empty strings, but it probably has to do with memory pressure. It seems to
                // happen only during stack overflow recovery.

                message = L"Unknown error (";
                message += constructorName;
                message += L"); potential stack overflow detected";
                stackOverflow = true;
            }
            else if (constructorName.GetLength() > 0)
            {
                message = L"Unknown error (";
                message += constructorName;
                message += L")";
                stackOverflow = false;
            }
            else
            {
                message = L"Unknown error; an unrecognized object was thrown and not caught";
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

                if ((frameCount < 1) || (constructorName == L"SyntaxError"))
                {
                    auto hScriptResourceName = hMessage->GetScriptResourceName();
                    if (!hScriptResourceName.IsEmpty() && !hScriptResourceName->IsNull() && !hScriptResourceName->IsUndefined())
                    {
                        auto hScriptName = FROM_MAYBE_DEFAULT(hScriptResourceName->ToString(m_hContext));
                        if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                        {
                            stackTrace += L"\n    at ";
                            stackTrace += CreateStdString(hScriptName);
                        }
                        else
                        {
                            stackTrace += L"\n    at <anonymous>";
                        }
                    }
                    else
                    {
                        stackTrace += L"\n    at <anonymous>";
                    }

                    stackTrace += L':';
                    stackTrace += std::to_wstring(FROM_MAYBE_DEFAULT(hMessage->GetLineNumber(m_hContext)));
                    stackTrace += L':';
                    stackTrace += std::to_wstring(FROM_MAYBE_DEFAULT(hMessage->GetStartColumn(m_hContext)) + 1);

                    auto hSourceLine = FROM_MAYBE_DEFAULT(hMessage->GetSourceLine(m_hContext));
                    if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                    {
                        stackTrace += L" -> ";
                        stackTrace += CreateStdString(hSourceLine);
                    }

                    usedSourceLine = true;
                }

                for (int index = 0; index < frameCount; index++)
                {
                    auto hFrame = hMessageStackTrace->GetFrame(index);
                    stackTrace += L"\n    at ";

                    auto hFunctionName = hFrame->GetFunctionName();
                    if (!hFunctionName.IsEmpty() && (hFunctionName->Length() > 0))
                    {
                        if (hFrame->IsConstructor())
                        {
                            stackTrace += L"new ";
                        }

                        stackTrace += CreateStdString(hFunctionName);
                        stackTrace += L" (";
                    }

                    auto hScriptName = hFrame->GetScriptName();
                    if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                    {
                        stackTrace += CreateStdString(hScriptName);
                    }
                    else
                    {
                        stackTrace += L"<anonymous>";
                    }

                    stackTrace += L':';
                    auto lineNumber = hFrame->GetLineNumber();
                    if (lineNumber != v8::Message::kNoLineNumberInfo)
                    {
                        stackTrace += std::to_wstring(lineNumber);
                    }

                    stackTrace += L':';
                    auto column = hFrame->GetColumn();
                    if (column != v8::Message::kNoColumnInfo)
                    {
                        stackTrace += std::to_wstring(column);
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
                            stackTrace += L" -> ";
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

        throw V8Exception(V8Exception::Type::General, m_Name, std::move(message), std::move(stackTrace), std::move(hostException), isolateExecutionScope.ExecutionStarted());
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
                auto hHostException = ImportValue(exception.GetException());
                if (!hHostException.IsEmpty() && hHostException->IsObject())
                {
                    ASSERT_EVAL(FROM_MAYBE(hException->Set(m_hContext, m_hHostExceptionKey, hHostException)));
                }

                ThrowException(hException);
            }
        }

    FROM_MAYBE_CATCH_CONSUME
}
