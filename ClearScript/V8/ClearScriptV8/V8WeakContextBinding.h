// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8WeakContextBinding
//-----------------------------------------------------------------------------

class V8WeakContextBinding final: public SharedPtrTarget
{
public:

    V8WeakContextBinding(V8IsolateImpl* pIsolateImpl, V8ContextImpl* pContextImpl):
        m_wrIsolate(pIsolateImpl->CreateWeakRef()),
        m_IsolateName(pIsolateImpl->GetName()),
        m_wrContext(pContextImpl->CreateWeakRef()),
        m_ContextName(pContextImpl->GetName())
    {
    }

    SharedPtr<V8IsolateImpl> GetIsolateImpl() const
    {
        SharedPtr<V8IsolateImpl> spIsolateImpl;
        if (TryGetIsolateImpl(spIsolateImpl))
        {
            return spIsolateImpl;
        }

        throw V8Exception(V8Exception::Type::General, m_IsolateName, StdString(L"The V8 runtime has been destroyed"), false /*executionStarted*/);
    }

    bool TryGetIsolateImpl(SharedPtr<V8IsolateImpl>& spIsolateImpl) const
    {
        auto spIsolate = m_wrIsolate.GetTarget();
        if (!spIsolate.IsEmpty())
        {
            spIsolateImpl = static_cast<V8IsolateImpl*>(spIsolate.GetRawPtr());
            return true;
        }

        return false;
    }

    SharedPtr<V8ContextImpl> GetContextImpl() const
    {
        SharedPtr<V8ContextImpl> spContextImpl;
        if (TryGetContextImpl(spContextImpl))
        {
            return spContextImpl;
        }

        throw V8Exception(V8Exception::Type::General, m_ContextName, StdString(L"The V8 script engine has been destroyed"), false /*executionStarted*/);
    }

    bool TryGetContextImpl(SharedPtr<V8ContextImpl>& spContextImpl) const
    {
        auto spContext = m_wrContext.GetTarget();
        if (!spContext.IsEmpty())
        {
            spContextImpl = static_cast<V8ContextImpl*>(spContext.GetRawPtr());
            return true;
        }

        return false;
    }

private:

    WeakRef<V8Isolate> m_wrIsolate;
    StdString m_IsolateName;
    WeakRef<V8Context> m_wrContext;
    StdString m_ContextName;
};
