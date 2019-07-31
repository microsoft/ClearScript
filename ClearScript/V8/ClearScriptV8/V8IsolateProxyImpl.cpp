// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // local helper functions
    //-------------------------------------------------------------------------

    static V8CpuProfile::Node^ CreateCpuProfileNode(const v8::CpuProfileNode& node);

    //-------------------------------------------------------------------------

    static String^ CreateManagedString(const char* pValue)
    {
        if (pValue != nullptr)
        {
            return gcnew String(pValue, 0, static_cast<int>(strlen(pValue)), Encoding::UTF8);
        }

        return nullptr;
    }

    //-------------------------------------------------------------------------

    static V8CpuProfile::Node^ CreateCpuProfileNode(const v8::CpuProfileNode& node)
    {
        auto gcNode = gcnew V8CpuProfile::Node;

        gcNode->NodeId = node.GetNodeId();
        gcNode->ScriptId = node.GetScriptId();

        gcNode->ScriptName = CreateManagedString(node.GetScriptResourceNameStr());
        gcNode->FunctionName = CreateManagedString(node.GetFunctionNameStr());
        gcNode->BailoutReason = CreateManagedString(node.GetBailoutReason());

        gcNode->LineNumber = node.GetLineNumber();
        gcNode->ColumnNumber = node.GetColumnNumber();
        gcNode->HitCount = node.GetHitCount();

        auto hitLineCount = node.GetHitLineCount();
        if (hitLineCount > 0)
        {
            std::vector<v8::CpuProfileNode::LineTick> hitLines(hitLineCount);
            if (node.GetLineTicks(&hitLines[0], static_cast<unsigned>(hitLines.size())))
            {
                auto gcHitLines = gcnew array<V8CpuProfile::Node::HitLine>(hitLineCount);

                for (auto index = 0U; index < hitLineCount; ++index)
                {
                    gcHitLines[index].LineNumber = hitLines[index].line;
                    gcHitLines[index].HitCount = hitLines[index].hit_count;
                }

                gcNode->HitLines = gcnew ReadOnlyCollection<V8CpuProfile::Node::HitLine>(dynamic_cast<IList<V8CpuProfile::Node::HitLine>^>(gcHitLines));
            }
        }

        auto childCount = node.GetChildrenCount();
        if (childCount > 0)
        {
            auto gcChildNodes = gcnew List<V8CpuProfile::Node^>(childCount);

            for (auto index = 0; index < childCount; ++index)
            {
                auto pChildNode = node.GetChild(index);
                if (pChildNode != nullptr)
                {
                    gcChildNodes->Add(CreateCpuProfileNode(*pChildNode));
                }
            }

            if (gcChildNodes->Count > 0)
            {
                gcNode->ChildNodes = gcnew ReadOnlyCollection<V8CpuProfile::Node^>(gcChildNodes);
            }
        }

        return gcNode;
    }

    //-------------------------------------------------------------------------

    static V8CpuProfile::Sample^ CreateCpuProfileSample(V8CpuProfile::Node^ gcNode, uint64_t time)
    {
        auto gcSample = gcnew V8CpuProfile::Sample;

        gcSample->Node = gcNode;
        gcSample->Timestamp = time;

        return gcSample;
    }

    //-------------------------------------------------------------------------

    static void CpuProfileCallback(const v8::CpuProfile& profile, void* pvArg)
    {
        auto pContext = static_cast<Tuple<IV8EntityProxy^, V8CpuProfile^>^*>(pvArg);
        auto gcProxy = (*pContext)->Item1;
        auto gcProfile = (*pContext)->Item2;

        gcProfile->Name = gcProxy->CreateManagedString(profile.GetTitle());
        gcProfile->StartTimestamp = profile.GetStartTime();
        gcProfile->EndTimestamp = profile.GetEndTime();

        auto pNode = profile.GetTopDownRoot();
        if (pNode != nullptr)
        {
            gcProfile->RootNode = CreateCpuProfileNode(*pNode);
        }

        auto sampleCount = profile.GetSamplesCount();
        if (sampleCount > 0)
        {
            auto gcSamples = gcnew List<V8CpuProfile::Sample^>(sampleCount);
            
            for (auto index = 0; index < sampleCount; ++index)
            {
                pNode = profile.GetSample(index);
                if (pNode != nullptr)
                {
                    auto gcNode = gcProfile->FindNode(pNode->GetNodeId());
                    _ASSERTE(gcNode != nullptr);

                    if (gcNode != nullptr)
                    {
                        gcSamples->Add(CreateCpuProfileSample(gcNode, profile.GetSampleTimestamp(index)));
                    }
                }
            }

            if (gcSamples->Count > 0)
            {
                gcProfile->Samples = gcnew ReadOnlyCollection<V8CpuProfile::Sample^>(gcSamples);
            }
        }
    }

    //-------------------------------------------------------------------------
    // V8IsolateProxyImpl implementation
    //-------------------------------------------------------------------------

    V8IsolateProxyImpl::V8IsolateProxyImpl(String^ gcName, V8RuntimeConstraints^ gcConstraints, V8RuntimeFlags flags, Int32 debugPort):
        m_gcLock(gcnew Object)
    {
        const v8::ResourceConstraints* pConstraints = nullptr;

        v8::ResourceConstraints constraints;
        if (gcConstraints != nullptr)
        {
            constraints.set_max_semi_space_size_in_kb(AdjustConstraint(gcConstraints->MaxNewSpaceSize) * 1024);
            constraints.set_max_old_space_size(AdjustConstraint(gcConstraints->MaxOldSpaceSize));
            pConstraints = &constraints;
        }

        V8Isolate::Options options;
        options.EnableDebugging = flags.HasFlag(V8RuntimeFlags::EnableDebugging);
        options.EnableRemoteDebugging = flags.HasFlag(V8RuntimeFlags::EnableRemoteDebugging);
        options.EnableDynamicModuleImports = flags.HasFlag(V8RuntimeFlags::EnableDynamicModuleImports);
        options.DebugPort = debugPort;

        try
        {
            m_pspIsolate = new SharedPtr<V8Isolate>(V8Isolate::Create(StdString(gcName), pConstraints, options));
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

    V8Script^ V8IsolateProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode)
    {
        try
        {
            return gcnew V8ScriptImpl(documentInfo, GetIsolate()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode)));
        }
        catch (const V8Exception& exception)
        {
            exception.ThrowScriptEngineException();
        }
    }

    //-------------------------------------------------------------------------

    V8Script^ V8IsolateProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes)
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
            auto gcScript = gcnew V8ScriptImpl(documentInfo, GetIsolate()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode), cacheType, cacheBytes));

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

    V8Script^ V8IsolateProxyImpl::Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted)
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
            auto gcScript = gcnew V8ScriptImpl(documentInfo, GetIsolate()->Compile(V8DocumentInfo(documentInfo), StdString(gcCode), cacheType, cacheBytes, tempCacheAccepted));

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

    V8RuntimeHeapInfo^ V8IsolateProxyImpl::GetHeapInfo()
    {
        v8::HeapStatistics heapStatistics;
        GetIsolate()->GetHeapStatistics(heapStatistics);

        auto gcHeapInfo = gcnew V8RuntimeHeapInfo();
        gcHeapInfo->TotalHeapSize = heapStatistics.total_heap_size();
        gcHeapInfo->TotalHeapSizeExecutable = heapStatistics.total_heap_size_executable();
        gcHeapInfo->TotalPhysicalSize = heapStatistics.total_physical_size();
        gcHeapInfo->UsedHeapSize = heapStatistics.used_heap_size();
        gcHeapInfo->HeapSizeLimit = heapStatistics.heap_size_limit();
        return gcHeapInfo;
    }

    //-------------------------------------------------------------------------

    V8Runtime::Statistics^ V8IsolateProxyImpl::GetStatistics()
    {
        auto statistics = GetIsolate()->GetStatistics();

        auto gcStatistics = gcnew V8Runtime::Statistics;
        gcStatistics->ScriptCount = statistics.ScriptCount;
        gcStatistics->ScriptCacheSize = statistics.ScriptCacheSize;
        gcStatistics->ModuleCount = statistics.ModuleCount;
        return gcStatistics;
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::CollectGarbage(bool exhaustive)
    {
        GetIsolate()->CollectGarbage(exhaustive);
    }

    //-------------------------------------------------------------------------

    bool V8IsolateProxyImpl::BeginCpuProfile(String^ gcName, V8CpuProfileFlags flags)
    {
        return GetIsolate()->BeginCpuProfile(StdString(gcName), v8::kLeafNodeLineNumbers, flags.HasFlag(V8CpuProfileFlags::EnableSampleCollection));
    }

    //-------------------------------------------------------------------------

    V8CpuProfile^ V8IsolateProxyImpl::EndCpuProfile(String^ gcName)
    {
        auto gcContext = Tuple::Create<IV8EntityProxy^, V8CpuProfile^>(this, gcnew V8CpuProfile);
        return GetIsolate()->EndCpuProfile(StdString(gcName), CpuProfileCallback, &gcContext) ? gcContext->Item2 : nullptr;
    }
    
    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::CollectCpuProfileSample()
    {
        return GetIsolate()->CollectCpuProfileSample();
    }

    //-------------------------------------------------------------------------

    UInt32 V8IsolateProxyImpl::CpuProfileSampleInterval::get()
    {
        return GetIsolate()->GetCpuProfileSampleInterval();
    }

    //-------------------------------------------------------------------------

    void V8IsolateProxyImpl::CpuProfileSampleInterval::set(UInt32 value)
    {
        GetIsolate()->SetCpuProfileSampleInterval(value);
    }

    //-------------------------------------------------------------------------

    String^ V8IsolateProxyImpl::CreateManagedString(v8::Local<v8::Value> hValue)
    {
        return GetIsolate()->CreateStdString(hValue).ToManagedString();
    }

    //-------------------------------------------------------------------------

    V8Context* V8IsolateProxyImpl::CreateContext(const StdString& name, const V8Context::Options& options)
    {
        return V8Context::Create(GetIsolate(), name, options);
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

    V8Isolate::CpuProfileCallbackT* V8IsolateProxyImpl::GetCpuProfileCallback()
    {
        return CpuProfileCallback;
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
