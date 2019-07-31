// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// SharedPtrTarget
//-----------------------------------------------------------------------------

class SharedPtrTarget
{
    PROHIBIT_COPY(SharedPtrTarget)

public:

    RefCount* GetRefCount()
    {
        return &m_RefCount;
    }

protected:

    class AddRefScope final
    {
        PROHIBIT_COPY(AddRefScope)
        PROHIBIT_HEAP(AddRefScope)

    public:

        explicit AddRefScope(RefCount* pRefCount):
            m_pRefCount(pRefCount),
            m_RefCountValue(pRefCount->Increment())
        {
        }

        size_t GetRefCountValue() const
        {
            return m_RefCountValue;
        }

        ~AddRefScope()
        {
            m_pRefCount->Decrement();
        }

    private:

        RefCount* m_pRefCount;
        size_t m_RefCountValue;
    };

    SharedPtrTarget():
         m_RefCount(0)
    {
    }

private:

    RefCount m_RefCount;
};

//-----------------------------------------------------------------------------

#define BEGIN_ADDREF_SCOPE \
    { \
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        AddRefScope t_AddRefScope(GetRefCount()); \
    __pragma(warning(default:4456))

#define END_ADDREF_SCOPE \
        IGNORE_UNUSED(t_AddRefScope); \
    }

//-----------------------------------------------------------------------------
// SharedPtrTraits
//-----------------------------------------------------------------------------

template <typename T>
class SharedPtrTraits final
{
    PROHIBIT_CONSTRUCT(SharedPtrTraits)

public:

    static void Destroy(T* pTarget)
    {
        delete pTarget;
    }
};

//-----------------------------------------------------------------------------
// SharedPtr
//-----------------------------------------------------------------------------

template <typename T>
class SharedPtr final
{
public:

    SharedPtr<T>()
    {
        Initialize();
    }

    explicit SharedPtr<T>(nullptr_t)
    {
        Initialize();
    }

    explicit SharedPtr<T>(T* pTarget)
    {
        Initialize(pTarget);
    }

    SharedPtr<T>(const SharedPtr<T>& that)
    {
        CopyInitialize(that);
    }

    SharedPtr<T>(SharedPtr<T>&& that) noexcept
    {
        MoveInitialize(that);
    }

    template <typename TOther>
    explicit SharedPtr<T>(const SharedPtr<TOther>& that)
    {
        CopyInitialize(that);
    }

    template <typename TOther>
    explicit SharedPtr<T>(SharedPtr<TOther>&& that)
    {
        MoveInitialize(that);
    }

    const SharedPtr<T>& operator=(nullptr_t)
    {
        Empty();
        return *this;
    }

    const SharedPtr<T>& operator=(T* pTarget)
    {
        SharedPtr<T> spHolder(m_pTarget, m_pRefCount);
        Initialize(pTarget);
        return *this;
    }

    const SharedPtr<T>& operator=(const SharedPtr<T>& that)
    {
        Copy(that);
        return *this;
    }

    const SharedPtr<T>& operator=(SharedPtr<T>&& that) noexcept
    {
        Move(that);
        return *this;
    }

    template <typename TOther>
    const SharedPtr<T>& operator=(const SharedPtr<TOther>& that)
    {
        Copy(that);
        return *this;
    }

    template <typename TOther>
    const SharedPtr<T>& operator=(SharedPtr<TOther>&& that)
    {
        Move(that);
        return *this;
    }

    T* operator->() const
    {
        _ASSERTE(m_pTarget != nullptr);
        return m_pTarget;
    }

    operator T*() const
    {
        return m_pTarget;
    }

    T* GetRawPtr() const
    {
        return m_pTarget;
    }

    bool IsEmpty() const
    {
        return m_pTarget == nullptr;
    }

    void Empty()
    {
        SharedPtr<T> spHolder(std::move(*this));
    }

    ~SharedPtr()
    {
        Release();
    }

private:

    SharedPtr<T>(T* pTarget, RefCount* pRefCount)
    {
        Initialize(pTarget, pRefCount);
    }

    void Initialize()
    {
        m_pTarget = nullptr;
        m_pRefCount = nullptr;
    }

    void Initialize(T* pTarget)
    {
        m_pTarget = pTarget;
        m_pRefCount = (m_pTarget != nullptr) ? AttachRefCount(m_pTarget) : nullptr;
    }

    void Initialize(T* pTarget, RefCount* pRefCount)
    {
        m_pTarget = pTarget;
        m_pRefCount = pRefCount;
    }

    template <typename TOther>
    void CopyInitialize(const SharedPtr<TOther>& that)
    {
        Initialize(that.m_pTarget, that.m_pRefCount);
        AddRef();
    }

    template <typename TOther>
    void MoveInitialize(SharedPtr<TOther>& that)
    {
        Initialize(that.m_pTarget, that.m_pRefCount);
        that.Initialize();
    }

    template <typename TOther>
    void Copy(const SharedPtr<TOther>& that)
    {
        SharedPtr<T> spHolder(m_pTarget, m_pRefCount);
        CopyInitialize(that);
    }

    template <typename TOther>
    void Move(SharedPtr<TOther>& that)
    {
        SharedPtr<T> spHolder(m_pTarget, m_pRefCount);
        MoveInitialize(that);
    }

    void AddRef()
    {
        if (m_pTarget != nullptr)
        {
            _ASSERTE(m_pRefCount);
            m_pRefCount->Increment();
        }
    }

    void Release()
    {
        if (m_pTarget != nullptr)
        {
            T* pTarget = m_pTarget;
            m_pTarget = nullptr;

            RefCount* pRefCount = m_pRefCount;
            m_pRefCount = nullptr;

            _ASSERTE(pRefCount);
            if (pRefCount->Decrement() == 0)
            {
                DetachRefCount(pTarget, pRefCount);
                SharedPtrTraits<T>::Destroy(pTarget);
            }
        }
    }

    static RefCount* AttachRefCount(SharedPtrTarget* pTarget)
    {
        RefCount* pRefCount = pTarget->GetRefCount();
        pRefCount->Increment();
        return pRefCount;
    }

    static void DetachRefCount(SharedPtrTarget* /*pTarget*/, RefCount* /*pRefCount*/)
    {
    }

    static RefCount* AttachRefCount(void* /*pvTarget*/)
    {
        return new RefCount(1);
    }

    static void DetachRefCount(void* /*pvTarget*/, RefCount* pRefCount)
    {
        delete pRefCount;
    }

    T* m_pTarget;
    RefCount* m_pRefCount;
};
