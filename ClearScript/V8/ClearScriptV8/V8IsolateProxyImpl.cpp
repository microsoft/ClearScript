// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8IsolateProxyImpl implementation
    //-------------------------------------------------------------------------

    V8IsolateProxyImpl::V8IsolateProxyImpl(String^ gcName, V8RuntimeConstraints^ gcConstraints, Boolean enableDebugging, Boolean enableRemoteDebugging, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        const V8IsolateConstraints* pConstraints = nullptr;

        V8IsolateConstraints constraints;
        if (gcConstraints != nullptr)
        {
            constraints.Set(AdjustConstraint(gcConstraints->MaxNewSpaceSize), AdjustConstraint(gcConstraints->MaxOldSpaceSize), AdjustConstraint(gcConstraints->MaxExecutableSize));
            pConstraints = &constraints;
        }

        try
        {
            m_pspIsolate = new SharedPtr<V8Isolate>(V8Isolate::Create(StdString(gcName), pConstraints, enableDebugging, enableRemoteDebugging, debugPort));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    UIntPtr V8IsolateProxyImpl::MaxHeapSize::get()
    {
        return (UIntPtr)GetIsolate()->GetMaxHeapSize();
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::MaxHeapSize::set(UIntPtr value)
    {
        GetIsolate()->SetMaxHeapSize(static_cast<size_t>(value));
    }

    //-------------------------------------------------------------------------

    TimeSpan V8IsolateProxyImpl::HeapSizeSampleInterval::get()
    {
        return TimeSpan::FromMilliseconds(GetIsolate()->GetHeapSizeSampleInterval());
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::HeapSizeSampleInterval::set(TimeSpan value)
    {
        GetIsolate()->SetHeapSizeSampleInterval(value.TotalMilliseconds);
    }

    //-------------------------------------------------------------------------

    UIntPtr V8IsolateProxyImpl::MaxStackUsage::get()
    {
        return (UIntPtr)GetIsolate()->GetMaxStackUsage();
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::MaxStackUsage::set(UIntPtr value)
    {
        GetIsolate()->SetMaxStackUsage(static_cast<size_t>(value));
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::AwaitDebuggerAndPause()
    {
        try
        {
            return GetIsolate()->AwaitDebuggerAndPause();
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8IsolateProxyImpl::Compile(String^ gcDocumentName, String^ gcCode)
    {
        try
        {
            return gcnew V8ScriptImpl(gcDocumentName, GetIsolate()->Compile(StdString(gcDocumentName), StdString(gcCode)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8IsolateProxyImpl::Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes)
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
            auto gcScript = gcnew V8ScriptImpl(gcDocumentName, GetIsolate()->Compile(StdString(gcDocumentName), StdString(gcCode), cacheType, cacheBytes));

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

    V8Script^ V8IsolateProxyImpl::Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted)
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
            auto gcScript = gcnew V8ScriptImpl(gcDocumentName, GetIsolate()->Compile(StdString(gcDocumentName), StdString(gcCode), cacheType, cacheBytes, tempCacheAccepted));

            cacheAccepted = tempCacheAccepted;
            return gcScript;
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8RuntimeHeapInfo^ V8IsolateProxyImpl::GetHeapInfo()
    {
        V8IsolateHeapInfo heapInfo;
        GetIsolate()->GetHeapInfo(heapInfo);

        auto gcHeapInfo = gcnew V8RuntimeHeapInfo();
        gcHeapInfo->TotalHeapSize = heapInfo.GetTotalHeapSize();
        gcHeapInfo->TotalHeapSizeExecutable = heapInfo.GetTotalHeapSizeExecutable();
        gcHeapInfo->TotalPhysicalSize = heapInfo.GetTotalPhysicalSize();
        gcHeapInfo->UsedHeapSize = heapInfo.GetUsedHeapSize();
        gcHeapInfo->HeapSizeLimit = heapInfo.GetHeapSizeLimit();
        return gcHeapInfo;
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::CollectGarbage(bool exhaustive)
    {
        GetIsolate()->CollectGarbage(exhaustive);
    }

    //-------------------------------------------------------------------------

    SharedPtr<V8Isolate> V8IsolateProxyImpl::GetIsolate()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspIsolate == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pspIsolate;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    V8IsolateProxyImpl::~V8IsolateProxyImpl()
    {
        SharedPtr<V8Isolate> spIsolate;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspIsolate != nullptr)
            {
                // hold V8 isolate for destruction outside lock scope
                spIsolate = *m_pspIsolate;
                delete m_pspIsolate;
                m_pspIsolate = nullptr;
            }

        END_LOCK_SCOPE

        if (!spIsolate.IsEmpty())
        {
            GC::SuppressFinalize(this);
        }
    }

    //-------------------------------------------------------------------------

    V8IsolateProxyImpl::!V8IsolateProxyImpl()
    {
        if (m_pspIsolate != nullptr)
        {
            delete m_pspIsolate;
            m_pspIsolate = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    int V8IsolateProxyImpl::AdjustConstraint(int value)
    {
        const int maxValueInMiB = 1024 * 1024;
        if (value > maxValueInMiB)
        {
            const double bytesPerMiB = 1024 * 1024;
            return Convert::ToInt32(Math::Ceiling(Convert::ToDouble(value) / bytesPerMiB));
        }

        return value;
    }

    //-------------------------------------------------------------------------

    ENSURE_INTERNAL_CLASS(V8IsolateProxyImpl)

}}}
