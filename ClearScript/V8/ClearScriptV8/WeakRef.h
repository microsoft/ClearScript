// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// forward declarations
//-----------------------------------------------------------------------------

template <typename T>
class WeakRefTarget;

template <typename T>
class WeakRefImpl;

//-----------------------------------------------------------------------------
// WeakRef
//-----------------------------------------------------------------------------

template <typename T>
class WeakRef final
{
    friend class WeakRefTarget<T>;

public:

    WeakRef<T>(const WeakRef<T>& that):
        m_spImpl(that.m_spImpl)
    {
    }

    WeakRef<T>(WeakRef<T>&& that) noexcept:
        m_spImpl(std::move(that.m_spImpl))
    {
    }

    const WeakRef<T>& operator=(const WeakRef<T>& that)
    {
        m_spImpl = that.m_spImpl;
        return *this;
    }

    const WeakRef<T>& operator=(WeakRef<T>&& that) noexcept
    {
        m_spImpl = std::move(that.m_spImpl);
        return *this;
    }

    SharedPtr<T> GetTarget() const
    {
        return m_spImpl->GetTarget();
    }

private:

    explicit WeakRef(WeakRefImpl<T>* pImpl):
        m_spImpl(pImpl)
    {
    }

    SharedPtr<WeakRefImpl<T>> m_spImpl;
};

//-----------------------------------------------------------------------------
// WeakRefTarget
//-----------------------------------------------------------------------------

template <typename T>
class WeakRefTarget: public SharedPtrTarget
{
public:

    WeakRefTarget<T>():
        m_spWeakRefImpl(new WeakRefImpl<T>(static_cast<T*>(this)))
    {
    }

    WeakRef<T> CreateWeakRef()
    {
        return WeakRef<T>(m_spWeakRefImpl);
    }

protected:

    ~WeakRefTarget()
    {
        m_spWeakRefImpl->OnTargetDeleted();
    }

private:

    SharedPtr<WeakRefImpl<T>> m_spWeakRefImpl;
};

//-----------------------------------------------------------------------------
// WeakRefImpl
//-----------------------------------------------------------------------------

template <typename T>
class WeakRefImpl final: public SharedPtrTarget
{
    friend class WeakRef<T>;
    friend class WeakRefTarget<T>;

private:

    explicit WeakRefImpl(T* pTarget):
        m_pTarget(pTarget)
    {
    }

    SharedPtr<T> GetTarget()
    {
        SharedPtr<T> spTarget;

        BEGIN_MUTEX_SCOPE(m_Mutex)

            if (m_pTarget != nullptr)
            {
                AddRefScope addRefScope(m_pTarget->GetRefCount());
                if (addRefScope.GetRefCountValue() > 1)
                {
                    spTarget = m_pTarget;
                }
            }

        END_MUTEX_SCOPE

        return spTarget;
    }

    void OnTargetDeleted()
    {
        BEGIN_MUTEX_SCOPE(m_Mutex)

            m_pTarget = nullptr;

        END_MUTEX_SCOPE
    }

    SimpleMutex m_Mutex;
    T* m_pTarget;
};
