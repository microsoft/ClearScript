// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8IsolateProxyImpl
    //-------------------------------------------------------------------------

    private ref class V8IsolateProxyImpl : V8IsolateProxy
    {
    public:

        V8IsolateProxyImpl(String^ gcName, V8RuntimeConstraints^ gcConstraints, Boolean enableDebugging, Int32 debugPort);

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

        virtual V8Script^ Compile(String^ gcDocumentName, String^ gcCode) override;
        virtual V8Script^ Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, [Out] array<Byte>^% gcCacheBytes) override;
        virtual V8Script^ Compile(String^ gcDocumentName, String^ gcCode, V8CacheKind cacheKind, array<Byte>^ gcCacheBytes, [Out] Boolean% cacheAccepted) override;
        virtual V8RuntimeHeapInfo^ GetHeapInfo() override;
        virtual void CollectGarbage(bool exhaustive) override;

        SharedPtr<V8Isolate> GetIsolate();

        ~V8IsolateProxyImpl();
        !V8IsolateProxyImpl();

    private:

        static int AdjustConstraint(int value);

        Object^ m_gcLock;
        SharedPtr<V8Isolate>* m_pspIsolate;
    };

}}}
