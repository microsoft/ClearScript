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

    virtual ~V8ScriptHolder() {}
};
