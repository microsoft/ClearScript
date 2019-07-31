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

    V8ContextProxyImpl::V8ContextProxyImpl(V8IsolateProxy^ gcIsolateProxy, String^ gcName, V8ScriptEngineFlags flags, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        V8Context::Options options;
        options.EnableDebugging = flags.HasFlag(V8ScriptEngineFlags::EnableDebugging);
        options.EnableRemoteDebugging = flags.HasFlag(V8ScriptEngineFlags::EnableRemoteDebugging);
        options.DisableGlobalMembers = flags.HasFlag(V8ScriptEngineFlags::DisableGlobalMembers);
        options.EnableDateTimeConversion = flags.HasFlag(V8ScriptEngineFlags::EnableDateTimeConversion);
        options.EnableDynamicModuleImports = flags.HasFlag(V8ScriptEngineFlags::EnableDynamicModuleImports);
        options.DebugPort = debugPort;

        try
        {
            auto gcIsolateProxyImpl = dynamic_cast<V8IsolateProxyImpl^>(gcIsolateProxy);
            m_pspContext = new SharedPtr<V8Context>(gcIsolateProxyImpl->CreateContext(StdString(gcName), options));
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

    Object^ V8ContextProxyImpl::Execute(UniqueDocumentInfo^ documentInfo, String^ gcCode, Boolean evaluate)
    {
        try
        {
            return ExportValue(GetContext()->Execute(V8DocumentInfo(documentInfo), StdString(gcCode), evaluate));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode)
    {
        try
        {
            return gcnew V8ScriptImpl(documentInfo, GetContext()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes)
    {
        #pragma warning(push)
        #pragma warning(disable:4947) /* 'Microsoft::ClearScript::V8::V8CacheKind::Parser': marked as obsolete */

        if (cacheKind == V8CacheKind::None)
        {
            gcCacheBytes = nullptr;
            return Compile(documentInfo, gcCode);
        }

        try
        {
            std::vector<uint8_t> cacheBytes;
            auto cacheType = (cacheKind == V8CacheKind::Parser) ? V8CacheType::Parser : V8CacheType::Code;
            auto gcScript = gcnew V8ScriptImpl(documentInfo, GetContext()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode), cacheType, cacheBytes));

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

        #pragma warning(pop)
    }

    //-------------------------------------------------------------------------

    V8Script^ V8ContextProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted)
    {
        #pragma warning(push)
        #pragma warning(disable:4947) /* 'Microsoft::ClearScript::V8::V8CacheKind::Parser': marked as obsolete */

        if ((cacheKind == V8CacheKind::None) || (gcCacheBytes == nullptr) || (gcCacheBytes->Length < 1))
        {
            cacheAccepted = false;
            return Compile(documentInfo, gcCode);
        }

        try
        {
            auto length = gcCacheBytes->Length;
            std::vector<uint8_t> cacheBytes(length);
            Marshal::Copy(gcCacheBytes, 0, (IntPtr)&cacheBytes[0], length);

            bool tempCacheAccepted;
            auto cacheType = (cacheKind == V8CacheKind::Parser) ? V8CacheType::Parser : V8CacheType::Code;
            auto gcScript = gcnew V8ScriptImpl(documentInfo, GetContext()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode), cacheType, cacheBytes, tempCacheAccepted));

            cacheAccepted = tempCacheAccepted;
            return gcScript;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }

        #pragma warning(pop)
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
        v8::HeapStatistics heapStatistics;
        GetContext()->GetIsolateHeapStatistics(heapStatistics);

        auto gcHeapInfo = gcnew V8RuntimeHeapInfo();
        gcHeapInfo->TotalHeapSize = heapStatistics.total_heap_size();
        gcHeapInfo->TotalHeapSizeExecutable = heapStatistics.total_heap_size_executable();
        gcHeapInfo->TotalPhysicalSize = heapStatistics.total_physical_size();
        gcHeapInfo->UsedHeapSize = heapStatistics.used_heap_size();
        gcHeapInfo->HeapSizeLimit = heapStatistics.heap_size_limit();
        return gcHeapInfo;
    }

    //-------------------------------------------------------------------------

    V8Runtime::Statistics^ V8ContextProxyImpl::GetRuntimeStatistics()
    {
        auto statistics = GetContext()->GetIsolateStatistics();

        auto gcStatistics = gcnew V8Runtime::Statistics;
        gcStatistics->ScriptCount = statistics.ScriptCount;
        gcStatistics->ScriptCacheSize = statistics.ScriptCacheSize;
        gcStatistics->ModuleCount = statistics.ModuleCount;
        return gcStatistics;
    }

    //-------------------------------------------------------------------------

    V8ScriptEngine::Statistics^ V8ContextProxyImpl::GetStatistics()
    {
        auto statistics = GetContext()->GetStatistics();

        auto gcStatistics = gcnew V8ScriptEngine::Statistics;
        gcStatistics->ScriptCount = statistics.ScriptCount;
        gcStatistics->ModuleCount = statistics.ModuleCount;
        gcStatistics->ModuleCacheSize = statistics.ModuleCacheSize;
        return gcStatistics;
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

    bool V8ContextProxyImpl::BeginCpuProfile(String^ gcName, V8CpuProfileFlags flags)
    {
        return GetContext()->BeginCpuProfile(StdString(gcName), v8::kLeafNodeLineNumbers, flags.HasFlag(V8CpuProfileFlags::EnableSampleCollection));
    }

    //-------------------------------------------------------------------------

    V8CpuProfile^ V8ContextProxyImpl::EndCpuProfile(String^ gcName)
    {
        auto gcContext = Tuple::Create<IV8EntityProxy^, V8CpuProfile^>(this, gcnew V8CpuProfile);
        return GetContext()->EndCpuProfile(StdString(gcName), V8IsolateProxyImpl::GetCpuProfileCallback(), &gcContext) ? gcContext->Item2 : nullptr;
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::CollectCpuProfileSample()
    {
        GetContext()->CollectCpuProfileSample();
    }

    //-------------------------------------------------------------------------

    UInt32 V8ContextProxyImpl::CpuProfileSampleInterval::get()
    {
        return GetContext()->GetCpuProfileSampleInterval();
    }

    //-------------------------------------------------------------------------

    void V8ContextProxyImpl::CpuProfileSampleInterval::set(UInt32 value)
    {
        GetContext()->SetCpuProfileSampleInterval(value);
    }

    //-------------------------------------------------------------------------

    String^ V8ContextProxyImpl::CreateManagedString(v8::Local<v8::Value> hValue)
    {
        return GetContext()->CreateStdString(hValue).ToManagedString();
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
            auto gcValue = dynamic_cast<DateTime^>(gcObject);
            if (gcValue != nullptr)
            {
                return V8Value(V8Value::DateTime, (gcValue->ToUniversalTime() - DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind::Utc)).TotalMilliseconds);
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
            int32_t result;
            if (value.AsInt32(result))
            {
                return result;
            }
        }

        {
            uint32_t result;
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

        {
            double result;
            if (value.AsDateTime(result))
            {
                return DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind::Utc) + TimeSpan::FromMilliseconds(result);
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
