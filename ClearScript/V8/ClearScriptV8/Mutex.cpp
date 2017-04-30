// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// SimpleMutexImpl
//-----------------------------------------------------------------------------

class SimpleMutexImpl
{
    PROHIBIT_COPY(SimpleMutexImpl)

public:

    SimpleMutexImpl()
    {
    }

    void Lock()
    {
        m_Mutex.lock();
    }

    bool TryLock()
    {
        return m_Mutex.try_lock();
    }

    void Unlock()
    {
        m_Mutex.unlock();
    }

private:

    std::mutex m_Mutex;
};

//-----------------------------------------------------------------------------
// SimpleMutex implementation
//-----------------------------------------------------------------------------

SimpleMutex::SimpleMutex():
    m_pImpl(new SimpleMutexImpl)
{
}

//-----------------------------------------------------------------------------

void SimpleMutex::Lock()
{
    m_pImpl->Lock();
}

//-----------------------------------------------------------------------------

bool SimpleMutex::TryLock()
{
    return m_pImpl->TryLock();
}

//-----------------------------------------------------------------------------

void SimpleMutex::Unlock()
{
    m_pImpl->Unlock();
}

//-----------------------------------------------------------------------------

SimpleMutex::~SimpleMutex()
{
    delete m_pImpl;
}

//-----------------------------------------------------------------------------
// RecursiveMutexImpl
//-----------------------------------------------------------------------------

class RecursiveMutexImpl
{
    PROHIBIT_COPY(RecursiveMutexImpl)

public:

    RecursiveMutexImpl()
    {
    }

    void Lock()
    {
        m_Mutex.lock();
    }

    bool TryLock()
    {
        return m_Mutex.try_lock();
    }

    void Unlock()
    {
        m_Mutex.unlock();
    }

private:

    std::recursive_mutex m_Mutex;
};

//-----------------------------------------------------------------------------
// RecursiveMutex implementation
//-----------------------------------------------------------------------------

RecursiveMutex::RecursiveMutex():
    m_pImpl(new RecursiveMutexImpl)
{
}

//-----------------------------------------------------------------------------

void RecursiveMutex::Lock()
{
    m_pImpl->Lock();
}

//-----------------------------------------------------------------------------

bool RecursiveMutex::TryLock()
{
    return m_pImpl->TryLock();
}

//-----------------------------------------------------------------------------

void RecursiveMutex::Unlock()
{
    m_pImpl->Unlock();
}

//-----------------------------------------------------------------------------

RecursiveMutex::~RecursiveMutex()
{
    delete m_pImpl;
}

//-----------------------------------------------------------------------------
// OnceFlagImpl
//-----------------------------------------------------------------------------

class OnceFlagImpl
{
    PROHIBIT_COPY(OnceFlagImpl)

public:

    OnceFlagImpl()
    {
    }

    void CallOnce(std::function<void()>&& func)
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
    m_pImpl(new OnceFlagImpl)
{
}

//-----------------------------------------------------------------------------

void OnceFlag::CallOnce(std::function<void()>&& func)
{
    m_pImpl->CallOnce(std::move(func));
}

//-----------------------------------------------------------------------------

OnceFlag::~OnceFlag()
{
    delete m_pImpl;
}
