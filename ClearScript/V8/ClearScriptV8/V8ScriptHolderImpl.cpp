// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8ScriptHolderImpl implementation
//-----------------------------------------------------------------------------

V8ScriptHolderImpl::V8ScriptHolderImpl(V8WeakContextBinding* pBinding, void* pvScript):
    m_spBinding(pBinding),
    m_pvScript(pvScript)
{
}

//-----------------------------------------------------------------------------

V8ScriptHolderImpl* V8ScriptHolderImpl::Clone() const
{
    return new V8ScriptHolderImpl(m_spBinding, m_spBinding->GetIsolateImpl()->AddRefV8Script(m_pvScript));
}

//-----------------------------------------------------------------------------

bool V8ScriptHolderImpl::IsSameIsolate(void* pvIsolate) const
{
    SharedPtr<V8IsolateImpl> spIsolateImpl;
    if (m_spBinding->TryGetIsolateImpl(spIsolateImpl))
    {
        return spIsolateImpl.GetRawPtr() == pvIsolate;
    }

    return false;
}

//-----------------------------------------------------------------------------

void* V8ScriptHolderImpl::GetScript() const
{
    return m_pvScript;
}

//-----------------------------------------------------------------------------

V8ScriptHolderImpl::~V8ScriptHolderImpl()
{
    SharedPtr<V8IsolateImpl> spIsolateImpl;
    if (m_spBinding->TryGetIsolateImpl(spIsolateImpl))
    {
        spIsolateImpl->ReleaseV8Script(m_pvScript);
    }
}
