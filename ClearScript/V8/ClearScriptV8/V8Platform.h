// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// V8 headers
//-----------------------------------------------------------------------------

#pragma warning(push, 3)

#define V8_DEPRECATION_WARNINGS
//#define V8_IMMINENT_DEPRECATION_WARNINGS

#include "v8.h"
#include "v8-platform.h"
#include "v8-inspector.h"
#include "v8-profiler.h"

#pragma warning(pop)

//-----------------------------------------------------------------------------
// V8FastPersistent - a (nearly) drop-in replacement for classic v8::Persistent
//
// WARNING: This class breaks encapsulation in order to avoid heap allocation.
// It makes assumptions about v8::Persistent implementation and memory layout.
//-----------------------------------------------------------------------------

template <typename T>
class V8FastPersistent final
{
    template <typename TOther>
    friend class V8FastPersistent;

public:

    V8FastPersistent<T>():
        m_pValue(nullptr)
    {
    }

    template <typename TOther>
    static V8FastPersistent<T> New(v8::Isolate* pIsolate, const v8::Local<TOther>& hValue)
    {
        v8::Persistent<T> hTemp(pIsolate, hValue);
        return V8FastPersistent<T>(GetPtrAndClear(hTemp));
    }

    template <typename TOther>
    static V8FastPersistent<T> New(v8::Isolate* pIsolate, const V8FastPersistent<TOther>& hValue)
    {
        v8::Persistent<T> hTemp(pIsolate, hValue.AsPersistent());
        return V8FastPersistent<T>(GetPtrAndClear(hTemp));
    }

    template <typename TOther>
    bool operator==(const v8::Local<TOther>& hValue) const
    {
        return AsPersistent() == hValue;
    }

    template <typename TOther>
    bool operator==(const V8FastPersistent<TOther>& hValue) const
    {
        return AsPersistent() == hValue.AsPersistent();
    }

    template <typename TOther>
    bool operator!=(const v8::Local<TOther>& hValue) const
    {
        return AsPersistent() != hValue;
    }

    template <typename TOther>
    bool operator!=(const V8FastPersistent<TOther>& hValue) const
    {
        return AsPersistent() != hValue.AsPersistent();
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

    v8::Local<T> operator->() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator v8::Local<TOther>() const
    {
        return CreateLocal();
    }

    v8::Local<T> CreateLocal(v8::Isolate* pIsolate) const
    {
        return v8::Local<T>::New(pIsolate, AsPersistent());
    }

    bool IsWeak() const
    {
        return AsPersistent().IsWeak();
    }

    template <typename TArg1, typename TArg2>
    V8FastPersistent<T> MakeWeak(v8::Isolate* pIsolate, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, V8FastPersistent<T>*, TArg1*, TArg2*))
    {
        IGNORE_UNUSED(pIsolate);
        _ASSERTE(!IsWeak() && !IsEmpty());
        AsPersistent().SetWeak(new WeakCallbackContext<TArg1, TArg2>(m_pValue, pArg1, pArg2, pCallback), WeakCallback, v8::WeakCallbackType::kParameter);
        return *this;
    }

    void ClearWeak()
    {
        _ASSERTE(IsWeak());
        auto pContext = AsPersistent().ClearWeak<WeakCallbackContextBase>();
        _ASSERTE(pContext);
        delete pContext;
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

    v8::Local<T> CreateLocal() const
    {
        return v8::Local<T>::New(v8::Isolate::GetCurrent(), AsPersistent());
    }

    const v8::Persistent<T>& AsPersistent() const
    {
        return *(reinterpret_cast<const v8::Persistent<T>*>(&m_pValue));
    }

    v8::Persistent<T>& AsPersistent()
    {
        return *(reinterpret_cast<v8::Persistent<T>*>(&m_pValue));
    }

    static T* GetPtrAndClear(v8::Persistent<T>& hValue)
    {
        auto ppValue = reinterpret_cast<T**>(&hValue);
        auto pValue = *ppValue;
        *ppValue = nullptr;
        _ASSERTE(hValue.IsEmpty());
        return pValue;
    }

    class WeakCallbackContextBase
    {
    public:

        virtual ~WeakCallbackContextBase() {}
    };

    template <typename TArg1, typename TArg2>
    class WeakCallbackContext final: public WeakCallbackContextBase
    {
    public:

        WeakCallbackContext(T* pValue, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, V8FastPersistent<T>*, TArg1*, TArg2*)):
            m_pValue(pValue),
            m_pArg1(pArg1),
            m_pArg2(pArg2),
            m_pCallback(pCallback)
        {
        }

        void InvokeCallback(v8::Isolate* pIsolate) const
        {
            V8FastPersistent<T> hTarget(m_pValue);
            m_pCallback(pIsolate, &hTarget, m_pArg1, m_pArg2);
        }

    private:

        T* m_pValue;
        TArg1* m_pArg1;
        TArg2* m_pArg2;
        void (*m_pCallback)(v8::Isolate*, V8FastPersistent<T>*, TArg1*, TArg2*);
    };

    template <typename TArg>
    static void WeakCallback(const v8::WeakCallbackInfo<TArg>& data)
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
class V8SafePersistent final
{
    template <typename TOther>
    friend class V8SafePersistent;

public:

