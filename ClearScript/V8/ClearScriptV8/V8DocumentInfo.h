// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

struct V8DocumentInfo final
{
public:

    V8DocumentInfo():
        m_IsModule(false),
        m_pvDocumentInfo(nullptr)
    {
    }

    V8DocumentInfo(const V8DocumentInfo& that):
        m_ResourceName(that.m_ResourceName),
        m_SourceMapUrl(that.m_SourceMapUrl),
        m_UniqueId(that.m_UniqueId),
        m_IsModule(that.m_IsModule),
        m_pvDocumentInfo((that.m_pvDocumentInfo != nullptr) ? HostObjectHelpers::AddRef(that.m_pvDocumentInfo) : nullptr)
    {
    }

    V8DocumentInfo(V8DocumentInfo&& that) noexcept:
        m_ResourceName(std::move(that.m_ResourceName)),
        m_SourceMapUrl(std::move(that.m_SourceMapUrl)),
        m_UniqueId(that.m_UniqueId),
        m_IsModule(that.m_IsModule),
        m_pvDocumentInfo(that.m_pvDocumentInfo)
    {
        that.m_pvDocumentInfo = nullptr;
    }

#ifdef _M_CEE

    explicit V8DocumentInfo(UniqueDocumentInfo^ documentInfo):
        m_ResourceName(MiscHelpers::GetUrlOrPath(documentInfo->Uri, documentInfo->UniqueName)),
        m_SourceMapUrl(MiscHelpers::GetUrlOrPath(documentInfo->SourceMapUri, String::Empty)),
        m_UniqueId(documentInfo->UniqueId)
    {
        m_IsModule = documentInfo->Category == ModuleCategory::Standard;
        m_pvDocumentInfo = V8ProxyHelpers::AddRefHostObject(documentInfo);
    }

#endif // !_M_CEE

    const V8DocumentInfo& operator=(const V8DocumentInfo& that)
    {
        V8DocumentInfo tempInfo(std::move(*this));
        m_ResourceName = that.m_ResourceName;
        m_SourceMapUrl = that.m_SourceMapUrl;
        m_UniqueId = that.m_UniqueId;
        m_IsModule = that.m_IsModule;
        m_pvDocumentInfo = (that.m_pvDocumentInfo != nullptr) ? HostObjectHelpers::AddRef(that.m_pvDocumentInfo) : nullptr;
        return *this;
    }

    const V8DocumentInfo& operator=(V8DocumentInfo&& that) noexcept
    {
        V8DocumentInfo tempInfo(std::move(*this));
        m_ResourceName = std::move(that.m_ResourceName);
        m_SourceMapUrl = std::move(that.m_SourceMapUrl);
        m_UniqueId = that.m_UniqueId;
        m_IsModule = that.m_IsModule;
        m_pvDocumentInfo = that.m_pvDocumentInfo;
        that.m_pvDocumentInfo = nullptr;
        return *this;
    }

    ~V8DocumentInfo()
    {
        if (m_pvDocumentInfo != nullptr)
        {
            HostObjectHelpers::Release(m_pvDocumentInfo);
        }
    }

    const StdString& GetResourceName() const { return m_ResourceName; }
    const StdString& GetSourceMapUrl() const { return m_SourceMapUrl; }
    uint64_t GetUniqueId() const { return m_UniqueId; }
    bool IsModule() const { return m_IsModule; }
    void* GetDocumentInfo() const { return m_pvDocumentInfo; }

private:

    StdString m_ResourceName;
    StdString m_SourceMapUrl;
    uint64_t m_UniqueId {};
    bool m_IsModule;
    void* m_pvDocumentInfo;
};
