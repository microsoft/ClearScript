// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ScriptHolderImpl
//-----------------------------------------------------------------------------

class V8ScriptHolderImpl final: public V8ScriptHolder
{
    PROHIBIT_COPY(V8ScriptHolderImpl)

public:

    V8ScriptHolderImpl(V8WeakContextBinding* pBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest);
    V8ScriptHolderImpl(V8WeakContextBinding* pBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest, StdString&& code);

    virtual V8ScriptHolderImpl* Clone() const override;
    virtual bool IsSameIsolate(void* pvIsolate) const override;
    virtual void* GetScript() const override;
    virtual const V8DocumentInfo& GetDocumentInfo() const override;
    virtual size_t GetCodeDigest() const override;
    virtual const StdString& GetCode() const override;

    virtual const std::vector<uint8_t>& GetCacheBytes() const override;
    virtual void SetCacheBytes(const std::vector<uint8_t>& cacheBytes) override;

    ~V8ScriptHolderImpl();

private:

    V8ScriptHolderImpl(V8WeakContextBinding* pBinding, void* pvScript, const V8DocumentInfo& documentInfo, size_t codeDigest, const StdString& code, const std::vector<uint8_t>& cacheBytes);

    SharedPtr<V8WeakContextBinding> m_spBinding;
    void* m_pvScript;
    V8DocumentInfo m_DocumentInfo;
    size_t m_CodeDigest;
    StdString m_Code;
    std::vector<uint8_t> m_CacheBytes;
};