    V8SafePersistent<T>():
        m_pImpl(nullptr)
    {
    }

    template <typename TOther>
    static V8SafePersistent<T> New(v8::Isolate* pIsolate, const v8::Local<TOther>& hValue)
    {
        return V8SafePersistent<T>(new v8::Persistent<T>(pIsolate, hValue));
    }

    template <typename TOther>
    static V8SafePersistent<T> New(v8::Isolate* pIsolate, const V8SafePersistent<TOther>& hValue)
    {
        return V8SafePersistent<T>(new v8::Persistent<T>(pIsolate, hValue.GetImpl()));
    }

    template <typename TOther>
    bool operator==(const v8::Local<TOther>& hValue) const
    {
        return GetImpl() == hValue;
    }

    template <typename TOther>
    bool operator==(const V8SafePersistent<TOther>& hValue) const
    {
        return GetImpl() == hValue.GetImpl();
    }

    template <typename TOther>
    bool operator!=(const v8::Local<TOther>& hValue) const
    {
        return GetImpl() != hValue;
    }

    template <typename TOther>
    bool operator!=(const V8SafePersistent<TOther>& hValue) const
    {
        return GetImpl() != hValue.GetImpl();
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
        return V8SafePersistent<T>(static_cast<v8::Persistent<T>*>(pvImpl));
    }

    v8::Local<T> operator->() const
    {
        return CreateLocal();
    }

    template <typename TOther>
    operator v8::Local<TOther>() const
    {
        return CreateLocal();
    }

    v8::Local<T> CreateLocal(v8::Isolate* pIsolate) const
    {
        return v8::Local<T>::New(pIsolate, GetImpl());
    }

    bool IsWeak() const
    {
        return (m_pImpl != nullptr) && m_pImpl->IsWeak();
    }

    template <typename TArg1, typename TArg2>
    V8SafePersistent<T> MakeWeak(v8::Isolate* pIsolate, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, V8SafePersistent<T>*, TArg1*, TArg2*))
    {
        IGNORE_UNUSED(pIsolate);
        _ASSERTE(!IsWeak() && !IsEmpty());
        m_pImpl->SetWeak(new WeakCallbackContext<TArg1, TArg2>(m_pImpl, pArg1, pArg2, pCallback), WeakCallback, v8::WeakCallbackType::kParameter);
        return *this;
    }

    void ClearWeak()
    {
        _ASSERTE(IsWeak());
        auto pContext = m_pImpl->ClearWeak<WeakCallbackContextBase>();
        _ASSERTE(pContext);
        delete pContext;
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

    explicit V8SafePersistent<T>(v8::Persistent<T>* pImpl):
        m_pImpl(pImpl)
    {
    }

    v8::Local<T> CreateLocal() const
    {
        return v8::Local<T>::New(v8::Isolate::GetCurrent(), GetImpl());
    }

    const v8::Persistent<T>& GetImpl() const
    {
        return (m_pImpl != nullptr) ? *m_pImpl : ms_EmptyImpl;
    }

    class WeakCallbackContextBase
    {
    public:

        virtual ~WeakCallbackContextBase() {}
    };

    template <typename TArg1, typename TArg2>
    class WeakCallbackContext final: public WeakCallbackContextBase
    {
    public:

        WeakCallbackContext(v8::Persistent<T>* pImpl, TArg1* pArg1, TArg2* pArg2, void (*pCallback)(v8::Isolate*, V8SafePersistent<T>*, TArg1*, TArg2*)):
            m_pImpl(pImpl),
            m_pArg1(pArg1),
            m_pArg2(pArg2),
            m_pCallback(pCallback)
        {
        }

        void InvokeCallback(v8::Isolate* pIsolate) const
        {
            V8SafePersistent<T> hTarget(m_pImpl);
            m_pCallback(pIsolate, &hTarget, m_pArg1, m_pArg2);
        }

    private:

        v8::Persistent<T>* m_pImpl;
        TArg1* m_pArg1;
        TArg2* m_pArg2;
        void (*m_pCallback)(v8::Isolate*, V8SafePersistent<T>*, TArg1*, TArg2*);
    };

    template <typename TArg>
    static void WeakCallback(const v8::WeakCallbackInfo<TArg>& data)
    {
        auto pContext = data.GetParameter();
        _ASSERTE(pContext);
        pContext->InvokeCallback(data.GetIsolate());
        delete pContext;
    }

    v8::Persistent<T>* m_pImpl;
    static const v8::Persistent<T> ms_EmptyImpl;
};

template <typename T>
const v8::Persistent<T> V8SafePersistent<T>::ms_EmptyImpl;

//-----------------------------------------------------------------------------
// define classic v8::Persistent replacement
//-----------------------------------------------------------------------------

template <typename T>
using Persistent = V8FastPersistent<T>;

//-----------------------------------------------------------------------------
// helper functions
//-----------------------------------------------------------------------------

template <typename T>
inline void* PtrFromHandle(const Persistent<T>& handle)
{
    return handle.ToPtr();
}

//-----------------------------------------------------------------------------

template <typename T>
inline Persistent<T> HandleFromPtr(void* pvObject)
{
    return Persistent<T>::FromPtr(pvObject);
}
