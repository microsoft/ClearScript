// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // local helper functions
    //-------------------------------------------------------------------------

    static void LockCallback(void* pvArg)
    {
        (*static_cast<Action^*>(pvArg))();
    }

    //-------------------------------------------------------------------------
    // V8ContextProxyImpl implementation
    //-------------------------------------------------------------------------

    V8ContextProxyImpl::V8ContextProxyImpl(V8IsolateProxy^ gcIsolateProxy, String^ gcName, Boolean enableDebugging, Boolean disableGlobalMembers, Boolean enableRemoteDebugging, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        try
        {
            auto gcIsolateProxyImpl = dynamic_cast<V8IsolateProxyImpl^>(gcIsolateProxy);
            m_pspContext = new SharedPtr<V8Context>(V8Context::Create(gcIsolateProxyImpl->GetIsolate(), StdString(gcName), enableDebugging, disableGlobalMembers, enableRemoteDebugging, debugPort));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    UIntPtr V8ContextProxyImpl::MaxRuntimeHeapSize::get()
    {
        return (UIntPtr)GetContext()->GetMaxIsolateHeapSize();
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::MaxRuntimeHeapSize::set(UIntPtr value)
    {
        GetContext()->SetMaxIsolateHeapSize(static_cast<size_t>(value));
    }

    //-------------------------------------------------------------------------

    TimeSpan V8ContextProxyImpl::RuntimeHeapSizeSampleInterval::get()
    {
        return TimeSpan::FromMilliseconds(GetContext()->GetIsolateHeapSizeSampleInterval());
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::RuntimeHeapSizeSampleInterval::set(TimeSpan value)
    {
        GetContext()->SetIsolateHeapSizeSampleInterval(value.TotalMilliseconds);
    }

    //-------------------------------------------------------------------------

    UIntPtr V8ContextProxyImpl::MaxRuntimeStackUsage::get()
    {
        return (UIntPtr)GetContext()->GetMaxIsolateStackUsage();
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::MaxRuntimeStackUsage::set(UIntPtr value)
    {
        GetContext()->SetMaxIsolateStackUsage(static_cast<size_t>(value));
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::InvokeWithLock(Action^ gcAction)
    {
        try
        {
            GetContext()->CallWithLock(LockCallback, &gcAction);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::GetRootItem()
    {
        try
        {
            return ExportValue(GetContext()->GetRootObject());
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::AddGlobalItem(String^ gcName, Object^ gcItem, Boolean globalMembers)
    {
        try
        {
            GetContext()->SetGlobalProperty(StdString(gcName), ImportValue(gcItem), globalMembers);
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::AwaitDebuggerAndPause()
    {
        try
        {
            return GetContext()->AwaitDebuggerAndPause();
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::Execute(String^ gcDocumentName, String^ gcCode, Boolean evaluate, Boolean discard)
    {
        try
        {
            return ExportValue(GetContext()->Execute(StdString(gcDocumentName), StdString(gcCode), evaluate, discard));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(String^ gcDocumentName, String^ gcCode)
    {
        try
        {
            return gcnew V8ScriptImpl(gcDocumentName, GetContext()->Compile(StdString(gcDocumentName), StdString(gcCode)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes)
    {
        if (cacheKind == V8CacheKind::None)
        {
            gcCacheBytes = nullptr;
            return Compile(gcDocumentName, gcCode);
        }

        try
        {
            std::vector<std::uint8_t> cacheBytes;
            auto cacheType = (cacheKind == V8CacheKind::Parser) ? V8CacheType::Parser : V8CacheType::Code;
            auto gcScript = gcnew V8ScriptImpl(gcDocumentName, GetContext()->Compile(StdString(gcDocumentName), StdString(gcCode), cacheType, cacheBytes));

            auto length = static_cast<int>(cacheBytes.size());
            if (length < 1)
            {
                gcCacheBytes = nullptr;
            }
            else
            {
                gcCacheBytes = gcnew array<Byte>(length);
                Marshal::Copy((IntPtr)&cacheBytes[0], gcCacheBytes, 0, length);
            }

            return gcScript;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted)
    {
        if ((cacheKind == V8CacheKind::None) || (gcCacheBytes == nullptr) || (gcCacheBytes->Length < 1))
        {
            cacheAccepted = false;
            return Compile(gcDocumentName, gcCode);
        }

        try
        {
            auto length = gcCacheBytes->Length;
            std::vector<std::uint8_t> cacheBytes(length);
            Marshal::Copy(gcCacheBytes, 0, (IntPtr)&cacheBytes[0], length);

            bool tempCacheAccepted;
            auto cacheType = (cacheKind == V8CacheKind::Parser) ? V8CacheType::Parser : V8CacheType::Code;
            auto gcScript = gcnew V8ScriptImpl(gcDocumentName, GetContext()->Compile(StdString(gcDocumentName), StdString(gcCode), cacheType, cacheBytes, tempCacheAccepted));

            cacheAccepted = tempCacheAccepted;
            return gcScript;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::Execute(V8Script^ gcScript, Boolean evaluate)
    {
        try
        {
            auto gcScriptImpl = dynamic_cast<V8ScriptImpl^>(gcScript);
            if (gcScriptImpl == nullptr)
            {
                throw gcnew ArgumentException(L"Invalid compiled script", L"script");
            }

            auto spContext = GetContext();
            auto spHolder = gcScriptImpl->GetHolder();
            if (!spContext->CanExecute(spHolder))
            {
                throw gcnew ArgumentException(L"Invalid compiled script", L"script");
            }

            return ExportValue(spContext->Execute(spHolder, evaluate));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::Interrupt()
    {
        GetContext()->Interrupt();
    }

    //-------------------------------------------------------------------------

    V8RuntimeHeapInfo^ V8ContextProxyImpl::GetRuntimeHeapInfo()
    {
        V8IsolateHeapInfo heapInfo;
        GetContext()->GetIsolateHeapInfo(heapInfo);

        auto gcHeapInfo = gcnew V8RuntimeHeapInfo();
        gcHeapInfo->TotalHeapSize = heapInfo.GetTotalHeapSize();
        gcHeapInfo->TotalHeapSizeExecutable = heapInfo.GetTotalHeapSizeExecutable();
        gcHeapInfo->TotalPhysicalSize = heapInfo.GetTotalPhysicalSize();
        gcHeapInfo->UsedHeapSize = heapInfo.GetUsedHeapSize();
        gcHeapInfo->HeapSizeLimit = heapInfo.GetHeapSizeLimit();
        return gcHeapInfo;
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::CollectGarbage(bool exhaustive)
    {
        GetContext()->CollectGarbage(exhaustive);
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::OnAccessSettingsChanged()
    {
        GetContext()->OnAccessSettingsChanged();
    }

    //-------------------------------------------------------------------------

    V8ContextProxyImpl::~V8ContextProxyImpl()
    {
        SharedPtr<V8Context> spContext;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspContext != nullptr)
            {
                // hold V8 context for destruction outside lock scope
                spContext = *m_pspContext;
                delete m_pspContext;
                m_pspContext = nullptr;
            }

        END_LOCK_SCOPE

        if (!spContext.IsEmpty())
        {
            GC::SuppressFinalize(this);
        }
    }

    //-------------------------------------------------------------------------

    V8ContextProxyImpl::!V8ContextProxyImpl()
    {
        if (m_pspContext != nullptr)
        {
            delete m_pspContext;
            m_pspContext = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    V8Value V8ContextProxyImpl::ImportValue(Object^ gcObject)
    {
        if (dynamic_cast<Nonexistent^>(gcObject) != nullptr)
        {
            return V8Value(V8Value::Nonexistent);
        }

        if (gcObject == nullptr)
        {
            return V8Value(V8Value::Undefined);
        }

        if (dynamic_cast<DBNull^>(gcObject) != nullptr)
        {
            return V8Value(V8Value::Null);
        }

        {
            auto gcValue = dynamic_cast<Char^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<SByte^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Byte^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int16^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt16^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int32^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt32^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Int64^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<UInt64^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Boolean^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Single^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Double^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<Decimal^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value((double)*gcValue);
            }
        }

        {
            auto gcValue = dynamic_cast<String^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(new StdString(gcValue));
            }
        }

        {
            auto gcValue = dynamic_cast<V8ObjectImpl^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(gcValue->GetHolder()->Clone(), gcValue->GetSubtype());
            }
        }

        return V8Value(new HostObjectHolderImpl(V8ProxyHelpers::AddRefHostObject(gcObject)));
    }

    //-------------------------------------------------------------------------

    Object^ V8ContextProxyImpl::ExportValue(const V8Value& value)
    {
        if (value.IsNonexistent())
        {
            return Nonexistent::Value;
        }

        if (value.IsUndefined())
        {
            return nullptr;
        }

        if (value.IsNull())
        {
            return DBNull::Value;
        }

        {
            bool result;
            if (value.AsBoolean(result))
            {
                return result;
            }
        }

        {
            double result;
            if (value.AsNumber(result))
            {
                return result;
            }
        }

        {
            std::int32_t result;
            if (value.AsInt32(result))
            {
                return result;
            }
        }

        {
            std::uint32_t result;
            if (value.AsUInt32(result))
            {
                return result;
            }
        }

        {
            const StdString* pString;
            if (value.AsString(pString))
            {
                return pString->ToManagedString();
            }
        }

        {
            V8ObjectHolder* pHolder;
            V8Value::Subtype subtype;
            if (value.AsV8Object(pHolder, subtype))
            {
                return gcnew V8ObjectImpl(pHolder->Clone(), subtype);
            }
        }

        {
            HostObjectHolder* pHolder;
            if (value.AsHostObject(pHolder))
            {
                return V8ProxyHelpers::GetHostObject(pHolder->GetObject());
            }
        }

        return nullptr;
    }

    //-------------------------------------------------------------------------

    SharedPtr<V8Context> V8ContextProxyImpl::GetContext()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspContext == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pspContext;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    ENSURE_INTERNAL_CLASS(V8ContextProxyImpl)

}}}
