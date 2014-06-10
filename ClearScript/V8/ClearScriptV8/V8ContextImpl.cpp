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

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

static HostObjectHolder* GetHostObjectHolder(Handle<Object> hObject)
{
    _ASSERTE(hObject->InternalFieldCount() > 0);
    return static_cast<HostObjectHolder*>(hObject->GetAlignedPointerFromInternalField(0));
}

//-----------------------------------------------------------------------------

static void SetHostObjectHolder(Handle<Object> hObject, HostObjectHolder* pHolder)
{
    _ASSERTE(hObject->InternalFieldCount() > 0);
    hObject->SetAlignedPointerInInternalField(0, pHolder);
}

//-----------------------------------------------------------------------------

static void* GetHostObject(Handle<Object> hObject)
{
    auto pHolder = ::GetHostObjectHolder(hObject);
    return (pHolder != nullptr) ? pHolder->GetObject() : nullptr;
}

//-----------------------------------------------------------------------------

template <typename T>
static V8ContextImpl* UnwrapContextImpl(const PropertyCallbackInfo<T>& info)
{
    return static_cast<V8ContextImpl*>(Local<External>::Cast(info.Data())->Value());
}

//-----------------------------------------------------------------------------

template <typename T>
static V8ContextImpl* UnwrapContextImpl(const FunctionCallbackInfo<T>& info)
{
    return static_cast<V8ContextImpl*>(Local<External>::Cast(info.Data())->Value());
}

//-----------------------------------------------------------------------------

template <typename T>
static void* UnwrapHostObject(const PropertyCallbackInfo<T>& info)
{
    return ::GetHostObject(info.Holder());
}

//-----------------------------------------------------------------------------

template <typename T>
static void* UnwrapHostObject(const FunctionCallbackInfo<T>& args)
{
    return ::GetHostObject(args.Holder());
}

//-----------------------------------------------------------------------------
// V8ContextImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_SCOPE \
    { \
        V8IsolateImpl::Scope t_IsolateScope(m_spIsolateImpl);

#define END_ISOLATE_SCOPE \
        IGNORE_UNUSED(t_IsolateScope); \
    }

#define BEGIN_CONTEXT_SCOPE \
    { \
        Scope t_ContextScope(this);

#define END_CONTEXT_SCOPE \
        IGNORE_UNUSED(t_ContextScope); \
    }

#define BEGIN_EXECUTION_SCOPE \
    { \
        V8IsolateImpl::ExecutionScope t_IsolateExecutionScope(m_spIsolateImpl); \
        TryCatch t_TryCatch;

#define END_EXECUTION_SCOPE \
        IGNORE_UNUSED(t_TryCatch); \
        IGNORE_UNUSED(t_IsolateExecutionScope); \
    }

#define VERIFY(RESULT) \
    Verify(t_TryCatch, RESULT)

#define VERIFY_CHECKPOINT() \
    Verify(t_TryCatch)

#define CALLBACK_RETURN(RESULT) \
    BEGIN_COMPOUND_MACRO \
        info.GetReturnValue().Set(RESULT); \
        return; \
    END_COMPOUND_MACRO

//-----------------------------------------------------------------------------

