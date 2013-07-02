// 
// Copyright © Microsoft Corporation. All rights reserved.
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
    return *hObject;
}

//-----------------------------------------------------------------------------

static Persistent<Object> ObjectHandleFromPtr(void* pvObject)
{
    return Persistent<Object>(static_cast<Object*>(pvObject));
}

//-----------------------------------------------------------------------------

static void* PtrFromScriptHandle(Persistent<Script> hScript)
{
    return *hScript;
}

//-----------------------------------------------------------------------------

static Persistent<Script> ScriptHandleFromPtr(void* pvScript)
{
    return Persistent<Script>(static_cast<Script*>(pvScript));
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

static V8ContextImpl* UnwrapContextImpl(const AccessorInfo& info)
{
    return static_cast<V8ContextImpl*>(Local<External>::Cast(info.Data())->Value());
}

//-----------------------------------------------------------------------------

static V8ContextImpl* UnwrapContextImpl(const Arguments& args)
{
    return static_cast<V8ContextImpl*>(Local<External>::Cast(args.Data())->Value());
}

//-----------------------------------------------------------------------------

static void* UnwrapHostObject(const AccessorInfo& info)
{
    return ::GetHostObject(info.Holder());
}

//-----------------------------------------------------------------------------

static void* UnwrapHostObject(const Arguments& args)
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

#define BEGIN_VERIFY_SCOPE \
        { \
            TryCatch t_TryCatch;

#define END_VERIFY_SCOPE \
            IGNORE_UNUSED(t_TryCatch); \
        }

#define VERIFY(RESULT) \
            Verify(t_TryCatch, RESULT)

#define VERIFY_CHECKPOINT() \
            Verify(t_TryCatch)

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
            auto hGlobalTemplate = ObjectTemplate::New();
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

            m_hHostObjectCookieName = CreatePersistent(String::New(L"{c2cf47d3-916b-4a3f-be2a-6ff567425808}"));
            m_hInnerExceptionName = CreatePersistent(String::New(L"inner"));

            m_hHostObjectTemplate = CreatePersistent(FunctionTemplate::New());
            m_hHostObjectTemplate->SetClassName(String::New(L"HostObject"));

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
        m_hContext->Global()->ForceSet(String::New(pName), hValue, (PropertyAttribute)(ReadOnly | DontDelete));
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
    BEGIN_VERIFY_SCOPE

        auto hScript = VERIFY(Script::Compile(String::New(pCode), String::New(pDocumentName)));
        return ExportValue(VERIFY(hScript->Run()));

    END_VERIFY_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ScriptHolder* V8ContextImpl::Compile(const wchar_t* pDocumentName, const wchar_t* pCode)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_VERIFY_SCOPE

        auto hScript = VERIFY(Script::New(String::New(pCode), String::New(pDocumentName)));
        return new V8ScriptHolderImpl(m_spIsolateImpl, ::PtrFromScriptHandle(CreatePersistent(hScript)));

    END_VERIFY_SCOPE
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
    BEGIN_VERIFY_SCOPE

        auto hScript = ::ScriptHandleFromPtr(pHolder->GetScript());
        return ExportValue(VERIFY(hScript->Run()));

    END_VERIFY_SCOPE
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

        return ExportValue(::ObjectHandleFromPtr(pvObject)->Get(String::New(pName)));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, const wchar_t* pName, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE

        ::ObjectHandleFromPtr(pvObject)->Set(String::New(pName), ImportValue(value));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(void* pvObject, const wchar_t* pName)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvObject)->Delete(String::New(pName));

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

        return ExportValue(::ObjectHandleFromPtr(pvObject)->Get(index));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(void* pvObject, int index, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE

        ::ObjectHandleFromPtr(pvObject)->Set(index, ImportValue(value));

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
    BEGIN_VERIFY_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        vector<Handle<Value>> importedArgs;
        ImportValues(args, importedArgs);

        if (asConstructor)
        {
            return ExportValue(VERIFY(hObject->CallAsConstructor((int)importedArgs.size(), importedArgs.data())));
        }

        return ExportValue(VERIFY(hObject->CallAsFunction(hObject, (int)importedArgs.size(), importedArgs.data())));

    END_VERIFY_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8ObjectMethod(void* pvObject, const wchar_t* pName, const vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_VERIFY_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvObject);

        auto hName = String::New(pName);
        if (!hObject->Has(hName))
        {
            auto hError = Exception::TypeError(String::New(L"Method or property not found"))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name.c_str(), *String::Value(hError), *String::Value(hError->Get(String::New(L"stack"))), V8Value(V8Value::Undefined));
        }

        auto hValue = hObject->Get(hName);
        if (hValue->IsUndefined() || hValue->IsNull())
        {
            auto hError = Exception::TypeError(String::New(L"Property value does not support invocation"))->ToObject();
            throw V8Exception(V8Exception::Type_General, m_Name.c_str(), *String::Value(hError), *String::Value(hError->Get(String::New(L"stack"))), V8Value(V8Value::Undefined));
        }

        vector<Handle<Value>> importedArgs;
        ImportValues(args, importedArgs);

        auto hMethod = VERIFY(hValue->ToObject());
        return ExportValue(VERIFY(hMethod->CallAsFunction(hObject, (int)importedArgs.size(), importedArgs.data())));

    END_VERIFY_SCOPE
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

            for (auto it = m_IntegerCache.begin(); it != m_IntegerCache.end(); it++)
            {
                Dispose(it->second);
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
    return External::New(this);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(Handle<Object> hObject, vector<wstring>& names)
{
    names.clear();

    auto hNames = hObject->GetPropertyNames();
    if (!hNames.IsEmpty())
    {
        auto nameCount = (int)hNames->Length();

        names.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = hNames->Get(index);
            if (!hName.IsEmpty())
            {
                wstring name(*String::Value(hName->ToString()));

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
        auto nameCount = (int)hNames->Length();

        indices.reserve(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            auto hName = hNames->Get(index);
            if (!hName.IsEmpty())
            {
                wstring name(*String::Value(hName->ToString()));

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

Handle<Value> V8ContextImpl::GetGlobalProperty(Local<String> hName, const AccessorInfo& info)
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
                    return (*it)->Get(hName);
                }
            }
        }
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetGlobalProperty(Local<String> hName, Local<Value> value, const AccessorInfo& info)
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
                    return value;
                }
            }
        }
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryGlobalProperty(Local<String> hName, const AccessorInfo& info)
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
                if ((*it)->Has(hName))
                {
                    return pContextImpl->GetIntegerHandle((*it)->GetPropertyAttributes(hName));
                }
            }
        }
    }

    return Handle<Integer>();
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteGlobalProperty(Local<String> hName, const AccessorInfo& info)
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
                            return HostObjectHelpers::DeleteProperty(::GetHostObject(*it), *String::Value(hName)) ? pContextImpl->GetTrue() : pContextImpl->GetFalse();
                        }
                        catch (const HostException&)
                        {
                            return pContextImpl->GetFalse();
                        }
                    }

                    return (*it)->Delete(hName) ? pContextImpl->GetTrue() : pContextImpl->GetFalse();
                }
            }
        }
    }

    return Handle<Boolean>();
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetGlobalPropertyNames(const AccessorInfo& info)
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
                auto nameCount = (int)(newEnd - names.begin());

                auto hImportedNames = Array::New(nameCount);
                for (auto index = 0; index < nameCount; index++)
                {
                    hImportedNames->Set(index, String::New(names[index].c_str()));
                }

                return hImportedNames;
            }
        }
        catch (const HostException& exception)
        {
            pContextImpl->ThrowScriptException(exception);
        }
    }

    return Handle<Array>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetGlobalProperty(unsigned __int32 index, const AccessorInfo& info)
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
                    return (*it)->Get(index);
                }
            }
        }
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetGlobalProperty(unsigned __int32 index, Local<Value> value, const AccessorInfo& info)
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
                    return value;
                }
            }
        }
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryGlobalProperty(unsigned __int32 index, const AccessorInfo& info)
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
                    return pContextImpl->GetIntegerHandle((*it)->GetPropertyAttributes(pContextImpl->CreateInteger(index)));
                }
            }
        }
    }

    return Handle<Integer>();
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteGlobalProperty(unsigned __int32 index, const AccessorInfo& info)
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
                    return (*it)->Delete(index) ? pContextImpl->GetTrue() : pContextImpl->GetFalse();
                }
            }
        }
    }

    return Handle<Boolean>();
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetGlobalPropertyIndices(const AccessorInfo& info)
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
                auto indexCount = (int)(newEnd - indices.begin());

                auto hImportedIndices = Array::New(indexCount);
                for (auto index = 0; index < indexCount; index++)
                {
                    hImportedIndices->Set(index, pContextImpl->CreateInteger(indices[index]));
                }

                return hImportedIndices;
            }
        }
        catch (const HostException& exception)
        {
            pContextImpl->ThrowScriptException(exception);
        }
    }

    return Handle<Array>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            return pContextImpl->GetTrue();
        }

        return pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), *String::Value(hName)));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetHostObjectProperty(Local<String> hName, Local<Value> hValue, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), *String::Value(hName), pContextImpl->ExportValue(hValue));
        return hValue;
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            return pContextImpl->GetIntegerHandle(ReadOnly | DontEnum | DontDelete);
        }

        vector<wstring> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);

        String::Value nameValue(hName);
        for (auto it = names.begin(); it != names.end(); it++)
        {
            if (it->compare(*nameValue) == 0)
            {
                return pContextImpl->GetIntegerHandle(None);
            }
        }

        return Handle<Integer>();
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Integer>();
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        return HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), *String::Value(hName)) ? pContextImpl->GetTrue() : pContextImpl->GetFalse();
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Boolean>();
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetHostObjectPropertyNames(const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        vector<wstring> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);
        auto nameCount = (int)names.size();

        auto hImportedNames = Array::New(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            hImportedNames->Set(index, String::New(names[index].c_str()));
        }

        return hImportedNames;
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Array>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        return pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), index));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetHostObjectProperty(unsigned __int32 index, Local<Value> hValue, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), index, pContextImpl->ExportValue(hValue));
        return hValue;
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);

        for (auto it = indices.begin(); it < indices.end(); it++)
        {
            if (*it == (int)index)
            {
                return pContextImpl->GetIntegerHandle(None);
            }
        }

        return Handle<Integer>();
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Integer>();
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        return HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), index) ? pContextImpl->GetTrue() : pContextImpl->GetFalse();
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Boolean>();
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetHostObjectPropertyIndices(const AccessorInfo& info)
{
    auto pContextImpl = ::UnwrapContextImpl(info);

    try
    {
        vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);
        auto indexCount = (int)indices.size();

        auto hImportedIndices = Array::New(indexCount);
        for (auto index = 0; index < indexCount; index++)
        {
            hImportedIndices->Set(index, pContextImpl->CreateInteger(indices[index]));
        }

        return hImportedIndices;
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Array>();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::InvokeHostObject(const Arguments& args)
{
    auto pContextImpl = ::UnwrapContextImpl(args);

    try
    {
        auto argCount = args.Length();

        vector<V8Value> exportedArgs;
        exportedArgs.reserve(argCount);

        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs.push_back(pContextImpl->ExportValue(args[index]));
        }

        return pContextImpl->ImportValue(HostObjectHelpers::Invoke(::UnwrapHostObject(args), exportedArgs, args.IsConstructCall()));
    }
    catch (const HostException& exception)
    {
        pContextImpl->ThrowScriptException(exception);
    }

    return Handle<Value>();
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisposeWeakHandle(Isolate* pIsolate, Persistent<Object>* phObject, void* pvV8ObjectCache)
{
    auto pHolder = ::GetHostObjectHolder(*phObject);
    ASSERT_EVAL(HostObjectHelpers::RemoveV8ObjectCacheEntry(pvV8ObjectCache, pHolder->GetObject()));

    delete pHolder;
    HostObjectHelpers::Release(pvV8ObjectCache);
    phObject->Dispose(pIsolate);
}

//-----------------------------------------------------------------------------

Persistent<Integer> V8ContextImpl::GetIntegerHandle(int value)
{
    auto it = m_IntegerCache.find(value);
    if (it == m_IntegerCache.end())
    {
        return m_IntegerCache[value] = CreatePersistent(CreateInteger(value));
    }

    return it->second;
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
            return Number::New(result);
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
            return String::New(pResult);
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

            auto hObject = m_hHostObjectTemplate->InstanceTemplate()->NewInstance();
            ::SetHostObjectHolder(hObject, pHolder->Clone());

            pvV8Object = ::PtrFromObjectHandle(MakeWeak(CreatePersistent(hObject), HostObjectHelpers::AddRef(m_pvV8ObjectCache), DisposeWeakHandle));
            HostObjectHelpers::CacheV8Object(m_pvV8ObjectCache, pHolder->GetObject(), pvV8Object);

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

    auto valueCount = (int)values.size();
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

        V8Value innerException(V8Value::Undefined);

        auto hException = tryCatch.Exception();
        if (hException->IsObject())
        {
            innerException = ExportValue(hException->ToObject()->Get(m_hInnerExceptionName));
        }

        throw V8Exception(V8Exception::Type_General, m_Name.c_str(), *String::Value(tryCatch.Exception()), *String::Value(tryCatch.StackTrace()), innerException);
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
    auto hException = Exception::Error(String::New(exception.GetMessage()))->ToObject();

    auto hInnerException = ImportValue(exception.GetException());
    if (!hInnerException.IsEmpty() && hInnerException->IsObject())
    {
        hException->Set(m_hInnerExceptionName, hInnerException);
    }

    ThrowException(hException);
}
