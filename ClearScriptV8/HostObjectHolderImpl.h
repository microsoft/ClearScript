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

    explicit HostObjectHolderImpl(void* pvObject);

    virtual HostObjectHolderImpl* Clone() const override;
    virtual void* GetObject() const override;

    ~HostObjectHolderImpl();

private:

    void* m_pvObject;
};
