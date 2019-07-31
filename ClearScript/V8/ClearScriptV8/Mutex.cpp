// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// SimpleMutex implementation
//-----------------------------------------------------------------------------

SimpleMutex::SimpleMutex():
    m_pImpl(new Impl)
{
}

//-----------------------------------------------------------------------------

void SimpleMutex::Lock()
{
    m_pImpl->lock();
}

//-----------------------------------------------------------------------------

bool SimpleMutex::TryLock()
{
    return m_pImpl->try_lock();
}

//-----------------------------------------------------------------------------

void SimpleMutex::Unlock()
{
    m_pImpl->unlock();
}

//-----------------------------------------------------------------------------

SimpleMutex::~SimpleMutex()
{
    delete m_pImpl;
}

//-----------------------------------------------------------------------------
// RecursiveMutex implementation
//-----------------------------------------------------------------------------

RecursiveMutex::RecursiveMutex():
    m_pImpl(new Impl)
{
}

//-----------------------------------------------------------------------------

void RecursiveMutex::Lock()
{
    m_pImpl->lock();
}

//-----------------------------------------------------------------------------

bool RecursiveMutex::TryLock()
{
    return m_pImpl->try_lock();
}

//-----------------------------------------------------------------------------

void RecursiveMutex::Unlock()
{
    m_pImpl->unlock();
}

//-----------------------------------------------------------------------------

RecursiveMutex::~RecursiveMutex()
{
    delete m_pImpl;
}

//-----------------------------------------------------------------------------
// OnceFlag::Impl
//-----------------------------------------------------------------------------

class OnceFlag::Impl final
{
    PROHIBIT_COPY(Impl)

public:

    Impl()
    {
    }

    void CallOnce(const std::function<void()>& func)
    {
        if (!m_Called)
        {
            BEGIN_MUTEX_SCOPE(m_Mutex)

                if (!m_Called)
                {
                    func();
                    m_Called = true;
                }

            END_MUTEX_SCOPE
        }
    }

private:

    std::atomic<bool> m_Called;
    SimpleMutex m_Mutex;
};

//-----------------------------------------------------------------------------
// OnceFlag implementation
//-----------------------------------------------------------------------------

OnceFlag::OnceFlag():
    m_pImpl(new Impl)
{
}

//-----------------------------------------------------------------------------

void OnceFlag::CallOnce(const std::function<void()>& func)
{
    m_pImpl->CallOnce(func);
}

//-----------------------------------------------------------------------------

OnceFlag::~OnceFlag()
{
    delete m_pImpl;
}
