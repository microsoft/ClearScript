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

static void* PtrFromObjectHandle(Persistent<Object> hObject)
{
    return hObject.ToPtr();
}

//-----------------------------------------------------------------------------

static Persistent<Object> ObjectHandleFromPtr(void* pvObject)
{
    return Persistent<Object>::FromPtr(pvObject);
}

//-----------------------------------------------------------------------------

static void* PtrFromScriptHandle(Persistent<Script> hScript)
{
    return hScript.ToPtr();
}

//-----------------------------------------------------------------------------

static Persistent<Script> ScriptHandleFromPtr(void* pvScript)
{
    return Persistent<Script>::FromPtr(pvScript);
}

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

#define BEGIN_CONTEXT_SCOPE_NOTHROW \
    { \
        Context::Scope t_ContextScopeNoThrow(m_hContext);

#define END_CONTEXT_SCOPE_NOTHROW \
        IGNORE_UNUSED(t_ContextScopeNoThrow); \
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

V8ContextImpl::V8ContextImpl(V8IsolateImpl* pIsolateImpl, const wchar_t* pName, bool enableDebugging, bool disableGlobalMembers, int debugPort):
    m_spIsolateImpl(pIsolateImpl)
{
    VerifyNotOutOfMemory();

    if (pName != nullptr)
    {
        m_Name = pName;
    }

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

        BEGIN_CONTEXT_SCOPE

            // Be careful when renaming the cookie or changing the way host objects are marked.
            // Such a change will require a corresponding change in the V8ScriptEngine constructor.

            m_hHostObjectCookieName = CreatePersistent(CreateString("{c2cf47d3-916b-4a3f-be2a-6ff567425808}"));
            m_hInnerExceptionName = CreatePersistent(CreateString("inner"));

            m_hHostObjectTemplate = CreatePersistent(CreateFunctionTemplate());
            m_hHostObjectTemplate->SetClassName(CreateString("HostObject"));

            m_hHostObjectTemplate->InstanceTemplate()->SetInternalFieldCount(1);
            m_hHostObjectTemplate->InstanceTemplate()->SetNamedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, Wrap());
            m_hHostObjectTemplate->InstanceTemplate()->SetIndexedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, Wrap());
            m_hHostObjectTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, Wrap());

        END_CONTEXT_SCOPE

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

