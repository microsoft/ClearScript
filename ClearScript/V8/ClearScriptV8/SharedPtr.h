// 
// Copyright © Microsoft Corporation. All rights reserved.
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
// RefCount
//-----------------------------------------------------------------------------

class RefCount
{
    PROHIBIT_COPY(RefCount)

public:

    RefCount(DWORD i_Count):
        m_Count(i_Count)
    {
    }

    DWORD Increment()
    {
        return ::InterlockedIncrement(&m_Count);
    }

    DWORD Decrement()
    {
        return ::InterlockedDecrement(&m_Count);
    }

private:

    DWORD m_Count;
};

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

private:

    RefCount m_RefCount;
};

//-----------------------------------------------------------------------------
// SharedPtr
//-----------------------------------------------------------------------------

template<typename T> class SharedPtr
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
        Copy(that);
    }

    SharedPtr<T>(SharedPtr<T>&& that)
    {
        Move(that);
    }

    template<typename U> SharedPtr<T>(const SharedPtr<U>& that)
    {
        Copy(that);
    }

    template<typename U> SharedPtr<T>(SharedPtr<U>&& that)
    {
        Move(that);
    }

    const SharedPtr<T>& operator=(nullptr_t)
    {
        Empty();
        return *this;
    }

    const SharedPtr<T>& operator=(T* pTarget)
    {
        Release();
        Initialize(pTarget);
        return *this;
    }

    const SharedPtr<T>& operator=(const SharedPtr<T>& that)
    {
        Release();
        Copy(that);
        return *this;
    }

    const SharedPtr<T>& operator=(SharedPtr<T>&& that)
    {
        Release();
        Move(that);
        return *this;
    }

    template<typename U> const SharedPtr<T>& operator=(const SharedPtr<U>& that)
    {
        Release();
        Copy(that);
        return *this;
    }

    template<typename U> const SharedPtr<T>& operator=(SharedPtr<U>&& that)
    {
        Release();
        Move(that);
        return *this;
    }

    T* operator->() const
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

    template<typename U> void Copy(const SharedPtr<U>& that)
    {
        m_pTarget = that.m_pTarget;
        m_pRefCount = that.m_pRefCount;
        AddRef();
    }

    template<typename U> void Move(SharedPtr<U>& that)
    {
        m_pTarget = that.m_pTarget;
        m_pRefCount = that.m_pRefCount;
        that.Initialize();
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

    void AttachRefCount(LPVOID /* pvTarget */)
    {
        m_pRefCount = new RefCount(1);
    }

    void DetachRefCount(LPVOID /* pvTarget */)
    {
        delete m_pRefCount;
    }

    T* m_pTarget;
    RefCount* m_pRefCount;
};
