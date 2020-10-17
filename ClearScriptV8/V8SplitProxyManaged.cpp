// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// V8SplitProxyManaged implementation
//-----------------------------------------------------------------------------

void** V8SplitProxyManaged::ms_pMethodTable = nullptr;
thread_local HostException* V8SplitProxyManaged::ms_pHostException = nullptr;

//-----------------------------------------------------------------------------

bool V8SplitProxyManaged::HasMethodTable() noexcept
{
    return ms_pMethodTable != nullptr;
}

//-----------------------------------------------------------------------------

void V8SplitProxyManaged::SetMethodTable(void** pMethodTable) noexcept
{
    ms_pMethodTable = pMethodTable;
}

//-----------------------------------------------------------------------------

void V8SplitProxyManaged::SetHostException(HostException&& exception) noexcept
{
    _ASSERTE(ms_pHostException == nullptr);
    ms_pHostException = new HostException(std::move(exception));
}

//-----------------------------------------------------------------------------

void V8SplitProxyManaged::ThrowHostException()
{
    if (ms_pHostException != nullptr)
    {
        HostException exception(std::move(*ms_pHostException));
        delete ms_pHostException;
        throw exception;
    }
}
