// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8ContextProxyImpl
    //-------------------------------------------------------------------------

    private ref class V8ContextProxyImpl sealed : V8ContextProxy, IV8EntityProxy
    {
    public:

        V8ContextProxyImpl(V8IsolateProxy^ gcIsolateProxy, String^ gcName, V8ScriptEngineFlags flags, Int32 debugPort);

        property UIntPtr MaxRuntimeHeapSize
        {
            virtual UIntPtr get() override;
            virtual void set(UIntPtr value) override;
        }

        property TimeSpan RuntimeHeapSizeSampleInterval
        {
            virtual TimeSpan get() override;
            virtual void set(TimeSpan value) override;
        }

        property UIntPtr MaxRuntimeStackUsage
        {
            virtual UIntPtr get() override;
            virtual void set(UIntPtr value) override;
        }

        virtual void InvokeWithLock(Action^ gcAction) override;
        virtual Object^ GetRootItem() override;
        virtual void AddGlobalItem(String^ gcName, Object^ gcItem, Boolean globalMembers) override;
        virtual void AwaitDebuggerAndPause() override;
        virtual Object^ Execute(UniqueDocumentInfo^ documentInfo, String^ gcCode, Boolean evaluate) override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode) override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes) override;
        virtual V8Script^ Compile(UniqueDocumentInfo^ documentInfo, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted) override;
        virtual Object^ Execute(V8Script^ gcScript, Boolean evaluate) override;
        virtual void Interrupt() override;
        virtual V8RuntimeHeapInfo^ GetRuntimeHeapInfo() override;
        virtual V8Runtime::Statistics^ GetRuntimeStatistics() override;
        virtual V8ScriptEngine::Statistics^ GetStatistics() override;
        virtual void CollectGarbage(bool exhaustive) override;
        virtual void OnAccessSettingsChanged() override;
        virtual bool BeginCpuProfile(String^ gcName, V8CpuProfileFlags flags) override;
        virtual V8CpuProfile^ EndCpuProfile(String^ gcName) override;
        virtual void CollectCpuProfileSample() override;

        property UInt32 CpuProfileSampleInterval
        {
            virtual UInt32 get() override;
            virtual void set(UInt32 value) override;
        }

        virtual String^ CreateManagedString(v8::Local<v8::Value> hValue);

        ~V8ContextProxyImpl();
        !V8ContextProxyImpl();

        static V8Value ImportValue(Object^ gcObject);
        static Object^ ExportValue(const V8Value& value);

    private:

        SharedPtr<V8Context> GetContext();

        Object^ m_gcLock;
        SharedPtr<V8Context>* m_pspContext;
    };

}}}
