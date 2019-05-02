// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8DebugListenerImpl
    //-------------------------------------------------------------------------

    private ref class V8DebugListenerImpl sealed : IV8DebugListener
    {
    public:

        V8DebugListenerImpl(HostObjectHelpers::DebugCallback&& callback);

        virtual void ConnectClient();
        virtual void SendCommand(String^ gcCommand);
        virtual void DisconnectClient();

        ~V8DebugListenerImpl();
        !V8DebugListenerImpl();

    private:

        bool TryGetCallback(SharedPtr<HostObjectHelpers::DebugCallback>& spCallback);

        Object^ m_gcLock;
        SharedPtr<HostObjectHelpers::DebugCallback>* m_pspCallback;
    };

}}}