V8ContextImpl::V8ContextImpl(V8IsolateImpl* pIsolateImpl, const StdString& name, bool enableDebugging, bool disableGlobalMembers, int debugPort):
    m_Name(name),
    m_spIsolateImpl(pIsolateImpl)
{
    VerifyNotOutOfMemory();

    BEGIN_ISOLATE_SCOPE

        if (disableGlobalMembers)
        {
            m_hContext = CreatePersistent(CreateContext());
        }
        else
        {
            auto hGlobalTemplate = CreateObjectTemplate();
            hGlobalTemplate->SetInternalFieldCount(1);
            hGlobalTemplate->SetNamedPropertyHandler(GetGlobalProperty, SetGlobalProperty, QueryGlobalProperty, DeleteGlobalProperty, GetGlobalPropertyNames);
            hGlobalTemplate->SetIndexedPropertyHandler(GetGlobalProperty, SetGlobalProperty, QueryGlobalProperty, DeleteGlobalProperty, GetGlobalPropertyIndices);

            m_hContext = CreatePersistent(CreateContext(nullptr, hGlobalTemplate));

            m_hGlobal = CreatePersistent(m_hContext->Global()->GetPrototype()->ToObject());
            _ASSERTE(m_hGlobal->InternalFieldCount() > 0);
            m_hGlobal->SetAlignedPointerInInternalField(0, this);
        }

        // Be careful when renaming the cookie or changing the way host objects are marked.
        // Such a change will require a corresponding change in the V8ScriptEngine constructor.

        m_hHostObjectCookieName = CreatePersistent(CreateString(StdString(L"{c2cf47d3-916b-4a3f-be2a-6ff567425808}")));
        m_hInnerExceptionName = CreatePersistent(CreateString(StdString(L"inner")));

        m_hHostObjectTemplate = CreatePersistent(CreateFunctionTemplate());
        m_hHostObjectTemplate->SetClassName(CreateString(StdString(L"HostObject")));

        m_hHostObjectTemplate->InstanceTemplate()->SetInternalFieldCount(1);
        m_hHostObjectTemplate->InstanceTemplate()->SetNamedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, Wrap());
        m_hHostObjectTemplate->InstanceTemplate()->SetIndexedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, Wrap());
        m_hHostObjectTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, Wrap());

        m_spIsolateImpl->AddContext(this, enableDebugging, debugPort);
        m_pvV8ObjectCache = HostObjectHelpers::CreateV8ObjectCache();

    END_ISOLATE_SCOPE
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

        auto hValue = ImportValue(value);
        m_hContext->Global()->ForceSet(CreateString(name), hValue, (PropertyAttribute)(ReadOnly | DontDelete));
        if (globalMembers && hValue->IsObject())
        {
            m_GlobalMembersStack.push_back(CreatePersistent(hValue->ToObject()));
        }

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(const StdString& documentName, const StdString& code, bool evaluate, bool /* discard */)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = VERIFY(Script::Compile(CreateString(code), CreateString(documentName)));
        auto hResult = VERIFY(hScript->Run());
        if (!evaluate)
        {
            hResult = GetUndefined();
        }

        return ExportValue(hResult);

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const StdString& documentName, const StdString& code)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = VERIFY(Script::New(CreateString(code), CreateString(documentName)));
        return new V8ScriptHolderImpl(GetWeakBinding(), ::PtrFromScriptHandle(CreatePersistent(hScript)));

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

        auto hScript = ::ScriptHandleFromPtr(pHolder->GetScript());
        auto hResult = VERIFY(hScript->Run());
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

