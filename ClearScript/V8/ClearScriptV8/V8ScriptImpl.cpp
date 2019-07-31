// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Managed.h"

namespace Microsoft {
namespace ClearScript {
namespace V8 {

    //-------------------------------------------------------------------------
    // V8ScriptImpl implementation
    //-------------------------------------------------------------------------

    V8ScriptImpl::V8ScriptImpl(ClearScript::UniqueDocumentInfo^ documentInfo, V8ScriptHolder* pHolder):
        V8Script(documentInfo, (UIntPtr)pHolder->GetCodeDigest()),
        m_gcLock(gcnew Object),
        m_pspHolder(new SharedPtr<V8ScriptHolder>(pHolder))
    {
    }

    //-------------------------------------------------------------------------

    SharedPtr<V8ScriptHolder> V8ScriptImpl::GetHolder()
    {
        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspHolder == nullptr)
            {
                throw gcnew ObjectDisposedException(ToString());
            }

            return *m_pspHolder;

        END_LOCK_SCOPE
    }

    //-------------------------------------------------------------------------

    V8ScriptImpl::~V8ScriptImpl()
    {
        SharedPtr<V8ScriptHolder> spHolder;

        BEGIN_LOCK_SCOPE(m_gcLock)

            if (m_pspHolder != nullptr)
            {
                // hold V8 script holder for destruction outside lock scope
                spHolder = *m_pspHolder;
                delete m_pspHolder;
                m_pspHolder = nullptr;
            }

        END_LOCK_SCOPE

        if (!spHolder.IsEmpty())
        {
            GC::SuppressFinalize(this);
        }
    }

    //-------------------------------------------------------------------------

    V8ScriptImpl::!V8ScriptImpl()
    {
        if (m_pspHolder != nullptr)
        {
            delete m_pspHolder;
            m_pspHolder = nullptr;
        }
    }

}}}
