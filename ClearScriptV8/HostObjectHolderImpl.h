// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HostObjectHolderImpl
//-----------------------------------------------------------------------------

class HostObjectHolderImpl final: public HostObjectHolder
{
    PROHIBIT_COPY(HostObjectHolderImpl)

public:

    HostObjectHolderImpl(void* pvObject, uint8_t subtype, uint16_t flags);

    virtual HostObjectHolderImpl* Clone() const override;

    virtual void* GetObject() const override;
    virtual uint8_t GetSubtype() const override;
    virtual uint16_t GetFlags() const override;

    ~HostObjectHolderImpl();

private:

    void* m_pvObject;
    uint8_t m_Subtype;
    uint16_t m_Flags;
};