V8Value V8ContextImpl::GetV8ObjectProperty(void* pvObject, const StdString& name)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        return ExportValue(::ObjectHandleFromPtr(pvObject)->Get(CreateString(name)));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        ::ObjectHandleFromPtr(pvObject)->Set(CreateString(name), ImportValue(value));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, const StdString& name)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvObject)->Delete(CreateString(name));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyNames(::ObjectHandleFromPtr(pvObject), names);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(void* pvObject, int index)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        return ExportValue(::ObjectHandleFromPtr(pvObject)->Get(index));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, int index, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        ::ObjectHandleFromPtr(pvObject)->Set(index, ImportValue(value));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, int index)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvObject)->Delete(index);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(void* pvObject, std::vector<int>& indices)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyIndices(::ObjectHandleFromPtr(pvObject), indices);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8Object(void* pvObject, const std::vector<V8Value>& args, bool asConstructor)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        std::vector<Handle<Value>> importedArgs;
        ImportValues(args, importedArgs);

        if (asConstructor)
        {
            return ExportValue(VERIFY(hObject->CallAsConstructor(static_cast<int>(importedArgs.size()), importedArgs.data())));
        }

        return ExportValue(VERIFY(hObject->CallAsFunction(hObject, static_cast<int>(importedArgs.size()), importedArgs.data())));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8ObjectMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        auto hName = CreateString(name);
        if (!hObject->Has(hName))
        {
            auto hError = Exception::TypeError(CreateString(StdString(L"Method or property not found")))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name, StdString(hError), StdString(hError->Get(CreateString(StdString(L"stack")))), V8Value(V8Value::Undefined));
        }

        auto hValue = hObject->Get(hName);
        if (hValue->IsUndefined() || hValue->IsNull())
        {
            auto hError = Exception::TypeError(CreateString(StdString(L"Property value does not support invocation")))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name, StdString(hError), StdString(hError->Get(CreateString(StdString(L"stack")))), V8Value(V8Value::Undefined));
        }

        std::vector<Handle<Value>> importedArgs;
        ImportValues(args, importedArgs);

        auto hMethod = VERIFY(hValue->ToObject());
        return ExportValue(VERIFY(hMethod->CallAsFunction(hObject, static_cast<int>(importedArgs.size()), importedArgs.data())));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ProcessDebugMessages()
{
    BEGIN_CONTEXT_SCOPE

        Debug::ProcessDebugMessages();

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ContextImpl::~V8ContextImpl()
{
    BEGIN_ISOLATE_SCOPE

        std::vector<void*> v8ObjectPtrs;
        HostObjectHelpers::GetAllCachedV8Objects(m_pvV8ObjectCache, v8ObjectPtrs);
        for (auto pvV8Object: v8ObjectPtrs)
        {
            auto hObject = ::ObjectHandleFromPtr(pvV8Object);
            delete ::GetHostObjectHolder(hObject);
            Dispose(hObject);
        }

        HostObjectHelpers::Release(m_pvV8ObjectCache);
        m_spIsolateImpl->RemoveContext(this);

        for (auto it = m_GlobalMembersStack.rbegin(); it != m_GlobalMembersStack.rend(); it++)
        {
            Dispose(*it);
        }

        Dispose(m_hHostObjectTemplate);
        Dispose(m_hInnerExceptionName);
        Dispose(m_hHostObjectCookieName);

        // As of V8 3.16.0, the global property getter for a disposed context
        // may be invoked during GC after the V8ContextImpl instance is gone.

        if (!m_hGlobal.IsEmpty())
        {
            _ASSERTE(m_hGlobal->InternalFieldCount() > 0);
            m_hGlobal->SetAlignedPointerInInternalField(0, nullptr);
            Dispose(m_hGlobal);
        }

        Dispose(m_hContext);
        V8::ContextDisposedNotification();

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::Wrap()
{
    return CreateExternal(this);
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

void V8ContextImpl::GetV8ObjectPropertyNames(Handle<Object> hObject, std::vector<StdString>& names)
{
    names.clear();

    auto hNames = hObject->GetPropertyNames();
    if (!hNames.IsEmpty())
    {
        auto nameCount = static_cast<int>(hNames->Length());

        names.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = hNames->Get(index);
            if (!hName.IsEmpty())
            {
                StdString name(hName);

                int propertyIndex;
                if (!HostObjectHelpers::TryParseInt32(name, propertyIndex))
                {
                    names.push_back(std::move(name));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(Handle<Object> hObject, std::vector<int>& indices)
{
    indices.clear();

    auto hNames = hObject->GetPropertyNames();
    if (!hNames.IsEmpty())
    {
        auto nameCount = static_cast<int>(hNames->Length());

        indices.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = hNames->Get(index);
            if (!hName.IsEmpty())
            {
                StdString name(hName);

                int propertyIndex;
                if (HostObjectHelpers::TryParseInt32(name, propertyIndex))
                {
                    indices.push_back(propertyIndex);
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Value>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if ((stack.size() > 0) && !hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->Has(hName))
                {
                    CALLBACK_RETURN((*it)->Get(hName));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetGlobalProperty(Local<String> hName, Local<Value> value, const PropertyCallbackInfo<Value>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if ((stack.size() > 0) && !hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->HasOwnProperty(hName) && (*it)->Set(hName, value))
                {
                    CALLBACK_RETURN(value);
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Integer>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if ((stack.size() > 0) && !hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->Has(hName))
                {
                    CALLBACK_RETURN((*it)->GetPropertyAttributes(hName));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteGlobalProperty(Local<String> hName, const PropertyCallbackInfo<Boolean>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->HasOwnProperty(hName))
                {
                    // WORKAROUND: Object::Delete() crashes if a custom property deleter calls
                    // ThrowException(). Interestingly, there is no crash if the same deleter is
                    // invoked directly from script via the delete operator.

                    if ((*it)->HasOwnProperty(pContextImpl->m_hHostObjectCookieName))
                    {
                        try
                        {
                            CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(::GetHostObject(*it), StdString(hName)));
                        }
                        catch (const HostException&)
                        {
                            CALLBACK_RETURN(false);
                        }
                    }

                    CALLBACK_RETURN((*it)->Delete(hName));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalPropertyNames(const PropertyCallbackInfo<Array>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        try
        {
            const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
            {
                std::vector<StdString> names;
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    std::vector<StdString> tempNames;
                    if ((*it)->HasOwnProperty(pContextImpl->m_hHostObjectCookieName))
                    {
                        HostObjectHelpers::GetPropertyNames(::GetHostObject(*it), tempNames);
                    }
                    else
                    {
                        pContextImpl->GetV8ObjectPropertyNames(*it, tempNames);
                    }

                    names.insert(names.end(), tempNames.begin(), tempNames.end());
                }

                sort(names.begin(), names.end());
                auto newEnd = unique(names.begin(), names.end());
                auto nameCount = static_cast<int>(newEnd - names.begin());

                auto hImportedNames = pContextImpl->CreateArray(nameCount);
                for (auto index = 0; index < nameCount; index++)
                {
                    hImportedNames->Set(index, pContextImpl->CreateString(names[index]));
                }

                CALLBACK_RETURN(hImportedNames);
            }
        }
        catch (const HostException& exception)
        {
            pContextImpl->ThrowScriptException(exception);
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Value>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->Has(index))
                {
                    CALLBACK_RETURN((*it)->Get(index));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetGlobalProperty(unsigned __int32 index, Local<Value> value, const PropertyCallbackInfo<Value>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            auto hName = pContextImpl->CreateInteger(index)->ToString();
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->HasOwnProperty(hName) && (*it)->Set(index, value))
                {
                    CALLBACK_RETURN(value);
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Integer>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->Has(index))
                {
                    CALLBACK_RETURN((*it)->GetPropertyAttributes(pContextImpl->CreateInteger(index)));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteGlobalProperty(unsigned __int32 index, const PropertyCallbackInfo<Boolean>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            auto hName = pContextImpl->CreateInteger(index)->ToString();
            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->HasOwnProperty(hName))
                {
                    CALLBACK_RETURN((*it)->Delete(index));
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetGlobalPropertyIndices(const PropertyCallbackInfo<Array>& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = static_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        try
        {
            const std::vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
            {
                std::vector<int> indices;
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    std::vector<int> tempIndices;
                    if ((*it)->HasOwnProperty(pContextImpl->m_hHostObjectCookieName))
                    {
                        HostObjectHelpers::GetPropertyIndices(::GetHostObject(*it), tempIndices);
                    }
                    else
                    {
                        pContextImpl->GetV8ObjectPropertyIndices(*it, tempIndices);
                    }

                    indices.insert(indices.end(), tempIndices.begin(), tempIndices.end());
                }

                sort(indices.begin(), indices.end());
                auto newEnd = unique(indices.begin(), indices.end());
                auto indexCount = static_cast<int>(newEnd - indices.begin());

                auto hImportedIndices = pContextImpl->CreateArray(indexCount);
                for (auto index = 0; index < indexCount; index++)
                {
                    hImportedIndices->Set(index, pContextImpl->CreateInteger(indices[index]));
                }

                CALLBACK_RETURN(hImportedIndices);
            }
        }
        catch (const HostException& exception)
        {
            pContextImpl->ThrowScriptException(exception);
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Value>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            CALLBACK_RETURN(true);
        }

        CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), StdString(hName))));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetHostObjectProperty(Local<String> hName, Local<Value> hValue, const PropertyCallbackInfo<Value>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), StdString(hName), pContextImpl->ExportValue(hValue));
        CALLBACK_RETURN(hValue);
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Integer>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            CALLBACK_RETURN(ReadOnly | DontEnum | DontDelete);
        }

        std::vector<StdString> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);

        StdString name(hName);
        for (auto it = names.begin(); it != names.end(); it++)
        {
            if (it->Compare(name) == 0)
            {
                CALLBACK_RETURN(None);
            }
        }
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteHostObjectProperty(Local<String> hName, const PropertyCallbackInfo<Boolean>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), StdString(hName)));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyNames(const PropertyCallbackInfo<Array>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        std::vector<StdString> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);
        auto nameCount = static_cast<int>(names.size());

        auto hImportedNames = pContextImpl->CreateArray(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            hImportedNames->Set(index, pContextImpl->CreateString(names[index]));
        }

        CALLBACK_RETURN(hImportedNames);
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Value>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), index)));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetHostObjectProperty(unsigned __int32 index, Local<Value> hValue, const PropertyCallbackInfo<Value>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), index, pContextImpl->ExportValue(hValue));
        CALLBACK_RETURN(hValue);
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::QueryHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Integer>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        std::vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);

        for (auto it = indices.begin(); it < indices.end(); it++)
        {
            if (*it == static_cast<int>(index))
            {
                CALLBACK_RETURN(None);
            }
        }
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DeleteHostObjectProperty(unsigned __int32 index, const PropertyCallbackInfo<Boolean>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), index));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetHostObjectPropertyIndices(const PropertyCallbackInfo<Array>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        std::vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);
        auto indexCount = static_cast<int>(indices.size());

        auto hImportedIndices = pContextImpl->CreateArray(indexCount);
        for (auto index = 0; index < indexCount; index++)
        {
            hImportedIndices->Set(index, pContextImpl->CreateInteger(indices[index]));
        }

        CALLBACK_RETURN(hImportedIndices);
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::InvokeHostObject(const FunctionCallbackInfo<Value>& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        auto argCount = info.Length();

        std::vector<V8Value> exportedArgs;
        exportedArgs.reserve(argCount);

        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs.push_back(pContextImpl->ExportValue(info[index]));
        }

        CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::Invoke(::UnwrapHostObject(info), exportedArgs, info.IsConstructCall())));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisposeWeakHandle(Isolate* pIsolate, Persistent<Object>* phObject, void* pvV8ObjectCache)
{
    IGNORE_UNUSED(pIsolate);

    auto pHolder = ::GetHostObjectHolder(*phObject);
    ASSERT_EVAL(HostObjectHelpers::RemoveV8ObjectCacheEntry(pvV8ObjectCache, pHolder->GetObject()));

    delete pHolder;
    phObject->Dispose();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::ImportValue(const V8Value& value)
{
    if (value.IsNonexistent())
    {
        return Handle<Value>();
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
        __int32 result;
        if (value.AsInt32(result))
        {
            return CreateInteger(result);
        }
    }

    {
        unsigned __int32 result;
        if (value.AsUInt32(result))
        {
            return CreateInteger(result);
        }
    }

    {
        const StdString* pString;
        if (value.AsString(pString))
        {
            return CreateString(*pString);
        }
    }

    {
        HostObjectHolder* pHolder;
        if (value.AsHostObject(pHolder))
        {
            auto pvV8Object = HostObjectHelpers::GetCachedV8Object(m_pvV8ObjectCache, pHolder->GetObject());
            if (pvV8Object != nullptr)
            {
                return CreateLocal(::ObjectHandleFromPtr(pvV8Object));
            }

            // WARNING: Instantiation may fail during script interruption. Check NewInstance()
            // result to avoid access violations and V8 fatal errors in ::SetObjectHolder().

            auto hObject = m_hHostObjectTemplate->InstanceTemplate()->NewInstance();
            if (!hObject.IsEmpty())
            {
                ::SetHostObjectHolder(hObject, pHolder = pHolder->Clone());
                pvV8Object = ::PtrFromObjectHandle(MakeWeak(CreatePersistent(hObject), m_pvV8ObjectCache, DisposeWeakHandle));
                HostObjectHelpers::CacheV8Object(m_pvV8ObjectCache, pHolder->GetObject(), pvV8Object);
            }

            return hObject;
        }
    }

    {
        V8ObjectHolder* pHolder;
        if (value.AsV8Object(pHolder))
        {
            return CreateLocal(::ObjectHandleFromPtr(pHolder->GetObject()));
        }
    }

    return GetUndefined();
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::ExportValue(Handle<Value> hValue)
{
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
        return V8Value(hValue->BooleanValue());
    }

    if (hValue->IsNumber())
    {
        return V8Value(hValue->NumberValue());
    }

    if (hValue->IsInt32())
    {
        return V8Value(hValue->Int32Value());
    }

    if (hValue->IsUint32())
    {
        return V8Value(hValue->Uint32Value());
    }

    if (hValue->IsString())
    {
        return V8Value(new StdString(hValue));
    }

    if (hValue->IsObject())
    {
        auto hObject = hValue->ToObject();
        if (hObject->HasOwnProperty(m_hHostObjectCookieName))
        {
            return V8Value(::GetHostObjectHolder(hObject)->Clone());
        }

        return V8Value(new V8ObjectHolderImpl(GetWeakBinding(), ::PtrFromObjectHandle(CreatePersistent(hObject))));
    }

    return V8Value(V8Value::Undefined);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ImportValues(const std::vector<V8Value>& values, std::vector<Handle<Value>>& importedValues)
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

void V8ContextImpl::Verify(const TryCatch& tryCatch)
{
    if (tryCatch.HasCaught())
    {
        if (!tryCatch.CanContinue())
        {
            throw V8Exception(V8Exception::Type_Interrupt, m_Name, StdString(L"Script execution interrupted by host"), StdString(tryCatch.StackTrace()), V8Value(V8Value::Undefined));
        }

        auto hException = tryCatch.Exception();

        StdString message;
        bool stackOverflow;

        StdString value(hException);
        if (value.GetLength() > 0)
        {
            message = std::move(value);
            stackOverflow = (_wcsicmp(message.ToCString(), L"RangeError: Maximum call stack size exceeded") == 0);
        }
        else
        {
            // It is unclear why V8 sometimes generates blank exceptions, although it probably has
            // to do with memory pressure. It seems to happen only during stack overflow recovery.

            message = L"Unknown error; potential stack overflow detected";
            stackOverflow = true;
        }

    #ifdef _DEBUG

        if (stackOverflow)
        {
            // Stack overflow conditions require extreme care, as V8's behavior can be erratic
            // until the stack is unwound a bit. Much of the code below can trigger unexpected
            // fatal errors in this context, so it makes sense to bypass it. On the other hand,
            // losing error information is also undesirable, and the detection code above is far
            // from robust. These sanity checks are intended to mitigate this fragility.

            _ASSERTE(hException->IsObject());
            _ASSERTE(StdString(hException->ToObject()->GetConstructorName()) == L"RangeError");
        }

    #endif // _DEBUG

        StdString stackTrace;
        V8Value innerException(V8Value::Undefined);

        if (stackOverflow)
        {
            stackTrace = message;
        }
        else
        {
            auto hStackTrace = tryCatch.StackTrace();
            if (!hStackTrace.IsEmpty())
            {
                stackTrace = StdString(hStackTrace);
            }

            auto hMessage = tryCatch.Message();
            if (!hMessage.IsEmpty())
            {
                if (message.GetLength() < 1)
                {
                    message = StdString(hMessage->Get());
                }

                stackTrace = message;

                auto hStackTrace = hMessage->GetStackTrace();
                int frameCount = !hStackTrace.IsEmpty() ? hStackTrace->GetFrameCount() : 0;

                if (frameCount < 1)
                {
                    auto hScriptResourceName = hMessage->GetScriptResourceName();
                    if (!hScriptResourceName.IsEmpty() && !hScriptResourceName->IsNull() && !hScriptResourceName->IsUndefined())
                    {
                        auto hScriptName = hScriptResourceName->ToString();
                        if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                        {
                            stackTrace += L"\n    at ";
                            stackTrace += StdString(hScriptName);
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
                    stackTrace += std::to_wstring(hMessage->GetLineNumber());
                    stackTrace += L':';
                    stackTrace += std::to_wstring(hMessage->GetStartColumn() + 1);

                    auto hSourceLine = hMessage->GetSourceLine();
                    if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                    {
                        stackTrace += L" -> ";
                        stackTrace += StdString(hSourceLine);
                    }
                }
                else
                {
                    for (int index = 0; index < frameCount; index++)
                    {
                        auto hFrame = hStackTrace->GetFrame(index);
                        stackTrace += L"\n    at ";

                        auto hFunctionName = hFrame->GetFunctionName();
                        if (!hFunctionName.IsEmpty() && (hFunctionName->Length() > 0))
                        {
                            if (hFrame->IsConstructor())
                            {
                                stackTrace += L"new ";
                            }

                            stackTrace += StdString(hFunctionName);
                            stackTrace += L" (";
                        }

                        auto hScriptName = hFrame->GetScriptName();
                        if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                        {
                            stackTrace += StdString(hScriptName);
                        }
                        else
                        {
                            stackTrace += L"<anonymous>";
                        }

                        stackTrace += L':';
                        auto lineNumber = hFrame->GetLineNumber();
                        if (lineNumber != Message::kNoLineNumberInfo)
                        {
                            stackTrace += std::to_wstring(lineNumber);
                        }

                        stackTrace += L':';
                        auto column = hFrame->GetColumn();
                        if (column != Message::kNoColumnInfo)
                        {
                            stackTrace += std::to_wstring(column);
                        }

                        if (!hFunctionName.IsEmpty() && (hFunctionName->Length() > 0))
                        {
                            stackTrace += L')';
                        }

                        if (index == 0)
                        {
                            auto hSourceLine = hMessage->GetSourceLine();
                            if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                            {
                                stackTrace += L" -> ";
                                stackTrace += StdString(hSourceLine);
                            }
                        }
                    }
                }
            }

            if (hException->IsObject())
            {
                innerException = ExportValue(hException->ToObject()->Get(m_hInnerExceptionName));
            }
        }

        throw V8Exception(V8Exception::Type_General, m_Name, std::move(message), std::move(stackTrace), std::move(innerException));
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
    auto hException = Exception::Error(CreateString(exception.GetMessage()))->ToObject();

    auto hInnerException = ImportValue(exception.GetException());
    if (!hInnerException.IsEmpty() && hInnerException->IsObject())
    {
        hException->Set(m_hInnerExceptionName, hInnerException);
    }

    ThrowException(hException);
}
