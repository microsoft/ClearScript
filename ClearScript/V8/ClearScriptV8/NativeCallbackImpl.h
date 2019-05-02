// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // NativeCallbackImpl
    //-------------------------------------------------------------------------

    private ref class NativeCallbackImpl sealed : INativeCallback
    {
    public:

        NativeCallbackImpl(HostObjectHelpers::NativeCallback&& callback);

        virtual void Invoke();

        ~NativeCallbackImpl();
        !NativeCallbackImpl();

    private:

        bool TryGetCallback(SharedPtr<HostObjectHelpers::NativeCallback>& spCallback);

        Object^ m_gcLock;
        SharedPtr<HostObjectHelpers::NativeCallback>* m_pspCallback;
    };

}}}
