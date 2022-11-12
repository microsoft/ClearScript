// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// forward declarations
//-----------------------------------------------------------------------------

class V8IsolateImpl;

//-----------------------------------------------------------------------------
// V8SharedObjectInfo
//-----------------------------------------------------------------------------

class V8SharedObjectInfo final : public SharedPtrTarget
{
public:

    V8SharedObjectInfo(std::shared_ptr<v8::BackingStore>&& spBackingStore, size_t offset, size_t size, size_t length):
        m_spBackingStore(std::move(spBackingStore)),
        m_Offset(offset),
        m_Size(size),
        m_Length(length)
    {
    }

    const std::shared_ptr<v8::BackingStore>& GetBackingStore() const
    {
        return m_spBackingStore;
    }

    size_t GetOffset() const
    {
        return m_Offset;
    }

    size_t GetSize() const
    {
        return m_Size;
    }

    size_t GetLength() const
    {
        return m_Length;
    }

private:

    std::shared_ptr<v8::BackingStore> m_spBackingStore;
    size_t m_Offset;
    size_t m_Size;
    size_t m_Length;
};

//-----------------------------------------------------------------------------
// V8ObjectHolder
//-----------------------------------------------------------------------------

class V8ObjectHolder: public SharedPtrTarget
{
public:

    virtual V8ObjectHolder* Clone() const = 0;
    virtual bool IsSameIsolate(const SharedPtr<V8IsolateImpl>& spThat) const = 0;
    virtual void* GetObject() const = 0;
    virtual int32_t GetIdentityHash() const = 0;
    virtual const SharedPtr<V8SharedObjectInfo>& GetSharedObjectInfo() const = 0;

    virtual ~V8ObjectHolder() {}
};