void V8ContextImpl::SetGlobalProperty(const wchar_t* pName, const V8Value& value, bool globalMembers)
{
    BEGIN_CONTEXT_SCOPE

        auto hValue = ImportValue(value);
        m_hContext->Global()->ForceSet(CreateString(pName), hValue, (PropertyAttribute)(ReadOnly | DontDelete));
        if (globalMembers && hValue->IsObject())
        {
            m_GlobalMembersStack.push_back(CreatePersistent(hValue->ToObject()));
        }

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(const wchar_t* pDocumentName, const wchar_t* pCode, bool /* discard */)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = VERIFY(Script::Compile(CreateString(pCode), CreateString(pDocumentName)));
        return ExportValue(VERIFY(hScript->Run()));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const wchar_t* pDocumentName, const wchar_t* pCode)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = VERIFY(Script::New(CreateString(pCode), CreateString(pDocumentName)));
        return new V8ScriptHolderImpl(m_spIsolateImpl, ::PtrFromScriptHandle(CreatePersistent(hScript)));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::CanExecute(V8ScriptHolder* pHolder)
{
    return m_spIsolateImpl.GetRawPtr() == pHolder->GetIsolate();
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(V8ScriptHolder* pHolder)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hScript = ::ScriptHandleFromPtr(pHolder->GetScript());
        return ExportValue(VERIFY(hScript->Run()));

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

void* V8ContextImpl::AddRefV8Object(void* pvObject)
{
    BEGIN_ISOLATE_SCOPE

        return ::PtrFromObjectHandle(CreatePersistent(::ObjectHandleFromPtr(pvObject)));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ReleaseV8Object(void* pvObject)
{
    BEGIN_ISOLATE_SCOPE

        Dispose(::ObjectHandleFromPtr(pvObject));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(void* pvObject, const wchar_t* pName)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        return ExportValue(::ObjectHandleFromPtr(pvObject)->Get(CreateString(pName)));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, const wchar_t* pName, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        ::ObjectHandleFromPtr(pvObject)->Set(CreateString(pName), ImportValue(value));

    END_EXECUTION_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, const wchar_t* pName)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvObject)->Delete(CreateString(pName));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(void* pvObject, vector<wstring>& names)
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

void V8ContextImpl::GetV8ObjectPropertyIndices(void* pvObject, vector<int>& indices)
{
    BEGIN_CONTEXT_SCOPE

        GetV8ObjectPropertyIndices(::ObjectHandleFromPtr(pvObject), indices);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8Object(void* pvObject, const vector<V8Value>& args, bool asConstructor)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        vector<Handle<Value>> importedArgs;
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

V8Value V8ContextImpl::InvokeV8ObjectMethod(void* pvObject, const wchar_t* pName, const vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_EXECUTION_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        auto hName = CreateString(pName);
        if (!hObject->Has(hName))
        {
            auto hError = Exception::TypeError(CreateString("Method or property not found"))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name.c_str(), *String::Value(hError), *String::Value(hError->Get(CreateString("stack"))), V8Value(V8Value::Undefined));
        }

        auto hValue = hObject->Get(hName);
        if (hValue->IsUndefined() || hValue->IsNull())
        {
            auto hError = Exception::TypeError(CreateString("Property value does not support invocation"))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name.c_str(), *String::Value(hError), *String::Value(hError->Get(CreateString("stack"))), V8Value(V8Value::Undefined));
        }

        vector<Handle<Value>> importedArgs;
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

        HostObjectHelpers::Release(m_pvV8ObjectCache);
        m_spIsolateImpl->RemoveContext(this);

        BEGIN_CONTEXT_SCOPE_NOTHROW

            for (auto it = m_GlobalMembersStack.rbegin(); it != m_GlobalMembersStack.rend(); it++)
            {
                Dispose(*it);
            }

            Dispose(m_hHostObjectTemplate);
            Dispose(m_hInnerExceptionName);
            Dispose(m_hHostObjectCookieName);

        END_CONTEXT_SCOPE_NOTHROW

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

        // exit handle scope to maximize GC benefit (see below)

    END_ISOLATE_SCOPE

    BEGIN_ISOLATE_SCOPE

        // The context is gone, but it may have contained host object holders
        // that are now awaiting collection. Forcing collection will release
        // the corresponding host objects. This isn't strictly necessary, but
        // it greatly simplifies certain test scenarios. The technique here is
        // not well documented, but it has been noted in several discussions.

        while (!V8::IdleNotification());

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::Wrap()
{
    return CreateExternal(this);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(Handle<Object> hObject, vector<wstring>& names)
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
                wstring name(*String::Value(hName));

                int propertyIndex;
                if (!HostObjectHelpers::TryParseInt32(name.c_str(), propertyIndex))
                {
                    names.push_back(name);
                }
            }
        }
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(Handle<Object> hObject, vector<int>& indices)
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
                wstring name(*String::Value(hName));

                int propertyIndex;
                if (HostObjectHelpers::TryParseInt32(name.c_str(), propertyIndex))
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
                            CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(::GetHostObject(*it), *String::Value(hName)));
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
            const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
            {
                vector<wstring> names;
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    vector<wstring> tempNames;
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
                    hImportedNames->Set(index, pContextImpl->CreateString(names[index].c_str()));
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
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
            const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
            if (stack.size() > 0)
            {
                vector<int> indices;
                for (auto it = stack.rbegin(); it != stack.rend(); it++)
                {
                    vector<int> tempIndices;
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

        CALLBACK_RETURN(pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), *String::Value(hName))));
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
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), *String::Value(hName), pContextImpl->ExportValue(hValue));
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

        vector<wstring> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);

        String::Value nameValue(hName);
        for (auto it = names.begin(); it != names.end(); it++)
        {
            if (it->compare(*nameValue) == 0)
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
        CALLBACK_RETURN(HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), *String::Value(hName)));
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
        vector<wstring> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);
        auto nameCount = static_cast<int>(names.size());

        auto hImportedNames = pContextImpl->CreateArray(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            hImportedNames->Set(index, pContextImpl->CreateString(names[index].c_str()));
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
        vector<int> indices;
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
        vector<int> indices;
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

        vector<V8Value> exportedArgs;
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
    HostObjectHelpers::Release(pvV8ObjectCache);
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
        const wchar_t* pResult;
        if (value.AsString(pResult))
        {
            return CreateString(pResult);
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
                pvV8Object = ::PtrFromObjectHandle(MakeWeak(CreatePersistent(hObject), HostObjectHelpers::AddRef(m_pvV8ObjectCache), DisposeWeakHandle));
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
        return V8Value(*String::Value(hValue));
    }

    if (hValue->IsObject())
    {
        auto hObject = hValue->ToObject();
        if (hObject->HasOwnProperty(m_hHostObjectCookieName))
        {
            return V8Value(::GetHostObjectHolder(hObject)->Clone());
        }

        return V8Value(new V8ObjectHolderImpl(this, ::PtrFromObjectHandle(CreatePersistent(hObject))));
    }

    return V8Value(V8Value::Undefined);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ImportValues(const vector<V8Value>& values, vector<Handle<Value>>& importedValues)
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
            throw V8Exception(V8Exception::Type_Interrupt, m_Name.c_str(), L"Script execution interrupted by host", *String::Value(tryCatch.StackTrace()), V8Value(V8Value::Undefined));
        }

        auto hException = tryCatch.Exception();

        wstring message;
        bool stackOverflow;

        String::Value value(hException);
        if (value.length() > 0)
        {
            message = *value;
            stackOverflow = (_wcsicmp(message.c_str(), L"RangeError: Maximum call stack size exceeded") == 0);
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
            _ASSERTE(wstring(*String::Value(hException->ToObject()->GetConstructorName())) == L"RangeError");
        }

    #endif // _DEBUG

        wstring stackTrace;
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
                stackTrace = *String::Value(hStackTrace);
            }

            auto hMessage = tryCatch.Message();
            if (!hMessage.IsEmpty())
            {
                if (message.length() < 1)
                {
                    message = *String::Value(hMessage->Get());
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
                            stackTrace += *String::Value(hScriptName);
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
                    stackTrace += to_wstring(hMessage->GetLineNumber());
                    stackTrace += L':';
                    stackTrace += to_wstring(hMessage->GetStartColumn() + 1);

                    auto hSourceLine = hMessage->GetSourceLine();
                    if (!hSourceLine.IsEmpty() && (hSourceLine->Length() > 0))
                    {
                        stackTrace += L" -> ";
                        stackTrace += *String::Value(hSourceLine);
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

                            stackTrace += *String::Value(hFunctionName);
                            stackTrace += L" (";
                        }

                        auto hScriptName = hFrame->GetScriptName();
                        if (!hScriptName.IsEmpty() && (hScriptName->Length() > 0))
                        {
                            stackTrace += *String::Value(hScriptName);
                        }
                        else
                        {
                            stackTrace += L"<anonymous>";
                        }

                        stackTrace += L':';
                        auto lineNumber = hFrame->GetLineNumber();
                        if (lineNumber != Message::kNoLineNumberInfo)
                        {
                            stackTrace += to_wstring(lineNumber);
                        }

                        stackTrace += L':';
                        auto column = hFrame->GetColumn();
                        if (column != Message::kNoColumnInfo)
                        {
                            stackTrace += to_wstring(column);
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
                                stackTrace += *String::Value(hSourceLine);
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

        throw V8Exception(V8Exception::Type_General, m_Name.c_str(), message.c_str(), stackTrace.c_str(), innerException);
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
