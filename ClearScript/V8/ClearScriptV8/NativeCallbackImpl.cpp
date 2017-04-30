// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // NativeCallbackImpl implementation
    //-------------------------------------------------------------------------

    NativeCallbackImpl::NativeCallbackImpl(HostObjectHelpers::NativeCallback&& callback):
        m_gcLock(gcnew Object),
        m_pspCallback(new SharedPtr<HostObjectHelpers::NativeCallback>(new HostObjectHelpers::NativeCallback(std::move(callback))))
    {
    }

    //-------------------------------------------------------------------------

    void NativeCallbackImpl::Invoke()
    {
        SharedPtr<HostObjectHelpers::NativeCallback> spCallback;
        if (TryGetCallback(spCallback))
        {
            try
            {
                (*spCallback)();
            }
            catch (const std::exception&)
            {
            }
            catch (...)
            {
            }
        }
    }

    //-------------------------------------------------------------------------

    NativeCallbackImpl::~NativeCallbackImpl()
    {
        SharedPtr<HostObjectHelpers::NativeCallback> spCallback;

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

    NativeCallbackImpl::!NativeCallbackImpl()
    {
        if (m_pspCallback != nullptr)
        {
            delete m_pspCallback;
            m_pspCallback = nullptr;
        }
    }

    //-------------------------------------------------------------------------

    bool NativeCallbackImpl::TryGetCallback(SharedPtr<HostObjectHelpers::NativeCallback>& spCallback)
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
