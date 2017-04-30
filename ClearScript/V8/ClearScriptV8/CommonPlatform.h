// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#pragma once

//-----------------------------------------------------------------------------
// standard library headers
//-----------------------------------------------------------------------------

#include <algorithm>
#include <cstdint>
#include <functional>
#include <queue>
#include <string>
#include <unordered_map>
#include <unordered_set>
#include <vector>

//-----------------------------------------------------------------------------
// global macros
//-----------------------------------------------------------------------------

#define PROHIBIT_CONSTRUCT(CLASS) \
    private: \
        CLASS();

//-----------------------------------------------------------------------------

#define PROHIBIT_HEAP(CLASS) \
    private: \
        void* operator new(size_t size); \
        void operator delete(void*, size_t);

//-----------------------------------------------------------------------------

#define PROHIBIT_COPY(CLASS) \
    private: \
        CLASS(const CLASS& that); \
        const CLASS& operator=(const CLASS& that);

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

#ifndef DECLSPEC_NORETURN
    #define DECLSPEC_NORETURN __declspec(noreturn)
#endif // !DECLSPEC_NORETURN

//-----------------------------------------------------------------------------

#define BEGIN_COMPOUND_MACRO \
    do \
    {

#define END_COMPOUND_MACRO \
    __pragma(warning(disable:4127)) /* conditional expression is constant */ \
    } \
    while (false) \
    __pragma(warning(default:4127))

//-----------------------------------------------------------------------------
// global helper functions
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
// PulseValueScope
//-----------------------------------------------------------------------------

template <typename T>
class PulseValueScope
{
    PROHIBIT_COPY(PulseValueScope)
    PROHIBIT_HEAP(PulseValueScope)

public:

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
    __pragma(warning(disable:4456)) /* declaration hides previous local declaration */ \
        PulseValueScope<std::remove_reference<decltype(*(ADDRESS))>::type> t_PulseValueScope((ADDRESS), (VALUE)); \
    __pragma(warning(default:4456))

#define END_PULSE_VALUE_SCOPE \
        IGNORE_UNUSED(t_PulseValueScope); \
    }
