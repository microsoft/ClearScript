// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// Microsoft Public License (MS-PL)
// 
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
// 
// 1. Definitions
// 
//   The terms "reproduce," "reproduction," "derivative works," and
//   "distribution" have the same meaning here as under U.S. copyright law. A
//   "contribution" is the original software, or any additions or changes to
//   the software. A "contributor" is any person that distributes its
//   contribution under this license. "Licensed patents" are a contributor's
//   patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// 
//   (A) Copyright Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free copyright license
//       to reproduce its contribution, prepare derivative works of its
//       contribution, and distribute its contribution or any derivative works
//       that you create.
// 
//   (B) Patent Grant- Subject to the terms of this license, including the
//       license conditions and limitations in section 3, each contributor
//       grants you a non-exclusive, worldwide, royalty-free license under its
//       licensed patents to make, have made, use, sell, offer for sale,
//       import, and/or otherwise dispose of its contribution in the software
//       or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// 
//   (A) No Trademark License- This license does not grant you rights to use
//       any contributors' name, logo, or trademarks.
// 
//   (B) If you bring a patent claim against any contributor over patents that
//       you claim are infringed by the software, your patent license from such
//       contributor to the software ends automatically.
// 
//   (C) If you distribute any portion of the software, you must retain all
//       copyright, patent, trademark, and attribution notices that are present
//       in the software.
// 
//   (D) If you distribute any portion of the software in source code form, you
//       may do so only under this license by including a complete copy of this
//       license with your distribution. If you distribute any portion of the
//       software in compiled or object code form, you may only do so under a
//       license that complies with this license.
// 
//   (E) The software is licensed "as-is." You bear the risk of using it. The
//       contributors give no express warranties, guarantees or conditions. You
//       may have additional consumer rights under your local laws which this
//       license cannot change. To the extent permitted under your local laws,
//       the contributors exclude the implied warranties of merchantability,
//       fitness for a particular purpose and non-infringement.
//       

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

    class AddRefScope
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
        AddRefScope t_AddRefScope(GetRefCount());

#define END_ADDREF_SCOPE \
        IGNORE_UNUSED(t_AddRefScope); \
    }

//-----------------------------------------------------------------------------
// SharedPtr
//-----------------------------------------------------------------------------

template <typename T>
class SharedPtr
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

    SharedPtr<T>(SharedPtr<T>&& that)
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

    const SharedPtr<T>& operator=(SharedPtr<T>&& that)
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
                delete pTarget;
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
