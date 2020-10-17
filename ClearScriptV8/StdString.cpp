// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

#include "ClearScriptV8Native.h"

//-----------------------------------------------------------------------------
// StdString implementation
//-----------------------------------------------------------------------------

size_t StdString::GetDigest() const
{
    return (sizeof(size_t) == 4) ? static_cast<size_t>(GetDigestAsUInt32()) : static_cast<size_t>(GetDigestAsUInt64());
}

//-----------------------------------------------------------------------------

uint32_t StdString::GetDigestAsUInt32() const
{
    uint32_t digest { 2166136261UL };
    const uint32_t prime { 16777619UL };

    auto pBytes = reinterpret_cast<const uint8_t*>(m_Value.data());
    size_t length { m_Value.length() * sizeof(StdChar) };

    for (size_t index = 0; index < length; index++)
    {
        digest ^= pBytes[index];
        digest *= prime;
    }

    return digest;
}

//-----------------------------------------------------------------------------

uint64_t StdString::GetDigestAsUInt64() const
{
    uint64_t digest { 14695981039346656037ULL };
    const uint64_t prime { 1099511628211ULL };

    auto pBytes = reinterpret_cast<const uint8_t*>(m_Value.data());
    size_t length { m_Value.length() * sizeof(StdChar) };

    for (size_t index = 0; index < length; index++)
    {
        digest ^= pBytes[index];
        digest *= prime;
    }

    return digest;
}

//-----------------------------------------------------------------------------

StdString::Value StdString::GetValue(const v8_inspector::StringView& stringView)
{
    auto length = stringView.length();
    if (length < 1)
    {
        return Value();
    }

    if (!stringView.is8Bit())
    {
        return Value(reinterpret_cast<const StdChar*>(stringView.characters16()), length);
    }

    auto pFirst = reinterpret_cast<const char*>(stringView.characters8());
    return UTF8Converter().from_bytes(pFirst, pFirst + length);
}
