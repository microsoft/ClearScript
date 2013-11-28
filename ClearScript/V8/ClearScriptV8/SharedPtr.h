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

    SharedPtrTarget():
         m_RefCount(0)
    {
    }

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
    friend class SharedPtrUtil;

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
        SharedPtrUtil::CopyInitialize(*this, that);
    }

    SharedPtr<T>(SharedPtr<T>&& that)
    {
        SharedPtrUtil::MoveInitialize(*this, that);
    }

    template <typename TOther>
    SharedPtr<T>(const SharedPtr<TOther>& that)
    {
        SharedPtrUtil::CopyInitialize(*this, that);
    }

    template <typename TOther>
    SharedPtr<T>(SharedPtr<TOther>&& that)
    {
        SharedPtrUtil::MoveInitialize(*this, that);
    }

    const SharedPtr<T>& operator=(nullptr_t)
    {
        Empty();
        return *this;
    }

    const SharedPtr<T>& operator=(T* pTarget)
    {
        if (m_pTarget != pTarget)
        {
            Release();
            Initialize(pTarget);
        }

        return *this;
    }

    const SharedPtr<T>& operator=(const SharedPtr<T>& that)
    {
        SharedPtrUtil::Copy(*this, that);
        return *this;
    }

    const SharedPtr<T>& operator=(SharedPtr<T>&& that)
    {
        SharedPtrUtil::Move(*this, that);
        return *this;
    }

    template <typename TOther>
    const SharedPtr<T>& operator=(const SharedPtr<TOther>& that)
    {
        SharedPtrUtil::Copy(*this, that);
        return *this;
    }

    template <typename TOther>
    const SharedPtr<T>& operator=(SharedPtr<TOther>&& that)
    {
        SharedPtrUtil::Move(*this, that);
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
        if (Release())
        {
            Initialize();
        }
    }

    ~SharedPtr()
    {
        Release();
    }

private:

    void Initialize()
    {
        m_pTarget = nullptr;
        m_pRefCount = nullptr;
    }

    void Initialize(T* pTarget)
    {
        if (pTarget != nullptr)
        {
            m_pTarget = pTarget;
            AttachRefCount(m_pTarget);
        }
        else
        {
            Initialize();
        }
    }

    void AddRef()
    {
        if (m_pTarget != nullptr)
        {
            _ASSERTE(m_pRefCount);
            m_pRefCount->Increment();
        }
    }

    bool Release()
    {
        if (m_pTarget != nullptr)
        {
            _ASSERTE(m_pRefCount);
            if (m_pRefCount->Decrement() == 0)
            {
                DetachRefCount(m_pTarget);
                delete m_pTarget;
            }

            return true;
        }

        return false;
    }

    void AttachRefCount(SharedPtrTarget* pTarget)
    {
        m_pRefCount = pTarget->GetRefCount();
        m_pRefCount->Increment();
    }

    void DetachRefCount(SharedPtrTarget* /* pTarget */)
    {
    }

    void AttachRefCount(void* /* pvTarget */)
    {
        m_pRefCount = new RefCount(1);
    }

    void DetachRefCount(void* /* pvTarget */)
    {
        delete m_pRefCount;
    }

    T* m_pTarget;
    RefCount* m_pRefCount;
};

//-----------------------------------------------------------------------------
// SharedPtrUtil
//-----------------------------------------------------------------------------

class SharedPtrUtil
{
    PROHIBIT_CONSTRUCT(SharedPtrUtil)

public:

    template <typename TTarget, typename TSource>
    static void CopyInitialize(SharedPtr<TTarget>& target, const SharedPtr<TSource>& source)
    {
        target.m_pTarget = source.m_pTarget;
        target.m_pRefCount = source.m_pRefCount;
        target.AddRef();
    }

    template <typename TTarget, typename TSource>
    static void MoveInitialize(SharedPtr<TTarget>& target, SharedPtr<TSource>& source)
    {
        target.m_pTarget = source.m_pTarget;
        target.m_pRefCount = source.m_pRefCount;
        source.Initialize();
    }

    template <typename TTarget, typename TSource>
    static void Copy(SharedPtr<TTarget>& target, const SharedPtr<TSource>& source)
    {
        if (target.m_pTarget != source.m_pTarget)
        {
            target.Release();
            CopyInitialize(target, source);
        }
    }

    template <typename TTarget, typename TSource>
    static void Move(SharedPtr<TTarget>& target, SharedPtr<TSource>& source)
    {
        if (target.m_pTarget != source.m_pTarget)
        {
            target.Release();
            MoveInitialize(target, source);
        }
    }
};
