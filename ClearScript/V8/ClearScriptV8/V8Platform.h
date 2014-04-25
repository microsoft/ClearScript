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
// V8 headers
//-----------------------------------------------------------------------------

#pragma warning(push, 3)

#include "v8.h"
#include "v8-debug.h"
using namespace v8;

#pragma warning(pop)

//-----------------------------------------------------------------------------
// V8FastPersistent - a (nearly) drop-in replacement for classic v8::Persistent
//
// WARNING: This class breaks encapsulation in order to avoid heap allocation.
// It makes assumptions about v8::Persistent implementation and memory layout.
//-----------------------------------------------------------------------------

template <typename T>
class V8FastPersistent
{
    template <typename TOther>
    friend class V8FastPersistent;

public:

    V8FastPersistent<T>():
        m_pValue(nullptr)
    {
    }

    template <typename TOther>
    static V8FastPersistent<T> New(Isolate* pIsolate, const Handle<TOther>& hValue)
    {
        Persistent<T> hTemp(pIsolate, hValue);
        return V8FastPersistent<T>(GetPtrAndClear(hTemp));
    }

    template <typename TOther>
    static V8FastPersistent<T> New(Isolate* pIsolate, const V8FastPersistent<TOther>& hValue)
    {
        Persistent<T> hTemp(pIsolate, hValue.AsPersistent());
        return V8FastPersistent<T>(GetPtrAndClear(hTemp));
    }

    bool IsEmpty() const
    {
        return AsPersistent().IsEmpty();
    }

    void* ToPtr() const
    {
        return m_pValue;
    }

    static V8FastPersistent<T> FromPtr(void* pvValue)
    {
        return V8FastPersistent<T>(static_cast<T*>(pvValue));
    }

    Local<T> operator->() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator Handle<TOther>() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator Local<TOther>() const
    {
        return CreateLocal();
    }

    Local<T> CreateLocal(Isolate* pIsolate) const
    {
        return Local<T>::New(pIsolate, AsPersistent());
    }

    template <typename TArg>
    V8FastPersistent<T> MakeWeak(Isolate* pIsolate, TArg* pArg, void (*pCallback)(Isolate*, V8FastPersistent<T>*, TArg*))
    {
        IGNORE_UNUSED(pIsolate);
        AsPersistent().SetWeak(new WeakCallbackContext<TArg>(m_pValue, pArg, pCallback), WeakCallback);
        return *this;
    }

    void Dispose()
    {
        AsPersistent().Reset();
    }

private:

    explicit V8FastPersistent<T>(T* pValue):
        m_pValue(pValue)
    {
    }

    Local<T> CreateLocal() const
    {
        return Local<T>::New(Isolate::GetCurrent(), AsPersistent());
    }

    const Persistent<T>& AsPersistent() const
    {
        return *(reinterpret_cast<const Persistent<T>*>(&m_pValue));
    }

    Persistent<T>& AsPersistent()
    {
        return *(reinterpret_cast<Persistent<T>*>(&m_pValue));
    }

    static T* GetPtrAndClear(Persistent<T>& hValue)
    {
        auto ppValue = reinterpret_cast<T**>(&hValue);
        auto pValue = *ppValue;
        *ppValue = nullptr;
        _ASSERTE(hValue.IsEmpty());
        return pValue;
    }

    template <typename TArg>
    class WeakCallbackContext
    {
    public:

        WeakCallbackContext(T* pValue, TArg* pArg, void (*pCallback)(Isolate*, V8FastPersistent<T>*, TArg*)):
            m_pValue(pValue),
            m_pArg(pArg),
            m_pCallback(pCallback)
        {
        }

        void InvokeCallback(Isolate* pIsolate) const
        {
            V8FastPersistent<T> hTarget(m_pValue);
            m_pCallback(pIsolate, &hTarget, m_pArg);
        }

    private:

        T* m_pValue;
        TArg* m_pArg;
        void (*m_pCallback)(Isolate*, V8FastPersistent<T>*, TArg*);
    };

    template <typename TArg>
    static void WeakCallback(const WeakCallbackData<T, TArg>& data)
    {
        auto pContext = data.GetParameter();
        _ASSERTE(pContext);
        pContext->InvokeCallback(data.GetIsolate());
        delete pContext;
    }

    T* m_pValue;
};

//-----------------------------------------------------------------------------
// V8SafePersistent - a (nearly) drop-in replacement for classic v8::Persistent
//-----------------------------------------------------------------------------

