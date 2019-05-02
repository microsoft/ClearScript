// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// IV8Entity
//-----------------------------------------------------------------------------

struct IV8Entity
{
    virtual StdString CreateStdString(v8::Local<v8::Value> hValue) = 0;
};
