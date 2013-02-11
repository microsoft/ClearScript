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

#include "ClearScriptV8.h"

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

static LPVOID PtrFromObjectHandle(Persistent<Object> hObject)
{
    return *hObject;
}

//-----------------------------------------------------------------------------

static Persistent<Object> ObjectHandleFromPtr(LPVOID pvV8Object)
{
    return Persistent<Object>(reinterpret_cast<Object*>(pvV8Object));
}

//-----------------------------------------------------------------------------

static HostObjectHolder* GetHostObjectHolder(Handle<Object> hObject)
{
    _ASSERTE(hObject->InternalFieldCount() > 0);
    return reinterpret_cast<HostObjectHolder*>(hObject->GetAlignedPointerFromInternalField(0));
}

//-----------------------------------------------------------------------------

static void SetHostObjectHolder(Handle<Object> hObject, HostObjectHolder* pHolder)
{
    _ASSERTE(hObject->InternalFieldCount() > 0);
    hObject->SetAlignedPointerInInternalField(0, pHolder);
}

//-----------------------------------------------------------------------------

static LPVOID GetHostObject(Handle<Object> hObject)
{
    auto pHolder = ::GetHostObjectHolder(hObject);
    return (pHolder != nullptr) ? pHolder->GetObject() : nullptr;
}

//-----------------------------------------------------------------------------

static V8ContextImpl* UnwrapContextImpl(const AccessorInfo& info)
{
    return reinterpret_cast<V8ContextImpl*>(Local<External>::Cast(info.Data())->Value());
}

//-----------------------------------------------------------------------------

static V8ContextImpl* UnwrapContextImpl(const Arguments& args)
{
    return reinterpret_cast<V8ContextImpl*>(Local<External>::Cast(args.Data())->Value());
}

//-----------------------------------------------------------------------------

static LPVOID UnwrapHostObject(const AccessorInfo& info)
{
    return ::GetHostObject(info.Holder());
}

//-----------------------------------------------------------------------------

static LPVOID UnwrapHostObject(const Arguments& args)
{
    return ::GetHostObject(args.Holder());
}

//-----------------------------------------------------------------------------

static void Verify(const TryCatch& tryCatch)
{
    if (tryCatch.HasCaught())
    {
        if (!tryCatch.CanContinue())
        {
            throw V8Exception(V8Exception::Type_Interrupt, L"Script execution interrupted by host", *String::Value(tryCatch.StackTrace()));
        }

        throw V8Exception(V8Exception::Type_General, *String::Value(tryCatch.Exception()), *String::Value(tryCatch.StackTrace()));
    }
}

//-----------------------------------------------------------------------------

template<typename T> static T Verify(const TryCatch& tryCatch, T result)
{
    ::Verify(tryCatch);
    return result;
}

//-----------------------------------------------------------------------------
// V8ContextImpl implementation
//-----------------------------------------------------------------------------

#define BEGIN_ISOLATE_SCOPE \
        { \
            Locker t_LockScope(m_pIsolate); \
            Isolate::Scope t_IsolateScope(m_pIsolate); \
            HandleScope t_HandleScope;

#define END_ISOLATE_SCOPE \
            t_HandleScope; \
            t_IsolateScope; \
            t_LockScope; \
        }

#define BEGIN_CONTEXT_SCOPE \
        { \
            Context::Scope t_ContextScope(m_hContext);

#define END_CONTEXT_SCOPE \
            t_ContextScope; \
        }

#define BEGIN_VERIFY_SCOPE \
        { \
            TryCatch t_TryCatch;

#define END_VERIFY_SCOPE \
            t_TryCatch; \
        }

#define VERIFY(result) \
            ::Verify(t_TryCatch, result)

#define VERIFY_CHECKPOINT() \
            ::Verify(t_TryCatch)

//-----------------------------------------------------------------------------

