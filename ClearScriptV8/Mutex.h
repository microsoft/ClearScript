// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// SimpleMutex
//-----------------------------------------------------------------------------

class SimpleMutex final
{
    PROHIBIT_COPY(SimpleMutex)

public:

    using Impl = std::mutex;

    SimpleMutex();

    Impl& GetImpl() { return *m_pImpl; }
    const Impl& GetImpl() const { return *m_pImpl; }

    void Lock();
    bool TryLock();
    void Unlock();

    ~SimpleMutex();

private:

    Impl* m_pImpl;
};

//-----------------------------------------------------------------------------
// RecursiveMutex
//-----------------------------------------------------------------------------

class RecursiveMutex final
{
    PROHIBIT_COPY(RecursiveMutex)

public:

    using Impl = std::recursive_mutex;

    RecursiveMutex();

    Impl& GetImpl() { return *m_pImpl; }
    const Impl& GetImpl() const { return *m_pImpl; }

    void Lock();
    bool TryLock();
    void Unlock();

    ~RecursiveMutex();

private:

    Impl* m_pImpl;
};

//-----------------------------------------------------------------------------
// NullMutex
//-----------------------------------------------------------------------------

class NullMutex final
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

template <class TMutex> class MutexLock final
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
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        MutexLock<decltype(MUTEX)> t_MutexLock(MUTEX); \
        DEFAULT_WARNING(4456)

#define END_MUTEX_SCOPE \
        IGNORE_UNUSED(t_MutexLock); \
    }

//-----------------------------------------------------------------------------
// OnceFlag
//-----------------------------------------------------------------------------

class OnceFlag final
{
    PROHIBIT_COPY(OnceFlag)

public:

    OnceFlag();

    void CallOnce(const std::function<void()>& func);

    ~OnceFlag();

private:

    class Impl;
    Impl* m_pImpl;
};
