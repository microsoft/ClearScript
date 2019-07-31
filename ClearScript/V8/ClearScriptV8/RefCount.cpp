// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// RefCount::Impl
//-----------------------------------------------------------------------------

class RefCount::Impl final
{
    PROHIBIT_COPY(Impl)

public:

    Impl(size_t count):
        m_Count(count)
    {
    }

    size_t Increment()
    {
        return ++m_Count;
    }

    size_t Decrement()
    {
        return --m_Count;
    }

private:

    std::atomic<size_t> m_Count;
};

//-----------------------------------------------------------------------------
// RefCount implementation
//-----------------------------------------------------------------------------

RefCount::RefCount(size_t count):
    m_pImpl(new Impl(count))
{
}

//-----------------------------------------------------------------------------

size_t RefCount::Increment()
{
    return m_pImpl->Increment();
}

//-----------------------------------------------------------------------------

size_t RefCount::Decrement()
{
    return m_pImpl->Decrement();
}

//-----------------------------------------------------------------------------

RefCount::~RefCount()
{
    delete m_pImpl;
}