V8ContextImpl::V8ContextImpl(LPCWSTR pName, bool enableDebugging, bool disableGlobalMembers, DebugMessageDispatcher* pDebugMessageDispatcher, int debugPort):
    m_pIsolate(Isolate::New()),
    m_DebugAgentEnabled(false),
    m_DebugMessageDispatchCount(0)
{
    BEGIN_ISOLATE_SCOPE

        if (disableGlobalMembers)
        {
            m_hContext = Context::New();
        }
        else
        {
            auto hGlobalTemplate = ObjectTemplate::New();
            hGlobalTemplate->SetInternalFieldCount(1);
            hGlobalTemplate->SetNamedPropertyHandler(GetGlobalProperty);

            m_hContext = Context::New(nullptr, hGlobalTemplate);

            m_hGlobal = Persistent<Object>::New(m_pIsolate, m_hContext->Global()->GetPrototype()->ToObject());
            _ASSERTE(m_hGlobal->InternalFieldCount() > 0);
            m_hGlobal->SetAlignedPointerInInternalField(0, this);
        }

        BEGIN_CONTEXT_SCOPE

            // Be careful when renaming the cookie or changing the way host objects are marked.
            // Such a change will require a corresponding change in the V8ScriptEngine constructor.

            m_hHostObjectCookieName = Persistent<String>::New(m_pIsolate, String::New(L"{c2cf47d3-916b-4a3f-be2a-6ff567425808}"));

            m_hHostObjectTemplate = Persistent<FunctionTemplate>::New(m_pIsolate, FunctionTemplate::New());
            m_hHostObjectTemplate->SetClassName(String::New("HostObject"));

            m_hHostObjectTemplate->InstanceTemplate()->SetInternalFieldCount(1);
            m_hHostObjectTemplate->InstanceTemplate()->SetNamedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyNames, Wrap());
            m_hHostObjectTemplate->InstanceTemplate()->SetIndexedPropertyHandler(GetHostObjectProperty, SetHostObjectProperty, QueryHostObjectProperty, DeleteHostObjectProperty, GetHostObjectPropertyIndices, Wrap());
            m_hHostObjectTemplate->InstanceTemplate()->SetCallAsFunctionHandler(InvokeHostObject, Wrap());

        END_CONTEXT_SCOPE

        if (enableDebugging)
        {
            Debug::SetDebugMessageDispatchHandler(pDebugMessageDispatcher);
            m_DebugAgentEnabled = Debug::EnableAgent(*String::Utf8Value(String::New(pName)), debugPort);
        }

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

