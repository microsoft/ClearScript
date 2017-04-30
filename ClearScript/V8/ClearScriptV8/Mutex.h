// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// SimpleMutex
//-----------------------------------------------------------------------------

class SimpleMutex
{
    PROHIBIT_COPY(SimpleMutex)

public:

    SimpleMutex();

    void Lock();
    bool TryLock();
    void Unlock();

    ~SimpleMutex();

private:

    class SimpleMutexImpl* m_pImpl;
};

//-----------------------------------------------------------------------------
// RecursiveMutex
//-----------------------------------------------------------------------------

class RecursiveMutex
{
    PROHIBIT_COPY(RecursiveMutex)

public:

    RecursiveMutex();

    void Lock();
    bool TryLock();
    void Unlock();

    ~RecursiveMutex();

private:

    class RecursiveMutexImpl* m_pImpl;
};

//-----------------------------------------------------------------------------
// NullMutex
//-----------------------------------------------------------------------------

class NullMutex
{
    PROHIBIT_COPY(NullMutex)

public:

    NullMutex() {}

    void Lock() {}
    bool TryLock() { return true; }
    void Unlock() {}
};

//-----------------------------------------------------------------------------
// MutexLock
//-----------------------------------------------------------------------------

template <class TMutex> class MutexLock
{
    PROHIBIT_COPY(MutexLock)
    PROHIBIT_HEAP(MutexLock)

public:

    explicit MutexLock(TMutex& mutex):
        m_Mutex(mutex)
    {
        m_Mutex.Lock();
    }

    MutexLock(TMutex& mutex, bool doLock):
        m_Mutex(mutex)
    {
        if (doLock)
        {
            m_Mutex.Lock();
        }
    }

    ~MutexLock()
    {
        m_Mutex.Unlock();
    }

private:

    TMutex& m_Mutex;
};

//-----------------------------------------------------------------------------
// lock scope macros
//-----------------------------------------------------------------------------

#define BEGIN_MUTEX_SCOPE(MUTEX) \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        MutexLock<decltype(MUTEX)> t_MutexLock(MUTEX); \
    __pragma(warning(default:4456))

#define END_MUTEX_SCOPE \
        IGNORE_UNUSED(t_MutexLock); \
    }

//-----------------------------------------------------------------------------
// OnceFlag
//-----------------------------------------------------------------------------

class OnceFlag
{
    PROHIBIT_COPY(OnceFlag)

public:

    OnceFlag();

    void CallOnce(std::function<void()>&& func);

    ~OnceFlag();

private:

    class OnceFlagImpl* m_pImpl;
};
