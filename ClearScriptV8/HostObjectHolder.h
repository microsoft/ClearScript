// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// HostObjectHolder
//-----------------------------------------------------------------------------

class HostObjectHolder
{
public:

    virtual HostObjectHolder* Clone() const = 0;

    virtual void* GetObject() const = 0;
    virtual uint8_t GetSubtype() const = 0;
    virtual uint16_t GetFlags() const = 0;

    virtual ~HostObjectHolder() {}
};
