// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8WeakContextBinding
//-----------------------------------------------------------------------------

class V8WeakContextBinding final: public SharedPtrTarget
{
public:

    V8WeakContextBinding(const SharedPtr<V8IsolateImpl>& spIsolateImpl, V8ContextImpl* pContextImpl):
        m_wrIsolate(spIsolateImpl->CreateWeakRef()),
        m_IsolateName(spIsolateImpl->GetName()),
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

        throw V8Exception(V8Exception::Type::General, m_IsolateName, StdString(SL("The V8 runtime has been destroyed")), false);
    }

    bool TryGetIsolateImpl(SharedPtr<V8IsolateImpl>& spIsolateImpl) const
    {
        auto spIsolate = m_wrIsolate.GetTarget();
        if (!spIsolate.IsEmpty())
        {
            spIsolateImpl = spIsolate.CastTo<V8IsolateImpl>();
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

        throw V8Exception(V8Exception::Type::General, m_ContextName, StdString(SL("The V8 script engine has been destroyed")), false);
    }

    bool TryGetContextImpl(SharedPtr<V8ContextImpl>& spContextImpl) const
    {
        auto spContext = m_wrContext.GetTarget();
        if (!spContext.IsEmpty())
        {
            spContextImpl = spContext.CastTo<V8ContextImpl>();
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
