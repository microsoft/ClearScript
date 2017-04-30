// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8DebugListenerImpl implementation
    //-------------------------------------------------------------------------

    V8DebugListenerImpl::V8DebugListenerImpl(HostObjectHelpers::DebugCallback&& callback):
        m_gcLock(gcnew Object),
        m_pspCallback(new SharedPtr<HostObjectHelpers::DebugCallback>(new HostObjectHelpers::DebugCallback(std::move(callback))))
    {
    }

    //-------------------------------------------------------------------------

    bool V8DebugListenerImpl::SendDebugCommand(String^ gcCommand)
    {
        SharedPtr<HostObjectHelpers::DebugCallback> spCallback;
        if (TryGetCallback(spCallback))
        {
            StdString command(gcCommand);
            (*spCallback)(HostObjectHelpers::DebugDirective::SendDebugCommand, &command);
            return true;
        }

        return false;
    }

    //-------------------------------------------------------------------------

    void V8DebugListenerImpl::DispatchDebugMessages()
    {
        SharedPtr<HostObjectHelpers::DebugCallback> spCallback;
        if (TryGetCallback(spCallback))
        {
            (*spCallback)(HostObjectHelpers::DebugDirective::DispatchDebugMessages, nullptr);
        }
    }

    //-------------------------------------------------------------------------

    V8DebugListenerImpl::~V8DebugListenerImpl()
    {
        SharedPtr<HostObjectHelpers::DebugCallback> spCallback;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspCallback != nullptr)
            {
                // hold callback for destruction outside lock scope
                spCallback = *m_pspCallback;
                delete m_pspCallback;
                m_pspCallback = nullptr;
            }

        END_LOCK_SCOPE

        if (!spCallback.IsEmpty())
        {
            GC::SuppressFinalize(this);
        }
    }

    //-------------------------------------------------------------------------

    V8DebugListenerImpl::!V8DebugListenerImpl()
    {
        if (m_pspCallback != nullptr)
        {
            delete m_pspCallback;
            m_pspCallback = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    bool V8DebugListenerImpl::TryGetCallback(SharedPtr<HostObjectHelpers::DebugCallback>& spCallback)
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspCallback == nullptr)
            {
                return false;
            }

            spCallback = *m_pspCallback;
            return true;

        END_LOCK_SCOPE
    }

}}}
