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

    virtual ~HostObjectHolder() {}
};
