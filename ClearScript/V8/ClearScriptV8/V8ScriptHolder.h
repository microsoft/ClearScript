// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ScriptHolder
//-----------------------------------------------------------------------------

class V8ScriptHolder
{
public:

    virtual V8ScriptHolder* Clone() const = 0;
    virtual bool IsSameIsolate(void* pvIsolate) const = 0;
    virtual void* GetScript() const = 0;
    virtual const V8DocumentInfo& GetDocumentInfo() const = 0;
    virtual size_t GetCodeDigest() const = 0;
    virtual const StdString& GetCode() const = 0;

    virtual const std::vector<uint8_t>& GetCacheBytes() const = 0;
    virtual void SetCacheBytes(const std::vector<uint8_t>& cacheBytes) = 0;

    virtual ~V8ScriptHolder() {}
};
