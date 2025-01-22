// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8ScriptHolderImpl implementation
//-----------------------------------------------------------------------------

V8ScriptHolderImpl::V8ScriptHolderImpl(const SharedPtr<V8WeakContextBinding>& spBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest):
    m_spBinding(spBinding),
    m_pvScript(pvScript),
    m_DocumentInfo(documentInfo),
    m_CodeDigest(codeDigest)
{
}

//-----------------------------------------------------------------------------

V8ScriptHolderImpl::V8ScriptHolderImpl(const SharedPtr<V8WeakContextBinding>& spBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest, StdString&& code):
    m_spBinding(spBinding),
    m_pvScript(pvScript),
    m_DocumentInfo(documentInfo),
    m_CodeDigest(codeDigest),
    m_Code(std::move(code))
{
}

//-----------------------------------------------------------------------------

V8ScriptHolderImpl::V8ScriptHolderImpl(const SharedPtr<V8WeakContextBinding>& spBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest, const StdString& code, const std::vector<uint8_t>& cacheBytes):
    m_spBinding(spBinding),
    m_pvScript(pvScript),
    m_DocumentInfo(documentInfo),
    m_CodeDigest(codeDigest),
    m_Code(code),
    m_CacheBytes(cacheBytes)
{
}

//-----------------------------------------------------------------------------

V8ScriptHolderImpl* V8ScriptHolderImpl::Clone() const
{
    return new V8ScriptHolderImpl(m_spBinding, m_spBinding->GetIsolateImpl()->AddRefV8Script(m_pvScript), m_DocumentInfo, m_CodeDigest, m_Code, m_CacheBytes);
}

//-----------------------------------------------------------------------------

bool V8ScriptHolderImpl::IsSameIsolate(const SharedPtr<V8IsolateImpl>& spThat) const
{
    SharedPtr<V8IsolateImpl> spIsolateImpl;
    if (m_spBinding->TryGetIsolateImpl(spIsolateImpl))
    {
        return spIsolateImpl == spThat;
    }

    return false;
}

//-----------------------------------------------------------------------------

void* V8ScriptHolderImpl::GetScript() const
{
    return m_pvScript;
}

//-----------------------------------------------------------------------------

const V8DocumentInfo& V8ScriptHolderImpl::GetDocumentInfo() const
{
    return m_DocumentInfo;
}

//-----------------------------------------------------------------------------

size_t V8ScriptHolderImpl::GetCodeDigest() const
{
    return m_CodeDigest;
}

//-----------------------------------------------------------------------------

const StdString& V8ScriptHolderImpl::GetCode() const
{
    return m_Code;
}

//-----------------------------------------------------------------------------

const std::vector<uint8_t>& V8ScriptHolderImpl::GetCacheBytes() const
{
    return m_CacheBytes;
}

//-----------------------------------------------------------------------------

void V8ScriptHolderImpl::SetCacheBytes(const std::vector<uint8_t>& cacheBytes)
{
    m_CacheBytes = cacheBytes;
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
