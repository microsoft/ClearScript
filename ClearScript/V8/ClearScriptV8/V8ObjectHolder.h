// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8ObjectHolder
//-----------------------------------------------------------------------------

class V8ObjectHolder
{
public:

    virtual V8ObjectHolder* Clone() const = 0;
    virtual void* GetObject() const = 0;

    virtual ~V8ObjectHolder() {}
};
