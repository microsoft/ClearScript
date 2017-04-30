// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HostObjectHolderImpl
//-----------------------------------------------------------------------------

class HostObjectHolderImpl: public HostObjectHolder
{
    PROHIBIT_COPY(HostObjectHolderImpl)

public:

    explicit HostObjectHolderImpl(void* pvObject);

    HostObjectHolderImpl* Clone() const;
    void* GetObject() const;

    ~HostObjectHolderImpl();

private:

    void* m_pvObject;
};
