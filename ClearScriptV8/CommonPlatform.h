// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// standard library headers
//-----------------------------------------------------------------------------

#define _SILENCE_CXX17_CODECVT_HEADER_DEPRECATION_WARNING
#define _ITERATOR_DEBUG_LEVEL 0

#include <algorithm>
#include <array>
#include <cassert>
#include <cmath>
#include <codecvt>
#include <cstdint>
#include <functional>
#include <list>
#include <locale>
#include <memory>
#include <queue>
#include <string>
#include <thread>
#include <unordered_map>
#include <unordered_set>
#include <vector>

//-----------------------------------------------------------------------------
// compiler support
//-----------------------------------------------------------------------------

#if defined(_MSC_VER)
    #define MANAGED_METHOD(TYPE) TYPE __stdcall
    #define NATIVE_ENTRY_POINT(TYPE) extern "C" __declspec(dllexport) TYPE __stdcall
    #define DISABLE_WARNING(ID) __pragma(warning(disable:ID))
    #define DEFAULT_WARNING(ID) __pragma(warning(default:ID))
    #define NORETURN __declspec(noreturn)
#elif defined(__clang__)
    #define MANAGED_METHOD(TYPE) TYPE __stdcall
    #define NATIVE_ENTRY_POINT(TYPE) extern "C" __attribute__((visibility("default"))) TYPE __stdcall
    #define DISABLE_WARNING(ID)
    #define DEFAULT_WARNING(ID)
    #define NORETURN __attribute__((noreturn))
    #define _ASSERTE assert
#else
    #error "ClearScript does not support this compiler"
#endif

//-----------------------------------------------------------------------------
// global macros
//-----------------------------------------------------------------------------

#define PROHIBIT_CONSTRUCT(CLASS) \
    CLASS() = delete;

//-----------------------------------------------------------------------------

#define PROHIBIT_HEAP(CLASS) \
    void* operator new(size_t size) = delete; \
    void operator delete(void*, size_t) = delete;

//-----------------------------------------------------------------------------

#define PROHIBIT_COPY(CLASS) \
    CLASS(const CLASS& that) = delete; \
    const CLASS& operator=(const CLASS& that) = delete;

//-----------------------------------------------------------------------------

#define IGNORE_UNUSED(NAME) ((void)(NAME))

#ifdef _DEBUG
    #define ASSERT_EVAL _ASSERTE
    #define DEBUG_EVAL IGNORE_UNUSED
#else // !_DEBUG
    #define ASSERT_EVAL IGNORE_UNUSED
    #define DEBUG_EVAL _ASSERTE
#endif // !_DEBUG

//-----------------------------------------------------------------------------

#define BEGIN_COMPOUND_MACRO \
    do \
    {

#define END_COMPOUND_MACRO \
    DISABLE_WARNING(4127) /* conditional expression is constant */ \
    } \
    while (false) \
    DEFAULT_WARNING(4127)

//-----------------------------------------------------------------------------
// global helper functions
//-----------------------------------------------------------------------------

template <typename TFlag>
inline bool HasFlag(TFlag mask, TFlag flag)
{
    using TUnderlying = std::underlying_type_t<TFlag>;
    return (static_cast<TUnderlying>(mask) & static_cast<TUnderlying>(flag)) != 0;
}

//-----------------------------------------------------------------------------

template <typename TFlag, typename... TOthers>
inline bool HasFlag(TFlag mask, TOthers... others)
{
    return HasFlag(mask, CombineFlags(others...));
}

//-----------------------------------------------------------------------------

template <typename TFlag>
inline bool HasFlags(TFlag mask, TFlag flags)
{
    using TUnderlying = std::underlying_type_t<TFlag>;
    return (static_cast<TUnderlying>(mask) & static_cast<TUnderlying>(flags)) == static_cast<TUnderlying>(flags);
}

//-----------------------------------------------------------------------------

template <typename TFlag, typename... TOthers>
inline bool HasFlags(TFlag mask, TOthers... others)
{
    return HasFlags(mask, CombineFlags(others...));
}

//-----------------------------------------------------------------------------

template <typename TFlag>
inline TFlag CombineFlags(TFlag flag1, TFlag flag2)
{
    using TUnderlying = std::underlying_type_t<TFlag>;
    return static_cast<TFlag>(static_cast<TUnderlying>(flag1) | static_cast<TUnderlying>(flag2));
}

//-----------------------------------------------------------------------------

template <typename TFlag, typename... TOthers>
inline TFlag CombineFlags(TFlag flag1, TFlag flag2, TOthers... others)
{
    return CombineFlags(flag1, CombineFlags(flag2, others...));
}

//-----------------------------------------------------------------------------
// StaticBase
//-----------------------------------------------------------------------------

struct StaticBase
{
    PROHIBIT_CONSTRUCT(StaticBase)
};

//-----------------------------------------------------------------------------
// Disposer
//-----------------------------------------------------------------------------

template <typename T>
struct Disposer final
{
    void operator()(T* pObject) const
    {
        if (pObject != nullptr)
        {
            pObject->Dispose();
        }
    }
};

//-----------------------------------------------------------------------------

template <typename T>
using UniqueDisposePtr = std::unique_ptr<T, Disposer<T>>;

//-----------------------------------------------------------------------------
// Deleter
//-----------------------------------------------------------------------------

template <typename T>
struct Deleter final
{
    void operator()(T* pObject) const
    {
        if (pObject != nullptr)
        {
            pObject->Delete();
        }
    }
};

//-----------------------------------------------------------------------------

template <typename T>
using UniqueDeletePtr = std::unique_ptr<T, Deleter<T>>;

//-----------------------------------------------------------------------------
// PulseValueScope
//-----------------------------------------------------------------------------

template <typename T>
class PulseValueScope final
{
    PROHIBIT_COPY(PulseValueScope)
    PROHIBIT_HEAP(PulseValueScope)

public:

    PulseValueScope(T* pValue, const T& value):
        m_pValue(pValue),
        m_OriginalValue(std::move(*pValue))
    {
        *m_pValue = value;
    }

    PulseValueScope(T* pValue, T&& value):
        m_pValue(pValue),
        m_OriginalValue(std::move(*pValue))
    {
        *m_pValue = std::move(value);
    }

    ~PulseValueScope()
    {
        *m_pValue = std::move(m_OriginalValue);
    }

private:

    T* m_pValue;
    T m_OriginalValue;
};

//-----------------------------------------------------------------------------

#define BEGIN_PULSE_VALUE_SCOPE(ADDRESS, VALUE) \
    { \
        DISABLE_WARNING(4456) /* declaration hides previous local declaration */ \
        PulseValueScope<std::remove_reference<decltype(*(ADDRESS))>::type> t_PulseValueScope((ADDRESS), (VALUE)); \
        DEFAULT_WARNING(4456)

#define END_PULSE_VALUE_SCOPE \
        IGNORE_UNUSED(t_PulseValueScope); \
    }

//-----------------------------------------------------------------------------
// StdBool - a blittable Boolean type for use with P/Invoke
//-----------------------------------------------------------------------------

using StdBool = int8_t;
