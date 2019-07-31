// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8IsolateProxyImpl
    //-------------------------------------------------------------------------

    private ref class V8IsolateProxyImpl sealed : V8IsolateProxy, IV8EntityProxy
    {
    public:

        V8IsolateProxyImpl(String^ gcName, V8RuntimeConstraints^ gcConstraints, V8RuntimeFlags flags, Int32 debugPort);

        property UIntPtr MaxHeapSize
        {
            virtual UIntPtr get() override;
            virtual void set(UIntPtr value) override;
        }

        property TimeSpan HeapSizeSampleInterval
        {
            virtual TimeSpan get() override;
            virtual void set(TimeSpan value) override;
        }

        property UIntPtr MaxStackUsage
        {
            virtual UIntPtr get() override;
            virtual void set(UIntPtr value) override;
        }

        virtual void AwaitDebuggerAndPause() override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode) override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes) override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted) override;
        virtual V8RuntimeHeapInfo^ GetHeapInfo() override;
        virtual V8Runtime::Statistics^ GetStatistics() override;
        virtual void CollectGarbage(bool exhaustive) override;
        virtual bool BeginCpuProfile(String^ gcName, V8CpuProfileFlags flags) override;
        virtual V8CpuProfile^ EndCpuProfile(String^ gcName) override;
        virtual void CollectCpuProfileSample() override;

        property UInt32 CpuProfileSampleInterval
        {
            virtual UInt32 get() override;
            virtual void set(UInt32 value) override;
        }

        virtual String^ CreateManagedString(v8::Local<v8::Value> hValue);
        V8Context* CreateContext(const StdString& name, const V8Context::Options& options);

        ~V8IsolateProxyImpl();
        !V8IsolateProxyImpl();

        static V8Isolate::CpuProfileCallbackT* GetCpuProfileCallback();

    private:

        SharedPtr<V8Isolate> GetIsolate();
        static int AdjustConstraint(int value);

        Object^ m_gcLock;
        SharedPtr<V8Isolate>* m_pspIsolate;
    };

}}}