void V8ContextImpl::SetGlobalProperty(LPCWSTR pName, const V8Value& value, bool globalMembers)
{
    BEGIN_CONTEXT_SCOPE

        auto hValue = ImportValue(value);
        m_hContext->Global()->Set(String::New(pName), hValue);
        if (globalMembers && hValue->IsObject())
        {
            m_GlobalMembersStack.push_back(Persistent<Object>::New(m_pIsolate, hValue->ToObject()));
        }

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::Execute(LPCWSTR pDocumentName, LPCWSTR pCode, bool /* discard */)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_VERIFY_SCOPE

        auto hScript = VERIFY(Script::New(String::New(pCode), String::New(pDocumentName)));
        return ExportValue(VERIFY(hScript->Run()));

    END_VERIFY_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::CallWithLock(LockCallback* pCallback, LPVOID pvArg)
{
    BEGIN_ISOLATE_SCOPE

        (*pCallback)(pvArg);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::Interrupt()
{
    V8::TerminateExecution(m_pIsolate);
}

//-----------------------------------------------------------------------------

int V8ContextImpl::IncrementDebugMessageDispatchCount()
{
    return ::InterlockedIncrement(&m_DebugMessageDispatchCount);
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ProcessDebugMessages()
{
    BEGIN_ISOLATE_SCOPE

        ::InterlockedExchange(&m_DebugMessageDispatchCount, 0);
        Debug::ProcessDebugMessages();

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisableDebugAgent()
{
    BEGIN_ISOLATE_SCOPE

        if (m_DebugAgentEnabled)
        {
            Debug::DisableAgent();
            Debug::SetMessageHandler2(nullptr);
            m_DebugAgentEnabled = false;
        }

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

LPVOID V8ContextImpl::AddRefV8Object(LPVOID pvV8Object)
{
    BEGIN_ISOLATE_SCOPE

        return ::PtrFromObjectHandle(Persistent<Object>::New(m_pIsolate, ::ObjectHandleFromPtr(pvV8Object)));

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::ReleaseV8Object(LPVOID pvV8Object)
{
    BEGIN_ISOLATE_SCOPE

        ::ObjectHandleFromPtr(pvV8Object).Dispose(m_pIsolate);

    END_ISOLATE_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName)
{
    BEGIN_CONTEXT_SCOPE

        return ExportValue(::ObjectHandleFromPtr(pvV8Object)->Get(String::New(pName)));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE

        ::ObjectHandleFromPtr(pvV8Object)->Set(String::New(pName), ImportValue(value));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(LPVOID pvV8Object, LPCWSTR pName)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvV8Object)->Delete(String::New(pName));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyNames(LPVOID pvV8Object, vector<wstring>& names)
{
    BEGIN_CONTEXT_SCOPE

        names.clear();

        auto hNames = ::ObjectHandleFromPtr(pvV8Object)->GetPropertyNames();
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

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::GetV8ObjectProperty(LPVOID pvV8Object, int index)
{
    BEGIN_CONTEXT_SCOPE

        return ExportValue(::ObjectHandleFromPtr(pvV8Object)->Get(index));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::SetV8ObjectProperty(LPVOID pvV8Object, int index, const V8Value& value)
{
    BEGIN_CONTEXT_SCOPE

        ::ObjectHandleFromPtr(pvV8Object)->Set(index, ImportValue(value));

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

bool V8ContextImpl::DeleteV8ObjectProperty(LPVOID pvV8Object, int index)
{
    BEGIN_CONTEXT_SCOPE

        return ::ObjectHandleFromPtr(pvV8Object)->Delete(index);

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

void V8ContextImpl::GetV8ObjectPropertyIndices(LPVOID pvV8Object, vector<int>& indices)
{
    BEGIN_CONTEXT_SCOPE

        indices.clear();

        auto hNames = ::ObjectHandleFromPtr(pvV8Object)->GetPropertyNames();
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

    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8Value V8ContextImpl::InvokeV8Object(LPVOID pvV8Object, const vector<V8Value>& args, bool asConstructor)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_VERIFY_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvV8Object);

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

V8Value V8ContextImpl::InvokeV8ObjectMethod(LPVOID pvV8Object, LPCWSTR pName, const vector<V8Value>& args)
{
    BEGIN_CONTEXT_SCOPE
    BEGIN_VERIFY_SCOPE

        auto hObject = ::ObjectHandleFromPtr(pvV8Object);

        auto hName = String::New(pName);
        if (!hObject->Has(hName))
        {
            throw V8Exception(V8Exception::Type_General, *String::Value(Exception::TypeError(String::New("Method or property not found"))), nullptr);
        }

        auto hValue = hObject->Get(hName);
        if (hValue->IsUndefined() || hValue->IsNull())
        {
            throw V8Exception(V8Exception::Type_General, *String::Value(Exception::TypeError(String::New("Property value does not support invcation"))), nullptr);
        }

        vector<Handle<Value>> importedArgs;
        ImportValues(args, importedArgs);

        auto hMethod = VERIFY(hValue->ToObject());
        return ExportValue(VERIFY(hMethod->CallAsFunction(hObject, (int)importedArgs.size(), importedArgs.data())));

    END_VERIFY_SCOPE
    END_CONTEXT_SCOPE
}

//-----------------------------------------------------------------------------

V8ContextImpl::~V8ContextImpl()
{
    BEGIN_ISOLATE_SCOPE

        if (m_DebugAgentEnabled)
        {
            Debug::DisableAgent();
            Debug::SetMessageHandler2(nullptr);
        }

        BEGIN_CONTEXT_SCOPE

            for (auto it = m_GlobalMembersStack.rbegin(); it != m_GlobalMembersStack.rend(); it++)
            {
                it->Dispose(m_pIsolate);
            }

            for (auto it = m_IntegerCache.begin(); it != m_IntegerCache.end(); it++)
            {
                it->second.Dispose(m_pIsolate);
            }

            m_hHostObjectTemplate.Dispose(m_pIsolate);
            m_hHostObjectCookieName.Dispose(m_pIsolate);

        END_CONTEXT_SCOPE

        // As of V8 3.16.0, the global property getter for a disposed context
        // may be invoked during GC after the V8ContextImpl instance is gone.

        if (!m_hGlobal.IsEmpty())
        {
            _ASSERTE(m_hGlobal->InternalFieldCount() > 0);
            m_hGlobal->SetAlignedPointerInInternalField(0, nullptr);
            m_hGlobal.Dispose(m_pIsolate);
        }

        m_hContext.Dispose(m_pIsolate);
        V8::ContextDisposedNotification();

        // The context is gone, but it may have contained host object holders
        // that are now awaiting collection. Forcing collection will release
        // the corresponding host objects. This isn't strictly necessary, but
        // it greatly simplifies certain test scenarios. The technique here is
        // not well documented, but it has been noted in several discussions.

        while (!V8::IdleNotification());

    END_ISOLATE_SCOPE

    m_pIsolate->Dispose();
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::Wrap()
{
    return External::New(this);
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetGlobalProperty(Local<String> hName, const AccessorInfo& info)
{
    auto hGlobal = info.Holder();
    _ASSERTE(hGlobal->InternalFieldCount() > 0);

    auto pContextImpl = reinterpret_cast<V8ContextImpl*>(hGlobal->GetAlignedPointerFromInternalField(0));
    if (pContextImpl != nullptr)
    {
        const vector<Persistent<Object>>& stack = pContextImpl->m_GlobalMembersStack;
        if (stack.size() > 0)
        {
            if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
            {
                return Handle<Value>();
            }

            for (auto it = stack.rbegin(); it != stack.rend(); it++)
            {
                if ((*it)->HasOwnProperty(hName))
                {
                    return (*it)->Get(hName);
                }
            }
        }
    }

    return hGlobal->GetRealNamedProperty(hName);
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    try
    {
        auto pContextImpl = ::UnwrapContextImpl(info);
        if (hName->Equals(pContextImpl->m_hHostObjectCookieName))
        {
            return True();
        }

        return pContextImpl->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), *String::Value(hName)));
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Value>();
    }
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetHostObjectProperty(Local<String> hName, Local<Value> hValue, const AccessorInfo& info)
{
    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), *String::Value(hName), ::UnwrapContextImpl(info)->ExportValue(hValue));
        return hValue;
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Value>();
    }
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    try
    {
        auto pContextImpl = ::UnwrapContextImpl(info);
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
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Integer>();
    }
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteHostObjectProperty(Local<String> hName, const AccessorInfo& info)
{
    try
    {
        return HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), *String::Value(hName)) ? True() : False();
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Boolean>();
    }
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetHostObjectPropertyNames(const AccessorInfo& info)
{
    try
    {
        vector<wstring> names;
        HostObjectHelpers::GetPropertyNames(::UnwrapHostObject(info), names);
        auto nameCount = (int)names.size();

        auto importedNames = Array::New(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            importedNames->Set(index, String::New(names[index].c_str()));
        }

        return importedNames;
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Array>();
    }
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::GetHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    try
    {
        return ::UnwrapContextImpl(info)->ImportValue(HostObjectHelpers::GetProperty(::UnwrapHostObject(info), index));
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Value>();
    }
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::SetHostObjectProperty(unsigned __int32 index, Local<Value> hValue, const AccessorInfo& info)
{
    try
    {
        HostObjectHelpers::SetProperty(::UnwrapHostObject(info), index, ::UnwrapContextImpl(info)->ExportValue(hValue));
        return hValue;
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Value>();
    }
}

//-----------------------------------------------------------------------------

Handle<Integer> V8ContextImpl::QueryHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    try
    {
        vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);

        for (auto it = indices.begin(); it < indices.end(); it++)
        {
            if (*it == (int)index)
            {
                return ::UnwrapContextImpl(info)->GetIntegerHandle(None);
            }
        }

        return Handle<Integer>();
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Integer>();
    }
}

//-----------------------------------------------------------------------------

Handle<Boolean> V8ContextImpl::DeleteHostObjectProperty(unsigned __int32 index, const AccessorInfo& info)
{
    try
    {
        return HostObjectHelpers::DeleteProperty(::UnwrapHostObject(info), index) ? True() : False();
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Boolean>();
    }
}

//-----------------------------------------------------------------------------

Handle<Array> V8ContextImpl::GetHostObjectPropertyIndices(const AccessorInfo& info)
{
    try
    {
        vector<int> indices;
        HostObjectHelpers::GetPropertyIndices(::UnwrapHostObject(info), indices);
        auto indexCount = (int)indices.size();

        auto importedIndices = Array::New(indexCount);
        for (auto index = 0; index < indexCount; index++)
        {
            importedIndices->Set(index, Int32::New(indices[index]));
        }

        return importedIndices;
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Array>();
    }
}

//-----------------------------------------------------------------------------

Handle<Value> V8ContextImpl::InvokeHostObject(const Arguments& args)
{
    try
    {
        auto pContextImpl = ::UnwrapContextImpl(args);
        auto argCount = args.Length();

        vector<V8Value> exportedArgs;
        exportedArgs.reserve(argCount);

        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs.push_back(pContextImpl->ExportValue(args[index]));
        }

        return pContextImpl->ImportValue(HostObjectHelpers::Invoke(::UnwrapHostObject(args), exportedArgs, args.IsConstructCall()));
    }
    catch (const V8Exception& exception)
    {
        ThrowException(String::New(exception.GetMessage()));
        return Handle<Value>();
    }
}

//-----------------------------------------------------------------------------

void V8ContextImpl::DisposeWeakHandle(Isolate* pIsolate, Persistent<Value> hValue, void* /* parameter */)
{
    delete ::GetHostObjectHolder(hValue->ToObject());
    hValue.Dispose(pIsolate);
}

//-----------------------------------------------------------------------------

Persistent<Integer> V8ContextImpl::GetIntegerHandle(int value)
{
    auto it = m_IntegerCache.find(value);
    if (it == m_IntegerCache.end())
    {
        return m_IntegerCache[value] = Persistent<Integer>::New(m_pIsolate, Integer::New(value));
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
        return Undefined();
    }

    if (value.IsNull())
    {
        return Null();
    }

    {
        bool result;
        if (value.AsBoolean(result))
        {
            return Boolean::New(result);
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
            return Int32::New(result);
        }
    }

    {
        unsigned __int32 result;
        if (value.AsUInt32(result))
        {
            return Uint32::New(result);
        }
    }

    {
        LPCWSTR pResult;
        if (value.AsString(pResult))
        {
            return String::New(pResult);
        }
    }

    {
        HostObjectHolder* pHolder;
        if (value.AsHostObject(pHolder))
        {
            auto hObject = m_hHostObjectTemplate->InstanceTemplate()->NewInstance();
            ::SetHostObjectHolder(hObject, pHolder->Clone());
            Persistent<Object>::New(m_pIsolate, hObject).MakeWeak(m_pIsolate, nullptr, DisposeWeakHandle);
            return hObject;
        }
    }

    {
        V8ObjectHolder* pHolder;
        if (value.AsV8Object(pHolder))
        {
            return ::ObjectHandleFromPtr(pHolder->GetObject());
        }
    }

    return Undefined();
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

        return V8Value(new V8ObjectHolderImpl(this, ::PtrFromObjectHandle(Persistent<Object>::New(m_pIsolate, hObject))));
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