template <typename T>
class V8SafePersistent
{
    template <typename TOther>
    friend class V8SafePersistent;

public:

    V8SafePersistent<T>():
        m_pImpl(nullptr)
    {
    }

    template <typename TOther>
    static V8SafePersistent<T> New(Isolate* pIsolate, const Handle<TOther>& hValue)
    {
        return V8SafePersistent<T>(new Persistent<T>(pIsolate, hValue));
    }

    template <typename TOther>
    static V8SafePersistent<T> New(Isolate* pIsolate, const V8SafePersistent<TOther>& hValue)
    {
        return V8SafePersistent<T>(new Persistent<T>(pIsolate, hValue.GetImpl()));
    }

    bool IsEmpty() const
    {
        return (m_pImpl == nullptr) || m_pImpl->IsEmpty();
    }

    void* ToPtr() const
    {
        return m_pImpl;
    }

    static V8SafePersistent<T> FromPtr(void* pvImpl)
    {
        return V8SafePersistent<T>(static_cast<Persistent<T>*>(pvImpl));
    }

    Local<T> operator->() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator Handle<TOther>() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator Local<TOther>() const
    {
        return CreateLocal();
    }

    Local<T> CreateLocal(Isolate* pIsolate) const
    {
        return Local<T>::New(pIsolate, GetImpl());
    }

    template <typename TArg>
    V8SafePersistent<T> MakeWeak(Isolate* pIsolate, TArg* pArg, void (*pCallback)(Isolate*, V8SafePersistent<T>*, TArg*))
    {
        IGNORE_UNUSED(pIsolate);
        _ASSERTE(m_pImpl != nullptr);
        m_pImpl->SetWeak(new WeakCallbackContext<TArg>(m_pImpl, pArg, pCallback), WeakCallback);
        return *this;
    }

    void Dispose()
    {
        if (m_pImpl != nullptr)
        {
            m_pImpl->Reset();
            delete m_pImpl;
            m_pImpl = nullptr;
        }
    }

private:

    explicit V8SafePersistent<T>(Persistent<T>* pImpl):
        m_pImpl(pImpl)
    {
    }

    Local<T> CreateLocal() const
    {
        return Local<T>::New(Isolate::GetCurrent(), GetImpl());
    }

    const Persistent<T>& GetImpl() const
    {
        return (m_pImpl != nullptr) ? *m_pImpl : ms_EmptyImpl;
    }

    template <typename TArg>
    class WeakCallbackContext
    {
    public:

        WeakCallbackContext(Persistent<T>* pImpl, TArg* pArg, void (*pCallback)(Isolate*, V8SafePersistent<T>*, TArg*)):
            m_pImpl(pImpl),
            m_pArg(pArg),
            m_pCallback(pCallback)
        {
        }

        void InvokeCallback(Isolate* pIsolate) const
        {
            V8SafePersistent<T> hTarget(m_pImpl);
            m_pCallback(pIsolate, &hTarget, m_pArg);
        }

    private:

        Persistent<T>* m_pImpl;
        TArg* m_pArg;
        void (*m_pCallback)(Isolate*, V8SafePersistent<T>*, TArg*);
    };

    template <typename TArg>
    static void WeakCallback(const WeakCallbackData<T, TArg>& data)
    {
        auto pContext = data.GetParameter();
        _ASSERTE(pContext);
        pContext->InvokeCallback(data.GetIsolate());
        delete pContext;
    }

    Persistent<T>* m_pImpl;
    static const Persistent<T> ms_EmptyImpl;
};

template <typename T>
const Persistent<T> V8SafePersistent<T>::ms_EmptyImpl;

//-----------------------------------------------------------------------------
// define classic v8::Persistent replacement
//-----------------------------------------------------------------------------

#define Persistent V8FastPersistent

//-----------------------------------------------------------------------------
// helper functions
//-----------------------------------------------------------------------------

inline void* PtrFromObjectHandle(Persistent<Object> hObject)
{
    return hObject.ToPtr();
}

//-----------------------------------------------------------------------------

inline Persistent<Object> ObjectHandleFromPtr(void* pvObject)
{
    return Persistent<Object>::FromPtr(pvObject);
}

//-----------------------------------------------------------------------------

inline void* PtrFromScriptHandle(Persistent<Script> hScript)
{
    return hScript.ToPtr();
}

//-----------------------------------------------------------------------------

inline Persistent<Script> ScriptHandleFromPtr(void* pvScript)
{
    return Persistent<Script>::FromPtr(pvScript);
}
