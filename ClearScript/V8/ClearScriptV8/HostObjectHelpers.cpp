// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

//-----------------------------------------------------------------------------
// local helper functions
//-----------------------------------------------------------------------------

static void DECLSPEC_NORETURN ThrowHostException(ScriptEngine^ gcEngine, Exception^ gcException)
{
    throw HostException(StdString(gcException->GetBaseException()->Message), (gcEngine != nullptr) ? V8ContextProxyImpl::ImportValue(gcEngine->MarshalToScript(gcException)) : V8Value(V8Value::Null));
}

//-----------------------------------------------------------------------------

static void DECLSPEC_NORETURN ThrowHostException(void* pvSource, Exception^ gcException)
{
    throw HostException(StdString(gcException->GetBaseException()->Message), V8ContextProxyImpl::ImportValue(V8ProxyHelpers::MarshalExceptionToScript(pvSource, gcException)));
}

//-----------------------------------------------------------------------------
// HostObjectHelpers implementation
//-----------------------------------------------------------------------------

void* HostObjectHelpers::AddRef(void* pvObject)
{
    return V8ProxyHelpers::AddRefHostObject(pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::Release(void* pvObject)
{
    V8ProxyHelpers::ReleaseHostObject(pvObject);
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetProperty(void* pvObject, const StdString& name)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetHostObjectProperty(pvObject, name.ToManagedString()));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetProperty(void* pvObject, const StdString& name, bool& isCacheable)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetHostObjectProperty(pvObject, name.ToManagedString(), isCacheable));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::SetProperty(void* pvObject, const StdString& name, const V8Value& value)
{
    try
    {
        V8ProxyHelpers::SetHostObjectProperty(pvObject, name.ToManagedString(), V8ContextProxyImpl::ExportValue(value));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::DeleteProperty(void* pvObject, const StdString& name)
{
    try
    {
        return V8ProxyHelpers::DeleteHostObjectProperty(pvObject, name.ToManagedString());
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetPropertyNames(void* pvObject, std::vector<StdString>& names)
{
    try
    {
        auto gcNames = V8ProxyHelpers::GetHostObjectPropertyNames(pvObject);
        auto nameCount = gcNames->Length;

        names.resize(nameCount);
        for (auto index = 0; index < nameCount; index++)
        {
            names[index] = StdString(gcNames[index]);
        }
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetProperty(void* pvObject, int index)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetHostObjectProperty(pvObject, index));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::SetProperty(void* pvObject, int index, const V8Value& value)
{
    try
    {
        V8ProxyHelpers::SetHostObjectProperty(pvObject, index, V8ContextProxyImpl::ExportValue(value));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::DeleteProperty(void* pvObject, int index)
{
    try
    {
        return V8ProxyHelpers::DeleteHostObjectProperty(pvObject, index);
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetPropertyIndices(void* pvObject, std::vector<int>& indices)
{
    try
    {
        auto gcIndices = V8ProxyHelpers::GetHostObjectPropertyIndices(pvObject);
        auto indexCount = gcIndices->Length;

        indices.resize(indexCount);
        for (auto index = 0; index < indexCount; index++)
        {
            indices[index] = gcIndices[index];
        }
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::Invoke(void* pvObject, bool asConstructor, const std::vector<V8Value>& args)
{
    try
    {
        auto argCount = static_cast<int>(args.size());

        auto exportedArgs = gcnew array<Object^>(argCount);
        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs[index] = V8ContextProxyImpl::ExportValue(args[index]);
        }

        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::InvokeHostObject(pvObject, asConstructor, exportedArgs));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::InvokeMethod(void* pvObject, const StdString& name, const std::vector<V8Value>& args)
{
    try
    {
        auto argCount = static_cast<int>(args.size());

        auto exportedArgs = gcnew array<Object^>(argCount);
        for (auto index = 0; index < argCount; index++)
        {
            exportedArgs[index] = V8ContextProxyImpl::ExportValue(args[index]);
        }

        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::InvokeHostObjectMethod(pvObject, name.ToManagedString(), exportedArgs));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

HostObjectHelpers::V8Invocability HostObjectHelpers::GetInvocability(void* pvObject)
{
    try
    {
        switch (V8ProxyHelpers::GetHostObjectInvocability(pvObject))
        {
            case Invocability::None:
                return V8Invocability::None;

            case Invocability::Delegate:
                return V8Invocability::Delegate;

            default:
                return V8Invocability::Other;
        }
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

V8Value HostObjectHelpers::GetEnumerator(void* pvObject)
{
    try
    {
        return V8ContextProxyImpl::ImportValue(V8ProxyHelpers::GetEnumeratorForHostObject(pvObject));
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvObject, gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::AdvanceEnumerator(void* pvEnumerator, V8Value& value)
{
    try
    {
        Object^ gcValue;
        if (V8ProxyHelpers::AdvanceEnumerator(pvEnumerator, gcValue))
        {
            value = V8ContextProxyImpl::ImportValue(gcValue);
            return true;
        }

        return false;
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(pvEnumerator, gcException);
    }
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::CreateV8ObjectCache()
{
    return V8ProxyHelpers::CreateV8ObjectCache();
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::CacheV8Object(void* pvCache, void* pvObject, void* pvV8Object)
{
    V8ProxyHelpers::CacheV8Object(pvCache, pvObject, pvV8Object);
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::GetCachedV8Object(void* pvCache, void* pvObject)
{
    return V8ProxyHelpers::GetCachedV8Object(pvCache, pvObject);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::GetAllCachedV8Objects(void* pvCache, std::vector<void*>& v8ObjectPtrs)
{
    auto gcV8ObjectPtrs = V8ProxyHelpers::GetAllCachedV8Objects(pvCache);
    auto v8ObjectCount = gcV8ObjectPtrs->Length;

    v8ObjectPtrs.resize(v8ObjectCount);
    for (auto index = 0; index < v8ObjectCount; index++)
    {
        v8ObjectPtrs[index] = gcV8ObjectPtrs[index].ToPointer();
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::RemoveV8ObjectCacheEntry(void* pvCache, void* pvObject)
{
    return V8ProxyHelpers::RemoveV8ObjectCacheEntry(pvCache, pvObject);
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::CreateDebugAgent(const StdString& name, const StdString& version, int port, bool remote, DebugCallback&& callback)
{
    return V8ProxyHelpers::CreateDebugAgent(name.ToManagedString(), version.ToManagedString(), port, remote, gcnew V8DebugListenerImpl(std::move(callback)));
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::SendDebugMessage(void* pvAgent, const StdString& content)
{
    return V8ProxyHelpers::SendDebugMessage(pvAgent, content.ToManagedString());
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::DestroyDebugAgent(void* pvAgent)
{
    V8ProxyHelpers::DestroyDebugAgent(pvAgent);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::QueueNativeCallback(NativeCallback&& callback)
{
    return MiscHelpers::QueueNativeCallback(gcnew NativeCallbackImpl(std::move(callback)));
}

//-----------------------------------------------------------------------------

void* HostObjectHelpers::CreateNativeCallbackTimer(int dueTime, int period, NativeCallback&& callback)
{
    return V8ProxyHelpers::CreateNativeCallbackTimer(dueTime, period, gcnew NativeCallbackImpl(std::move(callback)));
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::ChangeNativeCallbackTimer(void* pvTimer, int dueTime, int period)
{
    return V8ProxyHelpers::ChangeNativeCallbackTimer(pvTimer, dueTime, period);
}

//-----------------------------------------------------------------------------

void HostObjectHelpers::DestroyNativeCallbackTimer(void* pvTimer)
{
    V8ProxyHelpers::DestroyNativeCallbackTimer(pvTimer);
}

//-----------------------------------------------------------------------------

StdString HostObjectHelpers::LoadModule(const V8DocumentInfo& sourceDocumentInfo, const StdString& specifier, V8DocumentInfo& documentInfo)
{
    try
    {
        UniqueDocumentInfo^ uniqueDocumentInfo;
        StdString code(V8ProxyHelpers::LoadModule(sourceDocumentInfo.GetDocumentInfo(), specifier.ToManagedString(), ModuleCategory::Standard, uniqueDocumentInfo));
        documentInfo = V8DocumentInfo(uniqueDocumentInfo);
        return code;
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(ScriptEngine::Current, gcException);
    }
}

//-----------------------------------------------------------------------------

std::vector<std::pair<StdString, V8Value>> HostObjectHelpers::CreateModuleContext(const V8DocumentInfo& documentInfo)
{
    try
    {
        std::vector<std::pair<StdString, V8Value>> context;

        auto gcContext = V8ProxyHelpers::CreateModuleContext(documentInfo.GetDocumentInfo());
        if (gcContext != nullptr)
        {
            context.reserve(gcContext->Count);

            auto gcEnumerator = gcContext->GetEnumerator();
            while (gcEnumerator->MoveNext())
            {
                context.push_back(std::make_pair(StdString(gcEnumerator->Current.Key), V8ContextProxyImpl::ImportValue(gcEnumerator->Current.Value)));
            }
        }

        return context;
    }
    catch (Exception^ gcException)
    {
        ThrowHostException(ScriptEngine::Current, gcException);
    }
}

//-----------------------------------------------------------------------------

bool HostObjectHelpers::TryParseInt32(const StdString& text, int& result)
{
    return Int32::TryParse(text.ToManagedString(), NumberStyles::Integer, CultureInfo::InvariantCulture, result);
}
