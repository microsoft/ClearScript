// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// HostObjectHolderImpl implementation
//-----------------------------------------------------------------------------

HostObjectHolderImpl::HostObjectHolderImpl(void* pvObject, uint8_t subtype, uint16_t flags):
    m_pvObject(pvObject),
    m_Subtype(subtype),
    m_Flags(flags)
{
}

//-----------------------------------------------------------------------------

HostObjectHolderImpl* HostObjectHolderImpl::Clone() const
{
    return new HostObjectHolderImpl(HostObjectUtil::AddRef(m_pvObject), m_Subtype, m_Flags);
}

//-----------------------------------------------------------------------------

uint8_t HostObjectHolderImpl::GetSubtype() const
{
    return m_Subtype;
}

//-----------------------------------------------------------------------------

uint16_t HostObjectHolderImpl::GetFlags() const
{
    return m_Flags;
}

//-----------------------------------------------------------------------------

void* HostObjectHolderImpl::GetObject() const
{
    return m_pvObject;
}

//-----------------------------------------------------------------------------

HostObjectHolderImpl::~HostObjectHolderImpl()
{
    HostObjectUtil::Release(m_pvObject);
}
